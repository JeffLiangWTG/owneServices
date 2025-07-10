using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.GB.Business.GBCommonConstants;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	public class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.CusEntryLineFeeValidationTest
	{
		public void TestCheckAction()
		{
			var entryLineFees = CreateEntryLineFees(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			var entryLineFee1 = entryLineFees[0];
			var entryLineFee2 = entryLineFees[1];
			var entryLineFee3 = entryLineFees[2];

			entryLineFee1.CF_RateOverrideReasonCode = TaxOverrideReasonCodes.Override;
			AssertHasMessageError(entryLineFee1.CF_RateOverrideReasonCodeInfo, "If one fee has Action of 'OVR' then all other fees must also have Action of 'OVR'");
			AssertHasMessageError(entryLineFee1.CF_RateOverrideReasonCodeInfo, "If the fee has Action of 'OVR' then the Entry Line must have Additional Info of type 'OVR01'");
			entryLineFee1.EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().AdditionalInfos.AddNew().CSI_Code = AdditonalInfoCodes.Override;
			entryLineFee1.Validation.ValidateCF_RateOverrideReasonCode();
			AssertNoMessageError(entryLineFee1.CF_RateOverrideReasonCodeInfo, "If the fee has Action of 'OVR' then the Entry Line must have Additional Info of type 'OVR01'");

			entryLineFee2.CF_RateOverrideReasonCode = TaxOverrideReasonCodes.Override;
			AssertHasMessageError(entryLineFee2.CF_RateOverrideReasonCodeInfo, "If one fee has Action of 'OVR' then all other fees must also have Action of 'OVR'");

			entryLineFee2.EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().AdditionalInfos.AddNew().CSI_Code = AdditonalInfoCodes.Override;

			entryLineFee3.CF_RateOverrideReasonCode = TaxOverrideReasonCodes.Override;
			AssertNoMessageError(entryLineFee3.CF_RateOverrideReasonCodeInfo, "If one fee has Action of 'OVR' then all other fees must also have Action of 'OVR'");
		}

		EU.Business.Declaration.CusEntryLineFee[] CreateEntryLineFees(ZString declarationType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = declarationType;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "1";
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>().First();
			entryLine.Fees.AddNew();
			entryLine.Fees.AddNew();
			entryLine.Fees.AddNew();
			return entryLine.Fees.Cast<CusEntryLineFee>().ToArray();
		}
	}
}
