using System;
using System.Collections.Generic;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class BLArgentinaWrapperManifest : ISeaManifest
	{
		public BLArgentinaWrapperManifest(AsycudaBill bill)
		{
			this.bill = CargoWise.Common.Argument.NotNull(bill, "AsycudaBill cannot be null");
			header = this.bill.Header;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;

		string ISeaManifest.VoyageID => header.RegistrationNumber;

		string ISeaManifest.LoadingPort => header.AMA_CustomsLoadPort;

		string ISeaManifest.MasterNumber => header.AMA_CustomsLoadPort + header.AMA_MasterBill;

		DateTime ISeaManifest.DepartureDate => header.AMA_E_DEP.IsValid ? header.AMA_E_DEP.ToDateTime() : default;

		string ISeaManifest.OriginName => header.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, header.AMA_RL_NKPortOfLoading)?.RL_PortName ?? string.Empty;

		DateTime ISeaManifest.HouseLoadingDate => header.AMA_E_DEP.IsValid ? header.AMA_E_DEP.ToDateTime() : default;

		string ISeaManifest.OriginCountry => GetMappedCountry(bill.ABL_CustomsLoadPort.SubstringSafe(0, 2));

		string ISeaManifest.DischargePort => bill.ABL_CustomsDischargePort;

		string ISeaManifest.ArrivalCountryCode => GetMappedCountry(bill.ABL_CustomsDischargePort.SubstringSafe(0, 2));

		string ISeaManifest.HouseNumber => bill.ABL_BillNumber;

		string ISeaManifest.MarksAndNumbers => bill.ABL_MarksAndNumbers;

		string ISeaManifest.ConsigneeName => bill.ABL_ConsigneeName;

		string ISeaManifest.NotifyName => bill.ABL_NotifyPartyName;

		string ISeaManifest.ConsolidatedIndicator => bill.ABL_CargoStatus == CargoStatusList.Codes.FullShipment ? ARMessageConstants.BooleanTrueString : Core.Constants.BooleanFalseString;

		string ISeaManifest.TransshipmentIndicator => header.AMA_RL_NKPortOfDischarge == bill.ABL_RL_NKFinalDestination ? Core.Constants.BooleanFalseString : ARMessageConstants.BooleanTrueString;

		string ISeaManifest.ConsigneeRegistrationNumberType => bill.ABL_ConsigneeRegNoType;

		int ISeaManifest.ConsigneeRegistrationNumber => bill.ABL_ConsigneeRegNo.IsEmpty ? ZInt.Zero : ZInt.Parse(bill.ABL_ConsigneeRegNo);

		string ISeaManifest.ConsigneeCountry => GetMappedCountry(bill.ConsigneeCountry?.Code ?? string.Empty);

		string ISeaManifest.TariffPosition => string.Empty; // Will be filled in WI00431626

		string ISeaManifest.SecureLogisticalOperatorIndicator => ARMessageConstants.BooleanTrueString;

		string ISeaManifest.MonitoredTransitIndicator => bill.ABL_IsMonitoredTransit ? ARMessageConstants.BooleanTrueString : Core.Constants.BooleanFalseString;

		string ISeaManifest.RenarIndicator => bill.ABL_IsInformedToRenar ? ARMessageConstants.BooleanTrueString : Core.Constants.BooleanFalseString;

		string ISeaManifest.ShipperName => bill.ABL_ShipperName;

		string ISeaManifest.ShipperRegistrationNumber => bill.ABL_ShipperRegNo;

		string ISeaManifest.ShipperCountry => GetMappedCountry(bill.ShipperCountry?.Code ?? string.Empty);

		string ISeaManifest.CustomsOffice => header.AMA_CustomsOffice;

		string ISeaManifest.Description => bill.ABL_GoodsDescription;

		IReadOnlyCollection<IPack> ISeaManifest.Packs
		{
			get
			{
				var result = new List<IPack>();

				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new BLArgentinaWrapperPack(pack));
				}

				return result.ToArray();
			}
		}

		IReadOnlyCollection<IContainer> ISeaManifest.Containers
		{
			get
			{
				var result = new List<IContainer>();

				foreach (AsycudaPack pack in bill.Packs)
				{
					var container = (AsycudaContainer)pack.Container;
					if (container != null)
					{
						result.Add(new BLArgentinaWrapperContainer(container));
					}
				}

				return result.ToArray();
			}
		}

		IAuth ISeaManifest.Authentication => new BLArgentinaWrapperAuth();

		string GetMappedCountry(string countryCode)
		{
			var query = new ZQuery(RefCusMapSchema.ZZM_CW1orCommercialValue, countryCode);
			query.AddToFilter(RefCusMapSchema.ZZM_ZZP_NKMapType, ARMessageConstants.CountryMapType);
			query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Argentina);
			var cusMap = header.Factory.LoadTop1<RefCusMap>(query);

			return cusMap?.ZZM_CustomsValue ?? string.Empty;
		}
	}
}
