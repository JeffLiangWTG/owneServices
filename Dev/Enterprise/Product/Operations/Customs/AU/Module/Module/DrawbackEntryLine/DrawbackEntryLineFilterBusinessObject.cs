using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class DrawbackEntryLineFilterBusinessObject : Customs.Module.EntryLineFilterBusinessObject
	{
		public DrawbackEntryLineFilterBusinessObject(IBusinessObjectCollection gridCollection)
			: base(gridCollection)
		{
			this.gridCollection = gridCollection;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new DrawbackEntryLineFilterBusinessObject(gridCollection);

		readonly IBusinessObjectCollection gridCollection;

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				filter.AddToFilter(GetNatureTypeQuery());
				return filter;
			}
		}

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);
			var drawbackDateFilter = filters.AddDateFilter(DeclarationFilterConstants.DrawbackDate, GetDrawbackDateQuery);
			drawbackDateFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			drawbackDateFilter.IsActive = true;
			drawbackDateFilter.ReadOnly = true;
		}

		ZQuery GetNatureTypeQuery()
		{
			var result = new ZQuery();
			var cusEntryLineQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			var invoiceLineQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
			invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.NotContains, "IsPackToBondForLine_Hidden=Y");
			cusEntryLineQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
			result.AddToFilter(cusEntryLineQuery);
			return result;
		}

		ZQuery GetDrawbackDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			ZDBOnlySubQuery jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			AddDateRange(jobDeclarationQuery, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_EntrySubmittedDate, value1.Date, value2.Date);
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return dBOnlyQuery;
		}
	}
}
