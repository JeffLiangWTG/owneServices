using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageDetailsLayout))]
	sealed class G5V1TemporaryStorageDetailsLayoutTest : LayoutsAbstractTest
	{
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
				yield return (ESControlBagInstance.LAMEEntryNumberTextBox, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.LRNTextBox, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.MRNTextBox, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.CustomsStatusDropEdit, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.MessageStatusDropEdit, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.CircuitTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ESControlBagInstance.LAMEEntryDateDateEdit, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.AcceptanceDateDateEdit, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.ClearanceNumberTextBox, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.DsdtSdFormatHasUrlUserControl, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.DsdtSdFormatNoUrlUserControl, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.DsdtMrnBindingMemberUserControl, ControlWidthClass.Auto);
				yield return (ESControlBagInstance.DsdtMrnNumberTextBox, ControlWidthClass.Auto);
			}
		}

		public void TestIsMessageTypeTSMVisibility()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("MessageStatusDropEdit when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.MessageStatusDropEdit, header));
				AssertEquals("CircuitTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.CircuitTextBox, header));
				AssertEquals("ClearanceNumberTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.ClearanceNumberTextBox, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtSdFormatHasUrlUserControl when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatHasUrlUserControl, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				AssertEquals("MessageStatusDropEdit when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(ESControlBagInstance.MessageStatusDropEdit, header));
				AssertEquals("CircuitTextBox when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(ESControlBagInstance.CircuitTextBox, header));
				AssertEquals("ClearanceNumberTextBox when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(ESControlBagInstance.ClearanceNumberTextBox, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl when AMA_MessageType = TSM", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtSdFormatHasUrlUserControl when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatHasUrlUserControl, header));
			});
		}

		public void TestDsdtSdFormatUserControlVisibility()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
			entryNumber.CE_EntryNum = "12344321123";
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("DsdtSdFormatNoUrlUserControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtSdFormatHasUrlUserControl when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatHasUrlUserControl, header));

				entryNumber.CE_ParentID = header.PK;
				entryNumber.CE_ParentTable = header.TableName;
				Factory.Save();
				AssertEquals("DsdtSdFormatNoUrlUserControl when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtSdFormatHasUrlUserControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatHasUrlUserControl, header));
			});
		}

		public void TestVisibilityBetweenDsdtControls()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
			entryNumber.CE_EntryNum = "12344321123";
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var layout = ((IPanelLayoutProvider)new G5V1TemporaryStorageDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				header.CustomsStatus = "AAA";
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("DsdtMrnNumberTextBox visibility retruns true", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
				AssertEquals("DsdtMrnBindingMemberUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnBindingMemberUserControl, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.DsdtMrnNumberTextBox, header, out var resourceStringData);
				AssertEquals("DsdtMrnNumberTextBox Caption should be DSDT MRN", "DSDT MRN", resourceStringData.Caption);

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.DsdtMrnNumberTextBox, header, out resourceStringData);
				AssertEquals("DsdtMrnNumberTextBox visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
				AssertEquals("DsdtMrnBindingMemberUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnBindingMemberUserControl, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox Caption should be DSDT (SD Format)", "DSDT (SD Format)", resourceStringData.Caption);

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.DsdtMrnNumberTextBox, header, out resourceStringData);
				AssertEquals("DsdtMrnNumberTextBox visibility returns true", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
				AssertEquals("DsdtMrnBindingMemberUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnBindingMemberUserControl, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl visibility returns true", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox Caption should be DSDT MRN", "DSDT MRN", resourceStringData.Caption);

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.DsdtMrnNumberTextBox, header, out resourceStringData);
				AssertEquals("DsdtMrnNumberTextBox visibility returns true", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
				AssertEquals("DsdtMrnBindingMemberUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnBindingMemberUserControl, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox Caption should be DSDT (SD Format)", "DSDT (SD Format)", resourceStringData.Caption);

				header.CustomsStatus = ZString.Empty;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.DsdtMrnNumberTextBox, header, out resourceStringData);
				AssertEquals("DsdtMrnNumberTextBox visibility returns true", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
				AssertEquals("DsdtMrnBindingMemberUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnBindingMemberUserControl, header));
				AssertEquals("DsdtSdFormatNoUrlUserControl visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox Caption should be DSDT (SD Format)", "DSDT (SD Format)", resourceStringData.Caption);

				entryNumber.CE_ParentID = header.PK;
				entryNumber.CE_ParentTable = header.TableName;
				Factory.Save();
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.DsdtMrnNumberTextBox, header, out resourceStringData);
				AssertEquals("DsdtMrnNumberTextBox visibility returns false", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
				AssertEquals("DsdtMrnBindingMemberUserControl visibility returns true", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnBindingMemberUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox Caption should be DSDT MRN", "DSDT MRN", resourceStringData.Caption);
			});
		}

		public void TestIsMessageTypeLAMVisibility()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("LRNTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.LRNTextBox, header));
				AssertEquals("MRNTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.MRNTextBox, header));
				AssertEquals("MessageStatusDropEdit when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.MessageStatusDropEdit, header));
				AssertEquals("CircuitTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.CircuitTextBox, header));
				AssertEquals("AcceptanceDateDateEdit when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.AcceptanceDateDateEdit, header));
				AssertEquals("ClearanceNumberTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.ClearanceNumberTextBox, header));
				AssertEquals("DsdtSdFormatUserControl when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox when AMA_MessageType = G5X", true, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				AssertEquals("LRNTextBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.LRNTextBox, header));
				AssertEquals("MRNTextBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.MRNTextBox, header));
				AssertEquals("MessageStatusDropEdit when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.MessageStatusDropEdit, header));
				AssertEquals("CircuitTextBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.CircuitTextBox, header));
				AssertEquals("AcceptanceDateDateEdit when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.AcceptanceDateDateEdit, header));
				AssertEquals("ClearanceNumberTextBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.ClearanceNumberTextBox, header));
				AssertEquals("DsdtSdFormatUserControl when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtSdFormatNoUrlUserControl, header));
				AssertEquals("DsdtMrnNumberTextBox when AMA_MessageType = LAM", false, LayoutForTesting.IsVisible(ESControlBagInstance.DsdtMrnNumberTextBox, header));
			});
		}

		public void TestIsMessageTypeLAMVisibilityNewFields()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
				AssertEquals("LAMEEntryNumberTextBox when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.LAMEEntryNumberTextBox, header));
				AssertEquals("LAMEEntryDateDateEdit when AMA_MessageType = G5X", false, LayoutForTesting.IsVisible(ESControlBagInstance.LAMEEntryDateDateEdit, header));

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				AssertEquals("LAMEEntryNumberTextBox when AMA_MessageType = LAM", true, LayoutForTesting.IsVisible(ESControlBagInstance.LAMEEntryNumberTextBox, header));
				AssertEquals("LAMEEntryDateDateEdit when AMA_MessageType = LAM", true, LayoutForTesting.IsVisible(ESControlBagInstance.LAMEEntryDateDateEdit, header));
			});
		}

		public void TestGetMRNAndAcceptanceDateCaption()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new G5V1TemporaryStorageDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.MRNTextBox, header, out var mrnTextBoxResourceStringData);
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.AcceptanceDateDateEdit, header, out var acceptanceDateResourceStringData);
				AssertEquals("MRNTextBox: AMA_MessageType not TSM and Union Goods is false", "MRN", mrnTextBoxResourceStringData.Caption);
				AssertEquals("AcceptanceDateDateEdit: AMA_MessageType not TSM and Union Goods is false", "Acceptance Date", acceptanceDateResourceStringData.Caption);

				header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				header.UnionGoods = true;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.MRNTextBox, header, out mrnTextBoxResourceStringData);
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.AcceptanceDateDateEdit, header, out acceptanceDateResourceStringData);
				AssertEquals("MRNTextBox: AMA_MessageType TSM and Union Goods is true", "Entry Reference", mrnTextBoxResourceStringData.Caption);
				AssertEquals("AcceptanceDateDateEdit: AMA_MessageType not TSM and Union Goods is true", "Entry Date", acceptanceDateResourceStringData.Caption);

				header.UnionGoods = false;
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.MRNTextBox, header, out mrnTextBoxResourceStringData);
				layout.TryGetCaption(G5V1TemporaryStorageDetailsUserControlBag.Instance.AcceptanceDateDateEdit, header, out acceptanceDateResourceStringData);
				AssertEquals("MRNTextBox: AMA_MessageType TSM and Union Goods is false", "MRN", mrnTextBoxResourceStringData.Caption);
				AssertEquals("AcceptanceDateDateEdit: AMA_MessageType not TSM and Union Goods is true", "Acceptance Date", acceptanceDateResourceStringData.Caption);
			});
		}

		G5V1TemporaryStorageDetailsUserControlBag ESControlBagInstance => G5V1TemporaryStorageDetailsUserControlBag.Instance;

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageDetailsLayoutBuilder<EU.Business.CusTempStorage.TemporaryStorageHeader>();
	}
}
