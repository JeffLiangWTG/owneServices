using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public static class ControlExtensionMethods
	{
		public static ZGrid AddExchangeRateColumn(this ZGrid grid)
		{
			grid.ColumnStyles.Add
			(
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = InvoiceCharge.Schema.J7_ExchangeRate,
					Decimals = Constants.ExchangeRateDigit,
					CaptionResourceString = Res.GetData("58C072A9-922C-4729-B983-D79D6504E405", "Exchange Rate"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				}
			);

			return grid;
		}

		public static ZGrid AddIncludedInInvoiceColumn(this ZGrid grid, ZString caption)
		{
			grid.ColumnStyles.Add
			(
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
					Caption = caption,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				}
			);
			return grid;
		}

		public static void ReOrderColumnsAndChangeVisibility(this ZGrid grid, string[] orders)
		{
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				column.IsVisible = orders.Contains(column.ColumnName);
			}
			grid.ReOrderColumns(orders);
		}

		public static string[] HeaderChargeColumnsInOrder => new string[]
		{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_IsIncludedInITOT,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy
		};
		public static string[] LineChargeColumnsInOrder => new string[]
		{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.ChargeCodeDescription,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_Percentage,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy
		};

		public static string[] GroupChargeColumnsInOrder => new string[]
		{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_Percentage,
				InvoiceCharge.Schema.J7_IsDutiable,
				BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
				InvoiceCharge.Schema.J7_DistributeBy,
				InvoiceCharge.Schema.J7_FullOrPartialApportionment
		};

		public const string BindingPathForMessageSending = "SendingObjectsCollection.InvoiceHeader.";

		public static void ChangeBindingPaths(this Control.ControlCollection controls, ZBindingSource bindingSource, string newBindingPath)
		{
			var bindableControls = controls.Cast<Control>().Where(x => x is IBindTo bindable && !string.IsNullOrEmpty(bindable.BindTo));
			foreach (IBindTo bindableControl in bindableControls)
			{
				bindableControl.BindTo = GetNewBindTo(bindableControl.BindTo);
				bindingSource.SetBindingMember((Control)bindableControl, bindableControl.BindTo);
			}
			foreach (ZCalcFindBox control in controls.Cast<Control>().Where(x => x is ZCalcFindBox))
			{
				control.BindToAmount = GetNewBindTo(control.BindToAmount);
				var boxType = control.FindBoxType;
				control.BindToUnit = GetNewBindTo(control.BindToUnit);
				control.FindBoxType = boxType;
			}

			string GetNewBindTo(string current)
			{
				var bindingMember = current;
				var existingBindingPaths = current.Split('.');
				if (existingBindingPaths.Length > 0)
				{
					bindingMember = existingBindingPaths[existingBindingPaths.Length - 1];
				}
				return newBindingPath + bindingMember;
			}
		}
	}
}
