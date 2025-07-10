using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeader))]
	class CusInBondMoveHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			AssertEquals(CusInBondMoveHeader.PermitNumberType, moveHeader.BM_SubApplicationCode);
		}

		public void TestICusInBondMoveHeader()
		{
			AssertType<CusInBondMoveHeader>(Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>());
		}

		public void TestCalculateProperties()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			var permitNumberBizObj = moveHeader.PermitNumberBizObj;
			permitNumberBizObj.CE_EntryNum = "1234";
			permitNumberBizObj.CE_IssueDate = ZDateTime.BrettsBirthday;
			permitNumberBizObj.CE_ExpiryDate = new ZDateTime(2021, 6, 6);
			CombineAssertions(() =>
			{
				AssertEquals("BM_Calc_PermitNumber", "1234", moveHeader.BM_Calc_PermitNumber);
				AssertEquals("BM_Calc_IssueDate", ZDateTime.BrettsBirthday, moveHeader.BM_Calc_IssueDate);
				AssertEquals("BM_Calc_ValidityDate", new ZDateTime(2021, 6, 6), moveHeader.BM_Calc_ValidityDate);
			});
		}

		public void TestPermitNumberBizObj()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			var permitNumberBizObj = moveHeader.PermitNumberBizObj;
			CombineAssertions(() =>
			{
				AssertType<CusEntryNumber>("should be CusEntryNumber", permitNumberBizObj);
				AssertEquals("CE_ParentID", moveHeader.PK, permitNumberBizObj.CE_ParentID);
				AssertEquals("CE_EntryType", CusInBondMoveHeader.PermitNumberType, permitNumberBizObj.CE_EntryType);
				AssertEquals("CE_RN_NKCountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, permitNumberBizObj.CE_RN_NKCountryCode);
			});
		}

		public void TestDocumentSupporter()
		{
			var moveHeader = (CusInBondMoveHeader)GetNewBusinessObjectForDeleteTest(Factory);
			AssertType<CusInBondMoveHeaderDocumentSupporter>(moveHeader.DocumentSupporter);
		}

		public void TestDocManagerInfo()
		{
			var moveHeader = (CusInBondMoveHeader)GetNewBusinessObjectForDeleteTest(Factory);
			AssertNotNull(moveHeader.DocManagerInfo);
			AssertEquals(Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader, moveHeader.DocManagerInfo.DocManagerCode);
		}

		public void TestGetWarningBeforeBeingDeleted()
		{
			var moveHeader = (CusInBondMoveHeader)GetNewBusinessObjectForDeleteTest(Factory);
			moveHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMainForPK(moveHeader.PK, Core.Constants.RefDocTypes.AsycudaCusInBondMoveHeader);
			moveHeader.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3, 4 }, "test2", "TXT");
			moveHeader.DocManagerInfo.MasterFactory.Save();
			var warningMessage = moveHeader.GetWarningBeforeBeingDeleted();
			AssertEquals("This transit permit has related eDoc(s).", warningMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return moveHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			factory.Save();
			var header = factory.New<CusInBondHeader>();
			header.BH_GB = branch.PK;
			var moveHeader = factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return moveHeader;
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new CusInBondMoveHeaderLightValidationTester(bizObjToTest);

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		public void TestFirstMoveDetail()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			AssertNotNull(moveHeader.FirstMoveDetail);
		}

		public void TestSaveAndDelete()
		{
			var moveHeader = GetNewBusinessObject() as CusInBondMoveHeader;
			moveHeader.MovementDetails.AddNew();
			moveHeader.MovementDetails.AddNew();
			Factory.Save();

			AssertNotNull(Factory.Load<CusInBondMoveHeader>(moveHeader.PK));
			AssertEquals(2, Factory.Load<CusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeader.PK)).Length);
			moveHeader.Delete();
			AssertEquals(0, Factory.Load<CusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeader.PK)).Length);
		}

		public void TestHumanReadableNameCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var moveHeader = entryInstruction.CusInBondPermitsHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertHumanReadableNameCore(moveHeader, "PermitNumber", "AB", "desc", "PermitNumber-AB-desc");
				AssertHumanReadableNameCore(moveHeader, "", "AB", "desc", "AB-desc");
				AssertHumanReadableNameCore(moveHeader, "", "", "desc", "desc");
				AssertHumanReadableNameCore(moveHeader, "PermitNumber", "", "desc", "PermitNumber-desc");
				AssertHumanReadableNameCore(moveHeader, "PermitNumber", "AB", "", "PermitNumber-AB");
				AssertHumanReadableNameCore(moveHeader, "PermitNumber", "", "", "PermitNumber");
			});
		}

		void AssertHumanReadableNameCore(CusInBondMoveHeader moveHeader, ZString permitNumber, ZString style, ZString description, ZString expectName)
		{
			moveHeader.BM_Calc_PermitNumber = permitNumber;
			moveHeader.EntryInstruction.CEI_Style = style;
			moveHeader.EntryInstruction.CEI_Description = description;
			AssertEquals(expectName, moveHeader.HumanReadableName);
		}

		public void TestAddAllContainers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "NUMBER";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "";
			var moveHeader = entryInstruction.CusInBondPermitsHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("before add all", 0, moveHeader.FirstMoveDetail.Containers.Count);
				moveHeader.AddAllContainers();
				AssertEquals("after add all, count is 1", 1, moveHeader.FirstMoveDetail.Containers.Count);
				AssertEquals("after add all, BC_ContainerNum", "NUMBER", moveHeader.FirstMoveDetail.Containers[0].BC_ContainerNum);
				var container0 = moveHeader.FirstMoveDetail.Containers[0];

				var container2 = declaration.CusContainers.AddNew();
				container2.CO_ContainerNumber = "NUMBER2";
				moveHeader.AddAllContainers();
				AssertEquals("after add all again", 2, moveHeader.FirstMoveDetail.Containers.Count);
				AssertEquals("after add all again, BC_ContainerNum from container not change", container0, moveHeader.FirstMoveDetail.Containers[0]);
				AssertEquals("after add all again, BC_ContainerNum from container 2", "NUMBER2", moveHeader.FirstMoveDetail.Containers[1].BC_ContainerNum);
			});
		}

		public void TestMoveDetailType()
		{
			var moveHeader = GetNewBusinessObject() as CusInBondMoveHeader;
			AssertEquals(typeof(CusInBondMoveDetail), moveHeader.MovementDetailType);
		}
	}
}
