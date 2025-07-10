using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	sealed class AddressProvider : IAddress, IWesternAddress
	{
		public AddressProvider(AsycudaBill bill, string type, string messageType)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.type = Argument.NotNull(type, nameof(type));
			this.messageType = Argument.NotNull(messageType, nameof(messageType));
		}

		readonly AsycudaBill bill;
		readonly string type;
		readonly string messageType;

		public string Code
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperRegNo;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneeRegNo;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyRegNo;
					default:
						return string.Empty;
				}
			}
		}

		public string Name
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperName;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneeName;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyName;
					default:
						return string.Empty;
				}
			}
		}

		public string Address
		{
			get
			{
				switch (messageType)
				{
					case nameof(IHCH01Bills):
						switch (type)
						{
							case nameof(BillProvider.Shipper):
								return bill.ShipperAddress;
							case nameof(BillProvider.Consignee):
								return bill.ConsigneeAddress;
							default:
								return string.Empty;
						}
					default:
						return string.Empty;
				}
			}
		}

		public string Phone
		{
			get
			{
				ZString result;
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						result = bill.ABL_ShipperPhone;
						break;
					case nameof(BillProvider.Consignee):
						result = bill.ABL_ConsigneePhone;
						break;
					case nameof(BillProvider.Notifier):
						result = bill.ABL_NotifyPartyPhone;
						break;
					default:
						result = ZString.Empty;
						break;
				}

				return result.RemoveNonNumCharFromPhoneNumber();
			}
		}

		public string PostCode
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperPostcode;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneePostcode;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyPostcode;
					default:
						return string.Empty;
				}
			}
		}

		public string Street1
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperStreet1;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneeStreet1;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyStreet1;
					default:
						return string.Empty;
				}
			}
		}

		public string Street2
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperStreet2;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneeStreet2;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyStreet2;
					default:
						return string.Empty;
				}
			}
		}

		public string City
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperCity;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneeCity;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyCity;
					default:
						return string.Empty;
				}
			}
		}

		public string State
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_ShipperState;
					case nameof(BillProvider.Consignee):
						return bill.ABL_ConsigneeState;
					case nameof(BillProvider.Notifier):
						return bill.ABL_NotifyPartyState;
					default:
						return string.Empty;
				}
			}
		}

		public string CountryCode
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.Shipper):
						return bill.ABL_RN_NKShipperCountry;
					case nameof(BillProvider.Consignee):
						return bill.ABL_RN_NKConsigneeCountry;
					case nameof(BillProvider.Notifier):
						return bill.ABL_RN_NKNotifyPartyCountry;
					default:
						return string.Empty;
				}
			}
		}
	}
}
