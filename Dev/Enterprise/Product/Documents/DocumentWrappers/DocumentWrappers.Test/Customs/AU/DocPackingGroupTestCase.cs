using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocPackingGroup))]
	sealed class DocPackingGroupTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocPackingGroup.New(Packs, Factory) };
		}

		public void TestHouseBillNumber()
		{
			BillBizObj.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			BillBizObj.CU_BillNum = "HBL";
			AssertEquals("House Bill Number", "HBL", PacksWrapper.HouseBillNumber);

			BillBizObj.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			AssertEquals("House Bill Number for a master bill type", "", PacksWrapper.HouseBillNumber);
		}

		public void TestMasterBillNumber()
		{
			BillBizObj.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			BillBizObj.CU_BillNum = "ABC";
			AssertEquals("Master Bill Number", "ABC", PacksWrapper.MasterBillNumber);
		}

		public void TestPartShipConsignmentReference()
		{
			BillBizObj.CU_fPartShipConsignmentReference = "ABC";
			AssertEquals("Master Bill Number", "ABC", PacksWrapper.PartShipConsignmentReference);
		}

		public void TestContainerNumber()
		{
			CusContainer.CO_ContainerNumber = "CONT123";
			AssertEquals("Container number", "CONT123", PacksWrapper.ContainerNumber);
		}

		public void TestContainerType()
		{
			CusContainer.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("Container Type", "FCL", PacksWrapper.ContainerType);
		}

		public void TestFormattedContainerType()
		{
			CusContainer.CO_ContainerNumber = "CONT123";
			CusContainer.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("Formatted Container Type", "(FCL)CONT123", PacksWrapper.FormattedContainerType);
			Packs.CR_CO_Container = ZGuid.Empty;
			AssertEquals("Formatted Container Type", ZString.Empty, PacksWrapper.FormattedContainerType);
		}

		protected override string TestingCountry
		{
			get { return null; }
		}

		new string StoredCountry;
		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			Declaration = Factory.New<JobDeclaration>();
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Packs = Declaration.PackingGroups.AddNew();
			Packs.Declaration = Declaration;

			BillBizObj = Declaration.Bills.AddNew();
			CusContainer = Declaration.CusContainers.AddNew();

			Packs.CR_CO_Container = CusContainer.PK;
			Packs.CR_CU_HouseBill = BillBizObj.PK;

			PacksWrapper = DocPackingGroup.New(Packs, Factory);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			base.TearDown();
		}

		JobDeclaration Declaration;

		PackingGroup Packs;
		Bill BillBizObj;
		CusContainer CusContainer;
		DocPackingGroup PacksWrapper;
	}
}
