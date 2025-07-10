using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.DocWrappers.Testing
{
	[TestedType(typeof(DocROHARInvoice))]
	public class DocROHDocARInvoiceTest : DocumentWrapperTestCase
	{
		#region Override
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { InvoiceWrapper };
		}

		#endregion
		#region Set Up
		protected override void SetUp()
		{
			base.SetUp();
			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoiceWrapper = DocROHARInvoice.New(Invoice, Factory);
			base.SetUp();
		}

		#endregion
		public void TestAccountCodeOrDBRegistrationNumber()
		{
			OrgHeader org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			Invoice.AH_OH = org.PK;
			AssertEquals(org.OH_Code, InvoiceWrapper.AccountCodeOrDBRegistrationNumber);
			org.OH_IsGlobalAccount = false;
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Angola;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ExternalDebtorAccountCode;
			cusCode.OK_CustomsRegNo = "11111111";
			AssertEquals(org.OH_Code, InvoiceWrapper.AccountCodeOrDBRegistrationNumber);
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			AssertEquals("11111111", InvoiceWrapper.AccountCodeOrDBRegistrationNumber);
			org.OH_IsGlobalAccount = true;
			AssertEquals(org.OH_Code, InvoiceWrapper.AccountCodeOrDBRegistrationNumber);
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("11111111", InvoiceWrapper.AccountCodeOrDBRegistrationNumber);
		}

		InvoicingBase Invoice;
		DocROHARInvoice InvoiceWrapper;
	}
}
