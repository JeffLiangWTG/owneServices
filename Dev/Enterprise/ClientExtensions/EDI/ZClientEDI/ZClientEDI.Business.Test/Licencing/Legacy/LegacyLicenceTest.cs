using System.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	/// <summary>
	///  Tests for this class are integrated with the tests for LicenceCheckpoint.
	/// </summary>
	class LegacyLicenceTest : TestCase
	{
		public void TestAddToLicenceNode()
		{
			XmlDocument document = new XmlDocument();
			XmlNode node = document.CreateNode(XmlNodeType.Element, "TestNode", "");
			Assert("Precondition", string.IsNullOrEmpty(node.InnerXml));

			LegacyLicence licences = new LegacyLicence();
			licences.Type = "ODM";
			licences.SupportMode = "No Support";
			licences.ObsoleteHostedLocation = "SYD";
			licences.AddToLicenceNode(node);

			AssertEquals("Type", node.Attributes[0].Name);
			AssertEquals("ODM", node.Attributes[0].Value);

			AssertEquals("SupportMode", node.Attributes[1].Name);
			AssertEquals("No Support", node.Attributes[1].Value);

			AssertEquals("HostedLocation", node.Attributes[2].Name);
			AssertEquals("SYD", node.Attributes[2].Value);
		}

		public void TestLoadFromLicenceNode()
		{
			XmlDocument document = new XmlDocument();
			XmlNode node = document.CreateNode(XmlNodeType.Element, "TestNode", "");
			XmlAttribute attribute = document.CreateAttribute("Type");
			attribute.Value = "ADV";
			node.Attributes.Append(attribute);

			LegacyLicence licences = new LegacyLicence();
			AssertEquals("Precondition", null, licences.Type);

			licences.LoadFromLicenceNode(node);
			AssertEquals("ADV", licences.Type);
			AssertEquals("", licences.SupportMode);

			attribute = document.CreateAttribute("SupportMode");
			attribute.Value = "24-Hour Support";
			node.Attributes.Append(attribute);

			attribute = document.CreateAttribute("Order");
			attribute.Value = bool.TrueString;
			node.Attributes.Append(attribute);

			attribute = document.CreateAttribute("HostedLocation");
			attribute.Value = "MEL";
			node.Attributes.Append(attribute);

			licences.LoadFromLicenceNode(node);
			AssertEquals("24-Hour Support", licences.SupportMode);
			AssertEquals("MEL", licences.ObsoleteHostedLocation);
		}

		public void TestGetLicenceKeyContainsTypeAttribute()
		{
			LegacyLicence testLicence = new LegacyLicence();
			testLicence.Type = "ODM";
			string licenceKeyXml = testLicence.GenerateLicenceKey();
			XmlDocument document = new XmlDocument();
			document.LoadXml(licenceKeyXml);
			XmlNode licenceKeyNode = document.FirstChild;
			AssertEquals("ODM", licenceKeyNode.Attributes["Type"].Value);
		}

		public void TestLoadFromLicenceKeyShouldLoadType()
		{
			LegacyLicence licences1 = new LegacyLicence();
			licences1.Type = "ADV";

			LegacyLicence licences2 = new LegacyLicence(licences1.ToEncryptedKeyString());
			AssertEquals("ADV", licences2.Type);
		}

		public void TestLoadFromPreJun2010LicenceKey()
		{
			const string xml =
@"<LicenceKey Type=''>
  <CheckPoints>
    <FOR Type='NON' User='0' Expiry='00010101' />
    <ACC Type='NON' User='0' Expiry='00010101' />
    <COR Type='NON' User='0' Expiry='00010101' />
    <EMF Type='NON' User='0' Expiry='00010101' />
  </CheckPoints>
  <Company OrgPKThatGeneratedThisLicence='00000000-0000-0000-0000-000000000000' EnterpriseCode='' PhysicalServerID='' Code='' Name='' Address1='' Address2='' City='' PostCode='' State='' CountryPK='00000000-0000-0000-0000-000000000000' BusinessRegNo='' BusinessRegNo2='' LocalCurrencyCode='' IsReciprocal='False' IsGSTRegistered='False' IsGSTCashBasis='False' IsWHTRegistered='False' IsWHTCashBasis='False' />
  <Branches />
  <InstallationDetails AMSMode='OFF' />
</LicenceKey>";

			LicencesForTest licences = new LicencesForTest(xml);
			AssertEquals("", licences.SupportMode);
			AssertEquals("not custom order so COR is first", "COR", licences.CheckpointOrder[0].Name);
			AssertEquals("Forwarder", licences.Forwarder.DisplayName);
			AssertEquals("Accountant", licences.Accountant.DisplayName);
			AssertEquals("Core", licences.Core.DisplayName);
			AssertEquals("ExportManifest (CRN)", licences.ExportManifest.DisplayName);

			AssertEquals("ReportWriter", licences.ReportWriter.DisplayName);
		}

		public void TestLoadFromJun2010LicenceKey()
		{
			const string xml =
@"<LicenceKey Type='' SupportMode='24-Hour Support' Order='false' HostedLocation='SYD'>
  <CheckPoints>
    <FOR Type='NON' User='0' Expiry='00010101' Desc='newForwarder' />
    <ACC Type='NON' User='0' Expiry='00010101' Desc='newAccountant' />
    <COR Type='NON' User='0' Expiry='00010101' Desc='newCore' />
    <EMF Type='NON' User='0' Expiry='00010101' Desc='newExportManifest' />
  </CheckPoints>
  <Company OrgPKThatGeneratedThisLicence='00000000-0000-0000-0000-000000000000' EnterpriseCode='' PhysicalServerID='' Code='' Name='' Address1='' Address2='' City='' PostCode='' State='' CountryPK='00000000-0000-0000-0000-000000000000' BusinessRegNo='' BusinessRegNo2='' LocalCurrencyCode='' IsReciprocal='False' IsGSTRegistered='False' IsGSTCashBasis='False' IsWHTRegistered='False' IsWHTCashBasis='False' />
  <Branches />
  <InstallationDetails AMSMode='OFF' />
</LicenceKey>";

			LicencesForTest licences = new LicencesForTest(xml);
			LegacyLicence expected = new LegacyLicence();
			AssertEquals("24-Hour Support", licences.SupportMode);
			AssertEquals("SYD", licences.ObsoleteHostedLocation);
			AssertEquals("COR is first", "COR", licences.CheckpointOrder[0].Name);
			AssertEquals(expected.Forwarder.DisplayName, licences.Forwarder.DisplayName);
			AssertEquals(expected.Accountant.DisplayName, licences.Accountant.DisplayName);
			AssertEquals(expected.Core.DisplayName, licences.Core.DisplayName);
			AssertEquals(expected.ExportManifest.DisplayName, licences.ExportManifest.DisplayName);
			AssertEquals(expected.ReportWriter.DisplayName, licences.ReportWriter.DisplayName);
		}
	}

	class LicencesForTest : LegacyLicence
	{
		public LicencesForTest(string xml)
			: base()
		{
			LoadFromLicenceKey(xml);
		}
	}
}