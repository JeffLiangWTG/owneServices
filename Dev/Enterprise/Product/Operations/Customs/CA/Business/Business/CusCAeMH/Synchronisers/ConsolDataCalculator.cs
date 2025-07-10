using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolDataCalculator : Customs.Business.ConsolDataCalculator
	{
		public ConsolDataCalculator(ForwardingConsol consol, CusCAeMHMaster masterBill)
			: base(consol, Core.Constants.CountryCodes.Canada)
		{
			this.masterBill = masterBill;
			HookToMasterBillEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation();
		}
		readonly CusCAeMHMaster masterBill;

		public override IEnumerable<ZPropertyInfo> GetInfosAffectingTransportsOrder()
		{
			foreach (var info in base.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}
			yield return masterBill.BP_ModeOfTransportInfo;
		}

		void HookToMasterBillEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation()
		{
			masterBill.BP_ModeOfTransportInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
			masterBill.BP_ModeOfTransportInfo.ValueChanged += TransportsOrderAffecting_ValueChanged;
		}

		void UnHookToMasterBillEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation()
		{
			masterBill.BP_ModeOfTransportInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
		}

		protected override ZString GetConsolTransportMode()
		{
			return masterBill.BP_ModeOfTransport;
		}

		public override void Dispose()
		{
			base.Dispose();
			UnHookToMasterBillEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation();
		}

		public ZDateTime FirstCountryETADate
		{
			get
			{
				return GetFirstCountryDischargeDate(transport => transport.JW_ETA, transport => ZDateTime.Empty);
			}
		}

		public ZDateTime FirstCountryATADate
		{
			get
			{
				return GetFirstCountryDischargeDate(transport => ZDateTime.Empty, transport => transport.JW_ATA);
			}
		}

		public IZType GetDischargePort()
		{
			return FirstCountryPortOfDischarge?.RL_Code ?? ZString.Empty;
		}

		public IZType GetSubMasterCCN()
		{
			var subMasterCCN = consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.PCN);
			return subMasterCCN != null ? subMasterCCN.CE_EntryNum : ZString.Empty;
		}

		public IZType GetPrimaryCCN()
		{
			var primaryCCN = consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			return primaryCCN != null ? primaryCCN.CE_EntryNum : ZString.Empty;
		}

		IEnumerable<OrgAddress> GetSubLocationRelatedAddresses()
		{
			if (consol.ArrivalCTOAddress is OrgAddress ctoAddress)
			{
				yield return ctoAddress;
			}
			if (consol.UnpackDepotAddress is OrgAddress cfsAddress)
			{
				yield return cfsAddress;
			}
		}

		public IZType GetCBSADischargePort()
		{
			var result = ZString.Empty;
			var transportMode = consol.JK_TransportMode;
			if (transportMode == TransportTypeList.Codes.Air || transportMode == TransportTypeList.Codes.Sea)
			{
				var transports = GetTransports().ToList();
				if (transports.Count > 0 && !transports[0].JW_RL_NKLoadPort.IsCanadaPort())
				{
					var caDiscPorts = transports.Where(x => x.JW_RL_NKDiscPort.IsCanadaPort()).Take(2).ToArray();
					var caDiscPortCount = caDiscPorts.Length;
					if (caDiscPortCount >= 1)
					{
						if (transportMode == TransportTypeList.Codes.Air)
						{
							var lastTransport = transports.Last();
							if (caDiscPortCount == 1 && lastTransport.JW_RL_NKDiscPort.IsCanadaPort())
							{
								result = consol.ArrivalCTOAddress.GetCusCodesFromOrgAddress(OrgCusCode.CACodeTypes.CustomsOfficeCode);
							}
							else
							{
								result = caDiscPorts[0].GetFirstLocoMapFromDischargePort(CACustomsCodeType.Office, transportMode);
							}
						}
						if (result.IsEmpty && ShouldDefaultCustomsCodes)
						{
							result = caDiscPorts[0].GetFirstLocoMapFromDischargePort(CACustomsCodeType.Office, transportMode);
						}
					}
				}
			}
			else if (!ShouldDefaultCustomsCodes)
			{
				var custPortDefaulter = new CustomsCodesDefaulter(consol.Factory, masterBill.BP_CBSADischargePortInfo, GetSubLocationRelatedAddresses, GetTransports, () => consol.TransportMode, OrgCusCode.CACodeTypes.CustomsOfficeCode, true);
				result = custPortDefaulter.GetDefaultCustomsCode();
			}

			return result;
		}

		public IEnumerable<Transport> GetTransports()
		{
			return consol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder);
		}

		public IZType GetSubLocation()
		{
			var result = ZString.Empty;
			var transPortMode = consol.JK_TransportMode;
			if (transPortMode == TransportTypeList.Codes.Air)
			{
				var transports = GetTransports().ToList();
				if (transports.Count > 0 && !transports[0].JW_RL_NKLoadPort.IsCanadaPort())
				{
					var caDiscPorts = transports.Where(x => x.JW_RL_NKDiscPort.IsCanadaPort()).Take(2).ToArray();
					var caDiscPortCount = caDiscPorts.Length;
					if (caDiscPortCount == 1)
					{
						var lastTransport = transports.Last();
						if (lastTransport.JW_RL_NKDiscPort.IsCanadaPort())
						{
							result = consol.ArrivalCTOAddress.GetCusCodesFromOrgAddress(OrgCusCode.CodeTypes.ControlledPremisesID);
						}
						else
						{
							result = caDiscPorts[0].GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID);
						}
						if (result.IsEmpty && ShouldDefaultCustomsCodes)
						{
							result = caDiscPorts[0].GetFirstLocoMapFromDischargePort(CACustomsCodeType.SubLocation, TransportTypeList.Codes.Air);
						}
					}
					else if (caDiscPortCount > 1)
					{
						result = caDiscPorts[0].GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID);
					}
				}
			}
			else if (transPortMode != TransportTypeList.Codes.Sea && !ShouldDefaultCustomsCodes)
			{
				var sublocationDefaulter = new CustomsCodesDefaulter(consol.Factory, masterBill.BP_CBSADischargeSubLocationInfo, GetSubLocationRelatedAddresses, GetTransports, () => consol.TransportMode, OrgCusCode.CodeTypes.ControlledPremisesID, false);
				return sublocationDefaulter.GetDefaultCustomsCode();
			}

			return result;
		}

		ZBool ShouldDefaultCustomsCodes => _shouldDefaultCustomsCodes ?? (_shouldDefaultCustomsCodes = CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.GetFallBackValueAtAllLevels(CompanyPK.ToGuid(), BranchPK.ToGuid(), Guid.Empty)).Value;
		ZBool? _shouldDefaultCustomsCodes;

		ZGuid CompanyPK => masterBill.Company.PK;
		ZGuid BranchPK => masterBill.Branch?.PK ?? ZGuid.Empty;
	}
}
