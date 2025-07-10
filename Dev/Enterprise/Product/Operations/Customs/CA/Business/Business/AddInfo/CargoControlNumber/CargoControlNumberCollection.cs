using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CargoControlNumberCollection : ActiveBusinessObjectCollection<CargoControlNumber>
	{
		public CargoControlNumberCollection(BusinessObjectFactory factory, JobDeclaration master)
			: base(factory, new CACargoControlNumberNoResultRelationship(master))
		{
		}

		public CargoControlNumberCollection(JobDeclaration master)
			: base(master.Factory, master, GetQuery(master.TablePrefix), CusAddInfoSchema.B7_ParentID)
		{
			Factory.Saved += (f, successfully) => hasChangesChangedOccurred = !successfully;
		}

		internal JobDeclaration CACargoControlNumberDataMaster
		{
			get { return (JobDeclaration)Relationship.Master; }
		}

		static ZQuery GetQuery(ZString parentTableCode)
		{
			var result = new ZQuery(CusAddInfoSchema.B7_Type, Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.CACCN);
			result.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, parentTableCode);
			return result;
		}

		public CargoControlNumber AddNew(ZString ccNumber)
		{
			var newCCN = AddNew();
			newCCN.CY_CargoControlNumber = ccNumber;
			return newCCN;
		}

		public void Load()
		{
		}

		public void RemoveAndDeleteAll()
		{
			foreach (var cargoControlNumber in this.Where(ccn => !ccn.CA_IsFromNumbersTab).ToArray())
			{
				RemoveAndDelete(cargoControlNumber);
			}
		}

		public void RemoveAndDelete(CargoControlNumber ccn)
		{
			if (!ccn.IsDeleted)
			{
				RemoveFromRelationship(ccn);
				Delete(ccn);
			}
		}

		#region Overrides

		protected override void SetRelationshipDefaultsForElementCore(CargoControlNumber newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (!(Relationship is CACargoControlNumberNoResultRelationship))
			{
				var master = Relationship.Master;
				newElement.Parent = (JobDeclaration)master;
				newElement.B7_ParentID = master.PK;
			}
		}

		protected override void OnLoadedIntoCollectionCore(CargoControlNumber loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Parent = (JobDeclaration)Relationship.Master;
			UnhookEvents(loadedObject);
			HookEvents(loadedObject);
		}

		public override void Delete(CargoControlNumber businessObject)
		{
			UnhookEvents(businessObject);
			base.Delete(businessObject);
			FireHasChangesChanged(this, EventArgs.Empty);
		}

		#endregion

		#region Implementation

		void HookEvents(CargoControlNumber loadedObject)
		{
			loadedObject.CY_CargoControlNumberInfo.ValueChanged += CA_CCNInfoNumberInfo_ValueChanged;
			loadedObject.CA_CU_CCNInfoBillInfo.ValueChanged += CA_CU_CCNInfoBillInfo_ValueChanged;
			loadedObject.HasChangesChanged += FireHasChangesChanged;
		}

		void UnhookEvents(CargoControlNumber loadedObject)
		{
			loadedObject.CY_CargoControlNumberInfo.ValueChanged -= CA_CCNInfoNumberInfo_ValueChanged;
			loadedObject.CA_CU_CCNInfoBillInfo.ValueChanged -= CA_CU_CCNInfoBillInfo_ValueChanged;
			loadedObject.HasChangesChanged -= FireHasChangesChanged;
		}

		void CA_CCNInfoNumberInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ElementCCNInfoNumberValueChanged != null)
			{
				var cACargoControlNumber = (CargoControlNumber)((CACargoControlNumberAddInfo)sender).Parent;
				ElementCCNInfoNumberValueChanged(cACargoControlNumber);
			}
		}

		void CA_CU_CCNInfoBillInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ElementCUCCNInfoBillValueChanged != null)
			{
				var cACargoControlNumber = (CargoControlNumber)((CACargoControlNumberAddInfo)sender).Parent;
				ElementCUCCNInfoBillValueChanged(cACargoControlNumber);
			}
		}

		void FireHasChangesChanged(object sender, EventArgs e)
		{
			if (!hasChangesChangedOccurred
				&& (!(sender is BusinessObject) || ((BusinessObject)sender).HasChanges)
				&& HasChangesChangedInternal != null)
			{
				HasChangesChangedInternal(sender, e);
				hasChangesChangedOccurred = true;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		bool hasChangesChangedOccurred;

		event EventHandler HasChangesChangedInternal;
		public event EventHandler HasChangesChanged
		{
			add { HasChangesChangedInternal += value; }
			remove
			{
				hasChangesChangedOccurred = false;
				HasChangesChangedInternal -= value;
			}
		}
		public event CACargoControlNumberValueChangedDelegate ElementCCNInfoNumberValueChanged;
		public event CACargoControlNumberValueChangedDelegate ElementCUCCNInfoBillValueChanged;
		public delegate void CACargoControlNumberValueChangedDelegate(CargoControlNumber ccNumber);

		#endregion
	}

	#region CACargoControlNumberNoResultRelationship

	class CACargoControlNumberNoResultRelationship : AdhocCollectionRelationship
	{
		public CACargoControlNumberNoResultRelationship(JobDeclaration master)
			: base(typeof(CargoControlNumber))
		{
			this.master = master;
		}

		public override BusinessObject Master
		{
			get { return master; }
		}

		readonly BusinessObject master;
	}

	#endregion
}
