using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers.Testing
{
	sealed class DocJPAFRHeaderTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocJPAFRHeader.New(Header, Factory);
		}

		[TestDate(2020, 8, 6)]
		public void TestCurrentJapanYear()
		{
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			AssertEquals("2", testHeaderWrapper.CurrentJapanYear);
		}

		public void TestReporterName_BranchAddress_BranchPhone_ReporterID()
		{
			var testCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			testCompany1.GC_Name = "TestCompany1";
			var testCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			testCompany2.GC_Name = "TestCompany2";
			var testBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			testBranch1.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testBranch1.GB_GC = testCompany1.PK;
			testBranch1.GB_Address1 = "Address1 A";
			testBranch1.GB_Address2 = "Address1 B";
			testBranch1.GB_City = "City1";
			testBranch1.GB_State = "State1";
			testBranch1.GB_RL_NKHomePort = "JPABA";
			testBranch1.GB_Phone = "0426 829 924";
			var testBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			testBranch2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testBranch2.GB_GC = testCompany2.PK;
			testBranch2.GB_Address1 = "Address2 A";
			testBranch2.GB_Address2 = "Address2 B";
			testBranch2.GB_City = "City2";
			testBranch2.GB_State = "State2";
			testBranch2.GB_RL_NKHomePort = "AUSYD";
			testBranch2.GB_Phone = "0420 019 999";
			JPAFRRegistry.Instance.AFRReporterIDForDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AFRReporterID { ReporterID = "SYSID", Password = "123" });
			JPAFRRegistry.Instance.AFRReporterIDForDocument.SetValue(testCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, new AFRReporterID { ReporterID = "1STID", Password = "456" });
			Factory.Save();

			CombineAssertions(() =>
			{
				var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
				Header.JPH_GB_Branch = testBranch1.PK;
				AssertEquals("First ReporterName", "TestCompany1", testHeaderWrapper.ReporterName);
				AssertEquals("First BranchAddress", "Address1 A Address1 B City1 State1 JP", testHeaderWrapper.BranchAddress);
				AssertEquals("First BranchPhone", "+61 426 829 924", testHeaderWrapper.BranchPhone);
				AssertEquals("First ReporterID", "1STID", testHeaderWrapper.ReporterID);

				Header.JPH_GB_Branch = testBranch2.PK;
				AssertEquals("Second ReporterName", "TestCompany2", testHeaderWrapper.ReporterName);
				AssertEquals("Second BranchAddress", "Address2 A Address2 B City2 State2 AU", testHeaderWrapper.BranchAddress);
				AssertEquals("Second BranchPhone", "+61 420 019 999", testHeaderWrapper.BranchPhone);
				AssertEquals("Second ReporterID", "SYSID", testHeaderWrapper.ReporterID);
			});
		}

		public void TestVesselName_VesselCallSign_VesslRegistrationCountry()
		{
			var testVessel1 = Factory.NewWithValidTestData<RefVessel>();
			testVessel1.RV_Code = "TestVessel1";
			testVessel1.RV_RadioCallSign = "CallSign1";
			testVessel1.RV_RN_NKCountryOfReg = "JP";
			var testVessel2 = Factory.NewWithValidTestData<RefVessel>();
			testVessel2.RV_Code = "TestVessel2";
			testVessel2.RV_RadioCallSign = "CallSign2";
			testVessel2.RV_RN_NKCountryOfReg = "AU";
			Factory.Save();

			CombineAssertions(() =>
			{
				var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
				Header.JPH_VesselName = "TestVessel1";
				AssertEquals("First VesselName", "TestVessel1", testHeaderWrapper.VesselName);
				AssertEquals("First VesselCallSign", "CallSign1", testHeaderWrapper.VesselCallSign);
				AssertEquals("First VesselRegistrationCountry", "JP", testHeaderWrapper.VesselRegisteredCountry);

				Header.JPH_VesselName = "TestVessel2";
				AssertEquals("Second VesselName", "TestVessel2", testHeaderWrapper.VesselName);
				AssertEquals("Second VesselCallSign", "CallSign2", testHeaderWrapper.VesselCallSign);
				AssertEquals("Second VesselRegistrationCountry", "AU", testHeaderWrapper.VesselRegisteredCountry);

				Header.JPH_VesselName = "NOTFOUND";
				AssertEquals("Empty VesselName", "NOTFOUND", testHeaderWrapper.VesselName);
				AssertEquals("Empty VesselCallSign", string.Empty, testHeaderWrapper.VesselCallSign);
				AssertEquals("Empty VesselRegistrationCountry", string.Empty, testHeaderWrapper.VesselRegisteredCountry);
			});
		}

		public void TestVoyageNumber()
		{
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_Voyage = "Test1";
			AssertEquals("Test1", testHeaderWrapper.VoyageNumber);
			Header.JPH_Voyage = "Test2";
			AssertEquals("Test2", testHeaderWrapper.VoyageNumber);
		}

		public void TestPortOfLoading()
		{
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_RL_NKLoading = "Test1";
			AssertEquals("TEST1", testHeaderWrapper.PortOfLoading);
			Header.JPH_RL_NKLoading = "Test2";
			AssertEquals("TEST2", testHeaderWrapper.PortOfLoading);
		}

		public void TestETD()
		{
			var testDate1 = new ZDateTime(ZDateTime.MaxSmallDateTime.AddYears(-1));
			var testDate2 = new ZDateTime(ZDateTime.MaxSmallDateTime);
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_ETD = testDate1;
			AssertEquals(testDate1, testHeaderWrapper.ETD);
			Header.JPH_ETD = testDate2;
			AssertEquals(testDate2, testHeaderWrapper.ETD);
		}

		public void TestPortOfDischarge()
		{
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_RL_NKDischarge = "Test1";
			AssertEquals("TEST1", testHeaderWrapper.PortOfDischarge);
			Header.JPH_RL_NKDischarge = "Test2";
			AssertEquals("TEST2", testHeaderWrapper.PortOfDischarge);
		}

		public void TestETA()
		{
			var testDate1 = new ZDateTime(ZDateTime.MaxSmallDateTime.AddYears(-1));
			var testDate2 = new ZDateTime(ZDateTime.MaxSmallDateTime);
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_ETA = testDate1;
			AssertEquals(testDate1, testHeaderWrapper.ETA);
			Header.JPH_ETA = testDate2;
			AssertEquals(testDate2, testHeaderWrapper.ETA);
		}

		public void TestCarrierCode()
		{
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_CarrierCode = "Tes1";
			AssertEquals("Tes1", testHeaderWrapper.CarrierCode);
			Header.JPH_CarrierCode = "Tes2";
			AssertEquals("Tes2", testHeaderWrapper.CarrierCode);
		}

		public void TestMasterBillNumber()
		{
			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			Header.JPH_MasterBillNumber = "Test1";
			AssertEquals("Test1", testHeaderWrapper.MasterBillNumber);
			Header.JPH_MasterBillNumber = "Test2";
			AssertEquals("Test2", testHeaderWrapper.MasterBillNumber);
		}

		public void TestContainers_Bills()
		{
			var testBill1 = Header.Bills.AddNew();
			testBill1.JPB_BillNumber = "HB1";
			var testBill2 = Header.Bills.AddNew();
			testBill2.JPB_BillNumber = "HB2";
			var testBill3 = Header.Bills.AddNew();
			testBill3.JPB_BillNumber = "HB3";
			var testBill4 = Header.Bills.AddNew();
			testBill4.JPB_BillNumber = "HB3";
			var testBill5 = Header.Bills.AddNew();
			var testCont11 = testBill1.Containers.AddNew();
			testCont11.JPC_ContainerNum = "Cont11";
			var testCont12 = testBill1.Containers.AddNew();
			testCont12.JPC_ContainerNum = "Cont12";
			var testCont13 = testBill1.Containers.AddNew();
			testCont13.JPC_ContainerNum = "Cont12";
			var testCont21 = testBill2.Containers.AddNew();
			testCont21.JPC_ContainerNum = "Cont21";
			var testCont41 = testBill4.Containers.AddNew();
			testCont41.JPC_ContainerNum = "Cont41";
			var testCont42 = testBill4.Containers.AddNew();
			testCont42.JPC_ContainerNum = "Cont12";

			var testHeaderWrapper = DocJPAFRHeader.New(Header, Factory);
			AssertEquals(5, testHeaderWrapper.Bills.Count);
			AssertEquals(1, testHeaderWrapper.Bills.Count(bill => bill is DocJPAFRBills && (bill as DocJPAFRBills).HouseBillNumber == "HB1"));
			AssertEquals(1, testHeaderWrapper.Bills.Count(bill => bill is DocJPAFRBills && (bill as DocJPAFRBills).HouseBillNumber == "HB2"));
			AssertEquals(2, testHeaderWrapper.Bills.Count(bill => bill is DocJPAFRBills && (bill as DocJPAFRBills).HouseBillNumber == "HB3"));
			AssertEquals(1, testHeaderWrapper.Bills.Count(bill => bill is DocJPAFRBills && (bill as DocJPAFRBills).HouseBillNumber.IsEmpty));

			AssertEquals(8, testHeaderWrapper.Containers.Count);
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 1
				&& container.ContainerNumber == "Cont11"
				&& container.Bill.HouseBillNumber == "HB1";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 2
				&& container.ContainerNumber == "Cont12"
				&& container.Bill.HouseBillNumber == "HB1";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 3
				&& container.ContainerNumber == "Cont12"
				&& container.Bill.HouseBillNumber == "HB1";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 1
				&& container.ContainerNumber == "Cont21"
				&& container.Bill.HouseBillNumber == "HB2";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 1
				&& container.ContainerNumber == ""
				&& container.Bill.HouseBillNumber == "HB3";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 1
				&& container.ContainerNumber == "Cont41"
				&& container.Bill.HouseBillNumber == "HB3";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 2
				&& container.ContainerNumber == "Cont12"
				&& container.Bill.HouseBillNumber == "HB3";
			}));
			AssertEquals(1, testHeaderWrapper.Containers.Count(cont =>
			{
				var container = cont as DocJPAFRContainer;
				return container != null
				&& container.ContainerSequence == 1
				&& container.ContainerNumber == ""
				&& container.Bill.HouseBillNumber == "";
			}));
		}

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;
	}
}
