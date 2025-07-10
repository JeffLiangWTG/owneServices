using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageLayout : IPanelLayoutProvider
	{
		PanelLayout UCC6TemporaryStorage { get; }

		PanelLayout IPanelLayoutProvider.Layout => UCC6TemporaryStorage;

		public UCC6TemporaryStorageLayout()
		{
			UCC6TemporaryStorage = CreateUCC6TemporaryStorageLayout();
		}

		PanelLayout CreateUCC6TemporaryStorageLayout()
		{
			var builder = new TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.DeclarationDateDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.IsENSReuseCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.PresentationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GoodsPresentationDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.EstimatedDateOfArrivalDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PersonPresentingTheGoodsAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.PlaceOfUnloadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.HasHouseConsignmentCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.HasNoMasterBillCheckBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.LRNTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsStatusDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.MRNTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CRNTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.FRNTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CusAgentCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GuaranteeGroupBox, ControlWidthClass.Auto);
			builder.Add(commonBag.DocumentsTabControl, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.IsENSReuseCheckBox, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.TransportModeDropEdit, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.TransportTypeDropEdit, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.ArrivalTransportMeansCodeTextBox, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.PresentationCustomsOfficeCodeFindBox, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.GoodsPresentationDateEdit, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.EstimatedDateOfArrivalDateEdit, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.PersonPresentingTheGoodsAddressControl, x => !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.LocationOfGoodsUserControl, x => !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AuthorizationTypeDropEdit, x => !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AuthorizationOwnerGuidFindBox, x => !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AuthorizationNumberCodeFindBox, x => !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.CarrierAddressControl, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.PlaceOfUnloadingCodeFindBox, x => !x.IsTransfer && !x.IsDeconsolidation, x => x.AMA_MessageTypeInfo);

			return builder.Build();
		}
	}
}
