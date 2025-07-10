using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid))]
	sealed class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		public void TestDefaultVisibilities_Transfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			var previousDocument = header.PreviousDocuments.AddNew();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid()).Layout;

			AssertEquals("GoodItemIdentifierCalcEdit", false, layout.IsVisible(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance.GoodItemIdentifierCalcEdit, previousDocument));
		}

		public void TestDefaultVisibilities_Deconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			var previousDocument = header.PreviousDocuments.AddNew();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid()).Layout;

			AssertEquals("GoodItemIdentifierCalcEdit", false, layout.IsVisible(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance.GoodItemIdentifierCalcEdit, previousDocument));
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.TypeCodeFindBox, ControlWidthClass.Long),
					(euBag.ReferenceNumberTextBox, ControlWidthClass.Long),
					(euBag.GoodItemIdentifierCalcEdit, ControlWidthClass.Long),
				};
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid);

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>();
	}
}
