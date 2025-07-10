using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class CreditCODDataObjectValidation : AutoCreditCODDataObjectValidation
	{
		public CreditCODDataObjectValidation(AutoCreditCODDataObject parent) : base(parent)
		{
		}

		public new CreditCODDataObject Parent => (CreditCODDataObject)base.Parent;

		protected override void CheckCreditMethod()
		{
			base.CheckCreditMethod();

			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.CreditMethodInfo);
			ListValidation.ErrorIfInvalidCode(parent.CreditMethodInfo);
		}

		protected override void CheckReleasingEntryReference()
		{
			base.CheckReleasingEntryReference();
			var parent = Parent;

			MandatoryValidation.CheckEntered(parent.ReleasingEntryReferenceInfo);
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("A3165587-9F31-4DA6-8BD5-CC89C4CB88AC", "Enter a Customs cleared (BAE) Releasing Entry Reference."), parent.ReleasingEntryReferenceInfo);

			var header = parent.Header;
			if (header.FrCreditCODItemApplicators.Cast<CreditCODDataObject>().Count(x => x.CreditMethod == parent.CreditMethod
																							&& x.ReleasingEntryReference == parent.ReleasingEntryReference
																							&& x.PreviousEntryReference == parent.PreviousEntryReference
																							&& x.PreviousEntryLineNo == parent.PreviousEntryLineNo) > 1)
			{
				parent.ReleasingEntryReferenceInfo.AddError(Res.GetString("729cac21-b76c-48a7-b9fe-eb3a5aab7e59", "There are duplicate combination of Credit Method, Releasing Entry No., Previous Entry No. and Previous Entry Line No."));
			}

			if (parent.ReleasingEntryHeader?.AllEntryLines.Cast<CusEntryLine>().All(x => !x.CusProcedure?.IsGuaranteeReleased() ?? false) ?? false)
			{
				parent.ReleasingEntryReferenceInfo.AddMessageError(Res.GetString("305f1a93-0070-4d0b-8ba0-191e80513b55", "No procedure of this entry releases guarantee."));
			}
		}

		protected override void CheckPreviousEntryReference()
		{
			base.CheckPreviousEntryReference();

			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.PreviousEntryReferenceInfo);
			ListValidation.ErrorIfInvalidCode(parent.PreviousEntryReferenceInfo);

			if (parent.CreditMethod == CreditMethodList.Codes.CreditPreviousEntry)
			{
				if (parent.PreviousEntryHeader?.AllEntryLines.Cast<CusEntryLine>().All(x => !x.CusProcedure?.IsGuaranteeConsumed() ?? false) ?? false)
				{
					parent.PreviousEntryReferenceInfo.AddMessageError(Res.GetString("E00724BA-958C-4854-9A5D-D6209513EEFD", "No procedure of this entry consumes guarantee."));
				}
			}
		}

		protected override void CheckPreviousEntryLineNo()
		{
			base.CheckPreviousEntryLineNo();
			var parent = Parent;

			if (!parent.PreviousEntryLineNoReadOnly)
			{
				if (parent.PreviousEntryHeader != null)
				{
					if (parent.PreviousEntryLine == null)
					{
						parent.PreviousEntryLineNoInfo.AddError(Res.GetString("c748b259-ad1d-4ebe-af9b-0be7cfee0f5d", "Previous Entry {0} doesn't have line {1}.", parent.PreviousEntryReference, parent.PreviousEntryLineNo));
					}
					else if (!parent.PreviousEntryLine.CusProcedure?.IsGuaranteeConsumed() ?? false)
					{
						parent.PreviousEntryLineNoInfo.AddMessageError(Res.GetString("b131a23a-8198-46ea-b015-68130a698a5c", "Procedure ({0}) of this entry line doesn't consume guarantee.", parent.PreviousEntryLine.ProcedureCode));
					}
				}
			}
		}

		protected override void CheckAmount()
		{
			base.CheckAmount();
			var parent = Parent;
			if (parent.CreditMethod == CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine)
			{
				MandatoryValidation.CheckNotZero(parent.AmountInfo);
				if (parent.PreviousEntryLine != null)
				{
					var consumedAmount = parent.PreviousEntryLine.AmountAndTypeToBeGuaranteeds.FirstOrDefault(x => x.DebitType == EU.Business.GuaranteeDebitType.NORMAL);
					if (consumedAmount != null && parent.Amount > consumedAmount.AmountInDeclarationCurrency)
					{
						parent.AmountInfo.AddMessageError(Res.GetString("3ea175b5-a61e-496c-8283-0eda89484d5d", "Previous entry {0}, line No. {1} has consumed €{2}, but you are going to release €{3}.", parent.PreviousEntryReference, parent.PreviousEntryLineNo, consumedAmount.AmountInDeclarationCurrency, parent.Amount));
					}
				}
			}
		}
	}
}
