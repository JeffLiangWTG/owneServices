using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBCollection : Customs.Business.CusHAWBDependentCollection
	{
		public CTOCusHAWBCollection(CTOCusMAWB mAWB)
			: base(mAWB)
		{
			this.MAWB = mAWB;
		}

		public new CTOCusHAWB this[int index]
		{
			get { return (CTOCusHAWB)Elements[index]; }
		}

		public new CTOCusHAWB AddNew()
		{
			return (CTOCusHAWB)base.AddNew();
		}

		public void HookCollectionChanged(CTOCusHAWBAndPartShipCollection collection)
		{
			collection.CountChanged += new CollectionCountChangedEventHandler(AllChildrenBills_CountChanged);
		}

		#region Implementation

		protected readonly CTOCusMAWB MAWB;

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusHAWBSchema.CS_CM; }
		}

		void AllChildrenBills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && e.BizObject is CTOCusHAWB && !Contains(e.BizObject))
			{
				Add(e.BizObject);
			}
			else if (e.ItemRemoved && e.BizObject is CTOCusHAWB && Contains(e.BizObject))
			{
				Remove(e.BizObject);
			}
		}

		#endregion
	}
}
