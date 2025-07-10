using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	public abstract class AddCustomsAdditionalNumberDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected abstract string Code { get; }
		protected abstract string Description { get; }
		protected abstract bool IsUnique { get; }
		protected abstract bool IsAutomation { get; }

		protected override void AssertTransformationResults()
		{
			AssertAdditionalNumber(noAdditionalNumberRegistryGuid);
			AssertAdditionalNumber(errorAdditionalNumberRegistryGuid);

			void AssertAdditionalNumber(Guid registryGuid)
			{
				var userData = XDocument.Load(new MemoryStream(registryHelper.GetStmDataValue(registryGuid)));
				var rootElement = userData.Root;
				var additionalNumber = rootElement.Descendants("CustomsReferenceNumberType").FirstOrDefault(element => element.Element("Code").Value == Code);
				AssertNotNull(additionalNumber);
				AssertEquals(IsUnique ? "Y" : "N", additionalNumber.Element("IsUnique").Value);
				AssertEquals(IsAutomation ? "Y" : "N", additionalNumber.Element("IsAutomation").Value);
				AssertEquals(Description, additionalNumber.Element("Description").Value);
			}
		}

		protected Guid noAdditionalNumberRegistryGuid;
		protected Guid errorAdditionalNumberRegistryGuid;

		protected override void PrepareTestData()
		{
			var noAdditionalNumberRegistryDocument = new XmlDocument();
			noAdditionalNumberRegistryDocument.LoadXml(NoAdditionalNumberXmlData);
			noAdditionalNumberRegistryGuid = registryHelper.InsertStmDataRow("CustomsAdditionalReferenceNumbers", "BIN", Encoding.Unicode.GetBytes(noAdditionalNumberRegistryDocument.InnerXml));

			var errorAdditionalNumberRegistryDocument = new XmlDocument();
			errorAdditionalNumberRegistryDocument.LoadXml(ErrorAdditionalNumberXmlData);
			errorAdditionalNumberRegistryGuid = registryHelper.InsertStmDataRow("CustomsAdditionalReferenceNumbers", sD_Owner: Guid.NewGuid(), "BIN", Encoding.Unicode.GetBytes(errorAdditionalNumberRegistryDocument.InnerXml));
		}

		readonly RegistryTransformationHelper registryHelper = new RegistryTransformationHelper();

		string NoAdditionalNumberXmlData => @"<?xml version=""1.0"" encoding=""utf-16""?>
<CustomsReferenceNumberTypes
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <CustomsReferenceNumberType>
        <Code>COC</Code>
        <Description>Customs Office Code (Override)</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>AMS</Code>
        <Description>AMS Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>UBR</Code>
        <Description>Under Bond Approval Reference Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CON</Code>
        <Description>Carrier Contract Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CLC</Code>
        <Description>Client Contract Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>BKG</Code>
        <Description>Carrier Booking Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>RLB</Code>
        <Description>Railway Bill Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>UCR</Code>
        <Description>External (3rd party) Unique Consignment Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CAR</Code>
        <Description>Customs Authorization Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>LCR</Code>
        <Description>Letter Of Credit Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>ACA</Code>
        <Description>Pentant Advance Cargo Advice Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CCN</Code>
        <Description>Cargo Control Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>PCN</Code>
        <Description>Previous Cargo Control Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>BAG</Code>
        <Description>Courier Bag Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>COU</Code>
        <Description>Courier Consignment Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>NAC</Code>
        <Description>Contract Named Account</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>OAG</Code>
        <Description>Other Agent Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CQN</Code>
        <Description>Carrier Quote Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>TWR</Code>
        <Description>Transit Warehouse Receive</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>HIR</Code>
        <Description>eHub Interchange Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>ACI</Code>
        <Description>Advance Cargo Information Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>ISF</Code>
        <Description>US Import Security Filing (ISF) Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>JDR</Code>
        <Description>Declaration Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CLR</Code>
        <Description>Customer Load Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
</CustomsReferenceNumberTypes>";

		string ErrorAdditionalNumberXmlData => $@"<?xml version=""1.0"" encoding=""utf-16""?>
<CustomsReferenceNumberTypes
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <CustomsReferenceNumberType>
        <Code>COC</Code>
        <Description>Customs Office Code (Override)</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>AMS</Code>
        <Description>AMS Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>UBR</Code>
        <Description>Under Bond Approval Reference Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CON</Code>
        <Description>Carrier Contract Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CLC</Code>
        <Description>Client Contract Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>BKG</Code>
        <Description>Carrier Booking Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>RLB</Code>
        <Description>Railway Bill Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>UCR</Code>
        <Description>External (3rd party) Unique Consignment Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CAR</Code>
        <Description>Customs Authorization Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>LCR</Code>
        <Description>Letter Of Credit Number</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>ACA</Code>
        <Description>Pentant Advance Cargo Advice Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CCN</Code>
        <Description>Cargo Control Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>PCN</Code>
        <Description>Previous Cargo Control Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>BAG</Code>
        <Description>Courier Bag Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>COU</Code>
        <Description>Courier Consignment Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>NAC</Code>
        <Description>Contract Named Account</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>OAG</Code>
        <Description>Other Agent Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CQN</Code>
        <Description>Carrier Quote Number</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>TWR</Code>
        <Description>Transit Warehouse Receive</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>HIR</Code>
        <Description>eHub Interchange Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>ACI</Code>
        <Description>Advance Cargo Information Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>ISF</Code>
        <Description>US Import Security Filing (ISF) Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>JDR</Code>
        <Description>Declaration Reference</Description>
        <IsUnique>Y</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>CLR</Code>
        <Description>Customer Load Reference</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>N</IsAutomation>
    </CustomsReferenceNumberType>
    <CustomsReferenceNumberType>
        <Code>{Code}</Code>
        <Description>Error CMR Description</Description>
        <IsUnique>N</IsUnique>
        <IsAutomation>Y</IsAutomation>
    </CustomsReferenceNumberType>
</CustomsReferenceNumberTypes>";
	}
}
