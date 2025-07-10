using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public abstract class FindBoxWrapperWithBorderWiseIntegrationTestCase<TFindBoxPopup> : TransactionedTestCase where TFindBoxPopup : IFindBoxPopup
	{
		public void TestShowModal_WhenNoToolChosen_ShouldDoNothing()
		{
			ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			var data = new AdditionalDataForBorderWise("RIP BW-desk", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("9706");
			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Egypt))
				using (var findBoxWrapper = CreateFindBoxPopup(data))
				{
					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
					AssertShowModalDoesNothingWhenNoExternalToolChosen(findBoxWrapper, findBox);
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		protected virtual void AssertShowModalDoesNothingWhenNoExternalToolChosen(TFindBoxPopup findBoxWrapper, IFindBox findBox)
		{
			AssertNoExceptionThrown(() => findBoxWrapper.ShowModal(findBox, null));
		}

		public void TestShowModal_WhenUsingBorderWiseWeb_ShouldShowWaitingForResponseDialog_AndWhenEdiEntHyperlinkIsExecuted_ShouldReceiveArgumentsAndCloseDialog()
		{
			SetUpDataRegistry(false);
			ZFormModaliser.ShowDialogsInTest = true;
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox(null);
			try
			{
				ZGuid returnHookPK = default;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					returnHookPK = dialog.FormBizo.PK;
					var queryString = FormattableString.Invariant($"edient:Command=ShowEditForm&ControllerID=BorderWiseWebReturnHook&BusinessEntityPK={returnHookPK}&Hash=%2b6DGpkV6EnMFoOENBoYtbmOApzB%2fRMFUE&Args=2901.24.00%7c05");
					try
					{
						AssertNullOrEmpty(findBox.Code);
						AssertEquals(false, dialog.IsDisposed);
					}
					catch
					{
						((IDisposable)form).Dispose(); // Ensure that if the above fails we don't leave the dialog open forever.
						throw;
					}

					ShowEditFormUrlHandler.Instance.Handle(new QueryString(queryString));
					AssertEquals("2901.24.00", dialog.FormBizo.BorderWiseInvoiceLines[0].TariffCode);
					AssertEquals("05", dialog.FormBizo.BorderWiseInvoiceLines[0].StatCode);
					AssertEquals(true, dialog.IsDisposed);
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
					popupWrapper.ShowModal(findBox, null);
					AssertEquals("Should have used the tariff and stat code passed back from BorderWise Web via the edient hyperlink", "2901.24.00 05", findBox.Code);
					AssertNotNull(returnHookPK);
					AssertNotContains("Original URL should not include tariff heading since nothing was selected in the find box originally.", "tariff=", WebUrlLauncher.LastUrlLaunched);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestShowModal_WhenUsingBorderWiseWeb_AndExistingValuePresentInTextBox_ShouldPassThroughExistingTariffDetailsToBorderWise()
		{
			SetUpDataRegistry(false);
			ZFormModaliser.ShowDialogsInTest = true;
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			var factory = new BusinessObjectFactory();
			GlbCompany.GetCurrentCompany(factory).GC_RN_NKCountryCode = "AU";
			factory.Save();
			try
			{
				ZGuid? returnHookPK = null;

				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					returnHookPK = dialog.FormBizo.PK;
					var queryString = FormattableString.Invariant($"edient:Command=ShowEditForm&ControllerID=BorderWiseWebReturnHook&BusinessEntityPK={returnHookPK}&Hash=%2b6DGpkV6EnMFoOENBoYtbmOApzB%2fRMFUE&Args=2901.24.00%7c05");
					try
					{
						AssertEquals("1234.56.78 90", findBox.Code);
						AssertEquals(false, dialog.IsDisposed);
					}
					catch
					{
						((IDisposable)form).Dispose(); // Ensure that if the above fails we don't leave the dialog open forever.
						throw;
					}

					ShowEditFormUrlHandler.Instance.Handle(new QueryString(queryString));
					AssertEquals(true, dialog.IsDisposed);
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
					popupWrapper.ShowModal(findBox, null);
					AssertNotNull("Should have called form shown delegate", returnHookPK);
					
					var actualUrl = WebUrlLauncher.LastUrlLaunched;
					var indexOfHash = actualUrl.IndexOf("%26Hash%3D%");
					if (indexOfHash > 0)
					{
						var nextParameterStart = actualUrl.IndexOf("%26", indexOfHash + 1);
						actualUrl = actualUrl.Substring(0, indexOfHash + 10) + "(hash)" + actualUrl.Substring(nextParameterStart);
					}

					AssertEquals("Should have used the tariff and stat code passed back from BorderWise Web via the edient hyperlink", "2901.24.00 05", findBox.Code);
					AssertContains("Should include tariff heading, country, import/export flag, and return hyperlink so we can navigate to the proper tariff entry. Complete URL including hash: " + WebUrlLauncher.LastUrlLaunched, "tariff=1234.56.78&stat=90&impexp=I&c=AU&ret=", actualUrl);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestShowModal_WhenUsingBorderWiseWeb_CloseDialogBeforeSelectingTariffLine_ShouldNotThrowException()
		{
			SetUpDataRegistry(false);
			ZFormModaliser.ShowDialogsInTest = true;
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				ZGuid? returnHookPK = null;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					returnHookPK = dialog.FormBizo.PK;
					var queryString = FormattableString.Invariant($"edient:Command=ShowEditForm&ControllerID=BorderWiseWebReturnHook&BusinessEntityPK={returnHookPK}&Hash=%2b6DGpkV6EnMFoOENBoYtbmOApzB%2fRMFUE&Args=2901.24.00%7c05");
					dialog.Dispose();
					ShowEditFormUrlHandler.Instance.Handle(new QueryString(queryString));
					AssertEquals(true, dialog.IsDisposed);
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
					AssertNoExceptionThrown(() => popupWrapper.ShowModal(findBox, null));
					AssertNotNull("Should have called form shown delegate", returnHookPK);
					AssertEquals("Should not have altered the tariff code already present in the find box since the dialog was closed before the edient hyperlink was executed", "1234.56.78 90", findBox.Code);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		#region Implementation
		protected abstract TFindBoxPopup CreateFindBoxPopup(AdditionalDataForBorderWise filterData);
		protected virtual IFindBox CreateFindBox(string initialFindBoxCode)
		{
			var findBox = new Mock<IFindBox>();
			findBox.SetupProperty(f => f.Code, initialFindBoxCode);
			return findBox.Object;
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();

			base.TearDown();

			BorderWiseLauncher.BatchModeSettingResponse.Clear();
			OpenedFormCache.GetInstance().CloseAllCachedForms();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		protected void SetUpDataRegistry(bool useWebSocketClient)
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = useWebSocketClient;
		}
		#endregion
	}
}
