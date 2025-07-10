using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.GUI.Testing
{
	internal class PreShipmentExporterFormTest : TestCaseWithFactory
	{
		public void TestFormHeading()
		{
			AssertEquals("Pre-Shipment Exporter Form", ExporterForm.FormHeading);
		}

		public void TestOKButtonClick()
		{
			string outputPath = Path.Combine(Env.TempPath, "__PRESHIPMENTEXPORTERTEST");
			try
			{
				object lazyLoadShipment = Shipment;
				Factory.Save();
				ExporterForm.Show();
				CreateDirectory(outputPath);
				JASDataRegistry.Instance.JXCOutgoingDirectoryName = outputPath;
				JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = false;
				ExporterForm.OKButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Should not allow export, should show error message box", "There are errors that need to be corrected before JXC message can be exported", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should not be closed when there are errors/warnings", ExporterForm.Visible);
				PreShipmentWrapper preShipment = (PreShipmentWrapper)ExporterForm.BusinessEntity;
				ExporterForm.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = true;
				ExporterForm.OKButton.PerformClick();
				Application.DoEvents();
				AssertEquals("JXC Message for '" + preShipment.HumanReadableName + "' has been successfully exported to \"" + outputPath + "\"", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				Assert("Should be closed when export is successful", !ExporterForm.Visible);
				AssertEquals(DialogResult.OK, ExporterForm.DialogResult);
			}
			finally
			{
				TempDirectory.DeleteDirectory(outputPath);
			}
		}

		#region IJXCExportForm
		public void TestValidateAll()
		{
			JASOrgHeader consignee = Factory.NewWithValidTestData<JASOrgHeader>();
			consignee.OH_Code = "CON";
			consignee.OH_IsConsignee = true;
			JASOrgHeader consignor = Factory.NewWithValidTestData<JASOrgHeader>();
			consignor.OH_Code = "CONS";
			consignor.OH_IsConsignor = true;
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;
			Assert("Pre-condition", !PreShipmentWrapper.HasNotifications());
			Shipment.JS_TransportMode = "";
			((IJXCExportForm)ExporterForm).ValidateAll();
			Assert("Should have notifications now", PreShipmentWrapper.HasNotifications());
			Assert("Should have notifications now", Shipment.HasNotifications());
		}

		public void TestBusinessEntity()
		{
			AssertEquals("Should be the ZWinForm.BusinessEntity", PreShipmentWrapper, ((IJXCExportForm)ExporterForm).BusinessEntity);
		}

		#endregion
		#region Implementation
		protected override void TearDown()
		{
			ExporterForm.Dispose();
			base.TearDown();
		}

		void FillShipmentWithValidJXCData()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_HouseBill = "HB101";
			Shipment.JS_ShippedOnBoardDate = ZDateTime.Now;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "ITROM";
			Shipment.ConsigneePK = Factory.NewWithValidTestData<JASOrgHeader>().PK;
			Shipment.ConsignorPK = Factory.NewWithValidTestData<JASOrgHeader>().PK;
		}

		PreShipmentWrapper PreShipmentWrapper
		{
			get
			{
				return (PreShipmentWrapper)ExporterForm.BusinessEntity;
			}
		}

		PreShipmentExporterFormForTest ExporterForm
		{
			get
			{
				if (fExporterForm == null)
				{
					fExporterForm = new PreShipmentExporterFormForTest(Shipment);
				}

				return fExporterForm;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
					FillShipmentWithValidJXCData();
				}

				return fShipment;
			}
		}

		PreShipmentExporterFormForTest fExporterForm;
		JASForwardingShipment fShipment;
		#endregion
		#region class PreShipmentExporterFormForTest
		class PreShipmentExporterFormForTest : PreShipmentExporterForm
		{
			public PreShipmentExporterFormForTest(JASForwardingShipment shipment) : base(shipment)
			{
			}

			protected override JXCMessageGUIExportDirector GetNewJXCMessageGUIExportDirector(AirOceanMessageExporter exporter, IJXCExportForm form)
			{
				return (EnsureMessageCanBeExportedShouldAlwaysReturnTrue) ? new JXCMessageGUIExportDirectorForTest(exporter, form) : base.GetNewJXCMessageGUIExportDirector(exporter, form);
			}

			public bool EnsureMessageCanBeExportedShouldAlwaysReturnTrue;
		}

		#endregion
		#region class JXCMessageGUIExportDirectorForTest
		class JXCMessageGUIExportDirectorForTest : JXCMessageGUIExportDirector
		{
			public JXCMessageGUIExportDirectorForTest(JXCMessageExporter exporter, IJXCExportForm exportForm) : base(exporter, exportForm)
			{
			}

			public override bool EnsureMessageCanBeExported()
			{
				return true;
			}
		}
		#endregion
	}
}
