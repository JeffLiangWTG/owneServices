using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnModeAddInfoBizObj))]
	class OrgSupBuyLinkTrnModeAddInfoBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeleteCusCodeDatasWithOrgSupBuyLinkTrnMode()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
			var trnMode = (OrgSupBuyLinkTrnMode)(link.OrgSupBuyLinkTrnModes.FirstOrDefault() ?? link.OrgSupBuyLinkTrnModes.AddNew());
			trnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			trnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			var addInfo1 = (OrgSupBuyLinkTrnModeAddInfo)trnMode.AddInfo;
			var addInfoBizObj = new OrgSupBuyLinkTrnModeAddInfoBizObj(addInfo1);
			addInfoBizObj.OfficeOfDestination = "4100";
			Factory.Save();
			AssertCusCodeDataCount("CusCodeData should be saved with OrgSupBuyLinkTrnMode", 1);

			trnMode.Delete();
			Factory.Save();
			AssertCusCodeDataCount("CusCodeData should be deleted with OrgSupBuyLinkTrnMode", 0);

			void AssertCusCodeDataCount(string message, int count)
			{
				AssertEquals(message, count, new BusinessObjectFactory().Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, trnMode.PK)).Length);
			}
		}

		public void TestOfficeOfDestination()
		{
			var addInfoBizObj = GetNewBusinessObject() as OrgSupBuyLinkTrnModeAddInfoBizObj;
			AssertEquals(0, addInfoBizObj.CustomsOffices.Count);
			addInfoBizObj.OfficeOfDestination = "1111";
			AssertEquals(4, addInfoBizObj.OfficeOfDestinationInfo.MaxLength);
			AssertEquals(1, addInfoBizObj.CustomsOffices.Count);
			AssertEquals("CY_Code", CustomsOfficeTypeList.Codes.DES, addInfoBizObj.CustomsOffices[0].CY_Code);
			AssertEquals("CY_Data", "1111", addInfoBizObj.CustomsOffices[0].CY_Data);
			AssertHasMessageErrorContaining(addInfoBizObj.OfficeOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			addInfoBizObj.CustomsOffices[0].CY_Data = "2222";
			AssertEquals("2222", addInfoBizObj.OfficeOfDestination);
		}

		public void TestIsDeleted()
		{
			var link = Factory.New<OrgSupBuyLinkTrnMode>();
			var addInfo = link.AddInfo;
			var testItem = new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)addInfo);
			AssertEquals("New created OrgSupBuyLinkTrnModeAddInfoBizObj, IsDeleted shows false.", false, testItem.IsDeleted);

			link.Delete();
			AssertEquals("OrgSupBuyLinkTrnModeAddInfoBizObj should show IsDeleted when parent OrgSupBuyLinkTrnMode is deleted.", true, testItem.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject() => new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)Factory.New<OrgSupBuyLinkTrnMode>().AddInfo);
	}
}
