using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	class EmbeddedModulePopupTest : TestCaseWithDummy
	{
		public void TestMinimumSizeDoesNotChange()
		{
			using (var findBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy })
			using (var form = new ZForm())
			using (var dummyModule = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(dummyModule))
			{
				popup.ShowModal(findBox, form);

				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 305, true), popup.MinimumSize);
			}
		}

		public void TestRequireAtLeastOneItemToBeSelected()
		{
			using (var findBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy })
			using (var form = new ZForm())
			using (var dummyModule = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(dummyModule))
			{
				popup.ShowModal(findBox, form);

				AssertEquals(true, popup.ExposedOKButtonForTesting.Visible);
				AssertEquals("Cancel", popup.CancelButtonForTest.Text);

				popup.RequireAtLeastOneItemToBeSelected = false;

				AssertEquals(false, popup.ExposedOKButtonForTesting.Visible);
				AssertEquals("Close", popup.CancelButtonForTest.Text);
			}
		}

		public void TestUsesTheCodeAndDescriptionPropertiesFromListProvider()
		{
			var listProvider = new Mock<IFindBoxListProvider>();
			listProvider.Setup(p => p.GetCustomCodeDescription(It.IsAny<BusinessObject>()))
				.Returns(new CodeDescriptionPair("BLAH", "Something other than the default"));

			var list = new DummyBusinessObjectCollection(Factory) { FindBoxListProviderOverride = listProvider.Object };
			var dummy = list.AddNew();
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);

				findbox.List = list;
				findbox.ModuleID = DummyModuleIDs.Dummy;

				using (var dummyModule = new DummyFilterGridModule())
				{
					dummyModule.OverrideModuleDecisionProvider(dummyModule.GetModuleDecisionProviderForFindBoxPopup(findbox));

					using (var popup = new EmbeddedModulePopup(dummyModule))
					{
						// InitialiseDecisionProviderForFindBox
						popup.ShowModal(findbox, form);

						var grid = (ZDisplayGrid)dummyModule.DisplayGrid;
						var moduleColl = (DummyBusinessObjectCollection)grid.List;
						moduleColl.Add(Dummy);

						dummyModule.DisplayGrid.Select(0);
						popup.ExposedOKButtonForTesting.PerformClick();

						AssertEquals("FindboxCode", "BLAH", ((IFindBox)findbox).Code);
					}
				}
			}
		}

		public void TestEmbeddedModulePopupOKButtonStrategy()
		{
			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "ABC";
				using (var dummyModule = new DummyFilterGridModule())
				using (var popup = new EmbeddedModulePopup(dummyModule))
				{
					popup.ShowModal(findbox, form);

					AssertNotNull("strategy set from module decision provider", popup.EmbeddedModulePopupOKButtonStrategy);

					Dummy.Z0_Description = "";
					popup.EmbeddedModulePopupOKButtonStrategy = new DummyModuleDecisionProviderStrategy();

					var grid = (ZDisplayGrid)dummyModule.DisplayGrid;
					var moduleColl = (DummyBusinessObjectCollection)grid.List;
					moduleColl.Add(Dummy);

					grid.Select(0);
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("newly assigned strategy is used", "Selected", Dummy.Z0_Description);
					AssertEquals("FindboxCode", findbox.CodeBox.Text, popup.FindBoxCode);
				}
			}
		}

		class DummyModuleDecisionProviderStrategy : IEmbeddedModulePopupOKButtonStrategy
		{
			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
			{
				((DummyBusinessObject)selectedBusinessObject[0]).Z0_Description = "Selected";
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject activeModuleFilters)
			{
			}
		}

		public void TestSelectedEventIsCalledBeforeClosing()
		{
			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "ABC";
				using (var dummyModule = new DummyFilterGridModule())
				using (var popup = new EmbeddedModulePopup(dummyModule))
				{
					popup_SelectedCalled = false;
					popup.Selected += new EmbeddedModulePopup.SelectedEventHandler(popup_Selected);
					popup.ShowModal(findbox, form);
					AssertEquals(true, popup.Visible);
					popup.HandleSelection(new BusinessObject[] { Dummy });
					AssertEquals(false, popup.Visible);
					AssertEquals(true, popup_SelectedCalled);
				}
			}
		}

		public void TestToolbarColorFromTheme()
		{
			using (var dummyModule = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(dummyModule))
			{
				AssertEquals(SystemDataRegistry.Instance.ColorTheme.ToolbarColor, popup.ToolBarPanel.BackColor);
			}
		}

		bool popup_SelectedCalled;
		void popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			popup_SelectedCalled = true;
		}

		public void TestShowModalSetsInitialCodeForSearch_InvalidCode()
		{
			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "ABC";

				var dummyModule = new DummyFilterGridModule();

				new EmbeddedModulePopup(dummyModule).ShowModal(findbox, form);

				AssertEquals("ABC", ((ModuleTextFilter)dummyModule.DummyFilterBusinessObject[DummyBizoSchema.Z0_Code.Name]).Property);
			}
		}

		public void TestShowModalSetsInitialCodeForSearch_ValidCode()
		{
			var realDummy = Factory.New<DummyBusinessObject>();
			realDummy.Z0_Code = "DEF";
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "DEF";

				var dummyModule = new DummyFilterGridModule();

				new EmbeddedModulePopup(dummyModule).ShowModal(findbox, form);

				AssertEquals("DEF", ((ModuleTextFilter)dummyModule.DummyFilterBusinessObject[DummyBizoSchema.Z0_Code.Name]).Property);
			}
		}

		public void TestAllRowsLoaded()
		{
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			ResetDummies();

			using (DummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				((DummyFilterControl)DummyModule.EmbeddedControl).ResetFilter();
				using (Popup = new EmbeddedModulePopup(DummyModule))
				{
					Popup.Show();
					AssertEquals("All rows, including those that don't match the Additional filter, are loaded", 3, DummyModule.Grid.List.Count);
				}
			}
		}

		public void TestAdditionalFilterAppliedOnValidRow()
		{
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			ResetDummies();

			using (DummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				((DummyFilterControl)DummyModule.EmbeddedControl).ResetFilter();
				using (Popup = new EmbeddedModulePopup(DummyModule))
				{
					Popup.Show();
					for (var i = 0; i < DummyModule.Grid.List.Count; i++)
					{
						if (DummyModule.Grid[i, 0].ToString() == "ZUBS")
						{
							DummyModule.Grid.Select(i);
						}
					}

					Popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("Row that matches Additional Filter is selectable", false, UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
		}

		public void TestAdditionalFilterAppliedOnInvalidRow()
		{
			ResetDummies();

			using (DummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				IBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Number, 5));
				DummyModule.OverrideModuleDecisionProvider(new DummyModuleDecisionProvider(list));
				((DummyFilterControl)DummyModule.EmbeddedControl).ResetFilter();

				using (Popup = new EmbeddedModulePopup(DummyModule))
				{
					Popup.Show();
					for (var i = 0; i < DummyModule.Grid.List.Count; i++)
					{
						if (DummyModule.Grid[i, 0].ToString() == "TESTING")
						{
							DummyModule.Grid.Select(i);
						}
					}

					Popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("Row that DOESN'T match Additional Filter is not selectable", true, UnitTestUserNotification.Instance.LastMessage.WasError);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		#region TestSelectFromPopupWithoutDisplaying

		public void TestSelectFromPopupWithoutDisplaying()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			using (var findbox = new ZCodeFindBox())
			{
				var codeFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter("Q Description", DummyBizoSchema.Z0_Description);
				codeFilter.Property = "Q";
				codeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

				AssertEquals(SilentSelectResult.None, popup.SelectFromPopupWithoutDisplaying(findbox, popup));

				((IFindBox)findbox).Code = "XYZ";

				AssertEquals(SilentSelectResult.FoundNothing, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
				AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);

				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = "Q";
				dummy.Z0_Code = "ABC";
				Factory.Save();

				AssertEquals(SilentSelectResult.FoundOne, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
				AssertEquals("Selected code should be set", "ABC", ((IFindBox)findbox).Code);

				((IFindBox)findbox).Code = "XYZ";
				dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = "Q";
				Factory.Save();

				AssertEquals(SilentSelectResult.MultipleResults, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
				AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);
			}
		}

		public void TestSelectFromPopupWithoutDisplaying_WithRelationshipFilter()
		{
			using (var module = new DummyFilterGridModuleWithActiveBOC())
			{
				module.RelationshipFilter = new ZQuery(DummyBizoSchema.Z0_Bool, true);

				using (var popup = new EmbeddedModulePopup(module))
				using (var findbox = new ZCodeFindBox())
				{
					var codeFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter("Q Description", DummyBizoSchema.Z0_Description);
					codeFilter.Property = "Q";
					codeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

					AssertEquals(SilentSelectResult.None, popup.SelectFromPopupWithoutDisplaying(findbox, popup));

					((IFindBox)findbox).Code = "XYZ";

					AssertEquals(SilentSelectResult.FoundNothing, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
					AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);

					var dummy = Factory.New<DummyBusinessObject>();
					dummy.Z0_Description = "Q";
					dummy.Z0_Code = "ABC";
					Factory.Save();

					AssertEquals(SilentSelectResult.FoundNothing, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
					AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);

					dummy.Z0_Bool = true;
					Factory.Save();

					AssertEquals(SilentSelectResult.FoundOne, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
					AssertEquals("Selected code should be set", "ABC", ((IFindBox)findbox).Code);

					((IFindBox)findbox).Code = "XYZ";
					dummy = Factory.New<DummyBusinessObject>();
					dummy.Z0_Description = "Q";
					dummy.Z0_Bool = true;
					Factory.Save();

					AssertEquals(SilentSelectResult.MultipleResults, popup.SelectFromPopupWithoutDisplaying(findbox, popup));
					AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);
				}
			}
		}

		#endregion

		public void TestAutoRunSearchAndSelect_OnlyOneResult()
		{
			var realDummy = Factory.New<DummyBusinessObject>();
			realDummy.Z0_Code = "DEF";
			realDummy.Z0_Description = "Description";
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "D";

				var dummyModule = new DummyFilterGridModule();

				var popup = new EmbeddedModulePopup(dummyModule);
				((ZFilterStripCommonControl)popup.Module.EmbeddedControl).RunSearchOnEnteringAModuleOverride = true;
				Assert(popup.AutoSearchAndSelectIfOnlyOneRecord(findbox));
				AssertEquals("Should preform auto select", "DEF", findbox.CodeBox.Text);
				AssertEquals("Should preform auto select", "Description", findbox.DescriptionBox.Text);
			}
		}

		public void TestAutoRunSearchAndSelect_MultipleResults()
		{
			var realDummy1 = Factory.New<DummyBusinessObject>();
			realDummy1.Z0_Code = "D11";
			realDummy1.Z0_Description = "Description1";
			var realDummy2 = Factory.New<DummyBusinessObject>();
			realDummy2.Z0_Code = "D22";
			realDummy2.Z0_Description = "Description2";
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "D";

				var dummyModule = new DummyFilterGridModule();

				var popup = new EmbeddedModulePopup(dummyModule);
				((ZFilterStripCommonControl)popup.Module.EmbeddedControl).RunSearchOnEnteringAModuleOverride = true;
				Assert(!popup.AutoSearchAndSelectIfOnlyOneRecord(findbox));
				AssertEquals("Should not preform auto select", "D", findbox.CodeBox.Text);
				AssertEquals("Should not preform auto select", "", findbox.DescriptionBox.Text);
				popup.Dispose();
			}
		}

		public void TestAutoRunSearchAndSelect_NoResults()
		{
			var realDummy1 = Factory.New<DummyBusinessObject>();
			realDummy1.Z0_Code = "A11";
			realDummy1.Z0_Description = "Description1";
			var realDummy2 = Factory.New<DummyBusinessObject>();
			realDummy2.Z0_Code = "A22";
			realDummy2.Z0_Description = "Description2";
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.List = new DummyBusinessObjectCollection(Factory);
				findbox.ModuleID = DummyModuleIDs.Dummy;
				findbox.CodeBox.Text = "D";

				var dummyModule = new DummyFilterGridModule();

				var popup = new EmbeddedModulePopup(dummyModule);
				((ZFilterStripCommonControl)popup.Module.EmbeddedControl).RunSearchOnEnteringAModuleOverride = true;
				Assert(!popup.AutoSearchAndSelectIfOnlyOneRecord(findbox));
				AssertEquals("Should not preform auto select", "D", findbox.CodeBox.Text);
				AssertEquals("Should not preform auto select", "", findbox.DescriptionBox.Text);
				popup.Dispose();
			}
		}

		public void TestIsSelectionMandatory()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();

			Factory.Save();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var control = (ZFilterStripCommonControl)module.EmbeddedControl;
				control.RunSearchOnEnteringAModuleOverride = true;

				using (var popup = new EmbeddedModulePopup(module))
				{
					popup.Show();
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("Selecting an item from the grid should be mandatory by default, and yet...", UnitTestUserNotification.Instance.LastMessage.Text, "Please select an item from the grid.");
				}

				UnitTestUserNotification.Instance.ClearMessages();

				using (var popup = new EmbeddedModulePopup(module))
				{
					popup.IsSelectionMandatory = false;
					popup.Show();
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestFormCaption()
		{
			using (var form = new EmbeddedModulePopup())
			{
				AssertNull(form.Module);
				AssertNullOrEmpty(form.FormCaption);
			}

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new EmbeddedModulePopup(module))
			{
				AssertEquals("FormCaption should be equal to the module description", module.Description.ToString(), form.FormCaption);
			}
		}

		public void TestOkButtonCaption()
		{
			using (var form = new EmbeddedModulePopup())
			{
				AssertNull(form.Module);
				AssertNullOrEmpty(form.FormCaption);
			}

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new EmbeddedModulePopup(module, okButtonCaption: "TB"))
			{
				form.Show();
				AssertEquals("OK Button Caption should be as specified in parameter", form.ExposedOKButtonForTesting.Text, "TB");
			}
		}

		public void TestMinimumWidthShouldKeepWithTheFirstScaleResult()
		{
			MinimumWidthKeepWithFirstScale(1, 1);
			MinimumWidthKeepWithFirstScale(1.25f, 1.25f);
		}

		void MinimumWidthKeepWithFirstScale(float scaleX, float scaleY)
		{
			using (var disposeAble = ControlDpiScalingHelper.OverrideDPI_ForTesting(scaleX * 96, scaleY * 96))
			using (var dummyModule = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(dummyModule))
			{
				Assert($"DpiSetForTest does not work,Expected Dpix:{scaleX * 96} Actual:{ControlDpiScalingHelper.DpiX} Expected DpiY:{scaleY * 96} Actual:{ControlDpiScalingHelper.DpiY}", ControlDpiScalingHelper.DpiX == scaleX * 96 && ControlDpiScalingHelper.DpiY == scaleY * 96);
				AssertEquals("MinimumSize width should keep with the first scale result!", (int)(scaleX * 1100), popup.MinimumSize.Width);
			}
		}

		class DummyModuleDecisionProvider : IModuleDecisionProvider
		{
			public DummyModuleDecisionProvider(IBusinessObjectCollection list)
			{
				this.list = list;
			}

			readonly IBusinessObjectCollection list;

			#region IModuleDecisionProvider Members

			bool IModuleDecisionProvider.ShouldDisplayNotifications
			{
				get { return false; }
			}

			bool IModuleDecisionProvider.ShouldLoadFilterBizObj
			{
				get { return false; }
			}

			bool IModuleDecisionProvider.ShouldSaveFilterBizObj
			{
				get { return false; }
			}

			bool IModuleDecisionProvider.ShouldIgnoreAdditionalFilter
			{
				get { return false; }
			}

			bool IModuleDecisionProvider.AllowExcelExport
			{
				get { return false; }
			}

			bool IModuleDecisionProvider.EnablePreviousNextSupport
			{
				get { return false; }
			}

			IBusinessObjectCollection IModuleDecisionProvider.List
			{
				get { return list; }
			}

			void IModuleDecisionProvider.HandleDefaultAction(BusinessObject[] selectedBusinessObject)
			{
			}

			void IEmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
			{
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}

			void IModuleDecisionProvider.InitialiseFindBoxControllerLink(ZController controller)
			{
			}

			void IModuleDecisionProvider.SetFindBoxCodeDescription(BusinessObject bizo)
			{
			}

			#endregion
		}

		#region Implementation

		EmbeddedModulePopup Popup;
		ZFilterGridModule DummyModule;

		protected void ResetDummies()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.New<DummyBusinessObject>();
			var dummy2 = factory.New<DummyBusinessObject>();
			var dummy3 = factory.New<DummyBusinessObject>();

			TestCaseHelper.ClearTable(Dummy.TableName);
			TestCaseHelper.ClearTable(dummy1.TableName);
			TestCaseHelper.ClearTable(dummy2.TableName);
			TestCaseHelper.ClearTable(dummy3.TableName);

			dummy1.Z0_Number = 5;
			dummy1.Z0_Description = "ZUBS";

			dummy2.Z0_Number = 28;
			dummy2.Z0_Description = "IS";

			dummy3.Z0_Number = 24;
			dummy3.Z0_Description = "TESTING";

			factory.Save();
		}

		#endregion
	}

	#region Basher Test

	[TestedType(typeof(EmbeddedModulePopup))]
	public class EmbeddModulePopupBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EmbeddedModulePopup(new DummyFilterGridModule());
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}
	}

	#endregion
}
