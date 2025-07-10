using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class JobFilterProvider : FilterStripBusinessObject
	{
		public bool IsCFSModule { get; set; }

		public bool IsConsolModule { get; set; }

		public bool IsCustomsModule { get; set; }

		public bool IsForwardingModule { get; set; }

		public bool IsConsignmentModule { get; set; }

		public bool IsTransportModule { get; set; }

		public bool IsAgencyModule { get; set; }

		public bool IsWarehouseModule { get; set; }

		public ZQuery GetETAQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery;

			if (IsConsignmentModule)
			{
				jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				jobHeaderQuery = GetEstimatedOrArrivalDatesQuery(comparisonOperator, SailingFilterBuilder.Dates.ETA, date1, date2);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);
			if (IsForwardingModule)
			{
				var shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				var orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);

				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					orderSubQuery.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_JS, SQLComparisonOperator.NotEqual, null);
					var whsPivotSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketJobPivot), WhsDocketJobPivotSchema.WV_ParentId, true);
					var docsAndCartageOrderSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
					var orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP, true);
					docsAndCartageOrderSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(orderSubQuery, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(whsPivotSubQuery, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(docsAndCartageOrderSubQuery, JoinCondition.And);
				}
				else
				{
					orderSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
					shipmentQuery.AddSubQuery(orderSubQuery, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);

					var whsOrderResult = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					var whsPivotSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketJobPivot), WhsDocketJobPivotSchema.WV_ParentId);
					var whsOrderSubQuery = new ZDBOnlySubQuery(typeof(IWhsOrder), WhsDocketJobPivotSchema.WV_WD_Docket);

					whsOrderSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, WhsDocketSchema.WD_ExternalReference, comparisonOperator, orderNo);
					whsPivotSubQuery.AddToFilter(JoinCondition.And, WhsDocketJobPivotSchema.WV_DocketType, "ORD");
					whsPivotSubQuery.AddSubQuery(whsOrderSubQuery, JoinCondition.And);
					whsOrderResult.AddSubQuery(whsPivotSubQuery, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(whsOrderResult, JoinCondition.Or);

					var docsAndCartageOrderResult = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					var cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
					var orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP);

					orderItemSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderItemSchema.JT_OrderReference, comparisonOperator, orderNo);
					cartageSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);
					docsAndCartageOrderResult.AddSubQuery(cartageSubQuery, JoinCondition.And);
					jobHeaderQuery.AddSubQuery(docsAndCartageOrderResult, JoinCondition.Or);
				}
			}

			return jobHeaderQuery;
		}

		public ZQuery GetETDQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery;

			if (IsConsignmentModule)
			{
				jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				jobHeaderQuery = GetEstimatedOrArrivalDatesQuery(comparisonOperator, SailingFilterBuilder.Dates.ETD, date1, date2);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetATAQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery;

			if (IsConsignmentModule)
			{
				jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				jobHeaderQuery = GetEstimatedOrArrivalDatesQuery(comparisonOperator, SailingFilterBuilder.Dates.ATA, date1, date2);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetATDQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery;

			if (IsConsignmentModule)
			{
				jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				jobHeaderQuery = GetEstimatedOrArrivalDatesQuery(comparisonOperator, SailingFilterBuilder.Dates.ATD, date1, date2);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		protected virtual ZDBOnlyQuery GetEstimatedOrArrivalDatesQuery(DateComparisonOperator comparisonOperator, SailingFilterBuilder.Dates dateType, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);

			if (IsCFSModule)
			{
				ZDBOnlySubQuery shipmentQueryForConsol = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				builder.SetDateRange(dateType, comparisonOperator, date1, date2);
				shipmentQueryForConsol.AddToFilter(builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol));

				jobHeaderQuery.AddSubQuery(shipmentQueryForConsol, JoinCondition.Or);
			}

			if (IsConsolModule)
			{
				ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobHeaderQueryLinkColumn);
				builder.SetDateRange(dateType, comparisonOperator, date1, date2);
				consolQuery.AddToFilter(builder.ToConsolFilter());

				jobHeaderQuery.AddSubQuery(consolQuery, JoinCondition.Or);
			}

			if (IsForwardingModule || IsAgencyModule)
			{
				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);

				builder.SetDateRange(dateType, comparisonOperator, date1, date2);
				shipmentQuery.AddToFilter(builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol));
				if (IsAgencyModule)
				{
					shipmentQuery.AddToFilter(builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct), JoinCondition.Or);
				}

				if (dateType == SailingFilterBuilder.Dates.ETD)
				{
					AddDateTimeRange(shipmentQuery, comparisonOperator, JoinCondition.Or, JobShipmentSchema.JS_E_DEP, date1, date2);
				}
				else if (dateType == SailingFilterBuilder.Dates.ETA)
				{
					AddDateTimeRange(shipmentQuery, comparisonOperator, JoinCondition.Or, JobShipmentSchema.JS_E_ARV, date1, date2);
				}

				jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
			}

			if (IsCustomsModule)
			{
				ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
				builder.SetDateRange(dateType, comparisonOperator, date1, date2);
				declarationQuery.AddToFilter(builder.ToDeclarationFilter());

				if (dateType == SailingFilterBuilder.Dates.ETD)
				{
					AddDateTimeRange(declarationQuery, comparisonOperator, JoinCondition.Or, JobDeclarationSchema.JE_DateAtOrigin, date1, date2);
				}
				else if (dateType == SailingFilterBuilder.Dates.ETA)
				{
					AddDateTimeRange(declarationQuery, comparisonOperator, JoinCondition.Or, JobDeclarationSchema.JE_DateAtFinalDestination, date1, date2);
				}

				jobHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.Or);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetDeliveryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDeliveryDateQueryCore(comparisonOperator, date1, date2);
		}

		protected virtual ZQuery GetDeliveryDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsConsignmentModule)
			{
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				if (IsCustomsModule)
				{
					ZDBOnlySubQuery jobDocsAndCartageQuery2 = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
					AddDateTimeRange(jobDocsAndCartageQuery2, comparisonOperator, JoinCondition.And, JobDocsAndCartageSchema.JP_EstimatedDelivery, date1, date2);
					jobDocsAndCartageQuery2.AddToFilter(JoinCondition.And, JobDocsAndCartageSchema.JP_ParentTableCode, JobDeclarationSchema.Constants.Prefix);

					ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
					declarationQuery.AddSubQuery(jobDocsAndCartageQuery2, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.Or);
				}

				if (IsForwardingModule)
				{
					ZDBOnlySubQuery jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
					AddDateTimeRange(jobDocsAndCartageQuery, comparisonOperator, JoinCondition.And, JobDocsAndCartageSchema.JP_EstimatedDelivery, date1, date2);
					jobDocsAndCartageQuery.AddToFilter(JoinCondition.And, JobDocsAndCartageSchema.JP_ParentTableCode, JobShipmentSchema.Constants.Prefix);

					ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					shipmentSubQuery.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(shipmentSubQuery, JoinCondition.Or);
				}
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetPickupDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetPickupDateQueryCore(comparisonOperator, date1, date2);
		}

		protected virtual ZQuery GetPickupDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsConsignmentModule)
			{
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				ZDBOnlySubQuery jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				AddDateTimeRange(jobDocsAndCartageQuery, comparisonOperator, JoinCondition.And, JobDocsAndCartageSchema.JP_EstimatedPickup, date1, date2);
				jobDocsAndCartageQuery.AddToFilter(JoinCondition.And, JobDocsAndCartageSchema.JP_ParentTableCode, JobShipmentSchema.Constants.Prefix);

				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				shipmentQuery.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);

				jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);

				ZDBOnlySubQuery jobDocsAndCartageQuery2 = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				AddDateTimeRange(jobDocsAndCartageQuery2, comparisonOperator, JoinCondition.And, JobDocsAndCartageSchema.JP_EstimatedPickup, date1, date2);
				jobDocsAndCartageQuery2.AddToFilter(JoinCondition.And, JobDocsAndCartageSchema.JP_ParentTableCode, JobDeclarationSchema.Constants.Prefix);

				ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
				declarationQuery.AddSubQuery(jobDocsAndCartageQuery2, JoinCondition.And);

				jobHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.Or);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetCustomsClearanceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetCustomsClearanceDateQueryCore(comparisonOperator, date1, date2);
		}

		protected virtual ZQuery GetCustomsClearanceDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsConsignmentModule)
			{
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
				AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, date1, date2);

				ZQuery subQuery2 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
				subQuery2.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code);
				subQuery.AddToFilter(subQuery2, JoinCondition.And);

				if (IsForwardingModule || IsCFSModule)
				{
					ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					shipmentQuery.AddSubQuery(subQuery, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}

				if (IsConsolModule)
				{
					ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobHeaderQueryLinkColumn);
					consolQuery.AddSubQuery(subQuery, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(consolQuery, JoinCondition.Or);
				}

				if (IsCustomsModule)
				{
					ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
					declarationQuery.AddSubQuery(subQuery, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.Or);
				}
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetAWBCutOffDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetAWBCutOffDateQueryCore(comparisonOperator, date1, date2);
		}

		protected virtual ZQuery GetAWBCutOffDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsConsignmentModule)
			{
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);

				ZDBOnlySubQuery consolShipmentSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);

				ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
				AddDateTimeRange(consolSubQuery, comparisonOperator, JoinCondition.And, JobConsolSchema.JK_MasterBillIssueDate, date1, date2);
				consolShipmentSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				shipmentSubQuery.AddSubQuery(consolShipmentSubQuery, JoinCondition.And);
				jobHeaderQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetCompletionDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetCompletionDateCore(comparisonOperator, date1, date2);
		}

		protected virtual ZQuery GetCompletionDateCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsConsignmentModule)
			{
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				if (IsForwardingModule || IsCFSModule)
				{
					ZDBOnlySubQuery cartageQueryForShipment = new ZDBOnlySubQuery(typeof(CommonCartage), JobCartageSchema.JJ_ParentID);
					AddDateTimeRange(cartageQueryForShipment, comparisonOperator, JoinCondition.And, JobCartageSchema.JJ_A_JCL, date1, date2);

					ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					shipmentQuery.AddSubQuery(cartageQueryForShipment, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}

				if (IsConsolModule)
				{
					ZDBOnlySubQuery cartageQueryForConsol = new ZDBOnlySubQuery(typeof(CommonCartage), JobCartageSchema.JJ_ParentID);
					AddDateTimeRange(cartageQueryForConsol, comparisonOperator, JoinCondition.And, JobCartageSchema.JJ_A_JCL, date1, date2);

					ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobHeaderQueryLinkColumn);
					consolQuery.AddSubQuery(cartageQueryForConsol, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(consolQuery, JoinCondition.Or);
				}

				if (IsTransportModule)
				{
					ZDBOnlySubQuery cartageQuery = new ZDBOnlySubQuery(typeof(CommonCartage), JobHeaderQueryLinkColumn);
					AddDateTimeRange(cartageQuery, comparisonOperator, JoinCondition.And, JobCartageSchema.JJ_A_JCL, date1, date2);

					jobHeaderQuery.AddSubQuery(cartageQuery, JoinCondition.Or);
				}

				if (IsCustomsModule)
				{
					ZDBOnlySubQuery cartageSubQueryForDeclaration = new ZDBOnlySubQuery(typeof(CommonCartage), JobCartageSchema.JJ_ParentID);
					AddDateTimeRange(cartageSubQueryForDeclaration, comparisonOperator, JoinCondition.And, JobCartageSchema.JJ_A_JCL, date1, date2);

					ZDBOnlySubQuery jobDeclrationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
					jobDeclrationQuery.AddSubQuery(cartageSubQueryForDeclaration, JoinCondition.And);

					jobHeaderQuery.AddSubQuery(jobDeclrationQuery, JoinCondition.Or);
				}
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetReceivingAgentQuery(ZGuid receivingForwarderPK)
		{
			return GetReceivingAgentQueryCore(receivingForwarderPK);
		}

		protected virtual ZQuery GetReceivingAgentQueryCore(ZGuid receivingForwarderPK)
		{
			var query = new ZQuery();

			if (receivingForwarderPK.IsValid)
			{
				query = GetSendingRecevingAgentFullQuery(receivingForwarderPK, JobConsolSchema.JK_OA_ReceivingForwarderAddress);
			}

			return query;
		}

		public ZQuery GetSendingAgentQuery(ZGuid sendingForwarderPK)
		{
			return GetSendingAgentQueryCore(sendingForwarderPK);
		}

		protected virtual ZQuery GetSendingAgentQueryCore(ZGuid sendingForwarderPK)
		{
			var query = new ZQuery();

			if (sendingForwarderPK.IsValid)
			{
				query = GetSendingRecevingAgentFullQuery(sendingForwarderPK, JobConsolSchema.JK_OA_SendingForwarderAddress);
			}

			return query;
		}

		ZQuery GetSendingRecevingAgentFullQuery(ZGuid orgPK, SchemaColumn consolAddressForeignKeyColumn)
		{
			var jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			var getMainFilterQuery = new Func<ZDBOnlyQuery>(() =>
			{
				var result = new ZDBOnlyQuery(typeof(CommonConsol));
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), consolAddressForeignKeyColumn);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				return result;
			});

			if (IsForwardingModule || IsCFSModule)
			{
				var shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				var consolShipmentSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

				consolSubQuery.AddToFilter(getMainFilterQuery(), JoinCondition.And);
				consolShipmentSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				shipmentSubQuery.AddSubQuery(consolShipmentSubQuery, JoinCondition.And);
				jobHeaderQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			}
			else if (IsConsolModule)
			{
				var consolSubQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobHeaderQueryLinkColumn);
				consolSubQuery.AddToFilter(getMainFilterQuery(), JoinCondition.And);
				jobHeaderQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			}

			return jobHeaderQuery;
		}

		public ZQuery GetTransportMode(ZString value)
		{
			return GetTransportModeCore(value);
		}

		protected virtual ZQuery GetTransportModeCore(ZString value)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsConsignmentModule)
			{
				jobHeaderQuery.AddFilterAndZSQLParameterCollection("0 = 1", null);
			}
			else
			{
				if (IsCFSModule)
				{
					ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_TransportMode, value);

					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}

				if (IsConsolModule)
				{
					ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobHeaderQueryLinkColumn);
					consolQuery.AddToFilter(JobConsolSchema.JK_TransportMode, value);

					jobHeaderQuery.AddSubQuery(consolQuery, JoinCondition.Or);
				}

				if (IsForwardingModule)
				{
					ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_TransportMode, value);

					ZDBOnlySubQuery consolShipmentSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobShipmentSchema.PK);
					ZDBOnlySubQuery loadListConsolQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobConShipLinkSchema.JN_JK);
					loadListConsolQuery.AddToFilter(JoinCondition.And, JobConsolSchema.JK_IsCFS, 'Y');
					loadListConsolQuery.AddToFilter(JobConsolSchema.JK_TransportMode, value);

					consolShipmentSubQuery.AddSubQuery(loadListConsolQuery, JoinCondition.And);
					shipmentQuery.AddSubQuery(consolShipmentSubQuery, JoinCondition.Or);

					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}
				if (IsCustomsModule)
				{
					var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
					jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_TransportMode, value);
					jobHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.Or);
				}

				if (IsTransportModule)
				{
					var cartageQuery = new ZDBOnlySubQuery(typeof(CommonCartage), JobHeaderQueryLinkColumn);
					cartageQuery.AddToFilter(JobCartageSchema.JJ_ShippingTransportMode, value);

					jobHeaderQuery.AddSubQuery(cartageQuery, JoinCondition.Or);
				}
			}
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}

		public ZQuery GetServiceDirection(ZString value)
		{
			return GetServiceDirectionCore(value);
		}

		public ZQuery GetExcludingQuery()
		{
			var query = new ZQuery();
			if (IsForwardingModule)
			{
				query = new ZQuery(ViewGenericJobSchema.VJ_TableName, JobShipmentSchema.Constants.TableName);
			}
			else if (IsCustomsModule)
			{
				query = new ZQuery(ViewGenericJobSchema.VJ_TableName, JobDeclarationSchema.Constants.TableName);
			}
			return query;
		}

		protected virtual ZQuery GetServiceDirectionCore(ZString value)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsForwardingModule || IsCFSModule)
			{
				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				ZString sQL = String.Format(CultureInfo.InvariantCulture, @"
							JS_PK IN (	
								SELECT JS_PK
								FROM	dbo.JobShipment JS
								CROSS APPLY (SELECT TOP 1 Direction from dbo.GetJobDirection(JS_RL_NKOrigin, JS_RL_NKDestination, @homePortCode, @homePortCode, @IsDomesticDept, @IsExportDept, @IsImportDept, @companyPK)) dir
								WHERE dir.Direction = '{0}'
							)", value);

				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@companyPK", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
				@params.Add("@homePortCode", GlbBranch.CurrentBranch.HomePort.Code, GlbBranchSchema.GB_RL_NKHomePort);
				@params.Add("@IsDomesticDept", GlbDepartment.CurrentDepartment.GE_Domestic, GlbDepartmentSchema.GE_Domestic);
				@params.Add("@IsExportDept", GlbDepartment.CurrentDepartment.GE_Export, GlbDepartmentSchema.GE_Export);
				@params.Add("@IsImportDept", GlbDepartment.CurrentDepartment.GE_Import, GlbDepartmentSchema.GE_Import);
				shipmentQuery.AddFilterAndZSQLParameterCollection(sQL, @params);
				jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
			}

			if (IsCustomsModule)
			{
				var msgTypes = new List<string>();
				switch (value)
				{
					case "IMP":
						msgTypes.Add(JobMessageTypeList.Codes.Import);
						msgTypes.Add(JobMessageTypeList.Codes.WarehousedByExternalAgent);
						msgTypes.Add(JobMessageTypeList.Codes.ImportDeclarationByExternalBroker);
						break;
					case "EXP":
						msgTypes.Add(JobMessageTypeList.Codes.Export);
						msgTypes.Add(JobMessageTypeList.Codes.ExportDeclarationByExternalBroker);
						break;
				}

				var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
				jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, msgTypes);
				jobHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.Or);
			}

			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return jobHeaderQuery;
		}

		public ZQuery GetCarrierQuery(ZGuid carrierPK)
		{
			return GetCarrierQueryCore(carrierPK);
		}

		protected virtual ZQuery GetCarrierQueryCore(ZGuid carrierPK)
		{
			var jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (carrierPK.IsValid)
			{
				if (IsAgencyModule)
				{
					var shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobShipmentSchema.JS_OA_BookedShippingLineAddress);
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);

					shipmentQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}
			}

			return jobHeaderQuery;
		}

		public ZQuery GetServiceLevel(ZString serviceLevelCode)
		{
			return GetServiceLevelCore(serviceLevelCode);
		}

		protected virtual ZQuery GetServiceLevelCore(ZString serviceLevelCode)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (IsForwardingModule)
			{
				var masterQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				masterQuery.IgnoreActiveFilter = true;
				masterQuery.AddToFilter(JobShipmentSchema.JS_RS_NKServiceLevel, serviceLevelCode);
				masterQuery.AddToFilter(JobShipmentSchema.JS_IsCancelled, 0);

				var ratingQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);
				ratingQuery.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, 1);

				var shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.JS_TH_OneTimeQuote);
				shipmentQuery.IgnoreActiveFilter = true;
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_RS_NKServiceLevel, serviceLevelCode);
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsBooking, 1);
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, 0);
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, 0);
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, 0);
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsCancelled, 0);

				ratingQuery.AddSubQuery(RatingHeaderSchema.PK, shipmentQuery, JoinCondition.And);
				masterQuery.AddAsUnionQuery(ratingQuery);

				jobHeaderQuery.AddSubQuery(masterQuery, JoinCondition.Or);
			}

			if (IsCFSModule || IsAgencyModule)
			{
				var query = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				query.AddToFilter(JobShipmentSchema.JS_RS_NKServiceLevel, serviceLevelCode);
				jobHeaderQuery.AddSubQuery(query, JoinCondition.Or);
			}

			if (IsConsignmentModule)
			{
				var query = new ZDBOnlySubQuery(typeof(IDtbConsignment), JobHeaderQueryLinkColumn);
				query.AddToFilter(DtbConsignmentSchema.LTC_RS_NKServiceLevel, serviceLevelCode);
				jobHeaderQuery.AddSubQuery(query, JoinCondition.Or);
			}

			if (IsCustomsModule)
			{
				var query = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderQueryLinkColumn);
				query.AddToFilter(JobDeclarationSchema.JE_RS_NKServiceLevel, serviceLevelCode);
				jobHeaderQuery.AddSubQuery(query, JoinCondition.Or);
			}

			if (IsTransportModule)
			{
				var query = new ZDBOnlySubQuery(typeof(CommonCartage), JobHeaderQueryLinkColumn);
				query.AddToFilter(JobCartageSchema.JJ_RS_NKServiceLevel, serviceLevelCode);
				jobHeaderQuery.AddSubQuery(query, JoinCondition.Or);
			}

			if (IsWarehouseModule)
			{
				var query = new ZDBOnlySubQuery(typeof(IWhsDocket), JobHeaderQueryLinkColumn);
				query.AddToFilter(WhsDocketSchema.WD_RS_NKServiceLevel, serviceLevelCode);
				jobHeaderQuery.AddSubQuery(query, JoinCondition.Or);
			}

			return jobHeaderQuery;
		}

		public ZQuery GetPrincipalQuery(ZGuid principalPK)
		{
			return GetPrincipalQueryCore(principalPK);
		}

		protected virtual ZQuery GetPrincipalQueryCore(ZGuid principalPK)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if (principalPK.IsValid)
			{
				if (IsAgencyModule)
				{
					ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_OH_DeliveryAgent, principalPK);
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
					jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.Or);
				}
			}

			return jobHeaderQuery;
		}

		public ZQuery GetVoyageAndVesselQuery(SQLComparisonOperator comCperator, ZString voyage, ZString vessel)
		{
			return GetVoyageAndVesselQueryCore(comCperator, voyage, vessel);
		}

		protected virtual ZQuery GetVoyageAndVesselQueryCore(SQLComparisonOperator comCperator, ZString voyage, ZString vessel)
		{
			ZDBOnlyQuery jobQuery = new ZDBOnlyQuery(JobHeaderQueryBizObjType);

			if ((IsAgencyModule || IsForwardingModule) && (!string.IsNullOrWhiteSpace(voyage) || !string.IsNullOrWhiteSpace(vessel)))
			{
				var voyageQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderQueryLinkColumn);
				var builder = new SailingFilterBuilder(Factory);
				builder.VoyageFlightComparisonOperator = comCperator;
				builder.Vessel = vessel;
				builder.VoyageFlight = voyage;
				builder.IncludeArchived = false;
				voyageQuery.AddToFilter(builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports));
				jobQuery.AddSubQuery(voyageQuery, JoinCondition.Or);
			}

			return jobQuery;
		}

		public virtual ZString[] GetJobTypesApplicableToAFilter(string filterName)
		{
			return null;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}

		protected virtual Type JobHeaderQueryBizObjType
		{
			get
			{
				return typeof(Job);
			}
		}

		protected virtual SchemaColumn JobHeaderQueryLinkColumn
		{
			get
			{
				return JobHeaderSchema.JH_ParentID;
			}
		}
	}
}
