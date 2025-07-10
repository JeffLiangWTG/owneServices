using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderCollectionView : Customs.Business.CusSeaManOBLHeaderCollectionView
	{
		public CusSeaManOBLHeaderCollectionView(CusSeaManOBLHeaderCollection collection)
			: base(collection)
		{
		}

		public new CusSeaManOBLHeader AddNew()
		{
			return (CusSeaManOBLHeader)base.AddNew();
		}

		public new CusSeaManOBLHeader this[int index]
		{
			get { return (CusSeaManOBLHeader)base[index]; }
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			CusSeaManOBLHeader oceanBillHeader = (CusSeaManOBLHeader)elementToDelete;
			if (oceanBillHeader.CanDelete)
			{
				base.RemoveAndDelete(elementToDelete);
			}
			else
			{
				OnDeletingOceanBillWhenDisallowed(oceanBillHeader);
			}
		}

		public EventHandler DeletingOceanBillWhenDisallowed;

		void OnDeletingOceanBillWhenDisallowed(object oceanBillHeader)
		{
			if (DeletingOceanBillWhenDisallowed != null)
			{
				DeletingOceanBillWhenDisallowed(oceanBillHeader, EventArgs.Empty);
			}
		}
	}
}
