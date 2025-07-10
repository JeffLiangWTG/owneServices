using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	internal class SingleAccountingPeriodStartDateField : SingleAccountingPeriodField
	{
		public SingleAccountingPeriodStartDateField(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal SingleAccountingPeriodStartDateField(SingleAccountingPeriodStartDateFieldJsonData data)
			: base(data)
		{
		}

		#endregion

		protected override SingleAccountingPeriodFieldJsonData CreateJsonDataCore()
		{
			return new SingleAccountingPeriodStartDateFieldJsonData();
		}

		public override object ValueAsObject
		{
			get { return new AccountingPeriodCalculator(Factory).GetFirstDayForPeriod(SinglePeriod); }
		}

		protected override string NonEmptyWhereClause()
		{
			throw new NotSupportedException("This can only be used to get a value, not to provide a where clause filter. Use DateBasedAccountingPeriodField instead.");
		}
	}
}
