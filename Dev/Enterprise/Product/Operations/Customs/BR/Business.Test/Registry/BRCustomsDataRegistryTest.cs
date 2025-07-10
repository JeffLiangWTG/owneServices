using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.BR.Registry.BRCustomsDataRegistry;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Registry.Testing
{
	[TestedType(typeof(BRCustomsDataRegistry))]
	sealed class BRCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<BRCustomsDataRegistry>
	{
		public void TestBRShouldLoadTariffAttributesOfTestEnvironmentIsTesting()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			TestRegistryItem(ItemSet.ShouldLoadTariffAttributesOfTestEnvironment,
				"BRShouldLoadTariffAttributesOfTestEnvironment",
				Categories.Customs_Brazil,
				"Tariff Attributes Mode",
				@"This registry is used for selecting to which reference file (usually attributes) will be loaded in customs declarations and other modules.

By default, this registry value is set to the database instance of the license type. BE AWARE that if you change it to a value that does not match with ""Connection To XT Server (Production/Test/Local)"" your declaration might be created using the reference file selected, but MESSAGES MIGHT BE REJECTED BECAUSE THE REFERENCE FILE CODES DOES NOT MATCH TO THE ENVIRONMENT THAT WILL RECEIVE THE MESSAGE.

Should CargoWise load Test reference file rather than Production reference file?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestBRShouldLoadTariffAttributesOfTestEnvironmentIsProduction()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			TestRegistryItem(ItemSet.ShouldLoadTariffAttributesOfTestEnvironment,
				"BRShouldLoadTariffAttributesOfTestEnvironment",
				Categories.Customs_Brazil,
				"Tariff Attributes Mode",
				@"This registry is used for selecting to which reference file (usually attributes) will be loaded in customs declarations and other modules.

By default, this registry value is set to the database instance of the license type. BE AWARE that if you change it to a value that does not match with ""Connection To XT Server (Production/Test/Local)"" your declaration might be created using the reference file selected, but MESSAGES MIGHT BE REJECTED BECAUSE THE REFERENCE FILE CODES DOES NOT MATCH TO THE ENVIRONMENT THAT WILL RECEIVE THE MESSAGE.

Should CargoWise load Test reference file rather than Production reference file?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestBREnableUCRManifest()
		{
			TestRegistryItem(ItemSet.EnableUCRManifest, "IsBREnableUCRManifest", Categories.Customs_Brazil, "Enable UCR Manifest", "Should enable Brazil UCR Manifest creation?", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestIsTestLicence()
		{
			var registry = new BRCustomsDataRegistry();
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			Assert(!registry.IsTestLicence);
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			Assert(registry.IsTestLicence);
		}

		public void TestEnableBRLPCO()
		{
			TestRegistryItem(ItemSet.EnableLPCO, "EnableBRLPCO", Categories.Customs_Brazil_LPCO, "Enable LPCO", "Should enable Brazil LPCO creation?", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableBRForeignOperator()
		{
			TestRegistryItem(ItemSet.EnableForeignOperator, "EnableBRForeignOperator", Categories.Customs_Brazil_ForeignOperator, "Enable Register or Amend Foreign Operator", "Should enable Register or Amend a Foreign Operator?", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableBRImportLicense()
		{
			TestRegistryItem(ItemSet.EnableImportLicense, "EnableBRImportLicense", Categories.Customs_Brazil_ImportLicense, "Enable Import License", "Enable Import License", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableBRImportSiscomex()
		{
			TestRegistryItem(ItemSet.EnableImportSiscomex, "EnableBRImportSiscomex", Categories.Customs_Brazil_ImportSiscomex, "Enable Import (SISCOMEX Web)", "Enable Import (SISCOMEX Web)", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestBRTaxFeeCustomsPaymentBankAccount()
		{
			var accountPK = Guid.NewGuid();

			var registry = new BRCustomsDataRegistry();
			registry.TaxFeeCustomsPaymentBankAccount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, accountPK);
			AssertEquals("BRTaxFeeCustomsPaymentBankAccount", accountPK, registry.TaxFeeCustomsPaymentBankAccount.Value);
			AssertEquals("CountryFilterPKs", CountryFilterPKs.Brazil, registry.TaxFeeCustomsPaymentBankAccount.CountryFilterPKs);

			TestRegistryItem(ItemSet.TaxFeeCustomsPaymentBankAccount, "BRTaxFeeCustomsPaymentBankAccount", Categories.Customs_Brazil_ImportSiscomex, "Tax and Fee Payment Bank Account", "This Bank Account is used for payment taxes and fees related  to message sent to customs.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryFindBoxCollection.AccBankAccount, Guid.Empty);
		}

		public void TestEnableBRUpdateEntryStatusViaUniversalEventXML()
		{
			TestRegistryItem(ItemSet.EnableUpdateEntryStatusViaUniversalEventXML_ISW, "BREnableUpdateEntryStatusViaUniversalEventXML_ISW", Categories.Customs_Brazil_ImportSiscomex, "Update Entry Status via Universal Event XML (BLT Submit Type)", "Update Entry Status via Universal Event XML (BLT Submit Type)", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestEnableBRUpdateEntryStatusViaUniversalEventXMLImportLicense()
		{
			TestRegistryItem(ItemSet.EnableUpdateEntryStatusViaUniversalEventXML_LIC, "BREnableUpdateEntryStatusViaUniversalEventXML_LIC", Categories.Customs_Brazil_ImportLicense, "Update Entry Status via Universal Event XML (BLT Submit Type)", "Update Entry Status via Universal Event XML (BLT Submit Type)", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestBRSendExportDeclarationErrors()
		{
			TestRegistryItem(ItemSet.SendExportDeclarationErrorsTo, "BRSendExportDeclarationErrorsTo", Categories.Customs_Brazil_Export, "Send Export Declaration Errors", "Indicate the action to perform when failed message is detected.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.StaffMember);
		}

		public void TestBRGroupToSendExportDeclarationErrors()
		{
			var groupPK = Guid.NewGuid();

			var registry = new BRCustomsDataRegistry();
			registry.SendExportDeclarationErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("SendExportDeclarationErrorsToGroup", groupPK, registry.SendExportDeclarationErrorsToGroup.Value);
			AssertEquals("CountryFilterPKs", CountryFilterPKs.Brazil, registry.SendExportDeclarationErrorsToGroup.CountryFilterPKs);

			TestRegistryItem(ItemSet.SendExportDeclarationErrorsToGroup, "BRSendExportDeclarationErrorsToGroup", Categories.Customs_Brazil_Export, "Group To Send Export Declaration Errors To", "Indicate the email address/Group to deliver the error details in Export Declaration.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryFindBoxCollection.GlbGroup, Guid.Empty);
		}

		public void TestBREnableMercanteSystem()
		{
			TestRegistryItem(ItemSet.EnableMercanteSystem, "BREnableMercanteSystem", Categories.Customs_Brazil, "Enable Mercante System", "Enable Mercante System?", RegistryStorageFlags.Company, RegistryOptions.Default, false);
		}

		public void TestIsBREnableMercanteSystem()
		{
			Instance.EnableMercanteSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(Instance.IsMercanteSystemEnabled);

			Instance.EnableMercanteSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!Instance.IsMercanteSystemEnabled);
		}

		public void TestBRXTEndPointAddress_Test()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			TestGenericRegistryItem(ItemSet.XTEndPointAddress,
									"BRXTEndPointAddress",
									Categories.Customs_Brazil,
									"xT Endpoint Address",
									"The default value uses the xT test endpoint address.",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport,
									"https://xttest-customs-brpushnotification.wisegrid.net/BRC");
		}

		public void TestBRXTEndPointAddress_Prod()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			TestGenericRegistryItem(ItemSet.XTEndPointAddress,
									"BRXTEndPointAddress",
									Categories.Customs_Brazil,
									"xT Endpoint Address",
									"The default value uses the xT test endpoint address.",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport,
									"https://xt-customs-brpushnotification.wisegrid.net/BRC");
		}

		public void TestBRSendLPCOErrorsToGroup()
		{
			var groupPK = Guid.NewGuid();

			var registry = new BRCustomsDataRegistry();
			registry.SendLPCOErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("SendLPCOErrorsToGroup", groupPK, registry.SendLPCOErrorsToGroup.Value);
			AssertEquals("CountryFilterPKs", CountryFilterPKs.Brazil, registry.SendLPCOErrorsToGroup.CountryFilterPKs);

			TestRegistryItem(ItemSet.SendLPCOErrorsToGroup,
							"BRSendLPCOErrorsToGroup",
							Categories.Customs_Brazil_LPCO,
							"Group To Send LPCO Errors To",
							"Indicate the email address/Group to deliver the error details in LPCO.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryFindBoxCollection.GlbGroup, Guid.Empty);
		}

		public void TestSendLPCOErrorsTo()
		{
			TestRegistryItem(ItemSet.SendLPCOErrorsTo, "BRSendLPCOErrorsTo", Categories.Customs_Brazil_LPCO, "Send LPCO Errors", "Indicate the action to perform when failed message is detected.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.StaffMember);
		}

		public void TestSendImportDeclarationErrorsToGroup()
		{
			var groupPK = Guid.NewGuid();

			var registry = new BRCustomsDataRegistry();
			registry.SendImportDeclarationErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("SendImportDeclarationErrorsToGroup", groupPK, registry.SendImportDeclarationErrorsToGroup.Value);
			AssertEquals("CountryFilterPKs", CountryFilterPKs.Brazil, registry.SendImportDeclarationErrorsToGroup.CountryFilterPKs);

			TestRegistryItem(ItemSet.SendImportDeclarationErrorsToGroup,
							"BRSendImportDeclarationErrorsToGroup",
							Categories.Customs_Brazil_Import,
							"Group To Send Import Declaration Errors To",
							"Indicate the email address/Group to deliver the error details in Import Declaration.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryFindBoxCollection.GlbGroup, Guid.Empty);
		}

		public void TestSendImportDeclarationErrorsTo()
		{
			TestRegistryItem(ItemSet.SendImportDeclarationErrorsTo, "BRSendImportDeclarationErrorsTo", Categories.Customs_Brazil_Import, "Send Import Declaration Errors", "Indicate the action to perform when failed message is detected.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.StaffMember);
		}

		public void TestSendProductCatalogErrorsToGroup()
		{
			var groupPK = Guid.NewGuid();

			var registry = new BRCustomsDataRegistry();
			registry.SendProductCatalogErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("SendProductCatalogErrorsToGroup", groupPK, registry.SendProductCatalogErrorsToGroup.Value);
			AssertEquals("CountryFilterPKs", CountryFilterPKs.Brazil, registry.SendProductCatalogErrorsToGroup.CountryFilterPKs);

			TestRegistryItem(ItemSet.SendProductCatalogErrorsToGroup,
							"BRSendProductCatalogErrorsToGroup",
							Categories.Customs_Brazil_ProductCatalog,
							"Group To Send Product Catalog Errors To",
							"Indicate the email address/Group to deliver the error details in Product Catalog.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryFindBoxCollection.GlbGroup, Guid.Empty);
		}

		public void TestSendProductCatalogErrorsTo()
		{
			TestRegistryItem(ItemSet.SendProductCatalogErrorsTo, "BRSendProductCatalogErrorsTo", Categories.Customs_Brazil_ProductCatalog, "Send Product Catalog Errors", "Indicate the action to perform when failed message is detected.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.StaffMember);
		}

		public void TestSendSubscriptionErrorsTo()
		{
			TestRegistryItem(ItemSet.SendSubscriptionErrorsTo, "BRSendSubscriptionErrorsTo", Categories.Customs_Brazil_Push, "Send Subscription Errors", "Indicate the action to perform when failed message is detected.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.StaffMember);
		}

		public void TestSendSubscriptionErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendSubscriptionErrorsToGroup, "BRSendSubscriptionErrorsToGroup", Categories.Customs_Brazil_Push, "Group To Send Subscription Errors To", "Indicate the email address/Group to deliver the error details in Subscription.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryFindBoxCollection.GlbGroup, Guid.Empty);
		}

		public void TestEnableCatalogSystem()
		{
			TestRegistryItem(ItemSet.EnableCatalogModule, "BREnableCatalogModule", Categories.Customs_Brazil_ProductCatalog, "Enable Catalog", "Enable Catalog Module?", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestBRMaxNumberOfRowsInForeignOperatorMessage()
		{
			TestRegistryItem(ItemSet.MaxNumberOfRowsInForeignOperatorMessage, "BRMaxNumberOfRowsInForeignOperatorMessage", Categories.Customs_Brazil_ForeignOperator, "Maximum Number of Messages per Interchange", "Override the Default Value between 1 and 999", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, 100, 1, 999);
		}

		public void TestBRMaxNumberOfEntryLineInDuimpMessage()
		{
			TestRegistryItem(ItemSet.MaxNumberOfEntryLineInDuimpMessage, "BRMaxNumberOfEntryLineInDuimpMessage", Categories.Customs_Brazil_Import, "Maximum Number of Entry Lines in the message", "Override the Default Value between 1 and 999", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, 100, 1, 999);
		}

		public void TestBRMaxNumberOfRowsInProductCatalogMessage()
		{
			TestRegistryItem(ItemSet.MaxNumberOfRowsInProductCatalogMessage, "BRMaxNumberOfRowsInProductCatalogMessage", Categories.Customs_Brazil_ProductCatalog, "Maximum Number of Messages per Interchange", "Override the Default Value between 1 and 999", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, 100, 1, 999);
		}

		public void TestCatalogCodeCustomization()
		{
			TestGenericRegistryItem(
				ItemSet.CatalogCodeCustomization,
				"BRCatalogCodeCustomization",
				Categories.Customs_Brazil_ProductCatalog,
				"Catalog Code Customization",
				"Override this Registry Item to customize how the Catalog Codes are formatted.",
				RegistryStorageFlags.All);

			AssertType<UniqueNumberCustomisationRegistryDataType>(ItemSet.CatalogCodeCustomization.DataType);
			var dataType = ItemSet.CatalogCodeCustomization.DataType as UniqueNumberCustomisationRegistryDataType;
			AssertEquals("SupportsDirection", true, dataType.SupportsDirection);
			AssertEquals("GeneratedNumberName", "Catalog Code", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 50, dataType.MaxLength);
		}

		public void TestLPCOJobNumberCustomization()
		{
			TestGenericRegistryItem(
				ItemSet.LPCOJobNumberCustomization,
				"BRLPCOJobNumberCustomization",
				Categories.Customs_Brazil_LPCO,
				"LPCO Job Number Customization",
				"Override this Registry Item to customize how the LPCO Job Numbers are formatted.",
				RegistryStorageFlags.All);

			AssertType<UniqueNumberCustomisationRegistryDataType>(ItemSet.LPCOJobNumberCustomization.DataType);
			var dataType = ItemSet.LPCOJobNumberCustomization.DataType as UniqueNumberCustomisationRegistryDataType;
			AssertEquals("SupportsDirection", false, dataType.SupportsDirection);
			AssertEquals("GeneratedNumberName", "LPCO Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}
	}
}
