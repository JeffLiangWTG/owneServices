using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class NctsUserControlForPluginTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestDepartureDeclarationTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var departureDeclarationTabPage = control.FindSingleOrDefault<ZTabPage>("DepartureDeclarationTabPage");
					AssertEquals("DepartureDeclarationTabPage is visible", true, departureDeclarationTabPage.TabVisible);
					AssertNotNull("DeclarationDetailsTabUserControl isn't null", control.DeclarationDetailsTabUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestDepartureDeclarationTabPage_Invisible()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var departureDeclarationTabPage = control.FindSingleOrDefault<ZTabPage>("DepartureDeclarationTabPage");
					AssertNull("DepartureDeclarationTabPage is invisible", departureDeclarationTabPage);
					AssertNull("DeclarationDetailsTabUserControl is null", control.DeclarationDetailsTabUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestGoodsItemsTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPlugin(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var goodsItemsTabPage = control.FindSingleOrDefault<ZTabPage>("GoodsItemsTabPage");
					AssertEquals("GoodsItemsTabPage is visible", true, goodsItemsTabPage.TabVisible);
					AssertNotNull("NctsGoodsItemsUserControl isn't null", control.NctsGoodsItemsUserControl);
				});
			}
		}

		[RequiresSTA]
		public void TestGoodsItemsTabPage_Invisible()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new NctsUserControlForPlugin(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var goodsItemsTabPage = control.FindSingleOrDefault<ZTabPage>("GoodsItemsTabPage");
					AssertNull("GoodsItemsTabPage is invisible", goodsItemsTabPage);
					AssertNull("NctsGoodsItemsUserControl is null", control.NctsGoodsItemsUserControl);
				});
			}
		}

		[RequiresSTA]
		public void TestSecurityTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_FTZMove = true;

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var securityTabPage = control.FindSingleOrDefault<ZTabPage>("SecurityTabPage");
					AssertEquals("SecurityTabPage is visible", true, securityTabPage.TabVisible);
					var securityTabUserControl = control.SecurityTabUserControlExposed;
					AssertNotNull("SecurityTabUserControl isn't null", securityTabUserControl);
					AssertType<SecurityTabUserControl>("SecurityTabUserControl type", securityTabUserControl);
				});
			}
		}

		[RequiresSTA]
		public void TestSecurityTabPage_Invisible()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var securityTabPage = control.FindSingleOrDefault<ZTabPage>("SecurityTabPage");
					AssertNull("SecurityTabPage is invisible", securityTabPage);
					AssertNull("SecurityTabUserControl is null", control.SecurityTabUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestArrivalNotificationTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var arrivalNotificationTabPage = control.FindSingleOrDefault<ZTabPage>("ArrivalNotificationTabPage");
					AssertEquals("ArrivalNotificationTabPage is visible", true, arrivalNotificationTabPage.TabVisible);
					AssertNotNull("NctsArrivalUserControl isn't null", control.NctsArrivalUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestArrivalNotificationTabPage_Invisible()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var arrivalNotificationTabPage = control.FindSingleOrDefault<ZTabPage>("ArrivalNotificationTabPage");
					AssertNull("ArrivalNotificationTabPage is invisible", arrivalNotificationTabPage);
					AssertNull("NctsArrivalUserControl is null", control.NctsArrivalUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestUnloadingRemarksTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var unloadingRemarksTabPage = control.FindSingleOrDefault<ZTabPage>("UnloadingRemarksTabPage");
					AssertEquals("UnloadingRemarksTabPage is visible", true, unloadingRemarksTabPage.TabVisible);
					AssertNotNull("UnloadingRemarksUserControl isn't null", control.UnloadingRemarksUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestUnloadingRemarksTabPage_Invisible()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var unloadingRemarksTabPage = control.FindSingleOrDefault<ZTabPage>("UnloadingRemarksTabPage");
					AssertNull("UnloadingRemarksTabPage is invisible", unloadingRemarksTabPage);
					AssertNull("UnloadingRemarksUserControl is null", control.UnloadingRemarksUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestMessagesTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			using var form = new ZForm(nctsHeader);
			using var control = new NctsUserControlForPluginForTest(nctsHeader);

			form.Controls.Add(control);
			control.SetDataBinding(nctsHeader, "");
			form.Show();
			CombineAssertions(() =>
			{
				var messagesTabPage = control.FindSingleOrDefault<ZTabPage>("MessagesTabPage");
				control.MainTabControlExposed.SelectTab(messagesTabPage);

				AssertEquals("MessagesTabPage is visible", true, messagesTabPage.TabVisible);
				var messagesUserControl = control.FindSingleOrDefault<MessagesTabUserControl>("MessagesUserControl");
				AssertNotNull("MessagesUserControl isn't null", messagesUserControl);
				AssertEquals("MessagesUserControl binding", "Messages", messagesUserControl.BindingSource.DataMember);
			});
		}

		[RequiresSTA]
		public void TestMiscOptionsTabPageWhenIsSupported()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader, supportsMiscOptionsTabPage: true, supportsStatusTabPage: false))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var miscTabPage = control.FindSingleOrDefault<ZTabPage>("MiscOptionsTabPage");
					AssertEquals("MiscOptionsTabPage is visible", true, miscTabPage.TabVisible);
					var miscOptionsUserControl = control.FindSingleOrDefault<MiscOptionsUserControl>("MiscOptionsUserControl");
					AssertNotNull("MiscOptionsUserControl is not null", control.MiscOptionsUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestMiscTabPageWhenIsNotSupported()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader, supportsMiscOptionsTabPage: false, supportsStatusTabPage: false))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var miscTabPage = control.FindSingleOrDefault<ZTabPage>("MiscTabPage");
					AssertNull("MiscTabPage is invisible", miscTabPage);
					AssertNull("MiscUserControl is null", control.MiscOptionsUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestStatusTabPageWhenIsSupported()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader, supportsMiscOptionsTabPage: false, supportsStatusTabPage: true))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var statusTabPage = control.FindSingleOrDefault<ZTabPage>("StatusTabPage");
					AssertEquals("StatusTabPage is visible", true, statusTabPage.TabVisible);
					var declarationStatusUserControl = control.FindSingleOrDefault<DeclarationStatusTabUserControl>("DeclarationStatusUserControl");
					AssertNotNull("DeclarationStatusUserControl is not null", control.DeclarationStatusUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestStatusTabPageWhenIsNotSupported()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPluginForTest(nctsHeader, supportsMiscOptionsTabPage: false, supportsStatusTabPage: false))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var statusTabPage = control.FindSingleOrDefault<ZTabPage>("StatusTabPage");
					AssertNull("StatusTabPage is invisible", statusTabPage);
					AssertNull("DeclarationStatusUserControl is null", control.DeclarationStatusUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestDepartureDeclarationTabPageIsScrollable()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPlugin(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var departureDeclarationTabPage = control.FindSingleOrDefault<ZTabPage>("DepartureDeclarationTabPage");
					AssertEquals("DepartureDeclarationTabPage is scrollable", true, departureDeclarationTabPage.AutoScroll);
					AssertNotEquals("DepartureDeclarationTabPage AutoScrollMinSize not zero", new Size(0, 0), departureDeclarationTabPage.AutoScrollMinSize);
				});
			}
		}

		[RequiresSTA]
		public void TestMainTabControlDockFill()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new NctsUserControlForPlugin(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var mainTabControl = control.FindSingleOrDefault<ZTabControl>("MainTabControl");
					AssertEquals("MainTabControll Dock property set to fill", DockStyle.Fill, mainTabControl.Dock);
				});
			}
		}

		public void TestSupportsMiscOptionsTabPage()
		{
			using (var control = new NctsUserControlForPluginForFlagsTest(nctsHeader))
			{
				AssertEquals("SupportsMiscOptionsTabPage", false, control.SupportsMiscOptionsTabPageExposed);
			}
		}

		public void TestSupportsStatusTabPage()
		{
			using (var control = new NctsUserControlForPluginForFlagsTest(nctsHeader))
			{
				AssertEquals("SupportsStatusTabPage", false, control.SupportsStatusTabPageExposed);
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyUnloadingRemarksTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				CombineAssertions(() =>
				{
					var otherNotesTextBox = control.UnloadingRemarksUserControlExposed.FindSingle<ZTextBox>("OtherNotesTextBox");
					AssertEquals("Default OtherNotesTextBox.ReadOnly value", false, otherNotesTextBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;
					AssertEquals("When the whole UnloadingRemarksTabPage should be locked, OtherNotesTextBox.ReadOnly", true, otherNotesTextBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = "";
					AssertEquals("When the whole UnloadingRemarksTabPage should be unlocked, OtherNotesTextBox.ReadOnly", false, otherNotesTextBox.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyArrivalNotificationTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				CombineAssertions(() =>
				{
					var cnrTextBox = form.FindSingle<ZTextBox>("zTextBoxCRN");
					AssertEquals("Default zTextBoxCRN.ReadOnly value", false, cnrTextBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
					AssertEquals("When the whole ArrivalNotificationTabPage should be locked, zTextBoxCRN.ReadOnly", true, cnrTextBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = "";
					AssertEquals("When the whole ArrivalNotificationTabPage should be unlocked, zTextBoxCRN.ReadOnly", false, cnrTextBox.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyDepartureDeclarationTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				CombineAssertions(() =>
				{
					var declarationTypeDropEdit = form.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
					AssertEquals("Default DeclarationTypeDropEdit.ReadOnly value", false, declarationTypeDropEdit.ReadOnly);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole DepartureDeclarationTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
					nctsHeader.EffectiveMessageStatus = "";
					AssertEquals("When the whole DepartureDeclarationTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlySecurityTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_FTZMove = true;
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				control.MainTabControlExposed.SelectTab(control.SecurityTabPageExposed);
				CombineAssertions(() =>
				{
					var specificCircumstanceIndicatorDropEdit = form.FindSingle<ZDropEdit>("SpecificCircumstanceIndicatorDropEdit");
					AssertEquals("Default SpecificCircumstanceIndicatorDropEdit.ReadOnly value", false, specificCircumstanceIndicatorDropEdit.ReadOnly);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole SecurityTabPage should be locked, SpecificCircumstanceIndicatorDropEdit.ReadOnly", true, specificCircumstanceIndicatorDropEdit.ReadOnly);
					nctsHeader.EffectiveMessageStatus = "";
					AssertEquals("When the whole SecurityTabPage should be unlocked, SpecificCircumstanceIndicatorDropEdit.ReadOnly", false, specificCircumstanceIndicatorDropEdit.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyMiscOptionsTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader, supportsMiscOptionsTabPage: true, supportsStatusTabPage: false))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				control.MainTabControlExposed.SelectTab(control.MiscOptionsTabPageExposed);
				CombineAssertions(() =>
				{
					var branchFindBox = form.FindSingle<ZGuidFindBox>("BranchFindBox");
					AssertEquals("Default BranchFindBox.ReadOnly value", false, branchFindBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole MiscOptionsTabPage should be locked, BranchFindBox.ReadOnly", true, branchFindBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = "";
					AssertEquals("When the whole MiscOptionsTabPage should be unlocked, BranchFindBox.ReadOnly", false, branchFindBox.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestToggleReadOnlyGoodsItemsTabPage()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.GoodsItems.AddNew();
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				control.MainTabControlExposed.SelectTab(control.GoodsItemsTabPage);
				CombineAssertions(() =>
				{
					var descriptionOfGoodsTextBox = form.FindSingle<ZTextBox>("DescriptionOfGoodsTextBox");
					AssertEquals("Default DescriptionOfGoodsTextBox.ReadOnly value", false, descriptionOfGoodsTextBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("When the whole GoodsItemsTabPage should be locked, DescriptionOfGoodsTextBox.ReadOnly", true, descriptionOfGoodsTextBox.ReadOnly);
					nctsHeader.EffectiveMessageStatus = "";
					AssertEquals("When the whole GoodsItemsTabPage should be unlocked, DescriptionOfGoodsTextBox.ReadOnly", false, descriptionOfGoodsTextBox.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestShowMovementTabs()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new ZForm())
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				form.Controls.Add(control);
				control.SetDataBinding(nctsHeader, "");
				form.Show();
				CombineAssertions(() =>
				{
					var declarationTypeDropEdit = form.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
					AssertEquals("Default DeclarationTypeDropEdit.ReadOnly value", false, declarationTypeDropEdit.ReadOnly);

					var newFactory = new BusinessObjectFactory();
					var newFactoryHeader = newFactory.Load<NctsHeader>(nctsHeader.PK);
					newFactoryHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					newFactoryHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					newFactory.Save();
					control.ShowMovementTabs();
					AssertEquals("When the whole MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
					newFactoryHeader.EffectiveMessageStatus = "";
					newFactory.Save();
					control.ShowMovementTabs();
					AssertEquals("When the whole MainTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestShowUnloadingTab()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			Factory.Save();

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var unloadingRemarksTabPage = control.FindSingleOrDefault<ZTabPage>("UnloadingRemarksTabPage");
					AssertNull("Tab is not visible by default", unloadingRemarksTabPage);

					nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
					control.ShowUnloadingTab();
					unloadingRemarksTabPage = control.FindSingleOrDefault<ZTabPage>("UnloadingRemarksTabPage");
					AssertEquals("Tab is visible after calling ShowUnloadingTab", true, unloadingRemarksTabPage.TabVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestShouldCreateUnloadingRemarksUserControl()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");
				AssertEquals("It should be false for EU", false, control.ShouldCreateUnloadingRemarksUserControlExposed);
			}
		}

		[RequiresSTA]
		public void TestNullNctsHeader()
		{
			AssertNoExceptionThrown(() =>
			{
				using (var form = new ZForm())
				using (var control = new NctsUserControlForPluginForTest(null))
				{
					form.Controls.Add(control);

					control.SetDataBinding(null, "");

					form.Show();
				}
			});
		}

		[RequiresSTA]
		public void TestISupportMultipleResourceStringDataSupporterMembers()
		{
			using var userControl = new NctsUserControlForPlugin(nctsHeader);
			userControl.SetDataBinding(nctsHeader, "");

			var multipleResourceStringDataSupporter = userControl as ISupportMultipleResourceStringDataSupporter;
			AssertNotNull("UserControl as ISupportMultipleResourceStringDataSupporter", multipleResourceStringDataSupporter);
			AssertSame("NctsHeader and SupportMultipleResourceStringData Member", nctsHeader, multipleResourceStringDataSupporter.SupportMultipleResourceStringData);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		}
		NctsHeader nctsHeader;
	}

	class NctsUserControlForPluginForTest : NctsUserControlForPlugin
	{
		public NctsUserControlForPluginForTest(NctsHeader nctsMovement, ZBool supportsMiscOptionsTabPage, ZBool supportsStatusTabPage) : base(nctsMovement)
		{
			this.supportsMiscOptionsTabPage = supportsMiscOptionsTabPage;
			this.supportsStatusTabPage = supportsStatusTabPage;
		}
		readonly ZBool supportsMiscOptionsTabPage;
		readonly ZBool supportsStatusTabPage;

		public NctsUserControlForPluginForTest(NctsHeader nctsMovement) : this(nctsMovement, false, false)
		{
		}

		public DeclarationDetailsTabUserControl DeclarationDetailsTabUserControlExposed => DeclarationDetailsTabUserControl;

		public ZTabPage SecurityTabPageExposed => SecurityTabPage;

		public SecurityTabUserControl SecurityTabUserControlExposed => SecurityTabUserControl;

		public NctsArrivalUserControl NctsArrivalUserControlExposed => NctsArrivalUserControl;

		public UnloadingRemarksUserControl UnloadingRemarksUserControlExposed => UnloadingRemarksUserControl;

		public ZTabControl MainTabControlExposed => MainTabControl;

		public ZTabPage GoodsItemsTabPageExposed => GoodsItemsTabPage;

		public ZTabPage MiscOptionsTabPageExposed => MiscOptionsTabPage;

		public MiscOptionsUserControl MiscOptionsUserControlExposed => MiscOptionsUserControl;

		public DeclarationStatusTabUserControl DeclarationStatusUserControlExposed => DeclarationStatusUserControl;

		public bool ShouldCreateUnloadingRemarksUserControlExposed => ShouldCreateUnloadingRemarksUserControl;

		protected override ZBool SupportsMiscOptionsTabPage => supportsMiscOptionsTabPage;
		protected override ZBool SupportsStatusTabPage => supportsStatusTabPage;
	}

	class NctsUserControlForPluginForFlagsTest : NctsUserControlForPlugin
	{
		public NctsUserControlForPluginForFlagsTest(NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		public ZBool SupportsMiscOptionsTabPageExposed => SupportsMiscOptionsTabPage;

		public ZBool SupportsStatusTabPageExposed => SupportsStatusTabPage;
	}
}
