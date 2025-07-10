using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TemporaryStorageUserControlBag : ControlBag
	{
		public static TemporaryStorageUserControlBag Instance => TemporaryStorageControlBag.Value;

		protected TemporaryStorageUserControlBag()
		{
			CountryCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.CountryCodeFindBox));
			DeclarationDateDateEdit = RegisterControl(nameof(TemporaryStorageUserControl.DeclarationDateDateEdit));
			IsENSReuseCheckBox = RegisterControl(nameof(TemporaryStorageUserControl.IsENSReuseCheckBox));
			TransportModeDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.TransportModeDropEdit));
			TransportTypeDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.TransportTypeDropEdit));
			ArrivalTransportMeansCodeTextBox = RegisterControl(nameof(TemporaryStorageUserControl.ArrivalTransportMeansCodeTextBox));
			MessageTypeDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.MessageTypeDropEdit));
			DeclarantAddressControl = RegisterControl(nameof(TemporaryStorageUserControl.DeclarantAddressControl));
			RepresentativeAddressControl = RegisterControl(nameof(TemporaryStorageUserControl.RepresentativeAddressControl));
			SupervisingCustomsOfficeCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.SupervisingCustomsOfficeCodeFindBox));
			PresentationCustomsOfficeCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.PresentationCustomsOfficeCodeFindBox));
			GoodsPresentationDateEdit = RegisterControl(nameof(TemporaryStorageUserControl.GoodsPresentationDateEdit));
			EstimatedDateOfArrivalDateEdit = RegisterControl(nameof(TemporaryStorageUserControl.EstimatedDateOfArrivalDateEdit));
			PersonPresentingTheGoodsAddressControl = RegisterControl(nameof(TemporaryStorageUserControl.PersonPresentingTheGoodsAddressControl));
			CarrierAddressControl = RegisterControl(nameof(TemporaryStorageUserControl.CarrierAddressControl));
			PlaceOfUnloadingCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.PlaceOfUnloadingCodeFindBox));
			PlaceOfLoadingCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.PlaceOfLoadingCodeFindBox));
			LocationOfGoodsUserControl = RegisterControl(nameof(TemporaryStorageUserControl.LocationOfGoodsUserControl));
			GuaranteeGroupBox = RegisterControl(nameof(TemporaryStorageUserControl.GuaranteeGroupBox));
			LRNTextBox = RegisterControl(nameof(TemporaryStorageUserControl.LRNTextBox));
			MRNTextBox = RegisterControl(nameof(TemporaryStorageUserControl.MRNTextBox));
			CRNTextBox = RegisterControl(nameof(TemporaryStorageUserControl.CRNTextBox));
			FRNTextBox = RegisterControl(nameof(TemporaryStorageUserControl.FRNTextBox));
			MessageStatusDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.MessageStatusDropEdit));
			CustomsStatusDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.CustomsStatusDropEdit));
			CustomsStatusDateEdit = RegisterControl(nameof(TemporaryStorageUserControl.CustomsStatusDateEdit));
			AuthorizationOwnerGuidFindBox = RegisterControl(nameof(TemporaryStorageUserControl.AuthorizationOwnerGuidFindBox));
			AuthorizationNumberCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.AuthorizationNumberCodeFindBox));
			AuthorizationTypeDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.AuthorizationTypeDropEdit));
			CusAgentCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.CusAgentCodeFindBox));
			DocumentsTabControl = RegisterControl(nameof(TemporaryStorageUserControl.DocumentsTabControl));
			HasHouseConsignmentCheckBox = RegisterControl(nameof(TemporaryStorageUserControl.HasHouseConsignmentCheckBox));
			HasNoMasterBillCheckBox = RegisterControl(nameof(TemporaryStorageUserControl.HasNoMasterBillCheckBox));
		}

		public ControlReference CountryCodeFindBox { get; }

		public ControlReference DeclarationDateDateEdit { get; }

		public ControlReference MessageTypeDropEdit { get; }

		public ControlReference IsENSReuseCheckBox { get; }

		public ControlReference TransportModeDropEdit { get; }

		public ControlReference TransportTypeDropEdit { get; }

		public ControlReference ArrivalTransportMeansCodeTextBox { get; }

		public ControlReference DeclarantAddressControl { get; }

		public ControlReference RepresentativeAddressControl { get; }

		public ControlReference SupervisingCustomsOfficeCodeFindBox { get; }

		public ControlReference PresentationCustomsOfficeCodeFindBox { get; }

		public ControlReference GoodsPresentationDateEdit { get; }

		public ControlReference EstimatedDateOfArrivalDateEdit { get; }

		public ControlReference PersonPresentingTheGoodsAddressControl { get; }

		public ControlReference CarrierAddressControl { get; }

		public ControlReference PlaceOfUnloadingCodeFindBox { get; }

		public ControlReference PlaceOfLoadingCodeFindBox { get; }

		public ControlReference LocationOfGoodsUserControl { get; }

		public ControlReference GuaranteeGroupBox { get; }

		public ControlReference LRNTextBox { get; }

		public ControlReference MessageStatusDropEdit { get; }

		public ControlReference CustomsStatusDropEdit { get; }
		public ControlReference CustomsStatusDateEdit { get; }

		public ControlReference AuthorizationOwnerGuidFindBox { get; }
		public ControlReference AuthorizationNumberCodeFindBox { get; }
		public ControlReference AuthorizationTypeDropEdit { get; }

		public ControlReference MRNTextBox { get; }

		public ControlReference CRNTextBox { get; }

		public ControlReference FRNTextBox { get; }

		public ControlReference CusAgentCodeFindBox { get; }

		public ControlReference DocumentsTabControl { get; }

		public ControlReference HasHouseConsignmentCheckBox { get; }

		public ControlReference HasNoMasterBillCheckBox { get; }

		protected override Control CreateTemplate() => new TemporaryStorageUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TemporaryStorageUserControlBag> TemporaryStorageControlBag = new Lazy<TemporaryStorageUserControlBag>(() => new TemporaryStorageUserControlBag());
	}
}
