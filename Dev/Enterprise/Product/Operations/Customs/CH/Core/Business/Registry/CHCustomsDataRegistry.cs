using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

public sealed class CHCustomsDataRegistry : RegistryItemSet, Integration.Customs.CH.ICHCustomsRegistry
{
	#region Construction

	public static CHCustomsDataRegistry Instance => instance ?? (instance = new CHCustomsDataRegistry());

	[ThreadStatic]
	static CHCustomsDataRegistry instance;

	CHCustomsDataRegistry()
	{
	}

	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Switzerland;
	}

	#endregion

	public class Categories : CustomsDataRegistry.Categories
	{
		public static MultilingualString Customs_Switzerland_EBD => CombineCategories(Customs_Switzerland, ResString.GetMultilingualString("C36761B4-788C-4243-98A0-BC662CE79BA1", "EBD"));
		public static MultilingualString Customs_Switzerland_Passar => CombineCategories(Customs_Switzerland, ResString.GetMultilingualString("F60EB448-C15E-411E-A103-79653B137C7D", "Passar"));
		public static MultilingualString Customs_Switzerland_Passar_Documents => CombineCategories(Customs_Switzerland_Passar, ResString.GetMultilingualString("63D0512A-CA94-4A54-AB8F-C467A5D8DD43", "Documents"));
		public static MultilingualString Customs_Switzerland_Security => CombineCategories(Customs_Switzerland, ResString.GetMultilingualString("0F7FEAE3-47C4-4324-9D28-FBBDC07F2E43", "Security"));
		public static MultilingualString Customs_Switzerland_Edec => CombineCategories(Customs_Switzerland, ResString.GetMultilingualString("290F6F99-4C8E-452E-A8C6-BF2B22DDA3C2", "e-Dec"));
	}

	public override bool IsForProductivityWise => false;

	public IntRegistryItem MaximumFileSize => GetItem("CHEBDMaximumFileSize", () => new IntRegistryItem(
		"CHEBDMaximumFileSize",
		Categories.Customs_Switzerland_EBD,
		ResString.GetMultilingualString("6015A9CB-FF8B-4240-8D19-D101CA584E03", "Maximum File Size"),
		ResString.GetMultilingualString("DABBA43C-9F71-4E1B-9C89-60EECDF60035", "Maximum size of the accompanying document file that can be uploaded to customs (in MB)."),
		RegistryStorageFlags.System,
		RegistryOptions.Default,
		30,
		1,
		1000
		));

	readonly string[] defaultAllowedFileExtensions = { (NoResString)"pdf", (NoResString)"xls", (NoResString)"xlsx" };

	public StringArrayRegistryItem AllowedFileExtensions => GetItem("CHEBDAllowedFileExtensions", () =>
	{
		StringArrayRegistryItem result = new StringArrayRegistryItem(
			"CHEBDAllowedFileExtensions",
			Categories.Customs_Switzerland_EBD,
			ResString.GetMultilingualString("E47D5DBE-793E-47B1-8743-3C1B48E41DBC", "Allowed File Extensions"),
			ResString.GetMultilingualString("7F12498E-A42B-4298-9D94-131EB8DCB06E", "Accompanying document file types that can be uploaded to customs. Specify a list of file extensions."),
			RegistryStorageFlags.System,
			defaultAllowedFileExtensions);
		result.DataType.MaximumLength = 4;
		return result;
	});

	public IntRegistryItem MaxNumberOfGetMessageAttempts => GetItem("CHPassarMaxNumberOfAttempts", () => new IntRegistryItem(
		"CHPassarMaxNumberOfAttempts",
		Categories.Customs_Switzerland_Passar,
		ResString.GetMultilingualString("EB763007-96AD-4DF5-8214-65CA5ACB0B2E", "Maximum Number of Get Message Attempts"),
		ResString.GetMultilingualString("D192300D-BECE-48F2-A50E-6369535608D9", "Maximum number of attempts to retrieve a message from Passar out queue."),
		RegistryStorageFlags.System,
		RegistryOptions.Default,
		3,
		1,
		int.MaxValue));

	readonly string[] defaultAllowedCertificateCAIssuers = [
		(NoResString)"CN=Swiss Government Regular CA 01, OU=Certification Authorities, OU=Services, O=Admin, C=CH",
		(NoResString)"CN=Swiss Government Regular CA 02, OU=Swiss Government PKI, O=Bundesamt fuer Informatik und Telekommunikation (BIT), OID.2.5.4.97=NTRCH-CHE-221.032.573, C=CH"];

	public StringArrayRegistryItem AllowedCertificateCAIssuers => GetItem("CHSecurityAllowedCertificateCAIssuers", () =>
	{
		StringArrayRegistryItem result = new StringArrayRegistryItem(
			"CHSecurityAllowedCertificateCAIssuers",
			Categories.Customs_Switzerland_Security,
			ResString.GetMultilingualString("FC7594F7-BCA2-4BA4-8A87-B82690A411EC", "Allowed Certificate CA Issuers"),
			ResString.GetMultilingualString("F5469872-B934-483B-A026-40A47549AC20", "Specify a list of allowed Certificate CA Issuers for the verification of signed contents (for example eVV XML)."),
			RegistryStorageFlags.System,
			defaultAllowedCertificateCAIssuers);
		return result;
	});

	readonly string[] defaultRevokedCertificateCAIssuers = { (NoResString)"CN=Swiss Government Root CA II, OU=Certification Authorities, OU=Services, O=The Federal Authorities of the Swiss Confederation, C=CH" };

	public StringArrayRegistryItem RevokedCertificateCAIssuers => GetItem("CHSecurityRevokedCertificateCAIssuers", () =>
	{
		StringArrayRegistryItem result = new StringArrayRegistryItem(
			"CHSecurityRevokedCertificateCAIssuers",
			Categories.Customs_Switzerland_Security,
			ResString.GetMultilingualString("7EAD1555-D52D-4272-ACE0-105A5957B1B5", "Revoked Certificate CA Issuers"),
			ResString.GetMultilingualString("A7AEDAAE-CC62-4268-A1CB-44D6E3FC2585", "Specify a list of revoked Certificate CA Issuers for the verification of signed contents (for example eVV XML)."),
			RegistryStorageFlags.System,
			defaultRevokedCertificateCAIssuers);
		return result;
	});

	public IntRegistryItem MaxNumberOfSimultaneousDownloads => GetItem("CHPassarMaxNumberOfSimultaneousDownloads", () => new IntRegistryItem(
		"CHPassarMaxNumberOfSimultaneousDownloads",
		Categories.Customs_Switzerland_Passar_Documents,
		ResString.GetMultilingualString("75950AB5-948D-4CED-9CA8-C8F9BCF299B5", "Maximum Number of Simultaneous Downloads"),
		ResString.GetMultilingualString("293798A8-FD92-4334-9F9A-72FF876D4F73", "Maximum number of simultaneous document downloads from CharteraOutput application."),
		RegistryStorageFlags.Company,
		RegistryOptions.Default,
		500,
		1,
		500
		));

	public IntRegistryItem MaxNumberOfDownloadAttempts => GetItem("CHPassarMaxNumberOfDownloadAttempts", () => new IntRegistryItem(
		"CHPassarMaxNumberOfDownloadAttempts",
		Categories.Customs_Switzerland_Passar_Documents,
		ResString.GetMultilingualString("068267E8-B22A-41F3-9F7D-F1A12E6EC2C0", "Maximum Number of Download Attempts"),
		ResString.GetMultilingualString("04DDC86C-F008-4426-9B05-1BC2BC406321", "Maximum number of download attempts of a single document from CharteraOutput application."),
		RegistryStorageFlags.System,
		RegistryOptions.Default,
		50,
		1,
		255
		));

	public IntRegistryItem MaxNumberOfSearchAttempts => GetItem("CHPassarMaxNumberOfSearchAttempts", () => new IntRegistryItem(
		"CHPassarMaxNumberOfSearchAttempts",
		Categories.Customs_Switzerland_Passar_Documents,
		ResString.GetMultilingualString("31D0300F-8A74-4566-B089-4612833D018F", "Maximum Number of Search Request Attempts"),
		ResString.GetMultilingualString("0B64BEE7-022F-4E35-BECA-B9AC0090B293", "Maximum number of search request attempts for a certain time interval. After each attempt the next attempt time is increased (after 5 minutes, 10, 30, etc.)."),
		RegistryStorageFlags.System,
		RegistryOptions.Default,
		5,
		1,
		50
		));

	public ArrivalCustomerReferenceFormatRegistryItem ArrivalCustomerReferenceFormat => GetItem("CHPassarArrivalCustomerReferenceFormat", () => new ArrivalCustomerReferenceFormatRegistryItem(
		"CHPassarArrivalCustomerReferenceFormat",
		Categories.Customs_Switzerland_Passar,
		ResString.GetMultilingualString("BC81F7EE-8D96-4852-BD3C-E08A0381157C", "NCTS Arrival Customer Reference Customization"),
		ResString.GetMultilingualString("54B7EAB8-6813-4AF0-9F0C-ACD24885A98E", "Override this value to customize how NCTS Arrival Customer References are formatted."),
		RegistryStorageFlags.Company));

	public PassarSearchRequestConfigRegistryItem PassarSearchRequestConfig => GetItem("CHPassarSearchRequestTask", () => new PassarSearchRequestConfigRegistryItem(
		"CHPassarSearchRequestTask",
		Categories.Customs_Switzerland_Passar_Documents,
		ResString.GetMultilingualString("5B249AF3-E33D-4296-A0D0-A92D2A710394", "Enable Document Search Requests"),
		ResString.GetMultilingualString("11DC76FC-1461-432D-AA50-E23BC858A0EE", "This registry enables the periodical searching of documents on the Chartera Output application.\r\nThe searching starts regularly and searches for available documents created earlier than 5 minutes ago.\r\nWhen it is enabled, it searches for documents created in the past, not older than 30 days, from the latest search till now.\r\n It is possible to set the maximum number of hours to search in the past, configuring Search Documents Not Older Than (Hours) parameter."),
		RegistryStorageFlags.Company));

	public BooleanRegistryItem EnablePartnerTopic => GetItem("CHPassarEnablePartnerTopic", () => new BooleanRegistryItem(
		"CHPassarEnablePartnerTopic",
		CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar,
		ResString.GetMultilingualString("0711B2D3-26BD-47DB-9C5D-E501FD442C2C", "Enable Partner Topic filter"),
		ResString.GetMultilingualString("CCACD2CD-DCA6-4A91-A914-DDD96C394DEB", "If enabled, CargoWise passes \'{0}\' parameter value to Passar Get Messages function to get only the messages related to declarations created by  CargoWise.", "partnerTopic"),
		RegistryStorageFlags.System,
		false));

	public EdecBordereauConfigRegistryItem EdecBordereauConfig => GetItem("CHEdecBordereauConfiguration", () => new EdecBordereauConfigRegistryItem(
		"CHEdecBordereauConfiguration",
		Categories.Customs_Switzerland_Edec,
		ResString.GetMultilingualString("490A0213-D540-4D43-ADAE-027D6CD630A3", "Bordereau Configuration"),
		ResString.GetMultilingualString("5C9D3A8D-C11A-449E-B7E8-08398AC8B600", "If enabled, CargoWise downloads daily the Bordereau for the Customs Account specified in the processing Company.\r\nFor the retrieved Bordereau it downloads also the eVV documents specified in it. The process also requests the Bordereau for the past days till the value specified in ‘Maximum Number of Days’ parameter.\r\nIf the Bordereau already exists, it is not downloaded again."),
		RegistryStorageFlags.Company));

	public BooleanRegistryItem DeclarationActivationEnabled => GetItem("CHPassarEnableDeclarationActivation", () => new BooleanRegistryItem(
		"CHPassarEnableDeclarationActivation",
		CHCustomsDataRegistry.Categories.Customs_Switzerland_Passar,
		ResString.GetMultilingualString("8F206E22-16D3-4DCD-8367-3D5189B888CF", "Declaration Activation Enabled"),
		ResString.GetMultilingualString("A24EC6A7-AAF9-44A2-AF15-D537D8E7D6B0", "Set this to true to enable the Declaration Activation module."),
		RegistryStorageFlags.Company,
		RegistryOptions.IsOnlyForDevelopers,
		false));

	IRegistryItem Integration.Customs.CH.ICHCustomsRegistry.DeclarationActivationEnabled => DeclarationActivationEnabled;
}

