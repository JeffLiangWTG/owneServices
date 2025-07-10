using System;
using CargoWise.Types;
using Enterprise.Client.SWL.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWL
{
	public sealed class SWLDataRegistry : RegistryItemSet
	{
		[ThreadStatic]
		static SWLDataRegistry instance;
		public static SWLDataRegistry Instance
		{
			get { return instance ?? (instance = new SWLDataRegistry()); }
		}

		public static MultilingualString ShipnetCategory
		{
			get
			{
				return CombineCategories(LinerAgencyDataRegistry.Categories.LinerAgency, ResString.GetMultilingualString("26f8625c-d1d4-4a98-8648-9ac8e0d2de17", "Shipnet"));
			}
		}

		public override bool IsForProductivityWise => false;

		#region Shipnet Enabling
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem IsShipnetEnable
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IsShipnetEnable", delegate
				{
					return new BooleanRegistryItem(
						"IsShipnetEnable",
						ShipnetCategory,
						(NoResString)"Enable Shipnet Module",
						(NoResString)"Set this to True to enable the Shipnet Module",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion
		#endregion

		#region Shipnet Backup Directory

		public StringRegistryItem ShipnetBackupDirectoryItem
		{
			get
			{
				return GetItem<StringRegistryItem>("ShipnetBackupDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ShipnetBackupDirectory",
						ShipnetCategory,
						ResString.GetMultilingualString("d5d3904e-8a14-40c6-8f22-a351b9bb30c1", "Backup Directory"),
						ResString.GetMultilingualString("39b74b71-a7c8-4128-acde-62d5559e822c", "A directory to store a copy of the Shipnet Exported files"),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region Shipnet Backup Directory

		public IntRegistryItem ShipnetPurgeBackupItem
		{
			get
			{
				return GetItem<IntRegistryItem>("ShipnetPurgeBackup", delegate
				{
					return new IntRegistryItem(
						"ShipnetPurgeBackup",
						ShipnetCategory,
						ResString.GetMultilingualString("ADD0C513-D424-4329-AD09-71140814C3C2", "Purge Backup"),
						ResString.GetMultilingualString("04141AA2-BC6A-4D8F-82F0-715CBDCC8DA1", "Backup files will be purged after a set period of time in days"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue, 30, 30, 90);
				});
			}
		}

		#endregion

		#region Shipnet Notification Email Group

		public GuidRegistryItem ShipnetNotificationEmailGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("ShipnetNotificationEmailGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ShipnetNotificationEmailGroup",
						ShipnetCategory,
						ResString.GetMultilingualString("851c2ea3-00ae-486e-bd08-e303b6521836", "Notification Email Group"),
						ResString.GetMultilingualString("62fcdd78-eeeb-4256-aa9c-5d739b506b44", "The group that will be sent email notifications for a Shipnet import."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						RegistryFactory.Instance.GetGroupPK("ALL"));

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region ShipnetHighWaterMarkItem

		public DateTimeRegistryItem ShipnetHighWaterMarkItem
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("ShipnetHighWaterMark", delegate
				{
					return new DateTimeRegistryItem(
						"ShipnetHighWaterMark",
						ShipnetCategory,
						ResString.GetMultilingualString("c1390f54-effd-4050-9ee6-0267349e5710", "High Water Mark"),
						ResString.GetMultilingualString("84194afa-1982-40a9-b9a5-f9b4e6bd551b", "The time from then on logs get considered to export Shipnet data"),
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.Company,
						RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
						ZDateTime.Now.ToDateTime(),
						false);
				});
			}
		}

		#endregion

		#region GetShipnetSetupBusinessObject

		public RegistryBusinessObjectTemplate GetShipnetSetupBusinessObject(ZGuid orgPK)
		{
			RegistryBusinessObjectTemplate result = null;
			if (orgPK.IsValid)
			{
				result = (RegistryBusinessObjectTemplate)ShipnetSetupRaw.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid());
				ShipnetSetupBusinessObject shipnetSetupBusinessObject = (ShipnetSetupBusinessObject)result;
				shipnetSetupBusinessObject.IsShipnetCarrier = (((IRegistryItemInternals)ShipnetSetupRaw).HasActualValue(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid()));
				shipnetSetupBusinessObject.CompanyDataPK = orgPK;
			}

			return result;
		}

		#endregion

		#region SetOrDeleteShipnetSetupBusinessObject

		public void SetOrDeleteShipnetSetupBusinessObject(ZGuid orgPK, RegistryBusinessObjectTemplate bizObj)
		{
			if (bizObj is ShipnetSetupBusinessObject)
			{
				if (orgPK.IsValid && bizObj != null)
				{
					bizObj.RunPreSaveValidation();
					if (!bizObj.HasErrors)
					{
						if (((ShipnetSetupBusinessObject)bizObj).IsShipnetCarrier)
						{
							ShipnetSetupRaw.SetValue(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid(), bizObj);
						}
						else
						{
							((IRegistryItemInternals)ShipnetSetupRaw).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid());
						}
					}
				}
			}
			else
			{
				throw new ArgumentException("BizObj type passed into SetOrDeleteShipnetSetupBusinessObject(ZGuid OrgPK, RegistryBusinessObjectTemplate BizObj) must be derived from ShipnetSetupBusinessObject. Error on type : " + bizObj.GetType());
			}
		}

		#endregion

		#region ShipnetSetupRaw
		#region SuppressResourceStringsCheckRegion

		internal SWLRegistryItem ShipnetSetupRaw
		{
			get
			{
				return GetItem("ShipnetSetupRaw", () => new SWLRegistryItem(
					"ShipnetSetupRaw",
					ShipnetCategory,
					(NoResString)"Setup Shipnet Carriers Data",
					null,
					RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue));
			}
		}
		#endregion
		#endregion
	}
}
