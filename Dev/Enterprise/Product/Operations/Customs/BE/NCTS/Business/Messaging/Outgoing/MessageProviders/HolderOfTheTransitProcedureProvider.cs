using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class HolderOfTheTransitProcedureProvider : PartyProvider, IHolderOfTheTransitProcedure
	{
		readonly bool includeContactPerson;
		public HolderOfTheTransitProcedureProvider(JobDocAddress jobDocAddress, bool isTransitionPeriodAES30 = false) : this(jobDocAddress, true, isTransitionPeriodAES30: isTransitionPeriodAES30)
		{
		}

		public HolderOfTheTransitProcedureProvider(JobDocAddress jobDocAddress, bool includeContactPerson, bool isTransitionPeriodAES30 = false) : base(jobDocAddress, isTransitionPeriodAES30: isTransitionPeriodAES30)
		{
			this.includeContactPerson = includeContactPerson;
		}

		public string TirHolderIdentificationNumber => tirHolderIdentificationNumber ?? (tirHolderIdentificationNumber = ((NctsHeader)jobDocAddress?.Parent)?.MovementHeader.BM_InBondEntryType.ToString() == Constants.OrgCusCodeTypes.TransitOperationHolder
			? jobDocAddress?.Organisation?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers).FirstOrDefault()?.OK_CustomsRegNo
			: null);
		string tirHolderIdentificationNumber;

		protected override ZBool ExcludeContactPersonBasedOnData(IContactPerson contactPersonToValidate) => IsAllContactPersonDataEmpty(contactPersonToValidate);

		protected override ZBool IncludeContactPerson => includeContactPerson;

		protected override IContactPerson GetContactPersonProvider() => HolderOfTransitProcedureContactPersonProvider.NewOrNull(jobDocAddress);
	}
}
