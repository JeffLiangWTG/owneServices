using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOReceivalMessageManager : CMRMessageManager
	{
		public CTOReceivalMessageManager(ExportCustomsManifestLines line)
		{
			this.line = line;
		}
		readonly ExportCustomsManifestLines line;

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { line.CTORECStatusCalculator };

		internal override CMRAmendmentGenerator GetAmendmentManager(CargoWise.EntityFramework.BusinessObject bizo) => new CTORECAmendmentGenerator((ExportCustomsManifestLines)bizo);

		internal override EDIMessageCollection GetMessages(CargoWise.EntityFramework.BusinessObject bizo) => ((ExportCustomsManifestLines)bizo).Messages;

		internal override string GetStatus() => line.CTORECStatus.Code;

		internal override CMRMessageBuilder[] GetBuilder(CargoWise.EntityFramework.BusinessObject bizo) => new CMRMessageBuilder[] { new CTORECMessageBuilder((ExportCustomsManifestLines)bizo) };

		public override CargoWise.EntityFramework.BusinessObject BusinessObject => line;

		public override string MessageFriendlyName => "CTO Receival Report - #" + line.EL_LineNo + " - " + (line.IsExemptLine ? line.EL_TypeOfCAN : line.EL_CAN);
	}
}
