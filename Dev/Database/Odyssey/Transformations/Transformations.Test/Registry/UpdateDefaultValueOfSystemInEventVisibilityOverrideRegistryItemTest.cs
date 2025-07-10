using System.IO;
using System.Linq;
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
	[TestedType(typeof(UpdateDefaultValueOfSystemInEventVisibilityOverrideRegistryItem))]
	public class UpdateDefaultValueOfSystemInEventVisibilityOverrideRegistryItemTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var userData = XDocument.Load(new MemoryStream(RegistryHelper.GetStmDataValue("EventVisibilityOverride")));
			var rootElement = userData.Root;

			var eventVisibilityOverrideElements = rootElement.Descendants("EventVisibilityOverride");
			foreach (var eventVisibilityOverrideElement in eventVisibilityOverrideElements)
			{
				var arrayOfEventVisibilityOverrideSetting = eventVisibilityOverrideElement.Descendants("ArrayOfEventVisibilityOverrideSetting").FirstOrDefault();
				var eventVisibilityOverrideSettings = arrayOfEventVisibilityOverrideSetting.Descendants("EventVisibilityOverrideSetting");
				foreach (var eventVisibilityOverrideSetting in eventVisibilityOverrideSettings)
				{
					AssertEquals("N", eventVisibilityOverrideSetting.Element("IsSystem").Value);
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateDefaultValueOfSystemInEventVisibilityOverrideRegistryItem();
		}

		protected override void PrepareTestData()
		{
			var document = new XmlDocument();
			document.LoadXml(xmlData);
			RegistryHelper.InsertStmDataRow("EventVisibilityOverride", "BIN", Encoding.Unicode.GetBytes(document.InnerXml));
		}

		readonly RegistryTransformationHelper RegistryHelper = new RegistryTransformationHelper();

		const string xmlData = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfEventVisibilityOverride xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<EventVisibilityOverride>
    <CodeMaxLength>3</CodeMaxLength>
    <Code>CLH</Code>
    <Description>Container Load List</Description>
    <ArrayOfEventVisibilityOverrideSetting>
        <EventVisibilityOverrideSetting>
            <CodeMaxLength>3</CodeMaxLength>
            <Code>AED</Code>
            <Description />
            <EventCode>AED</EventCode>
            <Description />
            <IsActive>N</IsActive>
            <EventReference>Y</EventReference>
            <QuickView>N</QuickView>
            <EventDescriptionOverride>All Export Documents Received</EventDescriptionOverride>
            <DuplicateEventsHandlingMethod>ALL</DuplicateEventsHandlingMethod>
        </EventVisibilityOverrideSetting>
        <EventVisibilityOverrideSetting>
            <CodeMaxLength>3</CodeMaxLength>
            <Code>AID</Code>
            <Description />
            <EventCode>AID</EventCode>
            <Description />
            <IsActive>N</IsActive>
            <EventReference>Y</EventReference>
            <QuickView>N</QuickView>
            <EventDescriptionOverride>All Import Documents Received</EventDescriptionOverride>
            <DuplicateEventsHandlingMethod>ALL</DuplicateEventsHandlingMethod>
        </EventVisibilityOverrideSetting>
    </ArrayOfEventVisibilityOverrideSetting>
</EventVisibilityOverride>
</ArrayOfEventVisibilityOverride>";
	}
}
