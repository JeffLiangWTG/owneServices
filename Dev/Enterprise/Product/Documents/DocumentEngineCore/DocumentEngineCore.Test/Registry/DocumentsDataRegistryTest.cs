using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentsDataRegistry))]
	sealed class DocumentsDataRegistryTest : RegistryItemSetTestCaseWithFactory<DocumentsDataRegistry>
	{
		public void TestStoredNumberOfSupersededTemplates()
		{
			AssertEquals("StoredNumberOfSupersededTemplates", ItemSet.StoredNumberOfSupersededTemplates.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents, ItemSet.StoredNumberOfSupersededTemplates.Category);
			AssertEquals("Stored number of superseded templates", ItemSet.StoredNumberOfSupersededTemplates.Caption);
			AssertEquals("This setting manages how many superseded templates are retained.", ItemSet.StoredNumberOfSupersededTemplates.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.StoredNumberOfSupersededTemplates.Storage);
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.StoredNumberOfSupersededTemplates.Options);
			AssertEquals(1, (int)(ItemSet.StoredNumberOfSupersededTemplates.DataType as IntRegistryDataType).LowerBound);
			AssertEquals(Int32.MaxValue, (int)(ItemSet.StoredNumberOfSupersededTemplates.DataType as IntRegistryDataType).UpperBound);
		}

		public void TestSignaturePlaceholderSize()
		{
			AssertEquals("DocumentSignaturePlaceholderSize", ItemSet.DocumentSignaturePlaceholderSize.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, ItemSet.DocumentSignaturePlaceholderSize.Category);
			AssertEquals("Document Signature Placeholder Size", ItemSet.DocumentSignaturePlaceholderSize.Caption);
			var expectedHint = @"This setting specifies the initial memory allocation for digital signatures in your documents, measured in kilobytes (KB).

Default Size: We start with a pre-set size of 16 KB to accommodate typical signatures. This size is a balance between ensuring enough space for most signatures and maintaining a compact document size.
Dynamic Adjustment: If a signature exceeds this initial allocation, the system will automatically expand the memory space to fit it. This ensures that all documents, even those with unexpectedly large signatures, are processed successfully without any manual intervention.
Impact on Document Size: Please note that increasing this value might result in larger overall document sizes. We recommend leaving it at the default unless you frequently encounter issues with signatures not fitting the allocated space.
Tips for Optimal Use: If you're unsure about adjusting this setting, keep it at the default. It's optimized for general use. Adjust only if you have specific needs for larger signatures.";
			AssertEquals(expectedHint, ItemSet.DocumentSignaturePlaceholderSize.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DocumentSignaturePlaceholderSize.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.DocumentSignaturePlaceholderSize.Options);
			AssertEquals(16, (int)(ItemSet.DocumentSignaturePlaceholderSize.DataType as IntRegistryDataType).LowerBound);
			AssertEquals(128, (int)(ItemSet.DocumentSignaturePlaceholderSize.DataType as IntRegistryDataType).UpperBound);
		}

		public void TestDocumentSigningNotificationGroup()
		{
			GuidRegistryItem item = ItemSet.DocumentSigningNotificationGroup;
			AssertEquals("DocumentSigningNotificationGroup", item.Name);
			AssertEquals("Document Signing Notification Group", item.Caption);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, item.Category);
			AssertEquals("The staff group that will be notified about errors in the Document Signing Service.", item.Hint);
			AssertEquals(typeof(GuidFindBoxRegistryEditorInfo), item.EditorInfo.GetType());
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
		}

		public void TestDocumentsAllowedForSigning()
		{
			AssertEquals("DocumentsAllowedForSigning", ItemSet.DocumentsAllowedForSigning.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, ItemSet.DocumentsAllowedForSigning.Category);
			AssertEquals("Documents Allowed For Signing", ItemSet.DocumentsAllowedForSigning.Caption);
			var expectedHint = @"This list contains the document types which have been enabled for remote document signing. If document types are not in this list, they will be blocked from remote signing.

Note: If the logged-in user is a CWSupport user, ignore this rule and allow any document type to be saved and sent for remote document signing using DOS service task.";
			AssertEquals(expectedHint, ItemSet.DocumentsAllowedForSigning.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DocumentsAllowedForSigning.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.DocumentsAllowedForSigning.Options);
		}

		public void TestSigningServiceSignedByLabel()
		{
			TestRegistryItem(
				ItemSet.SigningServiceSignedByLabel,
				"SigningServiceSignedByLabel",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Signing Service Signed By Label",
				"The name of the signing entity displayed on the visible signature and incorporated into the digital signature.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				ProductName,
				testValueToSetAndRead: "WTG");

			AssertEquals("DataType.MaxLength", 80, ((StringRegistryDataType)ItemSet.SigningServiceSignedByLabel.DataType).MaxLength);
			AssertEquals("DataType.MinLength", 3, ((StringRegistryDataType)ItemSet.SigningServiceSignedByLabel.DataType).MinLength);
		}

		public void TestDefaultSigningServiceSignedByLabelString()
		{
			using (BrandingFactory.ConfigureTemporary(() => new DummyCargoWiseOneBranding()))
			{
				AssertEquals("Test Product", ItemSet.SigningServiceSignedByLabel.DefaultValue);
			}
		}

		class DummyCargoWiseOneBranding : WiseTechGlobalBranding
		{
			protected override string ProductName => "Test Product";
			protected override string ProductBrandingName => "Test Product Brand";
			protected override Image ProductLogo => throw new NotImplementedException();

			protected override Icon ProductIcon => throw new NotImplementedException();

			protected override string ProductSupportName => throw new NotImplementedException();

			protected override Image SplashScreenImage => throw new NotImplementedException();

			protected override Image LoginScreenImage => throw new NotImplementedException();

			protected override Image AboutScreenImage => throw new NotImplementedException();
		}

		public void TestDocumentSigningServiceCertificateExpiryDate()
		{
			EnvProxy.SetHostedLocationForTest("");
			TestRegistryItem(
				ItemSet.DocumentSigningServiceCertificateExpiryDate,
				"DocumentSigningServiceCertificateExpiryDate",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Document Signing Service Expiry Date",
				"This registry manually stores the date the current document signing digital signature certificate expires.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				DateTime.MinValue);

			var editInfo = (DateTimeRegistryEditorInfo)ItemSet.DocumentSigningServiceCertificateExpiryDate.EditorInfo;
			AssertEquals(ZDateTimePickerFormat.Long, editInfo.DateTimeFormat);
		}

		public void TestDocumentSigningServiceLastCertificateExpiryNotificationDate()
		{
			TestRegistryItem(
				ItemSet.DocumentSigningServiceLastCertificateExpiryNotificationDate,
				"DocumentSigningServiceLastCertificateExpiryNotificationDate",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Last Document Signing Service Expiry Notification Date",
				"This registry stores the last time a notification email was sent regarding the expiring of the document signing digital signature certificate.",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsHidden,
				DateTime.MinValue);

			var editInfo = (DateTimeRegistryEditorInfo)ItemSet.DocumentSigningServiceLastCertificateExpiryNotificationDate.EditorInfo;
			AssertEquals(ZDateTimePickerFormat.Long, editInfo.DateTimeFormat);
		}

		public void TestSigningServiceSignedByLabel_InvalidChars()
		{
			AssertEquals("The value cannot contain following characters: .", ItemSet.SigningServiceSignedByLabel.GetValidationErrorMessage("WTG. Good", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("", ItemSet.SigningServiceSignedByLabel.GetValidationErrorMessage("WTG is Good", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestSigningServiceRetriesInterval()
		{
			TestRegistryItem(
				ItemSet.SigningServiceRetriesInterval,
				"SigningServiceRetriesInterval",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Signing Service Retries Interval",
				"This setting is used to determine the minimum wait time (in minutes) between retries. The DOS service task will retry up to a maximum of three attempts before failing.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				10, 0, 60);
		}

		public void TestOverrideSignedByUsername()
		{
			TestRegistryItem(
				ItemSet.OverrideSignedByUsername,
				"OverrideSignedByUsername",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Override Signed By Username",
				"When set, this name will be used instead of the currently logged in user name displayed on the visible signature.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				null);

			AssertEquals("DataType.MaxLength", 57, ((StringRegistryDataType)ItemSet.OverrideSignedByUsername.DataType).MaxLength);
			AssertEquals("DataType.MinLength", 2, ((StringRegistryDataType)ItemSet.OverrideSignedByUsername.DataType).MinLength);
		}

		public void TestVisibleSignatureFontAndBackColor()
		{
			TestGenericRegistryItem(
				ItemSet.VisibleSignatureFontAndBackColor,
				"VisibleSignatureFontAndBackColor",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Visible Signature Font And Background Colors",
				"Overriding this setting will change the default font color and background color on the visible digital signature on signed documents. The background color is used if there is no background image set in the 'Signature Background Image' registry setting.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				new ColorPairSelector());
		}

		public void TestSigningServiceProviderBatching_HostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			TestRegistryItem(
				ItemSet.SigningServiceProviderBatching,
				"SigningServiceProviderBatching",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Signing Service Provider Batching",
				"This registry defines the batch size of document hashes to be included in a single signing transaction for countries and remote signing service providers that support batching. Set this registry to a number greater than zero to override the default batch number.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				0, 0, 10);
		}

		public void TestSigningServiceProviderBatching_NotHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("");
			TestRegistryItem(
				ItemSet.SigningServiceProviderBatching,
				"SigningServiceProviderBatching",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Signing Service Provider Batching",
				"This registry defines the batch size of document hashes to be included in a single signing transaction for countries and remote signing service providers that support batching. Set this registry to a number greater than zero to override the default batch number.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				0, 0, 10);
		}

		public void TestCloudSigningServiceProviderAPIEndpoint_Production()
		{
			AssertCloudSigningServiceProviderAPIEndpoint(true);
		}

		public void TestCloudSigningServiceProviderAPIEndpoint_NonProduction()
		{
			AssertCloudSigningServiceProviderAPIEndpoint(false);
		}

		void AssertCloudSigningServiceProviderAPIEndpoint(bool isProduction)
		{
			var licenceType = isProduction ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProduction, Env.Instance.IsProductionSystem);

			var registry = ItemSet.CloudSigningServiceProviderAPIEndpoint;

			AssertEquals("CloudSigningServiceProviderAPIEndpoint", registry.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, registry.Category);
			AssertEquals("Cloud Signing Service Provider API Endpoint", registry.Caption);
			AssertEquals("Determines the URL used for the external document signing service provider API.", registry.Hint);
			AssertEquals(RegistryStorageFlags.Company, registry.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registry.Options);

			var expectedCountries = new Dictionary<string, string[]>();
			expectedCountries.Add(CountryCodes.India, new string[] { "https://remotesigning-prod.emudhra.com/api/signdoc", "https://staging-rsds.emudhra.com/api/signdoc" });
			expectedCountries.Add(CountryCodes.Portugal, new string[] { "https://qscd.digitalsign.pt", "https://qscd-dev.digitalsign.pt" });

			var currentCompany = Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.Equal, Env.CurrentCompany.PK));
			var countries = Factory.Load<IRefCountry>(new ZQuery());
			foreach (var country in countries)
			{
				currentCompany.SetCountry(country.RN_Code);

				if (expectedCountries.ContainsKey(country.RN_Code))
				{
					var expectedUrl = isProduction ? expectedCountries[country.RN_Code][0] : expectedCountries[country.RN_Code][1];

					AssertEquals(expectedUrl, registry.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

					var branch = Factory.LoadTop1<IGlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, currentCompany.PK));
					AssertEquals(expectedUrl, registry.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty));
				}
				else
				{
					AssertEquals(string.Empty, registry.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				}
			}
		}

		[TestDate(2026, 01, 05, 10, 00, 00)]
		public void TestEnableDocumentSigningService_Production()
		{
			AssertEnableDocumentSigningService(true);
		}

		public void TestEnableDocumentSigningService_NonProduction()
		{
			AssertEnableDocumentSigningService(false);
		}

		public void AssertEnableDocumentSigningService(bool isProduction)
		{
			var licenceType = isProduction ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProduction, Env.Instance.IsProductionSystem);

			var registry = ItemSet.EnableDocumentSigningService;

			AssertEquals("EnableDocumentSigningService", registry.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, registry.Category);
			AssertEquals("Enable Document Signing Service", registry.Caption);
			AssertEquals("When this registry is enabled, the document signing service tabs, controls, and fields will be made available to the users of the company that has been enabled. Otherwise, the functionality is only available to CW1 Support users.", registry.Hint);
			AssertEquals(RegistryStorageFlags.Company, registry.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registry.Options);

			var expectedCountries = new Dictionary<string, ZDate[]>();
			expectedCountries.Add(CountryCodes.India, new ZDate[] { new ZDate(2021, 01, 01), new ZDate(2020, 11, 01) });
			expectedCountries.Add(CountryCodes.Portugal, new ZDate[] { new ZDate(2026, 01, 01), ZDate.Empty });

			var today = ZDateTime.Today;
			var currentCompany = Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.Equal, Env.CurrentCompany.PK));

			var countries = Factory.Load<IRefCountry>(new ZQuery());
			foreach (var country in countries)
			{
				currentCompany.SetCountry(country.RN_Code);

				if (expectedCountries.ContainsKey(country.RN_Code))
				{
					var expectedDate = isProduction ? expectedCountries[country.RN_Code][0] : expectedCountries[country.RN_Code][1];
					var expectedValue = expectedDate != ZDate.Empty && today >= expectedDate.ToDateTime();

					AssertEquals(expectedValue, registry.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

					var branch = Factory.LoadTop1<IGlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, currentCompany.PK));
					AssertEquals(expectedValue, registry.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty));
				}
				else
				{
					AssertEquals(false, registry.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				}
			}
		}

		public void TestDocumentSigningServiceCredentials()
		{
			AssertEquals("SigningServiceProviderAPICredentials", ItemSet.DocumentSigningServiceCredentials.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, ItemSet.DocumentSigningServiceCredentials.Category);
			AssertEquals("Signing Service Provider API Credentials", ItemSet.DocumentSigningServiceCredentials.Caption);
			AssertEquals("This registry defines the credentials required for accessing the document signing service provider resources.", ItemSet.DocumentSigningServiceCredentials.Hint);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.DocumentSigningServiceCredentials.Storage);
		}

		public void TestDocumentSigningServicePartnerCredentials_ProductionSystem() => AssertDocumentSigningServicePartnerCredentials(true);

		public void TestDocumentSigningServicePartnerCredentials_NonProductionSystem() => AssertDocumentSigningServicePartnerCredentials(false);

		void AssertDocumentSigningServicePartnerCredentials(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var itemSet = GetNewItemSet();
			AssertEquals("DocumentSigningServicePartnerCredentials", itemSet.DocumentSigningServicePartnerCredentials.Name);
			AssertEquals(DocumentsDataRegistry.Categories.Documents_DocumentSigningService, itemSet.DocumentSigningServicePartnerCredentials.Category);
			AssertEquals("Signing Service Provider Partner Credentials", itemSet.DocumentSigningServicePartnerCredentials.Caption);
			AssertEquals("Used to store the Cloud Signing Service Provider's Partner ID.", itemSet.DocumentSigningServicePartnerCredentials.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.DocumentSigningServicePartnerCredentials.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.DocumentSigningServicePartnerCredentials.Options);

			var defaultValue = itemSet.DocumentSigningServicePartnerCredentials.DefaultValue;
			if (isProductionSystem)
			{
				AssertEquals("9815455884", defaultValue.PartnerID);
				AssertEquals("0b9ec0fa756dcf02d6512f08459a0be5612e820d8847af25f24f8b4711c771ab", defaultValue.PartnerAccessKey);
			}
			else
			{
				AssertEquals("6340408123", defaultValue.PartnerID);
				AssertEquals("96ea58b42b7ae1f2796f0e1344ee159d8eb793a21c17f9203b496d77a1fe3129", defaultValue.PartnerAccessKey);
			}
		}

		public void TestDocumentSigningServiceAuthenticatorCredentials_ProductionSystem() => AssertDocumentSigningServiceAuthenticatorCredentials(true);

		public void TestDocumentSigningServiceAuthenticatorCredentials_NonProductionSystem() => AssertDocumentSigningServiceAuthenticatorCredentials(false);

		void AssertDocumentSigningServiceAuthenticatorCredentials(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var item = GetNewItemSet().DocumentSigningServicePartnerCredentialsAccessToken;
			AssertNotNull(item);
			AssertEquals("DocumentSigningServicePartnerCredentialsAccessToken", item.Name);
			AssertEquals("Documents/Document Signing Service/Portugal (PT)", item.Category);
			AssertEquals("Partner Credentials and Access Token", item.Caption);
			AssertEquals("Used to store the Cloud Signing Service Provider's credentials.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);

			var defaultValue = item.DefaultValue;
			if (isProductionSystem)
			{
				//TODO: insert *real* Production credentials
				AssertEquals("wisetechglobal", defaultValue.ClientID);
				AssertEquals("42lpvdn6gzzcg8vquvtvcwckya", defaultValue.AccessKey);
			}
			else
			{
				AssertEquals("wisetechglobal", defaultValue.ClientID);
				AssertEquals("42lpvdn6gzzcg8vquvtvcwckya", defaultValue.AccessKey);
			}
		}

		public void TestSignatureBackgroundImage()
		{
			const int maxHeight = 100;
			const int minHeight = 100;
			const int maxWidth = 700;
			const int minWidth = 150;

			TestGenericRegistryItem(
				ItemSet.SignatureBackgroundImage,
				"SignatureBackgroundImage",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService,
				"Signature Background Image",
				$"The image used as background of the visible signature. Minimum size {minWidth}x{minHeight} (Width x Height), maximum size {maxWidth}x{maxHeight} (Width x Height).",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				null);

			AssertEquals("DataType.MinWidth", minWidth, ((ImageRegistryDataType)ItemSet.SignatureBackgroundImage.DataType).MinWidth);
			AssertEquals("DataType.MinHeight", minHeight, ((ImageRegistryDataType)ItemSet.SignatureBackgroundImage.DataType).MinHeight);
			AssertEquals("DataType.MaxWidth", maxWidth, ((ImageRegistryDataType)ItemSet.SignatureBackgroundImage.DataType).MaxWidth);
			AssertEquals("DataType.MaxHeight", maxHeight, ((ImageRegistryDataType)ItemSet.SignatureBackgroundImage.DataType).MaxHeight);
		}

		public void TestSignatureImagePositioningAnchor()
		{
			TestRegistryItem(
				ItemSet.SignatureImagePositioningAnchor,
				"SignatureImagePositioningAnchor",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService_SignatureImagePositioning,
				"Anchor",
				"The corner to use as a reference for positioning the signature image.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				DocumentsDataRegistry.Instance.SignatureImagePositioningAnchorsPairList,
				DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft);
		}

		public void TestSignatureImagePositioningHorizontalMargin()
		{
			TestRegistryItem(
				ItemSet.SignatureImagePositioningHorizontalMargin,
				"SignatureImagePositioningHorizontalMargin",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService_SignatureImagePositioning,
				"Horizontal Margin",
				"Specifies the horizontal positioning (in millimeters) of the signature image relative to the anchor point. If you need to set this to a value over 200, consider changing the anchor point.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				3, 0, 200);
		}

		public void TestSignatureImagePositioningVerticalMargin()
		{
			TestRegistryItem(
				ItemSet.SignatureImagePositioningVerticalMargin,
				"SignatureImagePositioningVerticalMargin",
				DocumentsDataRegistry.Categories.Documents_DocumentSigningService_SignatureImagePositioning,
				"Vertical Margin",
				"Specifies the vertical positioning (in millimeters) of the signature image relative to the anchor point. If you need to set this to a value over 200, consider changing the anchor point.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				3, 0, 200);
		}

		public void TestEnableSpecificPageRangesPrintingOption()
		{
			TestRegistryItem(
				ItemSet.EnableSpecificPageRangesPrintingOption,
				"EnableSpecificPageRangesPrintingOption",
				DocumentsDataRegistry.Categories.Documents,
				"Enable Specific Page Ranges Printing Option",
				"Enables the Specific Page Ranges Printing Option.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestDocumentImages()
		{
			TestGenericRegistryItem(
				ItemSet.DocumentImages,
				"DocumentImages",
				DocumentsDataRegistry.Categories.Documents,
				"Document Images",
				"Image files that are used with document and report macros.",
				RegistryStorageFlags.All);
		}

		public void TestLogDocumentRenderer()
		{
			TestGenericRegistryItem(
				ItemSet.LogDocumentRenderer,
				"LogDocumentRenderer",
				DocumentsDataRegistry.Categories.Documents,
				"Log Document Renderer",
				"Enables the logging of the rendering of documents.",
				RegistryStorageFlags.All,
				RegistryOptions.IsOnlyForDevelopers);
		}

		public void TestAirFreightIncludeTransportProviderOnQuotation()
		{
			string hint = "This will determine whether the Transport Provider (Shipping or Air line) will be mentioned on the Quotation document.";
			TestRegistryItem(
				ItemSet.AirFreightIncludeTransportProviderOnQuotation,
				"AirFreightIncludeTransportProviderOnQuotation",
				DocumentsDataRegistry.Categories.Documents_QuotationsandRates_IncludeTransportProvideronQuotationDocument,
				"Air Freight",
				hint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestSeaFreightIncludeTransportProviderOnQuotation()
		{
			string hint = "This will determine whether the Transport Provider (Shipping or Air line) will be mentioned on the Quotation document.";
			TestRegistryItem(
				ItemSet.SeaFreightIncludeTransportProviderOnQuotation,
				"SeaFreightIncludeTransportProviderOnQuotation",
				DocumentsDataRegistry.Categories.Documents_QuotationsandRates_IncludeTransportProvideronQuotationDocument,
				"Sea Freight",
				hint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestWebPrintDocumentPackMaxSize()
		{
			TestRegistryItem(
				ItemSet.WebPrintDocumentPackMaxSize,
				"WebPrintDocumentPackMaxSize",
				DocumentsDataRegistry.Categories.Documents,
				"Remote Printing Doc Pack Maximum Batch Size",
				@"Maximum batch size, in Megabytes, for Doc Pack print jobs to be sent to the Remote Printing print server.

By default, Doc Packs are sent to the print server in one batch.  When there are many documents in the Doc Pack, the size of the batch can be too large for the network to handle and this can disrupt the delivery of the print jobs.  This setting will split larger Doc Packs into several batches when sending to the print server.

The default value is 0 for no limit on the batch size (i.e. doing it in a single batch) and the maximum batch size is 1000 Megabytes.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				0, 0, 1000);
		}

		public void TestWebPrintAllowDirectPrintPrintPushNotification()
		{
			TestRegistryItem(
				ItemSet.WebPrintAllowDirectPrintPrintPushNotification,
				"WebPrintAllowDirectPrintPrintPushNotification",
				DocumentsDataRegistry.Categories.Documents,
				"Allow Pushing Print Jobs Directly to Print Server",
				@"When enabled, print jobs can be pushed immediately to the Print Server via the WebPrint Client. Additionally, the Enable Print Nudging option in the WebPrint Client Configuration needs to be enabled.

Print jobs that are under 30 Kilobytes will be pushed directly to the Print Server. Larger print jobs will trigger a nudge notification to the Print Server to download the print job.

Note: Pushing print jobs directly to the Print Server may cause print jobs to be printed out of order. If you have documents that need to be printed in sequential order, do NOT enable this option.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestWebPrintNudge()
		{
			var item = ItemSet.WebPrintNudge;
			AssertEquals("Name", "WebPrintNudge", item.Name);
			AssertEquals("Category", DocumentsDataRegistry.Categories.Documents, item.Category);
			AssertContains("Caption", "Remote Printing Nudge", item.Caption);

			var hintText = @"This setting controls how Document Engine sends nudge messages to Remote Printing web service.

When option 'IP address' (default) is selected, nudge messages will be sent as HTTP request directly to the web service IP address.

When option 'Web service URL address' is selected, nudge messages will be sent as HTTP request to the web service URL address.

NOTE:
If direct connection to the web service IP address is not possible, this setting will be automatically changed to 'Web service URL address'.
It will switch back to 'IP address' after specified number of hours.";

			AssertContains("Hint", hintText, item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForController, item.Options);
			var editorInfo = item.EditorInfo as WebPrintNudgeEditorInfo;
			AssertNotNull(editorInfo);

			var defaultValue = item.DefaultValue;
			AssertEquals("EnableIPAddress", true, defaultValue.EnableIPAddress);
			AssertEquals("EnableUrlAddressValidHours", 24, defaultValue.SwtichBackToIPAddressIntervalInHours);
		}

		public void TestWebPrintNudgeSuspending()
		{
			var item = ItemSet.WebPrintNudgeSuspending;
			AssertEquals("Name", "WebPrintNudgeSuspending", item.Name);
			AssertEquals("Category", DocumentsDataRegistry.Categories.Documents, item.Category);
			AssertContains("Caption", "Remote Printing Nudge Suspending", item.Caption);

			var hintText = @"This setting controls how to suspend remote printing nudge request.

Minutes E.g:
Maximum error count: 3, Interval minutes: 15, Suspend minutes: 10
If there are more than 3 errors within 15 minutes, Remote Printing Nudge will be suspended for 10 minutes.

Hours E.g:
Maximum error count: 10, Interval hours: 1, Suspend hours: 1
If there are more than 10 errors within 1 hour, Remote Printing Nudge will be suspended for 1 hour.";

			AssertContains("Hint", hintText, item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForController, item.Options);
			var editorInfo = item.EditorInfo as WebPrintNudgeSuspendingEditorInfo;
			AssertNotNull(editorInfo);

			var defaultValue = item.DefaultValue;
			AssertEquals("MaxErrorsInMinutes", 3, defaultValue.MaxErrorsInMinutes);
			AssertEquals("IntervalMinutes", 15, defaultValue.IntervalMinutes);
			AssertEquals("SuspendMinutes", 10, defaultValue.SuspendMinutes);
			AssertEquals("MaxErrorsInHours", 10, defaultValue.MaxErrorsInHours);
			AssertEquals("IntervalHours", 1, defaultValue.IntervalHours);
			AssertEquals("SuspendHours", 1, defaultValue.SuspendHours);
		}

		public void TestWebPrintForceToUseHTTPSForWebPrintRequests()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("IsHostedWithCargowise", true, EnvProxy.IsHostedWithCargowise);

			TestGenericRegistryItem(
				ItemSet.WebPrintForceToUseHTTPSForWebPrintRequests,
				"WebPrintForceToUseHTTPSForWebPrintRequests",
				DocumentsDataRegistry.Categories.Documents,
				"Force to use HTTPS for WebPrint requests",
				"When enabled, Remote Printing Web Application Server will receive nudging HTTPS requests, please disable it if something goes wrong when enabled.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				true);
		}

		public void TestQuoteOpeningText()
		{
			TestMultilingualRegistryItem(
				ItemSet.QuoteOpeningText,
				"QuoteOpeningText",
				"Documents/Quotations and Rates",
				"Quote Entry Opening Text",
				"The default quote page opening text.",
				RegistryStorageFlags.All,
				TextEditorType.Memo,
				"");
		}

		public void TestQuoteClosingText()
		{
			TestMultilingualRegistryItem(
				ItemSet.QuoteClosingText,
				"QuoteClosingText",
				"Documents/Quotations and Rates",
				"Quote Entry Closing Text",
				"The default quote page closing text.",
				RegistryStorageFlags.All,
				TextEditorType.Memo,
				"");
		}

		public void TestAddressPosition()
		{
			string hint = "Set the position of the address on DocBuilder documents to the left or right.";
			TestRegistryItem(
				ItemSet.AddressPosition,
				"AddressPosition",
				DocumentsDataRegistry.Categories.Documents_DocBuilder,
				"Address Position",
				hint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new AddressPositionList(),
				string.Empty);
		}

		public void TestSuppressPrintingOfFlightDate()
		{
			TestRegistryItem(
				ItemSet.SuppressPrintingOfFlightDate,
				"SuppressPrintingOfFlightDate",
				DocumentsDataRegistry.Categories.Documents,
				"Suppress Export Flight Details",
				"Use this registry to configure the details to be suppressed on export documents (those issued to the Consignor) until at least one of the Consol ETD or Shipment Actual Pickup dates are in the past.\r\n" +
"For the U.S., flight details configured in this Registry will be suppressed for Consolidations with passenger flights whose ATD (Actual Time of Departure) of the final routing leg is not in the past.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				"Suppress",
				false,
				Enum.GetNames(typeof(SuppressFields)).Length,
				RegistrySuppressionHelper.GetDefaultFields(Env.CurrentBranch.PK).ToArray());
		}

		public void TestEDocsToBeAutoAttachedForDelivery()
		{
			string hint = "Select which eDocs Documents of a given Document Type to automatically attach when a Document Type is configured to be attached on delivery of a given Document. NB: This only has an effect when there are multiple Documents with the same Document Type selected in the one eDocs module.";
			TestRegistryItem(
				ItemSet.EDocsToBeAutoAttachedForDelivery,
				"EDocsToBeAutoAttachedForDelivery",
				DocumentsDataRegistry.Categories.Documents,
				"eDocs Documents Auto-Attached by Type",
				hint,
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				ItemSet.DeliverEDocsOptions,
				DeliverEDocsOptionList.Codes.SendMostRecentSystemGeneratedAndAllManuallyAdded);
		}

		public void TestExcelDefaultRenderingFormat()
		{
			string hint = @"Select the Excel rendering format that the Document Engine will use by default to generate reports/documents. This is the format you will get when you preview any document TIF/PDF documents and decided to open them in Excel.

Note: If you choose Excel 97-2003 rendering by default and you deliver a document/report as PDF/TIF that hits the 65,535 rows limit, Document Engine will automatically use Excel 2007 rendering instead.";

			TestRegistryItem(
				ItemSet.ExcelDefaultRenderingFormat,
				"ExcelDefaultRenderingFormat",
				DocumentsDataRegistry.Categories.Documents,
				"Excel default rendering format",
				hint,
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				ItemSet.ExcelFileFormatOptions,
				ExcelFileFormatOptionList.Codes.XLSX);
		}

		public void TestAutomaticallySwitchToXLSXIfRequired()
		{
			string hint = @"This is to let Document Engine automatically switch to Excel 2007 XLSX file format if a document/report generated as XLS contains too many rows for Excel 2003.
This will overcome rows limitation (65,535) from Excel 2003.
If this setting is disabled and a document delivered as XLS exceeds 65,535 rows, users will be prompted if they want to use XLSX format instead.";

			TestRegistryItem(
				ItemSet.AutomaticallySwitchToXLSXIfRequired,
				"AutomaticallySwitchToXLSXIfRequired",
				DocumentsDataRegistry.Categories.Documents,
				"Automatically switch to XLSX file format if required",
				hint,
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				true);
		}

		public void TestDeliverDocumentsToPrintersInPdfFormat()
		{
			var hint = @"When this registry item is turned on, the system will use PDF as the default format for print jobs rather than Excel format.
Combined with the ""Embed Fonts In PDF"" registry setting, this will avoid having to install fonts on the WebPrint Client servers.";

			TestRegistryItem(
				ItemSet.DeliverDocumentsToPrintersInPdfFormat,
				"DeliverDocumentsToPrintersInPdfFormat",
				RawDataRegistry.Categories.Documents,
				"Deliver Documents to printers in PDF format",
				hint,
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				false);
		}

		public void TestDeliveryMethodWhenNoContactSpecified()
		{
			string hint = "When no default contact can be found for a document delivery, the system will try to send to the main office address using the delivery method specified in this option.";
			TestRegistryItem(
				ItemSet.DeliveryMethodWhenNoContactSpecified,
				"DeliveryMethodWhenNoContactSpecified",
				DocumentsDataRegistry.Categories.Documents,
				"Delivery Method When No Contact Specified",
				hint,
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				ItemSet.DeliveryMethodOptions,
				DeliveryMethodOptionList.Codes.EmailFaxThenPrint);
		}

		public void TestRedirectedDocumentAddressFormatting()
		{
			string hint = Res.GetString("0c760187-cb56-4db8-855e-428ac45c0712", "When sending a Document that has an 'Official' recipient (e.g.: AR Invoice), the default behavior is to print the 'Official' contact and no other at the top of the Document. This setting allows you to override this behavior, and will affect the generation of the recipient name/address on all documents that have an 'Official' recipient.");
			TestRegistryItem(ItemSet.RedirectedDocumentAddressFormatting, "RedirectedDocumentAddressFormatting", DocumentsDataRegistry.Categories.Documents_AddressFormatting, "'Redirect To' Address Handling", hint, RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.Default, DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormattingOptions, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
		}

		public void TestAttentionPrefixForAddressContactNameLineText()
		{
			var value = ItemSet.AttentionPrefixForAddressContactNameLineText.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as MultilingualString;
			AssertNotNull("AttentionPrefixForAddressContactNameLineText", value);
			AssertEquals("ATTENTION: Example Name", value.ToString());
			using (var grmMockResourceStrings = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				grmMockResourceStrings.Put("27d8188d-3f6a-47a0-8b8b-e1b3050b9a95", new ResourceStringData("27d8188d-3f6a-47a0-8b8b-e1b3050b9a95", string.Empty, string.Empty, "{0}: Attention", string.Empty));
				grmMockResourceStrings.Put("9d2091f5-58e2-4578-9e97-21bb780df196", new ResourceStringData("9d2091f5-58e2-4578-9e97-21bb780df196", string.Empty, string.Empty, "Name Example", string.Empty));

				AssertEquals("ATTENTION: Example Name", value.ToString());
				AssertEquals("ATTENTION: Example Name", value.ToString(Res.DefaultLanguage));
				AssertEquals("Name Example: Attention", value.ToString(Core.SharedConstants.Languages.German));
			}
		}

		public void TestRedirectToPrefixForAddressContactNameLineText()
		{
			var value = ItemSet.RedirectToPrefixForAddressContactNameLineText.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as MultilingualString;
			AssertNotNull("RedirectToPrefixForAddressContactNameLineText", value);
			AssertEquals("REDIRECT TO: Example Name", value.ToString());
			using (var grmMockResourceStrings = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				grmMockResourceStrings.Put("d70ea7db-06e1-4749-8879-44e63402096d", new ResourceStringData("d70ea7db-06e1-4749-8879-44e63402096d", string.Empty, string.Empty, "{0}: Redirect To", string.Empty));
				grmMockResourceStrings.Put("9d2091f5-58e2-4578-9e97-21bb780df196", new ResourceStringData("9d2091f5-58e2-4578-9e97-21bb780df196", string.Empty, string.Empty, "Name Example", string.Empty));

				AssertEquals("REDIRECT TO: Example Name", value.ToString());
				AssertEquals("REDIRECT TO: Example Name", value.ToString(Res.DefaultLanguage));
				AssertEquals("Name Example: Redirect To", value.ToString(Core.SharedConstants.Languages.German));
			}
		}

		public void TestDetentionAdviceOpeningText()
		{
			TestGenericRegistryItem(
				ItemSet.DetentionAdviceOpeningText,
				"DetentionAdviceOpeningText",
				"Documents/Liner & Agency/Detention Advice",
				"Opening Text",
				"The opening text for the Detention Advice document.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestDetentionAdviceClosingText()
		{
			TestGenericRegistryItem(
				ItemSet.DetentionAdviceClosingText,
				"DetentionAdviceClosingText",
				"Documents/Liner & Agency/Detention Advice",
				"Closing Text",
				"The closing text for the Detention Advice document.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestAgencyBookingConfirmationOpeningText()
		{
			TestGenericRegistryItem(
				ItemSet.AgencyBookingConfirmationOpeningText,
				"AgencyBookingConfirmationOpeningText",
				"Documents/Liner & Agency/Booking Confirmation",
				"Opening Text",
				"The opening text for the Booking Confirmation document.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestAgencyBookingConfirmationClosingText()
		{
			TestGenericRegistryItem(
				ItemSet.AgencyBookingConfirmationClosingText,
				"AgencyBookingConfirmationClosingText",
				"Documents/Liner & Agency/Booking Confirmation",
				"Closing Text",
				"The closing text for the Booking Confirmation document.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				string.Empty);
		}

		#region DangerousGoodsStatement

		public void TestDangerousGoodsStatement()
		{
			string defaultValue = "This is to certify that the above named materials " +
			"are properly classified, described, packaged, marked and labeled, " +
			"and are in proper condition for transport according " +
			"to applicable domestic and international regulations.";
			AssertEquals(defaultValue, ItemSet.DangerousGoodsStatement.Value);

			ItemSet.DangerousGoodsStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST1");
			AssertEquals("TEST1", ItemSet.DangerousGoodsStatement.Value);

			ItemSet.DangerousGoodsStatement.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST2");
			AssertEquals("TEST2", ItemSet.DangerousGoodsStatement.Value);
		}

		#endregion

		#region Airline Security Statement

		#region Known Shipper
		public void TestAirlineSecurityStatementKnownShipper()
		{
			string defaultValue = "Company ________________________________ is in compliance with its TSA approved security program and all applicable security directives. "
				+ "Our number assigned by TSA is ________________________________. All cargo tendered in conjunction with this certification was either 1) accepted from a known "
				+ "shipper or an unknown shipper in accordance with TSA requirements specified in the Indirect Air Carrier Standard Security Program or 2) accepted under transfer "
				+ "from another aircraft operator, foreign air carrier, or IAC operating under a TSA-approved or accepted security program. The individual whose name appears below "
				+ "certifies that he or she is an employee or authorized representative of _______________________________________ and understands that any fraudulent or false "
				+ "statement made in connection with this certification may subject this individual and _______________________ to both (1) civil penalties under 49 CFR 1540.103(b) "
				+ "and (2) fines and/or imprisonment of not more than 5 years under 18 U.S.C. 1001.";
			AssertEquals(defaultValue, ItemSet.AirlineSecurityStatementKnownShipper.Value);

			ItemSet.AirlineSecurityStatementKnownShipper.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST1");
			AssertEquals("TEST1", ItemSet.AirlineSecurityStatementKnownShipper.Value);

			ItemSet.AirlineSecurityStatementKnownShipper.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST2");
			AssertEquals("TEST2", ItemSet.AirlineSecurityStatementKnownShipper.Value);

			ItemSet.AirlineSecurityStatementKnownShipper.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "TEST3");
			AssertEquals("TEST3", ItemSet.AirlineSecurityStatementKnownShipper.Value);
		}

		#endregion

		#region Unknown Shipper

		public void TestAirlineSecurityStatementUnknownShipper()
		{
			string defaultValue = "Company ________________________________ is in compliance with its TSA approved security program and all applicable security directives. "
				+ "Our number assigned by TSA is ________________________________. This shipment contains cargo originating from an unknown shipper not exempted by TSA. "
				+ "This shipment must be transported ONLY on ALL-CARGO AIRCRAFT. The individual whose name appears below certifies that he or she is an employee or Authorized "
				+ "Representative of _______________________________________________ and understands that any fraudulent or false statement made in connection with this "
				+ "certification may subject this individual and _______________________________________________ to both civil penalties under 49 CFR Part 1540.103(b) and fines "
				+ "and/or imprisonment of not more than 5 years under 18 U.S.C. 1001.";
			AssertEquals(defaultValue, ItemSet.AirlineSecurityStatementUnknownShipper.Value);

			ItemSet.AirlineSecurityStatementUnknownShipper.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST1");
			AssertEquals("TEST1", ItemSet.AirlineSecurityStatementUnknownShipper.Value);

			ItemSet.AirlineSecurityStatementUnknownShipper.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST2");
			AssertEquals("TEST2", ItemSet.AirlineSecurityStatementUnknownShipper.Value);

			ItemSet.AirlineSecurityStatementUnknownShipper.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "TEST3");
			AssertEquals("TEST3", ItemSet.AirlineSecurityStatementUnknownShipper.Value);
		}

		#endregion

		#region AWB Security Declaration

		public void TestAWBSecurityDeclarationStatement()
		{
			string defaultValue = "I, <LoginFullName>, an authorized representative of <CompanyName>, declare that the cargo on AWB <MasterBill> has been secured for carriage by air as a result of the procedure indicated above. I understand that a false declaration may lead to legal action being taken.";
			AssertEquals(defaultValue, ItemSet.AWBSecurityDeclarationStatement.Value);

			ItemSet.AWBSecurityDeclarationStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some new value bla bla");
			AssertEquals("Some new value bla bla", ItemSet.AWBSecurityDeclarationStatement.Value);
		}

		#endregion

		#endregion

		#region Airline Security Statement

		public void TestDomesticHAWBTerms()
		{
			MultilingualString @default = ResString.GetMultilingualString("00938b0a-d372-4a8e-9c8b-1d0108054946", "It is agreed that the goods described herein are accepted in " +
			"apparent good order and condition (except as noted) for carriage SUBJECT TO CONDITIONS OF CONTRACT ON " +
			"THE REVERSE SIDE HEREOF. ALL GOODS MAY BE CARRIED BY ANY OTHER MEANS INCLUDING ROAD OR ANY OTHER " +
			"CARRIER UNLESS SPECIFIC CONTRARY INSTRUCTIONS ARE GIVEN HEREON BY THE SHIPPER, AND THE SHIPPER AGREES " +
			"THAT THE SHIPMENT MAY BE CARRIED VIA INTERMEDIATE STOPPING PLACES WHICH THE CARRIER DEEMS APPROPRIATE. " +
			"THE SHIPPERS ATTENTION IS DRAWN TO THE NOTICE CONCERNING CARRIER'S LIMITATIONS OF LIABILITY." +
			"\r\nShipper may increase such limitation of liability by declaring a higher value for carriage and " +
			"paying supplemental charge if required." +
			"\r\n* The Terms and Conditions as noted on the reverse side of this Transport Document are not applicable " +
			"for OCEAN shipments. These shipments will be subject to the Terms and Conditions of the appointed carrier, " +
			"including Limitation of Liability.");

			AssertEquals(@default, ItemSet.DomesticHAWBTerms.Value);

			ItemSet.DomesticHAWBTerms.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST1");
			AssertEquals("TEST1", ItemSet.DomesticHAWBTerms.Value);

			ItemSet.DomesticHAWBTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST2");
			AssertEquals("TEST2", ItemSet.DomesticHAWBTerms.Value);

			ItemSet.DomesticHAWBTerms.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "TEST3");
			AssertEquals("TEST3", ItemSet.DomesticHAWBTerms.Value);
		}

		#endregion

		#region TestDeliveryOrderTermsAndConditions

		public void TestDeliveryOrderTermsAndConditions()
		{
			BusinessObject principal = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			principal[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject companyData = (BusinessObject)principal["CompanyData"];
			companyData[OrgCompanyDataSchema.Constants.OB_CRIsShipsAgencyPrincipal] = true;
			Factory.Save();

			DeliveryOrderCollection collection = new DeliveryOrderCollection();
			DeliveryOrder element = collection.AddNew();

			element.PrincipalPK = principal.PK;
			element.PrintParameter = "PDO";
			element.Image = new Bitmap(10, 10);

			ItemSet.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			DeliveryOrderCollection obtainedCollection = ItemSet.DeliveryOrderTermsAndConditions.Value;

			AssertEquals(1, obtainedCollection.Count);
			AssertEquals(principal.PK, obtainedCollection[0].PrincipalPK);
		}

		#endregion

		public void TestConsoleRateConfirmationClosingText()
		{
			AssertEquals("DefaultValue", string.Empty, ItemSet.ConsoleRateConfirmationClosingText.DefaultValue);
			ItemSet.ConsoleRateConfirmationClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Value");
			AssertEquals("Value", "New Value", ItemSet.ConsoleRateConfirmationClosingText.Value);
		}

		public void TestConsoleRateConfirmationOpeningText()
		{
			AssertEquals("DefaultValue", string.Empty, ItemSet.ConsoleRateConfirmationOpeningText.DefaultValue);
			ItemSet.ConsoleRateConfirmationOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Value");
			AssertEquals("Value", "New Value", ItemSet.ConsoleRateConfirmationOpeningText.Value);
		}

		public void TestPODRegistry()
		{
			TestRegistryItem(ItemSet.IncludeEstimatedMilesonesOnPODDocument, "IncludeEstimatedMilesonesOnPODDocument", "Documents/Forwarding/Shipment/Proof of Delivery", "Include Estimated Milestones on POD Document", "Show Estimated Milestones on POD Document if there is no Actual Date", RegistryStorageFlags.Company, false);
			TestRegistryItem(ItemSet.ShowMilestonesOnPODDocument, "ShowMilestonesOnPODDocument", "Documents/Forwarding/Shipment/Proof of Delivery", "Show Milestones On POD Document", "Display Milestones on POD Document", RegistryStorageFlags.Company, false);
		}

		public void TestImportDeliveryOrderDetentionChargesText()
		{
			AssertEquals("", ItemSet.ImportDeliveryOrderDetentionChargesText.Value);

			ItemSet.ImportDeliveryOrderDetentionChargesText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "random charge text");
			AssertEquals("random charge text", ItemSet.ImportDeliveryOrderDetentionChargesText.Value);
		}

		public void TestImportDeliveryOrderDeliveryClerkNote()
		{
			AssertEquals("", ItemSet.ImportDeliveryOrderDeliveryClerkNote.Value);

			ItemSet.ImportDeliveryOrderDeliveryClerkNote.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "random comment");
			AssertEquals("random comment", ItemSet.ImportDeliveryOrderDeliveryClerkNote.Value);
		}

		public void TestLetterOfIndemnity()
		{
			Assert("OpeningText.CountryFilterPK", ItemSet.LetterOfIndemnityOpeningText.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Singapore));
			Assert("ClosingText.CountryFilterPK", ItemSet.LetterOfIndemnityClosingText.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Singapore));

			AssertEquals("OpeningText.DefaultValue", "Please alter the markings on the above Delivery Order in respect of the under mentioned cargo.", ItemSet.LetterOfIndemnityOpeningText.DefaultValue);
			AssertEquals("ClosingText.DefaultValue", "We hereby undertake and agree to indemnify you against all consequences and/or liabilities of any kind whatsoever directly or indirectly arising from or relating to the said delivery, and immediately on demand on all payment made by you in respect of such consequences and/or liabilities including costs as between solicitor and client and all any sues demanded by you for the defense of any proceeding brought against you by reason of the delivery aforesaid.", ItemSet.LetterOfIndemnityClosingText.DefaultValue);

			ItemSet.LetterOfIndemnityOpeningText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "O");
			ItemSet.LetterOfIndemnityClosingText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "C");

			AssertEquals("OpeningText", "O", ItemSet.LetterOfIndemnity.OpeningText);
			AssertEquals("ClosingText", "C", ItemSet.LetterOfIndemnity.ClosingText);
		}

		public void TestContainerDetentionReminderOpeningText()
		{
			AssertEquals("DefaultValue", "We have no record of the return of the containers noted below, please note that Detention Charges will be applied for all containers not returned by their due date.", ItemSet.ContainerDetentionReminderOpeningText.DefaultValue);

			ItemSet.ContainerDetentionReminderOpeningText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "Baked not fried.");
			AssertEquals("Value", "Baked not fried.", ItemSet.ContainerDetentionReminderOpeningText.Value);
		}

		public void TestDisplayLogo()
		{
			TestRegistryItem(ItemSet.DisplayLogo, "DisplayLogo", "Documents/Forwarding/Shipment/Work Sheet", "Display Logo", "Display the logo on the Work sheet document that prints from Shipments and Customs Declarations.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, true);
		}

		public void TestFOBAndienung()
		{
			Assert("ClosingText.CountryFilterPK", ItemSet.FOBAndienungClosingText.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Germany));
			Assert("ClosingText.CountryFilterPK", ItemSet.FOBAndienungInsuranceCoveredText.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Germany));
			Assert("ClosingText.CountryFilterPK", ItemSet.FOBAndienungInsuranceNotCoveredText.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Germany));

			AssertEquals("ClosingText.DefaultValue", "", ItemSet.FOBAndienungClosingText.DefaultValue);

			ItemSet.FOBAndienungClosingText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "C");
			AssertEquals("ClosingText", "C", ItemSet.FOBAndienungClosingText.Value);

			ItemSet.FOBAndienungInsuranceCoveredText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "covered by");
			AssertEquals("covered by", "covered by", ItemSet.FOBAndienungInsuranceCoveredText.Value);

			ItemSet.FOBAndienungInsuranceNotCoveredText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "not covered by");
			AssertEquals("not covered by", "not covered by", ItemSet.FOBAndienungInsuranceNotCoveredText.Value);
		}

		public void TestVerpflichtungsscheinAccountNumber()
		{
			Assert("CountryFilterPK", ItemSet.VerpflichtungsscheinAccountNumber.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Germany));
			AssertEquals("DefaultValue", "", ItemSet.VerpflichtungsscheinAccountNumber.DefaultValue);

			ItemSet.VerpflichtungsscheinAccountNumber.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "account number");
			AssertEquals("New number", "account number", ItemSet.VerpflichtungsscheinAccountNumber.Value);
		}

		public void TestIcelandArrivalNoticeFooter()
		{
			Assert("Iceland CountryFilterPK", ItemSet.IcelandAirArrivalNoticeFooter.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Iceland));
			Assert("Iceland CountryFilterPK", ItemSet.IcelandSeaArrivalNoticeFooter.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Iceland));

			AssertEquals("IcelandAirArrivalNoticeFooter DefaultValue", "", ItemSet.IcelandAirArrivalNoticeFooter.DefaultValue);
			AssertEquals("IcelandSeaArrivalNoticeFooter DefaultValue", "", ItemSet.IcelandSeaArrivalNoticeFooter.DefaultValue);

			ItemSet.IcelandAirArrivalNoticeFooter.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "AIR");
			AssertEquals("IcelandAirArrivalNoticeFooter", "AIR", ItemSet.IcelandAirArrivalNoticeFooter.Value);

			ItemSet.IcelandSeaArrivalNoticeFooter.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "SEA");
			AssertEquals("IcelandSeaArrivalNoticeFooter", "SEA", ItemSet.IcelandSeaArrivalNoticeFooter.Value);
		}

		public void TestForwardersCertificateOfReceiptLogo()
		{
			AssertEquals("DefaultValue", null, ItemSet.ForwardersCertificateOfReceiptLogo.DefaultValue);

			using (Image systemLogo = new Bitmap(10, 10))
			using (Image companyLogo = new Bitmap(15, 15))
			using (Image branchLogo = new Bitmap(20, 20))
			{
				ItemSet.ForwardersCertificateOfReceiptLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLogo);
				Assert("Obtained value from registry was incorrect", Utilities.IsImageEqual(systemLogo, ItemSet.ForwardersCertificateOfReceiptLogo.Value));

				ItemSet.ForwardersCertificateOfReceiptLogo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, companyLogo);
				Assert("Obtained value from registry was incorrect", Utilities.IsImageEqual(companyLogo, ItemSet.ForwardersCertificateOfReceiptLogo.Value));

				ItemSet.ForwardersCertificateOfReceiptLogo.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, branchLogo);
				Assert("Obtained value from registry was incorrect", Utilities.IsImageEqual(branchLogo, ItemSet.ForwardersCertificateOfReceiptLogo.Value));
			}
		}

		public void TestForwardersCertificateOfReceiptFCRClause()
		{
			string defaultFCRClauseText = "The Forwarder certifies having received and assumed control of the above mentioned goods in external apparent good order and condition at the disposal of the Consignee." + System.Environment.NewLine +
				"This Forwarder Certificate of Receipt is not a document of title as far as the goods are concerned." + System.Environment.NewLine +
				"The production or surrendering of this Forwarder Certificate of Receipt will not entitle its holder to take delivery of the goods." + System.Environment.NewLine +
				"Once the goods are received by the Forwarder from the shipper, the right of disposing the goods rests with the Consignee." + System.Environment.NewLine +
				"The goods and instructions are accepted and dealt with subject to the standard terms and conditions, available on request, of the forwarder issuing this Forwarder Certificate of Receipt.";
			AssertEquals("DefaultValue", defaultFCRClauseText, ItemSet.ForwardersCertificateOfReceiptFCRClause.DefaultValue);

			ItemSet.ForwardersCertificateOfReceiptFCRClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "New FCR Clause System");
			AssertEquals("ForwardersCertificateOfReceiptFCRClause.Value", "New FCR Clause System", ItemSet.ForwardersCertificateOfReceiptFCRClause.Value);

			ItemSet.ForwardersCertificateOfReceiptFCRClause.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "New FCR Clause Company");
			AssertEquals("ForwardersCertificateOfReceiptFCRClause.Value", "New FCR Clause Company", ItemSet.ForwardersCertificateOfReceiptFCRClause.Value);

			ItemSet.ForwardersCertificateOfReceiptFCRClause.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "New FCR Clause Branch");
			AssertEquals("ForwardersCertificateOfReceiptFCRClause.Value", "New FCR Clause Branch", ItemSet.ForwardersCertificateOfReceiptFCRClause.Value);
		}

		public void TestRequestForCollectChargesOpeningText()
		{
			AssertEquals("DefaultValue", "Please provide details of all charges to collect for the shipment noted below.",
			ItemSet.RequestForCollectChargesOpeningText.DefaultValue);

			ItemSet.RequestForCollectChargesOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Value");
			AssertEquals("RequestForCollectChargesOpeningText.Value", "New Value", ItemSet.RequestForCollectChargesOpeningText.Value);

			ItemSet.RequestForCollectChargesOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertEquals("RequestForCollectChargesOpeningText.Value", "", ItemSet.RequestForCollectChargesOpeningText.Value);
		}

		public void TestRequestForCollectChargesClosingText()
		{
			ItemSet.RequestForCollectChargesClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Value");
			AssertEquals("RequestForCollectChargesOpeningText.Value", "New Value", ItemSet.RequestForCollectChargesClosingText.Value);

			ItemSet.RequestForCollectChargesClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertEquals("RequestForCollectChargesOpeningText.Value", "", ItemSet.RequestForCollectChargesClosingText.Value);
		}

		public void TestCarrierBookingRequestOpeningText()
		{
			TestMultilingualRegistryItem(ItemSet.CarrierBookingRequestOpeningText, "CarrierBookingRequestOpeningText", "Documents/Forwarding/Consol/Carrier Booking Request", "Opening Text", "The opening text for the Carrier Booking Request document.", RegistryStorageFlags.All, TextEditorType.Memo, string.Empty);
		}

		public void TestCarrierBookingRequestClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.CarrierBookingRequestClosingText, "CarrierBookingRequestClosingText", "Documents/Forwarding/Consol/Carrier Booking Request", "Closing Text", "The closing text for the Carrier Booking Request document.", RegistryStorageFlags.All, TextEditorType.Memo, string.Empty);
		}

		public void TestRequestForServiceOpeningText()
		{
			TestMultilingualRegistryItem(ItemSet.RequestForServiceOpeningText, "RequestForServiceOpeningText", "Documents/Service/Request For Service", "Opening Text", "The opening text for the Request for Service document.", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
		}

		public void TestDeliveryInformationClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.DeliveryInformationClosingText, "DeliveryInformationClosingText", "Documents/Forwarding/Shipment/Delivery Information", "Closing Text", "The closing text for the Delivery Information document.", RegistryStorageFlags.All, TextEditorType.Memo, string.Empty);
		}

		public void TestDeliveryInformationOpeningText()
		{
			TestMultilingualRegistryItem(ItemSet.DeliveryInformationOpeningText, "DeliveryInformationOpeningText", "Documents/Forwarding/Shipment/Delivery Information", "Opening Text", "The opening text for the Delivery Information document.", RegistryStorageFlags.All, TextEditorType.Memo, string.Empty);
		}

		public void TestRequestForServiceClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.RequestForServiceClosingText, "RequestForServiceClosingText", "Documents/Service/Request For Service", "Closing Text", "The closing text for the Request for Service document.", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
		}

		public void TestAuthorisationForServiceOpeningText()
		{
			TestMultilingualRegistryItem(ItemSet.AuthorisationForServiceOpeningText, "AuthorisationForServiceOpeningText", "Documents/Service/Authorization For Service", "Opening Text", Res.GetString("5bd277f9-0c3d-4876-9cfe-ce868a649449", "The opening text for the Authorization for Service document."), RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
		}

		public void TestAuthorisationForServiceClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.AuthorisationForServiceClosingText, "AuthorisationForServiceClosingText", "Documents/Service/Authorization For Service", "Closing Text", Res.GetString("1ea2415f-359a-4f39-816c-17c4dadc8014", "The closing text for the Authorization for Service document."), RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
		}

		public void TestRequestForProfitShareOpeningText_Shipment()
		{
			TestMultilingualRegistryItem(ItemSet.RequestForProfitShareOpeningText_Shipment, "RequestForProfitShareOpeningText_Shipment", "Documents/Forwarding/Shipment/Request For Profit Share", "Opening Text", "The opening text for the Request for Profit Share.", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
		}

		public void TestGuaranteeDeclarationText()
		{
			string defaultValue = Res.GetString("1d3f335a-b710-4909-8c1a-307c02dfa7c7", @"In consideration of your complying with our above request we hereby agree as follows: -
1. To indemnify you and hold you harmless in respect of any liability, loss or damage of whatsoever nature which you may sustain by reason of delivering the goods to us (the Consignee) in accordance with our request.
2. To pay you on demand the amount of any loss or damage which the master and/or agents of the vessel or any other of your servants or agents whatsoever may incur as a result of delivering the goods as aforesaid.
3. In the event of any proceedings being commenced against you or any of your servant or agents in connection with the delivery of the goods as aforesaid, to provide you or them from time to time on demand with sufficient to defend the same.
4. If called upon to do so at any time while the goods are in our custody, possession or control to redeliver the same to you.
5. To produce and deliver to you the Bills of Lading for the above goods duly endorsed, as soon as these documents shall have arrived.
6. Liability of each and every person under this indemnity shall be joint and several and shall not be conditional upon your proceeding first against any person, whether or not such person is party to or liable under this indemnity.
7. This indemnity shall be construed in accordance with Singapore law and each and every person liable under this indemnity shall at your request summit to the jurisdiction of the High Court of Singapore and the liability there under shall be determined accordingly.");

			AssertEquals("DefaultValue", defaultValue, ItemSet.GuaranteeDeclarationText.DefaultValue);

			ItemSet.GuaranteeDeclarationText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Guarantee This.");
			AssertEquals("PreviewEngineToUse.Value", "Guarantee This.", ItemSet.GuaranteeDeclarationText.Value);

			ItemSet.GuaranteeDeclarationText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			AssertEquals("GuaranteeDeclarationText.Value", "", ItemSet.GuaranteeDeclarationText.Value);
		}

		public void TestDisplayContainerDetailsOnConsol()
		{
			AssertEquals("DefaultValue", true, ItemSet.DisplayContainerDetailsOnConsol.DefaultValue);
			ItemSet.DisplayContainerDetailsOnConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("New Value", false, ItemSet.DisplayContainerDetailsOnConsol.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.DisplayContainerDetailsOnConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("New Value", true, ItemSet.DisplayContainerDetailsOnConsol.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDisplayPackLinesByContainerOnConsol()
		{
			TestRegistryItem(ItemSet.DisplayPackLinesByContainerOnConsol,
					"DisplayPackLinesByContainerOnConsol",
					DocumentsDataRegistry.Categories.Documents_Forwarding_Consol_ForwardingInstruction,
					"Display Pack Lines as part of Containers",
					"This registry item controls whether packing lines details are displayed as part of Containers on Consols.",
					RegistryStorageFlags.All,
					false);
		}

		public void TestReleaseType()
		{
			TestRegistryItem(ItemSet.ReleaseType, "ConsolReleaseType", DocumentsDataRegistry.Categories.Documents_Forwarding_Consol_ForwardingInstruction, "Release Type", "Default Release Type for Consol", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList(), "");
		}

		public void TestHBLChargesDefaultDisplay()
		{
			TestRegistryItem(ItemSet.HBLChargesDefaultDisplay, "HBLChargesDisplay", DocumentsDataRegistry.Categories.Documents_Forwarding_Shipment_BillofLading, "HBL Charges Default Display", "The default type of charges to display on House Bills of Lading", RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, DocumentsDataRegistry.Instance.HBLChargesDefaultDisplayTypesPairList, DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges);
		}

		public void TestHBLChargesDefaultDisplayTypesPairList()
		{
			const string expected =
				"NON - No Charges showing\r\n" +
				"SHW - Show Collect Charges\r\n" +
				"PPD - Show Prepaid Charges\r\n" +
				"AGR - Show \"As Agreed\" in the charges section\r\n" +
				"ALL - Show Prepaid & Collect Charges\r\n" +
				"CCL - Show Original \"As Agreed\" & Copy with Collect Charges\r\n" +
				"CPP - Show Original \"As Agreed\" & Copy with Prepaid Charges\r\n" +
				"CAL - Show Original \"As Agreed\" & Copy with Prepaid & Collect Charges\r\n" +
				"";

			AssertMultilineASCIIEquals("", expected, ItemSet.HBLChargesDefaultDisplayTypesPairList.ElementsAsString);
		}

		public void TestRequestForProfitShareOpeningText_Consol()
		{
			TestMultilingualRegistryItem(ItemSet.RequestForProfitShareOpeningText_Consol, "RequestForProfitShareOpeningText_Consol", "Documents/Forwarding/Consol/Request For Profit Share", "Opening Text", "The opening text for the Request for Profit Share.", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
		}

		public void TestDisplayContainerDetailsOnShipment()
		{
			AssertEquals("DefaultValue", true, ItemSet.DisplayContainerDetailsOnShipment.DefaultValue);
			ItemSet.DisplayContainerDetailsOnShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("New Value", false, ItemSet.DisplayContainerDetailsOnShipment.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.DisplayContainerDetailsOnShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("New Value", true, ItemSet.DisplayContainerDetailsOnShipment.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDocumentPreviewFormLayout()
		{
			((IRegistryItemInternals)ItemSet.DocumentPreviewFormLayout).DeleteValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty);
			AssertEquals("IsSetByCurrentUser", false, ItemSet.DocumentPreviewFormLayout.IsSetByCurrentUser);

			ItemSet.DocumentPreviewFormLayout.UserValue = new DocumentPreviewFormLayout(true, 7, 8);

			AssertEquals("IsSetByCurrentUser", true, ItemSet.DocumentPreviewFormLayout.IsSetByCurrentUser);
			AssertEquals("UserValue.IsThumbnailPanelVisible", true, ItemSet.DocumentPreviewFormLayout.UserValue.IsThumbnailPanelVisible);
			AssertEquals("UserValue.ThumbnailPanelPixelWidth", 7, ItemSet.DocumentPreviewFormLayout.UserValue.ThumbnailPanelPixelWidth);
			AssertEquals("UserValue.ZoomValue", 8, ItemSet.DocumentPreviewFormLayout.UserValue.ZoomValue);
		}

		public void TestGatePassClauseText()
		{
			AssertEquals("DefaultValue", "Received in apparent good order and condition subject to the terms and conditions contained in the relevant bill of lading, and the exceptions noted hereon.", ItemSet.GatePassClauseText.DefaultValue);

			ItemSet.GatePassClauseText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "Clause Text");
			AssertEquals("Value", "Clause Text", ItemSet.GatePassClauseText.Value);
		}

		public void TestShowContractNumbersOnShippingQuotationDocuments()
		{
			TestRegistryItem(
				ItemSet.ShowContractNumbersOnShippingQuotationDocuments,
				"ShowContractNumbersOnShippingQuotationDocuments",
				DocumentsDataRegistry.Categories.Documents_QuotationsandRates,
				"Show contract numbers on shipping quotation documents.",
				"Controls whether to show or hide Contract Number on the quotation documents.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestLocalCurrencyOnSpotQuote()
		{
			AssertEquals(RegistryStorageFlags.Company, ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.Storage);
			AssertEquals("Controls whether local currency equivalents and exchange rates appear on the Spot Quote document.\r\n\r\nOnly applies to legacy versions of the document.", ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.Hint);
			AssertEquals("Documents/Quotations and Rates", ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.Category);
			AssertEquals(true, ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.DefaultValue);
			AssertEquals("Show local currency equivalents and exchange rates on the Spot Quote Pricing Page.", ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.Caption);
			ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.ShowLocalCurrencyonSpotQuotePricingPage.Value);
		}

		public void TestShowGatePassStatementText()
		{
			AssertEquals("DefaultValue", true, ItemSet.ShowGatePassStatementText.DefaultValue);

			ItemSet.ShowGatePassStatementText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			AssertEquals(false, ItemSet.ShowGatePassStatementText.Value);
		}

		public void TestHBLAndHAWBBrandingOption()
		{
			AssertEquals("DefaultValue", HBLAndHAWBBrandingOptionEditorInfo.AgentBranded, ItemSet.HBLAndHAWBBrandingOption.DefaultValue);

			ItemSet.HBLAndHAWBBrandingOption.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.ClientBranded);
			AssertEquals("Value", HBLAndHAWBBrandingOptionEditorInfo.ClientBranded, ItemSet.HBLAndHAWBBrandingOption.Value);
		}

		public void TestOrderDelayAlertText()
		{
			AssertEquals("DefaultValue", "PLEASE NOTE, YOUR ORDER HAS BEEN DELAYED, PLEASE SEE UPDATED ORDER DETAILS BELOW", ItemSet.OrderDelayAlertText.DefaultValue);

			ItemSet.OrderDelayAlertText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "New Value");
			AssertEquals("Value", "New Value", ItemSet.OrderDelayAlertText.Value);
		}

		public void TestUserSignOff()
		{
			AssertEquals("ShowPublishedStaffDetailsOnDocuments.DefaultValue", true, ItemSet.ShowPublishedStaffDetailsOnDocuments.DefaultValue);
			AssertEquals("ShowUserTitleOnDocuments.DefaultValue", true, ItemSet.ShowUserTitleOnDocuments.DefaultValue);

			ItemSet.ShowUserTitleOnDocuments.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);
			AssertEquals("ShowPublishedStaffDetailsOnDocuments.Value", true, ItemSet.ShowPublishedStaffDetailsOnDocuments.Value);
			AssertEquals("ShowUserTitleOnDocuments.Value", false, ItemSet.ShowUserTitleOnDocuments.Value);

			ItemSet.ShowPublishedStaffDetailsOnDocuments.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);
			AssertEquals("ShowPublishedStaffDetailsOnDocuments.Value", false, ItemSet.ShowPublishedStaffDetailsOnDocuments.Value);
			AssertEquals("ShowUserTitleOnDocuments.Value", false, ItemSet.ShowUserTitleOnDocuments.Value);
		}

		public void TestLandedCostingClosingText()
		{
			ItemSet.LandedCostingClosingText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "RWAH!");
			AssertEquals("Value", "RWAH!", ItemSet.LandedCostingClosingText.Value);
		}

		public void TestPullLandedCostingDataFromBillingTabOnly()
		{
			AssertEquals("PullLandedCostingDataFromBillingTabOnly.DefaultValue", false, ItemSet.PullLandedCostingDataFromBillingTabOnly.DefaultValue);
			AssertEquals("Pull Landed Costing details from Billing Tab Only", ItemSet.PullLandedCostingDataFromBillingTabOnly.Caption);

			ItemSet.PullLandedCostingDataFromBillingTabOnly.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals("PullLandedCostingDataFromBillingTabOnly.Value", true, ItemSet.PullLandedCostingDataFromBillingTabOnly.Value);

			ItemSet.PullLandedCostingDataFromBillingTabOnly.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			AssertEquals("PullLandedCostingDataFromBillingTabOnly.Value", false, ItemSet.PullLandedCostingDataFromBillingTabOnly.Value);
		}

		public void TestDoNotPullZeroAmountsFromBillingTab()
		{
			AssertEquals("DoNotPullZeroAmountsFromBillingTab.DefaultValue", false, ItemSet.DoNotPullZeroAmountsFromBillingTab.DefaultValue);

			ItemSet.DoNotPullZeroAmountsFromBillingTab.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals("DoNotPullZeroAmountsFromBillingTab.Value", true, ItemSet.DoNotPullZeroAmountsFromBillingTab.Value);

			ItemSet.DoNotPullZeroAmountsFromBillingTab.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			AssertEquals("DoNotPullZeroAmountsFromBillingTab.Value", false, ItemSet.DoNotPullZeroAmountsFromBillingTab.Value);
		}

		public void TestShippingOrderClosingText()
		{
			ItemSet.ShippingOrderClosingText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "Closing!");
			AssertEquals("Value", "Closing!", ItemSet.ShippingOrderClosingText.Value);
		}

		public void TestIncludeCFXInRatingCalculation()
		{
			AssertEquals("Default Value", true, ItemSet.IncludeCFXinExchangeRateOnQuotationPrinting.Value);
			ItemSet.IncludeCFXinExchangeRateOnQuotationPrinting.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.IncludeCFXinExchangeRateOnQuotationPrinting.Value);

			ItemSet.IncludeCFXinExchangeRateOnQuotationPrinting.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.IncludeCFXinExchangeRateOnQuotationPrinting.Value);
		}

		public void TestDelayAlertOpeningAndClosingText()
		{
			ItemSet.DelayAlertClosingText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "Delay Alert Closing Text.");
			ItemSet.DelayAlertOpeningText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "Delay Alert Opening Text.");

			AssertEquals("DelayAlertClosingText", "Delay Alert Closing Text.", ItemSet.DelayAlertClosingText.Value);
			AssertEquals("DelayAlertOpeningText", "Delay Alert Opening Text.", ItemSet.DelayAlertOpeningText.Value);
		}

		public void TestCertificateOfOriginStandardClause()
		{
			AssertEquals("DefaultValue", Res.GetString("7c00f16f-8a1f-4515-bc1b-abdc3b1fb28b", "I the undersigned, duly authorized by the above exporter and having made the necessary enquiries HEREBY CERTIFY THAT the goods listed above are of origin, production and manufacture in:"), ItemSet.CertificateOfOriginStandardClause.DefaultValue);

			ItemSet.CertificateOfOriginStandardClause.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "Hopping Mad");
			AssertEquals("Value", "Hopping Mad", ItemSet.CertificateOfOriginStandardClause.Value);
		}

		public void TestLocalChamberOfCommerceInformation()
		{
			AssertEquals("DefaultValue", "", ItemSet.LocalChamberOfCommerceInformation.DefaultValue);
			Assert("CountryFilterPK", ItemSet.LocalChamberOfCommerceInformation.CountryFilterPKs.Contains(Core.Constants.CountryGuids.UnitedStates));
			Assert("CountryFilterPK", ItemSet.LocalChamberOfCommerceInformation.CountryFilterPKs.Contains(Core.Constants.CountryGuids.PuertoRico));

			ItemSet.LocalChamberOfCommerceInformation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "information");
			AssertEquals("Value", "information", ItemSet.LocalChamberOfCommerceInformation.Value);
		}

		public void TestNotaryPublicInformation()
		{
			AssertEquals("DefaultValue", "", ItemSet.NotaryPublicInformation.DefaultValue);
			Assert("CountryFilterPK", ItemSet.NotaryPublicInformation.CountryFilterPKs.Contains(Core.Constants.CountryGuids.UnitedStates));

			ItemSet.NotaryPublicInformation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "information");
			AssertEquals("Value", "information", ItemSet.NotaryPublicInformation.Value);
		}

		public void TestShipmentDelayAlertAlertText()
		{
			AssertEquals("DefaultValue", "PLEASE NOTE, YOUR SHIPMENT HAS BEEN DELAYED, PLEASE SEE UPDATED SHIPMENT DETAILS BELOW", ItemSet.ShipmentDelayAlertAlertText.DefaultValue);

			ItemSet.ShipmentDelayAlertAlertText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "ALERT!");
			AssertEquals("Value", "ALERT!", ItemSet.ShipmentDelayAlertAlertText.Value);
		}

		public void TestCustomsDelayAlertAlertText()
		{
			AssertEquals("DefaultValue", "PLEASE NOTE, YOUR SHIPMENT HAS BEEN DELAYED, PLEASE SEE UPDATED SHIPMENT DETAILS BELOW", ItemSet.CustomsDelayAlertAlertText.DefaultValue);

			ItemSet.CustomsDelayAlertAlertText.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "WARNING!");
			AssertEquals("Value", "WARNING!", ItemSet.CustomsDelayAlertAlertText.Value);
		}

		public void TestWatermark()
		{
			Watermark watermark = new Watermark();
			watermark.TextWatermark = "Pudding";

			ItemSet.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, watermark);
			AssertEquals("Value.TextWatermark", "Pudding", ItemSet.Watermark.Value.TextWatermark);
		}

		public void TestEmailFormat()
		{
			EmailFormat emailFormat = new EmailFormat();
			emailFormat.Separator = "#";
			emailFormat.Disclaimer = "blah blah";
			ItemSet.EmailFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailFormat);
			AssertEquals("#", ItemSet.EmailFormat.Value.Separator);
			AssertEquals("blah blah", ItemSet.EmailFormat.Value.Disclaimer);
		}

		public void TestCompanyDisplayName()
		{
			AssertEquals("DataType.MaxLength", 35, ((StringRegistryDataType)ItemSet.CompanyDisplayName.DataType).MaxLength);
			ItemSet.CompanyDisplayName.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "Label This");
			AssertEquals("Value", "Label This", ItemSet.CompanyDisplayName.Value);
		}

		public void TestShowOnlyPrintersUserCanPrintTo()
		{
			TestRegistryItem(ItemSet.ShowOnlyPrintersUserCanPrintToRaw, "ShowOnlyPrintersUserCanPrintTo", "", "", "", RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, true);

			ItemSet.ShowOnlyPrintersUserCanPrintToRaw.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("ShowOnlyPrintersUserCanPrintTo", false, ItemSet.ShowOnlyPrintersUserCanPrintTo);

			ItemSet.ShowOnlyPrintersUserCanPrintTo = true;
			AssertEquals("GetValue()", true, ItemSet.ShowOnlyPrintersUserCanPrintToRaw.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestContainerLiabilityStatementLiabilityWarningText()
		{
			TestMultilingualRegistryItem(ItemSet.ContainerLiabilityStatementLiabilityWarningText, "ContainerLiabilityStatementAdministrationFeeText", "Documents/Forwarding/Shipment/Container Liability Statement", "Liability Warning Text", "Text describing the Container Liability warning.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, TextEditorType.Memo, "By issuing the Delivery Orders to you for the above mentioned period of time, and by accepting them, you (as the owner or agent of the goods) therefore accept the Terms and Conditions of the Bill of Lading which require the return of the containers in a clean and undamaged condition within the stated free time to the container depot.\r\n\r\nFailure to comply with this agreement will result in all detention charges, cost of repairs or internal cleaning of the containers and an administration fee per container being debited to your company (see above charges).");
		}

		public void TestContainerLiabilityAcceptanceText()
		{
			TestMultilingualRegistryItem(ItemSet.ContainerLiabilityAcceptanceText, "ContainerLiabilityAcceptanceText", "Documents/Forwarding/Shipment/Container Liability Statement", "Liability Acceptance Text", "Text describing the Container Liability Acceptance.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, TextEditorType.Memo, "The signing of this agreement is your acceptance of the above terms.");
		}

		public void TestEFTRequestOpeningAndClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.EFTRequestOpeningTextRaw, "EFTRequestOpeningText", "Documents/Forwarding/Customs/EFT Request", "Opening Text", "This is the opening text for the EFT Request document.", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);
			TestMultilingualRegistryItem(ItemSet.EFTRequestClosingTextRaw, "EFTRequestClosingText", "Documents/Forwarding/Customs/EFT Request", "Closing Text", "This is the closing text for the EFT Request document.", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, TextEditorType.Memo, string.Empty);

			ItemSet.EFTRequestOpeningTextRaw.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "EFT Opening");
			ItemSet.EFTRequestClosingTextRaw.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, "EFT Closing");
			DocumentOpenCloseText openCloseText = ItemSet.EFTRequest;
			AssertEquals("EFTRequest.OpeningText", "EFT Opening", openCloseText.OpeningText);
			AssertEquals("EFTRequest.ClosingText", "EFT Closing", openCloseText.ClosingText);
		}

		public void TestCustomsIncoTermsOverride()
		{
			TestGenericRegistryItem(ItemSet.CustomsIncoTermsOverride,
				"CustomsIncoTermsOverride",
				DocumentsDataRegistry.Categories.Documents_Customs,
				Res.GetString("68d0b369-1d90-05a4-4a29-b9e351ed04f6", "Customs Incoterms Override"),
				Res.GetString("c782e822-4a2c-1d8a-4db2-3207c843a46e", "Documents like 'Commercial Invoice' will normally show the Incoterm as entered on the Invoice which is applicable only to that Customs Country/Region. The document can show the equivalent International Incoterm code by mapping it here."),
				RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestCustomsIncoTermsOverride_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			TestGenericRegistryItem(ItemSet.CustomsIncoTermsOverride,
				"CustomsIncoTermsOverride",
				DocumentsDataRegistry.Categories.Documents_Customs,
				Res.GetString("68d0b369-1d90-05a4-4a29-b9e351ed04f6", "Customs Incoterms Override"),
				Res.GetString("c782e822-4a2c-1d8a-4db2-3207c843a46e", "Documents like 'Commercial Invoice' will normally show the Incoterm as entered on the Invoice which is applicable only to that Customs Country/Region. The document can show the equivalent International Incoterm code by mapping it here."),
				RegistryStorageFlags.Company, RegistryOptions.IsHidden);
		}

		public void TestReportDBCommandTimeOut()
		{
			AssertEquals(900, ItemSet.ReportDBCommandTimeOut.DefaultValue);
			ItemSet.ReportDBCommandTimeOut.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 600);
			AssertEquals(600, ItemSet.ReportDBCommandTimeOut.Value);
			AssertEquals(RegistryOptions.PreserveTestValue, ItemSet.ReportDBCommandTimeOut.Options);
			ItemSet.ReportDBCommandTimeOut.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 300);

			IntRegistryDataType dataType = (IntRegistryDataType)ItemSet.ReportDBCommandTimeOut.DataType;
			AssertEquals("DataType.LowerBound", 300, (int)dataType.LowerBound);
			AssertEquals("DataType.UpperBound", 1800, (int)dataType.UpperBound);
		}

		public void TestReportDBCommandMaximumDegreeOfParallelism()
		{
			TestRegistryItem(ItemSet.ReportDBCommandMaximumDegreeOfParallelism,
				"ReportDBCommandMaximumDegreeOfParallelism",
				"Documents",
				"Report Command Maximum Degree Of Parallelism",
				@"The MAXDOP(Maximum Degree of Parallelism) value specifies how many processors are used to execute a single SQL query.
Note: A high value could accelerate query execution, but significantly raises CPU usage and potentially affects system performance. The value of zero indicates that it is unset, and the decision to parallelize the query is made by the database engine.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				0);
		}

		public void TestEPrintEmailAddress()
		{
			AssertEquals(string.Empty, ItemSet.EPrintEmailAddress.Value);
			ItemSet.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com");
			AssertEquals("test@test.com", ItemSet.EPrintEmailAddress.Value);
		}

		public void TestAllowDocumentsToBeModified()
		{
			TestRegistryItem(ItemSet.AllowDocumentsToBeModified, "AllowDocumentsToBeModified", "Documents", "Allow Documents To Be Modified", "When printing / previewing documents, there is a button that allows user to view the document in an editing mode and user would be able to override most of the fields on that document before printing. Turning on this registry setting will enable users to modify documents. Individual staff / document can then be granted or denied 'modify' access in the Security tab of Staff or Group form. If this registry is turned off, then this feature is disabled throughout the system.", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, true);
		}

		public void TestAddEmailFaxCoverNote()
		{
			TestRegistryItem(ItemSet.AddEmailFaxCoverNote, "AddEmailFaxCoverNote", "Documents", "Add Email/Fax Cover Note", "Specifies whether the email fax cover note will be added to the email body when delivering documents through email.", RegistryStorageFlags.Company, true);
		}

		public void TestAddEmailSignature()
		{
			TestRegistryItem(ItemSet.AddEmailSignature, "AddEmailSignature", "Documents", "Add Email Signature", "Specifies whether the email signature will be added to the email footer when delivering documents through email.", RegistryStorageFlags.Company, true);
		}

		public void TestEmbedFontsInPDF()
		{
			TestRegistryItem(ItemSet.EmbedFontsInPDF, "EmbedFontsInPDF", "Documents", "Embed Fonts In PDF", $"Ensures that fonts are embedded in PDF document so that when these documents are rendered on non-{BrandingFactory.Instance.ProductName} workstations, the fonts display correctly.", RegistryStorageFlags.System, true);
		}

		public void TestUseRecompileQueryHint()
		{
			TestRegistryItem(ItemSet.UseRecompileQueryHint, "UseRecompileQueryHint", "Documents", "Use recompile SQL Query Hint", "This will determinate whether 'recompile' SQL Query Hint will be used by default for all documents.", RegistryStorageFlags.System, true);
		}

		public void TestEnableNZCFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableNZCFTASubmissionToCAB, "EnableNZCFTASubmissionToCAB", "Documents/Digital Docs/Certification", "NZCFTA", "Enable submission of NZCFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableCPTPPSubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableCPTPPSubmissionToCAB, "EnableCPTPPSubmissionToCAB", "Documents/Digital Docs/Certification", "CPTPP", "Enable submission of CPTPP Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableRCEPSubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableRCEPSubmissionToCAB, "EnableRCEPSubmissionToCAB", "Documents/Digital Docs/Certification", "RCEP", "Enable submission of RCEP Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableAANZFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableAANZFTASubmissionToCAB, "EnableAANZFTASubmissionToCAB", "Documents/Digital Docs/Certification", "AANZFTA", "Enable submission of AANZFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableJAEPASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableJAEPASubmissionToCAB, "EnableJAEPASubmissionToCAB", "Documents/Digital Docs/Certification", "JAEPA", "Enable submission of JAEPA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableAUKFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableAUKFTASubmissionToCAB, "EnableAUKFTASubmissionToCAB", "Documents/Digital Docs/Certification", "A-UKFTA", "Enable submission of A-UKFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableTAFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableTAFTASubmissionToCAB, "EnableTAFTASubmissionToCAB", "Documents/Digital Docs/Certification", "TAFTA", "Enable submission of TAFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableIAECTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableIAECTASubmissionToCAB, "EnableIAECTASubmissionToCAB", "Documents/Digital Docs/Certification", "IA-ECTA", "Enable submission of IA-ECTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableKAFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableKAFTASubmissionToCAB, "EnableKAFTASubmissionToCAB", "Documents/Digital Docs/Certification", "KAFTA", "Enable submission of KAFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnablePAFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnablePAFTASubmissionToCAB, "EnablePAFTASubmissionToCAB", "Documents/Digital Docs/Certification", "PAFTA", "Enable submission of PAFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableIACEPASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableIACEPASubmissionToCAB, "EnableIACEPASubmissionToCAB", "Documents/Digital Docs/Certification", "IA-CEPA", "Enable submission of IA-CEPA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableCertOfOriginAUSubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableCertOfOriginAUSubmissionToCAB, "EnableCertOfOriginAUSubmissionToCAB", "Documents/Digital Docs/Certification", "Cert of Origin AU", "Enable submission of Cert of Origin AU Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableCOOUSSubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableCOOUSSubmissionToCAB, "EnableCOOUSSubmissionToCAB", "Documents/Digital Docs/Certification", "COOUS", "Enable submission of US Cert of Origin to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestCertOfOriginIndemnity()
		{
			var expectedResult = @"The Applicant (or the Applicant on behalf of the Consignor), by utilizing WiseTech Certification, certifies that:
	a)	the goods mentioned in the certificates and/or other documents originate in the country (countries) specified in the documents(s) and comply with the rules of origin applicable in the country (countries) to those goods
	b)	the information in the certificates and/or other documents provided to WiseTech Certification is accurate, true and complete
	c)	they (the applicant) will advise WiseTech Certification and any other person(s) to whom the applicant provides the Certificates and/or other documents promptly in writing of any inaccuracy, omission or change in such information, or in the origin of the goods
	d)	they (the applicant) will maintain, and present upon request, such documentation as is necessary to verify the truth, accuracy and completeness of all Certificates, and/or other documents, issued by WiseTech Certification
	e)	in consideration for WiseTech Certification’s issuance of Certificates or Origin and/or other documents, the applicant agrees to release, discharge and hold harmless WiseTech Certification from any liability in connection with the issuance of the Certificates and/or other documents and to indemnify WiseTech Certification in respect of any costs herewith
	f)	in the event of requests which stem from a legitimate enquiry from someone in possession of statutory authority e.g. Police, Department of Foreign Affairs & Trade or officials acting with authority of a Court Order, I/we hereby permit WiseTech Certification to allow direct access, under the power of statutory authority, to such commercial information as may be required as part of the enquiry
	g)	the applicant is authorized to give the undertakings set out herein";

			TestRegistryItem(
				ItemSet.CertOfOriginIndemnity,
				"CertOfOriginIndemnity",
				"Documents/Digital Docs/Certification",
				"Cert of Origin Indemnity",
				"Indemnity Terms",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.Memo,
				expectedResult);
		}

		public void TestEnableCertOfOriginIndemnity()
		{
			TestRegistryItem(ItemSet.EnableCertOfOriginIndemnity, "EnableCertOfOriginIndemnity", "Documents/Digital Docs/Certification", "Enable Cert of Origin Indemnity", "Indemnity Terms", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableCOONZSubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableCOONZSubmissionToCAB, "EnableCOONZSubmissionToCAB", "Documents/Digital Docs/Certification", "Cert of Origin NZ", "Enable submission of Cert of Origin NZ to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableChAFTASubmissionToCAB()
		{
			TestRegistryItem(ItemSet.EnableChAFTASubmissionToCAB, "EnableChAFTASubmissionToCAB", "Documents/Digital Docs/Certification", "ChAFTA", "Enable submission of ChAFTA Certificate to CAB.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableVerboseLoggingForCertification()
		{
			TestRegistryItem(ItemSet.EnableVerboseLoggingForCertification, "EnableVerboseLoggingForCertification", "Documents/Digital Docs/Certification", "Verbose Logging", "Enable Verbose Logging", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableSubmissionToCustomsAuthority()
		{
			TestRegistryItem(ItemSet.EnableSubmissionToCustomsAuthority, "EnableSubmissionToCustomsAuthority", "Documents/Digital Docs/Certification", "Enable Submission to Customs Authority", "When turned off, Customs Connector will not forward the submission to the customs authority and will simply return a rejection.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestUseNewDocBuilderForwardingDocuments()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderForwardingDocuments, "UseNewDocStripForwardingDocuments", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Freight Documents", "When printing Freight Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestUseNewDocBuilderForwardingDocuments_PW()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderForwardingDocuments, "UseNewDocStripForwardingDocuments", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Freight Documents", "When printing Freight Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		public void TestUseNewDocBuilderOrderManagerDocuments()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderOrderManagerDocuments, "UseNewDocBuilderOrderManagerDocuments", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Order Manager Documents", "When printing Order Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestUseNewDocBuilderOrderManagerDocuments_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderOrderManagerDocuments, "UseNewDocBuilderOrderManagerDocuments", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Order Manager Documents", "When printing Order Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		public void TestUseNewDocBuilderWarehouseDocumentsOnly()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderWarehouseDocumentsOnly, "UseNewDocBuilderWarehouseDocumentsOnly", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Warehouse Documents Only", "When printing Warehouse Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, true);
		}

		public void TestUseNewDocBuilderWarehouseDocumentsOnly_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderWarehouseDocumentsOnly, "UseNewDocBuilderWarehouseDocumentsOnly", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Warehouse Documents Only", "When printing Warehouse Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderLinerAndAgencyDocumentsOnly()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderLinerAndAgencyDocumentsOnly, "UseNewDocBuilderLinerAndAgencyDocumentsOnly", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Liner And Agency Documents Only", "When printing Liner And Agency Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestUseNewDocBuilderLinerAndAgencyDocumentsOnly_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderLinerAndAgencyDocumentsOnly, "UseNewDocBuilderLinerAndAgencyDocumentsOnly", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Liner And Agency Documents Only", "When printing Liner And Agency Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		public void TestUseNewDocBuilderOrganizationDocumentsOnly()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderOrganizationDocumentsOnly, "UseNewDocBuilderOrganizationDocumentsOnly", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Organization Documents Only", "When printing Organization Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		#region Accounting
		public void TestUseNewDocBuilderAccountingVoucher()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderAccountingVoucher, "UseNewDocBuilderAccountingVoucher", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Accounting Voucher", @"This registry setting controls the document template that is used when printing the Accounting Voucher document.

Select 'Yes' - The system will use the DocBuilder Accounting Voucher document.

Select 'No' - The system will use the Legacy Accounting Voucher document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestUseNewDocBuilderInvoice()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderARInvoice, "UseNewDocBuilderARInvoice", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder AR Invoice", @"This registry setting controls the document template that is used when printing the AR Invoice document.

Select 'Yes' - The system will use the DocBuilder AR Invoice document.

Select 'No' - The system will use the Legacy AR Invoice document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderRemittanceAdvice()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderRemittanceAdvice, "UseNewDocBuilderRemittanceAdvice", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Remittance Advice", @"This registry setting controls the document template that is used when printing the Remittance Advice.

Select 'Yes' - The system will use the DocBuilder Remittance Advice.

Select 'No' - The system will use the Legacy Remittance Advice.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderPaymentVoucher()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderPaymentVoucher, "UseNewDocBuilderPaymentVoucher", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Payment Voucher", @"This registry setting controls the document template that is used when printing the Payment Voucher.

Select 'Yes' - The system will use the DocBuilder Payment Voucher.

Select 'No' - The system will use the Legacy Payment Voucher.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderReceiptDocument()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderReceiptDocument, "UseNewDocBuilderReceiptDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Receipt Document", @"This registry setting controls the document template that is used when printing the Receipt document.

Select 'Yes' - The system will use the DocBuilder Receipt document.

Select 'No' - The system will use the Legacy Receipt document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderMatchingDocument()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderMatchingDocument, "UseNewDocBuilderMatchingDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Matching Document", @"This registry setting controls the document template that is used when printing the Matching document.

Select 'Yes' - The system will use the DocBuilder Matching document.

Select 'No' - The system will use the Legacy Matching document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderStatementDocument()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderStatementDocument, "UseNewDocBuilderStatementDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Statement Document", @"This registry setting controls the document template that is used when printing the Statement document.

Select 'Yes' - The system will use the DocBuilder Statement document.

Select 'No' - The system will use the Legacy Statement document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderCostConfirmationDocument()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderCostConfirmationDocument, "UseNewDocBuilderCostConfirmationDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Cost Confirmation Document", @"This registry setting controls the document template that is used when printing the cost confirmation document.

Select 'Yes' - The system will use the DocBuilder cost confirmation document.

Select 'No' - The system will use the Legacy cost confirmation document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderSupplementaryDetail()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderSupplementaryDetail, "UseNewDocBuilderSupplementaryDetail", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Periodic Invoice Supplementary Detail", @"This registry setting controls the document template that is used when printing the supplementary detail document attached to periodic invoices.

Select 'Yes' - The system will use the DocBuilder supplementary detail document.

Select 'No' - The system will use the Legacy supplementary detail document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseNewDocBuilderAccountingVoucher_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderAccountingVoucher, "UseNewDocBuilderAccountingVoucher", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Accounting Voucher", @"This registry setting controls the document template that is used when printing the Accounting Voucher document.

Select 'Yes' - The system will use the DocBuilder Accounting Voucher document.

Select 'No' - The system will use the Legacy Accounting Voucher document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		public void TestUseNewDocBuilderInvoice_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderARInvoice, "UseNewDocBuilderARInvoice", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder AR Invoice", @"This registry setting controls the document template that is used when printing the AR Invoice document.

Select 'Yes' - The system will use the DocBuilder AR Invoice document.

Select 'No' - The system will use the Legacy AR Invoice document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderRemittanceAdvice_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderRemittanceAdvice, "UseNewDocBuilderRemittanceAdvice", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Remittance Advice", @"This registry setting controls the document template that is used when printing the Remittance Advice.

Select 'Yes' - The system will use the DocBuilder Remittance Advice.

Select 'No' - The system will use the Legacy Remittance Advice.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderPaymentVoucher_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderPaymentVoucher, "UseNewDocBuilderPaymentVoucher", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Payment Voucher", @"This registry setting controls the document template that is used when printing the Payment Voucher.

Select 'Yes' - The system will use the DocBuilder Payment Voucher.

Select 'No' - The system will use the Legacy Payment Voucher.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderReceiptDocument_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderReceiptDocument, "UseNewDocBuilderReceiptDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Receipt Document", @"This registry setting controls the document template that is used when printing the Receipt document.

Select 'Yes' - The system will use the DocBuilder Receipt document.

Select 'No' - The system will use the Legacy Receipt document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderMatchingDocument_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderMatchingDocument, "UseNewDocBuilderMatchingDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Matching Document", @"This registry setting controls the document template that is used when printing the Matching document.

Select 'Yes' - The system will use the DocBuilder Matching document.

Select 'No' - The system will use the Legacy Matching document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderStatementDocument_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderStatementDocument, "UseNewDocBuilderStatementDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Statement Document", @"This registry setting controls the document template that is used when printing the Statement document.

Select 'Yes' - The system will use the DocBuilder Statement document.

Select 'No' - The system will use the Legacy Statement document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderCostConfirmationDocument_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderCostConfirmationDocument, "UseNewDocBuilderCostConfirmationDocument", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Cost Confirmation Document", @"This registry setting controls the document template that is used when printing the cost confirmation document.

Select 'Yes' - The system will use the DocBuilder cost confirmation document.

Select 'No' - The system will use the Legacy cost confirmation document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		public void TestUseNewDocBuilderSupplementaryDetail_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderSupplementaryDetail, "UseNewDocBuilderSupplementaryDetail", DocumentsDataRegistry.Categories.Documents_DocBuilder_Accounting, "Use DocBuilder Periodic Invoice Supplementary Detail", @"This registry setting controls the document template that is used when printing the supplementary detail document attached to periodic invoices.

Select 'Yes' - The system will use the DocBuilder supplementary detail document.

Select 'No' - The system will use the Legacy supplementary detail document.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
		}

		#endregion

		public void TestUseNewDocBuilderRatingAndQuotationDocuments()
		{
			TestRegistryItem(ItemSet.UseNewDocBuilderRatingAndQuotationDocuments, "UseNewDocBuilderRatingAndQuotationDocuments", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Rating and Quotation Documents", "When printing Rating and Quotation Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestUseNewDocBuilderRatingAndQuotationDocuments_ProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			TestRegistryItem(ItemSet.UseNewDocBuilderRatingAndQuotationDocuments, "UseNewDocBuilderRatingAndQuotationDocuments", DocumentsDataRegistry.Categories.Documents_DocBuilder, "Use DocBuilder Rating and Quotation Documents", "When printing Rating and Quotation Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		public void TestDocBuilderTheme()
		{
			TestGenericRegistryItem(ItemSet.DocBuilderTheme, "DocBuilderTheme", DocumentsDataRegistry.Categories.Documents_DocBuilder, "DocBuilder Theme", "Allows you to change the colors used for DocBuilder documents. You can either chose from the pre-defined themes, or create your own.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
		}

		#region Test Client and Agent Branding Registry Items

		public void TestEnableClientBranding()
		{
			AssertEquals("DefaultValue", false, ItemSet.EnableClientBranding.DefaultValue);

			ItemSet.EnableClientBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("New Value", true, ItemSet.EnableClientBranding.Value);

			ItemSet.EnableClientBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("New Value", false, ItemSet.EnableClientBranding.Value);
		}

		public void TestEnableAgentBranding()
		{
			AssertEquals("DefaultValue", false, ItemSet.EnableAgentBranding.DefaultValue);

			ItemSet.EnableAgentBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("New Value", true, ItemSet.EnableAgentBranding.Value);

			ItemSet.EnableAgentBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("New Value", false, ItemSet.EnableAgentBranding.Value);
		}

		public void TestClientTariffAndLevels()
		{
			ItemSet.EnableClientBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestClientAndAgentBrandingCollectionRegistryItems(new ClientTariffAndLevelCollection(), ItemSet.ClientTariffAndLevels);
		}

		public void TestAgentDocumentBrand()
		{
			ItemSet.EnableAgentBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestClientAndAgentBrandingCollectionRegistryItems(new AgentDocumentBrandCollection(), ItemSet.AgentDocumentBrand);
		}

		public void TestHBLAgentBrandingImage()
		{
			ItemSet.EnableAgentBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestHybridBrandingCollectionRegistryItems(new HybridDocumentBrandCollection(), ItemSet.HBLAgentBrandingImage);
		}

		public void TestHAWBAgentBrandingImage()
		{
			ItemSet.EnableAgentBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestHybridBrandingCollectionRegistryItems(new HybridDocumentBrandCollection(), ItemSet.HAWBAgentBrandingImage);
		}

		public void TestPrincipalDocumentBrand()
		{
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.PrincipalDocumentBrand.Storage);

			BusinessObject principal = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			principal[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject companyData = (BusinessObject)principal["CompanyData"];
			companyData[OrgCompanyDataSchema.Constants.OB_CRIsShipsAgencyPrincipal] = true;
			Factory.Save();

			PrincipalBrandingCollection collection = new PrincipalBrandingCollection();
			PrincipalBranding branding = collection.AddNew();

			branding.PrincipalPK = principal.PK;
			branding.Code = "COD";
			branding.Image = new Bitmap(10, 10);
			branding.BrandName = "Brand";
			branding.BrandEmailAddress = "Brand@Brand.com";

			ItemSet.PrincipalDocumentBrand.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			PrincipalBrandingCollection obtainedCollection = ItemSet.PrincipalDocumentBrand.Value;

			AssertEquals(1, obtainedCollection.Count);
			AssertEquals(branding.Code, obtainedCollection[0].Code);
		}

		public void TestLocalTransportCompanyBrand()
		{
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.LocalTransportCompanyBrand.Storage);

			BusinessObject localTransportCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			localTransportCompany[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			localTransportCompany[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			Factory.Save();

			var collection = new LocalTransportCompanyBrandingCollection();
			var branding = collection.AddNew();

			branding.LocalTransportCompanyPK = localTransportCompany.PK;
			branding.Image = new Bitmap(10, 10);

			ItemSet.LocalTransportCompanyBrand.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			LocalTransportCompanyBrandingCollection brandingCollection = ItemSet.LocalTransportCompanyBrand.Value;

			AssertEquals(1, brandingCollection.Count);
			AssertEquals(branding.Code, brandingCollection[0].Code);

			AssertEquals("LocalTransportCompanyBrand", ItemSet.LocalTransportCompanyBrand.Name);
			AssertEquals("Local Transport Company Label Branding", ItemSet.LocalTransportCompanyBrand.Caption);
			AssertEquals("The following branding settings will be used for transport company labels.", ItemSet.LocalTransportCompanyBrand.Hint);
		}

		void TestHybridBrandingCollectionRegistryItems(ClientAndAgentBrandingCollection collection, IRegistryItem registryItem)
		{
			ClientAndAgentBrandingBusinessObject element = (ClientAndAgentBrandingBusinessObject)collection.AddNew();

			try
			{
				element.CodeList.AddPair("XYZ", "");
				element.Code = "XYZ";
				element.Description = (NoResString)"XYZ Desc";
				element.Image = new Bitmap(10, 10);

				registryItem.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, collection);
				ClientAndAgentBrandingCollection obtainedCollection = (ClientAndAgentBrandingCollection)registryItem.Value;

				AssertEquals("ObtainedCollection.FindByCode(\"XYZ\").Description", "XYZ Desc", obtainedCollection.FindByCode("XYZ").Description);
			}
			finally
			{
				element.CodeList.RemoveCode("XYZ");
			}
		}

		void TestClientAndAgentBrandingCollectionRegistryItems(ClientAndAgentBrandingCollection collection, IRegistryItem registryItem)
		{
			DocumentBrandingBusinessObject element = (DocumentBrandingBusinessObject)collection.AddNew();

			try
			{
				element.CodeList.AddPair("XYZ", "");
				element.Code = "XYZ";
				element.Description = (NoResString)"XYZ Desc";
				element.Image = new Bitmap(10, 10);
				element.BrandName = "blah";
				element.BrandEmailAddress = "blah@blah.com";

				registryItem.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, collection);
				ClientAndAgentBrandingCollection obtainedCollection = (ClientAndAgentBrandingCollection)registryItem.Value;

				AssertEquals("ObtainedCollection.FindByCode(\"XYZ\").Description", "XYZ Desc", obtainedCollection.FindByCode("XYZ").Description);
			}
			finally
			{
				element.CodeList.RemoveCode("XYZ");
			}
		}

		#endregion

		#region Tests for Quotation Document Items

		public void TestAlternativeRateFormat()
		{
			TestRegistryItem(ItemSet.AlternativeRateFormat,
					"AlternativeRateFormat",
					"Documents/Quotations and Rates",
					"Use Alternative Per Unit Rate Format",
					"If this option is turned on, per unit rates for containers will be printed on individual lines.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue,
					true);
		}

		public void TestAcceptancePageOpeningText()
		{
			ItemSet.AcceptancePageOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cover Page Text New");
			AssertEquals("New Value", "Cover Page Text New", ItemSet.AcceptancePageOpeningText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAcceptancePageClosingText()
		{
			ItemSet.AcceptancePageClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cover Page Text New");
			AssertEquals("New Value", "Cover Page Text New", ItemSet.AcceptancePageClosingText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPrintInheritedOriginDestinationChargesDefault()
		{
			try
			{
				AssertEquals("DefaultValue", true, ItemSet.PrintInheritedOriginChargesDefault.DefaultValue);
				ItemSet.PrintInheritedOriginChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
				AssertEquals("Value", false, ItemSet.PrintInheritedOriginChargesDefault.Value);

				AssertEquals("DefaultValue", true, ItemSet.PrintInheritedDestinationChargesDefault.DefaultValue);
				ItemSet.PrintInheritedDestinationChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
				AssertEquals("Value", false, ItemSet.PrintInheritedDestinationChargesDefault.Value);
			}
			finally
			{
				ItemSet.PrintInheritedOriginChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
				ItemSet.PrintInheritedDestinationChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			}
		}

		public void TestShowCalculationDescriptionOnOneOffQuotes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ShowCalculationDescriptionOnOneOffQuotesCode.Yes, "Under Charge Description generated per Client Organization Invoicing Setup, Calculation details of the Charge is Printed");
			result.AddPair(ShowCalculationDescriptionOnOneOffQuotesCode.No, "Under Charge Description generated per Client Organization Invoicing Setup, Calculation details of the Charge is NOT Printed");
			result.AddPair(ShowCalculationDescriptionOnOneOffQuotesCode.Charge, "Print Charge Description defaulted in One Off Quote ignoring Client Organization Invoicing Setup");

			var expectedLookUpList = new ReadOnlyCodeDescriptionPairList(result);

			TestRegistryItem
			(
				ItemSet.ShowCalculationDescriptionOnOneOffQuotes,
				"ShowCalculationDescriptionOnOneOffQuotes",
				"Documents/Quotations and Rates",
				"One Off Quotes - Include Calculation Description",
				"This registry controls whether the Invoicing Setup of Client Organization and/or mathematical calculation included for displaying Charges in One Off Quotes.",
				RegistryStorageFlags.All,
				RegistryOptions.PreserveTestValue,
				expectedLookUpList,
				ShowCalculationDescriptionOnOneOffQuotesCode.No
			);
		}

		#endregion

		public void TestBOLChargesDefaultDisplay()
		{
			using (RawDataRegistry.Instance.SetTemporaryProductivityWiseModeEnabledForTest(true))
			{
				TestRegistryItem(ItemSet.BOLChargesDefaultDisplay,
				"BOLChargesDisplay",
				DocumentsDataRegistry.Categories.Documents_Forwarding_Consol,
				"Bill Of Lading Charges Default Display",
				"The requested default type of charges display to print on Bill of Lading",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				DocumentsDataRegistry.Instance.HBLChargesDefaultDisplayTypesPairList,
				DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges);
			}

			RegistryItemDictionary.Instance.PurgeAll();

			using (RawDataRegistry.Instance.SetTemporaryProductivityWiseModeEnabledForTest(false))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, DocumentsDataRegistry.Instance.BOLChargesDefaultDisplay.Options);
			}
		}

		public void TestBOLPrintTotalCharges()
		{
			TestRegistryItem(
				ItemSet.BOLPrintTotalCharges,
				"BOLPrintTotalCharges",
				DocumentsDataRegistry.Categories.Documents_Forwarding_Shipment_BillofLading,
				"Print Total Charges",
				"Override this registry to include the total of charges displayed on a house bill.\r\n\r\nThe total will not be displayed if charges are \"As Agreed\" or \"Print Charges as Lump Sum\" is applicable for the HBL destination country/region.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestIncludeCancelledInvoicesInDocumentPacks()
		{
			TestRegistryItem(
				ItemSet.IncludeCancelledInvoicesInDocumentPacks,
				"IncludeCancelledInvoicesInDocumentPacks",
				DocumentsDataRegistry.Categories.Documents,
				"Include Canceled Invoices In Document Packs",
				"This setting controls whether Canceled Invoices are included within Document Packs.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestFreightChargesConversionFactorDisplayOption()
		{
			AssertEquals("Default FreightChargesConversionFactorDisplayOption", "CON", ItemSet.FreightChargesConversionFactorDisplayOption.Value);
			ItemSet.FreightChargesConversionFactorDisplayOption.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "W/M");
			AssertEquals("FreightChargesConversionFactorDisplayOption", "W/M", ItemSet.FreightChargesConversionFactorDisplayOption.Value);
		}

		#region Warehouse Sub-Category

		public void TestWarehouseCartageAdviceOpeningText()
		{
			TestMultilingualRegistryItem(ItemSet.WarehouseCartageAdviceOpeningText, "WarehouseCartageAdviceOpeningText", "Documents/Warehouse/Cartage Advice", "Opening Text", "The Opening text that will be displayed at the header of every Warehouse Cartage Advice document.", RegistryStorageFlags.All, TextEditorType.Memo, string.Empty);
		}

		public void TestWarehouseCartageAdviceClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.WarehouseCartageAdviceClosingText, "WarehouseCartageAdviceClosingText", "Documents/Warehouse/Cartage Advice", "Closing Text", "The Closing text that will be displayed at the bottom of every Warehouse Cartage Advice document.", RegistryStorageFlags.All, TextEditorType.Memo, string.Empty);
		}

		#endregion

		#region Transport Booking Sub-Category

		public void TestTransportBookingExportCartageAdviceOpeningText()
		{
			TestMultilingualRegistryItem(ItemSet.TransportBookingCartageAdviceOpeningText,
				"TransportBookingCartageAdviceOpeningText",
				"Documents/Transport Booking/Cartage Advice",
				"Opening Text",
				"The Opening text that will be displayed at the header of every Transport Booking Cartage Advice document.",
				RegistryStorageFlags.All,
				TextEditorType.Memo,
				string.Empty);
		}

		public void TestTransportBookingExportCartageAdviceClosingText()
		{
			TestMultilingualRegistryItem(ItemSet.TransportBookingCartageAdviceClosingText,
				"TransportBookingCartageAdviceClosingText",
				"Documents/Transport Booking/Cartage Advice",
				"Closing Text",
				"The Closing text that will be displayed at the bottom of every Transport Booking Cartage Advice document.",
				RegistryStorageFlags.All,
				TextEditorType.Memo,
				string.Empty);
		}

		#endregion

		public void TestUseScheduledTaskDescriptionInEmailSubjectForScheduledReports()
		{
			string hint = @"This setting specifies what will be used in the email subject when delivering a Scheduled Report via email.
If enabled, the Scheduled Task Description will be used, otherwise the Report Name will be used in the email subject.";

			TestRegistryItem(
				ItemSet.UseScheduledTaskDescriptionInEmailSubjectForScheduledReports,
				"UseScheduledTaskDescriptionInEmailSubjectForScheduledReports",
				DocumentsDataRegistry.Categories.Documents,
				"Use Scheduled Task Description In Email Subject For Scheduled Reports",
				hint,
				RegistryStorageFlags.Company,
				false);
		}

		public void TestDocumentDeliveryDefaultLanguage()
		{
			var value = ItemSet.DocumentDeliveryDefaultLanguage.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertNotNull("DocumentDeliveryDefaultLanguage", value);
			AssertEquals("DocumentDeliveryDefaultLanguage", 0, value.Count);
		}

		public void TestEnableFormSupportforDocumentDeliveryCompletionActions()
		{
			TestRegistryItem(ItemSet.EnableFormSupportforDocumentDeliveryCompletionActions,
				"EnableFormSupportforDocumentDeliveryCompletionActions",
				DocumentsDataRegistry.Categories.Documents,
				"Enable Form Support for Document Delivery Completion Actions",
				"Set this to true to allow forms to be selected for the EDC and DOC document delivery types",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region Implementation

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableFormSupportforDocumentDeliveryCompletionActions";
				yield return "EnableNZCFTASubmissionToCAB";
				yield return "EnableCPTPPSubmissionToCAB";
				yield return "EnableRCEPSubmissionToCAB";
				yield return "CertOfOriginIndemnity";
				yield return "EnableAANZFTASubmissionToCAB";
				yield return "EnableCertOfOriginIndemnity";
				yield return "EnableCOONZSubmissionToCAB";
				yield return "EnableChAFTASubmissionToCAB";
				yield return "EnableJAEPASubmissionToCAB";
				yield return "EnableVerboseLoggingForCertification";
				yield return "EnableSubmissionToCustomsAuthority";
				yield return "EnableAUKFTASubmissionToCAB";
				yield return "EnableTAFTASubmissionToCAB";
				yield return "EnableIAECTASubmissionToCAB";
				yield return "EnableKAFTASubmissionToCAB";
				yield return "EnablePAFTASubmissionToCAB";
				yield return "EnableIACEPASubmissionToCAB";
				yield return "EnableCertOfOriginAUSubmissionToCAB";
				yield return "EnableCOOUSSubmissionToCAB";
			}
		}

		#endregion
	}
}
