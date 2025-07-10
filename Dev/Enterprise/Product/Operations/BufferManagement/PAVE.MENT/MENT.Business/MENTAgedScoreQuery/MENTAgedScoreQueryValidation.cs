using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreQueryValidation : AutoMENTAgedScoreQueryValidation
	{
		public MENTAgedScoreQueryValidation(AutoMENTAgedScoreQuery parent)
			: base(parent)
		{
		}

		protected new MENTAgedScoreQuery Parent
		{
			get { return (MENTAgedScoreQuery)base.Parent; }
		}

		protected override void CheckMAQ_QueryDescription()
		{
			base.CheckMAQ_QueryDescription();
			if (Parent.IsSavedByFactory)
			{
				MandatoryValidation.CheckEntered(Parent.MAQ_QueryDescriptionInfo);
			}
		}

		protected override void CheckMAQ_SqlText()
		{
			base.CheckMAQ_SqlText();

			if (Parent.MAQ_IsActive && !Parent.Linked)
			{
				MandatoryValidation.CheckEntered(Parent.MAQ_SqlTextInfo);
				SqlValidation.CheckValidStatement(Parent.MAQ_SqlTextInfo, Parent.MAQ_SqlText, Parent.MAQ_SqlText, MENTConstants.AgedScoreValueColumn, MENTConstants.ReleaseGroupColumn, MENTConstants.ComponentColumn, MENTConstants.AttributeValueColumn, MENTConstants.StaffColumn);
			}

			if (Parent.Linked && !Parent.MAQ_SqlText.IsEmpty)
			{
				Parent.MAQ_SqlTextInfo.AddWarning(Res.GetString("0392ee82-123c-43ec-979c-dd5b9312df43", "This query is linked and will use the linked query's text."));
			}

			if (Parent.MAQ_SqlTextInfo.HasChanges && !Env.Security.MENTAgedScoreQueryChangeQueryText.IsAllowed)
			{
				Parent.MAQ_SqlTextInfo.AddError(Env.Security.MENTAgedScoreQueryChangeQueryText.ErrorMessageForNotAllowed);
			}
		}

		protected override void CheckMAQ_Code()
		{
			base.CheckMAQ_Code();

			if (Parent.IsSavedByFactory)
			{
				MandatoryValidation.CheckEntered(Parent.MAQ_CodeInfo);

				if (!Parent.MAQ_Code.IsEmpty && Parent.MAQ_BAB_RelatedAcceptabilityBand.IsEmpty)
				{
					var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_Code, Parent.MAQ_Code);
					query.AddToFilter(MENTAgedScoreQuerySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

					var results = Parent.Factory.Load<MENTAgedScoreQuery>(query);

					if (results.Length > 0)
					{
						Parent.MAQ_CodeInfo.AddError(Res.GetString("3a0147d6-f854-4de6-a3bf-500121ac99d7", "The Code you have entered is not unique."));
					}

					Regex nonAlphaNumericRegex = new Regex(@"^[a-z0-9]+\s*$", RegexOptions.IgnoreCase);
					if (!nonAlphaNumericRegex.IsMatch(Parent.MAQ_Code))
					{
						Parent.MAQ_CodeInfo.AddError(Res.GetString("315f4038-233b-4c19-8488-34026a50a4b6", "Codes can only contain Alpha Numeric Characters"));
					}
				}
			}
		}

		protected override void CheckMAQ_IsFaulty()
		{
			base.CheckMAQ_IsFaulty();

			if (Parent.MAQ_IsFaulty)
			{
				Parent.MAQ_IsFaultyInfo.AddWarning(Res.GetString("a1701eac-72d1-4b16-9def-bfb794d87403", "Check the notes tab to view the reason why this has been marked as faulty."));
			}
		}

		protected override void CheckMAQ_PurgeDays()
		{
			base.CheckMAQ_PurgeDays();
			CompareValidation.CheckNumberNotNegative(Parent.MAQ_PurgeDaysInfo);
		}

		protected override void CheckMAQ_PurgeAllButLatestQuantity()
		{
			base.CheckMAQ_PurgeAllButLatestQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.MAQ_PurgeAllButLatestQuantityInfo);
		}
	}
}
