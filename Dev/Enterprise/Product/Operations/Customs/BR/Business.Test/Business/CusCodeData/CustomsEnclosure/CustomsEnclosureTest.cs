using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CustomsEnclosure))]
	public class CustomsEnclosureTest : Customs.Business.Testing.CusCodeDataTest<CustomsEnclosure>
	{
		public void TestSetDefaultValues()
		{
			var enclosure = Factory.New<CustomsEnclosure>();
			AssertEquals("CY_Type", Common.BR.CusCodeDataTypeList.Codes.CustomsEnclosure, enclosure.CY_Type);
		}

		public void TestValidation()
		{
			var attribute = Factory.New<CustomsEnclosure>();
			AssertType<CustomsEnclosureValidation>(attribute.Validation);
		}

		public void TestDelete()
		{
			var enclosure = CreateNewCustomsEnclosure(Factory);
			enclosure.CY_Data = "1234567";
			Factory.Save();
			Assert("CustomsEnclosure should be saved when CY_Data is not empty", enclosure.IsInDatabase);

			enclosure.CY_Data = "";
			Factory.Save();
			Assert("CustomsEnclosure should be deleted when CY_Data is empty", enclosure.IsDeleted);
		}

		CustomsEnclosure CreateNewCustomsEnclosure(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().CustomsEnclosures.AddNew(Constants.CustomsOfficeCodes.BoardingOffice, "1234567");
		}

		protected override IEnumerable<CustomsEnclosure> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateNewCustomsEnclosure(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewCustomsEnclosure(factory);

		protected override BusinessObject GetNewBusinessObject() => CreateNewCustomsEnclosure(Factory);
	}
}
