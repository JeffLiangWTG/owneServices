using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Synchronisers;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillSynchroniser : ManifestBillSynchroniser<AsycudaBill>
	{
		public AsycudaBillSynchroniser(AsycudaBill destination, ForwardingShipment shipmentSource)
			: base(destination, shipmentSource)
		{
		}

		protected override Customs.Business.ConsolDataCalculator ConsolDataCalculator
		{
			get { return new ConsolDataCalculator(base.consolSource, ""); }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_MarksAndNumbersInfo, Source.JS_MarksAndNumbersInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_GoodsDescriptionInfo, () => GetGoodsDescriptionType(), () => GetSourceGoodsDescriptionInfo()));
			HookFreightValueSynchronisers(Synchronisers);
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_InsuranceValueInfo, Source.JS_InsuranceValueInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_RX_NKInsuranceValueCurrencyInfo, Source.JS_RX_NKInsuranceCurrencyInfo));
			Synchronisers.Add(GetAsycudaPackCollectionSynchroniser());
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_BolTypeInfo, GetSourceShipmentType, GetInfosAffectingShipmentType));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_JS_ShipmentInfo, GetSourceShipmentPK, GetNoInfo));
			var asycudaTransportDocumentCollectionSynchroniser = GetAsycudaTransportDocumentCollectionSynchroniser();
			if (asycudaTransportDocumentCollectionSynchroniser != null)
			{
				Synchronisers.Add(asycudaTransportDocumentCollectionSynchroniser);
			}
			if (Destination.SynchronisePaymentType)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.ABL_PrepaidCollectInfo, () => GetPaymentType(), () => GetPaymentInfo()));
			}
		}

		protected virtual AsycudaPackCollectionSynchroniser GetAsycudaPackCollectionSynchroniser() => new AsycudaPackCollectionSynchroniser(Source, Destination);

		protected virtual AsycudaTransportDocumentCollectionSynchroniser GetAsycudaTransportDocumentCollectionSynchroniser() => null;

		protected virtual void HookFreightValueSynchronisers(List<ISynchroniser> synchronisers)
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_FreightValueInfo, Source.JS_GoodsValueInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_RX_NKFreightValueCurrencyInfo, Source.JS_RX_NKGoodsValueCurrInfo));
		}

		protected IZType GetGoodsDescriptionType()
		{
			return Source.DetailedGoodsDescriptionNoteText != string.Empty ? Source.DetailedGoodsDescriptionNoteText : Source.JS_GoodsDescription;
		}

		IEnumerable<ZPropertyInfo> GetSourceGoodsDescriptionInfo()
		{
			return new ZPropertyInfo[] { Source.DetailedGoodsDescriptionNoteTextInfo, Source.JS_GoodsDescriptionInfo };
		}

		protected override void AddPacksSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_ManifestUQInfo, GetPackType, GetSourcevaluesAffectingPackType));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_ManifestQtyInfo, GetPackQty, GetSourcevaluesAffectingPackQty));
		}

		IEnumerable<ZPropertyInfo> GetSourcevaluesAffectingPackType()
		{
			yield return Source.JS_F3_NKPackTypeInfo;
		}

		IZType GetPackType()
		{
			var result = Source.JS_F3_NKPackType;
			if (Destination.IsManifestUQNeedToConvert)
			{
				var refPack = AsycudaPackHelper.LoadRefPackForManifestBill(Destination.Factory, Source.JS_F3_NKPackType, Destination.CountryCode);
				result = refPack == null ? Source.JS_F3_NKPackType : refPack.RP_CustomsPack;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourcevaluesAffectingPackQty()
		{
			yield return Source.JS_OuterPacksInfo;
			yield return Source.JS_F3_NKPackTypeInfo;
		}

		IZType GetPackQty()
		{
			var result = Source.JS_OuterPacks;
			if (Destination.IsManifestUQNeedToConvert)
			{
				var refPack = AsycudaPackHelper.LoadRefPackForManifestBill(Destination.Factory, Source.JS_F3_NKPackType, Destination.CountryCode);
				result = refPack == null ? Source.JS_OuterPacks : new ZDecimal(refPack.ConversionFactor * Source.JS_OuterPacks).ToZInt();
			}
			return result;
		}

		protected override BusinessObjectSynchroniser GetAddressSynchroniser(IManifestBillAddress address, JobDocAddress source)
		{
			return new AsycudaBillAddressSynchroniser(address, source);
		}

		protected override IZType GetPlaceOfReceipt()
		{
			return Source.JS_RL_NKDestination;
		}
		protected override IEnumerable<ZPropertyInfo> GetInfosAffectingPlaceOfReceipt()
		{
			return new[] { Source.JS_RL_NKDestinationInfo };
		}

		protected override IZType GetPortOfLading()
		{
			return Source.JS_RL_NKOrigin;
		}

		protected override IEnumerable<ZPropertyInfo> GetInfosAffectingPortOfLading()
		{
			return new[] { Source.JS_RL_NKOriginInfo };
		}

		protected override IZType GetLastForeignPort()
		{
			return Source.JS_RL_NKOrigin;
		}
		protected override IEnumerable<ZPropertyInfo> GetLastForeignPortInfo()
		{
			return new[] { Source.JS_RL_NKOriginInfo };
		}

		protected virtual IZType GetSourceShipmentType()
		{
			return new ZString(Source.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster ? Core.Constants.ShipmentTypes.CoLoadMaster : Core.Constants.ShipmentTypes.StandardHouse);
		}
		protected IEnumerable<ZPropertyInfo> GetInfosAffectingShipmentType()
		{
			return new[] { Source.JS_ShipmentTypeInfo };
		}

		protected IZType GetSourceShipmentPK()
		{
			return Source.PK;
		}
		protected IEnumerable<ZPropertyInfo> GetNoInfo()
		{
			return System.Array.Empty<ZPropertyInfo>();
		}

		protected virtual IZType GetPaymentType()
		{
			return Source.JS_PaymentTermAutoratingOverride == PaymentType.Collect ? (ZString)DomesticPaymentTerms.Collect : Source.JS_PaymentTermAutoratingOverride;
		}
		protected virtual IEnumerable<ZPropertyInfo> GetPaymentInfo() => new [] { Source.JS_PaymentTermAutoratingOverrideInfo };
	}
}
