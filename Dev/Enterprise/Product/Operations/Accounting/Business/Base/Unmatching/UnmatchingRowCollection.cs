using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	public class UnmatchingRowCollection : NonPersistentBusinessObjectCollection<UnmatchingRow>
	{
		public UnmatchingRowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UnmatchingRow(Factory);
		}

		#region Load

		public void SetFilterHelper(MatchGroupFilterHelper filterHelper)
		{
			this.FilterHelper = filterHelper;
		}

		MatchGroupFilterHelper FilterHelper;

		public override void Load(ZQuery unusedFilter)
		{
			Load();
		}

		public override void Load()
		{
			if (FilterHelper != null)
			{
				LoadUsingFilterHelper(FilterHelper);
			}
		}

		public override int GetEstimatedLoadCount(ZQuery completeFilter)
		{
			ZInt countToReturn = 0;
			if (FilterHelper != null)
			{
				string sQL = string.Format("SELECT COUNT(*) AS Count FROM ({0}) m", FilterHelper.PlainFilterWithoutParamValues);
				DynamicBusinessObjectCollection countCollection = new DynamicBusinessObjectCollection(Factory);
				countCollection.Load(sQL, FilterHelper.FilterParameters);
				countToReturn = (ZInt)countCollection[0]["Count"];
			}

			return countToReturn;
		}

		public static DynamicBusinessObjectCollection LoadFilterHelper(BusinessObjectFactory factory, MatchGroupFilterHelper filterHelper)
		{
			var dynBizOs = new DynamicBusinessObjectCollection(factory);
			dynBizOs.Load(filterHelper.PlainFilterWithoutParamValues, filterHelper.FilterParameters);
			return dynBizOs;
		}

		public override void Add(BusinessObject businessObject)
		{
			if (businessObject is DynamicBusinessObject dynBizO)
			{
				UnmatchingRow newRow = this.AddNew();
				if (dynBizO[UnmatchingRow.Schema.MatchDate] is ZDateTime)
				{
					newRow.MatchDate = (ZDateTime)dynBizO[UnmatchingRow.Schema.MatchDate];
				}
				if (dynBizO[UnmatchingRow.Schema.MatchGroupNum] is ZString)
				{
					newRow.MatchGroupNum = (ZString)dynBizO[UnmatchingRow.Schema.MatchGroupNum];
				}
			}
			else
			{
				base.Add(businessObject);
			}
		}

		void LoadUsingFilterHelper(MatchGroupFilterHelper filterHelper)
		{
			using (SuspendListChanged())
			{
				RemoveAll();
				var dynBizos = LoadFilterHelper(Factory, filterHelper);
				foreach (BusinessObject bizo in dynBizos)
				{
					Add(bizo);
				}
			}

			if (AfterLoaded != null)
			{
				AfterLoaded(this, new EventArgs());
			}
		}

		public event EventHandler AfterLoaded;

		#endregion

		public ZBool ContainsMatchGroupNumber(ZString matchGroupNum)
		{
			if (this.Count == 0)
			{
				return false;
			}
			foreach (UnmatchingRow row in this)
			{
				if (row.MatchGroupNum == matchGroupNum)
				{
					return true;
				}
			}
			return false;
		}

#if DEBUG

		public MatchGroupFilterHelper FilterHelperExposedForTest
		{
			get { return FilterHelper; }
		}

#endif
	}
}
