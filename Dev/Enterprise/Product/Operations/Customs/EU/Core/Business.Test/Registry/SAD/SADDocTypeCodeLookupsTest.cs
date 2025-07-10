using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing.Registry
{
	public class SADDocTypeCodeLookupsTest : TestCaseWithFactory
	{
		public void TestDocTypeCodeList()
		{
			var newDocType = Factory.New<RefDocType>();
			newDocType.RT_Desc = "New Z Document Type";
			newDocType.RT_DocType = "ZZZ";
			newDocType.RT_IsActive = ZBool.True;
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			newDocType.RT_SaveVersions = true;

			var anotherNewDocType = Factory.New<RefDocType>();
			anotherNewDocType.RT_Desc = "New Y Document Type";
			anotherNewDocType.RT_DocType = "YYY";
			anotherNewDocType.RT_IsActive = ZBool.True;
			anotherNewDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			anotherNewDocType.RT_IsPublished = true;

			var andAnotherNewDocType = Factory.New<RefDocType>();
			andAnotherNewDocType.RT_Desc = "New A Document Type";
			andAnotherNewDocType.RT_DocType = "XXX";
			andAnotherNewDocType.RT_IsActive = ZBool.True;
			andAnotherNewDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;

			var actualList = SADDocTypeCodeLookups.GetCachedAllOrSupplyChainLogisticsDocTypeList(Factory);
			AssertEquals("RT_ReferenceType : SupplyChainLogistics", "New Z Document Type", actualList.GetDescriptionFromCode("ZZZ"));
			AssertEquals("RT_ReferenceType : All", "New A Document Type", actualList.GetDescriptionFromCode("XXX"));
			AssertEquals("RT_ReferenceType : ClientSupplierRelationship", null, actualList.GetDescriptionFromCode("YYY"));
		}
	}
}
