using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRIMDRMessage : CMRImportDeclarationMessage, IOutstandingPaymentInfoProvider
	{
		public CMRIMDRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.IMD;
		}

		#region Report in Email Notification

		public override ZString GetReport()
		{
			var reportBuilder = new ZStringBuilder();
			reportBuilder.Append(base.GetReport());
			reportBuilder.Append(GetSecurityAmountIfAny());
			reportBuilder.Append(GetSecurityLiabilityAmountIfAny());
			reportBuilder.Append(GetMessageAdviceErrors());
			reportBuilder.Append(GetGSTRelatedErrorIfAny());
			return reportBuilder.ToString();
		}

		public override bool SupportsHTMLResponseEmails
		{
			get { return true; }
		}

		protected override ZString GetReportForHTMLHeaderSection()
		{
			var reportBuilder = new ZStringBuilder();
			reportBuilder.Append(base.GetReportForHTMLHeaderSection());
			reportBuilder.Append(GetSecurityAmountIfAny());
			reportBuilder.Append(GetSecurityLiabilityAmountIfAny());
			return reportBuilder.ToString();
		}

		protected override ZString GetReportForHTMLFooterSection()
		{
			var reportBuilder = new ZStringBuilder();
			reportBuilder.Append(base.GetReportForHTMLFooterSection());
			reportBuilder.Append(GetGSTRelatedErrorIfAny());
			return reportBuilder.ToString();
		}

		protected string GetSecurityAmountIfAny()
		{
			var totalSecurityConcession = IMDRInfoProvider.TotalSecurityConcession;
			return totalSecurityConcession > 0 ?
				"\r\nTotal Security Concession Amount: " + totalSecurityConcession.ToString() + "\r\n" : string.Empty;
		}

		protected string GetSecurityLiabilityAmountIfAny()
		{
			var totalSecurityLiability = IMDRInfoProvider.TotalSecurityLiability;
			return totalSecurityLiability > 0 ?
				"\r\nTotal Security Uncollected Amount: " + totalSecurityLiability.ToString() + "\r\n" : string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected ZString GetGSTRelatedErrorIfAny()
		{
			var result = ZString.Empty;

			var importer = (EM_LinkedObject as CusEntryHeader)?.Declaration?.Importer;
			if (importer != null)
			{
				var stringBuilder = new ZStringBuilder();
				bool isGSTDeferred = importer.MiscServ.OM_IMIsGSTDeferred;
				if (IMDRInfoProvider.TotalPayableGST > 0 && IMDRInfoProvider.TotalDeferredGST == 0 && isGSTDeferred)
				{
					stringBuilder.Append("\r\nGST-Deferred Configuration Discrepancy: The message from Customs has GST PAYABLE amounts, not deferred amounts.\r\nThis indicates that Importer, ");
					stringBuilder.Append(importer.OH_FullNameTruncated);
					stringBuilder.Append(" is NOT registered with Customs as GST-deferrable, but configured as GST-deferrable in CargoWise One.\r\nYou should contact Customs about this and rectify the configuration at Organisation form to reflect Customs's data\r\nas GST calculated by CargoWise One will be different from Customs calculation and declaration questions depending on this configuration can be asked wrongly on amendments.");
				}
				else if (IMDRInfoProvider.TotalPayableGST == 0 && IMDRInfoProvider.TotalDeferredGST > 0 && !isGSTDeferred)
				{
					stringBuilder.Append("\r\nGST-Deferred Configuration Discrepancy: The message from Customs has GST DEFERRED amounts, not GST payable amounts.\r\nThis indicates that Importer, ");
					stringBuilder.Append(importer.OH_FullNameTruncated);
					stringBuilder.Append(" is registered as GST-deferrable with Customs, but not configured so in CargoWise One.\r\nYou should rectify the configuration at Organisation form as GST calculated by CargoWise One will be different from Customs Calculation\r\nand declaration questions depending on this configuration can be asked wrongly on amendments.");
				}

				result = stringBuilder.ToString();
			}

			return result;
		}

		protected override ZString AdditionalLineReference(ZString lineNumber)
		{
			var result = ZString.Empty;

			if (!lineNumber.IsEmpty && lineNumber != "0")
			{
				result = "LINE " + lineNumber;
			}

			return result;
		}

		public override ZString StatusOfLinesReport
		{
			get { return ZString.Empty; }
		}

		protected override ZString GetErrorSectionDescription(ZString status)
		{
			var allUpperStatus = status.ToUpper();

			if (allUpperStatus == BaseImportDeclarationMessageProcessor.ClearStatus ||
				allUpperStatus == BaseImportDeclarationMessageProcessor.HeldStatus ||
				allUpperStatus == BaseImportDeclarationMessageProcessor.FinalisedStatus ||
				allUpperStatus == BaseImportDeclarationMessageProcessor.WithdrawnStatus)
			{
				return "\r\n\r\nWarnings:\r\n";
			}
			else
			{
				return base.GetErrorSectionDescription(status);
			}
		}

		protected override string GetAdditionalMessageAdviceForThisError(string errorCode)
		{
			var result = new ZStringBuilder();
			result.Append(base.GetAdditionalMessageAdviceForThisError(errorCode));

			switch (errorCode)
			{
				case "ID1005":
					result.Append(AdditionalMessageAdviceForCPDecQuestion);
					break;
				case "ID0294":
					result.Append(ExchangeRateMessageAdvice);
					break;
			}

			return result.ToString();
		}

		ZString AdditionalMessageAdviceForCPDecQuestion
		{
			get
			{
				var result = ZString.Empty;

				var entryHeader = EM_LinkedObject as CusEntryHeader;
				var cPDecGenDate = entryHeader?.Declaration?.AddInfo.ZA_CPQuestionGenDate_Hidden ?? ZDateTime.Empty;
				if (cPDecGenDate.IsValid)
				{
					var log = new CMRReferenceFileUpdateLog(Factory);
					log.LoadLastSuccessfulUpdate();
					var lastSuccessUpdate = log.SuccessfulUpdateFileTimeStamp.ToZDateTime();

					if (lastSuccessUpdate.IsValid && cPDecGenDate < lastSuccessUpdate)
					{
						var stringBuilder = new ZStringBuilder();
						stringBuilder.Append("CP Dec questions for the job were generated on the version of reference files updated on ");
						stringBuilder.Append(cPDecGenDate.ToShortDateString());

						stringBuilder.Append(". But the last successful update of the reference files was done on ");
						stringBuilder.Append(lastSuccessUpdate.ToShortDateString());

						stringBuilder.Append(".\r\nCustoms might have added more mandatory questions for the tariff in the meantime.");
						stringBuilder.Append(" Please open the job and regenerate Declaration questions by clicking Brokerage > Regenerate Declaration Questions");

						result = stringBuilder.ToString();
					}
				}

				return result;
			}
		}

		internal const string ExchangeRateMessageAdvice = "This error often relates to not having the most recent exchange rates in the database. " +
			"Customs might have published new exchange rates. You should run a manual update of the exchange rates Reference Files. " +
			"Select Maintain > System > Service Tasks and run the REF Service Task (Code = REF) by choosing Schedule Now in the menu. " +
			"Once this is run, re-select the job and click Brokerage > Perform Apportionment. " +
			"Then simply Relodge the Entry(s).";

		#endregion

		#region IOutstandingPaymentInfoProvider Members

		SegmentGroup5MessageSection IOutstandingPaymentInfoProvider.Group5Section => CUSRES.Group5;
		GISSegmentMessageSection IOutstandingPaymentInfoProvider.GISSection => CUSRES.GIS;

		#endregion

		#region IMDRInfoProvider

		public IMDRInfoProvider IMDRInfoProvider => fIMDRInfoProvider ?? (fIMDRInfoProvider = new IMDRInfoProvider(CUSRES));
		IMDRInfoProvider fIMDRInfoProvider;

		#endregion
	}
}
