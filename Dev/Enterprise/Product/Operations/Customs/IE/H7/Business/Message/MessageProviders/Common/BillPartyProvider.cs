using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.H7.Business
{
	public class BillPartyProvider : CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty
	{
		public BillPartyProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType)
		{
			this.bill = bill;
			this.addressType = addressType;
		}

		readonly AsycudaBill bill;
		readonly AsycudaBillAddress.AddressType addressType;

		public string Id
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						string consigneeId;
						if (!bill.ABL_ConsigneeRegNo.IsEmpty)
						{
							switch (bill.ABL_ConsigneeRegNoType)
							{
								case ImporterIdentificationTypes.Codes.EOR:
									consigneeId = bill.ABL_ConsigneeRegNo;
									break;
								case ImporterIdentificationTypes.Codes.CGT or ImporterIdentificationTypes.Codes.ITX or ImporterIdentificationTypes.Codes.PYE:
									consigneeId = bill.ABL_ConsigneeRegNoType + bill.ABL_ConsigneeRegNo;
									break;
								default:
									consigneeId = GetDefaultConsigneeId();
									break;
							}
						}
						else
						{
							consigneeId = GetDefaultConsigneeId();
						}

						return consigneeId;
					case AsycudaBillAddress.AddressType.Shipper:
						var shipperId = string.Empty;
						if (bill.Shipper != null)
						{
							shipperId = bill.ABL_ShipperRegNo;
						}

						return shipperId;
					default:
						return null;
				}
			}
		}

		public string Name => CachedValueHelper.GetValue(ref nameCached, () =>
		{
			if (IdIsDefault())
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneeName;
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperName;
					default:
						return null;
				}
			}

			return null;
		});
		CachedValue<string> nameCached;

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => IdIsDefault() ? new BillAddressProvider(bill, addressType) : null);
		CachedValue<IAddress> addressCached;

		public IContact Contact => null;

		protected const string NotRegistered = "NR";

		bool IdIsDefault() => Id.IsNullOrEmpty() || Id.Equals(NotRegistered);

		string GetDefaultConsigneeId() => ConsigneeHasNoNameOrAddress ? null : NotRegistered;

		bool ConsigneeHasNoNameOrAddress => bill.ABL_ConsigneeName.IsEmpty
										&& bill.ABL_ConsigneeStreet1.IsEmpty
										&& bill.ABL_ConsigneeStreet2.IsEmpty
										&& bill.ABL_ConsigneeCity.IsEmpty
										&& bill.ABL_ConsigneePostcode.IsEmpty
										&& bill.ABL_RN_NKConsigneeCountry.IsEmpty;
	}
}
