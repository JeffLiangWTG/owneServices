using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.GUI.Testing
{
	internal class JASConsolFormTest : TestCaseWithFactory
	{
		#region Export Air / Ocean Message
		public void TestExportJXCAirOcean_NotSaved()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("You must save before you can export data to JXC file", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirOcean_ConsolHasNoShipments()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("Consol does not have any shipments", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirOcean_IncompatibleShipments()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("Consol is incompatible for JXC messaging. It contains shipments with different Transport Modes (i.e. Air Shipment and Ocean Shipment)", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirOcean_UnsupportedTransportMode()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("Could not export JXC message from this Consol", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirOcean_HasErrors()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ConsolForm.Show();
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("There are errors that need to be corrected before JXC message can be exported", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirOceanMenuItem_SuccessfulForSea()
		{
			Assert("Pre-condition", !ConsolForm.ExportCalled);
			AssertNull("Pre-condition", ConsolForm.LastExporterUsed);
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			ConsolForm.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = true;
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			Assert("Should be exported", ConsolForm.ExportCalled);
			AssertEquals("Should use OceanMessageExporter", typeof(OceanMessageExporter), ConsolForm.LastExporterUsed.GetType());
			AssertEquals("Should use Consol as the HeaderData", ConsolForm.BusinessEntity, ConsolForm.LastExporterUsed.HeaderData);
		}

		public void TestExportJXCAirOceanMenuItem_SuccessfulForAir()
		{
			Assert("Pre-condition", !ConsolForm.ExportCalled);
			AssertNull("Pre-condition", ConsolForm.LastExporterUsed);
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ConsolForm.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = true;
			JXCAirOceanMenuItem.PerformClick();
			Application.DoEvents();
			Assert("Should be exported", ConsolForm.ExportCalled);
			AssertEquals("Should use AirMessageExporter", typeof(AirMessageExporter), ConsolForm.LastExporterUsed.GetType());
			AssertEquals("Should use Consol as the HeaderData", ConsolForm.BusinessEntity, ConsolForm.LastExporterUsed.HeaderData);
		}

		public void TestConsolDocumentSupporter_GettingDataStateBeforeRun_HasErrorsForNonControllerUser()
		{
			bool isController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				consol.Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ConsolForm.Show();
				DocumentSupporterDataState dataState = ConsolForm.Consol.DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
				Application.DoEvents();
				AssertEquals("Do you want to continue anyway? (The message will most likely be rejected by JASWW JXC Validator)", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("Should not be exported", !ConsolForm.ExportCalled);
				Assert("There are validation errors, should not be valid", !dataState.IsValid);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = isController;
			}
		}

		public void TestConsolDocumentSupporter_GettingDataStateBeforeRun_HasErrors_CancelPrinting()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ConsolForm.Show();
			DocumentSupporterDataState dataState = ConsolForm.Consol.DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Application.DoEvents();
			UnitTestUserNotification.PreviousMessage secondLastMessage = UnitTestUserNotification.Instance.PreviousMessages[UnitTestUserNotification.Instance.PreviousMessages.Length - 2];
			AssertEquals("There are errors that need to be corrected before JXC message can be exported", secondLastMessage.Text);
			Assert(secondLastMessage.WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
			UnitTestUserNotification.PreviousMessage lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Do you want to continue anyway? (The message will most likely be rejected by JASWW JXC Validator)", lastMessage.Text);
			Assert(lastMessage.WasQuestion);
			Assert("Cancelled by the user, should not be valid", !dataState.IsValid);
		}

		public void TestConsolDocumentSupporter_GettingDataStateBeforeRun_HasErrors_ContinuePrinting()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ConsolForm.Show();
			DocumentSupporterDataState dataState = ConsolForm.Consol.DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Application.DoEvents();
			UnitTestUserNotification.PreviousMessage secondLastMessage = UnitTestUserNotification.Instance.PreviousMessages[UnitTestUserNotification.Instance.PreviousMessages.Length - 2];
			AssertEquals("There are errors that need to be corrected before JXC message can be exported", secondLastMessage.Text);
			Assert(secondLastMessage.WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
			UnitTestUserNotification.PreviousMessage lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Do you want to continue anyway? (The message will most likely be rejected by JASWW JXC Validator)", lastMessage.Text);
			Assert(lastMessage.WasQuestion);
			Assert("Should be valid, the user ignored the warning", dataState.IsValid);
		}

		public void TestConsolDocumentSupporter_GettingDataStateBeforeRun_SuccessfulForAir()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ConsolForm.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = true;
			DocumentSupporterDataState dataState = ConsolForm.Consol.DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Application.DoEvents();
			AssertEquals("Should use AirMessageExporter", typeof(AirMessageExporter), ConsolForm.LastExporterUsed.GetType());
			AssertEquals("Should use Consol as the HeaderData", ConsolForm.BusinessEntity, ConsolForm.LastExporterUsed.HeaderData);
		}

		public void TestConsolDocumentSupporter_GettingDataStateBeforeRun_SuccessfulForSea()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ConsolForm.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = true;
			DocumentSupporterDataState dataState = ConsolForm.Consol.DocumentSupporter.GetDataStateBeforeRun(PrintExportManifest);
			Application.DoEvents();
			AssertEquals("Should use OceanMessageExporter", typeof(OceanMessageExporter), ConsolForm.LastExporterUsed.GetType());
			AssertEquals("Should use Consol as the HeaderData", ConsolForm.BusinessEntity, ConsolForm.LastExporterUsed.HeaderData);
		}

		DocumentCommand PrintExportManifest
		{
			get
			{
				return Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.SU_MenuPath, "Departure/Manifest"));
			}
		}

		DocumentCommand PrintFinalMasterDocument
		{
			get
			{
				return Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Print Final Master"));
			}
		}

		MenuItem JXCAirOceanMenuItem
		{
			get
			{
				if (fJXCAirOceanMenuItem == null)
				{
					fJXCAirOceanMenuItem = ConsolForm.ActionsMenuItem.MenuItems.FindByText("Export JXC Air/Ocean Message(s)");
					AssertNotNull("JXC Air / Ocean Export menu item should exist. Should be added in the form constructor", fJXCAirOceanMenuItem);
				}

				return fJXCAirOceanMenuItem;
			}
		}

		MenuItem fJXCAirOceanMenuItem;
		#endregion
		#region Export Air Profit Share Message
		public void TestExportJXCAirProfitShare_NotSaved()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("You must save before you can export data to JXC file", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirProfitShare_ConsolHasNoShipments()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("Consol does not have any shipments", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirProfitShare_NotAnAirConsol()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Shipments.AddNew();
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("Consol is not an Air Consol", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirProfitShare_ShipmentJobsNotClosed_CancelExport()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			JASJob job = (JASJob)new Accounting.Business.JobInvoicing.Job.Loader(shipment).TryCreate();
			job.JH_JobNum = "101";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment.Job.JH_Status = JobHeaderStatus.Working.Code;
			consol.Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("There are Shipment Jobs which have not been closed and/or there are shipments without invoices.\r\nDo you want to continue anyway? (The message might contain incorrect figures)", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);
			Assert("Should not be exported, cancelled by the user", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirProfitShare_ShipmentJobsNotClosed_IgnoreWarning()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			JASJob job = (JASJob)new Accounting.Business.JobInvoicing.Job.Loader(shipment).TryCreate();
			job.JH_JobNum = "101";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment.Job.JH_Status = JobHeaderStatus.Working.Code;
			consol.Factory.Save();
			ConsolForm.Show();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			UnitTestUserNotification.PreviousMessage secondLastMessage = UnitTestUserNotification.Instance.PreviousMessages[UnitTestUserNotification.Instance.PreviousMessages.Length - 2];
			AssertEquals("There are Shipment Jobs which have not been closed and/or there are shipments without invoices.\r\nDo you want to continue anyway? (The message might contain incorrect figures)", secondLastMessage.Text);
			Assert(secondLastMessage.WasQuestion);
			AssertEquals("There are errors that need to be corrected before JXC message can be exported", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should not be exported", !ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirProfitShare_ShipmentJobsNotClosed_IgnoreWarningExportAnyway()
		{
			JASForwardingConsol consol = (JASForwardingConsol)ConsolForm.BusinessEntity;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			JASJob job = (JASJob)new Accounting.Business.JobInvoicing.Job.Loader(shipment).TryCreate();
			job.JH_JobNum = "101";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment.Job.JH_Status = JobHeaderStatus.Working.Code;
			consol.Factory.Save();
			ConsolForm.Show();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			UnitTestUserNotification.PreviousMessage secondLastMessage = UnitTestUserNotification.Instance.PreviousMessages[UnitTestUserNotification.Instance.PreviousMessages.Length - 2];
			AssertEquals("There are Shipment Jobs which have not been closed and/or there are shipments without invoices.\r\nDo you want to continue anyway? (The message might contain incorrect figures)", secondLastMessage.Text);
			Assert(secondLastMessage.WasQuestion);
			AssertEquals("There are errors that need to be corrected before JXC message can be exported", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasError);
			Assert("Should be exported", ConsolForm.ExportCalled);
		}

		public void TestExportJXCAirProfitShare_Successful()
		{
			ConsolForm.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = true;
			JXCAirProfitShareMenuItem.PerformClick();
			Application.DoEvents();
			Assert("Should be exported", ConsolForm.ExportCalled);
			AssertEquals("Should use ProfitShareMessageExporter", typeof(ProfitShareMessageExporter), ConsolForm.LastExporterUsed.GetType());
			AssertEquals("Should use Consol as the HeaderData", ConsolForm.BusinessEntity, ConsolForm.LastExporterUsed.HeaderData);
		}

		MenuItem JXCAirProfitShareMenuItem
		{
			get
			{
				if (fJXCAirProfitShareMenuItem == null)
				{
					fJXCAirProfitShareMenuItem = ConsolForm.ActionsMenuItem.MenuItems.FindByText("Export JXC Air Profit Share Message");
					AssertNotNull("Export JXC Air Profit Share Message menu item should exist. Should be added in the form constructor", fJXCAirProfitShareMenuItem);
				}

				return fJXCAirProfitShareMenuItem;
			}
		}

		MenuItem fJXCAirProfitShareMenuItem;
		#endregion
		public void TestJXCMessageGUIExportDirector()
		{
			AirOceanMessageExporter exporter = AirOceanMessageExporter.New(ConsolForm.BusinessEntity as JASForwardingConsol);
			AssertEquals("Should be of type " + typeof(JXCMessageGUIExportDirector).FullName, typeof(JXCMessageGUIExportDirector), ConsolForm.BaseGetNewGUIExportDirector(exporter, ConsolForm).GetType());
		}

		public void TestValidateAllAndPopulateAWBWarningsCorrectly()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			consol.JK_ConsolMode = "LSE";
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUBNE";
			JASForwardingShipment shipment = Factory.NewWithValidTestData<JASForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			var consignee = Factory.NewWithValidTestData<JASOrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.Addresses[0].OA_City = "Brisbane";
			consignee.Addresses[0].OA_RL_NKRelatedPortCode = "AUBNE";
			shipment.ConsigneePK = consignee.PK;
			var consignor = Factory.NewWithValidTestData<JASOrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "Consignore";
			consignor.Addresses[0].OA_City = "Auckland";
			consignor.Addresses[0].OA_RL_NKRelatedPortCode = "NZAKL";
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_ActualWeight = 1.23;
			shipment.JS_OuterPacks = 6;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.AWBHeader.Populate();
			consol.Shipments.Add(shipment);
			consol.AWBHeader.Populate();
			Factory.Save();
			consol.UnRegisterEditableChildObject(consol.AWBHeader);
			shipment.UnRegisterEditableChildObject(shipment.AWBHeader);
			using (JASConsolForm form = new JASConsolForm(consol))
			{
				new Business.JXC.Export.Validations.JXCDomainValidationManager(form.BusinessEntity.Factory).ManageJXCValidations(JXCExportValidationType.Air);
				((IJXCExportForm)form).ValidateAll();
				Assert("No Errors", !((BusinessObject)form.BusinessEntity).HasErrors);
				System.Collections.Generic.List<JXCWarningInfo> warnings = new System.Collections.Generic.List<JXCWarningInfo>(new JXCWarningInfoCollector(form.BusinessEntity));
				AssertNotEquals("Should have warnings", 0, warnings.Count);
				bool hasShipmentAWBWarnings = false;
				bool hasConsolAWBWarnings = false;
				bool hasNatureAndQtyOfGoodsWarnings = false;
				foreach (var warning in warnings)
				{
					if (warning.BizObj.HumanReadableName.Contains("House Air Waybill for Shipment"))
					{
						hasShipmentAWBWarnings = true;
					}

					if (warning.BizObj.HumanReadableName.Contains("Master Air Waybill for Consol"))
					{
						hasConsolAWBWarnings = true;
					}

					if (warning.BizObj.HumanReadableName.Contains("Nature and Quantity of Goods"))
					{
						hasNatureAndQtyOfGoodsWarnings = true;
					}
				}

				Assert("Has shipment AWB warnings", hasShipmentAWBWarnings);
				Assert("Has consol AWB warnings", hasConsolAWBWarnings);
				Assert("Has nature and qty of goods warnings", hasNatureAndQtyOfGoodsWarnings);
			}
		}

		public void TestValidateAllAndNoAWBWarningsForSEA()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUBNE";
			JASForwardingShipment shipment = Factory.NewWithValidTestData<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var consignee = Factory.NewWithValidTestData<JASOrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.Addresses[0].OA_City = "Brisbane";
			consignee.Addresses[0].OA_RL_NKRelatedPortCode = "AUBNE";
			shipment.ConsigneePK = consignee.PK;
			var consignor = Factory.NewWithValidTestData<JASOrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "Consignore";
			consignor.Addresses[0].OA_City = "Auckland";
			consignor.Addresses[0].OA_RL_NKRelatedPortCode = "NZAKL";
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_ActualWeight = 1.23;
			shipment.JS_OuterPacks = 6;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.AWBHeader.Populate();
			consol.Shipments.Add(shipment);
			consol.AWBHeader.Populate();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			using (JASConsolForm form = new JASConsolForm(consol))
			{
				new Business.JXC.Export.Validations.JXCDomainValidationManager(form.BusinessEntity.Factory).ManageJXCValidations(JXCExportValidationType.Ocean);
				((IJXCExportForm)form).ValidateAll();
				System.Collections.Generic.List<JXCWarningInfo> warnings = new System.Collections.Generic.List<JXCWarningInfo>(new JXCWarningInfoCollector(form.BusinessEntity));
				AssertNotEquals("Should have warnings", 0, warnings.Count);
				bool hasShipmentHouseBillWarning = false;
				bool hasShipmentAWBWarnings = false;
				bool hasConsolAWBWarnings = false;
				bool hasNatureAndQtyOfGoodsWarnings = false;
				foreach (var warning in warnings)
				{
					if (warning.WarningMessage.Equals("You have not entered a House Bill Number."))
					{
						hasShipmentHouseBillWarning = true;
					}

					if (warning.BizObj.HumanReadableName.Contains("House Air Waybill for Shipment"))
					{
						hasShipmentAWBWarnings = true;
					}

					if (warning.BizObj.HumanReadableName.Contains("Master Air Waybill for Consol"))
					{
						hasConsolAWBWarnings = true;
					}

					if (warning.BizObj.HumanReadableName.Contains("Nature and Quantity of Goods"))
					{
						hasNatureAndQtyOfGoodsWarnings = true;
					}
				}

				Assert("Has shipment house bill number warning", hasShipmentHouseBillWarning);
				Assert("Has NO shipment AWB warnings", !hasShipmentAWBWarnings);
				Assert("Has NO consol AWB warnings", !hasConsolAWBWarnings);
				Assert("Has NO nature and qty of goods warnings", !hasNatureAndQtyOfGoodsWarnings);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			OutputPath = Path.Combine(Env.TempPath, "__CONSOLFORMTEST");
			CreateDirectory(OutputPath);
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = false;
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = OutputPath;
			Consol = Factory.New<JASForwardingConsol>();
			ConsolForm = new JASConsolFormForTest(Consol);
		}

		protected override void TearDown()
		{
			JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			ConsolForm.Dispose();
			TempDirectory.DeleteDirectory(OutputPath);
			base.TearDown();
		}

		JASConsolFormForTest ConsolForm;
		JASForwardingConsol Consol;
		string OutputPath;
		#endregion
		#region class JASConsolFormForTest
		class JASConsolFormForTest : JASConsolForm
		{
			public JASConsolFormForTest(JASForwardingConsol consol) : base(consol)
			{
			}

			public new MenuItem ActionsMenuItem
			{
				get
				{
					return base.ActionsMenuItem;
				}
			}

			protected override JXCMessageGUIExportDirector GetNewGUIExportDirector(JXCMessageExporter exporter, IJXCExportForm exportForm)
			{
				AssertEquals("Should be passing in the current instance", this, exportForm);
				return new JXCMessageGUIExportDirectorForTest(exporter, exportForm as JASConsolFormForTest, EnsureMessageCanBeExportedShouldAlwaysReturnTrue);
			}

			public JXCMessageGUIExportDirector BaseGetNewGUIExportDirector(JXCMessageExporter exporter, IJXCExportForm exportForm)
			{
				return base.GetNewGUIExportDirector(exporter, exportForm);
			}

			public bool ExportCalled;
			public JXCMessageExporter LastExporterUsed;
			public bool EnsureMessageCanBeExportedShouldAlwaysReturnTrue;
		}

		#endregion
		#region class JXCMessageGUIExportDirectorForTest
		class JXCMessageGUIExportDirectorForTest : JXCMessageGUIExportDirector
		{
			public JXCMessageGUIExportDirectorForTest(JXCMessageExporter exporter, JASConsolFormForTest exportForm, bool ensureMessageCanBeExportedShouldAlwaysReturnTrue) : base(exporter, exportForm)
			{
				this.EnsureMessageCanBeExportedShouldAlwaysReturnTrue = ensureMessageCanBeExportedShouldAlwaysReturnTrue;
				exportForm.LastExporterUsed = exporter;
			}

			public override bool EnsureMessageCanBeExported()
			{
				bool result;
				if (EnsureMessageCanBeExportedShouldAlwaysReturnTrue)
				{
					result = true;
				}
				else
				{
					result = base.EnsureMessageCanBeExported();
				}

				return result;
			}

			public override void Export()
			{
				ExportForm.ExportCalled = true;
			}

			new JASConsolFormForTest ExportForm
			{
				get
				{
					return (JASConsolFormForTest)base.ExportForm;
				}
			}

			readonly bool EnsureMessageCanBeExportedShouldAlwaysReturnTrue;
		}
		#endregion
	}
}
