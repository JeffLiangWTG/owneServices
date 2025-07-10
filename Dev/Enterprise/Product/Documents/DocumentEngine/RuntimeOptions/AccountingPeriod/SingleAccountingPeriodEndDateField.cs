using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	internal class SingleAccountingPeriodEndDateField : SingleAccountingPeriodField
	{
		public SingleAccountingPeriodEndDateField(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal SingleAccountingPeriodEndDateField(SingleAccountingPeriodEndDateFieldJsonData data)
			: base(data)
		{
		}

		#endregion

		protected override SingleAccountingPeriodFieldJsonData CreateJsonDataCore()
		{
			return new SingleAccountingPeriodEndDateFieldJsonData();
		}

		public override object ValueAsObject
		{
			get { return new AccountingPeriodCalculator(Factory).GetLastDayForPeriod(SinglePeriod); }
		}

		protected override string NonEmptyWhereClause()
		{
			throw new NotSupportedException("This can only be used to get a value, not to provide a where clause filter. Use DateBasedAccountingPeriodField instead.");
		}
	}
}
