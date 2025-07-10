using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTORemovalMessageManager : CMRMessageManager
	{
		public CTORemovalMessageManager(ExportCustomsManifestLines line)
		{
			this.line = line;
		}
		readonly ExportCustomsManifestLines line;

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { line.CTOREMStatusCalculator };

		internal override CMRAmendmentGenerator GetAmendmentManager(CargoWise.EntityFramework.BusinessObject bizo) => new CTOREMAmendmentGenerator((ExportCustomsManifestLines)bizo);

		internal override EDIMessageCollection GetMessages(CargoWise.EntityFramework.BusinessObject bizo) => ((ExportCustomsManifestLines)bizo).Messages;

		internal override string GetStatus() => line.CTOREMStatus.Code;

		internal override CMRMessageBuilder[] GetBuilder(CargoWise.EntityFramework.BusinessObject bizo) => new CMRMessageBuilder[] { new CTOREMMessageBuilder((ExportCustomsManifestLines)bizo) };

		public override CargoWise.EntityFramework.BusinessObject BusinessObject => line;

		public override string MessageFriendlyName => "CTO Removal Report - #" + line.EL_LineNo + " - " + (line.IsExemptLine ? line.EL_TypeOfCAN : line.EL_CAN);

		public override bool CanSendOriginal => base.CanSendOriginal && IsReceivalSent;

		// Amendments goes off HasActiveMessages, which defaults to !CanSendOriginal in base.
		// We don't want our hiding of this manager until IsReceivalSent to trigger amendments.
		// Calling base.CanSendOriginal here doesn't work either.
		public override bool HasActiveMessages => IsReceivalSent && base.HasActiveMessages;

		bool IsReceivalSent => line.CTORECStatus.Code != CMRBaseStatuses.Codes.NotSent;
	}
}
