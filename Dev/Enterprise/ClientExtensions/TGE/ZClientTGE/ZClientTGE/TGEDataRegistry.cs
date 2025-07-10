using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Client.TGE.Business;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TGE
{
	public sealed class TGEDataRegistry : RegistryItemSet
	{
		TGEDataRegistry() { }

		#region Instance
		public static TGEDataRegistry Instance
		{
			get { return fInstance ?? (fInstance = new TGEDataRegistry()); }
		}
		[ThreadStatic]
		static TGEDataRegistry fInstance;
		#endregion

		public override bool IsForProductivityWise => false;

		internal const string TGECategory = "TGE Client Extensions";
		internal const string PMSCategory = TGECategory + "/PMS";
		internal const string CSSCategory = TGECategory + "/Export of ACA and Customs Airfreight Status";

		#region PMS System

		#region PMSFileImportDirectory

		public ZString PMSFileImportDirectory
		{
			get { return new ZString(PMSFileImportDirectoryRaw.Value); }
			set { PMSFileImportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region PMSFileArchiveDirectory

		public ZString PMSFileArchiveDirectory
		{
			get { return new ZString(PMSFileArchiveDirectoryRaw.Value); }
			set { PMSFileArchiveDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region CodeMapPMSOrganisation

		public ZGuid CodeMapPMSOrganisation
		{
			get { return new ZGuid(CodeMapPMSOrganisationRaw.Value); }
			set { CodeMapPMSOrganisationRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToGuid() : Guid.Empty); }
		}

		#endregion

		#region PMSNotificationGroup

		public Guid PMSNotificationGroup
		{
			get { return PMSNotificationGroupRaw.Value; }
			set { PMSNotificationGroupRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#endregion

		#region CSS Interface

		internal ServiceTaskDataTransferSwitchRegistryItem CSSDataTransferSwitchRegistryItem
		{
			get
			{
				return GetItem<ServiceTaskDataTransferSwitchRegistryItem>("TGECSSDataTransferRegistryItem", delegate
				{
					return new ServiceTaskDataTransferSwitchRegistryItem(
						"TGECSSDataTransferRegistryItem",
						(NoResString)CSSCategory,
						(NoResString)"Data Export Settings",
						(NoResString)"Please fill in all the fields provided below.",
						RegistryStorageFlags.Company);
				});
			}
		}

		internal ZString CSSExportDirectory
		{
			get { return CSSDataTransferSwitchRegistryItem.Value.Directory; }
		}

		internal ZGuid CSSExportNotifyGroup
		{
			get { return CSSDataTransferSwitchRegistryItem.Value.GroupPK; }
		}

		internal ZBool EnableCSSDataExport
		{
			get { return CSSDataTransferSwitchRegistryItem.Value.EnableInterface; }
		}

		#region CustomsStatusCode

		internal TGEEventRegistryBusinessObjectCollection CSSImportCustomsStatusCodes
		{
			get
			{
				if (CurrentCountryIsAustralia)
				{
					return CSSImportCustomsStatusCodesItem.Value;
				}
				else
				{
					return new TGEEventRegistryBusinessObjectCollection();
				}
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				CSSImportCustomsStatusCodesItem.SetValue(CurrentCompanyPKGuid, Guid.Empty, Guid.Empty, value);
			}
		}

		internal TGEEventsRegistryItem CSSImportCustomsStatusCodesItem
		{
			get
			{
				return GetItem<TGEEventsRegistryItem>("TGECSSImportCustomsStatusCodes", delegate
				{
					return new TGEEventsRegistryItem("TGECSSImportCustomsStatusCodes",
						CSSCategory, "Import Customs Status Code", "Import Cutoms Codes that will trigger a data export.", RegistryStorageFlags.Company);
				});
			}
		}

		internal TGEEventRegistryBusinessObjectCollection CSSExportCustomsStatusCodes
		{
			get
			{
				if (CurrentCountryIsAustralia)
				{
					return CSSExportCustomsStatusCodesItem.Value;
				}
				else
				{
					return new TGEEventRegistryBusinessObjectCollection();
				}
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				CSSExportCustomsStatusCodesItem.SetValue(CurrentCompanyPKGuid, Guid.Empty, Guid.Empty, value);
			}
		}

		internal TGEEventsRegistryItem CSSExportCustomsStatusCodesItem
		{
			get
			{
				return GetItem<TGEEventsRegistryItem>("TGECSSExportCustomsStatusCodes", delegate
				{
					return new TGEEventsRegistryItem("TGECSSExportCustomsStatusCodes",
					   CSSCategory, "Export Customs Status Code", "Export Cutoms Codes that will trigger a data export.", RegistryStorageFlags.Company);
				});
			}
		}

		internal bool CSSInterfaceReadyToGo
		{
			get
			{
				if (EnableCSSDataExport && !CSSExportDirectory.IsEmpty && Directory.Exists(CSSExportDirectory) && !CSSImportCustomsStatusCodes.Count.Equals(0)
					&& !CSSExportCustomsStatusCodes.Count.Equals(0) && CurrentCountryIsAustralia)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region PMSFileImportDirectoryRaw

		internal StringRegistryItem PMSFileImportDirectoryRaw
		{
			get
			{
				return GetItem<StringRegistryItem>("PMSFileImportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("PMSFileImportDirectory", (NoResString)PMSCategory, (NoResString)"PMS File Import Directory", (NoResString)"DataRow is the Directory for storing all the incoming tsv files", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region PMSFileArchiveDirectoryRaw

		internal StringRegistryItem PMSFileArchiveDirectoryRaw
		{
			get
			{
				return GetItem<StringRegistryItem>("PMSFileArchiveDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("PMSFileArchiveDirectory", (NoResString)PMSCategory, (NoResString)"PMS File Archive Directory", (NoResString)"DataRow is the Directory for storing all the archived tsv files", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region CodeMapPMSOrganisationRaw

		internal GuidRegistryItem CodeMapPMSOrganisationRaw
		{
			get
			{
				return GetItem<GuidRegistryItem>("CodeMapPMSOrganisation", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CodeMapPMSOrganisation", (NoResString)PMSCategory, (NoResString)"PMS Organisation used for Code Mapping", (NoResString)"DataRow is the PMS organisation behind which all organisations are code mapped for edi transmission", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		#endregion

		#region PMSNotificationGroupRaw

		internal GuidRegistryItem PMSNotificationGroupRaw
		{
			get
			{
				return GetItem<GuidRegistryItem>("PMS Notification Group", delegate
				{
					return new GuidRegistryItem("PMS Notification Group", (NoResString)PMSCategory, (NoResString)"PMS Notification Group", null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup), RegistryStorageFlags.System, RegistryOptions.NotCached, Guid.Empty);
				});
			}
		}

		#endregion

		#endregion

		Guid CurrentCompanyPKGuid
		{
			get { return GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		void ThrowExceptionIfNotInAustralia()
		{
			if (!CurrentCountryIsAustralia)
			{
				throw new DeveloperNotificationException("Should not use an Australian Registry Item for an non Australian system");
			}
		}

		static bool CurrentCountryIsAustralia
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia; }
		}
	}
}
