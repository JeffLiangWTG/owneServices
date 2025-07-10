using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CustomsOffice))]
	class CustomsOfficeTest : Customs.Business.Testing.CusCodeDataTest<CustomsOffice>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, customsOffice.SupportsNotes);
		}

		public void TestSetDefaultValues()
		{
			var office = Factory.New<CustomsOffice>();
			AssertEquals("CY_Type", Constants.CusCodeDataTypes.Codes.CustomsOffice, office.CY_Type);
			AssertEquals("CY_Code", CustomsOfficeTypeList.Codes.DES, office.CY_Code);
		}

		public void TestCY_Code()
		{
			customsOffice.CY_Code = CustomsOfficeTypeList.Codes.DES;
			AssertEquals("目的地海关", customsOffice.Description);
		}

		public void TestSaving()
		{
			Assert("Customs Office not saved", !customsOffice.IsInDatabase);
			Factory.Save();
			Assert("Customs Office should be saved", customsOffice.IsInDatabase);
			customsOffice.CY_Data = "ZZZ";
			Factory.Save();
			Assert("Nonempty Customs Office should NOT be deleted", !customsOffice.IsDeleted);
			customsOffice.CY_Data = ZString.Empty;
			Factory.Save();
			Assert("Empty Customs Office should be deleted", customsOffice.IsDeleted);
		}

		public void TestIsSavedByFactory()
		{
			customsOffice.CY_Data = ZString.Empty;
			Assert("IsSavedByFactory", !customsOffice.IsSavedByFactory);
			customsOffice.CY_Data = "XXX";
			Assert("IsSavedByFactory", customsOffice.IsSavedByFactory);
			customsOffice.Delete();
			Assert("IsSavedByFactory", !customsOffice.IsSavedByFactory);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewCustomsOffice(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewCustomsOffice(factory);

		protected override IEnumerable<CustomsOffice> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewCustomsOffice(factory);
		}

		CustomsOffice GetNewCustomsOffice(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var customsOffice = factory.New<CustomsOffice>();
			customsOffice.CY_ParentID = declaration.PK;
			customsOffice.CY_ParentTableCode = declaration.TablePrefix;
			customsOffice.CY_Code = CustomsOfficeTypeList.Codes.DES;
			customsOffice.CY_Data = "XXX";
			return customsOffice;
		}

		CustomsOffice customsOffice => BusinessObject as CustomsOffice;
	}
}
