using System;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using SharedDocManagerRegistry = CargoWise.Definitions.DocManagerRegistry;

namespace Enterprise.DocumentScanning.Business
{
	public sealed class DocManagerRegistry : RegistryItemSet
	{
		#region Singleton Pattern

		public static DocManagerRegistry Instance => instance ?? (instance = new DocManagerRegistry());

		[ThreadStatic]
		static DocManagerRegistry instance;

		DocManagerRegistry()
		{
		}

		#endregion
		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : SystemDataRegistry.Categories
		{
			public static MultilingualString System_DocManager_DocumentIngestion => CombineCategories(System_DocManager, ResString.GetMultilingualString("5945AEED-9F79-4152-94A5-BF6DA5BAC3B1", "Document Ingestion"));
			public static MultilingualString System_DocManager_DocumentIngestion_ParserConfiguration => CombineCategories(System_DocManager_DocumentIngestion, ResString.GetMultilingualString("6675815B-7C04-428A-AE02-2D7312660D29", "Parser Configuration"));
			public static MultilingualString System_DocManager_DocumentIngestion_ParseTypes => CombineCategories(DocManagerRegistry.Categories.System_DocManager_DocumentIngestion, ResString.GetMultilingualString("4D306534-5F93-46FA-BABB-C30BC8A5C99A", "Parse Types"));
			public static MultilingualString System_DocManager_DocumentIngestion_ParseTypes_CommercialInvoice => CombineCategories(System_DocManager_DocumentIngestion_ParseTypes, ResString.GetMultilingualString("C85D3069-E5A9-47EF-9D7F-5B719688DDE8", "Commercial Invoice"));
			public static MultilingualString System_DocManager_DocumentIngestion_ParseTypes_AccountPayableInvoice => CombineCategories(System_DocManager_DocumentIngestion_ParseTypes, ResString.GetMultilingualString("E9C74464-8895-4238-979E-652A4AB82B0E", "AP Invoice"));
		}

		#endregion

		#region EDocsAuthTokenEncryptionKey

		public StringRegistryItem EDocsAuthTokenEncryptionKey
		{
			get
			{
				return GetItem("EDocsAuthTokenEncryptionKey", () => new StringRegistryItem(
					name: "EDocsAuthTokenEncryptionKey",
					category: Categories.System_DocManager_DocumentIngestion,
					caption: null,
					hint: null,
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue));
			}
		}

		#endregion

		public BooleanRegistryItem EnableDSPServiceTaskCollectDebugData  // this is temporary registry to assit FR, will be removed later
		{
			get
			{
				return GetItem("EnableDSPServiceTaskCollectDebugData", () => new BooleanRegistryItem(
					name: "EnableDSPServiceTaskCollectDebugData",
					category: Categories.System_DocManager_DocumentIngestion,
					(NoResString)"Enable DSP Service Task Collect Debug Data",
					(NoResString)"This is a temporary registry only to assist Accounting team's functional test, will be removed later",
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport,
					defaultValue: false));
			}
		}

		#region EnableCommercialInvoiceDocumentParsing

		public BooleanRegistryItem EnableCommercialInvoiceDocumentParsing
		{
			get
			{
				return GetItem("EnableCommercialInvoiceDocumentParsing", () => new BooleanRegistryItem(
					name: "EnableCommercialInvoiceDocumentParsing",
					category: Categories.System_DocManager_DocumentIngestion_ParseTypes_CommercialInvoice,
					caption: ResString.GetMultilingualString("081A5DF0-B385-4D20-935B-7AC91D20BF09", "Enable CIV Parsing"),
					hint: ResString.GetMultilingualString("0D4EAFDC-7FA5-494C-B1C2-0CF1A3DDADDB", "Enable parsing of the CIV Parse Type"),
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
					defaultValue: false));
			}
		}

		#endregion

		#region EnableAccountsPayableInvoiceDocumentParsing

		public BooleanRegistryItem EnableAccountsPayableInvoiceDocumentParsing
		{
			get
			{
				return GetItem("EnableAccountsPayableInvoiceDocumentParsing", () => new BooleanRegistryItem(
					name: "EnableAccountsPayableInvoiceDocumentParsing",
					category: Categories.System_DocManager_DocumentIngestion_ParseTypes_AccountPayableInvoice,
					caption: ResString.GetMultilingualString("4F170178-4D83-469B-95BD-4FD9FA49C4F3", "Enable PIN Parsing"),
					hint: ResString.GetMultilingualString("25EB2251-1A54-42C2-8FB0-2F52543DC233", "Enable parsing of the PIN Parse Type"),
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
					defaultValue: false));
			}
		}

