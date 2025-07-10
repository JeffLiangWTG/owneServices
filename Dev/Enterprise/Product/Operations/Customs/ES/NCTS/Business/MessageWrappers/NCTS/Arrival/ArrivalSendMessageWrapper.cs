using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalSendMessageWrapper : NctsHeaderMessageWrapper, IArrivalMessageDataProvider
	{
		public ArrivalSendMessageWrapper(NctsHeader header, ICertificateProvider certificateData, ZString arrivalType)
			: base(header, certificateData)
		{
			DocumentMessageName = Argument.NotNullOrEmpty(arrivalType, nameof(arrivalType));
		}

		public ZBool IsOnlyArrivalNotification => DocumentMessageName == DeclarationMessageTypeList.Codes.NctsArrivalNotification;

		public ZBool IsOnlyUnloadingRemarks => DocumentMessageName == DeclarationMessageTypeList.Codes.NctsUnloadingRemarks;

		public ZBool IsArrivalWithAVI => DeclarationMessageTypeList.IsArrivalWithAVI(DocumentMessageName);

		public ZBool IsArrivalWithOBS => DeclarationMessageTypeList.IsArrivalWithOBS(DocumentMessageName);

		public ZBool IsArrivalWithTNN => DeclarationMessageTypeList.IsArrivalWithTNN(DocumentMessageName);

		public ZString DocumentMessageName { get; }

		public ZString CustomsProcedureCategory1 => nctsHeader.MovementHeader?.BM_InBondEntryType ?? ZString.Empty; //Taken form NCTSDeparture declaration

		public ZString CustomsProcedureCategory2 => nctsHeader.DepartureCustomsOffice?.OfficeCode ?? ZString.Empty;

		public ZString CustomsTransitDestinationOffice => nctsHeader.DestinationCustomsOffice?.OfficeCode.Right(6) ?? ZString.Empty;

		public IArrivalCustomsEffectiveDestinationOffice CustomsOfficesOfDestination => CachedValueHelper.GetValue(ref customsOfficesOfDestinationCached, () => new ArrivalCustomsEffectiveDestinationOfficeWrapper(nctsHeader));
		CachedValue<IArrivalCustomsEffectiveDestinationOffice> customsOfficesOfDestinationCached;

		public ZDateTime DateOfArrival => nctsHeader.ArrivalMovementHeader.BM_EntryDate;

		public ZDateTime DateOfUnloading => nctsHeader.UnloadingRemark.G9_UnloadingDate;

		public ZString GoodsInContainerIndicator => nctsHeader.DepartureHeaderContainers.Count > 0 ? ContainerIndicator1 : ContainerIndicator0;
		const string ContainerIndicator0 = "0";
		const string ContainerIndicator1 = "1";

		public ZString UnloadingComplianceIndicator => nctsHeader.UnloadingRemark.G9_Conform == YesNoList.Codes.Yes ? UnloadingComplianceIndicator1 : UnloadingComplianceIndicator0;
		const string UnloadingComplianceIndicator0 = "0";
		const string UnloadingComplianceIndicator1 = "1";

		public ZString SealsInGoodState => nctsHeader.UnloadingRemark.G9_StateOfSealsOk == YesNoList.Codes.Yes ? SealsInGoodStateTrue : SealsInGoodStateFalse;
		const string SealsInGoodStateFalse = "M";
		const string SealsInGoodStateTrue = "B";

		public ZString AttachedDocumentTypeA
		{
			get
			{
				var bondEntryType = nctsHeader.ArrivalMovementHeader.BM_InBondEntryType;
				return bondEntryType.Equals(AttachedDocDAT) ? AttachedDocA :
					bondEntryType.Equals(AttachedDocDUA) ? AttachedDoc4 :
					string.Empty;
			}
		}

		const string AttachedDocDAT = "DAT";
		const string AttachedDocA = "A";
		const string AttachedDocDUA = "DUA";
		const string AttachedDoc4 = "4";

		public ZBool ReceiverComplianceForAutoDischarge => nctsHeader.ESNctsHeader.CEN_AutomaticCompletion;

		public ZString DirectlyLoadedOnCompletion => nctsHeader.ESNctsHeader.CEN_AutomaticTranshipment ? DirectlyLoadedOnCompletion1 : DirectlyLoadedOnCompletion0;
		const string DirectlyLoadedOnCompletion0 = "0";
		const string DirectlyLoadedOnCompletion1 = "1";

		public IReadOnlyCollection<string> SealsStateDiscrepancies
		{
			get
			{
				if (sealsStateDiscrepancies == null)
				{
					const string discrepanceCode1 = "1";
					const string discrepanceCode2 = "2";
					sealsStateDiscrepancies = nctsHeader.Seals
						.Cast<Seal>()
						.Select(seal => seal.IsBroken ? discrepanceCode2 : discrepanceCode1)
						.ToList().AsReadOnly();
				}
				return sealsStateDiscrepancies;
			}
		}
		IReadOnlyCollection<string> sealsStateDiscrepancies;

		public ZString TIRCompletionNumber => nctsHeader.ESNctsHeader.CEN_TIRArrival ? nctsHeader.ESNctsHeader.CEN_TIRCarnetPage.ToString() : string.Empty;

		public ZString TIRIsCompleteUnloading => nctsHeader.ESNctsHeader.CEN_TIRArrival ?
													(nctsHeader.ESNctsHeader.CEN_TIRPartialUnloading ? CompleteUnloadingP : CompleteUnloadingT) :
													string.Empty;
		const string CompleteUnloadingP = "P";
		const string CompleteUnloadingT = "T";

		public IReadOnlyCollection<ZString> SealCodes => sealCodes ?? (sealCodes = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Select(x => ZString.Join(new ZString[] { x.BC_Seal1, x.BC_Seal2 })).ToList().AsReadOnly());
		IReadOnlyCollection<ZString> sealCodes;

		public IReadOnlyCollection<ZString> SealCodesWithDiscrepancies => sealCodesWithDiscrepancies ?? (sealCodesWithDiscrepancies = nctsHeader.Seals.Cast<Seal>().Select(x => x.CY_Data).ToList().AsReadOnly());
		IReadOnlyCollection<ZString> sealCodesWithDiscrepancies;

		public IArrivalReferencesGroup ReferenceGroup => CachedValueHelper.GetValue(ref referenceGroupCached, () => new ArrivalReferencesGroupWrapper(nctsHeader));
		CachedValue<IArrivalReferencesGroup> referenceGroupCached;

		public ZString TransitTransportMedium => nctsHeader.UnloadedMeansOfTransportAtDepartureIdentity;

		public ZString TransportId => TransitTransportMedium;

		public ZString TransportNationality => nctsHeader.UnloadedMeansOfTransportAtDepartureNationality;

		public IPartyNameProvider Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyNameWrapper.New(nctsHeader.DeclarantAddress));
		CachedValue<IPartyNameProvider> declarantCached;

		public ZString UnloadingObservations => nctsHeader.HeaderUnloadingNotes;

		public ZBool IsDeclarationInEuros => true;

		public IReadOnlyCollection<IArrivalLine> Lines => lines ?? (lines = GetLines());
		IReadOnlyCollection<IArrivalLine> lines;

		IReadOnlyCollection<IArrivalLine> GetLines()
		{
			var arrivalLines = new List<IArrivalLine>();
			if (IsArrivalWithTNN)
			{
				arrivalLines.AddRange(nctsHeader.MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Select(x => new ArrivalLineWrapperForDeparture(x)));
			}
			if (IsArrivalWithOBS)
			{
				arrivalLines.AddRange(nctsHeader.UnloadingMovementHeader.GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Where(x => x.ShouldAddUnloadingGoodsToWrapper).Select(x => new ArrivalLineWrapper(x)));
			}
			return arrivalLines.ToList().AsReadOnly();
		}

		public ZInt TotalNumberOfGoods => CachedValueHelper.GetValue(ref totalNumberOfGoodsCached, () =>
		{
			var numberOfGoods = ZInt.Zero;
			if (IsArrivalWithTNN)
			{
				numberOfGoods += nctsHeader.MovementHeader.GoodsItems.Count;
			}
			if (IsArrivalWithOBS)
			{
				numberOfGoods += nctsHeader.UnloadingMovementHeader.GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Count(x => x.ShouldAddUnloadingGoodsToWrapper);
			}
			return numberOfGoods;
		});
		CachedValue<ZInt> totalNumberOfGoodsCached;

		public ZLong TotalNumberOfPackageElements => CachedValueHelper.GetValue(ref totalNumberOfPackageElementsCached, () =>
		{
			var amount = ZLong.Zero;
			if (IsArrivalWithTNN)
			{
				amount += nctsHeader.MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Sum(goodItem => GetPackageAmountFromDepartureGoodItem(goodItem));
			}
			if (IsArrivalWithOBS)
			{
				amount += nctsHeader.UnloadingMovementHeader.GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Sum(goodItem => GetPackageAmountFromNonDepartureGoodItem(goodItem));
			}
			return amount;
		});
		CachedValue<ZLong> totalNumberOfPackageElementsCached;

		protected override ZString LocalReferenceNumberCore => DocumentMessageName + base.LocalReferenceNumberCore;
	}
}
