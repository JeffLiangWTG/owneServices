using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CompleteHolderOfTheTransitProcedureWrapper : NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper, INCTSCompleteHolderOfTheTransitProcedure
	{
		public static NCTS5CompleteHolderOfTheTransitProcedureWrapper New(NctsHeader nctsHeader, ZBool isRepresentativeDeclared, bool isInPhase5TransitionPeriod) =>
						nctsHeader?.Principal?.Address?.Header == null
																? null
																: new NCTS5CompleteHolderOfTheTransitProcedureWrapper(nctsHeader?.Principal, (nctsHeader?.MovementHeader?.BM_InBondEntryType ?? ZString.Empty) == NctsPhase5DeclarationTypeList.Codes.TIR, isRepresentativeDeclared, isInPhase5TransitionPeriod);

		NCTS5CompleteHolderOfTheTransitProcedureWrapper(JobDocAddress docAd, ZBool isTIR, ZBool isRepresentativeDeclared, bool isInPhase5TransitionPeriod) : base(docAd.Address, isTIR, isInPhase5TransitionPeriod)
		{
			this.isRepresentativeDeclared = isRepresentativeDeclared;
			docAddress = docAd;
			addressOverride = docAd.E2_AddressOverride;
		}
		readonly ZBool isRepresentativeDeclared;
		readonly JobDocAddress docAddress;
		readonly ZBool addressOverride;

		public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = isRepresentativeDeclared ? null : PartyContactWrapper.New(docAddress, addressOverride));
		PartyContactWrapper contactPerson;
	}
}
