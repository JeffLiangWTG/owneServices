using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class ExitHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<ExitHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ExitHeaderProvider(null));
		}

		public void TestLRN_EmptyMRN()
		{
			consignment.CXC_LocalReference = "LRN123";
			AssertEquals("LRN123", Provider.LRN);
		}

		public void TestMRN()
		{
			consignment.CXC_MovementReference = "MRN456";
			AssertEquals("MRN456", Provider.MRN);
		}

		public void TestActualExitCustomsOffice()
		{
			report.CER_OfficeOfExit = "DE027";
			AssertEquals("DE027", Provider.ActualExitCustomsOffice);
		}

		public void TestDeclarant()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "EOR123", Core.Constants.CountryCodes.Greece);
			org.MainAddress.CustomsCodes.AddNew("EBS", "EBS123", Core.Constants.CountryCodes.Germany);
			report.Declarant.E2_OA_Address = org.MainAddress.PK;
			var declarant = Provider.Declarant;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR123", declarant.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS123", declarant.EoriBranchSuffix);
			});
		}

		public void TestRepresentative()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "EOR123", Core.Constants.CountryCodes.Greece);
			org.MainAddress.CustomsCodes.AddNew("EBS", "EBS123", Core.Constants.CountryCodes.Germany);
			report.Representative.E2_OA_Address = org.MainAddress.PK;
			var representative = Provider.Representative;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR123", representative.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS123", representative.EoriBranchSuffix);
			});
		}

		public void TestExitCarrier()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "EOR123", Core.Constants.CountryCodes.Greece);
			org.MainAddress.CustomsCodes.AddNew("EBS", "EBS123", Core.Constants.CountryCodes.Germany);
			header.CXH_OA_Carrier = org.MainAddress.PK;
			var carrier = Provider.ExitCarrier;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR123", carrier.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS123", carrier.EoriBranchSuffix);
			});
		}

		public void TestRegistrationNumberExternal()
		{
			consignment.CXC_ReferenceNumber = "REF789";
			AssertEquals("REF789", Provider.RegistrationNumberExternal);
		}

		protected override ExitHeaderProvider GetProvider() => new ExitHeaderProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
		}
		CusExitHeader header;
		CusExitConsignment consignment;
		CusExitReport report;
	}
}
