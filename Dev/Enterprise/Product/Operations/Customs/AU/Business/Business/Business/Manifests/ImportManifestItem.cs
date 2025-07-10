using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportManifestItem : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ImportManifestItem(BaseCusSeaManOBLHeader relatedBusinessObject, JobSailing sailing, ImportManifestItemCollection parentCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			this.RelatedBusinessObjectList.Add(relatedBusinessObject);
			this.sailing = sailing;
			this.ParentCollection = parentCollection;
		}

		public readonly ImportManifestItemCollection ParentCollection;
		readonly JobSailing sailing;

		#region Schema

		static class Schema
		{
			public const string ShouldSave = "ShouldSave";
			public const string DischargePort = "DischargePort";
			public const string VesselName = "VesselName";
			public const string VoyageNumber = "VoyageNumber";
			public const string BillNumber = "BillNumber";
			public const string IsCargoList = "IsCargoList";
		}

		#endregion

		#region Properties

		#region Bound Properties

		#region ShouldSave

		public ZBool ShouldSave
		{
			get { return shouldSave; }
			set
			{
				SetNonPersistentPropertyValue(ShouldSaveInfo, ref shouldSave, value);
			}
		}
		ZBool shouldSave;

		public ZPropertyInfo ShouldSaveInfo
		{
			get { return GetZPropertyInfo(Schema.ShouldSave); }
		}

		#endregion

		#region DischargePort

		[CargoWise.ComponentModel.MaxLength(5)]
		public ZString DischargePort
		{
			get { return HasRelatedObjects ? RelatedBusinessObjectList[0].BO_RL_NKDischargePort : ZString.Empty; }
		}

		public ZPropertyInfo DischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.DischargePort); }
		}

		#endregion

		#region Vessel Name

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString VesselName
		{
			get
			{
				return sailing == null ? ZString.Empty : sailing.JX_JV_NKVessel;
			}
		}

		public ZPropertyInfo VesselNameInfo
		{
			get { return GetZPropertyInfo(Schema.VesselName); }
		}

		#endregion

		#region VoyageNumber

		[CargoWise.ComponentModel.MaxLength(10)]
		public ZString VoyageNumber
		{
			get { return sailing == null ? ZString.Empty : sailing.JX_JV_VoyageFlight; }
		}

		public ZPropertyInfo VoyageNumberInfo
		{
			get { return GetZPropertyInfo(Schema.VoyageNumber); }
		}

		#endregion

		#region Bill Number

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString BillNumber
		{
			get { return HasRelatedObjects ? RelatedBusinessObjectList[0].BO_OceanBill : ZString.Empty; }
		}

		public ZPropertyInfo BillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BillNumber); }
		}

		#endregion

		#region IsCargoList

		public ZBool IsCargoList
		{
			get { return HasRelatedObjects && (RelatedBusinessObjectList[0] as CusSeaManOBLHeaderCargoLine) != null; }
		}

		public ZPropertyInfo IsCargoListInfo
		{
			get { return GetZPropertyInfo(Schema.IsCargoList); }
		}

		#endregion

		#endregion

		#region NonBoundProperties

		#region RelatedBusinessObjectList

		public List<BaseCusSeaManOBLHeader> RelatedBusinessObjectList
		{
			get
			{
				if (relatedBusinessObjectList == null)
				{
					relatedBusinessObjectList = new List<BaseCusSeaManOBLHeader>();
				}

				return relatedBusinessObjectList;
			}
		}
		List<BaseCusSeaManOBLHeader> relatedBusinessObjectList;

		public void DeleteRelatedBusinessObjects()
		{
			foreach (BaseCusSeaManOBLHeader relatedBusinessObject in RelatedBusinessObjectList)
			{
				if (relatedBusinessObject != null)
				{
					relatedBusinessObject.Delete();
				}
			}
		}

		#endregion

		#region HasRelatedObjects

		bool HasRelatedObjects
		{
			get { return RelatedBusinessObjectList.Count > 0; }
		}

		#endregion

		#endregion

		#endregion

		#region Lookups

		#region Selected Port

		[CargoWise.ComponentModel.MaxLength(5)]
		public ZString SelectedPort
		{
			get { return ParentCollection.Manifest.Lookups.SelectedPort; }
			set
			{
				CheckMaximumLength(SelectedPortInfo, value);
				ParentCollection.Manifest.Lookups.SelectedPort = value;
				SelectedPortInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SelectedPortInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedPort)); }
		}

		#endregion

		#region Arrival Ports

		public CodeDescriptionPairList ArrivalPorts
		{
			get { return ParentCollection.Manifest.Lookups.AllArrivalPortsWithAllValue; }
		}

		#endregion

		#region ItemsView

		public ImportManifestItemCollection ItemsView
		{
			get { return ParentCollection.ItemsView; }
		}

		#endregion

		#endregion

		#region Overrides

		protected override void OnFactorySaving()
		{
			if (!ParentCollection.IsRefinedForSave)
			{
				int itemsCountBeforeSaving = ParentCollection.Count;
				ParentCollection.SavedBillsCount = CountSavedBills();

				ParentCollection.RefineItemsToSave();
				ParentCollection.IsRefinedForSave = true;

				if (itemsCountBeforeSaving != 0 && !ParentCollection.HasItemsToSave)
				{
					this.Delete();
				}
			}

			base.OnFactorySaving();
		}

		public override bool ReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Help Methods

		public int CountSavedBills()
		{
			List<String> billNumbersList = new List<string>();

			foreach (ImportManifestItem item in ParentCollection)
			{
				if (item.ShouldSave && !billNumbersList.Contains(item.BillNumber) && !item.HasWarnings)
				{
					billNumbersList.Add(item.BillNumber);
				}
			}

			return billNumbersList.Count;
		}

		#endregion

	}
}
