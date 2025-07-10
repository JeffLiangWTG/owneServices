using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class DEPDATHouseConsignmentProvider : IDEPDATHouseConsignment
	{
		public static DEPDATHouseConsignmentProvider NewOrNull(NctsBill nctsBill, IDEPDATHeader headerProvider) => nctsBill != null && headerProvider != null ? new DEPDATHouseConsignmentProvider(nctsBill, headerProvider) : null;

		DEPDATHouseConsignmentProvider(NctsBill nctsBill, IDEPDATHeader headerProvider)
		{
			this.nctsBill = Argument.NotNull(nctsBill, nameof(nctsBill));
			this.headerProvider = Argument.NotNull(headerProvider, nameof(headerProvider));
		}
		readonly NctsBill nctsBill;
		readonly IDEPDATHeader headerProvider;

		public int SequenceNumber => nctsBill.SequenceNumber;

		public string CountryOfDispatch => CachedValueHelper.GetValue(ref countryOfDispatch,
			() => NotInPhase5TransitionPeriod && string.IsNullOrWhiteSpace(headerProvider.CountryOfDispatch)
				? GoodsItems.SameOrDefault(x => x.EffectiveCountryOfDispatch)
				: null);
		CachedValue<string> countryOfDispatch;

		public string CountryOfDestination => CachedValueHelper.GetValue(ref countryOfDestination,
			() => NotInPhase5TransitionPeriod && string.IsNullOrWhiteSpace(headerProvider.CountryOfDestination)
				? GoodsItems.SameOrDefault(x => x.EffectiveCountryOfDestination)
				: null);
		CachedValue<string> countryOfDestination;

		public decimal GrossMass => nctsBill.B0_Weight.Round(3).Normalize();

		public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR,
			() => NotInPhase5TransitionPeriod && string.IsNullOrWhiteSpace(headerProvider.ReferenceNumberUCR)
				? GoodsItems.SameOrDefault(x => x.EffectiveReferenceNumberUCR)
				: null);
		CachedValue<string> referenceNumberUCR;

		public INCTSPartyIDAddressContact Consignor => CachedValueHelper.GetValue(ref consignor,
			() => NotInPhase5TransitionPeriod && headerProvider.Consignor == null
				? NCTSPartyIDAddressContactProvider.NewOrNull(nctsBill.EffectiveConsignor, fallback: true)
				: null);
		CachedValue<INCTSPartyIDAddressContact> consignor;

		public INCTSPartyIDAddressContact Consignee => CachedValueHelper.GetValue(ref consignee,
			() => NotInPhase5TransitionPeriod && headerProvider.Consignee == null && headerProvider.ShouldPopulateConsignee
				? NCTSPartyIDAddressContactProvider.NewOrNull(GoodsItems.FirstOrDefault()?.EffectiveConsignee, fallback: true)
				: null);
		CachedValue<INCTSPartyIDAddressContact> consignee;

		public IReadOnlyCollection<INCTSActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = nctsBill.CusSupplyChainActorReferences.Select(x => NCTSActorProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSActor> additionalSupplyChainActors;

		public IReadOnlyCollection<INCTSDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = nctsBill.PreviousDocuments.Select(x => NCTSDocumentProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSDocument> previousDocuments;

		public IReadOnlyCollection<INCTSDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = nctsBill.SupportingDocuments.Select(x => NCTSDocumentProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSDocument> supportingDocuments;

		public IReadOnlyCollection<INCTSDocument> TransportDocuments => transportDocuments ??
			(transportDocuments = AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select(x => NCTSDocumentProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSDocument> transportDocuments;

		public IReadOnlyCollection<INCTSDocument> AdditionalReferences => additionalReferences ??
			(additionalReferences = AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select(x => NCTSDocumentProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSDocument> additionalReferences;

		public IReadOnlyCollection<INCTSAdditionalInformation> AdditionalInformation => additionalInformation ??
			(additionalInformation = AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(x => NCTSAdditionalInformationProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSAdditionalInformation> additionalInformation;

		public string MethodOfPayment => CachedValueHelper.GetValue(ref methodOfPayment,
			() => NotInPhase5TransitionPeriod && string.IsNullOrWhiteSpace(headerProvider.MethodOfPayment)
				? GoodsItems.FirstOrDefault()?.EffectiveMethodOfPayment
				: null);
		CachedValue<string> methodOfPayment;

		public IReadOnlyCollection<IDEPDATConsignmentItem> ConsignmentItems => consignmentItems ??= GoodsItems
			.Select(x => DEPDATConsignmentItemProvider.NewOrNull(x, headerProvider, this))
			.OrderBy(e => e.GoodsItemNumber).ToArray();
		IReadOnlyCollection<IDEPDATConsignmentItem> consignmentItems;

		IEnumerable<NctsDepartureCargoDesc> GoodsItems => goodsItems ?? (goodsItems = nctsBill.GoodsItems.Cast<NctsDepartureCargoDesc>().ToArray());
		NctsDepartureCargoDesc[] goodsItems;

		IEnumerable<NctsBillAdditionalDocument> AdditionalDocuments => additionalDocuments ?? (additionalDocuments = nctsBill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().ToArray());
		NctsBillAdditionalDocument[] additionalDocuments;

		bool NotInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref notInPhase5TransitionPeriod, () => !nctsBill.IsInPhase5TransitionPeriod);
		CachedValue<bool> notInPhase5TransitionPeriod;
	}
}
