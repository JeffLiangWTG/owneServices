using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageLayout))]
	sealed class UCC6TemporaryStorageLayoutTest : LayoutsAbstractTest
	{
		public void TestDefaultVisibilities_Transfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageLayout()).Layout;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;

			CombineAssertions(() =>
			{
				AssertEquals("IsENSReuseCheckBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.IsENSReuseCheckBox, header));
				AssertEquals("TransportModeDropEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.TransportModeDropEdit, header));
				AssertEquals("TransportTypeDropEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.TransportTypeDropEdit, header));
				AssertEquals("ArrivalTransportMeansCodeTextBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header));
				AssertEquals("PresentationCustomsOfficeCodeFindBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.PresentationCustomsOfficeCodeFindBox, header));
				AssertEquals("GoodsPresentationDateEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.GoodsPresentationDateEdit, header));
				AssertEquals("EstimatedDateOfArrivalDateEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.EstimatedDateOfArrivalDateEdit, header));
				AssertEquals("CarrierAddressControl", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.CarrierAddressControl, header));
				AssertEquals("PlaceOfUnloadingCodeFindBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.PlaceOfUnloadingCodeFindBox, header));
			});
		}

		public void TestDefaultVisibilities_Deconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageLayout()).Layout;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;

			CombineAssertions(() =>
			{
				AssertEquals("IsENSReuseCheckBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.IsENSReuseCheckBox, header));
				AssertEquals("TransportModeDropEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.TransportModeDropEdit, header));
				AssertEquals("TransportTypeDropEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.TransportTypeDropEdit, header));
				AssertEquals("ArrivalTransportMeansCodeTextBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header));
				AssertEquals("PresentationCustomsOfficeCodeFindBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.PresentationCustomsOfficeCodeFindBox, header));
				AssertEquals("GoodsPresentationDateEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.GoodsPresentationDateEdit, header));
				AssertEquals("EstimatedDateOfArrivalDateEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.EstimatedDateOfArrivalDateEdit, header));
				AssertEquals("PersonPresentingTheGoodsAddressControl", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.PersonPresentingTheGoodsAddressControl, header));
				AssertEquals("LocationOfGoodsUserControl", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.LocationOfGoodsUserControl, header));
				AssertEquals("AuthorizationTypeDropEdit", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.AuthorizationTypeDropEdit, header));
				AssertEquals("AuthorizationOwnerGuidFindBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.AuthorizationOwnerGuidFindBox, header));
				AssertEquals("AuthorizationNumberCodeFindBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.AuthorizationNumberCodeFindBox, header));
				AssertEquals("CarrierAddressControl", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.CarrierAddressControl, header));
				AssertEquals("PlaceOfUnloadingCodeFindBox", false, layout.IsVisible(TemporaryStorageUserControlBag.Instance.PlaceOfUnloadingCodeFindBox, header));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TemporaryStorageUserControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Medium);
				yield return (TemporaryStorageUserControlBag.Instance.DeclarationDateDateEdit, ControlWidthClass.Medium);
				yield return (TemporaryStorageUserControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.IsENSReuseCheckBox, ControlWidthClass.Auto);
				yield return (TemporaryStorageUserControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.TransportTypeDropEdit, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
				yield return (TemporaryStorageUserControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.PresentationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.GoodsPresentationDateEdit, ControlWidthClass.Medium);
				yield return (TemporaryStorageUserControlBag.Instance.EstimatedDateOfArrivalDateEdit, ControlWidthClass.Medium);
				yield return (TemporaryStorageUserControlBag.Instance.PersonPresentingTheGoodsAddressControl, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.PlaceOfUnloadingCodeFindBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.AuthorizationTypeDropEdit, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.HasHouseConsignmentCheckBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.HasNoMasterBillCheckBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TemporaryStorageUserControlBag.Instance.LRNTextBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.CustomsStatusDateEdit, ControlWidthClass.Medium);
				yield return (TemporaryStorageUserControlBag.Instance.MRNTextBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.CRNTextBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.FRNTextBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.CusAgentCodeFindBox, ControlWidthClass.Long);
				yield return (TemporaryStorageUserControlBag.Instance.GuaranteeGroupBox, ControlWidthClass.Auto);
				yield return (TemporaryStorageUserControlBag.Instance.DocumentsTabControl, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();
	}
}
