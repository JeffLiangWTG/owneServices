using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Enumeration;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RegistryApplyForm : RegistryComparisonForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RegistryApplyForm()
		{
			InitializeComponent();
		}

		Color ColorForItemsWhosValuesCanNotBeSet { get { return Color.Orange; } }

		public RegistryApplyForm(RegistryComparisonBusinessObject bo, bool hideInactiveChildren = false)
			: base(bo, hideInactiveChildren)
		{
			InitializeComponent();

			SetColorOfNonApplicableItems();
		}

		void SetColorOfNonApplicableItems()
		{
			int totalItems = 0;
			int notApplicableItems = 0;

			foreach (var node in RegistryItemTreeViewBuilder.GetLeafRegistryItemNodes(treeView.Nodes, requireChecked: false))
			{
				totalItems++;

				var registryItem = (IRegistryItem)node.Tag;
				if (!BusinessEntity.BaseLevel.CanSetValueOf(registryItem))
				{
					SetColorOfBranch(node, ColorForItemsWhosValuesCanNotBeSet);
					notApplicableItems++;
				}
			}

			var noItemsAreApplicable = (totalItems == notApplicableItems);
			btnApplyOverrides.Enabled = !noItemsAreApplicable;

			if (notApplicableItems > 0)
			{
				TellUserWhySomeItemsAreColored(noItemsAreApplicable);
			}
		}

		void TellUserWhySomeItemsAreColored(bool noItemsCanBeSetAtThisLevel)
		{
			var context = new DialogDefaultContext(new ZGuid("16EBBD10-9367-40CF-A912-F90C14AA0160"),
				ResString.GetMultilingualString("2C39C955-E9B2-46B9-9315-D7D2F607D314", "Why are some items colored?"),
				ZMessageBoxButtons.OK,
				ZMessageBoxIcon.Information,
				showCheckboxOnly: true);

			var message = noItemsCanBeSetAtThisLevel ?
				ResString.GetMultilingualString("5F4780D6-0463-4AAC-BB96-CC134F8AE319", "No items can be applied at this level. \r\n\r\nYou may view their values but can not apply them.") :
				ResString.GetMultilingualString("51EAD4B4-2308-4B4B-9D4F-915EB9C8B383", "Some items can not be applied at this level. They have been highlighted orange. \r\n\r\nWhen you apply overrides nothing will happen to items that are not applicable.");

			Globals.Message.ShowOrDefault(context, message);
		}

		void SetColorOfBranch(TreeNode leaf, Color color)
		{
			var parentsToSet = ZEnumerable
				.Iterate(leaf, node => node.Parent)
				.TakeWhile(node => node != null && AllChildrenHaveColor(node, color));

			foreach (var node in parentsToSet)
			{
				node.ForeColor = color;
			}
		}

		static bool AllChildrenHaveColor(TreeNode node, Color color)
		{
			return node.Nodes.Cast<TreeNode>().All(sibling => sibling.ForeColor == color);
		}

		void btnApplyOverrides_Click(object sender, EventArgs e)
		{
			var registryEditCheckpoint = Env.Security.SystemRegistryEdit;
			if (registryEditCheckpoint.IsAllowed)
			{
				ApplySelectedValues();
			}
			else
			{
				registryEditCheckpoint.ShowError();
			}
		}

		void ApplySelectedValues()
		{
			var itemsToUse = ValidateItems(CheckedRegistryItems, BusinessEntity.OverrideLevel, BusinessEntity.BaseLevel);
			if (itemsToUse != null)
			{
				if (itemsToUse.Count > 0)
				{
					Db.Connection.RunTransactioned(() =>
					{
						foreach (var item in itemsToUse)
						{
							var overrideValue = BusinessEntity.OverrideLevel.GetValueOf(item);
							BusinessEntity.BaseLevel.SetValueOf(item, overrideValue);
							AddChangedLogForItem(item, BusinessEntity.BaseLevel);
						}
					});

					Globals.Message.Show(Res.GetString("0D52665D-B2F9-48C9-9B1A-94C4F257545D", "The selected, applicable values were successfully changed."));
					UpdateDisplay();
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("C0B7FD3F-4922-4C56-BDEC-DB9C9AE63830", "No checked items were applicable, so no values were changed"));
				}
			}
		}

		IList<IRegistryItem> ValidateItems(IEnumerable<IRegistryItem> items, IOverrideLevel levelToGet, IOverrideLevel levelToSet)
		{
			var fallbackLevel = levelToSet.GetFallbackLevel();

			var successfulItems = new List<IRegistryItem>();
			foreach (var item in items)
			{
				if (levelToSet.CanSetValueOf(item))
				{
					var newValue = levelToGet.GetValueOf(item);
					var validationMessage = item.GetValidationErrorMessage(newValue, fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK);

					if (string.IsNullOrEmpty(validationMessage))
					{
						successfulItems.Add(item);
					}
					else if (UserWantsToCancelFromError(item, validationMessage))
					{
						return null;
					}
				}
			}

			return successfulItems;
		}

		bool UserWantsToCancelFromError(IRegistryItem item, string validationMessage)
		{
			using (ZFormStrategy.SuppresseNewFormInTransactionWarning())
			{
				var result = Globals.Message.Show(
					Res.GetString("73922C63-0017-4E97-83B6-3BCA96FBC93E", "When attempting to set the value of '{0}', the following error occurred:\r\n\t{1}\r\n\r\nWould you like to skip this item and continue?", GetNameForDisplay(item), validationMessage),
					Res.GetString("B037B6EF-1388-4450-A753-455294785B96", "Error when setting a registry item"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Error);

				return result == DialogResult.No;
			}
		}

		void AddChangedLogForItem(IRegistryItem item, IOverrideLevel level)
		{
			var stmDataRow = factory.Load<StmData>(level.GetPkOfEntry(item));
			if (stmDataRow != null)
			{
				var logs = stmDataRow.GetLogs();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				logs.AddNew(AutoEvents.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				factory.Save();
			}
		}

		static string GetNameForDisplay(IRegistryItem item)
		{
			return item.Categories.First().Replace("/", " > ") + " > " + item.Name;
		}

		void btnSelectDifferent_Click(object sender, EventArgs e)
		{
			ShowApplyFormForAnotherLevel(BusinessEntity.AllComparedItems, BusinessEntity.OverrideLevel);
		}
	}
}
