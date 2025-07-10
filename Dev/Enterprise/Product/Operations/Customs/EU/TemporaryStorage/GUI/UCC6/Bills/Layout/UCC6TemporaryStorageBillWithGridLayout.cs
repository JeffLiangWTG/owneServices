using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageBillWithGridLayout : IPanelLayoutWithGridProvider
	{
		public UCC6TemporaryStorageBillWithGridLayout()
		{
			BillDetailLayout = CreateUCC6TemporaryStorageLayout();
		}

		PanelLayout BillDetailLayout { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetailLayout;

		public Type GridUserControlType => typeof(UCC6TemporaryStorageBillGridControl);

		PanelLayout CreateUCC6TemporaryStorageLayout()
		{
			var builder = new UCC6TemporaryStorageBillDetailBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.BillNumberTextEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UCRNumberTextEdit, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightWithUnitUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ConsignorAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyAddressControl, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.UCRNumberTextEdit, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.ConsignorAddressControl, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.ConsigneeAddressControl, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.NotifyPartyAddressControl, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);

			return builder.Build();
		}
	}
}
