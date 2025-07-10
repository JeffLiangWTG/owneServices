using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStorageLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	public UCC6TemporaryStorageLayout()
	{
		Layout = CreateLayout();
	}

	PanelLayout CreateLayout()
	{
		var builder = new TemporaryStorageLayoutBuilder();
		var euBag = builder.CommonBag;
		var itBag = TemporaryStorageUserControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddColumn();
		builder.Add(euBag.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.TransportTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
		builder.Add(euBag.DeclarantAddressControl, ControlWidthClass.Long);
		builder.Add(euBag.RepresentativeAddressControl, ControlWidthClass.Long);
		builder.Add(itBag.RepresentativeQualificationDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.AccountNameDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.PresentationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.LocationOfGoodsUserControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.MessageStatusDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CustomsStatusDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CustomsStatusDateEdit, ControlWidthClass.Medium);

		builder.SetVisibility(euBag.TransportModeDropEdit, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
		builder.SetVisibility(euBag.TransportTypeDropEdit, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
		builder.SetVisibility(euBag.ArrivalTransportMeansCodeTextBox, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
		builder.SetVisibility(euBag.PresentationCustomsOfficeCodeFindBox, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);

		return builder.Build();
	}
}
