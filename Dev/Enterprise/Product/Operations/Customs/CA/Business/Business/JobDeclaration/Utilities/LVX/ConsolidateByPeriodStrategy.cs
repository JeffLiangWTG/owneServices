using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateByPeriodStrategy : ConsolidationStrategy
	{
		public ConsolidateByPeriodStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

		#region IConsolidationStrategy Members

		public override void AddMatchingFilter(ZQuery query)
		{
			var entryAuthorisationDate = Wrapper.EntryAuthorisationDate;
			query.AddToFilter(JobDeclarationSchema.JE_EntryAuthorisationDate, SQLComparisonOperator.GreaterThanOrEqualTo, entryAuthorisationDate);
			query.AddToFilter(JobDeclarationSchema.JE_EntryAuthorisationDate, SQLComparisonOperator.LessThanOrEqualTo, entryAuthorisationDate.AddMonths(1).AddDays(-1));
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			return IsMonthMatching(declaration.JE_EntryAuthorisationDate, Wrapper.EntryAuthorisationDate);
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			var period = Wrapper.EntryAuthorisationDate;
			if (!IsMonthMatching(period, declaration.JE_EntryAuthorisationDate))
			{
				declaration.JE_EntryAuthorisationDate = period;
			}
		}

		#endregion

		#region GetEffectiveSetting

		protected override bool GetHasAcknowledged()
		{
			return true;
		}

		#endregion

		bool IsMonthMatching(ZDateTime date1, ZDateTime date2)
		{
			return date1.Year == date2.Year && date1.Month == date2.Month;
		}
	}
}
