using System;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	sealed class Phase5MessageSendingGridColumnBag
	{
		public static Phase5MessageSendingGridColumnBag Instance => instance ?? (instance = new Phase5MessageSendingGridColumnBag());

		[ThreadStatic]
		static Phase5MessageSendingGridColumnBag instance;

		Phase5MessageSendingGridColumnBag()
		{
			DestinationCustomsOfficeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.DestinationCustomsOfficeCode, 100);
			ConsigneeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.Consignee, 100);
			TC11DeliveryDateColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.TC11DeliveryDate, 60);
			EnquiryTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.EnquiryText, 160);
			MessageStatusTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsHeaderMessageSendingObject.Schema.MessageStatus, 60);
		}

		public IGridColumnReference DestinationCustomsOfficeCodeFindBoxColumn { get; }

		public IGridColumnReference ConsigneeFindBoxColumn { get; }

		public IGridColumnReference TC11DeliveryDateColumn { get; }

		public IGridColumnReference EnquiryTextBoxColumn { get; }

		public IGridColumnReference MessageStatusTextBoxColumn { get; }
	}
}
