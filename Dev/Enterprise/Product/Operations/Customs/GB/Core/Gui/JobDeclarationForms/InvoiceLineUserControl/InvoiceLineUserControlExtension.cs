using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public static class InvoiceLineUserControlExtension
	{
		public static void SetTariffFindBox(this EUInvoiceLineUserControl control, bool useUniversalTariff)
		{
			var controlMatched = (useUniversalTariff && control.tariffFindBox is Universal.GUI.TariffFindBox) || (!useUniversalTariff && control.tariffFindBox is TariffFindBox);
			if (!controlMatched)
			{
				control.ClassificationDetailsGroupBox.Controls.Remove(control.tariffFindBox);
				if (!useUniversalTariff)
				{
					control.tariffFindBox = new TariffFindBox
					{
						ParameterForediTariff = GetParameterForediTariff(control)
					};
				}
				else
				{
					control.CreateUniversalTariffFindBox();
				}
				control.SetTariffFindBoxProperty();
			}
		}

		public static void SetTariffColumnStyleInfo(this EUInvoiceLineUserControl control, bool useUniversalTariff)
		{
			var gridColumnStyleInfoForFormattedTariff = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(control.TariffColumnName);
			var controlMatched = (useUniversalTariff && gridColumnStyleInfoForFormattedTariff is Universal.GUI.TariffColumnStyleInfo) || (!useUniversalTariff && gridColumnStyleInfoForFormattedTariff is TariffColumnStyleInfo);
			if (!controlMatched)
			{
				if (gridColumnStyleInfoForFormattedTariff != null)
				{
					control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(gridColumnStyleInfoForFormattedTariff);
				}

				if (!useUniversalTariff)
				{
					var tariffColumnStyleInfo = new TariffColumnStyleInfo()
					{
						ParameterForediTariff = GetParameterForediTariff(control),
						ColumnName = control.TariffColumnName
					};
					control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(6, tariffColumnStyleInfo);
				}
				else
				{
					control.AddUniversalTariffColumnToGrid();
				}
			}
		}

		static string GetParameterForediTariff(EUInvoiceLineUserControl control) => control is ExportInvoiceLineUserControl ? "E" : "I";
	}
}
