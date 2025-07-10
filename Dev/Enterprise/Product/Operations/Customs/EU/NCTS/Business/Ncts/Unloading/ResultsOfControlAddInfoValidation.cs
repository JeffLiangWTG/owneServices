//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoResultsOfControlAddInfoValidation
//
//    This class should be used for overriding validation in AutoResultsOfControlAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ResultsOfControlAddInfoValidation : AutoResultsOfControlAddInfoValidation
	{
		public ResultsOfControlAddInfoValidation(AutoResultsOfControlAddInfo parent) : base(parent)
		{
		}

		public new ResultsOfControlAddInfo Parent
		{
			get { return (ResultsOfControlAddInfo)base.Parent; }
		}

		protected override void CheckG9_CorrectedValue()
		{
			base.CheckG9_CorrectedValue();
			if (Parent.IsNationalityAtDept)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.G9_CorrectedValueInfo, Parent.ArrivalParent.Lookups.Countries);
			}
		}
	}
}
