using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RecommendationLetter : AutoAURecommendationLetter
	{
		public RecommendationLetter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public QuarantineExDocHeader Header
		{
			get { return Parent as QuarantineExDocHeader; }
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business|ZA_LetterNumber", Caption = "Letter Number")]
		public override ZString ZA_LetterNumber
		{
			get { return base.ZA_LetterNumber; }
			set { base.ZA_LetterNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business|ZA_LetterDate", Caption = "Letter Date")]
		public override ZDateTime ZA_LetterDate
		{
			get { return base.ZA_LetterDate; }
			set { base.ZA_LetterDate = value; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Recommendation Letter"; }
		}

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			Data.MarkAsNeedingValidation();
		}

		#endregion
	}
}
