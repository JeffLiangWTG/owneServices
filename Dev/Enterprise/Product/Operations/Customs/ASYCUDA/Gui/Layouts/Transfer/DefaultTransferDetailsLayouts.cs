using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class DefaultTransferDetailsLayouts : IPanelLayoutProvider
	{
		public DefaultTransferDetailsLayouts()
		{
			TransferDetailsLayout = CreateTransferDetailsLayout();
		}

		PanelLayout TransferDetailsLayout { get; }

		PanelLayout IPanelLayoutProvider.Layout => TransferDetailsLayout;

		PanelLayout CreateTransferDetailsLayout()
		{
			var builder = new TransferDetailsLayoutBuilder<AsycudaTransferHeader>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.DestinationPortCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.TransferTypeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierIDTextBox, ControlWidthClass.Long);
			builder.Add(common.OnwardCarrierCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.DestinationWarehouseAddressControl, ControlWidthClass.Long);
			builder.Add(common.DestinationWarehouseIDTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
