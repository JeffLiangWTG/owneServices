using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class EmbeddedModuleTestHelper
	{
		readonly EmbeddedModulePopup popup;
		public FilterStripBusinessObject FilterBizo => popup.Module.FilterBusinessObject;

		public EmbeddedModuleTestHelper(EmbeddedModulePopup popup)
		{
			this.popup = popup;
		}

		public FilterStrip AddFilter<T>(string filterDescription, Action<T> propertyValueSetter) where T : ModuleFilter
		{
			var strip = FilterBizo.FilterStrips.AddNew(filterDescription);
			var filterControl = (ZFilterStripCommonControl)popup.Module.EmbeddedControl;
			filterControl.AddFilterStrip(strip);
			var filter = (T)strip.CurrentModuleFilter;
			propertyValueSetter(filter);

			return strip;
		}

		public void RemoveAllFilters(string filterDescription)
		{
			var strips = FilterBizo.FilterStrips.Cast<FilterStrip>().Where(x => x.FilterDescription == filterDescription);
			RemoveFilterStrips(strips.ToArray());
		}

		void RemoveFilterStrips(FilterStrip[] strips)
		{
			FilterBizo.FilterStrips.RemoveRange(strips);

			foreach (var strip in strips.Where(strip => !FilterBizo.FilterStrips.Cast<FilterStrip>().Any(x => x.CurrentModuleFilter == strip.CurrentModuleFilter)))
			{
				strip.CurrentModuleFilter.IsActive = false;
			}
		}

		public static void SetListToStoreSearchResultsWhenPopupShown<T>(List<T> results)
			where T : BusinessObject
		{
			ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
			{
				results.Clear();
				var popup = (EmbeddedModulePopup)dialog;
				results.AddRange(popup.Module.GridCollection.Cast<T>());
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static void SaveUserDefinedFilterLayoutInAnotherLayout(string innerLayoutName, string outerLayoutName, ModuleIdentifier moduleToCreate = null)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleToCreate ?? DummyModuleIDs.Dummy))
			using (var form = (EmbeddedModulePopup)module.ShowPopup())
			{
				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = ModuleUserDefinedFilter.GetPrefixedDescription(innerLayoutName);
				module.Grid.Focus();

				SetDelegateForModulePopup_SaveFilterInAnotherLayout(innerLayoutName, outerLayoutName);

				form.ExposedOKButtonForTesting.PerformClick();
				Application.DoEvents();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		static void SetDelegateForModulePopup_SaveFilterInAnotherLayout(string innerLayoutName, string outerLayoutName)
		{
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog2 =>
			{
				var saveLayoutPopup = (SaveLayoutForm)dialog2;
				saveLayoutPopup.Show();
				Application.DoEvents();

				var isUserDefinedCheckBox = saveLayoutPopup.FindAll<ZCheckBox>(control => control.Name.Contains("User")).First();
				isUserDefinedCheckBox.Checked = true;

				var layoutNameTextBox = saveLayoutPopup.FindAll<Control>(control => control.Text == innerLayoutName).First();
				layoutNameTextBox.Text = outerLayoutName;

				var saveButton = saveLayoutPopup.FindAll<ZButton>(control => !control.Name.Contains("Cancel")).First();
				saveLayoutPopup.Show();
				Application.DoEvents();

				saveButton.PerformClick();
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static BusinessObject[] PerformFindOnModulePopupAndGetResults(ZFilterGridModule module, ZForm form)
		{
			module.Grid.Focus();

			var toolStrip = form.FindAll<ZFilterStripBaseControl>().First();
			toolStrip.Find();
			form.Show();
			Application.DoEvents();

			module.Grid.SelectAllElements();

			return module.Grid.SelectedElements;
		}

		public static void PerformFindOnModulePopup(ZFilterGridModule module, ZForm form)
		{
			PerformFindOnModulePopupAndGetResults(module, form);
		}

		/// <summary>
		/// Use this instead of OpenUserDefinedFilterStripPopupButton(String, ZForm) when your moduleFilter to open is currently active
		/// For example, if it's selected in a module.
		/// </summary>
		public static void OpenUserDefinedFilterStripPopupButton(ModuleUserDefinedFilter moduleFilter, ZForm form)
		{
			// this gets around a weird quirk where, if the filter strip we're clicking has any hope of activating, it'll actually duplicate itself first
			// this is because it's active when we try to select it, and instead of just re-selecting it, it makes a copy
			// to get around it, deactivate the filter to select, then re-select it, properly triggering events such that we can click the '...' button on it
			moduleFilter.IsActive = false;

			OpenUserDefinedFilterStripPopupButton(moduleFilter.MultilingualDescription, form);
		}

		/// <summary>
		/// Note: this opens up a popup, so make sure you have ZFormModaliser delegates set BEFORE calling this method
		/// </summary>
		public static void OpenUserDefinedFilterStripPopupButton(string moduleFilterName, ZForm form)
		{
			ForceSelectFilterFromDropDown(moduleFilterName, form);

			var filterStrips = form.FindAll<ZFilterStrip>();
			var filterToEdit = filterStrips.First();
			var editBox = filterToEdit.FindAll<ZFilterCollectionFindBox>().First();
			editBox.PopupButton.PerformClick();
		}

		/// <summary>
		/// Selects the chosen filter name from the form, then nudges the ZFilterStripDropEdit binding to recognise that fact.
		/// Use the filtername as it would appear to the user in the filterdropdown.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static void ForceSelectFilterFromDropDown(string moduleFilterName, ZForm form)
		{
			var dropdown = form.FindAll<ZFilterStripDropEdit>().First();
			var index = 0;
			foreach (var filter in dropdown.List)
			{
				if (filter.ToString().Contains(moduleFilterName))
				{
					break;
				}
				else
				{
					index++;
				}
			}

			var filterToSelect = dropdown.List[index];
			dropdown.OnItemSelected((ICodeDescription)filterToSelect, true);

			form.Show();
			Application.DoEvents();
		}

		#region Assertions

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static void AssertModuleGridContentsAfterChangingFilter(string filterText, ZGuid bizoToFindPK)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				AssertModuleGridContentsAfterChangingFilter(module, form, filterText, bizoToFindPK);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static void AssertModuleGridContentsAfterChangingFilter(ZFilterGridModule module, ZForm form, string filterText, ZGuid bizoToFindPK)
		{
			module.FilterBusinessObject.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(filterText));

			var filterControl = form.FindSingle<ZFilterStripBaseControl>();
			var filterStrip = filterControl.FindSingle<ZFilterStrip>();
			filterStrip.FilterDescriptionDropEdit.CodeBox.Text = ModuleUserDefinedFilter.GetSuffixedDescription(filterText);
			module.Grid.Focus();

			var toolStrip = form.FindAll<ZFilterStripBaseControl>().First();
			toolStrip.Find();
			form.Show();
			Application.DoEvents();

			module.Grid.SelectAllElements();

			var foundBizos = module.Grid.SelectedElements;

			NUnit.Framework.Assertion.AssertContainsExactElementsInAnyOrder(
				"We changed what we're filtering by, and should have new results in our grid",
				new[] { bizoToFindPK },
				foundBizos.Select(bizos => bizos.PK));
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static void PopupUserDefinedFilter(Action<UserDefinedFilterEmbeddedModulePopup> actionOnFilterStripsLoaded, string filterName = "Me Filter", bool isUserDefinedFilterPublished = false)
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties(filterName, isUserDefinedFilterPublished);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = ModuleUserDefinedFilter.GetSuffixedDescription(filterName);
				module.Grid.Focus();
				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = dialog as UserDefinedFilterEmbeddedModulePopup;

					if (popup != null)
					{
						popup.FilterStripsLoaded += (_, x_) =>
						{
							actionOnFilterStripsLoaded(popup);
						};
					}
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
			}
		}

		#endregion
	}
}
