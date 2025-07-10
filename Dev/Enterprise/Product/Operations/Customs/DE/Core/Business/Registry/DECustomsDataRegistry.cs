using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.DE;
using ResString = Enterprise.Customs.DE.Business.ResString;

namespace Enterprise.Customs.DE.Registry
{
	public sealed class DECustomsDataRegistry : RegistryItemSet, IDECustomsRegistry
	{
		#region Construction

		public static DECustomsDataRegistry Instance => instance ?? (instance = new DECustomsDataRegistry());

		[ThreadStatic]
		static DECustomsDataRegistry instance;

		DECustomsDataRegistry()
		{
		}

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Germany;
		}

		#endregion

		public class Categories : CustomsDataRegistry.Categories
		{
			public static MultilingualString Customs_Germany_EMCS => CombineCategories(Customs_Germany, ResString.GetMultilingualString("62C6D720-E8D0-4A7A-80E0-2994556232C8", "EMCS"));
			public static MultilingualString Customs_Germany_ATLAS => CombineCategories(Customs_Germany, ResString.GetMultilingualString("FEC3E353-2031-41FD-91B2-349350ED6187", "ATLAS"));
			public static MultilingualString Customs_Germany_MonthlyClosing => CombineCategories(Customs_Germany, ResString.GetMultilingualString("B8AFD79A-1C19-4D5F-A7DD-304C7A767402", "Monthly Closing"));
			public static MultilingualString Customs_Germany_TaxChangeAssessment => CombineCategories(Customs_Germany, ResString.GetMultilingualString("FFEA3C9A-EEEB-4E88-B647-DBA47FBD048C", "Tax Change Assessment"));
			public static MultilingualString Customs_Germany_BondedWarehouse => CombineCategories(Customs_Germany, ResString.GetMultilingualString("B83606E4-A81C-4E21-BB77-9A30B30414FD", "Bonded Warehouse"));
			public static MultilingualString Customs_Germany_ZELOS => CombineCategories(Customs_Germany, ResString.GetMultilingualString("A4F30779-EE68-4048-B404-FAEBF4C4782D", "ZELOS"));
		}

		public override bool IsForProductivityWise => false;

		public MessageVersionRegistryItem CustomsMessageVersion => GetItem("CustomsMessageVersion", () => new MessageVersionRegistryItem(
			"CustomsMessageVersion",
			CustomsDataRegistry.Categories.Customs_Germany,
			ResString.GetMultilingualString("94CE4868-FCC1-4E91-88D1-2D13E1E6E7F5", "Customs Message Version"),
			ResString.GetMultilingualString("D7C8F0B3-C0BF-4133-BD9F-EECAA946E0A9", "Current version of customs messages the company is configured at customs to submit."),
			RegistryStorageFlags.Company,
			new MessageVersionRegistryCollection().DefaultCollection));

