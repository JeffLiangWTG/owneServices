using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5MessageSendingGridColumnBag
	{
		public static Phase5MessageSendingGridColumnBag Instance => instance ?? (instance = new Phase5MessageSendingGridColumnBag());

		[ThreadStatic]
		static Phase5MessageSendingGridColumnBag instance;

		Phase5MessageSendingGridColumnBag()
		{
			ShouldSendCheckBoxColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.ShouldSend, 40);
			LrnTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoNctsHeaderMessageSendingObject.Schema.LRN, 160, c => c.IsMandatory = true);
			MrnTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoNctsHeaderMessageSendingObject.Schema.MRN, 160, c => c.IsMandatory = true);
			MessageTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(AutoNctsHeaderMessageSendingObject.Schema.MessageType, 100, c => c.IsMandatory = true);
			ReleaseRequestDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.ReleaseRequest, 100, c => c.IsMandatory = true);
			JustificationTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.Justification, 100, c => c.IsMandatory = true);
		}

		public IGridColumnReference ShouldSendCheckBoxColumn { get; }

		public IGridColumnReference LrnTextBoxColumn { get; }

		public IGridColumnReference MrnTextBoxColumn { get; }

		public IGridColumnReference MessageTypeDropEditColumn { get; }

		public IGridColumnReference ReleaseRequestDropEditColumn { get; }

		public IGridColumnReference JustificationTextBoxColumn { get; }
	}
}
