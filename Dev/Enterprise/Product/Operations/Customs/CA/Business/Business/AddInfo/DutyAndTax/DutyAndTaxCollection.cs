using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	public class DutyAndTaxCollection : ActiveBusinessObjectCollection<DutyAndTax>, IDutyAndTaxCollection
	{
		public DutyAndTaxCollection(BusinessObjectFactory factory, IDutyAndTaxData master)
			: base(factory, new DutyAndTaxNoResultRelationship(master))
		{
		}

		public DutyAndTaxCollection(IDutyAndTaxData master)
			: base(((BusinessObject)master).Factory, ((BusinessObject)master), GetQuery(((BusinessObject)master).TablePrefix), CusAddInfoSchema.B7_ParentID)
		{
			Factory.Saved += (f, successfully) => hasChangesChangedOccurred = !successfully;
		}

		internal IDutyAndTaxData DutyAndTaxDataMaster
		{
			get { return (IDutyAndTaxData)Relationship.Master; }
		}

		public static ZQuery GetQuery(ZString parentTableCode)
		{
			var result = new ZQuery(CusAddInfoSchema.B7_Type, Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.CADutyAndTax);
			result.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, parentTableCode);
			return result;
		}

		public DutyAndTax AddNew(ZString taxType)
		{
			var newTax = AddNew();
			newTax.C1_TaxType = taxType;
			return newTax;
		}

		#region Overrides

		protected override void SetRelationshipDefaultsForElementCore(DutyAndTax newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (!(Relationship is DutyAndTaxNoResultRelationship))
			{
				var master = Relationship.Master;
				newElement.Parent = (IDutyAndTaxData)master;
				newElement.B7_ParentID = master.PK;
				newElement.B7_ParentTableCode = master.TablePrefix;
			}
		}

		protected override void OnLoadedIntoCollectionCore(DutyAndTax loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Parent = (IDutyAndTaxData)Relationship.Master;
			UnhookEvents(loadedObject);
			HookEvents(loadedObject);
		}

		public override void Delete(DutyAndTax businessObject)
		{
			if (businessObject.IsInDatabase)
			{
				((IBusinessObjectCollectionInternals)this).HasChangesFromDelete = true;
			}
			UnhookEvents(businessObject);
			base.Delete(businessObject);
			FireHasChangesChanged(this, EventArgs.Empty);
		}

		#endregion

		#region Implementation

		IDutyAndTax IDutyAndTaxCollection.AddNew() => this.AddNew();

		IDutyAndTax IDutyAndTaxCollection.FirstOrDefault() => this.FirstOrDefault();

		void HookEvents(DutyAndTax loadedObject)
		{
			loadedObject.C1_OverrideInfo.ValueChanged += C1_OverrideInfo_ValueChanged;
			loadedObject.C1_TaxTypeInfo.ValueChanged += C1_TaxTypeInfo_ValueChanged;
			loadedObject.C1_CodeInfo.ValueChanged += C1_CodeInfo_ValueChanged;
			loadedObject.HasChangesChanged += FireHasChangesChanged;
		}

		void UnhookEvents(DutyAndTax loadedObject)
		{
			loadedObject.C1_OverrideInfo.ValueChanged -= C1_OverrideInfo_ValueChanged;
			loadedObject.C1_TaxTypeInfo.ValueChanged -= C1_TaxTypeInfo_ValueChanged;
			loadedObject.C1_CodeInfo.ValueChanged -= C1_CodeInfo_ValueChanged;
			loadedObject.HasChangesChanged -= FireHasChangesChanged;
		}

		void C1_TaxTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ElementTaxTypeValueChanged != null)
			{
				var dutyAndTax = (DutyAndTax)((CADutyAndTaxAddInfo)sender).Parent;
				ElementTaxTypeValueChanged(dutyAndTax);
			}
		}

		void C1_OverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ElementOverrideValueChanged != null)
			{
				var dutyAndTax = (DutyAndTax)((CADutyAndTaxAddInfo)sender).Parent;
				ElementOverrideValueChanged(dutyAndTax);
			}
		}

		void C1_CodeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ElementCodeValueChanged != null)
			{
				var dutyAndTax = (DutyAndTax)((CADutyAndTaxAddInfo)sender).Parent;
				ElementCodeValueChanged(dutyAndTax);
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
		public event DutyAndTaxValueChangedDelegate ElementOverrideValueChanged;
		public event DutyAndTaxValueChangedDelegate ElementTaxTypeValueChanged;
		public event DutyAndTaxValueChangedDelegate ElementCodeValueChanged;
		public delegate void DutyAndTaxValueChangedDelegate(DutyAndTax tax);

		#endregion
	}

	#region DutyAndTaxNoResultRelationship

	class DutyAndTaxNoResultRelationship : AdhocCollectionRelationship
	{
		public DutyAndTaxNoResultRelationship(IDutyAndTaxData master)
			: base(typeof(DutyAndTax))
		{
			this.master = (BusinessObject)master;
		}

		public override BusinessObject Master
		{
			get { return master; }
		}

		readonly BusinessObject master;
	}

	#endregion
}
