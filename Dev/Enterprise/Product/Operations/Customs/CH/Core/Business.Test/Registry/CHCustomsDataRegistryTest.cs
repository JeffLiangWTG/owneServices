using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.CHCustomsDataRegistry;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHCustomsDataRegistry))]
class CHCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<CHCustomsDataRegistry>
{
	public void TestCategories()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(Categories.Customs_Switzerland_EBD), "Customs/Country or Region Specific/Switzerland/EBD", Categories.Customs_Switzerland_EBD.ToString());
			AssertEquals(nameof(Categories.Customs_Switzerland_Passar), "Customs/Country or Region Specific/Switzerland/Passar", Categories.Customs_Switzerland_Passar.ToString());
			AssertEquals(nameof(Categories.Customs_Switzerland_Security), "Customs/Country or Region Specific/Switzerland/Security", Categories.Customs_Switzerland_Security.ToString());
		});
	}

	public void TestMaximumFileSize()
	{
		TestRegistryItem(ItemSet.MaximumFileSize,
			"CHEBDMaximumFileSize",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_EBD,
			"Maximum File Size",
			"Maximum size of the accompanying document file that can be uploaded to customs (in MB).",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			30,
			1,
			1000);
	}

	public void TestAllowedFileExtensions()
	{
		TestGenericRegistryItem(ItemSet.AllowedFileExtensions,
			"CHEBDAllowedFileExtensions",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_EBD,
			"Allowed File Extensions",
			"Accompanying document file types that can be uploaded to customs. Specify a list of file extensions.",
			RegistryStorageFlags.System);
		AssertContainsExactElementsInAnyOrder(new string[] { "pdf", "xls", "xlsx" }, ItemSet.AllowedFileExtensions.DefaultValue);
		AssertEquals("MaximumLength", 4, ItemSet.AllowedFileExtensions.DataType.MaximumLength);
	}

	public void TestMaxNumberOfGetMessageAttempts()
	{
		TestRegistryItem(ItemSet.MaxNumberOfGetMessageAttempts,
			"CHPassarMaxNumberOfAttempts",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar,
			"Maximum Number of Get Message Attempts",
			"Maximum number of attempts to retrieve a message from Passar out queue.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			3,
			1,
			int.MaxValue);
	}

	public void TestSecurityAllowedCertificateCAIssuers()
	{
		TestGenericRegistryItem(ItemSet.AllowedCertificateCAIssuers,
			"CHSecurityAllowedCertificateCAIssuers",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Security,
			"Allowed Certificate CA Issuers",
			"Specify a list of allowed Certificate CA Issuers for the verification of signed contents (for example eVV XML).",
			RegistryStorageFlags.System);

		string[] expectedDefaults = [
			"CN=Swiss Government Regular CA 01, OU=Certification Authorities, OU=Services, O=Admin, C=CH",
			"CN=Swiss Government Regular CA 02, OU=Swiss Government PKI, O=Bundesamt fuer Informatik und Telekommunikation (BIT), OID.2.5.4.97=NTRCH-CHE-221.032.573, C=CH"];

		AssertContainsExactElementsInAnyOrder(expectedDefaults, ItemSet.AllowedCertificateCAIssuers.DefaultValue);
	}

	public void TestSecurityRevokedCertificateCAIssuers()
	{
		TestGenericRegistryItem(ItemSet.RevokedCertificateCAIssuers,
			"CHSecurityRevokedCertificateCAIssuers",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Security,
			"Revoked Certificate CA Issuers",
			"Specify a list of revoked Certificate CA Issuers for the verification of signed contents (for example eVV XML).",
			RegistryStorageFlags.System);
		AssertContainsExactElementsInAnyOrder(new string[] { "CN=Swiss Government Root CA II, OU=Certification Authorities, OU=Services, O=The Federal Authorities of the Swiss Confederation, C=CH" }, ItemSet.RevokedCertificateCAIssuers.DefaultValue);
	}

	public void TestMaxNumberOfSimultaneousDownloads()
	{
		TestRegistryItem(ItemSet.MaxNumberOfSimultaneousDownloads,
			"CHPassarMaxNumberOfSimultaneousDownloads",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar_Documents,
			"Maximum Number of Simultaneous Downloads",
			"Maximum number of simultaneous document downloads from CharteraOutput application.",
			RegistryStorageFlags.Company,
			RegistryOptions.Default,
			500,
			1,
			500);
	}

	public void TestMaxNumberOfDownloadAttempts()
	{
		TestRegistryItem(ItemSet.MaxNumberOfDownloadAttempts,
			"CHPassarMaxNumberOfDownloadAttempts",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar_Documents,
			"Maximum Number of Download Attempts",
			"Maximum number of download attempts of a single document from CharteraOutput application.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			50,
			1,
			255);
	}

	public void TestMaxNumberOfSearchRequestAttempts()
	{
		TestRegistryItem(ItemSet.MaxNumberOfSearchAttempts,
			"CHPassarMaxNumberOfSearchAttempts",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar_Documents,
			"Maximum Number of Search Request Attempts",
			"Maximum number of search request attempts for a certain time interval. After each attempt the next attempt time is increased (after 5 minutes, 10, 30, etc.).",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			5,
			1,
			50);
	}

	public void TestArrivalCustomerReferenceFormat()
	{
		TestGenericRegistryItem(ItemSet.ArrivalCustomerReferenceFormat,
			"CHPassarArrivalCustomerReferenceFormat",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar,
			"NCTS Arrival Customer Reference Customization",
			"Override this value to customize how NCTS Arrival Customer References are formatted.",
			RegistryStorageFlags.Company);
	}

	public void TestPassarSearchRequestTask()
	{
		TestGenericRegistryItem(ItemSet.PassarSearchRequestConfig,
			"CHPassarSearchRequestTask",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar_Documents,
			"Enable Document Search Requests",
			"This registry enables the periodical searching of documents on the Chartera Output application.\r\nThe searching starts regularly and searches for available documents created earlier than 5 minutes ago.\r\nWhen it is enabled, it searches for documents created in the past, not older than 30 days, from the latest search till now.\r\n It is possible to set the maximum number of hours to search in the past, configuring Search Documents Not Older Than (Hours) parameter.",
			RegistryStorageFlags.Company);
	}

	public void TestPassarEnablePartnerTopic()
	{
		TestRegistryItem(ItemSet.EnablePartnerTopic,
			"CHPassarEnablePartnerTopic",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar,
			"Enable Partner Topic filter",
			"If enabled, CargoWise passes \'partnerTopic\' parameter value to Passar Get Messages function to get only the messages related to declarations created by  CargoWise.",
			RegistryStorageFlags.System,
			false);
	}

	public void TestEdecBordereauConfigurations()
	{
		TestGenericRegistryItem(ItemSet.EdecBordereauConfig,
			"CHEdecBordereauConfiguration",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Edec,
			"Bordereau Configuration",
			"If enabled, CargoWise downloads daily the Bordereau for the Customs Account specified in the processing Company.\r\nFor the retrieved Bordereau it downloads also the eVV documents specified in it. The process also requests the Bordereau for the past days till the value specified in ‘Maximum Number of Days’ parameter.\r\nIf the Bordereau already exists, it is not downloaded again.",
			RegistryStorageFlags.Company);
	}

	public void TestDeclarationActivationEnabled()
	{
		TestRegistryItem(ItemSet.DeclarationActivationEnabled,
			"CHPassarEnableDeclarationActivation",
			CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar,
			"Declaration Activation Enabled",
			"Set this to true to enable the Declaration Activation module.",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false);
	}
}
