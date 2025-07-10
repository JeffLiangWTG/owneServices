using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureLineWrapper : NctsDepartureBaseDepartureLineWrapper, IDepartureLine
	{
		public DepartureLineWrapper(NctsDepartureCargoDesc goodsItem)
			: base(goodsItem)
		{
		}

		public ZString CountryOfDeparture => goodsItem.BY_RN_NKCountryOfDispatch;

		public ZString CountryOfDestination => goodsItem.BY_RN_NKCountryOfDestination;

		public ZString GoodsCustomsProcedureCategory5 => goodsItem.BY_Type;

		public ZString GoodsCountryOfOrigin => goodsItem.BY_RN_NKCountryOfDispatch;

		public ZString GoodsCountryOfDestination => goodsItem.BY_RN_NKCountryOfDestination;

		public ZDecimal NetWeightInKG => goodsItem.NetMassInKilograms;

		public ZDecimal FiscalUnitsNumber => goodsItem.BY_CustomsSecondQuantity;

		public ZString FiscalUnitsQualifier => goodsItem.BY_CustomsSecondUnitQty.ConvertCargoWiseToES(goodsItem.Factory);

		public IPartyProvider GoodsConsignor => CachedValueHelper.GetValue(ref goodsConsignor, () => PartyWrapper.New(goodsItem.Consignor));
		CachedValue<IPartyProvider> goodsConsignor;

		public IPartyProvider GoodsConsignee => CachedValueHelper.GetValue(ref goodsConsignee, () => NctsHeaderConsigneeWrapper.New(goodsItem.Consignee));
		CachedValue<IPartyProvider> goodsConsignee;

		public IPartyProvider SecurityGoodsConsignor => CachedValueHelper.GetValue(ref securityGoodsConsignor, () => PartyWrapper.New(goodsItem.SecurityConsignor));
		CachedValue<IPartyProvider> securityGoodsConsignor;

		public IPartyProvider SecurityGoodsConsignee => CachedValueHelper.GetValue(ref securityGoodsConsignee, () => PartyWrapper.New(goodsItem.SecurityConsignee));
		CachedValue<IPartyProvider> securityGoodsConsignee;

		public IDepartureInternalPackagesInfo InternalPackages => internalPackages ?? (internalPackages = new DepartureInternalPackagesInfoWrapper(goodsItem));
		DepartureInternalPackagesInfoWrapper internalPackages;

		public IVehiclePackagesInfoCommon VehiclePackages => vehiclePackages ?? (vehiclePackages = goodsItem.IsVehicles ? new DepartureVehiclePackagesInfoWrapper(goodsItem) : null);
		DepartureVehiclePackagesInfoWrapper vehiclePackages;

		public ZDecimal TotalGoodValueInEuros => goodsItem.BY_MonetaryValue;

		public IReadOnlyCollection<IDepartureDocuments> Documents
		{
			get
			{
				if (documents == null)
				{
					documents = goodsItem.SupportingDocuments
						.Cast<NctsSupportingDocument>()
						.Select(doc => new DepartureDocumentWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))
						.ToList().AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<DepartureDocumentWrapper> documents;

		public ZString GoodsTransportMethodOfPayment => goodsItem.AdditionalInfos.Count > 0 ? goodsItem.AdditionalInfos[0].CSI_Code : ZString.Empty;

		public ZString GoodsCountryCode => goodsItem.AdditionalInfos.Count > 0 ? goodsItem.AdditionalInfos[0].CSI_RN_NKCountryCode : ZString.Empty;
	}
}
