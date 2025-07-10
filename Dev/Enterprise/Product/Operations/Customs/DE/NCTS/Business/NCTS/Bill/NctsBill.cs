using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsBill : EU.NCTS.Business.NctsBill
	{
		public NctsBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobDocAddress EffectiveConsignor => Factory.GetValue(ref effectiveConsignor, () =>
			Consignor.GetDocAddressOrNullIfInvalid() ??
			Header.Consignor.GetDocAddressOrNullIfInvalid());
		CachedProperty<JobDocAddress> effectiveConsignor;

		public JobDocAddress EffectiveConsignee => Factory.GetValue(ref effectiveConsignee, () =>
			Consignee.GetDocAddressOrNullIfInvalid() ??
			Header.Consignee.GetDocAddressOrNullIfInvalid());
		CachedProperty<JobDocAddress> effectiveConsignee;

		public string EffectiveCountryOfDispatch => B0_RN_NKCountryOfExport.ValueOrNullIfEmpty() ?? Header.MovementHeader.BM_RN_NKCountryOfDispatch.ValueOrNullIfEmpty();

		public string EffectiveCountryOfDestination => B0_RN_NKCountryOfDestination.ValueOrNullIfEmpty() ?? Header.MovementHeader.BM_RL_NKDestinationPort.ValueOrNullIfEmpty();

		public string EffectiveReferenceNumberUCR => B0_ReferenceID.ValueOrNullIfEmpty() ?? Header.MovementHeader.BM_UniqueConsignmentReference.ValueOrNullIfEmpty();

		public new EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

		public new EU.NCTS.Business.INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> ArrivalGoodsItems => (EU.NCTS.Business.INctsArrivalCargoDescCollection<NctsArrivalCargoDesc>)base.ArrivalGoodsItems;

		public new EU.NCTS.Business.INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> AdditionalDocuments => (EU.NCTS.Business.INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)base.AdditionalDocuments;

		public new EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments => (EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;

		public new EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos => (EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)base.ArrivalTransportInfos;

		protected override EU.NCTS.Business.INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		protected override EU.NCTS.Business.INctsArrivalCargoDescCollection<EU.NCTS.Business.NctsArrivalCargoDesc> GetNewArrivalGoodsItems() => new EU.NCTS.Business.NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>(this);

		protected override EU.NCTS.Business.INctsBillAdditionalDocumentCollection<EU.NCTS.Business.NctsBillAdditionalDocument> GetAdditionalDocuments()
		{
			var result = new EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>(this);
			result.SetReadOnlyIncludingChildren(IsPhase5Arrival);
			return result;
		}

		protected override EU.NCTS.Business.ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments()
		{
			var result = new EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);
			result.SetReadOnlyIncludingChildren(IsPhase5Arrival);
			return result;
		}

		protected override EU.NCTS.Business.IArrivalCusTransportMeansCollection<EU.NCTS.Business.ArrivalCusTransportMeans> GetNewArrivalTransportInfos() => new EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments()
		{
			var result = new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);
			result.SetReadOnlyIncludingChildren(IsPhase5Arrival);
			return result;
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(NctsBillAdditionalDocument);
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(NctsSupportingDocument);
			return cusSupportingInfoTypes;
		}

		public new EU.NCTS.Business.IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> DepartureTransportInfos => (EU.NCTS.Business.DepartureCusTransportMeansCollection<DepartureCusTransportMeans>)base.DepartureTransportInfos;

		protected override EU.NCTS.Business.IDepartureCusTransportMeansCollection<EU.NCTS.Business.DepartureCusTransportMeans> GetNewDepartureTransportInfos() => new EU.NCTS.Business.DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(this);

		protected override EU.NCTS.Business.INctsCustomsEntryIntegrator GetCustomsEntryIntegratorCore() => new NctsBillCustomsEntryIntegrator(this);
	}
}
