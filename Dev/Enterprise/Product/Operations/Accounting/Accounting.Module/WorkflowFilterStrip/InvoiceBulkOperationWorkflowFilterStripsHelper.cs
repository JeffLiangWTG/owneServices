using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class InvoiceBulkOperationWorkflowFilterStripsHelper : WorkflowFilterStripsHelper, IInvoiceBulkOperationWorkflowFilterStripsHelper
	{
		public InvoiceBulkOperationWorkflowFilterStripsHelper(ZString templateCode, Type businessObjectType, BusinessObjectFactory factory, bool shouldAddRelatedMilestoneFilters)
			: base(businessObjectType, templateCode, factory)
		{
			ShouldAddRelatedMilestoneFilters = shouldAddRelatedMilestoneFilters;
		}

		public override bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return typeof(AccTransactionLines).IsAssignableFrom(BusinessObjectType) || typeof(JobHeader).IsAssignableFrom(BusinessObjectType) || typeof(GenericConsol).IsAssignableFrom(BusinessObjectType);
		}

		#region Filters

		protected override WorkflowModuleFilter GetNewFilter(string description, WorkflowModuleFilterTypes filterType)
		{
			return new InvoiceBulkOperationWorkflowModuleDateFilter(description, BusinessObjectType, filterType, TemplateCode);
		}

		protected override WorkflowModuleTextFilter GetNewTextFilter(string description, GetTextQuery queryDelegate, IList list)
		{
			return new InvoiceBulkOperationWorkflowModuleTextFilter(description, queryDelegate, list, BusinessObjectType, TemplateCode);
		}

		public static ZDBOnlySubQuery AddTransportBookingParentsInMilestoneFilter(ZDBOnlySubQuery milestoneQuery)
		{
			var processTaskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID, DtbBookingSchema.PK);
			foreach (var andPart in milestoneQuery.GetCompositeParts())
			{
				processTaskQuery.AddToFilter(andPart, JoinCondition.And);
			}
			processTaskQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ParentTableCode, DtbBookingSchema.Constants.Prefix);

			var bookingQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.KM_KB_Booking, DtbBookingConsolidationSchema.PK);
			bookingQuery.AddSubQuery(processTaskQuery, JoinCondition.And);

			var bookingConsolidationQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingConsolidationSchema.KB_ParentID);
			bookingConsolidationQuery.AddSubQuery(bookingQuery, JoinCondition.And);

			milestoneQuery.AddAsUnionQuery(bookingConsolidationQuery, true);

			return milestoneQuery;
		}

		#endregion
	}
}
