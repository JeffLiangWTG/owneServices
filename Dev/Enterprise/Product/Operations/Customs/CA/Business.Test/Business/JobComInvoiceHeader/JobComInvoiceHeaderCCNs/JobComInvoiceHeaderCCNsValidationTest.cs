using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceHeaderCCNsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ2_ReferenceNumber_List()
		{
			var headerCCN = header.CargoControlNumbersList.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(headerCCN.J2_ReferenceNumberInfo, "XXXXX", "00001");
		}

		public void TestCheckJ2_ReferenceNumber_Duplicate()
		{
			const string messageError = "Duplicated Cargo Control Numbers on this invoice";
			var headerCCN = header.CargoControlNumbersList.AddNew();
			headerCCN.J2_ReferenceNumber = "00001";
			var headerCCN2 = header.CargoControlNumbersList.AddNew();
			headerCCN2.J2_ReferenceNumber = "00001";
			CombineAssertions(() =>
			{
				AssertHasErrorContaining("Duplicate", headerCCN2.J2_ReferenceNumberInfo, messageError);
				headerCCN2.J2_ReferenceNumber = "00002";
				AssertNoErrorContaining("Unique", headerCCN2.J2_ReferenceNumberInfo, messageError);
			});
		}

		public void TestCheckJ2_ReferenceNumber_ThereIsOnlyOneCCN()
		{
			var headerCCN = header.CargoControlNumbersList.AddNew();
			AssertEquals(1, headerCCN.Lookups.CargoControlNumbersList.Count);
			AssertEquals("00001", headerCCN.Lookups.CargoControlNumbersList[0].Code);
			headerCCN.J2_ReferenceNumber = "1234";
			AssertNoMessageError(headerCCN.J2_ReferenceNumberInfo, "When only one CCN number exists, CCN must be at the Entry Level only. Please remove from the invoice level.");
			headerCCN.J2_ReferenceNumber = "00001";
			AssertHasMessageError(headerCCN.J2_ReferenceNumberInfo, "When only one CCN number exists, CCN must be at the Entry Level only. Please remove from the invoice level.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header = declaration.Invoices.AddNew();
			var release = declaration.ReleaseStatuses.AddNew();
			release.RL_CargoControlNumber = "00001";
		}
		JobComInvoiceHeader header;
	}
}
