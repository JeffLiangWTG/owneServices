using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7BillLayouts))]
	sealed class EUH7BillLayoutsTest : LayoutsAbstractTest
	{
		[RequiresSTA]
		public void TestMessagesTabOnBillLevel()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();

				var billsAndPacksTabControl = form.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var messagesTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>("billsAndPacksTabControl_TabPage_EUH7MessagesUserControl");
				billsAndPacksTabControl.SelectedTab = messagesTabPage;

				var messagesUserControl = messagesTabPage.FindSingle<EUH7MessagesUserControl>();
				AssertNotNull(messagesUserControl);
			}
		}

		[RequiresSTA]
		public void TestCaptions()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using var form = new AsycudaBillForm(bill);
			form.Show();

			CombineAssertions(() =>
			{
				AssertCaption(CommonBillControlBag.Instance.ShipperAddressControl, "Shipper", "Shipper", "Shipper", "The party shipping the goods.");
				AssertCaption(CommonBillControlBag.Instance.ConsigneeAddressControl, "Consignee", "CNE", "Consignee", "The party to whom the goods are consigned.");
				AssertCaption(CommonBillControlBag.Instance.NotifyPartyAddressControl, "Notify Party", "NP", "Notify Party", "The party to be notified at entry of the arrival of the goods, as stipulated in the master bill of lading or master air waybill.");
			});

			void AssertCaption(ControlReference controlReference, string expectedCaption, string expectedShortCaption, string expectedMediumCaption, string expectedFullDescription)
			{
				LayoutForTesting.TryGetCaption(controlReference, null, out var captionData);
				AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
				AssertEquals($"ShortCaption for {controlReference.ControlName}", expectedShortCaption, captionData?.ShortCaption);
				AssertEquals($"MediumCaption for {controlReference.ControlName}", expectedMediumCaption, captionData?.MediumCaption);
				AssertEquals($"FullDescription for {controlReference.ControlName}", expectedFullDescription, captionData?.FullDescription);
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.MovementReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EUH7BillControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.ContainerUserControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CarrierReferenceTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.StandAloneDeclarationUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			}
		}
	}
}
