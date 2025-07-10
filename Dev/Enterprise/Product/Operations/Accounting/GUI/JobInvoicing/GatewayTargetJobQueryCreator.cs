using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	internal static class GatewayTargetJobQueryCreator
	{
		internal static ZDBOnlyQuery CreateQueryForInvoiceTargetJob(BusinessObjectFactory factory, ForwardingShipment forwardingShipment)
		{
			Argument.NotNull(forwardingShipment, "forwardingShipment");

			var possibleGatewayConsolPks = new List<ZGuid>();
			possibleGatewayConsolPks.AddRange(forwardingShipment.Consols.GetPKs());
			return CreateQueryForInvoiceTargetJobCore(factory, possibleGatewayConsolPks, forwardingShipment.PK);
		}

		internal static ZDBOnlyQuery CreateQueryForInvoiceTargetJob(BusinessObjectFactory factory, ForwardingConsol forwardingConsol)
		{
			Argument.NotNull(forwardingConsol, "forwardingConsol");

			var possibleGatewayConsolPks = new List<ZGuid>();
			var attachedShipments = forwardingConsol.Shipments.Cast<ForwardingShipment>();
			attachedShipments.ForEach(s => possibleGatewayConsolPks.AddRange(s.Consols.GetPKs()));
			return CreateQueryForInvoiceTargetJobCore(factory, possibleGatewayConsolPks, forwardingConsol.PK);
		}

		static ZDBOnlyQuery CreateQueryForInvoiceTargetJobCore(BusinessObjectFactory factory, IEnumerable<ZGuid> possibleGatewayConsolPks, ZGuid invoiceTargetId)
		{
			var jobHeaderPKs = GetJobHeaderPKs(factory, possibleGatewayConsolPks.Distinct());
			if (!jobHeaderPKs.Any())
			{
				return null;
			}
			else
			{
				var jobChargeTargetQuery = new ZDBOnlySubQuery(typeof(JobChargeTarget), JobChargeTargetSchema.JRT_JR);
				jobChargeTargetQuery.AddToFilter(JobChargeTargetSchema.JRT_InvoiceTargetID, invoiceTargetId);

				var jobChargeQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AL_ARLine);
				jobChargeQuery.AddSubQuery(JobChargeSchema.PK, jobChargeTargetQuery, JoinCondition.And);

				var transactionLineQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				transactionLineQuery.AddSubQuery(AccTransactionLinesSchema.PK, jobChargeQuery, JoinCondition.And);

				var transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				transactionHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_JH, jobHeaderPKs.Distinct());
				transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.PK, transactionLineQuery, JoinCondition.And);
				return transactionHeaderQuery;
			}
		}

		static IEnumerable<ZGuid> GetJobHeaderPKs(BusinessObjectFactory factory, IEnumerable<ZGuid> possibleGatewayConsolPks)
		{
			var businessObjCollection = new DynamicBusinessObjectCollection(factory);
			var parameters = new[] { ZSqlParameter.New("@ParentIDs", possibleGatewayConsolPks.ToArray(), JobHeaderSchema.JH_ParentID, true) };
			businessObjCollection.Load(@"SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN (SELECT Value FROM @ParentIDs)", parameters);
			return businessObjCollection.Select(x => (ZGuid)(x[JobHeaderSchema.Constants.PK]));
		}
	}
}
