using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RenameOptionFromESToEUAccountingIntegrationOptionsRegistryItem))]
	public class RenameOptionFromESToEUAccountingIntegrationOptionsRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameOptionFromESToEUAccountingIntegrationOptionsRegistryItem();
		}

		readonly string _testData = "<?xml version=\"1.0\" encoding=\"utf-16\"?><AccountingIntegrationOptions><EnableAccountingIntegration>Y</EnableAccountingIntegration><PreApprovalBillingJob>N</PreApprovalBillingJob><APPostDSB>Y</APPostDSB><ARPostDSB>N</ARPostDSB><CDSCustomsStatusCodes/><ChiefCustomsStatusCodes/><ESCustomsStatusCodes>AABBCCDDEEFF</ESCustomsStatusCodes></AccountingIntegrationOptions>";
		readonly string _sdName = "EnableAccountingIntegration";

		protected override void PrepareTestData()
		{
			Helper.DeleteStmDataRow(_sdName);
			Helper.InsertStmDataRow(_sdName, "BIN", Encoding.Unicode.GetBytes(_testData));
		}

		protected override void AssertTransformationResults()
		{
			var value = Helper.GetStmDataValue(_sdName);
			var xmlString = Encoding.Unicode.GetString(value);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlString);

			var xmlNode = xmlDocument.SelectSingleNode("AccountingIntegrationOptions");

			var oldNode = xmlNode.SelectSingleNode("ESCustomsStatusCodes");
			AssertNull(oldNode);
			var renamedNode = xmlNode.SelectSingleNode("EUCustomsStatusCodes");
			AssertNotNull(renamedNode);
			AssertEquals("AABBCCDDEEFF", renamedNode.InnerText);
		}
	}
}
