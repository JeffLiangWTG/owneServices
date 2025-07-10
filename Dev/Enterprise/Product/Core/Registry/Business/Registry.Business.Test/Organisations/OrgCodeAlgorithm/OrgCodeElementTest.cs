using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeElement))]
	sealed class OrgCodeElementTest : RegistryBusinessObjectTemplateTestCase<OrgCodeElement>
	{
		OrgCodeElement element;

		OrgCodeElement Element
		{
			get { return element ?? (element = (OrgCodeElement)GetNewBusinessObject()); }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override void CheckAllPropertiesAreEqual(OrgCodeElement originalBusinessObject, OrgCodeElement newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("LengthInfo.ReadOnly", originalBusinessObject.LengthInfo.ReadOnly, newBusinessObject.LengthInfo.ReadOnly);
		}

		protected override OrgCodeElement GetBusinessObjectToClone()
		{
			Element.Length = 5;
			Element.Order = 3;
			return Element;
		}

		protected override OrgCodeElement GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgCodeElement("ZZZ");
		}

		public void TestConstructors()
		{
			OrgCodeElement element = new OrgCodeElement(OrgCodeElementDescription.CountryCode);
			AssertEquals("Description", OrgCodeElementDescription.CountryCode, element.Description);
			AssertEquals("Length", ZByte.Zero, element.Length);
			AssertEquals("LengthInfo.ReadOnly", true, element.LengthInfo.ReadOnly);

			element = new OrgCodeElement(OrgCodeElementDescription.FirstName, 2);
			AssertEquals("Description", OrgCodeElementDescription.FirstName, element.Description);
			AssertEquals("Length", (ZByte)2, element.Length);
			AssertEquals("LengthInfo.ReadOnly", false, element.LengthInfo.ReadOnly);
		}

		public void TestEmpty()
		{
			AssertEquals("Empty", true, BizObj.Empty);
			BizObj.Length = 1;
			AssertEquals("Empty", true, BizObj.Empty);
			BizObj.Order = 1;
			AssertEquals("Empty", false, BizObj.Empty);
		}

		public void TestIOrgCodeElementDescription()
		{
			var result = ((IOrgCodeElement)BizObj).Description;
			AssertEquals("ZZZ", result);
		}

		public void TestIOrgCodeElementLength()
		{
			BizObj.Length = 10;
			var result = ((IOrgCodeElement)BizObj).Length;
			AssertEquals(10, result);
		}

		public void TestIOrgCodeElementOrder()
		{
			BizObj.Order = 5;
			var result = ((IOrgCodeElement)BizObj).Order;
			AssertEquals(5, result);
		}

		public void TestLengthInfoReadOnly()
		{
			OrgCodeElement element = new OrgCodeElement(OrgCodeElementDescription.CountryCode);
			AssertEquals("LengthInfo.ReadOnly", true, element.LengthInfo.ReadOnly);
			element = new OrgCodeElement(OrgCodeElementDescription.UnlocoCode);
			AssertEquals("LengthInfo.ReadOnly", true, element.LengthInfo.ReadOnly);
			element = new OrgCodeElement(OrgCodeElementDescription.IataCode);
			AssertEquals("LengthInfo.ReadOnly", true, element.LengthInfo.ReadOnly);

			element = new OrgCodeElement(OrgCodeElementDescription.FirstName);
			AssertEquals("LengthInfo.ReadOnly", false, element.LengthInfo.ReadOnly);
			element = new OrgCodeElement(OrgCodeElementDescription.SecondName);
			AssertEquals("LengthInfo.ReadOnly", false, element.LengthInfo.ReadOnly);
			element = new OrgCodeElement(OrgCodeElementDescription.LastName);
			AssertEquals("LengthInfo.ReadOnly", false, element.LengthInfo.ReadOnly);
			element = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber);
			AssertEquals("LengthInfo.ReadOnly", false, element.LengthInfo.ReadOnly);
			element = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber);
			AssertEquals("LengthInfo.ReadOnly", false, element.LengthInfo.ReadOnly);
		}

		public void TestRunPreSaveValidation()
		{
			Element.Length = 10;
			Element.Order = 10;
			using (Element.SuspendValidationTesting())
			{
				Element.ClearAllNotifications();
			}
			Element.RunPreSaveValidation();
			AssertHasErrors(Element.LengthInfo);
			AssertHasErrors(Element.OrderInfo);
		}

		public void TestSettingPropertiesRefreshesParentCurrentOrgCodeLength()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();

			algorithm.Elements[0].Order = 2;
			algorithm.Elements[0].Length = 5;
			AssertEquals("CurrentOrgCodeLength", (ZByte)5, algorithm.CurrentOrgCodeLength);

			algorithm.Elements[0].Length = 3;
			AssertEquals("CurrentOrgCodeLength", (ZByte)3, algorithm.CurrentOrgCodeLength);

			algorithm.Elements[0].Order = 0;
			AssertEquals("CurrentOrgCodeLength", ZByte.Zero, algorithm.CurrentOrgCodeLength);
		}

		public void TestValidateLength()
		{
			Element.Length = 10;
			AssertHasError(Element.LengthInfo, "Please enter a 'Length' less than or equal to 9.");

			Element.Length = 9;
			AssertNoErrors(Element.LengthInfo);
		}

		public void TestValidateOrder()
		{
			Element.Order = 8;
			AssertHasError(Element.OrderInfo, "Please enter an 'Order' less than or equal to 7.");

			Element.Order = 7;
			AssertNoErrors(Element.OrderInfo);

			OrgCodeElementCollection collection = new OrgCodeElementCollection();
			OrgCodeElement anotherElement = new OrgCodeElement("AAA");
			collection.Add(Element);
			collection.Add(anotherElement);

			anotherElement.Order = 7;
			AssertHasError(anotherElement.OrderInfo, "The Order has been duplicated and must be unique.");

			anotherElement.Order = 6;
			AssertNoErrors(anotherElement.OrderInfo);

			collection.RemoveAll();
			collection.Load();
			OrgCodeElement codeSpecificUniqueNumberElement = collection[OrgCodeElementDescription.CodeSpecificUniqueNumber];
			OrgCodeElement globallyUniqueNumberElement = collection[OrgCodeElementDescription.GloballyUniqueNumber];

			globallyUniqueNumberElement.Order = 1;
			codeSpecificUniqueNumberElement.Order = 2;
			AssertHasError(codeSpecificUniqueNumberElement.OrderInfo, "The Code Specific Unique Number cannot be included in the algorithm because the Globally Unique Number is already included.");

			codeSpecificUniqueNumberElement.Order = 0;
			AssertNoErrors(codeSpecificUniqueNumberElement.OrderInfo);

			OrgCodeElement firstNameElement = collection[OrgCodeElementDescription.FirstName];
			firstNameElement.Order = 2;
			globallyUniqueNumberElement.ValidateOrder();
			AssertHasError(globallyUniqueNumberElement.OrderInfo, "The Globally Unique Number must be the last element in the algorithm.");

			globallyUniqueNumberElement.Order = 3;
			AssertNoErrors(globallyUniqueNumberElement.OrderInfo);

			globallyUniqueNumberElement.Order = 0;
			codeSpecificUniqueNumberElement.Order = 1;
			AssertHasError(codeSpecificUniqueNumberElement.OrderInfo, "The Code Specific Unique Number must be the last element in the algorithm.");

			codeSpecificUniqueNumberElement.Order = 3;
			AssertNoErrors(codeSpecificUniqueNumberElement.OrderInfo);
		}
	}
}
