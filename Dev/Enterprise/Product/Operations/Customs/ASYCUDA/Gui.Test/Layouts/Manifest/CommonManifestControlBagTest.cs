using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(CommonManifestControlBag))]
	sealed class CommonManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonManifestControlBag.CountryTextBox);
				yield return nameof(CommonManifestControlBag.JobReferenceTextBox);
				yield return nameof(CommonManifestControlBag.RegistrationDateEdit);
				yield return nameof(CommonManifestControlBag.RegistrationNumberTextBox);
				yield return nameof(CommonManifestControlBag.MessageStatusTextBox);
				yield return nameof(CommonManifestControlBag.MessageStatusDropEdit);
				yield return nameof(CommonManifestControlBag.CustomsStatusDropEdit);
				yield return nameof(CommonManifestControlBag.RegistrationDateTimeDateEdit);
				yield return nameof(CommonManifestControlBag.ConveyanceCountryCodeFindBox);
				yield return nameof(CommonManifestControlBag.AgentTypeDropEdit);
				yield return nameof(CommonManifestControlBag.VoyageFlightTextBox);
				yield return nameof(CommonManifestControlBag.MastersNameTextBox);
				yield return nameof(CommonManifestControlBag.VehicleRegistrationTextBox);
				yield return nameof(CommonManifestControlBag.ManifestNumberFromMasterBillTextBox);
				yield return nameof(CommonManifestControlBag.ManifestNumberTextBox);
				yield return nameof(CommonManifestControlBag.VesselCodeFindBox);
				yield return nameof(CommonManifestControlBag.VesselNameTextBox);
				yield return nameof(CommonManifestControlBag.IssueDateDateEdit);
				yield return nameof(CommonManifestControlBag.EstDepartureDateEdit);
				yield return nameof(CommonManifestControlBag.TransportModeDropEdit);
				yield return nameof(CommonManifestControlBag.ContainerModeDropEdit);
				yield return nameof(CommonManifestControlBag.PortOfLoadingCodeFindBox);
				yield return nameof(CommonManifestControlBag.PortOfDischargeCodeFindBox);
				yield return nameof(CommonManifestControlBag.BuyersConsolidationCheckBox);
				yield return nameof(CommonManifestControlBag.CarrierAddressControl);
				yield return nameof(CommonManifestControlBag.MasterBOLTextBox);
				yield return nameof(CommonManifestControlBag.RadioCallSignTextBox);
				yield return nameof(CommonManifestControlBag.Trailer1RegNoTextBox);
				yield return nameof(CommonManifestControlBag.Trailer2RegNoTextBox);
				yield return nameof(CommonManifestControlBag.Trailer1RegCountryCodeFindBox);
				yield return nameof(CommonManifestControlBag.Trailer2RegCountryCodeFindBox);
				yield return nameof(CommonManifestControlBag.DischargeTerminalAddressControl);
				yield return nameof(CommonManifestControlBag.DeconsolidateAddressControl);
				yield return nameof(CommonManifestControlBag.CustomsOfficeDropEdit);
				yield return nameof(CommonManifestControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(CommonManifestControlBag.ManifestTypeDropEdit);
				yield return nameof(CommonManifestControlBag.PortOfFirstArrivalCodeFindBox);
				yield return nameof(CommonManifestControlBag.ShippingAgentAddressControl);
				yield return nameof(CommonManifestControlBag.CarrierCodeTextBox);
				yield return nameof(CommonManifestControlBag.NatureDropEdit);
				yield return nameof(CommonManifestControlBag.EstArrivalDateEdit);
				yield return nameof(CommonManifestControlBag.ShortEstArrivalDateEdit);
				yield return nameof(CommonManifestControlBag.CustomsOriginPortCodeFindBox);
				yield return nameof(CommonManifestControlBag.CustomsLoadPortCodeFindBox);
				yield return nameof(CommonManifestControlBag.CustomsDischargePortCodeFindBox);
				yield return nameof(CommonManifestControlBag.LloydsNumberTextBox);
				yield return nameof(CommonManifestControlBag.DateAtCustomsOfficeDateEdit);
				yield return nameof(CommonManifestControlBag.GoodsLocationCodeFindBox);
				yield return nameof(CommonManifestControlBag.MasterBillTextBox);
				yield return nameof(CommonManifestControlBag.CarrierReferenceTextBox);
				yield return nameof(CommonManifestControlBag.PaymentMethodDropEdit);
				yield return nameof(CommonManifestControlBag.ETADateEdit);
				yield return nameof(CommonManifestControlBag.GuaranteeTextBox);
				yield return nameof(CommonManifestControlBag.ManifestTypeShortCodeLengthDropEdit);
				yield return nameof(CommonManifestControlBag.CustomsOfficeShortCodeLengthDropEdit);
				yield return nameof(CommonManifestControlBag.ActArrivalDateEdit);
				yield return nameof(CommonManifestControlBag.RecipientReferenceDropEdit);
				yield return nameof(CommonManifestControlBag.CustomsProfileDropEdit);
				yield return nameof(CommonManifestControlBag.BranchGuidFindBox);
				yield return nameof(CommonManifestControlBag.DeclarantAddressControl);
				yield return nameof(CommonManifestControlBag.RepresentativeAddressControl);
				yield return nameof(CommonManifestControlBag.SpecialCargoCodeDropEdit);
				yield return nameof(CommonManifestControlBag.OriginCodeFindBox);
				yield return nameof(CommonManifestControlBag.DestinationCodeFindBox);
				yield return nameof(CommonManifestControlBag.GoodsDescriptionTextBox);
				yield return nameof(CommonManifestControlBag.RegistrationYearEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonManifestControlBag.Instance;
	}
}
