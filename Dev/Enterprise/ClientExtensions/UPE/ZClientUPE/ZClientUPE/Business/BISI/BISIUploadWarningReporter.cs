using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class BISIUploadWarningReporter
	{
		public void SendWarningEmailIfRequired(INotifications notifications)
		{
			SendWarningEmailIfRequired(ZDateTime.Now, notifications);
		}

		protected virtual void SendWarningEmailIfRequired(ZDateTime now, INotifications notifications)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbGroup recipientGroup = factory.Load<GlbGroup>(UPEDataRegistry.Instance.WarningReportNotificationGroup);

			BISILateShipment[] shipments = ReadLateShipmentsFromDB();
			notifications.Notify(new InfoNotification("Preparing Warning Report"));
			EmailDef email = CreateWarningEmail(shipments, now);

			if (recipientGroup != null && email != null)
			{
				Env.OutgoingMailManager.CreateAndSave(email, recipientGroup.PK.ToGuid(), GroupSourceLocator.GetFromGroup(recipientGroup));
				notifications.Notify(new InfoNotification("Warning Report Sent"));
			}
			else
			{
				notifications.Notify(new InfoNotification("No outstanding shipments found"));
			}
			SetUploadWarningHWM(shipments);
		}

		#region Implementation

		void SetUploadWarningHWM(BISILateShipment[] shipments)
		{
			ZDateTime maxUploadDate = UPEDataRegistry.Instance.BISIUploadWarningHWM;
			foreach (BISILateShipment shipment in shipments)
			{
				if (maxUploadDate.IsEmpty || shipment.BISIUploadDate > maxUploadDate)
				{
					maxUploadDate = shipment.BISIUploadDate;
				}
			}
			UPEDataRegistry.Instance.BISIUploadWarningHWM = maxUploadDate;
		}

		#region CreateWarningEmail

		EmailDef CreateWarningEmail(BISILateShipment[] shipments, ZDateTime now)
		{
			EmailDef result = null;
			string reportContent = GenerateReportContent(shipments);
			if (reportContent != null)
			{
				result = new EmailDef();
				result.Subject = "BISI Upload Warning";
				result.Body = reportContent;
				result.Attachments.Add(new AttachmentDef("BISIUploadWarning" + now.ToString("yyyyMMddHHmm") + ".txt", Encoding.UTF8.GetBytes(reportContent)));
			}
			return result;
		}

		#endregion

		#region GenerateReportContent

		protected string GenerateReportContent()
		{
			return GenerateReportContent(ReadLateShipmentsFromDB());
		}

		string GenerateReportContent(BISILateShipment[] shipments)
		{
			string result = null;
			if (shipments.Length > 0)
			{
				StringWriter writer = new StringWriter();
				GenerateReportContent(writer, shipments);
				result = writer.GetStringBuilder().ToString();
			}
			return result;
		}

		void GenerateReportContent(TextWriter writer, BISILateShipment[] shipments)
		{
			BISILateShipment[] newShipments = GetNewOrUnactionedShipments(shipments, ForNewLines);
			BISILateShipment[] unactionedShipments = GetNewOrUnactionedShipments(shipments, ForUnactionedLines);

			WriteShipmentsSectionWithHeading(writer, newShipments, "The following Shipment Numbers have not been received back from EBS within 60 Minutes:");
			if (newShipments.Length > 0 && unactionedShipments.Length > 0)
			{
				writer.WriteLine("");
				writer.WriteLine("");
			}
			WriteShipmentsSectionWithHeading(writer, unactionedShipments, "The following Shipment Numbers have been previously notified but not yet responded to:");

			writer.WriteLine("");
			writer.WriteLine("--- End of Report ---");
		}

		#endregion

		#region WriteShipmentsSectionWithHeading

		void WriteShipmentsSectionWithHeading(TextWriter writer, BISILateShipment[] shipments, string sectionHeading)
		{
			if (shipments.Length > 0)
			{
				writer.WriteLine(sectionHeading);
				writer.WriteLine("");
				WriteFieldHeadings(writer);
				WriteShipmentArray(writer, shipments);
			}
		}

		void WriteFieldHeadings(TextWriter writer)
		{
			writer.WriteLine("Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $");
			writer.WriteLine("                               Report                      Code");
		}

		void WriteShipmentArray(TextWriter writer, BISILateShipment[] shipments)
		{
			BISILateShipment previousShipment = null;
			for (int i = 0; i < shipments.Length; i++)
			{
				BISILateShipment shipment = shipments[i];
				if (previousShipment != null && previousShipment.ChargeLines.Length == 0 && shipment.ChargeLines.Length > 0)
				{
					writer.WriteLine("");
				}

				WriteShipment(writer, shipment);
				if (shipment.ChargeLines.Length > 0 && i != shipments.Length - 1)
				{
					writer.WriteLine("");
				}
				previousShipment = shipment;
			}
		}

		void WriteShipment(TextWriter writer, BISILateShipment shipment)
		{
			ZDecimal totalChargesForCurrentHAWB = 0.0m;
			for (int i = 0; i < shipment.ChargeLines.Length; i++)
			{
				BISILateShipmentChargeLine chargeLine = shipment.ChargeLines[i];
				totalChargesForCurrentHAWB += chargeLine.GrossAmount;

				bool isFirstCharge = (i == 0);
				bool isLastCharge = (i == shipment.ChargeLines.Length - 1);
				shipment.WriteShipmentChargeLine(writer, chargeLine, isFirstCharge, isLastCharge, totalChargesForCurrentHAWB);
			}
			if (shipment.ChargeLines.Length == 0)
			{
				shipment.WriteShipmentChargeLine(writer, null, true, true, totalChargesForCurrentHAWB);
			}
		}

		#endregion

		#region ReadLateShipmentsFromDB / GetNewOrUnactionedShipments

		BISILateShipment[] ReadLateShipmentsFromDB()
		{
			ArrayList result = new ArrayList();
			DbCommand command = Db.Connection.Command(string.Format(
				"SELECT * FROM vw_Report_ClientBISIUploadWarningReport WHERE BillingTerms != '{0}' ORDER BY BISIUploadDate, CusHAWBPK",
				BillingTermsCodeDescriptionPairList.Codes.FreeDomicile));

			ZDateTime bISIUploadWarningHWM = UPEDataRegistry.Instance.BISIUploadWarningHWM;
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					BISILateShipment currentShipment = null;
					bool hasMore;
					do
					{
						hasMore = ReadNextLateShipmentFromDB(reader, bISIUploadWarningHWM, out currentShipment);
						result.Add(currentShipment);
					}
					while (hasMore);
				}
			}
			return (BISILateShipment[])result.ToArray(typeof(BISILateShipment));
		}

		bool ReadNextLateShipmentFromDB(IDataReader reader, ZDateTime bISIUploadWarningHWM, out BISILateShipment shipment)
		{
			bool result = false;
			shipment = new BISILateShipment(bISIUploadWarningHWM, reader);
			BISILateShipmentChargeLine chargeLine;
			do
			{
				chargeLine = new BISILateShipmentChargeLine(reader);
				if (chargeLine.ShortHAWB != shipment.ShortHAWB)
				{
					result = true;
					break;
				}
				if (!chargeLine.IsEmptyChargeForShipmentWithNoCharges)
				{
					shipment.AddChargeLine(chargeLine);
				}
			}
			while (reader.Read());
			return result;
		}

		const bool ForUnactionedLines = true;
		const bool ForNewLines = false;

		BISILateShipment[] GetNewOrUnactionedShipments(BISILateShipment[] shipments, bool forUnactionedLines)
		{
			ArrayList result = new ArrayList();
			foreach (BISILateShipment shipment in shipments)
			{
				if (shipment.IsUnactioned == forUnactionedLines)
				{
					result.Add(shipment);
				}
			}
			return (BISILateShipment[])result.ToArray(typeof(BISILateShipment));
		}

		#endregion

		#region BISILateShipment / BISILateShipmentChargeLine

		class BISILateShipment
		{
			public BISILateShipment(ZDateTime bISIUploadWarningHWM, IDataReader reader)
			{
				this.BISIUploadWarningHWM = bISIUploadWarningHWM;

				PK = (Guid)reader["CusHAWBPK"];
				ShortHAWB = (string)reader["ShortHAWB"];
				BatchNumber = (int)reader["BatchNumber"];
				ACAStatus = reader["ACAStatus"].ToString();
				BillingTermsDescription = reader["BillingTermsDescription"].ToString();
				BISIUploadDate = (DateTime)reader["BISIUploadDate"];
			}

			readonly ZDateTime BISIUploadWarningHWM;
			public readonly ZGuid PK;
			public readonly ZString ShortHAWB;
			public readonly ZInt BatchNumber;
			public readonly ZString ACAStatus;
			public readonly ZString BillingTermsDescription;
			public readonly ZDateTime BISIUploadDate;

			public BISILateShipmentChargeLine[] ChargeLines
			{
				get { return (BISILateShipmentChargeLine[])fChargeLines.ToArray(typeof(BISILateShipmentChargeLine)); }
			}
			readonly ArrayList fChargeLines = new ArrayList();

			public void AddChargeLine(BISILateShipmentChargeLine chargeLine)
			{
				fChargeLines.Add(chargeLine);
			}

			public void WriteShipmentChargeLine(TextWriter writer, BISILateShipmentChargeLine chargeLine, bool isFirstCharge, bool isLastCharge, ZDecimal totalChargesForCurrentHAWB)
			{
				writer.WriteLine(
					"{0,-18}   {1,-7}   {2,-8} {3,-16}   {4,-3}  {5}  {6}",
					isFirstCharge ? (string)ShortHAWB : "",
					isFirstCharge ? BatchNumber.ToString() : "",
					isFirstCharge ? (string)ACAStatus : "",
					isFirstCharge ? (string)BillingTermsDescription : "",
					chargeLine == null ? "-" : (string)chargeLine.ChargeType,
					chargeLine == null ? (new string(' ', 14) + "-") : string.Format("{0,15:########0.00}", chargeLine.GrossAmount),
					isLastCharge ? string.Format("{0,15:########0.00}", totalChargesForCurrentHAWB) : "");
			}

			public bool IsUnactioned
			{
				get { return BISIUploadDate <= BISIUploadWarningHWM; }
			}
		}

		class BISILateShipmentChargeLine
		{
			public BISILateShipmentChargeLine(IDataReader reader)
			{
				ShortHAWB = (string)reader["ShortHAWB"];
				ChargeType = reader["ChargeType"].ToString();
				GrossAmount = reader["GrossAmount"] is DBNull ? 0m : (decimal)reader["GrossAmount"];
			}

			public bool IsEmptyChargeForShipmentWithNoCharges
			{
				get { return ChargeType.IsEmpty; }
			}

			public readonly ZString ShortHAWB;
			public readonly ZString ChargeType;
			public readonly ZDecimal GrossAmount;
		}

		#endregion

		#endregion
	}
}

