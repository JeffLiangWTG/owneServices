using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class TemporaryStorageControlTest : TestCaseWithFactory
	{
		public void TestControlsTypeBinding()
		{
			using (var control = new TemporaryStorageUserControl())
			{
				AssertControlsTypeBinding(control.MessageTypeDropEdit, nameof(TemporaryStorageHeader.AMA_MessageType));
				AssertControlsTypeBinding(control.LRNTextBox, nameof(TemporaryStorageHeader.LRN));
				AssertControlsTypeBinding(control.DeclarantAddressControl, nameof(TemporaryStorageHeader.AMA_OA_Declarant));
				AssertControlsTypeBinding(control.RepresentativeAddressControl, nameof(TemporaryStorageHeader.AMA_OA_Representative));
				AssertControlsTypeBinding(control.CarrierAddressControl, nameof(TemporaryStorageHeader.AMA_OA_Carrier));
				AssertControlsTypeBinding(control.PersonPresentingTheGoodsAddressControl, nameof(TemporaryStorageHeader.AMA_OA_Presenter));
				AssertControlsTypeBinding(control.SupervisingCustomsOfficeCodeFindBox, nameof(TemporaryStorageHeader.AMA_CustomsOffice));
				AssertControlsTypeBinding(control.PresentationCustomsOfficeCodeFindBox, nameof(TemporaryStorageHeader.PresentationCustomsOffice));
				AssertControlsTypeBinding(control.AuthorizationOwnerGuidFindBox, nameof(TemporaryStorageHeader.AuthorizationOwner));
				AssertControlsTypeBinding(control.AuthorizationNumberCodeFindBox, nameof(TemporaryStorageHeader.AuthorizationNumber));
				AssertControlsTypeBinding(control.HasHouseConsignmentCheckBox, nameof(TemporaryStorageHeader.AMA_Calc_HasHouseConsignment));
				AssertControlsTypeBinding(control.MRNTextBox, nameof(TemporaryStorageHeader.MRN));
				AssertControlsTypeBinding(control.CRNTextBox, nameof(TemporaryStorageHeader.CRN));
				AssertControlsTypeBinding(control.FRNTextBox, nameof(TemporaryStorageHeader.FRN));
				AssertControlsTypeBinding(control.IsENSReuseCheckBox, nameof(TemporaryStorageHeader.IsENSReuse));
				AssertControlsTypeBinding(control.TransportTypeDropEdit, nameof(TemporaryStorageHeader.TransportType));
				AssertControlsTypeBinding(control.ArrivalTransportMeansCodeTextBox, nameof(TemporaryStorageHeader.ArrivalTransportMeansCode));
				AssertControlsTypeBinding(control.CustomsStatusDropEdit, nameof(TemporaryStorageHeader.CustomsStatus));
				AssertControlsTypeBinding(control.AuthorizationTypeDropEdit, nameof(TemporaryStorageHeader.AuthorizationType));
				AssertControlsTypeBinding(control.MessageStatusDropEdit, nameof(TemporaryStorageHeader.AMA_MessageStatus));
				AssertControlsTypeBinding(control.DeclarationDateDateEdit, nameof(TemporaryStorageHeader.DeclarationDate));
				AssertControlsTypeBinding(control.GoodsPresentationDateEdit, nameof(TemporaryStorageHeader.AMA_DateAtCustomsOffice));
				AssertControlsTypeBinding(control.EstimatedDateOfArrivalDateEdit, nameof(TemporaryStorageHeader.EstimatedDateOfArrival));
				AssertControlsTypeBinding(control.TransportModeDropEdit, nameof(TemporaryStorageHeader.AMA_TransportMode));
				AssertControlsTypeBinding(control.PlaceOfUnloadingCodeFindBox, nameof(TemporaryStorageHeader.PlaceOfUnloading));
				AssertControlsTypeBinding(control.PlaceOfLoadingCodeFindBox, nameof(TemporaryStorageHeader.PlaceOfLoading));
				AssertControlsTypeBinding(control.CusAgentCodeFindBox, nameof(TemporaryStorageHeader.AMA_GS_NKCustomsAgent));
				AssertControlsTypeBinding(control.CustomsStatusDateEdit, nameof(TemporaryStorageHeader.CustomsStatusDate));
			}

			void AssertControlsTypeBinding(IBindTo control, string bindTo)
			{
				AssertEquals(control.BindTo, bindTo);
			}
		}

		public void TestGuaranteeGroupBoxDynamicLayoutPanel()
		{
			var tempStorageRegHeader = Factory.New<TemporaryStorageHeader>();
			using (var userControl = new TemporaryStorageUserControl())
			{
				userControl.SetDataBinding(tempStorageRegHeader, "");
				var dynamicGuaranteePanel = userControl.DynamicGuaranteePanel;
				CombineAssertions(() =>
				{
					AssertEquals("DynamicGuaranteePanel Dock", DockStyle.Fill, dynamicGuaranteePanel.Dock);
					AssertEquals("Binding", nameof(tempStorageRegHeader.Guarantee), dynamicGuaranteePanel.GetBindingMember());
					AssertEquals("Within GuaranteeGroupBox", true, userControl.GuaranteeGroupBox.Controls.Contains(dynamicGuaranteePanel));

					DynamicLayoutPanelTest.AssertControlsOrder(dynamicGuaranteePanel,
					nameof(GuaranteeGroupBoxControlBag.BondNumberCodeFindBox),
					nameof(GuaranteeGroupBoxControlBag.AmountCalcDropEdit),
					nameof(GuaranteeGroupBoxControlBag.OverrideCheckBox));
				});
			}
		}

		public void TestGuaranteeGroupBox()
		{
			using (var userControl = new TemporaryStorageUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Guarantee", userControl.GuaranteeGroupBox.CaptionResourceString.Caption);
					AssertEquals("Visibility for ES", true, userControl.GuaranteeGroupBox.Visible);
				});
			}
		}

		public void TestControls()
		{
			using (var control = new TemporaryStorageUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("CountryCodeFindBox", control.FindSingle<ZCodeFindBox>("CountryCodeFindBox"));
					AssertNotNull("DeclarationDateDateEdit", control.FindSingle<ZDateEdit>("DeclarationDateDateEdit"));
					AssertNotNull("MessageTypeDropEdit", control.FindSingle<ZDropEdit>("MessageTypeDropEdit"));
					AssertNotNull("IsENSReuseCheckBox", control.FindSingle<ZCheckBox>("IsENSReuseCheckBox"));
					AssertNotNull("TransportModeDropEdit", control.FindSingle<ZDropEdit>("TransportModeDropEdit"));
					AssertNotNull("TransportTypeDropEdit", control.FindSingle<ZDropEdit>("TransportTypeDropEdit"));
					AssertNotNull("ArrivalTransportMeansCodeTextBox", control.FindSingle<ZTextBox>("ArrivalTransportMeansCodeTextBox"));
					AssertNotNull("DeclarantAddressControl", control.FindSingle<ZAddressControl>("DeclarantAddressControl"));
					AssertNotNull("RepresentativeAddressControl", control.FindSingle<ZAddressControl>("RepresentativeAddressControl"));
					AssertNotNull("SupervisingCustomsOfficeDropEdit", control.FindSingle<ZCodeFindBox>("SupervisingCustomsOfficeCodeFindBox"));
					AssertNotNull("PresentationCustomsOfficeTextBox", control.FindSingle<ZCodeFindBox>("PresentationCustomsOfficeCodeFindBox"));
					AssertNotNull("GoodsPresentationDateEdit", control.FindSingle<ZDateEdit>("GoodsPresentationDateEdit"));
					AssertNotNull("EstimatedDateOfArrivalDateEdit", control.FindSingle<ZDateEdit>("EstimatedDateOfArrivalDateEdit"));
					AssertNotNull("PersonPresentingTheGoodsAddressControl", control.FindSingle<ZAddressControl>("PersonPresentingTheGoodsAddressControl"));
					AssertNotNull("CarrierAddressControl", control.FindSingle<ZAddressControl>("CarrierAddressControl"));
					AssertNotNull("PlaceOfUnloadingCodeFindBox", control.FindSingle<ZCodeFindBox>("PlaceOfUnloadingCodeFindBox"));
					AssertNotNull("PlaceOfLoadingCodeFindBox", control.FindSingle<ZCodeFindBox>("PlaceOfLoadingCodeFindBox"));
					AssertNotNull("AuthorizationOwnerGuidFindBox", control.FindSingle<ZGuidFindBox>("AuthorizationOwnerGuidFindBox"));
					AssertNotNull("AuthorizationNumberCodeFindBox", control.FindSingle<ZCodeFindBox>("AuthorizationNumberCodeFindBox"));
					AssertNotNull("AuthorizationTypeDropEdit", control.FindSingle<ZDropEdit>("AuthorizationTypeDropEdit"));
					AssertNotNull("LocationOfGoodsUserControl", control.FindSingle<ZUserControl>("LocationOfGoodsUserControl"));
					AssertNotNull("LocationOfGoodsUserControl", control.FindSingle<ZCheckBox>("HasHouseConsignmentCheckBox"));

					AssertNotNull("LRNTextBox", control.FindSingle<ZTextBox>("LRNTextBox"));
					AssertNotNull("MessageStatusTextBox", control.FindSingle<ZDropEdit>("MessageStatusDropEdit"));
					AssertNotNull("CustomsStatusTextBox", control.FindSingle<ZDropEdit>("CustomsStatusDropEdit"));
					AssertNotNull("CustomsStatusDateEdit", control.FindSingle<ZDateEdit>("CustomsStatusDateEdit"));
					AssertNotNull("MRNTextBox", control.FindSingle<ZTextBox>("MRNTextBox"));
					AssertNotNull("CRNTextBox", control.FindSingle<ZTextBox>("CRNTextBox"));
					AssertNotNull("FRNTextBox", control.FindSingle<ZTextBox>("FRNTextBox"));
					AssertNotNull("CusAgentCodeFindBox", control.FindSingle<ZCodeFindBox>("CusAgentCodeFindBox"));
					AssertNotNull("PreviousDocumentsLayoutPanel", control.FindSingle<DynamicLayoutPanel>("PreviousDocumentsLayoutPanel"));
					AssertNotNull("PreviousDocumentUserControlTabPage", control.FindSingle<ZTabPage>("PreviousDocumentUserControlTabPage"));
					AssertNotNull("DocumentsTabControl", control.FindSingle<ZTemplateTabControl>("DocumentsTabControl"));
				});
			}
		}
	}
}
