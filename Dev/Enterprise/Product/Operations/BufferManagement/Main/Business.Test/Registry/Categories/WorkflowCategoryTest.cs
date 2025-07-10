using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Testing
{
	[TestedType(typeof(WorkflowCategory))]
	sealed class WorkflowCategoryTest : RegistryBusinessObjectTest
	{
		protected override bool RequiresFactory => false;

		public void TestXmlSerialiseDeserialiseFromExistingRecord()
		{
			var existingRecord = new WorkflowCategory();
			existingRecord.Code = "HEY";
			existingRecord.Description = (NoResString)"Give me a good reason!";

			var builder = new StringBuilder();
			var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
			using (var writer = XmlWriter.Create(builder, settings))
			{
				writer.WriteStartElement("root");
				((IXmlSerializable)existingRecord).WriteXml(writer);
				writer.WriteEndElement();
			}

			var newRecord = new WorkflowCategory();
			using (var sReader = new StringReader(builder.ToString()))
			using (var reader = XmlReader.Create(sReader))
			{
				reader.Read();
				((IXmlSerializable)newRecord).ReadXml(reader);
			}

			AssertEquals("HEY", newRecord.Code);
			AssertEquals("Give me a good reason!", newRecord.Description);
		}

		public void TestXmlDeserialiseFromOldFormatRecord()
		{
			const string oldFormatXml = @"<root><CodeMaxLength>3</CodeMaxLength><Code>HEY</Code><Description>Give me a good reason!</Description></root>";

			var newRecord = new WorkflowCategory();
			using (var stringReader = new StringReader(oldFormatXml))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				xmlReader.Read();
				((IXmlSerializable)newRecord).ReadXml(xmlReader);
			}
			AssertEquals("HEY", newRecord.Code);
			AssertEquals("Give me a good reason!", newRecord.Description);
		}

		public void TestValidateCode()
		{
			var category = new WorkflowCategory();
			category.Code = BMConstants.JobLevelWorkflowCategoryCode;
			AssertHasError(category.CodeInfo, "The code 'JOB' is reserved and may not be used as a Workflow Category code. Please enter a different code");

			category.Code = "XXX";
			AssertNoErrors(category.CodeInfo);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("Custom MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("ABC", clone.Code);
			AssertEquals("You want to clone me.", clone.Description);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (WorkflowCategory)base.GetBusinessObjectToClone();
			result.Code = (NoResString)"ABC";
			result.Description = (NoResString)"You want to clone me.";
			return result;
		}
	}
}
