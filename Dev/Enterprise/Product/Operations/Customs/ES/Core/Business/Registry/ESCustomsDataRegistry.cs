using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.ES.Business.ESConstants;
using ResString = Enterprise.Customs.ES.Business.ResString;

namespace Enterprise.Customs.ES.Registry;

public sealed class ESCustomsDataRegistry : RegistryItemSet
{
	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Spain;
	}

	internal abstract class Categories
	{
		public static MultilingualString Customs_Spain => CombineCategories(RawDataRegistry.Categories.Customs_CountryOrRegion, ResString.GetMultilingualString("F25ED101-B6E0-4F2D-9969-BE3149AA0485", "Spain"));
		public static MultilingualString Customs_Spain_QueryURLs => CombineCategories(Customs_Spain, ResString.GetMultilingualString("806ADE7D-3C78-45AE-822D-0ABE6785A716", "Query URLs"));
		public static MultilingualString Customs_Spain_QueryURLs_PUE => CombineCategories(Customs_Spain_QueryURLs, ResString.GetMultilingualString("C01BDAA3-CF0F-45B2-BD18-B68CCE4DAAF2", "PUE"));
		public static MultilingualString Customs_Spain_QueryURLs_PUE_ROHS_RAEE => CombineCategories(Customs_Spain_QueryURLs_PUE, ResString.GetMultilingualString("9495752C-EB37-4959-A37D-81C1AC3C8280", "ROHS-RAEE"));
		public static MultilingualString Customs_Spain_QueryURLs_PUE_COM => CombineCategories(Customs_Spain_QueryURLs_PUE, ResString.GetMultilingualString("A38117AE-EE95-4865-BFE9-3331F0BB4978", "COM"));
		public static MultilingualString Customs_Spain_QueryURLs_PUE_ECO => CombineCategories(Customs_Spain_QueryURLs_PUE, ResString.GetMultilingualString("13445D25-0C4B-431D-9734-BBF91CF4A384", "ECO"));
		public static MultilingualString Customs_Spain_QueryURLs_SummaryDeclarationStatus => CombineCategories(Customs_Spain_QueryURLs, ResString.GetMultilingualString("58006868-5A91-49F4-9F79-1353C86B3AB5", "Summary Declaration Status"));
		public static MultilingualString Customs_Spain_QueryURLs_ImportH1 => CombineCategories(Customs_Spain_QueryURLs, ResString.GetMultilingualString("47DD41BE-7995-4DEC-9EAC-29F0B7CA4B05", "Import H1"));
		public static MultilingualString Customs_Spain_Inbox_XT => CombineCategories(Customs_Spain, ResString.GetMultilingualString("CD30F409-CB28-4D9B-BE1D-510D23770A0E", "Inbox XT"));
		public static MultilingualString Customs_Spain_MessageVersion => CombineCategories(Customs_Spain, ResString.GetMultilingualString("E7750E30-5E98-43FC-93CD-02B6642D8DE7", "Message Version"));
	}

	public static ESCustomsDataRegistry Instance => instance ?? (instance = new ESCustomsDataRegistry());

	[ThreadStatic]
	static ESCustomsDataRegistry instance;

	ESCustomsDataRegistry() { }

	public override bool IsForProductivityWise => false;

	public BooleanRegistryItem EnableESMessagingThroughDirectxTInterface
	{
		get
		{
			return GetItem("EnableESMessagingThroughDirectxTInterface", delegate
			{
				var result = new BooleanRegistryItem(
					"EnableESMessagingThroughDirectxTInterface",
					Categories.Customs_Spain,
					(NoResString)"Enable ES Messaging Through Direct xT Interface",
					(NoResString)"Enable ES Messaging Through Direct xT Interface?",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
					true);
				return result;
			});
		}
	}

	public BooleanRegistryItem EnableESInboxMessagesThroughDirectxTInterface
	{
		get
		{
			return GetItem("EnableESInboxMessagesThroughDirectxTInterface", delegate
			{
				var result = new BooleanRegistryItem(
					"EnableESInboxMessagesThroughDirectxTInterface",
					Categories.Customs_Spain,
					(NoResString)"Enable ES Inbox Messages Through Direct xT Interface",
					(NoResString)"Enable ES Inbox Messages Through Direct xT Interface?",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
					true);
				return result;
			});
		}
	}

	public BooleanRegistryItem AllowEditEDIMessageBody
	{
		get
		{
			return GetItem("AllowEditEDIMessageBody", delegate
			{
				var result = new BooleanRegistryItem(
					"AllowEditEDIMessageBody",
					Categories.Customs_Spain,
					(NoResString)"Allow Edit EDI Message Body",
					(NoResString)@"Show ""Edit Message"" checkbox that allows users with CWSupport password to edit the declaration payload before sending it. This is a development/support feature, please keep it disabled in production.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
				return result;
			});
		}
	}

	public StringRegistryItem CustomsClearanceEmailFrom
	{
		get
		{
			return GetItem("CustomsClearanceEmailFrom", delegate
			{
				return new StringRegistryItem(
					"CustomsClearanceEmailFrom",
					Categories.Customs_Spain,
					ResString.GetMultilingualString("45AE7EC0-E349-448A-8013-CE2D25CC6A4A", "Customs Clearance Email From"),
					ResString.GetMultilingualString("D1B51F12-DA9E-4B9F-9971-59AB9C70DBF2", "Customs Clearance Email From"),
					RegistryStorageFlags.System,
					RegistryOptions.IsValueMandatory,
					"AgenciaTributaria@correo.aeat.es");
			});
		}
	}

	public StringRegistryItem CustomsClearanceEmailRecipient
	{
		get
		{
			return GetItem("CustomsClearanceEmailRecipient", delegate
			{
				return new StringRegistryItem(
					"CustomsClearanceEmailRecipient",
					Categories.Customs_Spain,
					ResString.GetMultilingualString("C4CFA554-C4B9-4B70-91F4-2D0097DF0128", "Customs Clearance Email Recipient"),
					ResString.GetMultilingualString("678C6FC9-5542-4713-B3A6-8E32A77E7DEA", "Customs Clearance Email Recipient"),
					new EmailStringRegistryDataType(),
					RegistryStorageFlags.Branch,
					RegistryOptions.Default);
			});
		}
	}

	public IntRegistryItem InboxXTExpirationPeriodDays
	{
		get
		{
			return GetItem("InboxXTExpirationPeriodDays", delegate
			{
				var result = new IntRegistryItem(
					"InboxXTExpirationPeriodDays",
					Categories.Customs_Spain_Inbox_XT,
					ResString.GetMultilingualString("5AD0694A-6C27-44D7-A6E9-4E084F764E5D", "Expiration Period (days)"),
					ResString.GetMultilingualString("4BB870BD-49AB-42B3-B14E-128B4BBD4B28", "This value represents the maximum number of days the system will poll Spanish Customs for a specific Inbox message."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
					7);
				return result;
			});
		}
	}

	public BooleanRegistryItem GenerateAutomaticallyEntryIntoTemporaryStorage
	{
		get
		{
			return GetItem("GenerateAutomaticallyEntryIntoTemporaryStorage", delegate
			{
				var result = new BooleanRegistryItem(
					"GenerateAutomaticallyEntryIntoTemporaryStorage",
					Categories.Customs_Spain,
					ResString.GetMultilingualString("5C2AC4E5-3163-4B5D-B09D-F56FC9EA98A7", "Generate automatically entry into Temporary Storage"),
					ResString.GetMultilingualString("654A5578-8DC8-45FE-BBA3-0F5C0A9C45E2", "Enable this option to automatically generate the entry into the Temporary Storage (in those managed locations) when a response with clearance is received from ES Customs."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					false);
				return result;
			});
		}
	}

	#region Query URLs

	public StringRegistryItem ExportStatusQueryUrl
	{
		get
		{
			return GetItem("ExportStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ExportStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("150D6EE1-6098-4BF4-96AB-5E941C7E3591", "Export Status"),
					ResString.GetMultilingualString("558B2A95-E508-46FF-92C5-D9DA22AC71E2", "Current version of Export Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ExportStatus);
			});
		}
	}

	public StringRegistryItem ImportDjpStatusQueryUrl
	{
		get
		{
			return GetItem("ImportDjpStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportDjpStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("84D10181-A24F-47B0-984F-E9BF165E1684", "Import DJP Status"),
					ResString.GetMultilingualString("D1EBC87C-2301-41BC-8E58-1B4200E75E32", "Current version of Import DJP Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportDjpStatus);
			});
		}
	}

	public StringRegistryItem ImportPdiStatusQueryUrl
	{
		get
		{
			return GetItem("ImportPdiStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportPdiStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("63A72A92-02D5-4A07-A163-1ACFEC45DE72", "Import PDI Status"),
					ResString.GetMultilingualString("DBEF7614-B776-4B58-A9E1-E55C256DD201", "Current version of Import PDI Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportPdiStatus);
			});
		}
	}

	public StringRegistryItem ImportStatusQueryUrl
	{
		get
		{
			return GetItem("ImportStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("EEFA0C1D-403B-435E-B59A-6BB70D337BF3", "Import Status"),
					ResString.GetMultilingualString("4B7D3A1B-80D0-4A46-851C-FCDBA07D4FF6", "Current version of Import Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportStatus);
			});
		}
	}

	public StringRegistryItem ImportStatusCanaryIslandsQueryUrl
	{
		get
		{
			return GetItem("ImportStatusCanaryIslandsQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportStatusCanaryIslandsQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("02341102-01D8-4B0A-9B3B-1DB58F37BA14", "Import Status (Canary Islands)"),
					ResString.GetMultilingualString("755C7724-CDDC-456F-885C-8E93FBE53B61", "Current version of Import Status (Canary Islands) URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportStatusCanaryIslands);
			});
		}
	}

	#region Import H1

	public StringRegistryItem ImportH1StatusQueryUrl
	{
		get
		{
			return GetItem("ImportH1StatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportH1StatusQueryUrl",
					Categories.Customs_Spain_QueryURLs_ImportH1,
					ResString.GetMultilingualString("E5096B7E-B6F2-4763-AFD9-6B23CAC045A2", "Status"),
					ResString.GetMultilingualString("BC787868-A2A4-4A47-9AC9-F3ABF60BDDFB", "Current version of Import H1 Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportH1Status);
			});
		}
	}

	public StringRegistryItem ImportH1StatusCanaryIslandsQueryUrl
	{
		get
		{
			return GetItem("ImportH1StatusCanaryIslandsQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportH1StatusCanaryIslandsQueryUrl",
					Categories.Customs_Spain_QueryURLs_ImportH1,
					ResString.GetMultilingualString("7539B0F1-0F06-4AF0-BB6A-C9875B0D73A0", "Status (Canary Islands)"),
					ResString.GetMultilingualString("0AF68300-6FB2-48DE-8B09-8E87B5F5FCB0", "Current version of Import H1 Status (Canary Islands) URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportH1StatusCanaryIslands);
			});
		}
	}

	public StringRegistryItem ImportH1DJPStatusQueryUrl
	{
		get
		{
			return GetItem("ImportH1DJPStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ImportH1DJPStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs_ImportH1,
					ResString.GetMultilingualString("ED1DE957-5B2D-4C4A-9B2B-2678B7682503", "DJP Status"),
					ResString.GetMultilingualString("601D6FEE-0294-426A-804C-EDF63B171232", "Current version of Import H1 DJP Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ImportH1DJPStatus);
			});
		}
	}

	#endregion

	public StringRegistryItem NctsTransitStatusQueryUrl
	{
		get
		{
			return GetItem("NctsTransitStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"NctsTransitStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("89DADCB5-5ECC-4EE5-BA81-52781971BA30", "NCTS Transit Status"),
					ResString.GetMultilingualString("9ED00316-7AB8-41DF-9D9F-06D304B4F10A", "Current version of NCTS Transit Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.NctsTransitStatus);
			});
		}
	}

	public StringRegistryItem T2lNonUCCReceptionStatusQueryUrl
	{
		get
		{
			return GetItem("T2lNonUCCReceptionStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"T2lNonUCCReceptionStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("34152C34-8A49-417A-81EC-173BE510E3DB", "T2L Non UCC Reception Status"),
					ResString.GetMultilingualString("D7941CB1-BA9B-4949-BC37-76C091D81535", "Current version of T2L Non UCC Reception Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.T2lNonUCCReceptionStatus);
			});
		}
	}

	public StringRegistryItem T2lExpeditionAndReceptionStatusQueryUrl
	{
		get
		{
			return GetItem("T2lExpeditionAndReceptionStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"T2lExpeditionAndReceptionStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("0DDA8E6D-A3EE-4D6B-A6F1-C1A240BCC532", "T2L Expedition and Reception Status"),
					ResString.GetMultilingualString("0FCE2C44-BA8D-44FA-ABD0-887044CC8FEA", "Current version of T2L Expedition and Reception Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.T2lExpeditionAndReceptionStatus);
			});
		}
	}

	public StringRegistryItem T2cClearanceStatusQueryUrl
	{
		get
		{
			return GetItem("T2cClearanceStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"T2cClearanceStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("47302152-6628-4309-8A90-A72A64B376FC", "T2C Clearance Status"),
					ResString.GetMultilingualString("5E8C22E4-999F-4ACA-8D5F-8537D2CA25B8", "Current version of T2C Clearance Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.T2cClearanceStatus);
			});
		}
	}

	public StringRegistryItem ExsStatusQueryUrl
	{
		get
		{
			return GetItem("ExsStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ExsStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("623E122B-EC27-4577-8291-E03439AAD01A", "EXS Status"),
					ResString.GetMultilingualString("108C09DB-0BA9-453A-84BD-C19C4C0CB21D", "Current version of EXS Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ExsStatus);
			});
		}
	}

	public StringRegistryItem DvdStatusQueryUrl
	{
		get
		{
			return GetItem("DvdStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"DvdStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("5E96B301-55B9-4644-9C98-E93579ED5BB6", "DVD Status"),
					ResString.GetMultilingualString("5E07B156-D905-40FA-93AE-834AF40F3BF6", "Current version of DVD Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.DvdStatus);
			});
		}
	}

	public StringRegistryItem DvdStatusCanaryIslandsQueryUrl
	{
		get
		{
			return GetItem("DvdStatusCanaryIslandsQueryUrl", delegate
			{
				return new StringRegistryItem(
					"DvdStatusCanaryIslandsQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("FED5AB49-BCF8-4564-A98A-A531ABD0A468", "DVD Status (Canary Islands)"),
					ResString.GetMultilingualString("F2BB7E3D-6290-4CE2-BC3B-B40D38F84552", "Current version of DVD Status (Canary Islands) URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.DvdStatusCanaryIslands);
			});
		}
	}

	public StringRegistryItem ExitControlStatusQueryUrl
	{
		get
		{
			return GetItem("ExitControlStatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"ExitControlStatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("D225E681-A6A3-4D8A-8B5B-CE025B5609FA", "Exit Control Status"),
					ResString.GetMultilingualString("263E17CE-F359-42F1-9812-C0370CF05F4B", "Current version of Exit Control URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.ExitControlStatus);
			});
		}
	}

	public StringRegistryItem G5V1StatusQueryUrl
	{
		get
		{
			return GetItem("G5V1StatusQueryUrl", delegate
			{
				return new StringRegistryItem(
					"G5V1StatusQueryUrl",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("A8686102-64FF-45F7-AD93-C9C55FF98D34", "G5 V1 Status"),
					ResString.GetMultilingualString("EFBE04AB-96B8-4C2D-9919-FE1086DE2C76", "Current version of G5 V1 URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.G5V1Status);
			});
		}
	}

	public StringRegistryItem SummaryDeclarationStatusQueryURL
	{
		get
		{
			return GetItem("SummaryDeclarationStatusQueryURL", delegate
			{
				return new StringRegistryItem(
					"SummaryDeclarationStatusQueryURL",
					Categories.Customs_Spain_QueryURLs,
					ResString.GetMultilingualString("7EBFD47D-0992-422B-9D12-CAA9117438F2", "Summary Declaration Status"),
					ResString.GetMultilingualString("7144FFF5-6380-47C6-920D-E3F55272B876", "Current version of Summary Declaration Status URL for customs query"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrls.SummaryDeclarationStatus);
			});
		}
	}

	#region PUE

	public StringRegistryItem PueSendMessageUrl
	{
		get
		{
			return GetItem("PueSendMessageUrl", delegate
			{
				return new StringRegistryItem(
					"PueSendMessageUrl",
					Categories.Customs_Spain_QueryURLs_PUE,
					ResString.GetMultilingualString("316532BC-2475-4DD6-8BAB-C0890CCF1441", "Send Message"),
					ResString.GetMultilingualString("5DE6161B-ADD7-4743-A6B6-8F1ECB2E74EA", "Current version of Send Message URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.SendMessageUrl);
			});
		}
	}

	public StringRegistryItem PueAnnexDocumentsUrl
	{
		get
		{
			return GetItem("PueAnnexDocumentsUrl", delegate
			{
				return new StringRegistryItem(
					"PueAnnexDocumentsUrl",
					Categories.Customs_Spain_QueryURLs_PUE,
					ResString.GetMultilingualString("A331C6C5-1745-4B0E-B556-458B4C570CA6", "Annex Documents"),
					ResString.GetMultilingualString("66652CEA-1638-444B-B6EC-8FBB35CF9D9E", "Current version of Annex Documents URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.AnnexDocumentsUrl);
			});
		}
	}

	public StringRegistryItem RohsRequestUrl
	{
		get
		{
			return GetItem("RohsRequestUrl", delegate
			{
				return new StringRegistryItem(
					"RohsRequestUrl",
					Categories.Customs_Spain_QueryURLs_PUE_ROHS_RAEE,
					ResString.GetMultilingualString("79DE5272-1AE9-4A5D-9D3D-93AC3166EA46", "ROHS Request"),
					ResString.GetMultilingualString("11833A63-680E-44A0-84BC-93C6896A3A64", "Current version of ROHS Request URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.RohsRaee.RequestUrl);
			});
		}
	}

	public StringRegistryItem RohsAdditionalDataUrl
	{
		get
		{
			return GetItem("RohsAdditionalDataUrl", delegate
			{
				return new StringRegistryItem(
					"RohsAdditionalDataUrl",
					Categories.Customs_Spain_QueryURLs_PUE_ROHS_RAEE,
					ResString.GetMultilingualString("7009C001-2541-46A9-906F-7E39960E47C5", "ROHS Additional Data"),
					ResString.GetMultilingualString("93DAABDE-C93A-44C3-ADF5-CED39B3C886D", "Current version of ROHS Additional Data URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.RohsRaee.AdditionalDataUrl);
			});
		}
	}

	public StringRegistryItem RohsStatusUrl
	{
		get
		{
			return GetItem("RohsStatusUrl", delegate
			{
				return new StringRegistryItem(
					"RohsStatusUrl",
					Categories.Customs_Spain_QueryURLs_PUE_ROHS_RAEE,
					ResString.GetMultilingualString("7C7812D4-D5CB-4D94-8E9F-33413704D284", "ROHS Status"),
					ResString.GetMultilingualString("0C4B745B-F1EF-4BE6-AC81-E660A21BAC33", "Current version of ROHS Status URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.RohsRaee.StatusUrl);
			});
		}
	}

	public StringRegistryItem ComRequestUrl
	{
		get
		{
			return GetItem("ComRequestUrl", delegate
			{
				return new StringRegistryItem(
					"ComRequestUrl",
					Categories.Customs_Spain_QueryURLs_PUE_COM,
					ResString.GetMultilingualString("C839BFE8-16A2-4086-82CA-60CCD9162AE8", "COM Request"),
					ResString.GetMultilingualString("57A0574C-6477-4D84-BB30-FBF70DFFD04C", "Current version of COM Request URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.Com.RequestUrl);
			});
		}
	}

	public StringRegistryItem ComAdditionalDataUrl
	{
		get
		{
			return GetItem("ComAdditionalDataUrl", delegate
			{
				return new StringRegistryItem(
					"ComAdditionalDataUrl",
					Categories.Customs_Spain_QueryURLs_PUE_COM,
					ResString.GetMultilingualString("45C03FBB-8AAD-4EBA-A1C8-1BC06D699303", "COM Additional Data"),
					ResString.GetMultilingualString("0F0621D3-F8DE-437D-8DC6-52578A4C8343", "Current version of COM Additional Data URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.Com.AdditionalDataUrl);
			});
		}
	}

	public StringRegistryItem ComStatusUrl
	{
		get
		{
			return GetItem("ComStatusUrl", delegate
			{
				return new StringRegistryItem(
					"ComStatusUrl",
					Categories.Customs_Spain_QueryURLs_PUE_COM,
					ResString.GetMultilingualString("075CC02E-BE4E-45C4-BCDB-A5F79B89ACFC", "COM Status"),
					ResString.GetMultilingualString("435D6C0D-E534-4364-9AC8-3EEAD7B25F71", "Current version of COM Status URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.Com.StatusUrl);
			});
		}
	}

	public StringRegistryItem EcoRequestUrl
	{
		get
		{
			return GetItem("EcoRequestUrl", delegate
			{
				return new StringRegistryItem(
					"EcoRequestUrl",
					Categories.Customs_Spain_QueryURLs_PUE_ECO,
					ResString.GetMultilingualString("7A6924D8-584F-455C-B1F8-FCF2C5B548F7", "ECO Request"),
					ResString.GetMultilingualString("EB78E5CC-51CA-4AED-84E0-ACAA06A68ED9", "Current version of ECO Request URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.Eco.RequestUrl);
			});
		}
	}

	public StringRegistryItem EcoAdditionalDataUrl
	{
		get
		{
			return GetItem("EcoAdditionalDataUrl", delegate
			{
				return new StringRegistryItem(
					"EcoAdditionalDataUrl",
					Categories.Customs_Spain_QueryURLs_PUE_ECO,
					ResString.GetMultilingualString("AB650515-F155-4040-A0A9-3605E452B04E", "ECO Additional Data"),
					ResString.GetMultilingualString("C63A6029-5F96-4E6F-81EF-26D483BBDDDE", "Current version of ECO Additional Data URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.Eco.AdditionalDataUrl);
			});
		}
	}

	public StringRegistryItem EcoStatusUrl
	{
		get
		{
			return GetItem("EcoStatusUrl", delegate
			{
				return new StringRegistryItem(
					"EcoStatusUrl",
					Categories.Customs_Spain_QueryURLs_PUE_ECO,
					ResString.GetMultilingualString("533262FA-7908-431A-9774-2F32F6101A2C", "ECO Status"),
					ResString.GetMultilingualString("AC176EFD-E943-4CB8-A5ED-E3185147C0E7", "Current version of ECO Status URL"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory,
					CustomsWebsiteUrlsPue.Eco.StatusUrl);
			});
		}
	}

	#endregion

	#endregion

	#region Message Version

	public CodePairRegistryItem ESExportMessageVersion
	{
		get
		{
			return GetItem("ESExportMessageVersion", delegate
			{
				return new CodePairRegistryItem(
					"ESExportMessageVersion",
					Categories.Customs_Spain_MessageVersion,
					ResString.GetMultilingualString("4C19CB80-9F6D-40EE-8793-EF7F4AE0EB8C", "Export"),
					ResString.GetMultilingualString("86F57254-3ABD-4BBB-9D5A-86F14C5CC7DA", "Current export message version configured to submit to Customs."),
					new CodeDescriptionPairListProvider(() => new EXPORTVersionNumberList()),
					RegistryStorageFlags.Branch,
					EXPORTVersionNumberList.Codes.Aes);
			});
		}
	}

	public CodePairRegistryItem EST2LMessageVersion
	{
		get
		{
			return GetItem("EST2LMessageVersion", delegate
			{
				return new CodePairRegistryItem(
					"EST2LMessageVersion",
					Categories.Customs_Spain_MessageVersion,
					ResString.GetMultilingualString("BD6539C7-0E7C-40D9-AD1E-FA497AB5167B", "T2L"),
					ResString.GetMultilingualString("BD681BA5-3A11-42CA-B812-4BCFAD10C626", "Current T2L message version configured to submit to Customs."),
					new CodeDescriptionPairListProvider(() => new T2LVersionNumberList()),
					RegistryStorageFlags.Branch,
					T2LVersionNumberList.Codes.RequestJecAndReceptionPous);
			});
		}
	}

	public CodePairRegistryItem ESImportMessageVersion
	{
		get
		{
			return GetItem("ESImportMessageVersion", delegate
			{
				return new CodePairRegistryItem(
					"ESImportMessageVersion",
					Categories.Customs_Spain_MessageVersion,
					ResString.GetMultilingualString("FE0F321A-CD7A-4043-82C4-62B91203D7B2", "Import (Developers Only)"),
					ResString.GetMultilingualString("AA253EE1-1848-4544-91DB-310160C4C5A4", "Current import message version configured to submit to Customs."),
					new CodeDescriptionPairListProvider(() => new IMPORTVersionNumberList()),
					RegistryStorageFlags.Branch,
					RegistryOptions.IsOnlyForDevelopers,
					IMPORTVersionNumberList.Codes.Ics);
			});
		}
	}

	#endregion
}
