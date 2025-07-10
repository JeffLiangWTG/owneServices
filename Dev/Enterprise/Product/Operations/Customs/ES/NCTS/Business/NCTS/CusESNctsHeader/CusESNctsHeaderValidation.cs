//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusESNctsHeaderValidation
//
//    This class should be used for overriding validation in AutoCusESNctsHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ES.NCTS.Business
{
	using CargoWise.EntityFramework;

	public class CusESNctsHeaderValidation : AutoCusESNctsHeaderValidation
	{
		public CusESNctsHeaderValidation(AutoCusESNctsHeader parent) : base(parent)
		{
		}

		new CusESNctsHeader Parent => (CusESNctsHeader)base.Parent;

		protected override void CheckCEN_NationalSimplificatorInd()
		{
			base.CheckCEN_NationalSimplificatorInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEN_NationalSimplificatorIndInfo);
		}

		protected override void CheckCEN_TNNDocumentType()
		{
			base.CheckCEN_TNNDocumentType();
			if (Parent.Header.IsPhaseStatusTNN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEN_TNNDocumentTypeInfo);
			}
		}

		protected override void CheckCEN_SummaryType()
		{
			base.CheckCEN_SummaryType();
			if (Parent.Header.IsPhase5Arrival)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEN_SummaryTypeInfo, messagePrefix: "[TR0070] ");
			}
		}

		protected override void CheckCEN_AutomaticCompletion()
		{
			base.CheckCEN_AutomaticCompletion();
			if (Parent.Header.IsPhase5Arrival && !Parent.CEN_AutomaticCompletion && Parent.Header.ArrivalMovementHeader.AuthorizationNumber.IsEmpty)
			{
				Parent.CEN_AutomaticCompletionInfo.AddWarning(Res.GetString("0DBFDCDA-FE5A-41FD-AE2C-7F2640145D27", "[NR0049] If Authorization Nº is empty (i.e. public location), Automatic Completion should be ticked."));
			}
		}
	}
}
