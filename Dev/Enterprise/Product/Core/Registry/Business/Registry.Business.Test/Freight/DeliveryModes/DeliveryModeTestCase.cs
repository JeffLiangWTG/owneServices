using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeliveryMode))]
	sealed class DeliveryModeTestCase : RegistryBusinessObjectTestCaseBase
	{
		public void TestReadElements()
		{
			var xml =
@"<DeliveryMode>
    <CodeMaxLength>7</CodeMaxLength>
    <Code>AAA</Code>
    <Description>AAA Description</Description>
    <UserDefinedCode>BBB</UserDefinedCode>
    <UserDefinedDescription>BBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB Description </UserDefinedDescription>
    <IsSystemDefined>Y</IsSystemDefined>
</DeliveryMode>";
			using (var stringReader = new StringReader(xml))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var bizObj = new DeliveryMode();
				xmlReader.ReadStartElement();
				((IXmlSerializable)bizObj).ReadXml(xmlReader);

				AssertEquals(7, bizObj.CodeMaxLength);
				AssertEquals("AAA", bizObj.Code);
				AssertEquals("AAA Description", bizObj.Description);
				AssertEquals("BBB", bizObj.UserDefinedCode);
				AssertEquals("BBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB Description ", bizObj.UserDefinedDescription);
				AssertEquals(256, bizObj.UserDefinedDescription_MaxLength);
				AssertEquals(true, bizObj.IsSystemDefined);
			}
		}

		public void TestWriteElements()
		{
			var bizObj = new DeliveryMode();
			bizObj.Code = "AAA";
			bizObj.Description = (NoResString)"AAA Description";
			bizObj.UserDefinedCode = "BBB";
			bizObj.UserDefinedDescription = (NoResString)"BBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB Description ";
			bizObj.CodeMaxLength = 7;
			bizObj.IsSystemDefined = true;
			string xml;
			using (var stream = new StringWriter())
			using (var writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("DeliveryMode");
				((IXmlSerializable)bizObj).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				xml = stream.ToString();
			}

			var expectedXml = @"<DeliveryMode><CodeMaxLength>7</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description><UserDefinedCode>BBB</UserDefinedCode><UserDefinedDescription>BBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB DescriptionBBB Description </UserDefinedDescription><IsSystemDefined>Y</IsSystemDefined></DeliveryMode>";

			AssertMultilineASCIIEquals(expectedXml, xml);
		}

		public void TestValidateUserDefinedCode_CheckIsUnique()
		{
			string errorMessage = string.Format("The User Defined Code has been duplicated and must be unique.");

			var collection = new DeliveryModeCollection();

			var bizObj1 = collection.AddNew();
			var bizObj2 = collection.AddNew();
			var bizObj3 = collection.AddNew();

			collection.Add(bizObj1);
			collection.Add(bizObj2);
			collection.Add(bizObj3);

			bizObj1.UserDefinedCode = "ABC";
			bizObj2.UserDefinedCode = "ABC";
			bizObj3.UserDefinedCode = "XYZ";

			AssertEquals("BizObj1.UserDefinedCode should not have errors", false, bizObj1.UserDefinedCodeInfo.HasError(errorMessage));
			AssertEquals("BizObj2.UserDefinedCode should have an error because it is not unique", true, bizObj2.UserDefinedCodeInfo.HasError(errorMessage));
			AssertEquals("BizObj3.UserDefinedCode should not have errors", false, bizObj3.UserDefinedCodeInfo.HasError(errorMessage));

			bizObj2.UserDefinedCode = "POP";
			AssertEquals("Precondition: BizObj1.UserDefinedCode should have no errors", false, bizObj1.UserDefinedCodeInfo.HasErrors());
			AssertEquals("Precondition: BizObj2.UserDefinedCode should have no errors", false, bizObj2.UserDefinedCodeInfo.HasErrors());
			AssertEquals("Precondition: BizObj3.UserDefinedCode should have no errors", false, bizObj1.UserDefinedCodeInfo.HasErrors());
		}

		public void TestReadOnlyStates()
		{
			var mode = GetNewBusinessObject() as DeliveryMode;

			mode.IsSystemDefined = true;
			AssertEquals("If mode is system defined, Code should be read only", true, mode.CodeInfo.ReadOnly);
			AssertEquals("If mode is system defined, Description should be read only", true, mode.DescriptionInfo.ReadOnly);
			AssertEquals("If mode is system defined, UserDefinedCode should not be read only", false, mode.UserDefinedCodeInfo.ReadOnly);
			AssertEquals("If mode is system defined, UserDefinedDescription should not be read only", false, mode.UserDefinedDescriptionInfo.ReadOnly);

			mode.IsSystemDefined = false;
			AssertEquals("If mode is not system defined, Code should not be read only", false, mode.CodeInfo.ReadOnly);
			AssertEquals("If mode is not system defined, Description should not be read only", false, mode.DescriptionInfo.ReadOnly);
			AssertEquals("If mode is not system defined, UserDefinedCode should not be read only", true, mode.UserDefinedCodeInfo.ReadOnly);
			AssertEquals("If mode is not system defined, UserDefinedDescription should not be read only", true, mode.UserDefinedDescriptionInfo.ReadOnly);
		}

		public void TestCanDelete()
		{
			var mode = GetNewBusinessObject() as DeliveryMode;

			mode.IsSystemDefined = true;
			AssertEquals("If mode is system defined, it can not been deleted", false, mode.CanDelete);

			mode.IsSystemDefined = false;
			AssertEquals("If mode is not system defined, it can be deleted", true, mode.CanDelete);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			DeliveryMode result = new DeliveryMode();

			result.CodeMaxLength = 7;
			result.Code = "CFS/CFS";
			result.Description = (NoResString)"CFS/CFS";
			result.UserDefinedCode = "CFS/CFS";
			result.UserDefinedDescription = (NoResString)"CFS/CFS";
			result.IsSystemDefined = true;

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
