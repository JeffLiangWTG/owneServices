using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public abstract class TransactionBillingSystem : BillingSystemWithDatabase
	{
		protected virtual PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new PriceItemUsage(factory, user, periodStart, systemCode, true);
		}

		protected bool IncludeSubCodeInSystemUsage { get; set; }
		protected bool IncludeReference1InSystemUsage { get; set; }
		protected bool IncludeReference2InSystemUsage { get; set; }
		protected bool IncludeReference3InSystemUsage { get; set; }

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			List<PriceItemUsage> result = new List<PriceItemUsage>();
			foreach (ClientChargeableUsage chargeableUsage in chargeableUsages)
			{
				var user = GetUsingPartyForSystemUsage(chargeableUsage);

				PriceItemUsage systemUsage = result.FirstOrDefault(
						x => UsingPartyComparer.IsEqual(x.User, user)
							&& x.PeriodStart == chargeableUsage.U1_PeriodStart
							&& (!IncludeSubCodeInSystemUsage || x.SubCode == chargeableUsage.U1_SubCode)
							&& (!IncludeReference1InSystemUsage || x.Reference1 == chargeableUsage.U1_Reference1)
							&& (!IncludeReference2InSystemUsage || x.Reference2 == chargeableUsage.U1_Reference2)
							&& (!IncludeReference3InSystemUsage || x.Reference3 == chargeableUsage.U1_Reference3));

				if (systemUsage == null)
				{
					systemUsage = CreateTransactionalSystemUsage(SystemCode, Context.Factory, user, chargeableUsage.U1_PeriodStart);
					if (IncludeSubCodeInSystemUsage)
					{
						systemUsage.SubCode = chargeableUsage.U1_SubCode;
					}

					systemUsage.Reference1 = chargeableUsage.U1_Reference1;
					systemUsage.Reference2 = chargeableUsage.U1_Reference2;
					systemUsage.Reference3 = chargeableUsage.U1_Reference3;
					systemUsage.Reference4 = chargeableUsage.U1_Reference4;

					result.Add(systemUsage);
				}

				systemUsage.TransactionCount += chargeableUsage.U1_UnitCountAsInt;
				systemUsage.ChargeableUsagePKs.Add(chargeableUsage.PK);
			}

			return result.ToArray();
		}

		protected virtual UsingParty GetUsingPartyForSystemUsage(ClientChargeableUsage chargeableUsage)
		{
			return new UsingParty(chargeableUsage);
		}

		protected override SystemUsage[] GetBillableSystemUsages(SystemUsage[] systemUsages)
		{
			return base.GetBillableSystemUsages(systemUsages).Where(x => x.User.OrganisationPK.IsEmpty || x.User.IsOrganisationActive).ToArray();
		}

		public static string CaptionOrDefault(string caption, string defaultCaption)
			=> !string.IsNullOrEmpty(caption) ? caption : defaultCaption;

		const int RefFieldCount = 5;

		protected SystemRawUsage BuildOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context, IEnumerable<StlRawUsageReportRefCaption> usageCaptions)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);

			string currentPriceItemcode = "";
			SummarySection summarySection = null;

			StlRawUsageReportRefCaption caption = null;

			var refArray = new string[RefFieldCount + 1];
			var columnIndexToRefIndex = new int[RefFieldCount + 1];

			while (reader.Read())
			{
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				if (string.Compare(priceItemCode, currentPriceItemcode, StringComparison.OrdinalIgnoreCase) != 0)
				{
					caption = usageCaptions.FirstOrDefault(x => string.Equals(x.UsageCode, priceItemCode, StringComparison.OrdinalIgnoreCase));
					BuildColumnToRef(caption, refArray, columnIndexToRefIndex);
					summarySection = new SummarySection(context.Factory);
					var headerLine = summarySection.Header;
					headerLine.TopLevelDescription = " - " + (caption?.UsageDescription ?? priceItemCode);
					headerLine.Column1 = "Client ID";
					PopulateColumns(refArray, columnIndexToRefIndex, headerLine);
					rawUsage.SummarySections.Add(summarySection);
					currentPriceItemcode = priceItemCode;
				}

				string clientId = (string)reader["TX_ClientID"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = summarySection.Lines.AddNew();
				line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);

				refArray[0] = (string)reader["TX_Reference1"];
				refArray[1] = (string)reader["TX_Reference2"];
				refArray[2] = (string)reader["TX_Reference3"];
				refArray[3] = (string)reader["TX_Reference4"];
				refArray[4] = (string)reader["TX_Reference5"];
				refArray[5] = messageTime.ToLongTimeString();

				PopulateColumns(refArray, columnIndexToRefIndex, line);
			}

			return rawUsage;
		}

		void PopulateColumns(string[] refArray, int[] columnIndexToRefIndex, SummaryLine line)
		{
			line.Column2 = ColToRef(2, refArray, columnIndexToRefIndex);
			line.Column3 = ColToRef(3, refArray, columnIndexToRefIndex);
			line.Column4 = ColToRef(4, refArray, columnIndexToRefIndex);
			line.Column5 = ColToRef(5, refArray, columnIndexToRefIndex);
			line.Column6 = ColToRef(6, refArray, columnIndexToRefIndex);
			line.Column7 = ColToRef(7, refArray, columnIndexToRefIndex);
		}

		/// <summary>
		/// For packing non-blank reference fields into summary line columns, since not all of the
		/// maximum possible five reference fields will be used.
		/// Only reference fields with a non-blank caption are copied to the summary line.
		/// For example, if only ref1 and ref3 have captions (ref 2, 4 and 5 not used) then
		///		column 1 contains ref1
		///		column 2 contains ref3
		///		column 3 contains message time
		/// </summary>
		/// <param name="caption">caption definition</param>
		/// <param name="refArray"></param>
		/// <param name="columnIndexToRefIndex"></param>
		void BuildColumnToRef(StlRawUsageReportRefCaption caption, string[] refArray, int[] columnIndexToRefIndex)
		{
			refArray[RefFieldCount] = "Message Time (UTC)";

			if (caption == null)
			{
				for (int i = 0; i < RefFieldCount; ++i)
				{
					refArray[i] = "Ref " + i.ToString(CultureInfo.InvariantCulture);
					columnIndexToRefIndex[i] = i;
				}
			}
			else
			{
				refArray[0] = caption.Ref1Caption;
				refArray[1] = caption.Ref2Caption;
				refArray[2] = caption.Ref3Caption;
				refArray[3] = caption.Ref4Caption;
				refArray[4] = caption.Ref5Caption;

				int colIndex = 0;
				for (int refIndex = 0; refIndex < RefFieldCount; ++refIndex)
				{
					if (!string.IsNullOrWhiteSpace(refArray[refIndex]))
					{
						columnIndexToRefIndex[colIndex++] = refIndex;
					}
				}

				// last column is always refArray[RefFieldCount], i.e., message time
				columnIndexToRefIndex[colIndex++] = RefFieldCount;

				for (int i = colIndex; i < RefFieldCount + 1; ++i)
				{
					columnIndexToRefIndex[i] = -1;
				}
			}
		}

		string ColToRef(int columnNumber, string[] refArray, int[] columnIndexToRefIndex)
		{
			int refIndex = columnIndexToRefIndex[columnNumber - 2];
			return refIndex >= 0 ? refArray[refIndex] : null;
		}

		protected void BuilderRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action, IEnumerable<StlRawUsageReportRefCaption> usageCaptions)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";
			StlRawUsageReportRefCaption caption = null;

			string currentPriceItemCode = "";

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string priceItemCode = (string)reader["TX_PriceItemCode"];
					if (string.Compare(priceItemCode, currentPriceItemCode, StringComparison.OrdinalIgnoreCase) != 0)
					{
						caption = usageCaptions.FirstOrDefault(x => string.Equals(x.UsageCode, priceItemCode, StringComparison.OrdinalIgnoreCase));
						string[] headerColumns = new string[] { "Type", clientHeaderColumn,
							CaptionOrDefault(caption?.Ref1Caption, "Ref 1"),
							CaptionOrDefault(caption?.Ref2Caption, "Ref 2"),
							CaptionOrDefault(caption?.Ref3Caption, "Ref 3"),
							CaptionOrDefault(caption?.Ref4Caption, "Ref 4"),
							CaptionOrDefault(caption?.Ref5Caption, "Ref 5"),
							"Message Time (UTC)" };
						var headerCsvLine = new OCsvLine(headerColumns);
						action(headerCsvLine.ToString());
						currentPriceItemCode = priceItemCode;
					}

					string client;
					if (isStlBilling)
					{
						string companyCode = (string)reader["CompanyCode"];
						client = companyCode;
					}
					else
					{
						string clientId = (string)reader["TX_ClientID"];
						client = clientId;
					}

					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					string ref5 = (string)reader["TX_Reference5"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					string[] dataValues = new string[] { caption?.UsageDescription ?? priceItemCode, client, ref1, ref2, ref3, ref4, ref5, messageTime.ToLongTimeString() };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		protected void BuildRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer, IEnumerable<StlRawUsageReportRefCaption> usageCaptions)
		{
			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string priceItemCode = (string)reader["TX_PriceItemCode"];

					string client;
					if (isStlBilling)
					{
						string companyCode = (string)reader["CompanyCode"];
						client = companyCode;
					}
					else
					{
						string clientId = (string)reader["TX_ClientID"];
						client = clientId;
					}

					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					string ref5 = (string)reader["TX_Reference5"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					var caption = usageCaptions.FirstOrDefault(x => string.Equals(x.UsageCode, priceItemCode, StringComparison.OrdinalIgnoreCase));

					string[] references = new string[] { caption?.UsageDescription ?? priceItemCode, ref1, ref2, ref3, ref4, ref5 };
					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Join(" ", references).Trim(), priceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}
	}
}

