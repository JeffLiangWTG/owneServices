using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonHolderOfTheTransitProcedureWrapper : PartyIdWrapper, INCTSCommonHolderOfTheTransitProcedure
	{
		public static NCTS5CommonHolderOfTheTransitProcedureWrapper New(NctsHeader nctsHeader) =>
						nctsHeader?.Principal?.Address?.Header == null
																? null
																: new NCTS5CommonHolderOfTheTransitProcedureWrapper(nctsHeader?.Principal?.Address?.Header, (nctsHeader?.MovementHeader?.BM_InBondEntryType ?? ZString.Empty) == NctsPhase5DeclarationTypeList.Codes.TIR);

		protected NCTS5CommonHolderOfTheTransitProcedureWrapper(OrgHeader orgH, ZBool isTIR) : base(orgH)
		{
			this.isTIR = isTIR;
		}
		protected readonly ZBool isTIR;

		public ZString TIRHolderIdentificationNumber => isTIR ? base.IdCore : ZString.Empty;

		protected override ZString IdCore => isTIR ? ZString.Empty : base.IdCore;
	}
}
