
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture.Schema;

	[ApplicationIdentifier(ServiceOptions.Codes.SupplementaryCargoReport)]
	public class SupplementaryCargoReportResponseMessageProcessor : ACIResponseMessageProcessorBase
	{
		public SupplementaryCargoReportResponseMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypeList.Codes.SupplementaryCargoReport, Res.GetString("103e8f3c-8e11-4d68-94c3-13f641876f18", "Supplementary Cargo Response"))
		{
		}

		#region Overridden Properties

		protected override IEDIFACTMessageAttachee GetLinkedObjectAndSetOnMessage(string objectReference)
		{
			var query = new ZQuery();
			var scaHouseQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, objectReference);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CusSCAHouse.Schema.TableName);
			scaHouseQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
			query.AddToFilter(scaHouseQuery);
			var cusSCAHouse = message.Factory.LoadTop1<CusSCAHouse>(query);
			if (cusSCAHouse != null)
			{
				message.EM_LinkUniqueID = cusSCAHouse.PK;
				message.EM_LinkTable = CusSCAHouseSchema.Constants.TableName;
				shipment = cusSCAHouse.Shipment;
			}
			return cusSCAHouse;
		}

		protected override EDIFACTMessageStatusCalculator StatusCalculator
		{
			get { return statusCalculator ?? (statusCalculator = new SupplementaryCargoReportStatusCalculator()); }
		}
		EDIFACTMessageStatusCalculator statusCalculator;

		protected override BusinessObject EmailResponseLinkedObject
		{
			get { return (BusinessObject)linkedObject; }
		}

		protected override ZString JobNumberLink
		{
			get { return EmailDefBuilder.GetJobLink(shipment, shipment.JS_UniqueConsignRef); }
		}

		#endregion

		ForwardingShipment shipment;
	}
}
