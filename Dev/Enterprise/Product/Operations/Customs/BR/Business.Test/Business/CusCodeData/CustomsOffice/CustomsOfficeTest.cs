using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CustomsOffice))]
	public class CustomsOfficeTest : Customs.Business.Testing.CusCodeDataTest<CustomsOffice>
	{
		public void TestSetDefaultValues()
		{
			var enclosure = Factory.New<CustomsOffice>();
			AssertEquals("CY_Type", Common.BR.CusCodeDataTypeList.Codes.CustomsOffice, enclosure.CY_Type);
		}

		public void TestValidation()
		{
			var attribute = Factory.New<CustomsOffice>();
			AssertType<CustomsOfficeValidation>(attribute.Validation);
		}

		public void TestDelete()
		{
			var enclosure = CreateNewCustomsOffice(Factory);
			enclosure.CY_Data = "1234567";
			Factory.Save();
			Assert("CustomsEnclosure should be saved when CY_Data is not empty", enclosure.IsInDatabase);

			enclosure.CY_Data = "";
			enclosure.CY_IsOverridden = true;
			Factory.Save();
			Assert("CustomsEnclosure should be saved when CY_IsOverridden is true", enclosure.IsInDatabase);

			enclosure.CY_IsOverridden = false;
			Factory.Save();
			Assert("CustomsEnclosure should be deleted when CY_Data is empty & CY_IsOverridden is false", enclosure.IsDeleted);
		}

		CustomsOffice CreateNewCustomsOffice(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().CustomsOffices.AddNew(Constants.CustomsOfficeCodes.BoardingOffice, "1234567");
		}

		protected override IEnumerable<CustomsOffice> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateNewCustomsOffice(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewCustomsOffice(factory);

		protected override BusinessObject GetNewBusinessObject() => CreateNewCustomsOffice(Factory);
	}
}
