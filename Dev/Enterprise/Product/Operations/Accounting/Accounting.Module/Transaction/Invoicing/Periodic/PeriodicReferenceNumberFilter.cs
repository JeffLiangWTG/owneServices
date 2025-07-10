using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class PeriodicReferenceNumberFilter : IPeriodicInvoicingReferenceNumberFilter
	{
		protected sealed class PeriodicInvoicingReferenceNumberFilterHelper : ReferenceNumberFilterHelper<ForwardingShipment>
		{
			public PeriodicInvoicingReferenceNumberFilterHelper(JobFilterProviderForPeriodicInvoice provider) : base()
			{
				Provider = provider;
			}

			readonly JobFilterProviderForPeriodicInvoice Provider;

			protected override ZQuery GetReferenceNumberFilterCore(SQLComparisonOperator opp, ZString country, ZString type, ZString number, bool notIn, bool filterOnEmptyNumber)
			{
				var jobHeaderQuery = new ZDBOnlyQuery(typeof(GenericJob));

				if (Provider.IsForwardingModule)
				{
					var entryNumFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
					entryNumFilter.AddToFilter(GetCusEntryNumFilter(opp, country, type, number, filterOnEmptyNumber));

					var shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), ViewGenericJobSchema.PK);
					shipmentQuery.AddSubQuery(entryNumFilter, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}

				if (Provider.IsCustomsModule)
				{
					var entryNumFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
					entryNumFilter.AddToFilter(GetCusEntryNumFilter(opp, country, type, number, filterOnEmptyNumber));

					var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), ViewGenericJobSchema.PK);
					jobDeclarationQuery.AddSubQuery(entryNumFilter, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.Or);
				}

				return jobHeaderQuery;
			}
		}

		IModuleFilter IPeriodicInvoicingReferenceNumberFilter.GetReferenceNumberFilter(FilterBusinessObject provider, ZString filterDescription)
		{
			var referenceNumberFilter = new ReferenceNumberFilter(filterDescription,
				new PeriodicInvoicingReferenceNumberFilterHelper((JobFilterProviderForPeriodicInvoice)provider).GetReferenceNumberFilter,
				new RefCountryCollection(new BusinessObjectFactory()))
				.WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			return referenceNumberFilter;
		}
	}
}
