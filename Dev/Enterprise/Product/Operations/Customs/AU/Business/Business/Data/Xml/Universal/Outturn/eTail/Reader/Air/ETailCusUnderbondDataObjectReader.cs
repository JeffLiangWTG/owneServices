using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ETailCusUnderbondDataObjectReader : ShipmentDataObjectReader<CusUnderbond>
	{
		public ETailCusUnderbondDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			var dataContextDataSourceCollection = dataObject.DataContext?.DataSourceCollection;
			if (dataContextDataSourceCollection != null)
			{
				forwardingConsolNumber = dataContextDataSourceCollection.FirstOrDefault(o => o.Type.ToString() == nameof(DataContextType.ForwardingConsol))?.Key;
				forwardingShipmentNumber = dataContextDataSourceCollection.FirstOrDefault(o => o.Type.ToString() == nameof(DataContextType.ForwardingShipment))?.Key;

				var forwardingConsol = factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, forwardingConsolNumber ?? ZString.Empty);
				var forwardingShipment = factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, forwardingShipmentNumber ?? ZString.Empty);

				if (forwardingConsol != null && forwardingShipment != null)
				{
					masterBill = forwardingConsol.JK_MasterBillNum;
					var cusMAWBQuery = new ZQuery(CusMAWBSchema.CM_MAWB, masterBill);
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, forwardingShipment.JS_HouseBill);
					var mAWBRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
					if (mAWBRecyclePeriod > 0)
					{
						cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
					}
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, ZBool.False);
					cusMAWBQuery.OrderBy = CusMAWBSchema.Constants.CM_SystemCreateTimeUtc + " desc";
					cusMAWB = factory.LoadTop1<CusMAWB>(cusMAWBQuery);
				}
			}
		}

		protected override void PopulateBusinessObject(CusUnderbond underbond)
		{
			var subshipments = dataObject.SubShipmentCollection;
			if (subshipments?.Any() ?? false)
			{
				SetOutturnDate(underbond);
				foreach (var shipment in subshipments.Where(x => x.ShipmentType.GetCodeAsUpperCase() == Core.Constants.ShipmentTypes.HighVolumeLowValue))
				{
					if (shipment.SubShipmentCollection != null)
					{
						foreach (var subshipment in shipment.SubShipmentCollection)
						{
							if (subshipment.DataContext.DataSourceCollection?.Any(o => o.Type.GetValueOrDefault() == Constants.HVLVConsignment) ?? false)
							{
								var reader = new ETailCusOutturnDataObjectReader(subshipment, logger, factory, underbond);
								reader.ReadIntoBusinessObject();
							}
						}
					}
				}
			}

			if (underbond.MAWB is CusMAWB mawb)
			{
				mawb.LogOutturnsReadyForSendingEvent(logger);
			}
		}

		void SetOutturnDate(CusUnderbond underbond)
		{
			var extraQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "RES=Outturn");
			var query = cusMAWB.Logs.MostRecentLogByEventTimeQuery(Events.HVLVReady, extraQuery);
			var evt = underbond.Factory.LoadTop1<StmALog>(query);
			underbond.C4_Outurned = evt != null ? evt.SL_EventTime : ZDateTime.Empty;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusUnderbond underbond)
		{
			var builder = new ZStringBuilder();
			if (cusMAWB == null)
			{
				builder.Append(Res.GetString("465556215-997D-42DE-A94C-B10ABED411CE", "No Air Cargo Report found for Consol {0} and Shipment {1}", forwardingConsolNumber, forwardingShipmentNumber));
			}
			else if (underbond == null)
			{
				builder.Append(Res.GetString("EEB61247-EBDF-4298-869B-33A39C3C6810", "No underbond found for Air Cargo Report with Master Bill {0}", masterBill));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override CusUnderbond GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CusUnderbond result = null;
			if (cusMAWB != null)
			{
				var underbondQuery = new ZQuery(CusUnderbondSchema.C4_ParentID, cusMAWB.PK);
				underbondQuery.OrderBy = CusUnderbondSchema.Constants.C4_ArrivalDate + " desc";
				result = factory.LoadTop1<CusUnderbond>(underbondQuery);
			}
			return result;
		}

		protected override IMatchingBusinessEntityFinder<CusUnderbond> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference Matching has been implemented for Underbond. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		public override DataContextType DataContextType => DataContextType.UnderBond;

		readonly ZString? forwardingConsolNumber;
		readonly ZString? forwardingShipmentNumber;
		readonly ZString? masterBill;
		readonly CusMAWB cusMAWB;
	}
}
