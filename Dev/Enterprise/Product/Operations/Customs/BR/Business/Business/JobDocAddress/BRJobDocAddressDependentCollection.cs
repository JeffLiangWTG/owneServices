using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public BRJobDocAddressDependentCollection(IDocAddresses parent) : base(parent)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (bizOAdded is JobDocAddress docAddress)
			{
				docAddress.E2_AddressOverrideInfo.ValueChanged += UpdateGeoLocation;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is JobDocAddress docAddress)
			{
				docAddress.E2_AddressOverrideInfo.ValueChanged -= UpdateGeoLocation;
			}
		}

		void UpdateGeoLocation(object sender, EventArgs e)
		{
			if (sender is JobDocAddress docAddress && docAddress.DocAddressType == DocAddressType.ClearanceLocalInvolvedParty)
			{
				if (docAddress.E2_AddressOverride && docAddress.Address != null)
				{
					docAddress.E2_GeoLocation = docAddress.Address.OA_GeoLocation;
				}
				else
				{
					docAddress.E2_GeoLocation = CargoWise.Types.ZGeography.Empty;
				}
			}
		}
	}
}
