using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	sealed class G5V1TemporaryStorageUserControlTest : TestCaseWithFactory
	{
		public void TestDocumentsTabControl()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("DeclarationDetailsGroupBox", control.FindSingle<ZGroupBox>("DeclarationDetailsGroupBox"));
				AssertNotNull("DynamicDetailsUserControlLayoutPanel", control.FindSingle<DynamicLayoutPanel>("DynamicDetailsUserControlLayoutPanel"));
				AssertNotNull("DocumentsTabControl", control.FindSingle<ZTemplateTabControl>("DocumentsTabControl"));
				AssertNotNull("PreviousDocumentsLayoutPanel", control.FindSingle<DynamicLayoutPanel>("PreviousDocumentsLayoutPanel"));
				AssertNotNull("PreviousDocumentUserControlTabPage", control.FindSingle<ZTabPage>("PreviousDocumentUserControlTabPage"));
				AssertNotNull("SupportingDocumentsLayoutPanel", control.FindSingle<DynamicLayoutPanel>("SupportingDocumentsLayoutPanel"));
				AssertNotNull("SupportingDocumentUserControlTabPage", control.FindSingle<ZTabPage>("SupportingDocumentUserControlTabPage"));
				AssertNotNull("AdditionalInfosLayoutPanel", control.FindSingle<DynamicLayoutPanel>("AdditionalInfosLayoutPanel"));
				AssertNotNull("AdditionalInfoUserControlTabPage", control.FindSingle<ZTabPage>("AdditionalInfoUserControlTabPage"));
			});
		}

		public void TestPreviousDocumentsControls()
		{
			var docsTabControl = control.FindSingle<ZTemplateTabControl>("DocumentsTabControl");
			var previousDocumentUserControlTabPage = docsTabControl.FindSingle<ZTabPage>("PreviousDocumentUserControlTabPage");
			var previousDocumentsLayoutPanel = previousDocumentUserControlTabPage.FindSingle<DynamicLayoutPanel>("PreviousDocumentsLayoutPanel");
			CombineAssertions(() =>
			{
				AssertEquals("PreviousDocumentUserControlTabPage caption", "Previous Docs", previousDocumentUserControlTabPage.CaptionResourceString.Caption);
				AssertNotNull("PreviousDocumentUserControlTabPage should contain PreviousDocumentsLayoutPanel", previousDocumentsLayoutPanel);

				var ucc6TemporaryStoragePreviousDocumentsUserControlWithGrid = previousDocumentsLayoutPanel.FindSingle<UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid>("UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid");
				AssertNotNull("PreviousDocumentsLayoutPanel should contain UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid", ucc6TemporaryStoragePreviousDocumentsUserControlWithGrid);
			});
		}

		public void TestSupportingDocumentsControls()
		{
			var docsTabControl = control.FindSingle<ZTemplateTabControl>("DocumentsTabControl");
			var supportingDocumentUserControlTabPage = docsTabControl.FindSingle<ZTabPage>("SupportingDocumentUserControlTabPage");
			var supportingDocumentsLayoutPanel = supportingDocumentUserControlTabPage.FindSingle<DynamicLayoutPanel>("SupportingDocumentsLayoutPanel");
			CombineAssertions(() =>
			{
				AssertEquals("SupportingDocumentUserControlTabPage caption", "Supporting Documents", supportingDocumentUserControlTabPage.CaptionResourceString.Caption);
				AssertNotNull("SupportingDocumentUserControlTabPage should contain SupportingDocumentsLayoutPanel", supportingDocumentsLayoutPanel);

				var ucc6TemporaryStorageSupportingDocumentsUserControlWithGrid = supportingDocumentsLayoutPanel.FindSingle<UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid>("UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid");
				AssertNotNull("SupportingDocumentsLayoutPanel should contain UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid", ucc6TemporaryStorageSupportingDocumentsUserControlWithGrid);
			});
		}

		public void TestAdditionalInfosControls()
		{
			var docsTabControl = control.FindSingle<ZTemplateTabControl>("DocumentsTabControl");
			var additionalInfoUserControlTabPage = docsTabControl.FindSingle<ZTabPage>("AdditionalInfoUserControlTabPage");
			var additionalInfosLayoutPanel = additionalInfoUserControlTabPage.FindSingle<DynamicLayoutPanel>("AdditionalInfosLayoutPanel");
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInfoUserControlTabPage caption", "Additional Information", additionalInfoUserControlTabPage.CaptionResourceString.Caption);
				AssertNotNull("AdditionalInfoUserControlTabPage should contain AdditionalInfosLayoutPanel", additionalInfosLayoutPanel);

				var ucc6TemporaryStorageAdditionalInfosUserControlWithGrid = additionalInfosLayoutPanel.FindSingle<UCC6TemporaryStorageAdditionalInfosUserControlWithGrid>("UCC6TemporaryStorageAdditionalInfosUserControlWithGrid");
				AssertNotNull("AdditionalInfosLayoutPanel should contain UCC6TemporaryStorageAdditionalInfosUserControlWithGrid", ucc6TemporaryStorageAdditionalInfosUserControlWithGrid);
			});
		}

		public void TestDestinationCustomsOfficeCodeFindBox()
		{
			var destinationCustomsOfficeCodeFindBox = control.DestinationCustomsOfficeCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", destinationCustomsOfficeCodeFindBox);
				destinationCustomsOfficeCodeFindBox.AssertThisControl(x => x.WithBindTo("DestinationCustomsOffice"));
			});
		}

		public void TestDestinationLocationOfGoodsUserControl()
		{
			var locationOfGoodsUserControl = control.DestinationLocationOfGoodsUserControl;
			CombineAssertions(() =>
			{
				AssertType<DestinationLocationOfGoodsUserControl>("Type", locationOfGoodsUserControl);
				locationOfGoodsUserControl.AssertThisControl(x => x.WithBindTo("."));
			});
		}

		public void TestCertificateDropEdit()
		{
			var certificateDropEdit = control.CertificateDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", certificateDropEdit);
				certificateDropEdit.AssertThisControl(x => x.WithBindTo("AMA_CustomsProfile"));
			});
		}

		public void TestTrainingCheckBox()
		{
			var trainingCheckBox = control.TrainingCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", trainingCheckBox);
				trainingCheckBox.AssertThisControl(x => x.WithCaption("Training Entry")
														 .WithMediumCaption("Training Entry")
														 .WithShortCaption("Training Entry")
														 .WithFullDescription("When checked the declaration will be sent to Test")
														 .WithBindTo("TrainingEntry"));
			});
		}

		public void TestIfSimplifiedCheckBox()
		{
			var isSimplifiedCheckBox = control.IsSimplifiedCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", isSimplifiedCheckBox);
				isSimplifiedCheckBox.AssertThisControl(x => x.WithBindTo("IsSimplified"));
			});
		}

		public void TestIsMovementOfContainersOnlyCheckBox()
		{
			var movementOfContainersOnlyCheckBox = control.MovementOfContainersOnlyCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", movementOfContainersOnlyCheckBox);
				movementOfContainersOnlyCheckBox.AssertThisControl(x => x.WithBindTo("MovementOfContainersOnly"));
			});
		}

		public void TestTransportDocumentTypeDropEdit()
		{
			var transportDocumentTypeDropEdit = control.TransportDocumentTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", transportDocumentTypeDropEdit);
				transportDocumentTypeDropEdit.AssertThisControl(x => x.WithBindTo("Bills.TypeOfBillDocument"));
			});
		}

		public void TestTransportDocumentTextBox()
		{
			var transportDocumentTextBox = control.TransportDocumentTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", transportDocumentTextBox);
				transportDocumentTextBox.AssertThisControl(x => x.WithBindTo("Bills.ABL_BillNumber"));
			});
		}

		public void TestUnionGoodsCheckBox()
		{
			var unionGoodsCheckBox = control.UnionGoodsCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", unionGoodsCheckBox);
				unionGoodsCheckBox.AssertThisControl(x => x.WithBindTo("UnionGoods"));
			});
		}

		public void TestGuaranteeGroupBoxDynamicLayoutPanel()
		{
			var tempStorageRegHeader = Factory.New<TemporaryStorageHeader>();
			using (var userControl = new G5V1TemporaryStorageUserControl())
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
			using (var userControl = new G5V1TemporaryStorageUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Guarantee", userControl.GuaranteeGroupBox.CaptionResourceString.Caption);
					AssertEquals("Visibility for ES", true, userControl.GuaranteeGroupBox.Visible);
				});
			}
		}

		public void TestManualLocationOfGoodsCodeFindBox()
		{
			var manualLocationOfGoodsCodeFindBox = control.ManualLocationOfGoodsCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ManualLocationOfGoodsCodeFindBox>("Type", manualLocationOfGoodsCodeFindBox);
				manualLocationOfGoodsCodeFindBox.AssertThisControl(x => x.WithBindTo("ManualDestinationCustomsOffice"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new G5V1TemporaryStorageUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		G5V1TemporaryStorageUserControl control;
	}
}
