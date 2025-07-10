using System.Linq;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class ImportQueryMessagePrettyFormatter : ImportGenericCommonMessagePrettyFormatter<IImportQuery>
	{
		public ImportQueryMessagePrettyFormatter(IImportQuery response, CusEntryHeader entryHeader) : base(response, entryHeader)
		{
		}

		protected override ZString CreateMessageDetailsAcceptedCore(string extraDataFromProcessing)
		{
			var messageDetails = new StringBuilder();

			AppendDeclarationData(messageDetails);
			AppendManagementData(messageDetails);
			AppendTaxesTitle(messageDetails);

			var tableCreator = GetNewNonVisibleTableCreator();
			AppendTotalAmountToPayAndGuaranteedTotal(messageDetails, tableCreator);
			AppendVATData(messageDetails, tableCreator);
			messageDetails.Append(tableCreator.ToHtml());

			AppendPaymentInfoTitle(messageDetails);
			var tableCreator2 = GetNewNonVisibleTableCreator();
			AppendPaymentInfo(messageDetails, tableCreator2);
			messageDetails.Append(tableCreator2.ToHtml());

			messageDetails.Append(blankLine);
			if (response.Administration == "AEAT")
			{
				AppendGuarantees(messageDetails, response.GRNGuarantees, null, extraDataFromProcessing);
			}
			else
			{
				AppendGuarantees(messageDetails, null, response.GRNGuarantees, extraDataFromProcessing);
			}
			AppendFees(messageDetails, false);
			AppendCertificates(messageDetails);

			return messageDetails.ToString();
		}

		void AppendManagementData(StringBuilder messageDetails)
		{
			messageDetails.Append(blankLine + ManagementDataHeaderText + blankLine);
			var tableCreator = GetNewNonVisibleTableCreator();
			WriteRowIfNotEmpty(tableCreator, AdministrationText, response.Administration);
			WriteRowIfNotEmpty(tableCreator, DeclarationTypeText, (response.DeclarationType + " - " + response.DeclarationTypeDescription));
			WriteRowIfNotEmpty(tableCreator, CustomsClearanceStatusText, (response.ClearanceStatus + " - " + response.ClearanceStatusDescription));
			WriteRowIfNotEmpty(tableCreator, UnfinishedPendenciesText, (response.UnfinishedPendencies + " - " + response.UnfinishedPendenciesDescription));
			messageDetails.Append(tableCreator.ToHtml());
			messageDetails.Append(blankLine);
		}

		protected override void AppendExtraDataToGuarantees(StringBuilder messageDetails, ZString extraData)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			WriteRowIfNotEmpty(tableCreator, AccountingStatusText, (response.AccountingStatus + " - " + response.AccountingStatusDescription));
			if (extraData.Contains(ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter))
			{
				var guaranteeStatusData = extraData.Split(ExtraDataFromProcessing.SymbolToSeparateExtraDataForPrettyFormatter).First(x => x.Contains(ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter));

				WriteRowIfNotEmpty(tableCreator, GuaranteeStatusText, guaranteeStatusData.RemoveSafe(0, ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter.Length));
			}
			messageDetails.Append(tableCreator.ToHtml());
			messageDetails.Append(blankLine);
		}

		protected override ZString GetPaymentProofNumber() => response.PaymentProofNumber;

		string ManagementDataHeaderText => GetH2Text(ResString.GetMultilingualString("EAF43A68-F410-4145-A5C3-0FCFA32442C1", "Management data"));
	}
}
