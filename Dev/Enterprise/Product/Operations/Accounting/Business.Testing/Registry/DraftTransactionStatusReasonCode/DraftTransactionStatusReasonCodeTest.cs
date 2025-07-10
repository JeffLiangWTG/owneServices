using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DraftTransactionStatusReasonCode))]
	public class DraftTransactionStatusReasonCodeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestValidateCode()
		{
			var registry = (DraftTransactionStatusReasonCode)BizObj;
			registry.Code = ZString.Empty;
			AssertHasError(registry.CodeInfo, "Please enter a Code.");

			registry.Code = "123";
			AssertNoErrors(registry.CodeInfo);

			registry.Code = "PA";
			AssertHasError(registry.CodeInfo, "Code must be 3 alphanumeric characters.");

			registry.Code = "PA1";
			AssertNoErrors(registry.CodeInfo);

			registry.Code = "PA@";
			AssertHasError(registry.CodeInfo, "Code must be 3 alphanumeric characters.");

			registry.Code = "AAA";
			AssertNoErrors(registry.CodeInfo);

			var duplicatedItem = registry.ParentCollection.AddNew();
			duplicatedItem.Code = registry.Code;

			registry.Code = "AAA";
			AssertHasError(registry.CodeInfo, "The Code has been duplicated and must be unique.");
			AssertHasError(duplicatedItem.CodeInfo, "The Code has been duplicated and must be unique.");
		}

		public void TestValidateDescription()
		{
			BizObj.Code = DraftTransactionStatusReasonCode.DefaultReasonCode;
			BizObj.EnglishDescription = ZString.Empty;
			AssertNoErrors(BizObj.DescriptionInfo);

			BizObj.Code = "AAA";
			BizObj.RunPreSaveValidation();
			AssertHasError(BizObj.DescriptionInfo, "Please enter a Description.");

			BizObj.EnglishDescription = "AAADesc";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		public void TestCanDelete()
		{
			BizObj.Code = "AAA";
			AssertEquals(true, BizObj.CanDelete);

			BizObj.Code = DraftTransactionStatusReasonCode.DefaultReasonCode;
			AssertEquals(false, BizObj.CanDelete);
		}

		public void TestReasonForNotAbleToDelete()
		{
			BizObj.Code = "AAA";
			AssertEquals(string.Empty, BizObj.ReasonForNotAbleToDelete);

			BizObj.Code = DraftTransactionStatusReasonCode.DefaultReasonCode;
			AssertEquals("Cannot delete default reason code.", BizObj.ReasonForNotAbleToDelete);
		}

		public void TestXMLSerialiseAndDeserialise()
		{
			var testBizo = new DraftTransactionStatusReasonCode();
			testBizo.Code = "TST";
			testBizo.EnglishDescription = "Test Description";
			testBizo.ANL = true;
			testBizo.DFT = true;
			testBizo.DSC = true;
			testBizo.DIS = true;
			testBizo.AFP = true;
			testBizo.AWA = true;
			testBizo.PRS = true;

			string xml;
			using (var stream = new StringWriter())
			using (var writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("DraftTransactionStatusReasonCode");
				((IXmlSerializable)testBizo).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();

				xml = stream.ToString();
			}

			const string expectedXML = @"<DraftTransactionStatusReasonCode>
<CodeMaxLength>3</CodeMaxLength>
<Code>TST</Code>
<Description>Test Description</Description>
<Value>N</Value>
<SystemDefined>False</SystemDefined>
<ANL>Y</ANL>
<DFT>Y</DFT>
<DSC>Y</DSC>
<DIS>Y</DIS>
<AFP>Y</AFP>
<AWA>Y</AWA>
<PRS>Y</PRS>
</DraftTransactionStatusReasonCode>";
			AssertMultilineASCIIEquals(expectedXML.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));

			var anotherTestBizo = new DraftTransactionStatusReasonCode();
			using (var stream = new StringReader(xml))
			using (var xmlReader = XmlReader.Create(stream))
			{
				xmlReader.Read();
				((IXmlSerializable)anotherTestBizo).ReadXml(xmlReader);
			}

			AssertEquals("TST", anotherTestBizo.Code);
			AssertEquals("Test Description", anotherTestBizo.EnglishDescription);
			AssertEquals(true, anotherTestBizo.ANL);
			AssertEquals(true, anotherTestBizo.DFT);
			AssertEquals(true, anotherTestBizo.DSC);
			AssertEquals(true, anotherTestBizo.DIS);
			AssertEquals(true, anotherTestBizo.AFP);
			AssertEquals(true, anotherTestBizo.AWA);
			AssertEquals(true, anotherTestBizo.PRS);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (DraftTransactionStatusReasonCode)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (DraftTransactionStatusReasonCode)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var registry = AccountingConfigurationRegistry.Instance.DraftTransactionStatusReasons.Value;
			registry.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			return registry.AddNew();
		}

		protected override bool IsCodeUniqueInCollection => true;

		#endregion
	}
}
