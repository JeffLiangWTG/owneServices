using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AdditionalElementWrapper))]
	class AdditionalElementWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "99999", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "X", parent, EnteringOrExiting.Both);
			AssertEquals("ElementCode", "99999", wrapper.ElementCode);
			AssertEquals("ElementName", "其他", wrapper.ElementName);
			AssertEquals("ElementValue", "X", wrapper.ElementValue);
		}

		public void TestValidateElementValue()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Exiting);
			wrapper.RunPreSaveValidation();
			AssertHasMessageErrorContaining(wrapper.ElementValueInfo, "enter");
			wrapper.ElementValue = "AA|AA";
			AssertHasErrorContaining(wrapper.ElementValueInfo, "The value has special character '|'.");
			AssertNoMessageErrorContaining(wrapper.ElementValueInfo, "enter");
			wrapper.ElementValue = "A";
			AssertNoErrorContaining(wrapper.ElementValueInfo, "The value has special character '|'.");
			AssertHasMessageErrorContaining(wrapper.ElementValueInfo, "The code you have selected is not in the list");
			wrapper.ElementValue = "1";
			AssertNoMessageErrorContaining(wrapper.ElementValueInfo, "The value has special character '|'.");
			AssertNoMessageErrorContaining(wrapper.ElementValueInfo, "The code you have selected is not in the list");
			wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Entering, false);
			wrapper.RunPreSaveValidation();
			AssertHasWarningContaining(wrapper.ElementValueInfo, "This additional information is not available for the selected Tariff");
		}

		public void TestElementValue_FieldType()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Both);
			AssertEquals("ElementValue_FieldType", nameof(FieldType.TextDropEdit), wrapper.ElementValue_FieldType);
			additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "99999", ZDateTime.Today);
			wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Both);
			AssertEquals("ElementValue_FieldType", nameof(FieldType.Text), wrapper.ElementValue_FieldType);
		}

		public void TestElementValue_MaxLength()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00000", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Both);
			AssertEquals("ElementValue_MaxLength", 50, wrapper.ElementValueInfo.MaxLength);
			additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "99999", ZDateTime.Today);
			wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Both);
			AssertEquals("ElementValue_MaxLength", 512, wrapper.ElementValueInfo.MaxLength);
		}

		public void TestElementValue_List()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Exiting);
			AssertContainsExactElementsInAnyOrder("ElementValue_List", new[] { "0", "1", "2" }, ((CodeDescriptionPairList)wrapper.ElementValue_List).GetAllCodes());
			wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Entering);
			AssertContainsExactElementsInAnyOrder("ElementValue_List", new[] { "3" }, ((CodeDescriptionPairList)wrapper.ElementValue_List).GetAllCodes());
		}

		public void TestElementValue_ReadOnly()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Entering);
			Assert("ElementValue_ReadOnly", !wrapper.ElementValueInfo.ReadOnly);
			wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Entering, false);
			Assert("ElementValue_ReadOnly", wrapper.ElementValueInfo.ReadOnly);
		}

		public void TestDefaultElementValue()
		{
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today);
			var wrapper = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Entering);
			AssertEquals("Default ElementValue", "3", wrapper.ElementValue);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalElementWrapper(CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today), "", parent, EnteringOrExiting.Both);
		}

		IAdditionalInformationWrapperParent parent;
		protected override void SetUp()
		{
			base.SetUp();
			parent = new DummyAdditionalInformationWrapperParent(Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
		}
	}
}
