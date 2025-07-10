using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestsSubclassesOf(typeof(ApplicationGUIProvider))]
	public abstract class ApplicationGUIProviderAbstractTest<TGuiProvider, THeader> : ZArchitecture.GUI.Testing.ZFormBasherTest
		where TGuiProvider : ApplicationGUIProvider
		where THeader : AsycudaManifestHeader
	{
		[RequiresSTA]
		public void TestGetMenuBuilder()
		{
			var header = CreateNewManifest();

			using (var form = new ZForm())
			{
				AssertEquals(ExpectedMenuBuilderType, ApplicationGUIProvider.GetApplicationGuiProvider(header).GetMenuBuilder(form, header).GetType());
			}
		}

		public void TestApplicationBusinessProviderType()
		{
			var header = CreateNewManifest();
			var guiProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var definedApplicationBusinessProviderType = guiProvider.ApplicationBusinessProviderType;
			AssertEquals("The ApplicationBusinessProviderType should be the same as the type of the current country's ApplicationBusinessProvider", header.ApplicationBusinessProvider.GetType(), definedApplicationBusinessProviderType);
		}

		public void TestApplicationGUIProviderType()
		{
			var header = CreateNewManifest();
			var guiProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertNotNull(guiProvider);
			AssertType<TGuiProvider>(guiProvider);
		}

		[RequiresSTA]
		public void TestGetNewAsycudaItemSelectionDialog()
		{
			var header = CreateNewManifest();
			var bill = header.Bills[0];
			var messageChooser = header.GetNewMessageChooser(new[] { bill }, string.Empty, true);
			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, "DS", string.Empty))
			{
				AssertEquals(ExpectedAsycudaItemSelectionDialogType, dlg.GetType());
			}
		}

		public void TestGetAsycudaContainerUserControl()
		{
			var header = CreateNewManifest();
			using (var control = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetAsycudaContainerUserControl())
			{
				AssertEquals(ExpectedContainerUserControlType, control.GetType());
			}
		}

		public void TestGetContainerCountrySpecificUserControl()
		{
			var header = CreateNewManifest();
			using (var control = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetContainerCountrySpecificUserControl())
			{
				AssertEquals(ExpectedContainerCountrySpecificUserControlType, control?.GetType());
			}
		}

		public void TestGetContainersGridExtraColumnInfos()
		{
			var header = CreateNewManifest();
			var columnInfos = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetContainersGridExtraColumnInfos(header).ToArray();
			AssertGetContainersGridExtraColumnInfos(columnInfos);
		}

		public void TestGetContainersGridColumnsOrder()
		{
			var header = CreateNewManifest();
			var columnsOrder = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetContainersGridColumnsOrder().ToArray();
			AssertGetContainersGridColumnsOrder(columnsOrder);
		}

		public void TestGetContainersGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var columnAvailability = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetContainersGridColumnAvailability();
			AssertGetContainersGridColumnAvailability(columnAvailability);
		}

		public void TestGetContainersGridColumnVisibility()
		{
			var header = CreateNewManifest();
			var columnVisibility = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetContainersGridColumnVisibility();
			AssertGetContainersGridColumnVisibility(columnVisibility);
		}

		public void TestGetContainersGridColumnsWidth()
		{
			var header = CreateNewManifest();
			var columnsWidth = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetContainersGridColumnsWidth();
			AssertGetContainersGridColumnsWidth(columnsWidth);
		}

		public void TestGetArrivalHeadersGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var columnAvailability = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetArrivalHeadersGridColumnAvailability();
			AssertGetArrivalHeadersGridColumnAvailability(columnAvailability);
		}

		public void TestGetArrivalLinesGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var columnAvailability = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetArrivalLinesGridColumnAvailability();
			AssertGetArrivalLinesGridColumnAvailability(columnAvailability);
		}

		public void TestGetTransferBillsGridExtraColumnInfos()
		{
			var header = CreateNewManifest();
			var columnInfos = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetTransferBillsGridExtraColumnInfos().ToArray();
			AssertGetTransferBillsGridExtraColumnInfos(columnInfos);
		}

		public void TestGetTransferBillsGridColumnsOrder()
		{
			var header = CreateNewManifest();
			var columnsOrder = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetTransferBillsGridColumnsOrder().ToArray();
			AssertGetTransferBillsGridColumnsOrder(columnsOrder);
		}

		public void TestGetPacksGridColumnsOrder()
		{
			var header = CreateNewManifest();
			var columnsOrder = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetPacksGridColumnsOrder();
			AssertGetPacksGridColumnsOrder(columnsOrder);
		}

		public void TestGetPacksGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var resultDict = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetPacksGridColumnAvailability(header);
			AssertGetPacksGridColumnAvailability(resultDict, header);
		}

		public void TestGetPacksGridColumnVisibility()
		{
			var header = CreateNewManifest();
			var columnVisibility = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetPacksGridColumnVisibility();
			AssertGetPacksGridColumnVisibility(columnVisibility);
		}

		public void TestGetMessagesGridExtraColumnInfos()
		{
			var header = CreateNewManifest();
			var columnInfos = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetMessagesGridExtraColumnInfos().ToArray();
			AssertGetMessagesGridExtraColumnInfos(columnInfos);
		}

		public void TestGetMessagesGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var columnAvailability = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetMessagesGridColumnAvailability(header);
			AssertGetMessagesGridColumnAvailability(columnAvailability);
		}

		public void TestGetMessagesGridColumnVisible()
		{
			var header = CreateNewManifest();
			var columnVisible = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetMessagesGridColumnVisible(header);
			AssertGetMessagesGridColumnVisible(columnVisible);
		}

		public void TestGetMessagesGridColumnsWidth()
		{
			var header = CreateNewManifest();
			var columnsWidth = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetMessagesGridColumnsWidth(header);
			AssertGetMessagesGridColumnsWidth(columnsWidth);
		}

		public void TestGetMessagesGridOrder()
		{
			var header = CreateNewManifest();
			var columns = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetMessagesGridOrder().ToArray();
			AssertGetMessagesGridOrder(columns);
		}

		public void TestGetBillsGridExtraColumnInfos()
		{
			var header = CreateNewManifest();
			var columnInfos = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridExtraColumnInfos().ToArray();
			AssertGetBillsGridExtraColumnInfos(columnInfos);
		}

		protected virtual void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), columnInfos.Select(s => s.ColumnName));
		}

		public void TestGetColumnsToRemoveInBillsGrid()
		{
			var header = CreateNewManifest();
			var columns = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetColumnsToRemoveInBillsGrid().ToArray();
			AssertColumnsToRemoveInBillsGrid(columns);
		}

		protected virtual void AssertColumnsToRemoveInBillsGrid(string[] columns)
		{
			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), columns);
		}

		public void TestGetPacksGridExtraColumnInfos()
		{
			var header = CreateNewManifest();
			var columnInfos = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetPacksGridExtraColumnInfos().ToArray();
			AssertGetPacksGridExtraColumnInfos(columnInfos);
		}

		protected virtual void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), columnInfos.Select(s => s.ColumnName));
		}

		public void TestGetBillsGridColumnsWidth()
		{
			var header = CreateNewManifest();
			var columnsWidth = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridColumnsWidth();
			AssertGetBillsGridColumnsWidth(columnsWidth);
		}

		public void TestGetBillsGridColumnVisiblilityOnValueChanged()
		{
			var header = CreateNewManifest();
			var columnsVisiblility = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridColumnVisiblilityOnValueChanged(header);
			AssertGetBillsGridColumnVisiblilityOnValueChanged(columnsVisiblility, header);
		}

		public void TestGetTransferBillCountrySpecificUserControl()
		{
			var header = CreateNewManifest();
			using (var control = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetTransferBillCountrySpecificUserControl())
			{
				AssertEquals(ExpectedTransferBillCountrySpecificUserControlType, control?.GetType());
			}
		}

		public void TestGetTaxesGridExtraColumnInfos()
		{
			var header = CreateNewManifest();
			var columnInfos = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetTaxesGridExtraColumnInfos().ToArray();
			AssertGetTaxesGridExtraColumnInfos(columnInfos);
		}

		public virtual void TestVesselCodeFindBox_PopupSelected()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var header = CreateNewManifest();
			var selectedEventArgs = new ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs(new BusinessObject[] { vessel });
			ApplicationGUIProvider.GetApplicationGuiProvider(header).VesselCodeFindBox_PopupSelected(header, null, selectedEventArgs);
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestBillAdditionalTabPageUserControls()
		{
			var header = CreateNewManifest();
			var additiontypes = new List<Type>();
			foreach (var tabPageUserControl in ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillAdditionalTabPageUserControl().OrderBy(t => t.TabPageSequence))
			{
				using (tabPageUserControl)
				{
					additiontypes.Add(tabPageUserControl?.GetType());
				}
			}

			AssertArrayEqualsByElements("Bill additional tab pages are displayed in correct order.", ExpectedBillAdditionalTabPageUserControls, additiontypes.ToArray());
		}

		public void TestGetBillPartiesTabPageVisibility()
		{
			AssertGetBillPartiesTabPageVisibility();
		}

		[RequiresSTA]
		public void TestAddBillPlugins()
		{
			var header = CreateNewManifest();

			using (var form = new ZForm())
			{
				ApplicationGUIProvider.GetApplicationGuiProvider(header).AddBillPlugins(form);
				AssertContainsExactElementsInAnyOrder(ExpectedBillPluginsControllerIDs, form.PlugIns.Instances.Select(x => x.ControllerID));
			}
		}

		public void TestGetBillFormCaption()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertNoExceptionThrown("No exception thrown when input is null", () => provider.GetBillFormCaption(null));
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "TESTBILL1";
			AssertEquals(ExpectedGetBillFormCaption, provider.GetBillFormCaption(bill));
		}

		public void TestManifestLayout()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var layout = provider.GetManifestLayout().Layout;

			if (layout == null)
			{
				Assert("Still using old layout engine", true);
				return;
			}

			AssertLessThanOrEqualTo("Layout for Manifest should not have more than 2 columns", layout.Columns.Count, MaxColumnsOfManifestLayout);
			CheckManifestControlGroups(layout);
		}

		public void TestGetBillLayout()
		{
			var header = CreateNewManifest();
			var billLayout = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillLayout();
			AssertType(ExpectedBillLayoutType, billLayout);
		}

		public void TestBillLayout()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var layout = provider.GetBillLayout().Layout;

			if (layout == null)
			{
				Assert("Still using old layout engine", true);
				return;
			}

			AssertLessThanOrEqualTo("Layout for Bill should not have more than 3 columns", layout.Columns.Count, 3);
		}

		public void TestGetBillPartiesLayout()
		{
			var header = CreateNewManifest();
			var billPartiesLayout = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillPartiesLayout();
			AssertType(ExpectedBillPartiesLayoutType, billPartiesLayout);
		}

		public void TestPackedItemDetailsLayout()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var layout = provider.GetPackedItemDetailsLayout().Layout;

			if (layout == null)
			{
				Assert("Still using old layout engine", true);
				return;
			}

			AssertLessThanOrEqualTo("Layout for Packed Items should not have more than 3 columns", layout.Columns.Count, 3);
		}

		public void TestPackAdditionalTabPageUserControls()
		{
			var header = CreateNewManifest();
			var additiontypes = new List<Type>();
			foreach (var tabPageUserControl in ApplicationGUIProvider.GetApplicationGuiProvider(header).GetPackAdditionalTabPageUserControl())
			{
				using (tabPageUserControl)
				{
					additiontypes.Add(tabPageUserControl?.GetType());
				}
			}

			AssertArrayEqualsByElements(additiontypes.ToArray(), ExpectedPackAdditionalTabPageUserControls);
		}

		public void TestTransferDetailsLayout()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var layout = provider.GetTransferDetailsLayout().Layout;

			AssertLessThanOrEqualTo("Layout for Transfer Details should not have more than 3 columns", layout.Columns.Count, 3);
		}

		public void TestGetApplicationGuiProvider()
		{
			var header = CreateNewManifest();
			Factory.Save();

			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			AssertNotNull("Should create a valid ApplicationGUIProvider.", provider);
			AssertSame("Should use the cached object.", provider, ApplicationGUIProvider.GetApplicationGuiProvider(header));

			var newHeader = CreateNewManifest();
			var newProvider = ApplicationGUIProvider.GetApplicationGuiProvider(newHeader);

			AssertSame("Should use the cached object.", newProvider, provider);

			header.Delete();
			newProvider = ApplicationGUIProvider.GetApplicationGuiProvider(newHeader);

			AssertSame("The ApplicationGUIProvider should not reference the header.", newProvider, provider);
			AssertNoExceptionThrown("Should not thorw any exceptions.", () =>
			{
				_ = newProvider.GetIdentifier(newHeader);
			});
		}

		public void TestGetHeaderAdditionalTabPageUserControls()
		{
			var header = CreateNewManifest();
			var expectedTabs = ExpectedGetHeaderAdditionalTabPageUserControlsTypes.ToArray();
			AssertHeaderAdditionalTabPageUserControls(header, expectedTabs);
		}

		protected void AssertHeaderAdditionalTabPageUserControls(AsycudaManifestHeader header, Type[] expectedTabs)
		{
			var actualTabs = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetHeaderAdditionalTabPageUserControls(header).Select(c =>
			{
				using (c)
				{
					return c.GetType();
				}
			}).ToArray();
			AssertArrayEqualsByElements(expectedTabs, actualTabs);
		}

		public void TestGetAsycudaMenuCaption()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var menuCaption = provider.GetAsycudaMenuCaption();

			AssertEquals(ExpectedMenuCaptionEnglishText, menuCaption?.EnglishText ?? string.Empty);
		}

		protected virtual string ExpectedMenuCaptionEnglishText => string.Empty;

		public void TestGetDutiesTabPageCaption()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var menuCaption = provider.GetDutiesTabPageCaption();

			AssertEquals(ExpectedDutiesTabPageCaption, menuCaption?.Caption ?? string.Empty);
		}

		protected virtual string ExpectedDutiesTabPageCaption => "Duties";

		public virtual void TestArrivalLinesReadOnlyIsTrueByDefault()
		{
			var header = CreateNewManifest();
			AssertEquals("ArrivalLinesReadOnly should be Read Only by default", true, ApplicationGUIProvider.GetApplicationGuiProvider(header).ArrivalLinesReadOnly());
		}

		public void TestShouldPositionMessagesTabAccordingToMessageLevel()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(ExpectedShouldPositionMessagesTabAccordingToMessageLevel, provider.ShouldPositionMessagesTabAccordingToMessageLevel);
		}

		public void TestGetHeaderMessagesUserControl()
		{
			var header = CreateNewManifest();
			using var messageUserControl = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetHeaderMessagesUserControl();
			AssertEquals(ExpectedHeaderMessagesUserControl, messageUserControl.GetType());
		}

		[RequiresSTA]
		public void TestGetBillMessagesUserControl()
		{
			var header = CreateNewManifest();
			using var messageUserControl = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillMessagesUserControl();
			AssertEquals(ExpectedBillMessagesUserControl, messageUserControl.GetType());
		}

		protected virtual bool ExpectedShouldPositionMessagesTabAccordingToMessageLevel => false;

		protected virtual IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => Enumerable.Empty<Type>();

		protected virtual Type ExpectedBillLayoutType => typeof(DefaultBillLayouts);

		protected virtual Type ExpectedBillPartiesLayoutType => typeof(DefaultBillPartiesLayouts);

		protected abstract Type ExpectedMenuBuilderType { get; }

		protected virtual Type ExpectedAsycudaItemSelectionDialogType => typeof(AsycudaItemSelectionDialog);

		protected virtual Type ExpectedContainerUserControlType => typeof(AsycudaContainerUserControl);

		protected virtual Type ExpectedContainerCountrySpecificUserControlType => null;

		protected virtual Type ExpectedHeaderMessagesUserControl => typeof(MessagesUserControl);

		protected virtual Type ExpectedBillMessagesUserControl => typeof(MessagesUserControl);

		protected virtual void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(0, columnInfos.Length);
		}

		protected virtual void AssertGetContainersGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(0, columnsOrder.Length);
		}

		protected virtual void AssertGetContainersGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(0, columnAvailability.Count);
		}

		protected virtual void AssertGetContainersGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
		{
			AssertEquals(1, columnVisibility.Count);
		}

		protected virtual void AssertGetContainersGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertEquals(0, columnsWidth.Count);
		}

		protected virtual void AssertGetArrivalHeadersGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(0, columnAvailability.Count);
		}

		protected virtual void AssertGetArrivalLinesGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(0, columnAvailability.Count);
		}

		protected virtual void AssertGetTransferBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(0, columnInfos.Length);
		}

		protected virtual void AssertGetTransferBillsGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(0, columnsOrder.Length);
		}

		protected virtual void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(null, columnsOrder);
		}

		protected virtual void AssertGetPacksGridColumnAvailability(IReadOnlyDictionary<bool, string[]> result, THeader header)
		{
			AssertEquals(0, result.Count);
		}

		protected virtual void AssertGetPacksGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
		{
			AssertEquals(0, columnVisibility.Count);
		}

		protected virtual void AssertGetMessagesGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(0, columnInfos.Length);
		}

		protected virtual void AssertGetMessagesGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(0, columnAvailability.Count);
		}

		protected virtual void AssertGetMessagesGridColumnVisible(IReadOnlyDictionary<bool, string[]> columnVisible)
		{
			AssertEquals(0, columnVisible.Count);
		}

		protected virtual void AssertGetMessagesGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertEquals(0, columnsWidth.Count);
		}

		protected virtual void AssertGetMessagesGridOrder(string[] columns)
		{
			AssertEquals(0, columns.Length);
		}

		protected virtual void AssertGetBillsGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertEquals(0, columnsWidth.Count);
		}

		protected virtual void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, AsycudaManifestHeader header)
		{
			AssertEquals(0, columnsVisiblility.Count);
		}

		protected virtual void AssertGetTaxesGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(0, columnInfos.Length);
		}

		protected virtual void AssertGetBillPartiesTabPageVisibility()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			Assert("Bill Parties Tab Page should be visible by default", provider.GetBillPartiesTabPageVisibility(header));
		}

		protected virtual Type ExpectedTransferBillCountrySpecificUserControlType => null;

		protected virtual Type[] ExpectedBillAdditionalTabPageUserControls => Array.Empty<Type>();

		protected virtual IEnumerable<ControllerID> ExpectedBillPluginsControllerIDs => Enumerable.Empty<ControllerID>();

		protected virtual string ExpectedGetBillFormCaption => null;

		protected virtual int MaxColumnsOfManifestLayout => 2;

		protected virtual Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[]
			{
				common.VesselCodeFindBox,
				common.VoyageFlightTextBox,
				common.LloydsNumberTextBox,
				common.RadioCallSignTextBox,
				common.ConveyanceCountryCodeFindBox
			});
			groups.Add("Road Transport", new[]
			{
				common.VehicleRegistrationTextBox,
				common.Trailer1RegNoTextBox,
				common.Trailer2RegNoTextBox,
				common.Trailer1RegCountryCodeFindBox,
				common.Trailer2RegCountryCodeFindBox
			});
			return groups;
		}

		protected virtual Type[] ExpectedPackAdditionalTabPageUserControls => Array.Empty<Type>();

		protected override Form GetFormToBashCore()
		{
			THeader header = CreateNewManifest();
			Factory.Save();
			var result = new ManifestForm(header);
			result.ControllerID = ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;
			return result;
		}

		public override Type FormToBashType => typeof(ManifestForm);

		protected virtual THeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<THeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "HELLO";
			_ = pack.PackedItemForTesting();
			return header;
		}

		protected TGuiProvider CreateNewGuiProvider()
		{
			var header = CreateNewManifest();
			var guiProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			return guiProvider as TGuiProvider;
		}

		void CheckManifestControlGroups(PanelLayout layout)
		{
			var groups = GetManifestControlGroups();

			var currentGroups = new Dictionary<string, ControlReference>();
			var terminatedGroups = new Dictionary<string, (ControlReference LastControl, ControlReference TerminatingControl)>();

			foreach (var control in layout.GetControlsInTabOrder())
			{
				var controlGroupNames = groups.Where(g => g.Value.Contains(control)).Select(g => g.Key).ToHashSet();

				foreach (var groupName in currentGroups.Keys.ToArray())
				{
					if (!controlGroupNames.Contains(groupName))
					{
						terminatedGroups[groupName] = (LastControl: currentGroups[groupName], TerminatingControl: control);
						currentGroups.Remove(groupName);
					}
				}

				foreach (var groupName in controlGroupNames)
				{
					if (terminatedGroups.TryGetValue(groupName, out var terminatedGroup))
					{
						Fail($"Controls '{terminatedGroup.LastControl}' and '{control}' from the '{groupName}' group are separated by control '{terminatedGroup.TerminatingControl}'.");
					}

					currentGroups[groupName] = control;
				}
			}
		}
	}
}
