using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusAttributeFilterValidationTest : BusinessObjectValidationTestCase
	{
		delegate CusAttributeFilterCollection GetAttributesDelegate(CusClassPartPivot pivot);

		public void TestCheckBG_AttributeValue1()
		{
			AssertAttributeValue1(x => x.Attributes1, x => x.Attributes2, x => x.Attributes3, CusAttributeFilterValidation.AttributeName.Attribute1);
			AssertAttributeValue1(x => x.Attributes2, x => x.Attributes1, x => x.Attributes3, CusAttributeFilterValidation.AttributeName.Attribute2);
			AssertAttributeValue1(x => x.Attributes3, x => x.Attributes2, x => x.Attributes1, CusAttributeFilterValidation.AttributeName.Attribute3);
		}

		void AssertAttributeValue1(GetAttributesDelegate getAttributesToTest, GetAttributesDelegate getOtherAttributes1, GetAttributesDelegate getOtherAttributes2, string attributeType)
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var attrib1 = getAttributesToTest(pivot).AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var attrib2 = getOtherAttributes1(pivot).AddNew();
			attrib2.BG_AttributeValue1 = "1";
			var attrib3 = getOtherAttributes2(pivot).AddNew();
			attrib3.BG_AttributeValue1 = "1";

			string messageError = CusAttributeFilterValidation.AttributeValueShouldBeUniqueFor(attributeType);
			var attrib4 = getAttributesToTest(pivot).AddNew();
			attrib4.BG_AttributeValue1 = "1";
			AssertHasError(attrib4.BG_AttributeValue1Info, messageError);

			attrib4.BG_AttributeValue1 = "2";
			AssertNoError(attrib4.BG_AttributeValue1Info, messageError);

			attrib1.BG_AttributeValue1 = "2";
			attrib4.BG_AttributeValue1 = "1";
			AssertNoError(attrib4.BG_AttributeValue1Info, messageError);

			attrib4.BG_AttributeValue1 = "2";
			AssertHasError(attrib4.BG_AttributeValue1Info, messageError);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			attrib4.BG_AttributeValue1 = "2";
			AssertNoError(attrib4.BG_AttributeValue1Info, messageError);
		}
	}
}
