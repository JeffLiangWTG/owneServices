using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class CUSPRLDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestColumnTSL_OwnerReferenceNumber_CharacterCasing()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var grid = cusprlDeclarationUserControl.FindSingle<ZGrid>("LinesGrid");
				AssertEquals(CharacterCasing.Normal, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OwnerReferenceNumber).CharacterCasing);
			}
		}

		public void TestOwnerReferenceNumberTextBox_CharacterCasing()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				AssertEquals(CharacterCasing.Normal, cusprlDeclarationUserControl.FindSingle<ZTextBox>("OwnerReferenceNumberTextBox").CharacterCasing);
			}
		}

		public void TestDestinationPlaceTextBox_CharacterCasing()
		{
			using (var control = new CUSPRLDeclarationUserControl())
			{
				AssertEquals(CharacterCasing.Normal, control.FindSingle<ZTextBox>("DestinationPlaceTextBox").CharacterCasing);
			}
		}

		public void TestColumnTSL_GoodsDescription_CharacterCasing()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var grid = cusprlDeclarationUserControl.FindSingle<ZGrid>("LinesGrid");
				AssertEquals(CharacterCasing.Normal, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_GoodsDescription).CharacterCasing);
			}
		}

		public void TestColumnReceptacle()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var grid = cusprlDeclarationUserControl.FindSingle<ZGrid>("LinesGrid");
				var columnStyle = grid.GetColumnStyle(CUSPRLCusTempStorageLine.Schema.Receptacle);
				AssertType<ZTextBoxColumnStyleInfo>(columnStyle);
			}
		}

		public void TestColumnContainerNumber()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var grid = cusprlDeclarationUserControl.FindSingle<ZGrid>("LinesGrid");
				var columnStyle = grid.GetColumnStyle(CUSPRLCusTempStorageLine.Schema.ContainerNumber);
				AssertType<ZTextBoxColumnStyleInfo>(columnStyle);
			}
		}

		public void TestTabStopsAreFalse()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var declarationPanel = cusprlDeclarationUserControl.Controls.Find("DeclarationPanel", true).First();
				CombineAssertions(() =>
				{
					AssertEquals("StatusTextBox", false, declarationPanel.Controls["StatusTextBox"].TabStop);
					AssertEquals("CreatedDateEdit", false, declarationPanel.Controls["CreatedDateEdit"].TabStop);
				});
			}
		}

		public void TestDeclarationNeverLocked()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			storageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;

			using (var form = new ZForm(storageHeader))
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				form.Controls.Add(cusprlDeclarationUserControl);
				form.SetDataBinding(storageHeader, ".");
				form.Show();
				AssertEquals(false, storageDec.ReadOnly);
			}
		}

		public void TestESumA_EAS2_GroupBoxCaption()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var groupBox = cusprlDeclarationUserControl.FindSingle<ZGroupBox>("ESumAGroupBox");
				AssertEquals("ESumA/EAS2", groupBox.CaptionResourceString.Caption);
			}
		}

		public void TestControls_TransportDocumentGroupBox()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var transportDocumentGroupBox = cusprlDeclarationUserControl.FindSingle<ZGroupBox>("TransportDocumentGroupBox");

				CombineAssertions(() =>
				{
					AssertNoExceptionThrown(() => transportDocumentGroupBox.FindSingle<ZCodeFindBox>("TransportNumberTypeCodeFindBox"));
					AssertNoExceptionThrown(() => transportDocumentGroupBox.FindSingle<ZTextBox>("TransportNumberTextBox"));
				});
			}
		}

		public void TestCarrierJobDocAddressControl()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var carrierDocAddressControl = cusprlDeclarationUserControl.FindSingle<ZDocAddressControl>("CarrierDocAddressControl");

				CombineAssertions(() =>
				{
					AssertEquals("BindingMember", $"{nameof(CusTempStorageJobHeader.CUSPRLCusTempStorageDec)}.{nameof(CUSPRLCusTempStorageDec.CusTempStorageLines)}.{nameof(CUSPRLCusTempStorageLine.CarrierDocAddress)}", carrierDocAddressControl.GetBindingMember());
					AssertEquals("BindToOrganizations", "CUSPRLCusTempStorageDec.CusTempStorageLines.Lookups.OrgHeaderCollection", carrierDocAddressControl.BindToOrganisations);
					AssertEquals("DisplayMode", ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox, carrierDocAddressControl.DisplayMode);
				});
			}
		}

		public void TestTransportDocumentMasterGroupBox()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var transportDocumentMasterGroupBox = cusprlDeclarationUserControl.FindSingle<ZGroupBox>("TransportDocumentMasterGroupBox");

				CombineAssertions(() =>
				{
					AssertNoExceptionThrown(() => transportDocumentMasterGroupBox.FindSingle<ZCodeFindBox>("TransportDocumentMasterTypeCodeFindBox"));
					AssertNoExceptionThrown(() => transportDocumentMasterGroupBox.FindSingle<ZTextBox>("TransportDocumentMasterReferenceNumberTextBox"));
				});
			}
		}

		public void TestTransportDocumentMasterTypeFindBox()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var typeCodeFindBox = cusprlDeclarationUserControl.FindSingle<ZCodeFindBox>("TransportDocumentMasterTypeCodeFindBox");

				CombineAssertions(() =>
				{
					AssertEquals("BindTo", $"{nameof(CusTempStorageJobHeader.CUSPRLCusTempStorageDec)}.{nameof(CUSPRLCusTempStorageDec.CusTempStorageLines)}.{nameof(CUSPRLCusTempStorageLine.TransportDocumentMaster)}.{nameof(TransportDocumentMaster.CSI_Code)}", typeCodeFindBox.GetBindingMember());
				});
			}
		}

		public void TestTransportDocumentMasterReferenceNumberTextBox()
		{
			using (var cusprlDeclarationUserControl = new CUSPRLDeclarationUserControl())
			{
				var referenceNumberTextBox = cusprlDeclarationUserControl.FindSingle<ZTextBox>("TransportDocumentMasterReferenceNumberTextBox");

				CombineAssertions(() =>
				{
					AssertEquals("BindTo", $"{nameof(CusTempStorageJobHeader.CUSPRLCusTempStorageDec)}.{nameof(CUSPRLCusTempStorageDec.CusTempStorageLines)}.{nameof(CUSPRLCusTempStorageLine.TransportDocumentMaster)}.{nameof(TransportDocumentMaster.CSI_ReferenceNumber)}", referenceNumberTextBox.GetBindingMember());
				});
			}
		}
	}
}
