using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class CommonBillControlBag : ControlBag
	{
		public static CommonBillControlBag Instance => commonBillControlBag.Value;

		CommonBillControlBag()
		{
			BillNumberTextBox = RegisterControl(nameof(CommonBillUserControl.BillNumberTextBox));
			OriginCodeFindBox = RegisterControl(nameof(CommonBillUserControl.OriginCodeFindBox));
			CargoTypeDropEdit = RegisterControl(nameof(CommonBillUserControl.CargoTypeDropEdit));
			FinalDestinationCodeFindBox = RegisterControl(nameof(CommonBillUserControl.FinalDestinationCodeFindBox));
			DischargePortCodeFindBox = RegisterControl(nameof(CommonBillUserControl.DischargePortCodeFindBox));
			ProcedureCodeFindBox = RegisterControl(nameof(CommonBillUserControl.ProcedureCodeFindBox));
			ManifestQtyCalcDropEdit = RegisterControl(nameof(CommonBillUserControl.ManifestQtyCalcDropEdit));
			ManifestQtyCalcEdit = RegisterControl(nameof(CommonBillUserControl.ManifestQtyCalcEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(CommonBillUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(CommonBillUserControl.NetWeightCalcDropEdit));
			VolumeCalcDropEdit = RegisterControl(nameof(CommonBillUserControl.VolumeCalcDropEdit));
			DepartureDateEdit = RegisterControl(nameof(CommonBillUserControl.DepartureDateEdit));
			BillIssueDateEdit = RegisterControl(nameof(CommonBillUserControl.BillIssueDateEdit));

			IncotermDropEdit = RegisterControl(nameof(CommonBillUserControl.IncotermDropEdit));
			GoodsDescriptionTextBox = RegisterControl(nameof(CommonBillUserControl.GoodsDescriptionTextBox));
			MarksAndNumbersTextBox = RegisterControl(nameof(CommonBillUserControl.MarksAndNumbersTextBox));
			ShipperAddressControl = RegisterControl(nameof(CommonBillUserControl.ShipperAddressControl));
			ConsigneeAddressControl = RegisterControl(nameof(CommonBillUserControl.ConsigneeAddressControl));
			NotifyPartyAddressControl = RegisterControl(nameof(CommonBillUserControl.NotifyPartyAddressControl));
			ForwarderAddressControl = RegisterControl(nameof(CommonBillUserControl.ForwarderAddressControl));
			DeliveryAgentAddressControl = RegisterControl(nameof(CommonBillUserControl.DeliveryAgentAddressControl));
			AgentAddressControl = RegisterControl(nameof(CommonBillUserControl.AgentAddressControl));
			BuyerAddressControl = RegisterControl(nameof(CommonBillUserControl.BuyerAddressControl));
			GoodsLocationAddressControl = RegisterControl(nameof(CommonBillUserControl.GoodsLocationAddressControl));

			CarrierReferenceTextBox = RegisterControl(nameof(CommonBillUserControl.CarrierReferenceTextBox));
			PrepaidCollectDropEdit = RegisterControl(nameof(CommonBillUserControl.PrepaidCollectDropEdit));
			RemarksTextBox = RegisterControl(nameof(CommonBillUserControl.RemarksTextBox));
			UCRNumberTextBox = RegisterControl(nameof(CommonBillUserControl.UCRNumberTextBox));

			TransportValueConvertToLocalCurrencyControl = RegisterControl(nameof(CommonBillUserControl.TransportValueConvertToLocalCurrencyControl));
			FreightValueConvertToLocalCurrencyControl = RegisterControl(nameof(CommonBillUserControl.FreightValueConvertToLocalCurrencyControl));
			InsuranceValueConvertToLocalCurrencyControl = RegisterControl(nameof(CommonBillUserControl.InsuranceValueConvertToLocalCurrencyControl));
			CustomsValueConvertToLocalCurrencyControl = RegisterControl(nameof(CommonBillUserControl.CustomsValueConvertToLocalCurrencyControl));
			DiscountValueConvertToLocalCurrencyControl = RegisterControl(nameof(CommonBillUserControl.DiscountValueConvertToLocalCurrencyControl));
			OtherChargesValueConvertToLocalCurrencyControl = RegisterControl(nameof(CommonBillUserControl.OtherChargesValueConvertToLocalCurrencyControl));

			CustomsEntryNumberTypeDropEdit = RegisterControl(nameof(CommonBillUserControl.CustomsEntryNumberTypeDropEdit));
			MessageStatusTextBox = RegisterControl(nameof(CommonBillUserControl.MessageStatusTextBox));
			RegistrationDateEdit = RegisterControl(nameof(CommonBillUserControl.RegistrationDateEdit));
			ShipmentTypeDropEdit = RegisterControl(nameof(CommonBillUserControl.ShipmentTypeDropEdit));
			CusJobNumberCodeFindBox = RegisterControl(nameof(CommonBillUserControl.CusJobNumberCodeFindBox));
			GoodsLocationDropEditWithFixedWidth = RegisterControl(nameof(CommonBillUserControl.GoodsLocationDropEditWithFixedWidth));
			BillStatusDropEdit = RegisterControl(nameof(CommonBillUserControl.BillStatusDropEdit));
			CargoStatusDropEdit = RegisterControl(nameof(CommonBillUserControl.CargoStatusDropEdit));
			LocationInformationTextBox = RegisterControl(nameof(CommonBillUserControl.LocationInformationTextBox));
			BillIssuerTextBox = RegisterControl(nameof(CommonBillUserControl.BillIssuerTextBox));
			CustomsEntryNumberTextBox = RegisterControl(nameof(CommonBillUserControl.CustomsEntryNumberTextBox));
			BillIssuerNameTextBox = RegisterControl(nameof(CommonBillUserControl.BillIssuerNameTextBox));
			BillIssuerCodeFindBox = RegisterControl(nameof(CommonBillUserControl.BillIssuerCodeFindBox));

			CustomsNumbersGroupBox = RegisterControl(nameof(CommonBillUserControl.CustomsNumbersGroupBox));
			AssociatedPacksGroupBox = RegisterControl(nameof(CommonBillUserControl.AssociatedPacksGroupBox));
			GoodsLocationCodeFindBox = RegisterControl(nameof(CommonBillUserControl.GoodsLocationCodeFindBox));
			SpecialCargoCodesDropEdit = RegisterControl(nameof(CommonBillUserControl.SpecialCargoCodesDropEdit));
			ContainerModeDropEdit = RegisterControl(nameof(CommonBillUserControl.ContainerModeDropEdit));
			SenderReferenceTextBox = RegisterControl(nameof(CommonBillUserControl.SenderReferenceTextBox));
		}

		public ControlReference IncotermDropEdit { get; }
		public ControlReference UCRNumberTextBox { get; }
		public ControlReference CargoTypeDropEdit { get; }
		public ControlReference CarrierReferenceTextBox { get; }
		public ControlReference RemarksTextBox { get; }
		public ControlReference NotifyPartyAddressControl { get; }
		public ControlReference ForwarderAddressControl { get; }
		public ControlReference ConsigneeAddressControl { get; }
		public ControlReference ShipperAddressControl { get; }
		public ControlReference DeliveryAgentAddressControl { get; }
		public ControlReference AgentAddressControl { get; }
		public ControlReference BuyerAddressControl { get; }
		public ControlReference GoodsLocationAddressControl { get; }
		public ControlReference CustomsValueConvertToLocalCurrencyControl { get; }
		public ControlReference InsuranceValueConvertToLocalCurrencyControl { get; }
		public ControlReference TransportValueConvertToLocalCurrencyControl { get; }
		public ControlReference FreightValueConvertToLocalCurrencyControl { get; }
		public ControlReference DiscountValueConvertToLocalCurrencyControl { get; }
		public ControlReference OtherChargesValueConvertToLocalCurrencyControl { get; }
		public ControlReference ManifestQtyCalcDropEdit { get; }
		public ControlReference ManifestQtyCalcEdit { get; }
		public ControlReference MarksAndNumbersTextBox { get; }
		public ControlReference GoodsDescriptionTextBox { get; }
		public ControlReference PrepaidCollectDropEdit { get; }
		public ControlReference FinalDestinationCodeFindBox { get; }
		public ControlReference DischargePortCodeFindBox { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference BillNumberTextBox { get; }
		public ControlReference VolumeCalcDropEdit { get; }
		public ControlReference OriginCodeFindBox { get; }
		public ControlReference MessageStatusTextBox { get; }
		public ControlReference RegistrationDateEdit { get; }
		public ControlReference ShipmentTypeDropEdit { get; }
		public ControlReference ProcedureCodeFindBox { get; }
		public ControlReference CusJobNumberCodeFindBox { get; }
		public ControlReference GoodsLocationDropEditWithFixedWidth { get; }
		public ControlReference BillStatusDropEdit { get; }
		public ControlReference CargoStatusDropEdit { get; }
		public ControlReference LocationInformationTextBox { get; }
		public ControlReference BillIssuerTextBox { get; }
		public ControlReference CustomsEntryNumberTextBox { get; }
		public ControlReference BillIssuerNameTextBox { get; }
		public ControlReference CustomsEntryNumberTypeDropEdit { get; }
		public ControlReference BillIssuerCodeFindBox { get; }
		public ControlReference CustomsNumbersGroupBox { get; }
		public ControlReference AssociatedPacksGroupBox { get; }
		public ControlReference GoodsLocationCodeFindBox { get; }
		public ControlReference DepartureDateEdit { get; }
		public ControlReference SpecialCargoCodesDropEdit { get; }
		public ControlReference ContainerModeDropEdit { get; }
		public ControlReference BillIssueDateEdit { get; }
		public ControlReference SenderReferenceTextBox { get; }

		protected override Control CreateTemplate() => new CommonBillUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CommonBillControlBag> commonBillControlBag = new Lazy<CommonBillControlBag>(() => new CommonBillControlBag());
	}
}
