using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerCollection : DependentBusinessObjectCollection<CusSCAContainer, CusSCAOceanBill>
	{
		public CusSCAContainerCollection(CusSCAOceanBill oceanBill, BusinessObjectFactory factory)
			: base(oceanBill, factory)
		{
			this.oceanBill = oceanBill;
		}

		public CusSCAContainer Find(ZString containerNumber)
		{
			CusSCAContainer result = null;
			foreach (CusSCAContainer container in this)
			{
				if (container.CN_ContainerNumber == containerNumber)
				{
					result = container;
					break;
				}
			}
			return result;
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var container = (CusSCAContainer)elementToDelete;
			if (container.CanDelete)
			{
				base.RemoveAndDelete(elementToDelete);
			}
			else
			{
				OnDeletingContainerWhenDisallowed(container);
			}
		}

		public event EventHandler DeletingContainerWhenDisallowed;

		void OnDeletingContainerWhenDisallowed(CusSCAContainer container)
		{
			if (DeletingContainerWhenDisallowed != null)
			{
				DeletingContainerWhenDisallowed(container, EventArgs.Empty);
			}
		}

		#region Implementation

		protected CusSCAOceanBill oceanBill;

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(CusSCAContainerSchema.CN_CB, oceanBill.PK);
			return result;
		}

		#endregion
	}
}