		public StringRegistryItem ZNetRecipientID
		{
			get
			{
				return GetItem("ZNetRecipientID", delegate
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					var isProductionSystem = registrationKey.DatabaseType == DatabaseTypes.Codes.Production;

					return new StringRegistryItem(
						"ZNetRecipientID",
						CustomsDataRegistry.Categories.Customs_Germany,
						ResString.GetMultilingualString("7D0715B9-C9A8-4BCE-AE3B-C266D6B36E0B", "Recipient ID"),
						ResString.GetMultilingualString("EBBEE0AA-C617-465F-930B-424F2A5DBC07", "Third Party Mailbox ID used for submitting messages to customs."),
						new StringRegistryDataType(CharacterCase.Upper),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						isProductionSystem ? "DECustoms" : "DECustomsTest"
					);
				});
			}
		}

		public ExportStatusRequestRecipientRegistryItem ExportStatusRequestRecipient => GetItem("ExportStatusRequestRecipient", () => new ExportStatusRequestRecipientRegistryItem(
			"ExportStatusRequestRecipient",
			CustomsDataRegistry.Categories.Customs_Germany,
			ResString.GetMultilingualString("214967F3-822F-49F9-AA9A-5791D3FCC97B", "Status Request Recipient"),
			ResString.GetMultilingualString("132BA4CC-3960-41EF-ADA0-9CE468939E55", "This is the standard message recipient for Export and NCTS Status Request messages."),
			RegistryStorageFlags.Company,
			new ExportStatusRequestRecipientRegistryCollection().DefaultCollection
		));

		public StringRegistryItem EMCSExciseTraderNumber => GetItem("DEEMCSExciseTraderNumber", () => new StringRegistryItem(
			"DEEMCSExciseTraderNumber",
			Categories.Customs_Germany_EMCS,
			ResString.GetMultilingualString("B0D21A25-0BC8-4665-9BCA-F2AFA4F2DBD0", "Excise Trader Number"),
			ResString.GetMultilingualString("8D2EA6D4-47DE-4BAA-AB61-B6D52933EA45", "This is the Default Excise Trader Number used for Sending EMCS Messages."),
			new ExciseTraderNumberStringRegistryDataType(),
			RegistryStorageFlags.Company,
			RegistryOptions.Default
		));

		public StringRegistryItem EMCSParticipantIdentificationNumber => GetItem("DEEMCSParticipantIdentificationNumber", () => new StringRegistryItem(
			"DEEMCSParticipantIdentificationNumber",
			Categories.Customs_Germany_EMCS,
			ResString.GetMultilingualString("F8276CAB-FA67-4C9F-9ED5-8A6FEBF54DF6", "Participant Identification Number"),
			ResString.GetMultilingualString("F4AC2FBA-06E4-4887-AACE-03957763FD3E", "This is the Default EMCS Participant Identification Number used for Sending EMCS Messages."),
			new NumericOnlyStringRegistryDataType { MaxLength = 25, MinLength = 25 },
			RegistryStorageFlags.Company,
			RegistryOptions.Default
		));

		public StringRegistryItem ATLASEORINumber => GetItem("DEATLASEORINumber", () => new StringRegistryItem(
			"DEATLASEORINumber",
			Categories.Customs_Germany_ATLAS,
			ResString.GetMultilingualString("8E6A0F67-7A92-486C-98CC-C187A7C09403", "EORI Number"),
			ResString.GetMultilingualString("3A1B059E-B79E-4275-9659-5B6F51D6B8A6", "This is the Default EORI Number used for Sending ATLAS Messages."),
			new EORINumberStringRegistryDataType(),
			RegistryStorageFlags.Company,
			RegistryOptions.Default
		));

		public StringRegistryItem ATLASEORIBranchSuffix => GetItem("DEATLASEORIBranchSuffix", () => new StringRegistryItem(
			"DEATLASEORIBranchSuffix",
			Categories.Customs_Germany_ATLAS,
			ResString.GetMultilingualString("8F933E3C-C0B3-48EF-8F7C-9ABECF121D5A", "EORI Branch Suffix"),
			ResString.GetMultilingualString("660E566A-7E5B-4611-B04A-E181292A7BCD", "This is the Default Branch Number used for Sending ATLAS Messages."),
			new NumericOnlyStringRegistryDataType { MaxLength = 4, MinLength = 4 },
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.Default
		));

		public StringRegistryItem ATLASParticipantIdentificationNumber => GetItem("DEATLASParticipantIdentificationNumber", () => new StringRegistryItem(
			"DEATLASParticipantIdentificationNumber",
			Categories.Customs_Germany_ATLAS,
			ResString.GetMultilingualString("8E909AB7-5C24-4D94-A40D-F0D1AC7813E3", "Participant Identification Number"),
			ResString.GetMultilingualString("FF6306E9-6FB1-4E0C-A564-5AD81B3DC51E", "This is the Default ATLAS Participant Identification Number used for Sending ATLAS Messages."),
			new NumericOnlyStringRegistryDataType { MaxLength = 25, MinLength = 25 },
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.Default
		));

		public IntRegistryItem MonthlyClosingMaximumNumberOfLines => GetItem("MonthlyClosingMaximumNumberOfLines", () => new IntRegistryItem(
			"MonthlyClosingMaximumNumberOfLines",
			Categories.Customs_Germany_MonthlyClosing,
			ResString.GetMultilingualString("C8DA25ED-3AC4-427D-9EE3-67ADB5C68ECB", "Maximum Number of Lines"),
			ResString.GetMultilingualString("1C588105-4A82-405E-B7B8-4656995783FD", "This is the Default Maximum Number of Lines used for Monthly Closing Module."),
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			15000,
			1000,
			15000
		));

		public BooleanRegistryItem ShowMonthlyClosing => GetItem("ShowMonthlyClosing", () => new BooleanRegistryItem(
			"ShowMonthlyClosing",
			Categories.Customs_Germany_MonthlyClosing,
			ResString.GetMultilingualString("22C6FEAB-25E3-4F79-9ADE-164546949B05", "Show Monthly Closing Module"),
			ResString.GetMultilingualString("7B509D8D-1528-4D5F-96F5-D3ECCB89E1E8", "Show Monthly Closing Module"),
			RegistryStorageFlags.System,
			false
		));

		IRegistryItem IDECustomsRegistry.ShowMonthlyClosing => ShowMonthlyClosing;

		public BillCustomisationRegistryItem MonthlyClosingJobNumberCustomization => GetItem("MonthlyClosingJobNumberCustomization", () => new BillCustomisationRegistryItem(
			"MonthlyClosingJobNumberCustomization",
			Categories.Customs_Germany_MonthlyClosing,
			ResString.GetMultilingualString("852C50A4-D38A-48CE-B526-B71A14841F7A", "Job Number Customization"),
			ResString.GetMultilingualString("8FBBA607-9A75-4EF0-88D5-30F65D1D78EE", "Override this value to customize how Monthly Closing Job Numbers are formatted"),
			RegistryStorageFlags.System,
			new ManifestJobNumberCustomisationRegistryDataType { GeneratedNumberName = ResString.GetMultilingualString("9CA27E92-1794-4EA7-950A-3118E64B5472", "Monthly Closing Job Number") }
		));

		public GroupNotificationRegistryItem<ImportGroupNotification> SendImportMessageErrors
		{
			get
			{
				return GetItem("SendImportMessageErrorsRegistry", () =>
				{
					return new GroupNotificationRegistryItem<ImportGroupNotification>("SendImportMessageErrorsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
						ResString.GetMultilingualString("58BC49A0-1DA0-47BF-A09F-5E9067007D5D", "Send Import Errors To"),
						ResString.GetMultilingualString("686E9682-AF23-419F-9AB3-9C0F3AAE2848", "Send Import message errors to staff member, nominated group or both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new ImportGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
				});
			}
		}

		public GroupNotificationRegistryItem<ImportGroupNotification> SendImportAcknowledgements
		{
			get
			{
				return GetItem("SendImportAcknowledgementsRegistry", () =>
				{
					return new GroupNotificationRegistryItem<ImportGroupNotification>("SendImportAcknowledgementsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
						ResString.GetMultilingualString("2DAFDED5-8F64-40A3-AD26-5B07B201A1D9", "Send Import Acknowledgements To"),
						ResString.GetMultilingualString("2B7EEEE7-AD13-4E79-8850-E8F16EB1D1EA", "Send Import acknowledgements to staff member, nominated group or both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new ImportGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
				});
			}
		}

		public SendAcknowledgementsRegistryItem SendTaxChangeAcknowledgements => GetItem("SendTaxChangeAcknowledgements", () => new SendAcknowledgementsRegistryItem(
			"SendTaxChangeAcknowledgements",
			Categories.Customs_Germany_TaxChangeAssessment,
			ResString.GetMultilingualString("A8DDBA55-D5A2-41C9-8B94-5ACF9DD94211", "Send Tax Change Acknowledgements To"),
			ResString.GetMultilingualString("596A7A9B-73AB-473D-A4C3-AB8AA6129DB3", "Send acknowledgements to nominated group"),
			RegistryStorageFlags.Company
		));

		public BooleanRegistryItem BondedWarehouseCreateDeclarationFromInventory => GetItem("CreateDeclarationFromInventory", () => new BooleanRegistryItem(
			"CreateDeclarationFromInventory",
			Categories.Customs_Germany_BondedWarehouse,
			ResString.GetMultilingualString("00995E76-0988-4D41-86A0-9043F4B206F5", "Create Declaration from Inventory"),
			ResString.GetMultilingualString("C494C2BD-07D5-4019-9B21-4A0585E31DD6", "Enable Create Declaration from Inventory Functionality"),
			RegistryStorageFlags.System,
			false
		));

		public BooleanRegistryItem BondedWarehouseCreateDeclarationFromWarehouseOrder => GetItem("CreateDeclarationFromWarehouseOrder", () => new BooleanRegistryItem(
			"CreateDeclarationFromWarehouseOrder",
			Categories.Customs_Germany_BondedWarehouse,
			ResString.GetMultilingualString("37C7A65B-D5E7-4574-9B14-866A819F0F49", "Create Declaration from Warehouse Order"),
			ResString.GetMultilingualString("8E144299-C4D9-4E8F-A4DF-19EA620E61E3", "Enable Create Declaration from Warehouse Order Functionality"),
			RegistryStorageFlags.System,
			false
		));

		public BooleanRegistryItem ShowZELOSModule => GetItem("ShowZELOSModule", () => new BooleanRegistryItem(
			"ShowZELOSModule",
			Categories.Customs_Germany_ZELOS,
			ResString.GetMultilingualString("709A76F1-4FBA-46A4-8810-2E0BAAE686A3", "Show ZELOS Module"),
			ResString.GetMultilingualString("3A5AF141-9E64-44EE-88CD-51524B4F3707", "Show ZELOS Module"),
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false
		));

		public GroupNotificationRegistryItem<TemporaryStorageGroupNotification> ZELOSSendAcknowlementsTo =>
			GetItem("ZELOSSendAcknowlementsTo", () => new GroupNotificationRegistryItem<TemporaryStorageGroupNotification>(
					"ZELOSSendAcknowlementsTo",
					Categories.Customs_Germany_ZELOS,
					ResString.GetMultilingualString("D1FCB7C6-CE39-4A2E-A3D6-F2A7581AC3E8", "Send Acknowledgements To"),
					ResString.GetMultilingualString("FCF9624C-6817-44F3-9A38-147B5BF3CA97", "Send Acknowledgements to staff member, nominated group or both"),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					new TemporaryStorageGroupNotification(Enterprise.Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
			));

		public GroupNotificationRegistryItem<TemporaryStorageGroupNotification> ZELOSSendErrorsTo =>
			GetItem("ZELOSSendErrorsTo", () => new GroupNotificationRegistryItem<TemporaryStorageGroupNotification>(
					"ZELOSSendErrorsTo",
					Categories.Customs_Germany_ZELOS,
					ResString.GetMultilingualString("AD160280-66DC-44E9-B5C8-4F1379C5848E", "Send Errors To"),
					ResString.GetMultilingualString("F9464560-4C24-4F3E-9997-00B68FEC944F", "Send Errors to staff member, nominated group or both"),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					new TemporaryStorageGroupNotification(Enterprise.Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
			));
	}
}
