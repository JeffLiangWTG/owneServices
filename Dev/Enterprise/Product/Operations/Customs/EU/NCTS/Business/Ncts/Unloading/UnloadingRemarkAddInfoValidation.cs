//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUnloadingRemarkAddInfoValidation
//
//    This class should be used for overriding validation in AutoUnloadingRemarkAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class UnloadingRemarkAddInfoValidation : AutoUnloadingRemarkAddInfoValidation
	{
		public UnloadingRemarkAddInfoValidation(AutoUnloadingRemarkAddInfo parent) : base(parent)
		{
		}

		protected override void CheckG9_Conform()
		{
			base.CheckG9_Conform();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.G9_ConformInfo);
		}

		protected override void CheckG9_StateOfSealsOk()
		{
			base.CheckG9_StateOfSealsOk();
			ListValidation.MessageErrorIfInvalidCode(Parent.G9_StateOfSealsOkInfo);
		}

		protected override void CheckG9_UnloadingCompletion()
		{
			base.CheckG9_UnloadingCompletion();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.G9_UnloadingCompletionInfo);
		}

		protected override void CheckG9_UnloadingDateIsNotEmpty()
		{
			CheckMandatoryUnloadingDate(Parent.G9_UnloadingDateInfo);
		}

		protected void CheckMandatoryUnloadingDate(ZPropertyInfo info)
		{
			if (!Parent.G9_UnloadingDate.IsValid || Parent.G9_UnloadingDate.IsEmpty)
			{
				if (errorUnloadingDateIsMissing != ZString.Empty && !info.HasError(errorUnloadingDateIsMissing))
				{
					info.AddMessageError(errorUnloadingDateIsMissing);
				}
			}
		}
		protected static string errorUnloadingDateIsMissing => Res.GetString("2DD9C975-7A94-4DBF-9692-70DFDB17A905", "You need to supply a valid unloading date.");

		protected override void CheckG9_UnloadingDateIsValidZDateTime()
		{
		}

		protected override void CheckG9_NoOfSeals()
		{
			base.CheckG9_NoOfSeals();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.G9_NoOfSealsInfo);
			var noOfSeals = Parent.G9_NoOfSeals;
			if (noOfSeals >= 0)
			{
				var nctsHeader = Parent.NctsHeader;
				if (nctsHeader != null && nctsHeader.Seals.Count > noOfSeals)
				{
					Parent.G9_NoOfSealsInfo.AddMessageError(Res.GetString("905E6D4D-BB55-4839-9C06-DCAB7FABA6EF", "The maximum number of seal numbers entered in the grid should be {0}.", noOfSeals));
				}
			}
		}

		protected new UnloadingRemarkAddInfo Parent => (UnloadingRemarkAddInfo)base.Parent;
	}
}
