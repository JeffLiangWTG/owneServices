using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.DocumentScanning.Business.DocManagerRegistry;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DocManagerRegistry))]
	public sealed class DocManagerRegistryTest : RegistryItemSetTestCaseWithFactory<DocManagerRegistry>
	{
		public void TestEDocsAuthTokenEncryptionKey()
		{
			TestStringRegistryItem(ItemSet.EDocsAuthTokenEncryptionKey,
				"EDocsAuthTokenEncryptionKey",
				Categories.System_DocManager_DocumentIngestion,
				string.Empty,
				string.Empty,
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
				"",
				CharacterCase.Normal);
		}

		public void TestEnableCommercialInvoiceDocumentParsing()
		{
			TestRegistryItem(ItemSet.EnableCommercialInvoiceDocumentParsing,
				"EnableCommercialInvoiceDocumentParsing",
				Categories.System_DocManager_DocumentIngestion_ParseTypes_CommercialInvoice,
				"Enable CIV Parsing",
				"Enable parsing of the CIV Parse Type",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: false);
		}

		public void TestEnableAccountsPayableInvoiceDocumentParsing()
		{
			TestRegistryItem(ItemSet.EnableAccountsPayableInvoiceDocumentParsing,
				"EnableAccountsPayableInvoiceDocumentParsing",
				Categories.System_DocManager_DocumentIngestion_ParseTypes_AccountPayableInvoice,
				"Enable PIN Parsing",
				"Enable parsing of the PIN Parse Type",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: false);
		}

		public void TestDocumentParserUrl()
		{
			TestStringRegistryItem(ItemSet.DocumentParserUrl,
				"DocumentParserUrl",
				Categories.System_DocManager_DocumentIngestion_ParserConfiguration,
				"Parser URL",
				"This URL is used to integrate with document parser.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal,
				"https://shipamaxintegration.com");
		}

		public void TestDocumentParserClientId()
		{
			TestStringRegistryItem(ItemSet.DocumentParserClientId,
				"DocumentParserClientId",
				Categories.System_DocManager_DocumentIngestion_ParserConfiguration,
				"Parser Client Id",
				"It is a permanent id coming from Azure application which represents document parser.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				"ff853bf7-76c5-432b-bd16-95035f16cbc6",
				CharacterCase.Normal);
		}

		public void TestShipamaxServiceCode()
		{
			var item = ItemSet.ShipamaxServiceCode;
			TestStringRegistryItem(item,
				"ShipamaxServiceCode",
				Categories.System_DocManager_DocumentIngestion_ParserConfiguration,
				"Document Ingestion Service Code",
				"User initials are sometimes required to save entities to the database. For System To System Trust requests, we will use this value for Document Ingestion as this information is not available in the token.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				"~SM",
				CharacterCase.Upper,
				"FOO");

			AssertEquals(1, ((StringRegistryDataType)item.DataType).MinLength);
			AssertEquals(3, ((StringRegistryDataType)item.DataType).MaxLength);
		}

		public void TestEnableEDocsVirusScanning()
		{
			TestRegistryItem(ItemSet.EnableEDocsVirusScanning,
				"EnableEDocsVirusScanning",
				Categories.System_DocManager,
				"Enable eDocs Virus Scanning",
				"When the registry is set to 'Yes', all the eDocs are scanned by the default system anti-virus software when they are uploaded and accessed.",
				RegistryStorageFlags.System,
				(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
				false
				);
		}

		public void TestConvertImagesToJpeg()
		{
			TestRegistryItem(ItemSet.ConvertImagesToJpeg,
				"ConvertImagesToJpeg",
				Categories.System_DocManager,
				"Convert images to JPEG format when possible",
				"When the registry is set to 'Yes', convert image documents to JPEG format to reduce image sizes when possible.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				true
				);
		}

		public void TestEDocsExternalStorageSize()
		{
			TestStringRegistryItem(ItemSet.EDocsExternalStorageSize,
				"EDocsExternalStorageSize",
				Categories.System_DocManager_S3Storage,
				string.Empty,
				string.Empty,
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
				"",
				CharacterCase.Normal);
		}

		public void TestUseCalculatedExternalStorageSize()
		{
			var enableExternalStorageCalculation = EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI;
			TestRegistryItem(ItemSet.UseCalculatedExternalStorageSize,
				"UseCalculatedExternalStorageSize",
				Categories.System_DocManager_S3Storage,
				"Use Calculated External Storage Size",
				"When enabled, the S3 bucket size will be calculated from the file sizes stored in the StorageDocs table in the DocManager database.",
				RegistryStorageFlags.System,
				enableExternalStorageCalculation ? (RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue) : RegistryOptions.IsHidden,
				enableExternalStorageCalculation);
		}	

		#region Restrict Email Allocation For Organization Contacts

		public void TestRestrictEmailAllocationForOrgContacts()
		{
			TestRegistryItem(ItemSet.RestrictEmailAllocationForOrgContacts,
				"RestrictEmailAllocationForOrgContacts",
				Categories.System_DocManager,
				"Restrict Email Allocation For Organization Contacts",
				"This setting, when enabled, will allow the DocManager Import Service Task to only allocate documents from Organization contacts.\r\n\r\nNote: When enabled, this setting will not supersede the System -> DocManager -> Email Addresses Allowed For Import Registry settings.",
				RegistryStorageFlags.System,RegistryOptions.PreserveTestValue,
				false);
		}

		#endregion

		#region File Upload Stream Buffer Size

		public void TestFileUploadStreamBufferSize()
		{
			TestRegistryItem(ItemSet.FileUploadStreamBufferSize,
				"FileUploadStreamBufferSize",
				Categories.System_DocManager,
				ResString.GetMultilingualString("A1E8AE81-55D4-4AB6-8380-EEEA5507FFB2", "File Upload Stream Buffer Size"),
				ResString.GetMultilingualString("024ED94D-9B13-4DA6-8886-1291BB3AD0CB", "Sets the File Stream buffer size when uploading files as eDocs. Setting this to 0 or a negative value will use the default of 4096"),
				RegistryStorageFlags.System,
				(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
				0);
		}

		#endregion

		#region Remember eDoc enable preview setting

		public void TestEDocsPreviewEnabled()
		{
			TestRegistryItem(ItemSet.EDocsPreviewEnabled,
				"EDocsPreviewEnabled",
				Categories.Forms,
				"eDocs enable preview setting",
				"Determines if the Enable Preview option is selected by default when opening the eDocs window.",
				RegistryStorageFlags.All,
				RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue | RegistryOptions.NotLogged,
				false);
		}

		#endregion

		#region Enable S3 File Action Usage Report

		public void TestEnableS3UsageReportHostedWithCargoWise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			TestRegistryItem(ItemSet.EnableS3UsageReport,
				"EnableS3UsageReport",
				Categories.System_DocManager_S3Storage,
				"S3 usage report setting",
				"Determines if the S3 usage report feature is enabled.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestEnableS3UsageReportSelfHosted()
		{
			EnvProxy.SetHostedLocationForTest("");
			TestRegistryItem(ItemSet.EnableS3UsageReport,
				"EnableS3UsageReport",
				Categories.System_DocManager_S3Storage,
				"S3 usage report setting",
				"Determines if the S3 usage report feature is enabled.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.NotLogged | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestEnableS3UsageReportEDIProd()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				TestRegistryItem(ItemSet.EnableS3UsageReport,
					"EnableS3UsageReport",
					Categories.System_DocManager_S3Storage,
					"S3 usage report setting",
					"Determines if the S3 usage report feature is enabled.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					true);
			}
		}

		#endregion

		#region ConditionallyVisibleRegistryItems

		protected override System.Collections.Generic.IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableCommercialInvoiceDocumentParsing";
				yield return "EnableAccountsPayableInvoiceDocumentParsing";
				yield return "DocumentParserUrl";
				yield return "ShipamaxServiceCode";
			}
		}

		#endregion

		#region EDocs Encryption Master Key Rotation Period

		public void TestEDocsEncryptionMasterKeyRotationSelfHosted()
		{
			EnvProxy.SetHostedLocationForTest(string.Empty);
			TestEDocsEncryptionMasterKeyRotation(RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue);
		}

		public void TestEDocsEncryptionMasterKeyRotationCargoWiseHosted()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			TestEDocsEncryptionMasterKeyRotation(RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
		}

		void TestEDocsEncryptionMasterKeyRotation(RegistryOptions options)
		{
			TestRegistryItem(ItemSet.EDocsEncryptionMasterKeyRotationPeriod,
				"EDocsEncryptionMasterKeyRotationPeriod",
				Categories.System_DocManager_S3Storage,
				"eDocs Encryption Master Key Rotation Period (Days)",
				"The number of days that the eDocs encryption master key should be rotated (60 - 9999).",
				RegistryStorageFlags.System,
				options,
				365, 60, 9999);
		}

		#endregion
	}
}
