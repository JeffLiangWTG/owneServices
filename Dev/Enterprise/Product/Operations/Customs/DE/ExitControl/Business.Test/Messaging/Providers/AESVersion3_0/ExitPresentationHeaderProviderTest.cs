using System;
using Enterprise.Customs.DE.Business.Testing;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class ExitPresentationHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<ExitPresentationHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ExitPresentationHeaderProvider(null));
		}

		public void TestExitCarrierContactPerson()
		{
			using (TestHelper.SetTemporaryCurrentUser(Factory, fullName: "John Citizen", workPhone: "9283 8272", email: "john.citizen@wisetechglobal.com"))
			{
				var exitCarrierContactPerson = Provider.ExitCarrierContactPerson;
				CombineAssertions(() =>
				{
					AssertEquals("PersonName", "John Citizen", exitCarrierContactPerson.PersonName);
					AssertEquals("PhoneNumber", "9283 8272", exitCarrierContactPerson.PhoneNumber);
					AssertEquals("MailAddress", "john.citizen@wisetechglobal.com", exitCarrierContactPerson.MailAddress);
				});
			}
		}

		public void TestAdditionalInformationCodes()
		{
			var info1 = consignment.AdditionalInfos.AddNew();
			info1.CSI_Code = "X";
			var info2 = consignment.AdditionalInfos.AddNew();
			info2.CSI_Code = "Y";
			var info3 = consignment.AdditionalInfos.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { "X", "Y" }, Provider.AdditionalInformationCodes);
		}

		protected override ExitPresentationHeaderProvider GetProvider() => new ExitPresentationHeaderProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
		}
		CusExitConsignment consignment;
		CusExitReport report;
	}
}
