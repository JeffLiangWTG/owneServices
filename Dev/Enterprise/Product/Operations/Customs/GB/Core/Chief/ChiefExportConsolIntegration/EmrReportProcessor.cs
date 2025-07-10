using CargoWise.Types;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	class EmrReportProcessor : ErsReportProcessor
	{
		public EmrReportProcessor(EmrReport emrReport, EDIMessage ediMessage, UkcinvUnderstander ukCinvUnderstander)
			: base(emrReport, ediMessage, ukCinvUnderstander)
		{
			this.emrReport = emrReport;
		}

		protected override void UpdateAndProcessConsol(MawbExportAddInfo mawbExportAddInfo, IRoutingProvider provider)
		{
			base.UpdateAndProcessConsol(mawbExportAddInfo, provider);
			if (!emrReport.CustomsReturnCode.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefCustomsReturnCode = emrReport.CustomsReturnCode.Left(mawbExportAddInfo.ME_ChiefCustomsReturnCodeInfo.MaxLength);
			}

			if (!emrReport.MasterRouteOfEntry.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefMasterRouteOfEntry = emrReport.MasterRouteOfEntry.Left(mawbExportAddInfo.ME_ChiefMasterRouteOfEntryInfo.MaxLength);
			}

			if (!emrReport.MasterStyleOfEntry.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefMasterStyleOfEntry = emrReport.MasterStyleOfEntry.Left(mawbExportAddInfo.ME_ChiefMasterStyleOfEntryInfo.MaxLength);
			}
		}

		protected override void AddAndPositivelyUpdateReceivedMessage(ForwardingConsol consol)
		{
			base.AddAndPositivelyUpdateReceivedMessage(consol);
			inboundEdiMessage.EM_MessageSubType = emrReport.CustomsReturnCode.Left(inboundEdiMessage.EM_MessageSubTypeInfo.MaxLength);
		}

		protected override HtmlTableCreator CreateHeaderTable()
		{
			var headerHtmlTable = new HtmlTableCreator(new string[] { "Field", "Value", "Meaning" });
			headerHtmlTable.WriteRow("Master UCR", ersReport.MasterUCR, "");
			headerHtmlTable.WriteRow("Movement Reference", ersReport.MovementReference, "");
			headerHtmlTable.WriteRow("Goods' location & Shed", ersReport.GoodsLocation + ersReport.Shed, "");
			headerHtmlTable.WriteRow("Arrival Date", ersReport.GoodsArrivalDateTime, "");
			headerHtmlTable.WriteRow("EPU", ersReport.EntryProcessingUnitNumber + " " + ersReport.EntryProcessingUnitID, "");
			headerHtmlTable.WriteRow("CRC", emrReport.CustomsReturnCode, emrReport.CustomsReturnCodeHuman);
			headerHtmlTable.WriteRow("Master Route", emrReport.MasterRouteOfEntry, GetRouteMeaning(emrReport.MasterRouteOfEntry));
			headerHtmlTable.WriteRow("Master Style", emrReport.MasterStyleOfEntry, GetStyleMeaning(emrReport.MasterStyleOfEntry));
			return headerHtmlTable;
		}

		protected override ZString MessageTitle
		{
			get { return "EMR - Notification of Master Route and Status"; }
		}

		readonly EmrReport emrReport;
	}
}
