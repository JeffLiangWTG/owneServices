using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class BISIShipmentsUploadedReporter
	{
		public BISIShipmentsUploadedReporter(IReadOnlyList<IShipmentData> uploadedShipments, int bISIUploadCurrentBatchNumber)
		{
			this.UploadedShipments = uploadedShipments;
			this.BISIUploadCurrentBatchNumber = bISIUploadCurrentBatchNumber;
		}

		public readonly IReadOnlyList<IShipmentData> UploadedShipments;
		public readonly int BISIUploadCurrentBatchNumber;

		public virtual void SendEmailIfRequired(INotifications notifications)
		{
			if (RecipientGroup != null)
			{
				notifications.Notify(new InfoNotification("Preparing Interchange Report"));
				EmailDef email = GenerateEmail();
				try
				{
					Env.OutgoingMailManager.CreateAndSave(email, RecipientGroup.PK.ToGuid(), GroupSourceLocator.GetFromGroup(RecipientGroup));
				}
				catch (EmailSendFailedException ex)
				{
					ErrorReporter.ReportOnce("52A9E458-AB25-4408-863E-40F11867FACA", "Exception when sending Interchange Report", ex);
				}
				notifications.Notify(new InfoNotification("Interchange Report Sent"));
			}
		}

		#region Implementation

		GlbGroup RecipientGroup
		{
			get
			{
				if (fRecipientGroup == null)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					fRecipientGroup = factory.Load<GlbGroup>(UPEDataRegistry.Instance.InterchangeReportNotificationGroup);
				}
				return fRecipientGroup;
			}
		}
		GlbGroup fRecipientGroup;

		EmailDef GenerateEmail()
		{
			StringWriter reportContent = new StringWriter();
			WriteReportContent(reportContent);

			EmailDef email = new EmailDef();
			email.Subject = "Interchange Report " + BISIUploadCurrentBatchNumber;

			email.Body = reportContent.GetStringBuilder().ToString();
			email.Attachments.Add(new AttachmentDef("BISIShipmentsUploaded" + ZDateTime.Now.ToString("yyyyMMddHHmm") + ".txt", Encoding.UTF8.GetBytes(reportContent.GetStringBuilder().ToString())));
			return email;
		}

		void WriteReportContent(TextWriter writer)
		{
			writer.WriteLine("Interchange Report");
			writer.WriteLine("");
			writer.WriteLine("Interchange #  : {0}", BISIUploadCurrentBatchNumber);
			writer.WriteLine("Uploaded       : {0:dd-MMM-yy HH:mm}", ZDateTime.Now);
			writer.WriteLine("Total Shipments: {0}", UploadedShipments.Count);

			if (UploadedShipments.Count > 0)
			{
				writer.WriteLine("Total Charges  : {0:0.00}", GetTotalCharges(UploadedShipments));

				Dictionary<ZString, List<IShipmentData>> groupedShipments = GetShipmentsGroupedByDischarge();
				foreach (KeyValuePair<ZString, List<IShipmentData>> destinationPortGroup in groupedShipments)
				{
					writer.WriteLine("");
					WriteShipmentsForDestinationPort(writer, destinationPortGroup.Key, destinationPortGroup.Value);
				}
			}
			writer.WriteLine("");

			writer.WriteLine("--- End of Report ---");
		}

		void WriteShipmentsForDestinationPort(TextWriter writer, ZString destinationPort, IReadOnlyList<IShipmentData> shipments)
		{
			writer.WriteLine("Destination Port " + destinationPort);
			writer.WriteLine("----------------------------------------------------------------------------------------");
			writer.WriteLine("Shipment #    Destination  Status    Billing         Charge     Amount $  Total Charge $");
			writer.WriteLine("                                      Terms           Code");
			writer.WriteLine("----------------------------------------------------------------------------------------");

			WriteShipments(writer, shipments);

			ZDecimal totalChargesForPort = GetTotalCharges(shipments);
			writer.WriteLine("----------------------------------------------------------------------------------------");
			writer.WriteLine("Total for Destination {0}: {1,-10}                        Sub-Total {2,15:########0.00}", destinationPort, shipments.Count, totalChargesForPort);
			writer.WriteLine("----------------------------------------------------------------------------------------");
		}

		void WriteShipments(TextWriter writer, IReadOnlyList<IShipmentData> shipments)
		{
			IShipmentData previousShipment = null;
			foreach (IShipmentData shipment in shipments)
			{
				if (previousShipment != null && (previousShipment.ChargesData.Count > 1 || shipment.ChargesData.Count > 1))
				{
					writer.WriteLine("");
				}
				WriteShipmentDetailsLines(writer, shipment);
				previousShipment = shipment;
			}
		}

		void WriteShipmentDetailsLines(TextWriter writer, IShipmentData shipment)
		{
			if (shipment.ChargesData.Count == 0)
			{
				WriteShipmentDetailsFirstLine(writer, shipment, "", "");
			}

			decimal totalCharges = 0.0m;
			for (int i = 0; i < shipment.ChargesData.Count; i++)
			{
				totalCharges += shipment.ChargesData[i].GrossAmount;
				WriteShipmentDetailsLine(writer, shipment, i, totalCharges);
			}
		}

		void WriteShipmentDetailsFirstLine(TextWriter writer, IShipmentData shipment, string typeCode, string grossAmount)
		{
			string totalCharges = "";
			switch (shipment.ChargesData.Count)
			{
				case 0: totalCharges = "           0.00"; break;
				case 1: totalCharges = grossAmount; break;
			}

			string billingTerms = new BillingTermsCodeDescriptionPairList().GetDescriptionFromCode(shipment.BillingTerms);
			writer.WriteLine(
				"{0,-11}   {1,-5}        {2,-8}  {3,-15} {4,-3} {5,-15} {6,-15}",
				shipment.ShipmentRef, shipment.DischargePort, shipment.CustomsStatus, billingTerms, typeCode, grossAmount, totalCharges);
		}

		void WriteShipmentDetailsLine(TextWriter writer, IShipmentData shipment, int chargeIndex, decimal totalCharges)
		{
			bool isFirstCharge = chargeIndex == 0;
			bool isLastCharge = (chargeIndex == shipment.ChargesData.Count - 1);
			string typeCode = shipment.ChargesData[chargeIndex].TypeCode;
			string grossAmount = string.Format("{0,15:########0.00}", shipment.ChargesData[chargeIndex].GrossAmount);

			if (isFirstCharge)
			{
				WriteShipmentDetailsFirstLine(writer, shipment, typeCode, grossAmount);
			}
			else
			{
				string totalChargesIfLast = isLastCharge ? string.Format("{0,15:########0.00}", totalCharges) : "";
				writer.WriteLine(
					"{0,-11}   {1,-5}        {2,-8}  {3,-15} {4,-3} {5} {6,-15}",
					"", "", "", "", typeCode, grossAmount, totalChargesIfLast);
			}
		}

		Dictionary<ZString, List<IShipmentData>> GetShipmentsGroupedByDischarge()
		{
			Dictionary<ZString, List<IShipmentData>> result = new Dictionary<ZString, List<IShipmentData>>();
			foreach (IShipmentData shipment in UploadedShipments)
			{
				List<IShipmentData> list = null;
				result.TryGetValue(shipment.DischargePort, out list);
				if (list == null)
				{
					list = new List<IShipmentData>();
					result.Add(shipment.DischargePort, list);
				}
				list.Add(shipment);
			}
			return result;
		}

		ZDecimal GetTotalCharges(IReadOnlyList<IShipmentData> shipments)
		{
			ZDecimal result = 0.0m;
			foreach (IShipmentData shipment in shipments)
			{
				foreach (ShipmentChargeData charge in shipment.ChargesData)
				{
					result += charge.GrossAmount;
				}
			}
			return result;
		}

		#endregion
	}
}
