using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.IT.Business.ResString;

namespace Enterprise.Customs.IT.Registry;

public sealed class ITCustomsDataRegistry : RegistryItemSet
{
	public static ITCustomsDataRegistry Instance
	{
		get { return instance ?? (instance = new ITCustomsDataRegistry()); }
	}

	[ThreadStatic]
	static ITCustomsDataRegistry instance;

	ITCustomsDataRegistry()
	{
	}

	public override bool IsForProductivityWise => false;

	public StringRegistryItem ITRecipientID
	{
		get
		{
			return GetItem("ITRecipientID", delegate
			{
				return new StringRegistryItem(
					"ITRecipientID",
					CustomsDataRegistry.Categories.Customs_Italy,
					ResString.GetMultilingualString("32AFD279-40DA-461D-8CA0-DECD9526C299", "Recipient ID"),
					ResString.GetMultilingualString("D2AD5197-EC48-4BE6-BD51-CABE9E2F605C", "Recipient ID"),
					RegistryStorageFlags.System,
					RegistryOptions.IsValueMandatory,
					"ITCustomsTest");
			});
		}
	}

	public BooleanRegistryItem ITGeneratePortTaxes
	{
		get
		{
			return GetItem("ITGeneratePortTaxes", delegate
			{
				return new BooleanRegistryItem(
					"ITGeneratePortTaxes",
					CustomsDataRegistry.Categories.Customs_Italy,
					ResString.GetMultilingualString("CEC2F105-E274-4050-841D-769841FF5DC7", "Generate Port Taxes"),
					ResString.GetMultilingualString("9C406A02-0509-4E34-B531-89E03BADCA7C", "Generate Port Taxes"),
					RegistryStorageFlags.Company,
					RegistryOptions.IsValueMandatory,
					true);
			});
		}
	}

	public ZBool IsPortTaxesCalculationEnabled => ITGeneratePortTaxes.Value;

	public CodePairRegistryItem ExportMessageVersion
	{
		get
		{
			return GetItem("ExportMessageVersion", delegate
			{
				return new CodePairRegistryItem(
					"ExportMessageVersion",
					CustomsDataRegistry.Categories.Customs_Italy,
					ResString.GetMultilingualString("A3898740-7E27-47FD-8E6D-872BACE373BA", "Export Message Version"),
					ResString.GetMultilingualString("968A7DFE-4D0B-4771-B5BD-C4B27371F82C", "The field below allows to choose the Message Version for Export declarations."),
					new CodeDescriptionPairListProvider(() => new ExportMessageVersionList()),
					RegistryStorageFlags.Company,
					ExportMessageVersionList.Codes.TXT);
			});
		}
	}

	public ZBool IsExportMessageVersionEnabled => ExportMessageVersion.Value == ExportMessageVersionList.Codes.BTX || ExportMessageVersion.Value == ExportMessageVersionList.Codes.BXM;

	public GuidRegistryItem BondedWarehouseNotificationGroup
	{
		get
		{
			return GetItem("ITBondedWarehouseNotificationGroup", () => new GuidRegistryItem(
				"ITBondedWarehouseNotificationGroup",
				ITCustomsDataRegistry.Categories.Customs_Italy_Notifications,
				ResString.GetMultilingualString("5651F64F-ED8D-40D9-B39E-4BC8E930694D", "Bonded Warehouse Notification group"),
				ResString.GetMultilingualString("B6C1C298-0D07-4689-9FA7-F2F6BDDF3346", "Bonded Warehouse movements notification settings."),
				new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory,
				Guid.Empty)
			{
				DataType = new NotificationGroupGuidRegistryDataType(),
				CountryFilterPKs = CountryFilterPKs.Italy
			});
		}
	}

	public BooleanRegistryItem UseUCMPForCategoryITC
	{
		get
		{
			return GetItem("UseUCMPForCategoryITC", delegate
			{
				return new BooleanRegistryItem(
					"UseUCMPForCategoryITC",
					RawDataRegistry.Categories.Customs_Italy,
					(NoResString)"Use UCMP For Category ITC",
					(NoResString)"If 'Yes', all Service Tasks with ITC category will be processed by UCMP Service Tasks instead of IT Service Tasks ('ITR', 'ITP', 'ITS').",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers,
					false);
			});
		}
	}

	#region Categories

	public abstract class Categories : RawDataRegistry.Categories
	{
		public static MultilingualString Customs_Italy_Notifications { get { return CombineCategories(Customs_Italy, ResString.GetMultilingualString("C79C5C6D-B500-42CB-8B7D-373DF8C6BCF1", "Notifications")); } }
	}

	#endregion
}
