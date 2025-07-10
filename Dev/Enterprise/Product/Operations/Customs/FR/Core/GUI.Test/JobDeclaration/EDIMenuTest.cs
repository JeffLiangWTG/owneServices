using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	public class EDIMenuTest : TestCaseWithFactory
	{
		public void TestSupplementaryMenuItemsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var menu = new EDIMenu
				{
					Declaration = declaration
				};

				var supplementaryEntryMenuItem = menu.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.SupplementaryEntry);
				Assert("When declaration is UCC6, SupplementaryEntryMenuItem is visible", supplementaryEntryMenuItem.Visible);

				var menuItem = supplementaryEntryMenuItem.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.NewRelatedDeclaration);
				Assert("When declaration is UCC6, NewRelatedDeclarationMenuItem is visible", menuItem.Visible);
				menuItem = supplementaryEntryMenuItem.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.NewEntryInstruction);
				Assert("When declaration is UCC6, NewEntryInstructionMenuItem is not visible", !menuItem.Visible);
				menuItem = supplementaryEntryMenuItem.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.ReUseEntryInstruction);
				Assert("When declaration is UCC6, ReUseEntryInstructionMenuItem is visible", menuItem.Visible);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var menu = new EDIMenu
				{
					Declaration = declaration
				};

				var supplementaryEntryMenuItem = menu.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.SupplementaryEntry);
				Assert("When declaration is not UCC6, SupplementaryEntryMenuItem is not visible", !supplementaryEntryMenuItem.Visible);
			}
		}

		public void TestAmendmentSnapshotManagementManuItemsCreator()
		{
			using var ediMenu = new EDIMenuForTest();
			var amendmentSnapshotManagementMenuItemsCreator = typeof(Customs.GUI.EDIMenu).GetField("amendmentSnapshotManagementMenuItemsCreator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ediMenu);
			AssertType<AmendmentSnapshotManagementMenuItemsCreator>(amendmentSnapshotManagementMenuItemsCreator);
		}

		public void TestSenCINMessagesMenu()
		{
			var dec = Factory.New<JobDeclaration>();

			var ediMenu = new EDIMenu
			{
				Declaration = dec
			};
			var sendCINMenu = ediMenu.MenuItems.FindByText("Send CIN");
			AssertNotNull("There should be a Send CIN menu", sendCINMenu);
			var send745Menu = sendCINMenu.MenuItems.FindByText("Send 745");
			AssertNotNull("There should be a Send 745 menu", send745Menu);
			var send755Menu = sendCINMenu.MenuItems.FindByText("Send 755");
			AssertNotNull("There should be a Send 755 menu", send755Menu);

			dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			ediMenu.RefreshMenu();
			AssertEquals("SendCIN menu should only be visible for export declarations.", false, sendCINMenu.Visible);
			dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			ediMenu.RefreshMenu();
			AssertEquals("SendCIN menu should be visible for air export declarations", true, sendCINMenu.Visible);
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			ediMenu.RefreshMenu();
			AssertEquals("SendCIN menu should be visible for air export declarations.", false, sendCINMenu.Visible);

			ediMenu.Dispose();
		}

		public void TestDeltaSendMenuVisible()
		{
			var dec = Factory.New<JobDeclaration>();

			var ediMenu = new EDIMenu
			{
				Declaration = dec
			};

			ediMenu.OnPopup(EventArgs.Empty);

			var sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaGMessageMenuItem");
			AssertEquals("send DeltaG message menu should be visible if UCC5", true, sendDeltaMenu.Visible);

			sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaIEMessageMenuItem");
			AssertEquals("send DeltaIE message menu should not be visible if UCC5", false, sendDeltaMenu.Visible);

			dec.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
			ediMenu.RefreshMenu();
			sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaIEMessageMenuItem");
			AssertEquals("send DeltaIE message menu should be visible if UCC6", true, sendDeltaMenu.Visible);

			sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaGMessageMenuItem");
			AssertEquals("send DeltaG message menu should not be visible if UCC6", false, sendDeltaMenu.Visible);

			dec.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.Interface;
			ediMenu.RefreshMenu();

			sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaGMessageMenuItem");
			AssertEquals("JE_ApplicationCode is interface then sendDeltaGMessageMenuItem should not be visible.", false, sendDeltaMenu.Visible);

			sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaIEMessageMenuItem");
			AssertEquals("JE_ApplicationCode is interface then sendDeltaIEMessageMenuItem should not be visible.", false, sendDeltaMenu.Visible);

			ediMenu.Dispose();
		}

		public void TestGenerateEntriesMenuVisible()
		{
			var dec = Factory.New<JobDeclaration>();

			var ediMenu = new EDIMenu
			{
				Declaration = dec
			};

			ediMenu.OnPopup(EventArgs.Empty);

			var generateEntriesMenu = ediMenu.MenuItems.FindByText("Generate Entries (Merge)");

			AssertEquals(true, generateEntriesMenu.Visible);

			ediMenu.Dispose();

			dec.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.Interface;

			var ediMenu2 = new EDIMenu
			{
				Declaration = dec
			};

			ediMenu2.OnPopup(EventArgs.Empty);

			generateEntriesMenu = ediMenu2.MenuItems.FindByText("Generate Entries (Merge)");

			AssertEquals(false, generateEntriesMenu.Visible);

			ediMenu2.Dispose();
		}

		public void TestLabelMenuSendDelta()
		{
			var dec = Factory.New<JobDeclaration>();

			var ediMenu = new EDIMenu
			{
				Declaration = dec
			};

			var fallbackSetting = new FallbackSettings();

			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			AssertEquals("Send Fallback", EDIMenu.LabelMenuSendDeltaG);

			fallbackSetting.Start = ZDateTime.Today.AddDays(5);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			AssertEquals("Send Delta", EDIMenu.LabelMenuSendDeltaG);
		}

		public void TestSendDeltaForUCC6()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "DI";
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = dec
			})
			{
				ediMenu.OnPopup(EventArgs.Empty);

				var sendDeltaMenu = ediMenu.MenuItems.FindByName("sendDeltaIEMessageMenuItem");
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertNoExceptionThrown(() => sendDeltaMenu.PerformClick());
			}
		}

		public void TestNoNullReferenceExceptionAnymoreWhenNoEntryInstruction()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var entry = dec.CustomsEntryHeaders.AddNew();

			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = dec
			})
			{
				ediMenu.OnPopup(EventArgs.Empty);

				var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Delta");
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertNoExceptionThrown(() => sendDeltaMenu.PerformClick());
				ErrorReporter.Clear();
			}
		}

		public void TestSendMultipleEntriesWithBondedWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				// create declaration and add first entry
				var (declaration, entryInstruction1, entry1, invoiceLine1) = CreateDeclarationWithBondedWarehouseSupported(helper);
				var messageInitiator = declaration.MessageInitiator as SendsMessagesToCustomsShutterUpperer;

				// send ANT message
				using (var ediMenu = new EDIMenuForTest
				{
					Declaration = declaration
				})
				{
					ediMenu.OnPopup(EventArgs.Empty);

					var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Delta");
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					sendDeltaMenu.PerformClick();
				}

				AssertEquals("No error yet.", true, string.IsNullOrEmpty(messageInitiator.InvalidOperationText));

				// there should be one hold note created
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry1.CH_WarehouseTransactionStatus);
				var note1 = entry1.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
				AssertXMLContains($"<PartNo>{helper.Part.OP_PartNum}</PartNo>", Compressor.UncompressAsString(note1.ST_NoteData));

				// do some modification against the first entry, like changing the product to Part2
				invoiceLine1.JI_PartNo = helper.Part2.OP_PartNum;

				// add second entry
				var whsWarehouse2 = helper.GetNewWhsWarehouse(helper.Warehouse2.MainAddress.PK, true, "N20", WarehouseConstants.WarehouseType.FreeTradeZone);
				whsWarehouse2.WW_IsVirtualWarehouse = false;
				var whsRowB = helper.WhsHelper.CreateRowAndGenerateLocations(whsWarehouse2, "B");
				Factory.Save();

				var locationBPK = helper.WhsHelper.FindLocation(whsWarehouse2.PK, "B").PK;
				var receiveB = helper.GetNewWhsReceive(whsWarehouse2.PK, helper.Importer.PK);
				receiveB.WD_CustomsParentReference = "2FREORI1234-B00002377-EDIDATEDI";
				receiveB.WD_TotalUnits = 100m;
				receiveB.WD_ExternalReferenceSplit = 1;
				var receiveLineB = helper.GetNewWhsReceiveLine(receiveB.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
				receiveLineB.WE_WL = locationBPK;
				var whsInventoryB = receiveLineB.Inventory;
				Factory.Save();

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_OA_Warehouse2 = whsWarehouse2.WW_OA_WarehouseAddress;

				var authorisationHeader2 = helper.Warehouse2.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("WHS 001");
				var authorisationRule2 = authorisationHeader2.CusAuthorisationRules.AddNew();
				authorisationRule2.CPR_RuleCode = Customs.FR.Business.CusAuthorisationRuleTypeList.Codes.AUT;
				authorisationRule2.CPR_ValueFrom = "12345";
				var authorisationUsage2 = entryInstruction2.CusAuthorizationUsages.AddNew();
				authorisationUsage2.AGC_Number = "WHS 001";
				authorisationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				authorisationUsage2.AGC_OH_Owner = helper.Warehouse2.PK;

				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.EntryNumber = "ENT002";
				entry2.CH_CEI_Instruction = entryInstruction2.PK;
				var entryLine2 = entry2.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 100;

				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				invoiceLine2.JI_InvoiceQuantity = 1;
				invoiceLine2.JI_CustomsQuantity = 1;
				invoiceLine2.JI_ValuationCode = ValuationMethodList.Codes._1;
				var procedure = helper.InwardCusProcedure;
				invoiceLine2.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
				invoiceLine2.JI_BondedWhsQuantity = 1;
				entryLine2.InvoiceLines.Add(invoiceLine2);

				// send ANT message again
				using (var ediMenu = new EDIMenuForTest
				{
					Declaration = declaration
				})
				{
					ediMenu.OnPopup(EventArgs.Empty);

					var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Delta");
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					sendDeltaMenu.PerformClick();
				}

				AssertEquals("We get an error because we're trying to send original message again on entry 1.", "There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.", messageInitiator.InvalidOperationText);

				// for the first entry, its note should not be changed, as the message sending is failed (because we cannot send original message when holding).
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry1.CH_WarehouseTransactionStatus);
				note1 = entry1.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
				// the product is still Part1
				AssertXMLContains($"<PartNo>{helper.Part.OP_PartNum}</PartNo>", Compressor.UncompressAsString(note1.ST_NoteData));

				// for the second entry, there should be one hold note created, as the message sending is successful.
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry2.CH_WarehouseTransactionStatus);
				var note2 = entry2.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
				AssertXMLContains($"<PartNo>{helper.Part2.OP_PartNum}</PartNo>", Compressor.UncompressAsString(note2.ST_NoteData));
			}
		}

		public void TestMessageNotSentToWarehouseWhenEntryIsNotSelectedInSendingForm()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var (declaration, instruction, entry1, invoiceLine1) = CreateDeclarationWithBondedWarehouseSupported(helper);

				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.EntryNumber = "ENT002";
				entry2.CH_CEI_Instruction = instruction.PK;
				var entryLine2 = entry2.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 100;

				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				invoiceLine2.JI_InvoiceQuantity = 1;
				invoiceLine2.JI_CustomsQuantity = 1;
				invoiceLine2.JI_ValuationCode = ValuationMethodList.Codes._1;
				var procedure = helper.InwardCusProcedure;
				invoiceLine2.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
				invoiceLine2.JI_BondedWhsQuantity = 1;
				entryLine2.InvoiceLines.Add(invoiceLine2);

				using (var ediMenu = new EDIMenuForTest { Declaration = declaration })
				{
					ediMenu.OnPopup(EventArgs.Empty);
					var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Delta");
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					ediMenu.OnlyFirstEntryIsSelectedInSendingForm = true;
					sendDeltaMenu.PerformClick();

					CombineAssertions("When entry is selected in SendingForm, message should be sent to Warehouse.", () =>
					{
						AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry1.CH_WarehouseTransactionStatus);
						var note = entry1.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
						AssertXMLContains($"<PartNo>{helper.Part.OP_PartNum}</PartNo>", Compressor.UncompressAsString(note.ST_NoteData));
					});

					CombineAssertions("When entry is not selected in SendingForm, message should not be sent to Warehouse.", () =>
					{
						AssertEquals(ZString.Empty, entry2.CH_WarehouseTransactionStatus);
						AssertEquals(0, entry2.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
					});
				}
			}
		}

		public void TestMessageNotSentToWarehouseWhenBondedWarehousingDisabledInEntry()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var (declaration, instruction, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper);

				using (var ediMenu = new EDIMenuForTest { Declaration = declaration })
				{
					ediMenu.OnPopup(EventArgs.Empty);
					var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Delta");
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
					authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
					var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
					authorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
					authorizationUsage.AGC_CPH_Authorization = authorisationHeader.PK;
					var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
					authorisationRule.CPR_RuleCode = FR.Business.CusAuthorisationRuleTypeList.Codes.CON;
					authorisationRule.CPR_ValueFrom = RuleCodeCONValueFromList.Codes.SUC;

					CombineAssertions("When entry.IsBondedWarehousingDisabled is true, message should not be sent to Warehouse.", () =>
					{
						AssertEquals("Prerequisite", true, entry.IsBondedWarehousingDisabled);
						sendDeltaMenu.PerformClick();

						AssertEquals(ZString.Empty, entry.CH_WarehouseTransactionStatus);
						AssertEquals(0, entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
					});

					authorisationRule.CPR_ValueFrom = RuleCodeCONValueFromList.Codes.CNA;

					CombineAssertions("When entry.IsBondedWarehousingDisabled is false, message should be sent to Warehouse.", () =>
					{
						AssertEquals("Prerequisite", false, entry.IsBondedWarehousingDisabled);
						sendDeltaMenu.PerformClick();

						AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);
						var note = entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
						AssertXMLContains($"<PartNo>{helper.Part.OP_PartNum}</PartNo>", Compressor.UncompressAsString(note.ST_NoteData));
					});
				}
			}
		}

		public void TestGetNewBondedWarehouseOperationDeterminer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var bondedWarehouseOperationDeterminer = ediMenu.GetNewBondedWarehouseOperationDeterminer(entry);
				AssertType<EntryBondedWarehouseOperationDeterminer>(bondedWarehouseOperationDeterminer);
			}
		}

		public void TestPortMessagingMenuItemVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "testref";

			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				AssertEquals(true, portMessagingMenuItem.Visible);

				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				AssertEquals(true, entryMessagingMenuItem.Visible);

				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Declaration Pre-Check (CAED)");
				AssertEquals(true, caedMessagingMenuItem.Visible);
			}
		}

		public void TestTracingRequestMenuItemVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "FRCDG";
			declaration.JE_RL_NKPortOfLoading = "USRTG";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "testref";

			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				AssertEquals("No container => The TRC menu should no be visible.", false, caedMessagingMenuItem.Visible);
			}

			declaration.CusContainers.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			AssertEquals("Prerequis: entry has container", 1, entry.Containers.Length);

			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				AssertEquals("Transport mode is Sea, declaration is Import and JE_RL_NKPortOfArrival is FR and has a Container => The TRC menu should be visible.", true, caedMessagingMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKPortOfArrival = "USRTG";
			declaration.JE_RL_NKPortOfLoading = "FRCDG";
			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				AssertEquals("Transport mode is Sea, declaration is Export and JE_RL_NKPortOfLoading is FR and has a Container => The TRC menu should be visible.", true, caedMessagingMenuItem.Visible);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				AssertEquals("Transport mode is Air => The TRC menu should be not visible.", false, caedMessagingMenuItem.Visible);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_RL_NKPortOfArrival = "FRCDG";
			declaration.JE_RL_NKPortOfLoading = "USRTG";
			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				AssertEquals("JE_RL_NKPortOfLoading is not FR but message type is Export => The TRC menu should be not visible.", false, caedMessagingMenuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "USRTG";
			declaration.JE_RL_NKPortOfLoading = "FRCDG";
			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				AssertEquals("JE_RL_NKPortOfArrival is not FR but message type is Import => The TRC menu should be not visible.", false, caedMessagingMenuItem.Visible);
			}
		}

		public void TestTracingRequestMenuItemOnClick_OneContainer()
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var currentBranch = GlbBranch.CurrentBranch;
			currentBranch.GB_Code = "SYD";
			var branchProxy = currentBranch.OrgProxy;
			var ci5Code = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "CI5_001", Core.Constants.CountryCodes.France);
			var sonCode = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SOA, "SOA_001", Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000002";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "FRXXX";

			var portOfArrival = CreateNewOrGetExistingRefUNLOCO("FRXXX");
			var portofLoading = CreateNewOrGetExistingRefUNLOCO("AUSYD");
			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryType = "BKG";
			entryNumber.CE_EntryNum = "BKG001";

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_MasterBill = "MAWB123";

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.JE_DateAtOrigin = new ZDateTime(2023, 05, 10);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2023, 05, 15);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "testref";

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "Container1";
			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_IsNonOperativeReefer = true;
			commonContainer.JC_ContainerNum = "NUM1";
			cusContainer.CO_JC = commonContainer.PK;

			var entryLine = entry.MergedLines.AddNew();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			AssertEquals("Prerequis: entry has 1 container", 1, entry.Containers.Length);

			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");
				caedMessagingMenuItem.PerformClick();

				var message = "Are you sure you want to send the TRC container tracing message to the port system?";
				Assert("Following query should be shown.", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(message));
				AssertEquals("Message is sent.", "Tracing Request (TRC) has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
				AssertContains(@"<BookingConfirmationReference>BKG001</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PortOfDestination Name=""RIS Inland waterways"">FRXXX</PortOfDestination>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType></ShipmentType>
    <WayBillNumber>MAWB123</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRXXX</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>RIS Inland waterways</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>B00000002</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <ContainerCollection>
      <Container>
        <ContainerNumber>NUM1</ContainerNumber>
        <NonOperatingReefer>true</NonOperatingReefer>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2023-05-15T00:00:00</Value>
      </Date>
    </DateCollection>", ediMessage.EM_MessageText);
			}
		}

		public void TestTracingRequestMenuItemOnClick_MoreThanOneContainer()
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var currentBranch = GlbBranch.CurrentBranch;
			currentBranch.GB_Code = "SYD";
			var branchProxy = currentBranch.OrgProxy;
			var ci5Code = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "CI5_001", Core.Constants.CountryCodes.France);
			var sonCode = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SOA, "SOA_001", Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000002";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "FRXXX";

			var portOfArrival = CreateNewOrGetExistingRefUNLOCO("FRXXX");
			var portofLoading = CreateNewOrGetExistingRefUNLOCO("AUSYD");

			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryType = "BKG";
			entryNumber.CE_EntryNum = "BKG001";

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_MasterBill = "MAWB123";

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.JE_DateAtOrigin = new ZDateTime(2023, 05, 10);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2023, 05, 15);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "testref";

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "Container1";
			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_IsNonOperativeReefer = true;
			commonContainer.JC_ContainerNum = "Num1";
			cusContainer.CO_JC = commonContainer.PK;

			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "Container2";
			var commonContainer2 = Factory.New<CommonContainer>();
			commonContainer2.JC_IsNonOperativeReefer = true;
			commonContainer2.JC_ContainerNum = "Num2";
			cusContainer2.CO_JC = commonContainer2.PK;

			var entryLine = entry.MergedLines.AddNew();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			Factory.Save();
			AssertEquals("Prerequis: entry has 2 containers", 2, entry.Containers.Length);

			var selectedContainers = declaration.CusContainers.Select(x => x.JobContainer).ToArray();
			var containerSelector = new DummyContainerSelector(selectedContainers);

			using (ObjectFactory.Substitute<IContainerSelector>(containerSelector))
			using (Factory.AddDisposableService())
			using (var messagingMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var portMessagingMenuItem = messagingMenu.MenuItems.FindByText("Port Messaging");
				var entryMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("testref");
				var caedMessagingMenuItem = entryMessagingMenuItem.MenuItems.FindByText("Tracing Request(TRC) (FR)");

				caedMessagingMenuItem.OnPopup(EventArgs.Empty);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				caedMessagingMenuItem.PerformClick();
				var message = "Are you sure you want to send the TRC container tracing message to the port system?";
				Assert("Following query should be shown.", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(message));
				AssertEquals("Message is sent.", "Tracing Request (TRC) has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
				AssertContains(@"<BookingConfirmationReference>BKG001</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PortOfDestination Name=""RIS Inland waterways"">FRXXX</PortOfDestination>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType></ShipmentType>
    <WayBillNumber>MAWB123</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRXXX</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>RIS Inland waterways</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>B00000002</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <ContainerCollection>
      <Container>
        <ContainerNumber>NUM1</ContainerNumber>
        <NonOperatingReefer>true</NonOperatingReefer>
      </Container>
      <Container>
        <ContainerNumber>NUM2</ContainerNumber>
        <NonOperatingReefer>true</NonOperatingReefer>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2023-05-15T00:00:00</Value>
      </Date>
    </DateCollection>", ediMessage.EM_MessageText);
			}
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			return result;
		}

		public void TestRefreshMenuWhenDeclarationIsNull()
		{
			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = null
			})
			{
				AssertNoExceptionThrown(() => ediMenu.OnPopup(EventArgs.Empty));
			}
		}

		public void TestBizObjFallbackPorpertiesWhenSendingDeltaGMessages()
		{
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "FRPAR";

			var importerAddress = Factory.New<OrgAddress>();
			importerAddress.FillWithValidTestData();
			importerAddress.OA_OH = importer.PK;
			importerAddress.OA_PostCode = "1234";
			importerAddress.OA_Code = "code";
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "FRTVA1234");
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "FR31211234");
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR31211234");

			importer.MainAddress.OA_PostCode = "1234";

			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "FR000040";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_RepresentativeID = "A4D9F6E0";

			var orgCusAccount2 = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount2.CZ_Account = "FR000040";
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount2.CZ_OH = importer.PK;
			orgCusAccount2.CZ_RepresentativeID = "A4D9F6E0";

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
			Factory.Save();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var declaration = Factory.New<JobDeclaration>();
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FREORI1234");
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "FRCBR1234");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_DeclarationReference = "B00002377";
			declaration.JE_CustomsProfile = "FR000040";
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_CustomsQuantity = 1;
			invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._1;
			invoiceLine1.JI_Weight = 10;
			invoiceLine1.JI_WeightUQ = "G";
			invoiceLine1.JI_NetWeight = 5;
			invoiceLine1.JI_WeightUQ = "G";
			declaration.SupportingDocuments.AddNew();
			declaration.SupportingDocuments[0].CSI_DateOfIssue = ZDateTime.Today;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "ENT001";
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			entryLine1.InvoiceLines.Add(invoiceLine1);

			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				ediMenu.OnPopup(EventArgs.Empty);

				var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Fallback");
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendDeltaMenu.PerformClick();
			}

			AssertEquals("Entry should have a fallback entry number.", "0000000001", entry1.FRCustomsFallbackNumber);
			Assert("Declaration should have a 5000 special mention.", declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == Business.Declaration.CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours));
		}

		public void TestSendDeltaMessageWithImportDeclarationWithSecurity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders.AddNew();
			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var sendDeltaMenu = GetSendDeltaMenu(ediMenu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.ImportMessaging.IsAllowed = false;
				sendDeltaMenu.PerformClick();

				AssertEquals("The ImportMessaging's error message should be shown.", Env.Security.ImportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.ImportMessaging.IsAllowed = true;
				sendDeltaMenu.PerformClick();

				AssertNotEquals("There should be no ImportMessaging's error message here.", Env.Security.ImportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestSendDeltaMessageWithExportDeclarationWithSecurity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();
			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				var sendDeltaMenu = GetSendDeltaMenu(ediMenu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.ExportMessaging.IsAllowed = false;
				sendDeltaMenu.PerformClick();

				AssertEquals("The ExportMessaging's error message should be shown.", Env.Security.ExportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.ExportMessaging.IsAllowed = true;
				sendDeltaMenu.PerformClick();

				AssertNotEquals("There should be no ExportMessaging's error message here.", Env.Security.ExportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestPreSaveDeclarationNotWorksWhenMessagingSecurityNotAllowed()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_CEI = instruction.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "DP1";
			department.GE_Warehouse = true;
			department.GE_CustomsBrokerage = true;

			var job = new JobHeader.Loader(declaration).TryCreate();
			job.JH_GE = department.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();
			using (var ediMenu = new EDIMenuForTest
			{
				Declaration = declaration
			})
			{
				ediMenu.RefreshMenu();
				var sendDeltaMenu = GetSendDeltaMenu(ediMenu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.HasChanges = true;
				shipment.HasChanges = true;
				Env.Security.ExportMessaging.IsAllowed = false;
				sendDeltaMenu.PerformClick();

				AssertEquals("The ExportMessaging's error message should be shown.", Env.Security.ExportMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.HasChanges = true;
				shipment.HasChanges = true;
				Env.Security.ExportMessaging.IsAllowed = true;
				sendDeltaMenu.PerformClick();

				AssertNotEquals("PreSaveDeclaration should not pass as both declaration and shipment have changed.", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestSendDeltaGMessage_OnlyValidSuccessMessageIsMessageSendSuccessful()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.FillWithValidTestData();
				org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SRTFR1234");
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FREORI1234");
				org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "FRCBR1234");
				org.OH_IsConsignee = true;
				org.OH_IsConsignor = true;
				var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
				frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;

				Factory.Save();

				var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
				orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
				orgCusAccount.CZ_Account = "FR000040";
				orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
				orgCusAccount.CZ_OH = org.PK;
				orgCusAccount.CZ_RepresentativeID = "A4D9F6E0";

				var declaration = Factory.New<JobDeclaration>();
				declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FREORI1234");
				declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "FRCBR1234");
				declaration.JE_CustomsProfile = "XYZ";
				declaration.JE_OH_Supplier = org.PK;
				declaration.JE_OH_Importer = org.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				declaration.JE_DeclarationReference = "B00002377";
				declaration.JE_CustomsProfile = "FR000040";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 100;

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_InvoiceQuantity = 1;
				invoiceLine.JI_CustomsQuantity = 1;
				invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
				invoiceLine.JI_Weight = 10;
				invoiceLine.JI_WeightUQ = "G";
				invoiceLine.JI_NetWeight = 5;
				invoiceLine.JI_WeightUQ = "G";

				Factory.Save();

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.MessageInitiator = messageInitiator;
				declaration.DoMerge();

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

				using (var ediMenu = new EDIMenuForTest { Declaration = declaration })
				{
					var sendDeltaMenu = GetSendDeltaMenu(ediMenu);

					CombineAssertions(() =>
					{
						CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
						FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						sendDeltaMenu.PerformClick();

						AssertEquals("Success message expected", DeltaGMessageSender.MessageSendSuccessful, UnitTestUserNotification.Instance.LastMessage.Text);

						org.OH_IsDebtor = true;
						org.CompanyData.OB_AROnCreditHold = true;
						org.OH_Code = "DJC123";
						Factory.Save();

						org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

						CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
						FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
						using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(
							GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
							AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
						{
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							sendDeltaMenu.PerformClick();

							AssertContains("Credit check message expected", "Credit Control Approval Request created", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					});
				}
			}
		}

		MenuItem GetSendDeltaMenu(EDIMenuForTest ediMenu)
		{
			ediMenu.OnPopup(EventArgs.Empty);

			var sendDeltaMenu = ediMenu.MenuItems.FindByText("Send Delta");
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			return sendDeltaMenu;
		}

		public (JobDeclaration, CusEntryInstruction, CusEntryHeader, JobComInvoiceLine) CreateDeclarationWithBondedWarehouseSupported(WhsDataTestHelper helper)
		{
			helper.Importer.MainAddress.OA_PostCode = "1234";
			helper.Importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "FRTVA1234");

			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
			whsWarehouse.WW_IsVirtualWarehouse = false;
			var whsRowA = helper.WhsHelper.CreateRowAndGenerateLocations(whsWarehouse, "A");
			Factory.Save();

			var locationAPK = helper.WhsHelper.FindLocation(whsWarehouse.PK, "A").PK;
			var receive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			receive.WD_CustomsParentReference = "2FREORI1234-B00002377-EDIDATEDI";
			receive.WD_TotalUnits = 100m;
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			receiveLine.WE_WL = locationAPK;
			var whsInventory = receiveLine.Inventory;
			Factory.Save();

			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "FR000040";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_OH = helper.Importer.PK;
			orgCusAccount.CZ_RepresentativeID = "A4D9F6E0";

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(helper.Importer);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FREORI1234");
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "FRCBR1234");
			helper.Importer.OH_RL_NKClosestPort = "FRPAR";
			helper.Supplier.OH_RL_NKClosestPort = "AUSYD";
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_DeclarationReference = "B00002377";
			declaration.JE_CustomsProfile = "FR000040";
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.SetSupportsBondedWarehousingForTesting(true);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse2 = whsWarehouse.WW_OA_WarehouseAddress;

			var authorisationHeader = helper.Warehouse.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("WHS 001");
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = Customs.FR.Business.CusAuthorisationRuleTypeList.Codes.AUT;
			authorisationRule.CPR_ValueFrom = "12345";
			var authorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorisationUsage.AGC_Number = "WHS 001";
			authorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			authorisationUsage.AGC_OH_Owner = helper.Warehouse.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
			var procedure = helper.InwardCusProcedure;
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
			invoiceLine.JI_BondedWhsQuantity = 1;
			declaration.SupportingDocuments.AddNew();
			declaration.SupportingDocuments[0].CSI_DateOfIssue = ZDateTime.Today;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "ENT001";
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			entryLine.InvoiceLines.Add(invoiceLine);

			return (declaration, entryInstruction, entry, invoiceLine);
		}
	}

	class EDIMenuForTest : EDIMenu
	{
		protected override DeltaGMessageSendingForm GetMessageSendingForm(DeltaGJobDeclarationMessageSendingObjectParent decWrapper)
		{
			return new ANTMessageSendingFormForTest(decWrapper, OnlyFirstEntryIsSelectedInSendingForm);
		}

		public new BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			return base.GetNewBondedWarehouseOperationDeterminer(supporter);
		}

		public bool OnlyFirstEntryIsSelectedInSendingForm { get; set; }
	}

	class ANTMessageSendingFormForTest : DeltaGMessageSendingForm
	{
		public ANTMessageSendingFormForTest(DeltaGJobDeclarationMessageSendingObjectParent declarationWrapper, bool onlyFirstEntryIsSelectedInSendingForm = false) : base(declarationWrapper)
		{
			if (onlyFirstEntryIsSelectedInSendingForm)
			{
				declarationWrapper.SendingObjectsCollection.OfType<DeltaGJobDeclarationMessageSendingObject>().ForEach(x => x.ShouldSend = false);
				var firstSendingObject = declarationWrapper.SendingObjectsCollection.FirstOrDefault() as DeltaGJobDeclarationMessageSendingObject;
				if (firstSendingObject != null)
				{
					firstSendingObject.ShouldSend = true;
				}
			}

			declarationWrapper.SendingObjectsCollection.OfType<DeltaGJobDeclarationMessageSendingObject>().ForEach(x => x.MessageType = EntryActionCodeList.Codes.ANT);
		}
	}

	sealed class DummyContainerSelector : IContainerSelector
	{
		public DummyContainerSelector(CommonContainer[] containersToReturn)
		{
			this.containersToReturn = containersToReturn;
		}

		readonly CommonContainer[] containersToReturn;

		public Either<string, CommonContainer[]> SelectContainers(CommonContainer[] containers, ContainerSelectorMode mode = ContainerSelectorMode.Print) => containersToReturn;
	}
}
