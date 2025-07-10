using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class QuickCalculateMenuItemManager
	{
		public QuickCalculateMenuItemManager(ZGrid grid, IRatingSupporter ratingProvider)
		{
			this.grid = grid;
			this.ratingProvider = ratingProvider;
		}

		readonly ZGrid grid;
		readonly IRatingSupporter ratingProvider;
		MenuItem quickCalculateMenuItem;

		public void AddMenuItem()
		{
			grid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			quickCalculateMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.QuickCalculate", "Quick Calculate"), QuickCalculate_Click);
			quickCalculateMenuItem.Shortcut = Shortcut.Ctrl0;
			quickCalculateMenuItem.ShowShortcut = true;
			grid.ContextMenu.MenuItems.Add(quickCalculateMenuItem);
			grid.ContextMenu.Popup += ContextMenu_Popup;
			grid.Hotkeys.RegisterHotKey(Keys.Control | Keys.NumPad0, QuickCalculate);
			grid.Hotkeys.RegisterHotKey(Keys.Control | Keys.D0, QuickCalculate);
		}

		void QuickCalculate()
		{
			if (IsQuickCalculatorAllowed())
			{
				quickCalculateMenuItem.PerformClick();
			}
		}

		bool IsQuickCalculatorAllowed()
		{
			var updateableCharge = (IQuickCalculatorCharge)grid.ListManager.GetCurrent();
			return !grid.ReadOnly && !(ChargeType.Comment.Equals(updateableCharge?.ChargeCode?.AC_ChargeType, StringComparison.InvariantCulture));
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			quickCalculateMenuItem.Enabled = grid.IsMouseOnAValidRow && IsQuickCalculatorAllowed();
		}

		void QuickCalculate_Click(object sender, EventArgs args)
		{
			RatingAdaptersProvider adaptersProvider;

			if (ratingProvider != null && (adaptersProvider = ratingProvider.AdaptersProvider) != null)
			{
				if (grid.ListManager != null && grid.ListManager.Position >= 0 && grid.ListManager.GetCurrent() != null)
				{
					var adapter = adaptersProvider.GetForQuickCalculate(new ErrorsOnlyUIInteractor());

					if (adapter != null)
					{
						if (adapter.StatusInformation.CanExecute)
						{
							var updateableCharge = (IQuickCalculatorCharge)grid.ListManager.GetCurrent();
							var allChargeCostAndAmountInfos = new List<ChargeCodeAndAmountInfo>();

							if (updateableCharge is JobConsolCost)
							{
								foreach (JobConsolCost jobConsolCost in grid.ListManager.List)
								{
									allChargeCostAndAmountInfos.Add(new ChargeCodeAndAmountInfo(jobConsolCost.ChargeCode, jobConsolCost.CostAmount, jobConsolCost.SellAmount));
								}
							}
							var quickCalculator = new JobChargeQuickCalculateBusinessObject(adapter, updateableCharge, allChargeCostAndAmountInfos);

							if ((quickCalculator.UpdateCost && string.IsNullOrEmpty(updateableCharge.CostCurrencyCode)) || (quickCalculator.UpdateSell && string.IsNullOrEmpty(updateableCharge.SellCurrencyCode)))
							{
								Globals.Message.Show(Res.GetString("5610E2AE-B44F-4C8C-A4FE-35A7ACA80899", "Please enter {0} before performing Quick Calculation.",
									quickCalculator.UpdateCost && string.IsNullOrEmpty(updateableCharge.CostCurrencyCode) && quickCalculator.UpdateSell &&
									string.IsNullOrEmpty(updateableCharge.SellCurrencyCode)
										? Res.GetString("7fe12095-d97f-4b5b-85cb-a04ecda75c19", "Cost and Sell Currencies")
										: quickCalculator.UpdateCost &&
									string.IsNullOrEmpty(updateableCharge.CostCurrencyCode)
										? Res.GetString("bc759438-d611-4b7b-bbc5-aef145cb1207", "Cost Currency")
										: Res.GetString("7ea1455e-a36a-45b8-91e8-3419ca13bf5e", "Sell Currency")));

								return;
							}

							ZFormModaliser.Show(new JobChargeQuickCalculateForm(quickCalculator), grid.FindForm());
						}
						else
						{
							Globals.Message.Show(adapter.StatusInformation.Message);
						}
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("c57a77e1-90e8-4df6-9c96-ea5be4c26c15", "Please click on a row before performing Quick Calculation."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("31dbd3d8-52c0-4954-89a3-67865cfb1d5d", "You can only perform Quick Calculations for jobs that support Auto Rating."));
			}
		}

		class ErrorsOnlyUIInteractor : Enterprise.Integration.Rating.IAutoRatingInteractor
		{
			public bool YesNoWarning(string warningMessage)
			{
				return true;
			}

			public void Log(LogType type, string message)
			{
				Globals.Message.ShowError(message);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				Globals.Message.ShowError(message);
			}
		}
	}
}