		#endregion

		#region DocumentParserUrl

		public StringRegistryItem DocumentParserUrl
		{
			get
			{
				return GetItem("DocumentParserUrl", () => new StringRegistryItem(
					name: "DocumentParserUrl",
					category: Categories.System_DocManager_DocumentIngestion_ParserConfiguration,
					caption: ResString.GetMultilingualString("B017F044-1D78-4ED5-B742-5A8B5A58B5CA", "Parser URL"),
					hint: ResString.GetMultilingualString("5788F660-20EE-48AD-8517-23CB28DC2E3E", "This URL is used to integrate with document parser."),
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					dataType: new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false }));
			}
		}

		#endregion

		#region DocumentParserClientId

		public StringRegistryItem DocumentParserClientId
		{
			get
			{
				return GetItem(SharedDocManagerRegistry.DocumentParserClientIdKey, delegate
				{
					var result = new StringRegistryItem(
						SharedDocManagerRegistry.DocumentParserClientIdKey,
						Categories.System_DocManager_DocumentIngestion_ParserConfiguration,
						(NoResString)"Parser Client Id",
						(NoResString)"It is a permanent id coming from Azure application which represents document parser.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						SharedDocManagerRegistry.DocumentParserClientId);
					return result;
				});
			}
		}

		#endregion

		#region ExternalStorageSize

		public StringRegistryItem EDocsExternalStorageSize
		{
			get
			{
				return GetItem("EDocsExternalStorageSize", () => new StringRegistryItem(
					name: "EDocsExternalStorageSize",
					category: Categories.System_DocManager_S3Storage,
					caption: null,
					hint: null,
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue));
			}
		}

		public BooleanRegistryItem UseCalculatedExternalStorageSize
		{
			get
			{
				var enableExternalStorageCalculation = EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI;
				return GetItem("UseCalculatedExternalStorageSize", delegate
				{
					var result = new BooleanRegistryItem(
						"UseCalculatedExternalStorageSize",
						Categories.System_DocManager_S3Storage,
						(NoResString)"Use Calculated External Storage Size",
						(NoResString)"When enabled, the S3 bucket size will be calculated from the file sizes stored in the StorageDocs table in the DocManager database.",
						RegistryStorageFlags.System,
						enableExternalStorageCalculation ? (RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue) : RegistryOptions.IsHidden,
						enableExternalStorageCalculation);
					return result;
				});
			}
		}

		#endregion

		#region ShipamaxIntegrationServiceCode

		public StringRegistryItem ShipamaxServiceCode
		{
			get
			{
				return GetItem(SharedDocManagerRegistry.ShipamaxServiceCodeKey, delegate
				{
					var result = new StringRegistryItem(
						SharedDocManagerRegistry.ShipamaxServiceCodeKey,
						Categories.System_DocManager_DocumentIngestion_ParserConfiguration,
						ResString.GetMultilingualString("91b3a3bc-e66c-4d5a-af5a-db6ab61f0b5a", "Document Ingestion Service Code"),
						ResString.GetMultilingualString("ec305d7e-4992-4c8f-82e4-f472a7d45d81", "User initials are sometimes required to save entities to the database. For System To System Trust requests, we will use this value for Document Ingestion as this information is not available in the token."),
						new StringRegistryDataType(CharacterCase.Upper, 1, 3),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						SharedDocManagerRegistry.ShipamaxServiceCode);
					return result;
				});
			}
		}

		#endregion

		#region eDocs Virus Scanning

		public BooleanRegistryItem EnableEDocsVirusScanning
		{
			get
			{
				return GetItem("EnableEDocsVirusScanning", () => new BooleanRegistryItem(
					name: "EnableEDocsVirusScanning",
					category: Categories.System_DocManager,
					caption: ResString.GetMultilingualString("D90CE7E0-A26E-4FC7-A976-51673B35C232", "Enable eDocs Virus Scanning"),
					hint: ResString.GetMultilingualString("9FFD7679-5068-42C5-98DD-81AA737494A3", "When the registry is set to 'Yes', all the eDocs are scanned by the default system anti-virus software when they are uploaded and accessed."),
					storage: RegistryStorageFlags.System,
					options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
					defaultValue: false));
			}
		}

		#endregion

		#region Restrict Email Allocation For Organization Contacts

		public BooleanRegistryItem RestrictEmailAllocationForOrgContacts
		{
			get
			{
				return GetItem("RestrictEmailAllocationForOrgContacts", delegate
				{
					return new BooleanRegistryItem(
						"RestrictEmailAllocationForOrgContacts",
						Categories.System_DocManager,
						ResString.GetMultilingualString("797A5FC5-17FD-4741-B3B5-91C1823D2E72", "Restrict Email Allocation For Organization Contacts"),
						ResString.GetMultilingualString("FDA4E1DA-3A4C-43A1-B7E7-305C1388EB39", "This setting, when enabled, will allow the DocManager Import Service Task to only allocate documents from Organization contacts.\r\n\r\nNote: When enabled, this setting will not supersede the {0} Registry settings.", ((IMultilingualRegistryItem)SystemDataRegistry.Instance.EmailAddressesAllowedForImport).LocationMultilingual),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region File Upload Stream Buffer Size

		public IntRegistryItem FileUploadStreamBufferSize
		{
			get
			{
				return GetItem("FileUploadStreamBufferSize", delegate
				{
					return new IntRegistryItem(
						name: "FileUploadStreamBufferSize",
						categories: Categories.System_DocManager,
						caption: ResString.GetMultilingualString("A1E8AE81-55D4-4AB6-8380-EEEA5507FFB2", "File Upload Stream Buffer Size"),
						hint: ResString.GetMultilingualString("024ED94D-9B13-4DA6-8886-1291BB3AD0CB", "Sets the File Stream buffer size when uploading files as eDocs. Setting this to 0 or a negative value will use the default of 4096"),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						defaultValue: 0);
				});
			}
		}

		#endregion

		#region Remember eDoc enable preview setting

		public BooleanRegistryItem EDocsPreviewEnabled
		{
			get
			{
				return GetItem("EDocsPreviewEnabled", delegate
				{
					return new BooleanRegistryItem(
						"EDocsPreviewEnabled",
						Categories.Forms,
						(NoResString)"eDocs enable preview setting",
						(NoResString)"Determines if the Enable Preview option is selected by default when opening the eDocs window.",
						RegistryStorageFlags.All,
						RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue | RegistryOptions.NotLogged,
						false);
				});
			}
		}

		#endregion

		#region Enable S3 File Action Usage Report

		public BooleanRegistryItem EnableS3UsageReport
		{
			get
			{
				return GetItem("EnableS3UsageReport", delegate
				{
					var enableUsageReport = EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI;
					return new BooleanRegistryItem(
						"EnableS3UsageReport",
						Categories.System_DocManager_S3Storage,
						(NoResString)"S3 usage report setting",
						(NoResString)"Determines if the S3 usage report feature is enabled.",
						RegistryStorageFlags.System,
						(enableUsageReport ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.NotLogged) | RegistryOptions.PreserveTestValue,
						enableUsageReport);
				});
			}
		}

		#endregion

		#region EDocs Encryption Master Key Rotation Peroid

		public IntRegistryItem EDocsEncryptionMasterKeyRotationPeriod
		{
			get
			{
				return GetItem("EDocsEncryptionMasterKeyRotationPeriod", delegate
				{
					var result = new IntRegistryItem(
						name: "EDocsEncryptionMasterKeyRotationPeriod",
						categories: Categories.System_DocManager_S3Storage,
						caption: ResString.GetMultilingualString("E53514C3-602C-4638-B92D-1F73BB9C8E4F", "eDocs Encryption Master Key Rotation Period (Days)"),
						hint: ResString.GetMultilingualString("77CD2974-52BE-4BC1-A177-4101EE1C1D2B", "The number of days that the eDocs encryption master key should be rotated (60 - 9999)."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						defaultValue: 365);

					result.DataType = new IntRegistryDataType(60, 9999);
					return result;
				});
			}
		}

		#endregion

		#region Convert Images To JPEG

		public BooleanRegistryItem ConvertImagesToJpeg
		{
			get
			{
				return GetItem("ConvertImagesToJpeg", () => new BooleanRegistryItem(
					name: "ConvertImagesToJpeg",
					category: Categories.System_DocManager,
					caption: ResString.GetMultilingualString("7CCFB8F3-4F2E-44E1-9336-24B68E616E08", "Convert images to JPEG format when possible"),
					hint: ResString.GetMultilingualString("016E2E82-4F31-430E-A57E-4DF376DEFDA0", "When the registry is set to 'Yes', convert image documents to JPEG format to reduce image sizes when possible."),
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
					defaultValue: true));
			}
		}

		#endregion
	}
}
