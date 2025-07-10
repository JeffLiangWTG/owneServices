using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JobFilterProviderForPeriodicInvoice))]
	public class JobFilterProviderForPeriodicInvoiceTest : JobFilterProviderTest
	{
		public void TestContainsAccountingDateFilterForCA()
		{
			Assert(JobFilterProvider.GetType().IsSubclassOf(typeof(JobFilterProviderForPeriodicInvoice)));
			var propertyInfo = typeof(JobFilterProviderForPeriodicInvoice).GetProperty("ModuleToFilterMapping", BindingFlags.Instance | BindingFlags.NonPublic);
			var mapping = (Dictionary<PeriodicInvoiceModule, string[]>)propertyInfo.GetValue(JobFilterProvider);

			Assert("Not exist in all filters when login with non-CA Company.", !mapping[PeriodicInvoiceModule.CFS].Contains(PeriodicInvoiceBaseJobFilterBusinessObject.ACCOUNTING_DATE));
			Assert("Not exist in Customs filters when login with non-CA Company.", !mapping[PeriodicInvoiceModule.Customs].Contains(PeriodicInvoiceBaseJobFilterBusinessObject.ACCOUNTING_DATE));

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Canada))
			{
				mapping = (Dictionary<PeriodicInvoiceModule, string[]>)propertyInfo.GetValue(GetFilterProvider());
				AssertEquals("Be the first filter in all filters when login with Canada Company.", PeriodicInvoiceBaseJobFilterBusinessObject.ACCOUNTING_DATE, mapping[PeriodicInvoiceModule.CFS][0]);
				AssertEquals("Be the first filter in Customs filters when login with Canada Company.", PeriodicInvoiceBaseJobFilterBusinessObject.ACCOUNTING_DATE, mapping[PeriodicInvoiceModule.Customs][0]);
			}
		}

		public void TestContainsETAAndETDForCustoms()
		{
			var propertyInfo = typeof(JobFilterProviderForPeriodicInvoice).GetProperty("ModuleToFilterMapping", BindingFlags.Instance | BindingFlags.NonPublic);
			var mapping = (Dictionary<PeriodicInvoiceModule, string[]>)propertyInfo.GetValue(JobFilterProvider);

			Assert(mapping[PeriodicInvoiceModule.Customs].Contains(PeriodicInvoiceBaseJobFilterBusinessObject.ETA));
			Assert(mapping[PeriodicInvoiceModule.Customs].Contains(PeriodicInvoiceBaseJobFilterBusinessObject.ETD));
		}

		public void TestAdditionalReferenceFilter()
		{
			var exceptedModules = new PeriodicInvoiceModule[]
			{
				PeriodicInvoiceModule.CFS,
				PeriodicInvoiceModule.Customs,
				PeriodicInvoiceModule.Forwarding,
			};

			var exceptedJobTypes = new ZString[]
			{
				JobInvoicingConsumerTypes.CFSShipment.Code,
				JobInvoicingConsumerTypes.Brokerage.Code,
				JobInvoicingConsumerTypes.Shipment.Code,
				JobInvoicingConsumerTypes.QuotedBooking.Code,
				JobInvoicingConsumerTypes.OneOffQuotation.Code
			};

			var propertyInfo = typeof(JobFilterProviderForPeriodicInvoice).GetProperty("ModuleToFilterMapping", BindingFlags.Instance | BindingFlags.NonPublic);
			var mapping = (Dictionary<PeriodicInvoiceModule, string[]>)propertyInfo.GetValue(JobFilterProvider);
			foreach (PeriodicInvoiceModule periodicInvoiceModule in Enum.GetValues(typeof(PeriodicInvoiceModule)))
			{
				if (exceptedModules.Contains(periodicInvoiceModule))
				{
					AssertEquals(true, mapping[periodicInvoiceModule].Contains(PeriodicInvoiceBaseJobFilterBusinessObject.Additional_Reference));
				}
				else if (periodicInvoiceModule == PeriodicInvoiceModule.Other)
				{
					AssertEquals(false, mapping.ContainsKey(periodicInvoiceModule));
				}
				else
				{
					AssertEquals(false, mapping[periodicInvoiceModule].Contains(PeriodicInvoiceBaseJobFilterBusinessObject.Additional_Reference));
				}
			}

			var jobTypes = JobFilterProvider.GetJobTypesApplicableToAFilter(PeriodicInvoiceBaseJobFilterBusinessObject.Additional_Reference);
			AssertContainsExactElementsInAnyOrder(exceptedJobTypes, jobTypes);
		}

		public void TestCarrierFilterAvailabilityForJobTypes()
		{
			var filterProvider = new JobFilterProviderForPeriodicInvoice(PeriodicInvoiceJobsFilter);
			var jobTypes = filterProvider.GetJobTypesApplicableToAFilter(PeriodicInvoiceBaseJobFilterBusinessObject.CARRIER);
			AssertContainsExactElementsInAnyOrder(new ZString[] { JobInvoicingConsumerTypes.AgencyBillOfLading.Code, JobInvoicingConsumerTypes.AgencyBooking.Code }, jobTypes);
		}

		public void TestPrincipalFilterAvailabilityForJobTypes()
		{
			var filterProvider = new JobFilterProviderForPeriodicInvoice(PeriodicInvoiceJobsFilter);
			var jobTypes = filterProvider.GetJobTypesApplicableToAFilter(PeriodicInvoiceBaseJobFilterBusinessObject.PRINCIPAL);
			AssertContainsExactElementsInAnyOrder(new ZString[] { JobInvoicingConsumerTypes.AgencyBillOfLading.Code, JobInvoicingConsumerTypes.AgencyBooking.Code }, jobTypes);
		}

		protected override ZQuery GetQuery(ZQuery seedQuery)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Job));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GenericJob.GenericJob), ViewGenericJobSchema.PK);
			subQuery.AddToFilter(seedQuery);
			query.AddSubQuery(JobHeaderSchema.JH_ParentID, subQuery, JoinCondition.And);
			return query;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DummyJobFilterProviderForPeriodicInvoice(PeriodicInvoiceJobsFilter, false);
		}

		protected override JobFilterProvider GetFilterProvider()
		{
			return new DummyJobFilterProviderForPeriodicInvoice(PeriodicInvoiceJobsFilter, false);
		}

		PeriodicInvoiceJobsFilterBusinessObject PeriodicInvoiceJobsFilter
		{
			get { return fPeriodicInvoiceJobsFilter ?? (fPeriodicInvoiceJobsFilter = new PeriodicInvoiceJobsFilterBusinessObject()); }
		}
		PeriodicInvoiceJobsFilterBusinessObject fPeriodicInvoiceJobsFilter;
	}
}
