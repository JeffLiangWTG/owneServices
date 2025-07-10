using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(UpdateHVLVDetailsPreScreeningConfigurationRegistryItemToAddFromToHSCode))]
	public class UpdateHVLVDetailsPreScreeningConfigurationRegistryItemToAddFromToHSCodeTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var userData = XDocument.Load(new MemoryStream(RegistryHelper.GetStmDataValue("HVLVDetailsPreScreeningConfiguration")));
			var rootElement = userData.Root;

			var hvlvPreScreeningRuleElements = rootElement.Descendants("HVLVPreScreeningRule");
			foreach (var hvlvPreScreeningRuleElement in hvlvPreScreeningRuleElements)
			{
				var hvlvPreScreeningFieldElements = hvlvPreScreeningRuleElement.Descendants("HVLVPreScreeningField");
				foreach (var hvlvPreScreeningFieldElement in hvlvPreScreeningFieldElements)
				{
					var hvlvPreScreeningValueElements = hvlvPreScreeningRuleElement.Descendants("HVLVPreScreeningValue");
					foreach (var hvlvPreScreeningValueElement in hvlvPreScreeningValueElements)
					{
						var fromHSCode = hvlvPreScreeningValueElement.Element("FromHSCode");
						var toHSCode = hvlvPreScreeningValueElement.Element("ToHSCode");

						CombineAssertions(() =>
						{
							AssertNotNull(fromHSCode);
							AssertNotNull(toHSCode);
						});
					}
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateHVLVDetailsPreScreeningConfigurationRegistryItemToAddFromToHSCode();
		}

		protected override void PrepareTestData()
		{
			var document = new XmlDocument();
			document.LoadXml(xmlData);
			RegistryHelper.InsertStmDataRow("HVLVDetailsPreScreeningConfiguration", "BIN", Encoding.Unicode.GetBytes(document.InnerXml));
		}

		readonly RegistryTransformationHelper RegistryHelper = new RegistryTransformationHelper();

		const string xmlData = @"<?xml version=""1.0"" encoding=""utf-16""?>
<HVLVDetailsPreScreeningConfiguration>
    <IsEnabled>Y</IsEnabled>
    <ArrayOfHVLVPreScreeningRule xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
        <HVLVPreScreeningRule>
            <TransportMode>SEA</TransportMode>
            <ETailer />
            <OriginCountryCode />
            <DestinationCountryCode />
            <ModuleType>SHP</ModuleType>
            <EmailNotificationType>NOE</EmailNotificationType>
            <EmailNotificationGroup />
            <ArrayOfHVLVPreScreeningField>
                <HVLVPreScreeningField>
                    <FieldDescription>Consignee</FieldDescription>
                    <ValidationRule>WRN</ValidationRule>
                    <DeMinimumValue>0</DeMinimumValue>
                    <IsDeMinimusValueOverride>N</IsDeMinimusValueOverride>
                    <CheckSameConsignee>N</CheckSameConsignee>
                    <MacrosScript />
                    <MessageText />
                    <IsMandatory>N</IsMandatory>
                    <ArrayOfHVLVPreScreeningValue>
                        <HVLVPreScreeningValue>
                            <ScreeningValue>12</ScreeningValue>
                            <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
                            <MessageTextPerValue />
                        </HVLVPreScreeningValue>
                    </ArrayOfHVLVPreScreeningValue>
                    <ArrayOfHVLVPreScreeningSpecialCharacterValue />
                </HVLVPreScreeningField>
                <HVLVPreScreeningField>
                    <FieldDescription>Origin HS Code</FieldDescription>
                    <ValidationRule>ERR</ValidationRule>
                    <DeMinimumValue>0</DeMinimumValue>
                    <IsDeMinimusValueOverride>N</IsDeMinimusValueOverride>
                    <CheckSameConsignee>N</CheckSameConsignee>
                    <MacrosScript />
                    <MessageText />
                    <IsMandatory>N</IsMandatory>
                    <ArrayOfHVLVPreScreeningValue>
                        <HVLVPreScreeningValue>
                            <ScreeningValue />
                            <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
                            <MessageTextPerValue />
                        </HVLVPreScreeningValue>
                        <HVLVPreScreeningValue>
                            <ScreeningValue />
                            <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
                            <MessageTextPerValue />
                        </HVLVPreScreeningValue>
                    </ArrayOfHVLVPreScreeningValue>
                    <ArrayOfHVLVPreScreeningSpecialCharacterValue />
                </HVLVPreScreeningField>
            </ArrayOfHVLVPreScreeningField>
        </HVLVPreScreeningRule>
    </ArrayOfHVLVPreScreeningRule>
</HVLVDetailsPreScreeningConfiguration>";
	}
}
