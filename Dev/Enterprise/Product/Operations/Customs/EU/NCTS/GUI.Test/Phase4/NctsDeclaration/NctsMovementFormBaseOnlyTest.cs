using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsMovementForm))]
	class NctsMovementFormBaseOnlyTest : NctsMovementFormAbstractTest<NctsHeader>
	{
		[RequiresSTA]
		public void TestCustomsOfficesSyncWithCustomsOfficesForDeparture()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			header.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			header.CustomsOfficesForDeparture.AddNew("DEP", "FR002300");
			header.CustomsOfficesForDeparture.AddNew("DES", "FR000010");

			using (var form = new NctsMovementForm(header))
			{
				form.Show();
				var grid = form.FindSingle<ZGrid>("CustomsOfficesGrid");
				grid.ListManager.AddNew();
				var current = (NctsEuOfficeCode)grid.ListManager.Current;
				current.CY_Code = "TRA";
				grid.ListManager.EndCurrentEdit();
				current.CY_Data = "DE00100";
				AssertEquals(3, header.CustomsOffices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[]
				{
					"DEP", "DES", "TRA"
				}, header.CustomsOffices.Select(x => x.CY_Code));
			}
		}

		public void TestCustomsOfficesSyncWithCustomsOffices()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			header.CustomsOffices.RemoveAndDeleteAll();
			header.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			header.CustomsOffices.AddNew("DEP", "FR002300");
			header.CustomsOffices.AddNew("DES", "FR000010");

			using (var form = new NctsMovementForm(header))
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				grid.Columns.AddTextColumn("CustomsOffices", 100);
				grid.SetDataBinding(header, "CustomsOffices");
				grid.ListManager.AddNew();
				var current = (NctsEuOfficeCode)grid.ListManager.Current;
				AssertEquals(2, header.CustomsOfficesForDeparture.Count);

				current.CY_Code = "TRA";
				grid.ListManager.EndCurrentEdit();
				current.CY_Data = "DE00100";
				AssertEquals(3, header.CustomsOfficesForDeparture.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[]
				{
					"DEP", "DES", "TRA"
				}, header.CustomsOfficesForDeparture.Select(x => x.CY_Code));
			}
		}

		[RequiresSTA]
		public void TestAddCreateArrivalDataFromDepartureMenuOption()
		{
			void AssertActionsMenuItem(bool isNull, ZString movementType)
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(movementType);
				using (var form = new NctsMovementForm(header))
				{
					form.Show();
					var menu = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First();
					var optionText = "Create Arrival Data from Departure";

					if (isNull)
					{
						AssertNull($"'{optionText}' option must be null for {movementType}", menu.MenuItems.FindByText(optionText));
					}
					else
					{
						AssertNotNull($"'{optionText}' option must be not null for {movementType}", menu.MenuItems.FindByText(optionText));
					}
				}
			}

			AssertActionsMenuItem(true, NctsMovementType.Codes.Departure);
			AssertActionsMenuItem(false, NctsMovementType.Codes.Arrival);
			AssertActionsMenuItem(false, NctsMovementType.Codes.DepartureAndArrival);
		}

		[RequiresSTA]
		public void TestCreateArrivalDataFromDepartureInTheSameDeclaration()
		{
			CombineAssertions(() =>
			{
				header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				var goodsItems = header.MovementHeader.GoodsItems;
				var goodsItem = goodsItems.AddNew();
				goodsItem.BY_Description = "Test1";

				var goodsItem2 = goodsItems.AddNew();
				goodsItem2.BY_Description = "Test2";

				AssertEquals("Prereq: 2 departure goods items", 2, goodsItems.Count);
				AssertEquals("Prereq: 0 arrival goods items", 0, header.ArrivalMovementHeader.GoodsItems.Count);

				using (var form = new NctsMovementForm(header))
				{
					form.Show();
					var menu = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First();
					var menuOption = menu.MenuItems.FindByText("Create Arrival Data from Departure");
					menuOption.PerformClick();

					var arrivalGoodsItems = header.ArrivalMovementHeader.GoodsItems;
					AssertEquals("expected 2 arrival goods items", 2, arrivalGoodsItems.Count);
					AssertEquals("expected same description for the first goodsItem", "Test1", arrivalGoodsItems[0].BY_Description);
					AssertEquals("expected same description for the second goodsItem", "Test2", arrivalGoodsItems[1].BY_Description);
				}
			});
		}

		public void TestCreateArrivalDataFromDepartureByMRN()
		{
			CombineAssertions(() =>
			{
				var headerDeparture = Factory.New<NctsHeader>();
				headerDeparture = Factory.New<NctsHeader>();
				headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);

				var cusEntryNumber = headerDeparture.Factory.New<CusEntryNumber>();
				cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				cusEntryNumber.CE_ParentID = headerDeparture.PK;
				cusEntryNumber.CE_ParentTable = headerDeparture.TableName;
				cusEntryNumber.CE_EntryNum = "MRN123";

				AssertEquals("Prereq: MovementReferenceNumber not empty", "MRN123", headerDeparture.MovementReferenceNumber);

				var headerArrival = Factory.New<NctsHeader>();
				headerArrival = Factory.New<NctsHeader>();
				headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);

				var cusEntryNumberArrival = headerArrival.Factory.New<CusEntryNumber>();
				cusEntryNumberArrival.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				cusEntryNumberArrival.CE_ParentID = headerArrival.PK;
				cusEntryNumberArrival.CE_ParentTable = headerArrival.TableName;
				cusEntryNumberArrival.CE_EntryNum = "MRN123";

				AssertEquals("Prereq: MovementReferenceNumber not empty", "MRN123", headerArrival.MovementReferenceNumber);

				Factory.Save();

				var company1 = Factory.New<GlbCompany>();
				company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_GC = company1.PK;

				headerDeparture.BH_GB = branch1.PK;

				var company2 = Factory.New<GlbCompany>();
				company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_GC = company2.PK;

				headerArrival.BH_GB = branch2.PK;

				using (var form = new NctsMovementForm(headerArrival))
				{
					form.Show();
					var menu = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First();
					var menuOption = menu.MenuItems.FindByText("Create Arrival Data from Departure");
					menuOption.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Use this departure’s data for populating Arrival Goods Items?"));
				}
			});
		}

		public void TestCreateArrivalDataFromDepartureDESisCopyToDSA()
		{
			CombineAssertions(() =>
			{
				header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

				var office = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				office.CY_Data = "Off1";

				var office2 = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				office2.CY_Data = "Off2";

				using (var form = new NctsMovementForm(header))
				{
					form.Show();
					var menu = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First();
					var menuOption = menu.MenuItems.FindByText("Create Arrival Data from Departure");
					menuOption.PerformClick();

					var arrival = header.ArrivalMovementHeader;
					Assert("If arrrival movement header has been created after the clik, then a new DSA office should have been created with value from DES office"
						, header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival && x.CY_Data == "Off1"));
				}
			});
		}

		[RequiresSTA]
		public void TestArrivalDataFromDepartureByMRNNotFound()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			var cusEntryNumberArrival = header.Factory.New<CusEntryNumber>();
			cusEntryNumberArrival.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumberArrival.CE_ParentID = header.PK;
			cusEntryNumberArrival.CE_ParentTable = header.TableName;
			cusEntryNumberArrival.CE_EntryNum = "MRN123";

			AssertEquals("Prereq: MovementReferenceNumber not empty", "MRN123", header.MovementReferenceNumber);

			using (var form = new NctsMovementForm(header))
			{
				form.Show();
				var menu = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First();
				var menuOption = menu.MenuItems.FindByText("Create Arrival Data from Departure");
				menuOption.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("No matching departure for MRN MRN123 found."));
			}
		}

		[RequiresSTA]
		public void TestCaption()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertContains("NCTS Transit Movement", form.Text);
			}
		}

		public void TestEdocsAndDocuments()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				Assert(form.SupportsEdocsExposed);
			}
		}

		public void TestSecurityVisibility()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var securityTab = form.SecurityTabPageExposed;
				Assert(!securityTab.TabVisible);
				header.BH_FTZMove = true;
				Assert("SecurityTab should NOT be visible when BH_FTZMove is true", securityTab.TabVisible);
				(securityTab.Parent as ZTabControl).SelectedTab = securityTab;
				header.MovementHeader.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
				header.MovementHeader.Validation.ValidateBM_PlaceOfUnloading();
				AssertHasMessageErrors("Should has a message error", header.MovementHeader.BM_PlaceOfUnloadingInfo);
				(securityTab.Parent as ZTabControl).SelectedTab = form.MainTabPageExposed;
				header.BH_FTZMove = false;
				Assert("SecurityTab should NOT be visible when BH_FTZMove is false", !securityTab.TabVisible);
			}
			header.BH_FTZMove = false;
			header.MovementHeader.GoodsItems.AddNew();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.GoodsItemsTabPageExposed.Select();
				form.GoodsItemsTabPageExposed.Show();
				var securityTab = form.NctsGoodsItemsUserControl.ItemSecurityTabPage;
				Assert(!securityTab.TabVisible);

				var giControl = (NctsGoodsItemsUserControl)form.Controls.Find("NctsGoodsItemsUserControl", true)[0];
				var giGrid = (ZGrid)(giControl.Controls.Find("GoodsItemsGrid", true)[0]);
				Assert(!giGrid.Columns.Contains(NctsCommonCargoDesc.Schema.BY_CommercialReferenceNumber));
				Assert(!giGrid.Columns.Contains(NctsCommonCargoDesc.Schema.BY_TransportChargesMethodOfPayment));
				header.BH_FTZMove = true;
				Assert(securityTab.TabVisible);
				Assert(giGrid.Columns.Contains(NctsCommonCargoDesc.Schema.BY_CommercialReferenceNumber));
				Assert(giGrid.Columns.Contains(NctsCommonCargoDesc.Schema.BY_TransportChargesMethodOfPayment));
			}
		}

		public void TestSimplifiedTransportVisibility()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var detailsTab = form.MainTabPageExposed;
				detailsTab.Select();
				detailsTab.Show();
				var normalGroupBox = detailsTab.Controls.Find("GoodsLocationNormalGroupBox", true)[0];
				var simplifiedGroupBox = detailsTab.Controls.Find("GoodsLocationSimplifiedGroupBox", true)[0];
				Assert(normalGroupBox.Visible);
				Assert(!simplifiedGroupBox.Visible);
				header.MovementHeader.IsSimplifiedNctsProcedure = true;
				Assert(!normalGroupBox.Visible);
				Assert(simplifiedGroupBox.Visible);
				AssertEquals(normalGroupBox.Top, simplifiedGroupBox.Top);
				header.MovementHeader.IsSimplifiedNctsProcedure = false;
				Assert(normalGroupBox.Visible);
				Assert(!simplifiedGroupBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestGuaranteeColumnHeaders()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var detailsTab = form.MainTabPageExposed;
				detailsTab.Select();
				detailsTab.Show();
				var grid = (ZGrid)detailsTab.Controls.Find("GuaranteesGrid", true)[0];
				var style = grid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_Password);
				AssertEquals("PIN", style.CaptionResourceString.ShortCaption);
			}
		}

		[RequiresSTA]
		public void TestGuaranteeLiabilityFractionColumn()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var detailsTab = form.MainTabPageExposed;
				detailsTab.Select();
				detailsTab.Show();
				var grid = (ZGrid)detailsTab.Controls.Find("GuaranteesGrid", true)[0];
				var style = grid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_SuretyCode);
				AssertEquals("LiabilityFraction should be visible", true, style.IsVisible);
			}
		}

		public void TestDepartureVisibilityFromArrival()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var departureTab = form.MainTabPageExposed;
				Assert("DepartureTab should NOT be visible when movement type is arrival", !departureTab.TabVisible);

				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				Factory.Save();
				Assert("DepartureTab should be visible when movement type is departure and arrival combined", departureTab.TabVisible);
			}
		}

		public void TestMiscOptionsUserControlType()
		{
			using (var form = new NctsDeclarationFormForTestWithMiscOptionsTabPageVisible(header))
			{
				form.Show();
				var miscOptionsDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("MiscOptionsTabDynamicUserControl");
				AssertType<MiscOptionsUserControl>(miscOptionsDynamicUserControl.HostedControl);
			}
		}

		public void TestMiscOptionsTabPageVisibility()
		{
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var miscOptionsTabPage = form.MiscOptionsTabPageExposed;
				Assert("Not visible", !miscOptionsTabPage.TabVisible);
			}

			using (var form = new NctsDeclarationFormForTestWithMiscOptionsTabPageVisible(header))
			{
				form.Show();
				var miscOptionsTabPage = form.MiscOptionsTabPageExposed;
				Assert("Visible", miscOptionsTabPage.TabVisible);
			}
		}

		public void TestDeclarationStatusTabUserControlType()
		{
			using (var form = new NctsDeclarationFormForTestWithStatusTabPageVisible(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabControlExposed.GetTabPage("StatusTabPage"));

				var declarationStatusDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("DeclarationStatusTabDynamicUserControl");
				AssertType<DeclarationStatusTabUserControl>(declarationStatusDynamicUserControl.HostedControl);
			}
		}

		public void TestDeclarationStatusTabPageVisibility()
		{
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var statusTabPage = form.StatusTabPageExposed;
				AssertNotNull("StatusTabPage", statusTabPage);
				Assert("Not visible", !statusTabPage.TabVisible);
			}

			using (var form = new NctsDeclarationFormForTestWithStatusTabPageVisible(header))
			{
				form.Show();
				var statusTabPage = form.StatusTabPageExposed;
				AssertNotNull("StatusTabPage", statusTabPage);
				Assert("Visible", statusTabPage.TabVisible);
			}
		}

		public void TestMessagesUserControlType()
		{
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				var messagesTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("MessagesTabDynamicUserControl");
				AssertType<MessagesTabUserControl>(messagesTabDynamicUserControl.HostedControl);
				AssertEquals("Messages", messagesTabDynamicUserControl.BindingSource.DataMember);
			}
		}

		public void TestToggleReadOnlyUnloadingRemarksTabPage()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.UnloadingRemarksTabPageExposed);
				CombineAssertions(() =>
				{
					var otherNotesTextBox = form.FindSingle<ZTextBox>("OtherNotesTextBox");
					AssertEquals("Default OtherNotesTextBox.ReadOnly value", false, otherNotesTextBox.ReadOnly);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;
					AssertEquals("When the whole UnloadingRemarksTabPage should be locked, OtherNotesTextBox.ReadOnly", true, otherNotesTextBox.ReadOnly);
					header.EffectiveMessageStatus = "";
					AssertEquals("When the whole UnloadingRemarksTabPage should be unlocked, OtherNotesTextBox.ReadOnly", false, otherNotesTextBox.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyArrivalNotificationTabPage()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.ArrivalNotificationTabPageExposed);
				CombineAssertions(() =>
				{
					var cnrTextBox = form.FindSingle<ZTextBox>("zTextBoxCRN");
					AssertEquals("Default zTextBoxCRN.ReadOnly value", false, cnrTextBox.ReadOnly);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
					AssertEquals("When the whole ArrivalNotificationTabPage should be locked, zTextBoxCRN.ReadOnly", true, cnrTextBox.ReadOnly);
					header.EffectiveMessageStatus = "";
					AssertEquals("When the whole ArrivalNotificationTabPage should be unlocked, zTextBoxCRN.ReadOnly", false, cnrTextBox.ReadOnly);
				});
			}
		}

		public void TestToggleReadOnlyMainTabPage()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabPageExposed);
				CombineAssertions(() =>
				{
					var declarationTypeDropEdit = form.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
					AssertEquals("Default DeclarationTypeDropEdit.ReadOnly value", false, declarationTypeDropEdit.ReadOnly);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
					header.EffectiveMessageStatus = "";
					AssertEquals("When the whole MainTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);
				});
			}
		}

		public void TestToggleReadOnlySecurityTabPage()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_FTZMove = true;
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.SecurityTabPageExposed);
				CombineAssertions(() =>
				{
					var specificCircumstanceIndicatorDropEdit = form.FindSingle<ZDropEdit>("SpecificCircumstanceIndicatorDropEdit");
					AssertEquals("Default SpecificCircumstanceIndicatorDropEdit.ReadOnly value", false, specificCircumstanceIndicatorDropEdit.ReadOnly);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole SecurityTabPage should be locked, SpecificCircumstanceIndicatorDropEdit.ReadOnly", true, specificCircumstanceIndicatorDropEdit.ReadOnly);
					header.EffectiveMessageStatus = "";
					AssertEquals("When the whole SecurityTabPage should be unlocked, SpecificCircumstanceIndicatorDropEdit.ReadOnly", false, specificCircumstanceIndicatorDropEdit.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyMiscOptionsTabPage()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new NctsDeclarationFormForTestWithMiscOptionsTabPageVisible(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MiscOptionsTabPageExposed);
				CombineAssertions(() =>
				{
					var branchFindBox = form.FindSingle<ZGuidFindBox>("BranchFindBox");
					AssertEquals("Default BranchFindBox.ReadOnly value", false, branchFindBox.ReadOnly);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole MiscOptionsTabPage should be locked, BranchFindBox.ReadOnly", true, branchFindBox.ReadOnly);
					header.EffectiveMessageStatus = "";
					AssertEquals("When the whole MiscOptionsTabPage should be unlocked, BranchFindBox.ReadOnly", false, branchFindBox.ReadOnly);
				});
			}
		}

		public void TestToggleReadOnlyGoodsItemsTabPage()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.GoodsItems.AddNew();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.GoodsItemsTabPageExposed);
				CombineAssertions(() =>
				{
					var descriptionOfGoodsTextBox = form.FindSingle<ZTextBox>("DescriptionOfGoodsTextBox");
					AssertEquals("Default DescriptionOfGoodsTextBox.ReadOnly value", false, descriptionOfGoodsTextBox.ReadOnly);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole GoodsItemsTabPage should be locked, DescriptionOfGoodsTextBox.ReadOnly", true, descriptionOfGoodsTextBox.ReadOnly);
					header.EffectiveMessageStatus = "";
					AssertEquals("When the whole GoodsItemsTabPage should be unlocked, DescriptionOfGoodsTextBox.ReadOnly", false, descriptionOfGoodsTextBox.ReadOnly);
				});
			}
		}

		public void TestShowMovementTabs()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsDeclarationFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabPageExposed);
				CombineAssertions(() =>
				{
					var declarationTypeDropEdit = form.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
					AssertEquals("Default DeclarationTypeDropEdit.ReadOnly value", false, declarationTypeDropEdit.ReadOnly);

					var newFactory = new BusinessObjectFactory();
					var newFactoryHeader = newFactory.Load<NctsHeader>(header.PK);
					newFactoryHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					newFactory.Save();
					form.ShowMovementTabs();
					AssertEquals("When the whole MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
					newFactoryHeader.EffectiveMessageStatus = "";
					newFactory.Save();
					form.ShowMovementTabs();
					AssertEquals("When the whole MainTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestDocDataPlugInIfDepartureMovement()
		{
			AssertDocDataPlugInBasedOnMovementTypeAndNctsConfiguration(NctsMovementType.Codes.Departure, isDocDataPlugInExpected: true);
		}

		[RequiresSTA]
		public void TestDocDataPlugInIfNotDepartureMovement()
		{
			AssertDocDataPlugInBasedOnMovementTypeAndNctsConfiguration(NctsMovementType.Codes.Arrival, isDocDataPlugInExpected: false);
		}

		void AssertDocDataPlugInBasedOnMovementTypeAndNctsConfiguration(ZString movementType, ZBool isDocDataPlugInExpected)
		{
			header.SetMovementType(movementType);
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfiguration(Factory, isDocDataPlugInExpected))
			using (var form = new NctsDeclarationFormForTest(header))
			{
				if (isDocDataPlugInExpected)
				{
					AssertNotNull("DocData PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
					AssertNotNull("DocumentVisualizer PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer));
				}
				else
				{
					AssertNull("DocData PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
					AssertNull("DocumentVisualizer PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		}

		NctsHeader header;

		class NctsDeclarationFormForTest : NctsMovementForm
		{
			public NctsDeclarationFormForTest(NctsHeader nctsMovement)
				: base(nctsMovement)
			{ }

			public bool SupportsEdocsExposed => SupportsEDocs;

			public ZTabPage SecurityTabPageExposed => SecurityTabPage;

			public ZTabPage MainTabPageExposed => MainTabPage;

			public NctsGoodsItemsUserControl NctsGoodsItemsUserControl => GoodsItemsTabDynamicUserControl.HostedControl as NctsGoodsItemsUserControl;

			public ZTabPage MiscOptionsTabPageExposed => MiscOptionsTabPage;

			public ZTabPage StatusTabPageExposed => StatusTabPage;

			public ZTemplateTabControl MainTabControlExposed => MainTabControl;

			public ZTabPage UnloadingRemarksTabPageExposed => UnloadingRemarksTabPage;

			public ZTabPage ArrivalNotificationTabPageExposed => ArrivalNotificationTabPage;

			public ZTabPage GoodsItemsTabPageExposed => GoodsItemsTabPage;
		}

		class NctsDeclarationFormForTestWithMiscOptionsTabPageVisible : NctsDeclarationFormForTest
		{
			public NctsDeclarationFormForTestWithMiscOptionsTabPageVisible(NctsHeader nctsMovement) : base(nctsMovement)
			{ }

			protected override ZBool SupportsMiscOptionsTabPage => true;
		}

		class NctsDeclarationFormForTestWithStatusTabPageVisible : NctsDeclarationFormForTest
		{
			public NctsDeclarationFormForTestWithStatusTabPageVisible(NctsHeader nctsMovement) : base(nctsMovement)
			{ }

			protected override ZBool SupportsStatusTabPage => true;
		}
	}
}
