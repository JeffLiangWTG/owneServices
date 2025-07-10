using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.HK.Business
{
	public static class ForwardingShipmentExtensionMethods
	{
		public static IEnumerable<ZString> GetTraxonLicenseNumbersFromShipment(this ForwardingShipment shipment)
		{
			if (shipment != null)
			{
				foreach (CusEntryNumber entryNumber in shipment.CusEntryNumbers)
				{
					if ((entryNumber.CE_EntryType == CusEntryNumberTypes.HongKong.ExportLicense && shipment.IsExport()) ||
						(entryNumber.CE_EntryType == CusEntryNumberTypes.HongKong.ImportLicense && shipment.IsImport()))
					{
						return entryNumber.CE_EntryNum.ToUpper().KeepChars(" ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789").Split(new char[] { ' ' }, 9);
					}
				}
			}
			return System.Array.Empty<ZString>();
		}

		public static ZDecimal GetWeightInKG(this ForwardingShipment shipment)
		{
			var result = ZDecimal.Zero;

			if (shipment != null)
			{
				result = new ZWeight(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight).InKilogramsSafe.Round(Traxon.MessageBuilders.TraxonRequest.WeightDecimals);
			}

			return result;
		}

		public static IOrgDetails GetConsigneeOrgDetails(this ForwardingShipment shipment)
		{
			var address = shipment.ConsigneeDocumentaryAddress;
			if (IsUnmatched(address))
			{
				return new UnmatchedOrgDetails(new UnmatchOrgRecords(shipment).FindUnmatchOrgByOrgType(OrganisationTypes.Consignee));
			}
			else
			{
				return new JobDocAddressDetails(address);
			}
		}

		public static IOrgDetails GetConsignorOrgDetails(this ForwardingShipment shipment)
		{
			var address = shipment.ConsignorDocumentaryAddress;
			if (IsUnmatched(address))
			{
				return new UnmatchedOrgDetails(new UnmatchOrgRecords(shipment).FindUnmatchOrgByOrgType(OrganisationTypes.Consignor));
			}
			else
			{
				return new JobDocAddressDetails(address);
			}
		}

		public static IOrgDetails GetNotifyPartyOrgDetails(this ForwardingShipment shipment)
		{
			var address = shipment.NotifyPartyDocumentaryAddress;
			if (IsUnmatched(address))
			{
				return new UnmatchedOrgDetails(null);
			}
			else
			{
				return new JobDocAddressDetails(address);
			}
		}

		static bool IsUnmatched(JobDocAddress address)
		{
			return address.Organisation != null && address.Organisation.PK == OrgHeader.UnmatchedOrganisationPK;
		}

		public static bool SendOtherInfoForShipment(this ForwardingShipment shipment, ForwardingConsol consol)
		{
			var result = false;
			var countryPks = HKDataRegistry.Instance.SendOtherCustomsInformation.Value;
			if (shipment != null && consol != null && (!shipment.IsImport() || countryPks.Contains(Constants.CountryGuids.HongKong)))
			{
				var dischargeCountryCode = consol.JK_RL_NKDischargePort.Trim().SubstringSafe(0, 2);
				if (!dischargeCountryCode.IsEmpty)
				{
					var countryPk = RefCountry.LoadFromCountryCode(consol.Factory, dischargeCountryCode)?.PK ?? ZGuid.Empty;
					if (!countryPk.IsEmpty && countryPks.Contains(countryPk.ToGuid()))
					{
						result = true;
					}
				}
			}

			return result;
		}

		public static bool SendACASForShipment(this ForwardingShipment shipment)
		{
			var result = false;
			var countryPks = HKDataRegistry.Instance.SendOtherCustomsInformation.Value;
			if (countryPks.Contains(Constants.CountryGuids.UnitedStates) && shipment != null && (!shipment.IsImport() || countryPks.Contains(Constants.CountryGuids.HongKong)))
			{
				result = shipment.AWBHeader?.GetACASCountryHandler().ShouldApplyACAS() ?? false;
			}

			return result;
		}
	}
}
