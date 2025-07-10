using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsBillLookups : CusInBondBillLookups
	{
		public NctsBillLookups(NctsBill parent)
			: base(parent)
		{
		}

		public ConsigneeCollection Consignees => new ConsigneeCollection(Factory);

		public ConsignorCollection Consignors => new ConsignorCollection(Factory);

		public CodeDescriptionPairList CountryList => Factory.GetCachedCountryNC008List(Parent.DataGroupingCode);

		public CodeDescriptionPairList TransportPaymentMethodList => Factory.GetCachedValue<TransportChargesModeOfPayment>();

		public CodeDescriptionPairList UnitOfQuantityList => new CodeDescriptionPairList();

		public CodeDescriptionPairList WeightUnitList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public RefVesselCollection Vessels
		{
			get
			{
				if (Parent.TransportTypeAtDeparture == NctsTransportTypeOfIdList.Codes._10)
				{
					return new RefVesselCollection(Factory, true);
				}
				else
				{
					return new RefVesselCollection(Factory);
				}
			}
		}

		public ZZRefCusCodeListCombinedCollection TransportNationalityList => Factory.GetNCNATCountryList();

		public CodeDescriptionPairList ModeOfTransportList => Factory.GetCachedValue<ModeOfTransportList>();

		public CodeDescriptionPairList TransportAtDepartureTypeOfIdList
		{
			get
			{
				var transportMode = Parent.InlandTransportModeAtDeparture;

				return Factory.GetCachedValue("EU.NCTS.TransportAtBorderTypeOfIdList_" + transportMode, () =>
				{
					var list = new CodeDescriptionPairList();
					switch (transportMode)
					{
						case EU.Business.ModeOfTransportList.Codes._1_SeaTransport:
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._10, NctsTransportTypeOfIdList.Descriptions._10);
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._11, NctsTransportTypeOfIdList.Descriptions._11);
							break;
						case EU.Business.ModeOfTransportList.Codes._2_RailTransport:
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._20, NctsTransportTypeOfIdList.Descriptions._20);
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._21, NctsTransportTypeOfIdList.Descriptions._21);
							break;
						case EU.Business.ModeOfTransportList.Codes._4_AirTransport:
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._40, NctsTransportTypeOfIdList.Descriptions._40);
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._41, NctsTransportTypeOfIdList.Descriptions._41);
							break;
						case EU.Business.ModeOfTransportList.Codes._8_InlandWaterwayTransport:
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._80, NctsTransportTypeOfIdList.Descriptions._80);
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._81, NctsTransportTypeOfIdList.Descriptions._81);
							break;
						case EU.Business.ModeOfTransportList.Codes._3_RoadTransport:
							list.AddPairIfNotExist(NctsTransportTypeOfIdList.Codes._30, NctsTransportTypeOfIdList.Descriptions._30);
							break;
						default:
							list = new NctsTransportTypeOfIdList();
							list.RemoveCode(NctsTransportTypeOfIdList.Codes._20);
							list.RemoveCode(NctsTransportTypeOfIdList.Codes._31);
							break;
					}
					return list;
				});
			}
		}

		protected new NctsBill Parent => (NctsBill)base.Parent;

		public override RefCurrencyCollection LinePriceCurrencies
		{
			get
			{
				var filter = new ZQuery(RefCurrencySchema.RX_IsActive, true);
				filter.AddToFilter(new ZQuery(RefCurrencySchema.RX_Desc, SQLComparisonOperator.NotEqual, ZString.Empty));
				return new(Factory, filter) { AdditionalFilter = { OrderBy = RefCurrencySchema.RX_Desc.Name } };
			}
		}
	}
}
