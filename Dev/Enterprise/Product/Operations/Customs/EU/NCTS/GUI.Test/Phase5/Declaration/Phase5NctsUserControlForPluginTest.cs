using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.EU.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5NctsUserControlForPluginTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestDeclarationDetailsUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var declarationDetails = control.FindSingleOrDefault<ZTabPage>("DepartureDeclarationTabPage");
					AssertEquals("DeclarationDetails is visible", true, declarationDetails.TabVisible);
					AssertEquals("Caption", "Details", declarationDetails.CaptionResourceString.Caption);
					var declarationDetailsUserControl = control.DeclarationDetailsTabUserControlExposed;
					AssertNotNull("DeclarationDetailsUserControl isn't null", declarationDetailsUserControl);
					AssertType<Phase5DeclarationDetailsTabUserControl>("DeclarationDetailsUserControl type", declarationDetailsUserControl);
				});
			}
		}

		public void TestSynchronizeMonetaryValue_ShouldBeCalledInSetDataBinding()
		{
			// Arrange
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate = NCTSTestHelper.CreateExchangeRate(Factory, 1.5, Core.Constants.CurrencyCodes.UnitedKingdom);
			Factory.Save();

			goodsItem.BY_LinePrice = 15;
			AssertEquals("Precondition", 10m, goodsItem.BY_MonetaryValue);

			exchangeRate.RE_SellRate = 1;

			// Act
			using var control = new Phase5NctsUserControlForPluginForTest(nctsHeader);

			control.SetDataBinding(nctsHeader, "");

			//Assert
			AssertEquals("SynchronizeMonetaryValue is called", 15m, goodsItem.BY_MonetaryValue);
		}

		[RequiresSTA]
		public void TestArrivalUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var arrivalNotification = control.FindSingleOrDefault<ZTabPage>("ArrivalNotificationTabPage");
					AssertEquals("arrivalNotification is visible", true, arrivalNotification.TabVisible);
					var arrivalNotificationUserControl = control.NctsArrivalUserControlExposed;
					AssertNotNull("ArrivalNotificationUserControl isn't null", arrivalNotificationUserControl);
					AssertType<Phase5ArrivalNotificationTabUserControl>("ArrivalNotificationUserControl type", arrivalNotificationUserControl);
				});
			}
		}

		[RequiresSTA]
		public void TestIncidentsUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ExportFlag = YesNoList.Codes.Yes;

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var incidentsNotification = control.FindSingleOrDefault<ZTabPage>("IncidentsTabPage");
					AssertEquals("Incidents is visible", true, incidentsNotification.TabVisible);
					AssertEquals("IncidentsTabPage.Caption", "Incidents", incidentsNotification.CaptionResourceString.Caption);

					var incidentsUserControl = control.IncidentsUserControlExposed;
					AssertNotNull("incidentsUserControl isn't null", incidentsUserControl);
					AssertType<Phase5EventTabUserControl>("incidentsUserControl type", incidentsUserControl);
				});
			}
		}

		public void TestIncidentsTabPage_Invisible()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ExportFlag = YesNoList.Codes.No;

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				var incidentsNotification = control.FindSingleOrDefault<ZTabPage>("IncidentsTabPage");
				AssertNull("Incidents is not visible", incidentsNotification);
			}
		}

		public void TestDeleteIncidentsDialogBox()
		{
			CombineAssertions(() =>
			{
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ExportFlag = YesNoList.Codes.Yes;

				using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
				{
					control.SetDataBinding(nctsHeader, "");

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					nctsHeader.BH_ExportFlag = YesNoList.Codes.No;
					AssertEquals("User prompt should not have a question yet as there was no incident details to delete", false, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

					nctsHeader.BH_ExportFlag = YesNoList.Codes.Yes;
					var incident = nctsHeader.EnRouteIncidents.AddNew();
					incident.BN_EndorsementDate = new CargoWise.Types.ZDateTime(2023, 10, 4);
					nctsHeader.BH_ExportFlag = YesNoList.Codes.No;
					AssertEquals("User prompt has a question", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Prompt text", "Incident details will be deleted. Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		[RequiresSTA]
		public void TestServicesUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var services = control.FindSingleOrDefault<ZTabPage>("ServicesTabPage");
					AssertEquals("services is visible", true, services.TabVisible);
					AssertEquals("ServicesTabPage.Caption", "Services", services.CaptionResourceString.Caption);

					var servicesUserControl = control.ServicesUserControlExposed;
					AssertNotNull("ServicesUserControl isn't null", servicesUserControl);
					AssertType<Phase5DeclarationServicesTabUserControl>("ServicesUserControl type", servicesUserControl);
				});
			}
		}

		public void TestHouseConsignmentsUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var houseConsignments = control.FindSingleOrDefault<ZTabPage>("HouseConsignmentsTabPage");
					AssertEquals("HouseConsignments is visible", true, houseConsignments.TabVisible);
					AssertEquals("HouseConsignmentsTabPage.Caption", "House Consignments", houseConsignments.CaptionResourceString.Caption);

					var houseConsignmentsUserControl = control.HouseConsignmentsUserControlExposed;
					AssertNotNull("HouseConsignmentsUserControl isn't null", houseConsignmentsUserControl);
					AssertType<HouseConsignmentsTabUserControl>("HouseConsignmentsUserControl type", houseConsignmentsUserControl);
				});
			}
		}

		[RequiresSTA]
		public void TestTransportAndPackagingUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var transportAndPackaging = control.FindSingleOrDefault<ZTabPage>("TransportAndPackagingTabPage");
					AssertEquals("TransportAndPackaging is visible", true, transportAndPackaging.TabVisible);
					AssertEquals("TransportAndPackagingTabPage.Caption", "Transport && Containers", transportAndPackaging.CaptionResourceString.Caption);

					var transportAndPackagingUserControl = control.TransportAndPackagingUserControlExposed;
					AssertNotNull("TransportAndPackagingUserControl isn't null", transportAndPackagingUserControl);
					AssertType<Phase5TransportAndPackagingTabUserControl>("TransportAndPackagingUserControl type", transportAndPackagingUserControl);
				});
			}
		}

		public void TestUnloadingRemarksUserControl()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var unloadingRemarks = control.FindSingleOrDefault<ZTabPage>("UnloadingRemarksTabPage");
					AssertEquals("UnloadingRemarks is visible", true, unloadingRemarks.TabVisible);
					var unloadingRemarksUserControl = control.UnloadingRemarksUserControlExposed;
					AssertNotNull("UnloadingRemarksUserControl isn't null", unloadingRemarksUserControl);
					AssertType<Phase5UnloadingRemarksTabUserControl>("UnloadingRemarksUserControl type", unloadingRemarksUserControl);
				});
			}
		}

		[RequiresSTA]
		public void TestMiscOptionsTabPageWhenArrival()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(Factory, true))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var miscTabPage = control.FindSingleOrDefault<ZTabPage>("MiscOptionsTabPage");
					AssertNull("MiscOptionsTabPage is invisible", miscTabPage);
				});
			}
		}

		[RequiresSTA]
		public void TestMiscOptionsTabPageWhenIsSupported()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(Factory, true))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var miscTabPage = control.FindSingleOrDefault<ZTabPage>("MiscOptionsTabPage");
					AssertEquals("MiscOptionsTabPage is visible", true, miscTabPage.TabVisible);
					AssertEquals("MiscOptionsTabPage.Caption", "Misc.", miscTabPage.CaptionResourceString.Caption);

					var miscOptionsUserControl = control.FindSingleOrDefault<Phase5DeclarationMiscTabUserControl>("MiscOptionsUserControl");
					AssertNotNull("MiscOptionsUserControl is not null", control.MiscOptionsUserControlExposed);
				});
			}
		}

		public void TestMessagesUserControl_Arrival()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using var form = new ZForm(nctsHeader);
			using var control = new Phase5NctsUserControlForPluginForTest(nctsHeader);
			form.Controls.Add(control);
			control.SetDataBinding(nctsHeader, "");
			form.Show();
			CombineAssertions(() =>
			{
				var messagesTabPage = control.FindSingleOrDefault<ZTabPage>("MessagesTabPage");
				control.MainTabControlExposed.SelectTab(messagesTabPage);

				AssertEquals("MessagesTabPage is visible", true, messagesTabPage.TabVisible);
				AssertEquals("MessagesTabPage.Caption", "Messages", messagesTabPage.CaptionResourceString.Caption);

				var messagesUserControl = control.FindSingle<MessagesTabUserControl>("MessagesUserControl");
				AssertEquals("MessagesUserControl Binding", "Messages", messagesUserControl.BindingSource.DataMember);
			});
		}

		public void TestMessagesUserControl_Departure()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			using var form = new ZForm(nctsHeader);
			using var control = new Phase5NctsUserControlForPluginForTest(nctsHeader);
			form.Controls.Add(control);
			control.SetDataBinding(nctsHeader, "");
			form.Show();
			var messagesTabPage = control.FindSingleOrDefault<ZTabPage>("MessagesTabPage");
			control.MainTabControlExposed.SelectTab(messagesTabPage);
			var messagesUserControl = control.FindSingle<MessagesTabUserControl>("MessagesUserControl");
			AssertEquals("MessagesUserControl Binding", "MovementHeader.MessagesForDisplay", messagesUserControl.BindingSource.DataMember);
		}

		[RequiresSTA]
		public void TestMiscTabPageWhenIsNotSupported()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var control = new Phase5NctsUserControlForPluginForTest(nctsHeader))
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(Factory, false))
			{
				control.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					var miscTabPage = control.FindSingleOrDefault<ZTabPage>("MiscOptionsTabPage");
					AssertNull("MiscOptionsTabPage is invisible", miscTabPage);
					AssertNull("MiscUserControl is null", control.MiscOptionsUserControlExposed);
				});
			}
		}

		[RequiresSTA]
		public void TestISupportMultipleResourceStringDataSupporterMembers()
		{
			using var userControl = new Phase5NctsUserControlForPlugin(nctsHeader);
			userControl.SetDataBinding(nctsHeader, "");

			var multipleResourceStringDataSupporter = userControl as ISupportMultipleResourceStringDataSupporter;
			AssertNotNull("UserControl as ISupportMultipleResourceStringDataSupporter", multipleResourceStringDataSupporter);
			AssertSame("NctsHeader and SupportMultipleResourceStringData Member", nctsHeader, multipleResourceStringDataSupporter.SupportMultipleResourceStringData);
		}

		public void TestTabPagesOrderOnDepartureForm()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var expectedTabPagesInOrder = new string[]
			{
				"DepartureDeclarationTabPage", "ServicesTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage",
				"MiscOptionsTabPage","MessagesTabPage"
			};

			using (TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(miscTabPageSupport: true))
			{
				AssertTabPagesOrder(expectedTabPagesInOrder);
			}
		}

		void AssertTabPagesOrder(string[] expectedTabPagesInOrder)
		{
			using (var form = new Phase5NctsUserControlForPlugin(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl");
				AssertSequencesEqual("TabPageNames", expectedTabPagesInOrder, mainTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));
			}
		}

		IDisposable TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(bool miscTabPageSupport)
		{
			return NctsConfigurationTestHelper.TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(Factory, miscTabPageSupport);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
		}

		public NctsHeader nctsHeader;
	}

	sealed class Phase5NctsUserControlForPluginForTest : Phase5NctsUserControlForPlugin
	{
		public Phase5NctsUserControlForPluginForTest(NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		public ZUserControl DeclarationDetailsTabUserControlExposed => DeclarationDetailsTabUserControl;

		public ZUserControl NctsArrivalUserControlExposed => NctsArrivalUserControl;

		public ZUserControl UnloadingRemarksUserControlExposed => UnloadingRemarksUserControl;

		public ZUserControl IncidentsUserControlExposed => IncidentsUserControl;

		public ZUserControl ServicesUserControlExposed => ServicesUserControl;

		public ZUserControl TransportAndPackagingUserControlExposed => TransportAndPackagingUserControl;

		public ZUserControl HouseConsignmentsUserControlExposed => HouseConsignmentsUserControl;

		public ZUserControl MiscOptionsUserControlExposed => MiscOptionsUserControl;
		public ZTabControl MainTabControlExposed => MainTabControl;

		public ZTabPage IncidentsTabPageExposed => IncidentsTabPage;
	}
}
