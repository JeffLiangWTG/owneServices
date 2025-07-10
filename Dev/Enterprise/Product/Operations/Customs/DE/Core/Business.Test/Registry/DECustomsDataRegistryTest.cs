using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(DECustomsDataRegistry))]
	sealed class DECustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<DECustomsDataRegistry>
	{
		public void TestAllRegistryItemsHaveDECountryFilter()
		{
			CombineAssertions(() =>
			{
				foreach (IRegistryItem registryItem in AllItems)
				{
					Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Germany));
				}
			});
		}

		public void TestIsForProductivityWise()
		{
			AssertEquals(false, DECustomsDataRegistry.Instance.IsForProductivityWise);
		}

		public void TestCustomsMessageVersion()
		{
			TestGenericRegistryItem(ItemSet.CustomsMessageVersion,
						"CustomsMessageVersion",
						CustomsDataRegistry.Categories.Customs_Germany,
						"Customs Message Version",
						"Current version of customs messages the company is configured at customs to submit.",
						RegistryStorageFlags.Company,
						RegistryOptions.Default);

			AssertType(typeof(MessageVersionDataType), DECustomsDataRegistry.Instance.CustomsMessageVersion.DataType);
		}

		public void TestZNetRecipientIDProduction()
		{
			AssertZNetRecipientID(DatabaseTypes.Codes.Production, "DECustoms");
		}

		public void TestZNetRecipientIDTest()
		{
			AssertZNetRecipientID(DatabaseTypes.Codes.Test, "DECustomsTest");
		}

		void AssertZNetRecipientID(string databaseType, string defaultValue)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = databaseType;

			TestGenericRegistryItem(
				ItemSet.ZNetRecipientID,
				"ZNetRecipientID",
				CustomsDataRegistry.Categories.Customs_Germany,
				"Recipient ID",
				"Third Party Mailbox ID used for submitting messages to customs.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				defaultValue
				);
		}

		public void TestEMCSExciseTraderNumber()
		{
			TestStringRegistryItem(ItemSet.EMCSExciseTraderNumber,
				"DEEMCSExciseTraderNumber",
				DECustomsDataRegistry.Categories.Customs_Germany_EMCS,
				"Excise Trader Number",
				"This is the Default Excise Trader Number used for Sending EMCS Messages.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				string.Empty,
				CharacterCase.Upper,
				"DE12345678901");
			AssertType<ExciseTraderNumberStringRegistryDataType>(ItemSet.EMCSExciseTraderNumber.DataType);
		}

		public void TestEMCSParticipantIdentificationNumber()
		{
			TestStringRegistryItem(ItemSet.EMCSParticipantIdentificationNumber,
				"DEEMCSParticipantIdentificationNumber",
				DECustomsDataRegistry.Categories.Customs_Germany_EMCS,
				"Participant Identification Number",
				"This is the Default EMCS Participant Identification Number used for Sending EMCS Messages.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				string.Empty,
				CharacterCase.Normal,
				"1234567890123456789012345");
			AssertEquals(25, ((StringRegistryDataType)ItemSet.EMCSParticipantIdentificationNumber.DataType).MinLength);
			AssertEquals(25, ((StringRegistryDataType)ItemSet.EMCSParticipantIdentificationNumber.DataType).MaxLength);
		}

		public void TestATLASEORINumber()
		{
			TestStringRegistryItem(ItemSet.ATLASEORINumber,
				"DEATLASEORINumber",
				DECustomsDataRegistry.Categories.Customs_Germany_ATLAS,
				"EORI Number",
				"This is the Default EORI Number used for Sending ATLAS Messages.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				string.Empty,
				CharacterCase.Upper,
				"GR123456789012345");
			AssertType<EORINumberStringRegistryDataType>(ItemSet.ATLASEORINumber.DataType);
		}

		public void TestATLASEORIBranchSuffix()
		{
			TestStringRegistryItem(ItemSet.ATLASEORIBranchSuffix,
				"DEATLASEORIBranchSuffix",
				DECustomsDataRegistry.Categories.Customs_Germany_ATLAS,
				"EORI Branch Suffix",
				"This is the Default Branch Number used for Sending ATLAS Messages.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				string.Empty,
				CharacterCase.Normal,
				"1234");
			AssertEquals(4, ((StringRegistryDataType)ItemSet.ATLASEORIBranchSuffix.DataType).MinLength);
			AssertEquals(4, ((StringRegistryDataType)ItemSet.ATLASEORIBranchSuffix.DataType).MaxLength);
		}

		public void TestATLASParticipantIdentificationNumber()
		{
			TestStringRegistryItem(ItemSet.ATLASParticipantIdentificationNumber,
				"DEATLASParticipantIdentificationNumber",
				DECustomsDataRegistry.Categories.Customs_Germany_ATLAS,
				"Participant Identification Number",
				"This is the Default ATLAS Participant Identification Number used for Sending ATLAS Messages.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				string.Empty,
				CharacterCase.Normal,
				"1234567890123456789012345");
			AssertEquals(25, ((StringRegistryDataType)ItemSet.ATLASParticipantIdentificationNumber.DataType).MinLength);
			AssertEquals(25, ((StringRegistryDataType)ItemSet.ATLASParticipantIdentificationNumber.DataType).MaxLength);
		}

		public void TestExportStatusRequestRecipient()
		{
			TestGenericRegistryItem(ItemSet.ExportStatusRequestRecipient,
				"ExportStatusRequestRecipient",
				DECustomsDataRegistry.Categories.Customs_Germany,
				"Status Request Recipient",
				"This is the standard message recipient for Export and NCTS Status Request messages.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);

			AssertType<ExportStatusRequestRecipientDataType>(DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.DataType);
		}

		public void TestMonthlyClosingMaximumNumberOfLines()
		{
			TestRegistryItem(ItemSet.MonthlyClosingMaximumNumberOfLines,
				"MonthlyClosingMaximumNumberOfLines",
				DECustomsDataRegistry.Categories.Customs_Germany_MonthlyClosing,
				"Maximum Number of Lines",
				"This is the Default Maximum Number of Lines used for Monthly Closing Module.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				15000,
				1000,
				15000);
		}

		public void TestShowMonthlyClosing()
		{
			TestRegistryItem(ItemSet.ShowMonthlyClosing,
				"ShowMonthlyClosing",
				DECustomsDataRegistry.Categories.Customs_Germany_MonthlyClosing,
				"Show Monthly Closing Module",
				"Show Monthly Closing Module",
				RegistryStorageFlags.System,
				false);
		}

		public void TestMonthlyClosingJobNumberCustomization()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.MonthlyClosingJobNumberCustomization;
				AssertType<BillCustomisationRegistryItem>("Registry Type", item);
				var dataType = (ManifestJobNumberCustomisationRegistryDataType)item.DataType;
				AssertEquals("DataType - GeneratedNumberName", "Monthly Closing Job Number", dataType.GeneratedNumberName);

				TestGenericRegistryItem(item,
					"MonthlyClosingJobNumberCustomization",
					DECustomsDataRegistry.Categories.Customs_Germany_MonthlyClosing,
					"Job Number Customization",
					"Override this value to customize how Monthly Closing Job Numbers are formatted",
					RegistryStorageFlags.System);
			});
		}

		public void TestSendImportMessageErrors()
		{
			var registryItem = DECustomsDataRegistry.Instance.SendImportMessageErrors;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<ImportGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendImportMessageErrorsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
					"Send Import Errors To",
					"Send Import message errors to staff member, nominated group or both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
			});
		}

		public void TestSendImportAcknowledgements()
		{
			var registryItem = DECustomsDataRegistry.Instance.SendImportAcknowledgements;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<ImportGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendImportAcknowledgementsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
					"Send Import Acknowledgements To",
					"Send Import acknowledgements to staff member, nominated group or both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
			});
		}

		public void TestSendTaxChangeAcknowledgements()
		{
			var registryItem = DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements;
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(registryItem,
				"SendTaxChangeAcknowledgements",
				DECustomsDataRegistry.Categories.Customs_Germany_TaxChangeAssessment,
				"Send Tax Change Acknowledgements To",
				"Send acknowledgements to nominated group",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);

				AssertType<SendAcknowledgementsRegistryDataType>("Type", registryItem.DataType);
			});
		}

		public void TestBondedWarehouseCreateDeclarationFromInventory()
		{
			TestRegistryItem(ItemSet.BondedWarehouseCreateDeclarationFromInventory,
				"CreateDeclarationFromInventory",
				DECustomsDataRegistry.Categories.Customs_Germany_BondedWarehouse,
				"Create Declaration from Inventory",
				"Enable Create Declaration from Inventory Functionality",
				RegistryStorageFlags.System,
				false);
		}

		public void TestBondedWarehouseCreateDeclarationFromWarehouseOrder()
		{
			TestRegistryItem(ItemSet.BondedWarehouseCreateDeclarationFromWarehouseOrder,
				"CreateDeclarationFromWarehouseOrder",
				DECustomsDataRegistry.Categories.Customs_Germany_BondedWarehouse,
				"Create Declaration from Warehouse Order",
				"Enable Create Declaration from Warehouse Order Functionality",
				RegistryStorageFlags.System,
				false);
		}

		public void TestZELOSShowModule()
		{
			var registryItem = DECustomsDataRegistry.Instance.ShowZELOSModule;
			CombineAssertions(() =>
			{
				TestRegistryItem(registryItem,
					"ShowZELOSModule",
					DECustomsDataRegistry.Categories.Customs_Germany_ZELOS,
					"Show ZELOS Module",
					"Show ZELOS Module",
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForDevelopers,
					false);
			});
		}

		public void TestZELOSSendAcknowledgementsTo()
		{
			var registryItem = DECustomsDataRegistry.Instance.ZELOSSendAcknowlementsTo;
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(registryItem,
					"ZELOSSendAcknowlementsTo",
					DECustomsDataRegistry.Categories.Customs_Germany_ZELOS,
					"Send Acknowledgements To",
					"Send Acknowledgements to staff member, nominated group or both",
					RegistryStorageFlags.Company,
					RegistryOptions.Default);
				AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
				AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			});
		}

		public void TestZELOSSendErrorsTo()
		{
			var registryItem = DECustomsDataRegistry.Instance.ZELOSSendErrorsTo;
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(registryItem,
					"ZELOSSendErrorsTo",
					DECustomsDataRegistry.Categories.Customs_Germany_ZELOS,
					"Send Errors To",
					"Send Errors to staff member, nominated group or both",
					RegistryStorageFlags.Company,
					RegistryOptions.Default);
				AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
				AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			});
		}

		void AssertDefaultValues(ImportGroupNotification defaultValue)
		{
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, defaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, defaultValue.SendGroupPK);
		}
	}
}
