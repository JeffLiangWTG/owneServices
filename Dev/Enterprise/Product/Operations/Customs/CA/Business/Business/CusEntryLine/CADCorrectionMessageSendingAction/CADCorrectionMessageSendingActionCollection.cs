using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public sealed class CADCorrectionMessageSendingActionCollection : Customs.Business.CusSupportingInfoCollection<CADCorrectionMessageSendingAction>
	{
		public CADCorrectionMessageSendingActionCollection(BusinessObject parent)
			: base(parent, Customs.Common.CA.CusSupportingInfoTypeList.Codes.CadCorrectionMessageSendingAction)
		{
		}

		protected override bool AllowNewCore => Master is CADCorrectionMessageSendingActionWrapper;

		protected override bool AllowRemoveCore => Master is CADCorrectionMessageSendingActionWrapper;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var action = (CADCorrectionMessageSendingAction)child;
			var master = (CADCorrectionMessageSendingActionWrapper)Master;
			var maxLineNum = (master.CADEntryHeader.MergedLines.SelectMany(line => line.AmendmentDetails.Cast<CADCorrectionMessageSendingAction>()).MaxOrDefault(x => x.CSI_LineNo)) + 1;
			action.CSI_LineNo = maxLineNum;
		}
	}
}
