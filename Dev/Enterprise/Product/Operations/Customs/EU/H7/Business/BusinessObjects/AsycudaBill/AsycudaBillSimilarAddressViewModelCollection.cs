using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillSimilarAddressViewModelCollection : NonPersistentBusinessObjectCollection<AsycudaBillSimilarAddressViewModel>, IDisposable
	{
		public AsycudaBillSimilarAddressViewModelCollection(IEnumerable<AsycudaBill> bills)
		{
			foreach (var bill in bills)
			{
				if (bill.ABL_OA_Consignee.IsEmpty)
				{
					Add(new AsycudaBillSimilarAddressViewModel(bill, ManifestBase.AsycudaBillAddress.AddressType.Consignee));
				}

				if (bill.ABL_OA_Shipper.IsEmpty)
				{
					Add(new AsycudaBillSimilarAddressViewModel(bill, ManifestBase.AsycudaBillAddress.AddressType.Shipper));
				}
			}
		}

		readonly List<AsycudaBillSimilarAddressViewModel> billAddressesWithNewOrgSelected = new List<AsycudaBillSimilarAddressViewModel>();

		public override void Remove(BusinessObject businessObject)
		{
			if (businessObject is AsycudaBillSimilarAddressViewModel viewModel && viewModel.LinkingAddressPK != Guid.Empty)
			{
				billAddressesWithNewOrgSelected.Add(viewModel);
			}

			base.Remove(businessObject);
		}

		public void ApplyLinkBillAddresses()
		{
			billAddressesWithNewOrgSelected.ForEach(b => b.ApplyLinkAddress());
		}

		public void ResetBillAddresses()
		{
			billAddressesWithNewOrgSelected.Clear();
		}

		public bool AnyBillLinkedWithOrg => billAddressesWithNewOrgSelected.Count > 0;

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		#region IDisposable Members

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.ForEach(viewModel => ((AsycudaBillSimilarAddressViewModel)viewModel).Dispose());
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}

