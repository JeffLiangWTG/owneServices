using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(CommonBillControlBag))]
	sealed class CommonBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonBillControlBag.BillNumberTextBox);
				yield return nameof(CommonBillControlBag.OriginCodeFindBox);
				yield return nameof(CommonBillControlBag.CargoTypeDropEdit);
				yield return nameof(CommonBillControlBag.FinalDestinationCodeFindBox);
				yield return nameof(CommonBillControlBag.ProcedureCodeFindBox);
				yield return nameof(CommonBillControlBag.ManifestQtyCalcDropEdit);
				yield return nameof(CommonBillControlBag.ManifestQtyCalcEdit);
				yield return nameof(CommonBillControlBag.GrossWeightCalcDropEdit);
				yield return nameof(CommonBillControlBag.NetWeightCalcDropEdit);
				yield return nameof(CommonBillControlBag.VolumeCalcDropEdit);
				yield return nameof(CommonBillControlBag.BillIssueDateEdit);
				yield return nameof(CommonBillControlBag.IncotermDropEdit);
				yield return nameof(CommonBillControlBag.GoodsDescriptionTextBox);
				yield return nameof(CommonBillControlBag.MarksAndNumbersTextBox);
				yield return nameof(CommonBillControlBag.ShipperAddressControl);
				yield return nameof(CommonBillControlBag.ConsigneeAddressControl);
				yield return nameof(CommonBillControlBag.NotifyPartyAddressControl);
				yield return nameof(CommonBillControlBag.ForwarderAddressControl);
				yield return nameof(CommonBillControlBag.DeliveryAgentAddressControl);
				yield return nameof(CommonBillControlBag.AgentAddressControl);
				yield return nameof(CommonBillControlBag.BuyerAddressControl);
				yield return nameof(CommonBillControlBag.GoodsLocationAddressControl);
				yield return nameof(CommonBillControlBag.CarrierReferenceTextBox);
				yield return nameof(CommonBillControlBag.PrepaidCollectDropEdit);
				yield return nameof(CommonBillControlBag.RemarksTextBox);
				yield return nameof(CommonBillControlBag.UCRNumberTextBox);
				yield return nameof(CommonBillControlBag.TransportValueConvertToLocalCurrencyControl);
				yield return nameof(CommonBillControlBag.FreightValueConvertToLocalCurrencyControl);
				yield return nameof(CommonBillControlBag.InsuranceValueConvertToLocalCurrencyControl);
				yield return nameof(CommonBillControlBag.CustomsValueConvertToLocalCurrencyControl);
				yield return nameof(CommonBillControlBag.DiscountValueConvertToLocalCurrencyControl);
				yield return nameof(CommonBillControlBag.OtherChargesValueConvertToLocalCurrencyControl);
				yield return nameof(CommonBillControlBag.CustomsEntryNumberTypeDropEdit);
				yield return nameof(CommonBillControlBag.MessageStatusTextBox);
				yield return nameof(CommonBillControlBag.RegistrationDateEdit);
				yield return nameof(CommonBillControlBag.ShipmentTypeDropEdit);
				yield return nameof(CommonBillControlBag.CusJobNumberCodeFindBox);
				yield return nameof(CommonBillControlBag.GoodsLocationDropEditWithFixedWidth);
				yield return nameof(CommonBillControlBag.BillStatusDropEdit);
				yield return nameof(CommonBillControlBag.CargoStatusDropEdit);
				yield return nameof(CommonBillControlBag.LocationInformationTextBox);
				yield return nameof(CommonBillControlBag.BillIssuerTextBox);
				yield return nameof(CommonBillControlBag.CustomsEntryNumberTextBox);
				yield return nameof(CommonBillControlBag.BillIssuerNameTextBox);
				yield return nameof(CommonBillControlBag.BillIssuerCodeFindBox);
				yield return nameof(CommonBillControlBag.CustomsNumbersGroupBox);
				yield return nameof(CommonBillControlBag.AssociatedPacksGroupBox);
				yield return nameof(CommonBillControlBag.GoodsLocationCodeFindBox);
				yield return nameof(CommonBillControlBag.DepartureDateEdit);
				yield return nameof(CommonBillControlBag.SpecialCargoCodesDropEdit);
				yield return nameof(CommonBillControlBag.ContainerModeDropEdit);
				yield return nameof(CommonBillControlBag.DischargePortCodeFindBox);
				yield return nameof(CommonBillControlBag.SenderReferenceTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonBillControlBag.Instance;
	}
}
