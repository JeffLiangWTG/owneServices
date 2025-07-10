using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureCusTransportMeansFilteredCollection : Customs.Business.FilteredCollection<DepartureCusTransportMeans>, IBusinessObjectCollection<IAdditionalWagonProvider>
	{
		public DepartureCusTransportMeansFilteredCollection(NctsBill bill) : base((BusinessObjectCollection)bill.DepartureTransportInfos)
		{
			this.bill = bill;
			Rebuild();
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return bill.DepartureTransportInfos.Count >= 1; }
		}

		readonly NctsBill bill;

		protected override bool IsThisPartOfTheCollection(BusinessObject bObject)
		{
			var transport = bObject as DepartureCusTransportMeans;
			return transport != null && transport.TPM_SequenceNumber >= 2;
		}

		protected override bool IsFilterEmpty => false;

		IAdditionalWagonProvider IBusinessObjectCollection<IAdditionalWagonProvider>.this[int index] => (IAdditionalWagonProvider)Elements[index];

		protected override void ClearFilterCore()
		{
		}

		IAdditionalWagonProvider IBusinessObjectCollection<IAdditionalWagonProvider>.AddNew() => AddNew();

		public IEnumerator<IAdditionalWagonProvider> GetEnumerator() => Elements.Cast<IAdditionalWagonProvider>().GetEnumerator();

		#endregion Implementation
	}
}
