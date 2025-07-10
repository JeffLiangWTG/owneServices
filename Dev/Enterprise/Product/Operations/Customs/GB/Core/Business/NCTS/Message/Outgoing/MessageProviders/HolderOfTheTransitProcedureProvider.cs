using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class HolderOfTheTransitProcedureProvider : KnownEoriPartyProvider, IHolderOfTheTransitProcedure
	{
		public HolderOfTheTransitProcedureProvider(JobDocAddress jobDocAddress, ZBool addKnownEoriLogic, bool isInPhase5TransitionPeriod) : base(jobDocAddress, addKnownEoriLogic, isInPhase5TransitionPeriod)
		{
		}

		public string TirHolderIdentificationNumber => tirHolderIdentificationNumber ?? (tirHolderIdentificationNumber = ((NctsHeader)jobDocAddress?.Parent)?.MovementHeader.BM_InBondEntryType.ToString() == Constants.OrgCusCodeTypes.TransitOperationHolder
			? jobDocAddress?.Organisation?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers).FirstOrDefault()?.OK_CustomsRegNo
			: null);
		string tirHolderIdentificationNumber;

		public new ZBool IsEmpty => string.IsNullOrEmpty(TirHolderIdentificationNumber) && base.IsEmpty;
	}
}
