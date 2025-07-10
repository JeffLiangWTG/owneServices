using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class UCC5TemporaryStorageLayout : IPanelLayoutProvider
	{
		public UCC5TemporaryStorageLayout()
		{
			layout = CreateLayout();
		}

		readonly PanelLayout layout;
		PanelLayout IPanelLayoutProvider.Layout => layout;

		PanelLayout CreateLayout()
		{
			var builder = new UCC5TemporaryStorageLayoutBuilder();
			var commonBag = builder.CommonBag;
			var ieBag = TemporaryStorageUserControlBag.Instance;

			builder.AddControlBag(ieBag);

			builder.AddColumn();
			builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Medium);
			builder.Add(ieBag.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarationDateDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
			builder.Add(ieBag.BorderTransportTypeDropEdit, ControlWidthClass.Long);
			builder.Add(ieBag.BorderTransportIDTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(ieBag.RepresentativeStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.PresentationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(ieBag.CustomsOfficeofLodgementCodeFindBox, ControlWidthClass.Long);
			builder.Add(ieBag.CustomsOfficeOfFirstEntryCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GoodsPresentationDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.EstimatedDateOfArrivalDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PersonPresentingTheGoodsAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.PlaceOfUnloadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.PlaceOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.HasHouseConsignmentCheckBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.LRNTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsStatusDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.MRNTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CusAgentCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DocumentsTabControl, ControlWidthClass.Auto);

			builder.AddControlBehaviour<ZDateEdit>(commonBag.CustomsStatusDateEdit, (customsStatusDateEdit, header) =>
			{
				customsStatusDateEdit.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			});

			return builder.Build();
		}
	}
}
