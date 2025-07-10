using CargoWise.Types;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	class EaaEalReportProcessor : EmrReportProcessor
	{
		public EaaEalReportProcessor(EaaEalReport eaaEalReport, EDIMessage ediMessage, UkcinvUnderstander ukCinvUnderstander)
			: base(eaaEalReport, ediMessage, ukCinvUnderstander)
		{
			this.eaaEalReport = eaaEalReport;
		}

		protected override void UpdateAndProcessConsol(MawbExportAddInfo mawbExportAddInfo, IRoutingProvider provider)
		{
			base.UpdateAndProcessConsol(mawbExportAddInfo, provider);
			if (!eaaEalReport.CustomsReturnCode.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefCustomsReturnCode = eaaEalReport.CustomsReturnCode.Left(mawbExportAddInfo.ME_ChiefCustomsReturnCodeInfo.MaxLength);
			}
		}

		protected override ZString MessageTitle
		{
			get { return ukCinvUnderstander.Class.ToString(); }
		}

		readonly EaaEalReport eaaEalReport;
	}
}
