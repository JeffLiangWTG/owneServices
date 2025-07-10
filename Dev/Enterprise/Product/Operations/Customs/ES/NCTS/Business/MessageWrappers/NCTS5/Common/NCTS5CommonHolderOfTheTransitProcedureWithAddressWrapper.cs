using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper : NCTS5CommonHolderOfTheTransitProcedureWrapper, INCTSCommonHolderOfTheTransitProcedureWithAddress
	{
		public new static NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper New(NctsHeader nctsHeader) => New(nctsHeader, isInPhase5TransitionPeriod: false);

		public static NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper New(NctsHeader nctsHeader, bool isInPhase5TransitionPeriod) =>
						nctsHeader?.Principal?.Address?.Header == null
																? null
																: new NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper(nctsHeader?.Principal?.Address, (nctsHeader?.MovementHeader?.BM_InBondEntryType ?? ZString.Empty) == NctsPhase5DeclarationTypeList.Codes.TIR, isInPhase5TransitionPeriod);

		protected NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper(OrgAddress orgA, ZBool isTIR, bool isInPhase5TransitionPeriod) : base(orgA?.Header, isTIR)
		{
			orgAddress = orgA;
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}
		protected readonly OrgAddress orgAddress;
		readonly bool isInPhase5TransitionPeriod;

		public INCTSCommonAddressInfo Address => address ?? (address = (orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual
																			&& (TIRHolderIdentificationNumber == OrgHeaderExtension.GetPASCode(orgHeader)
																				|| Id == OrgHeaderExtension.GetPASCode(orgHeader)))
																		? NCTS5CommonAddressInfoWrapper.New(orgAddress, isInPhase5TransitionPeriod: isInPhase5TransitionPeriod)
																		: null);
		NCTS5CommonAddressInfoWrapper address;
	}
}
