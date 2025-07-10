using System;
using CargoWise.Common;
using FlexCel.Core;

namespace Enterprise.RemotePrinting.Engine
{
	public class PrinterDriverAdapter
	{
		public PrinterDriverAdapter(ExcelFile xls)
		{
			Argument.NotNull(xls, nameof(xls));
			this.Xls = xls;
		}

		readonly ExcelFile Xls;

		public void SetPrinterDriverSettings(byte[] newSettings)
		{
			Argument.NotNull(newSettings, nameof(newSettings));
			if (newSettings.Length <= 1)
			{
				throw new ArgumentException("Invalid argument.", nameof(newSettings));
			}

			var paperSize = (newSettings[0] * 256) + newSettings[1];
			byte[] printerDriverSettings = new byte[newSettings.Length - 2];

			for (int i = 0; i < printerDriverSettings.Length; i++)
			{
				printerDriverSettings[i] = newSettings[i + 2];
			}
			Xls.ActiveSheet = 1;
			Xls.SetPrinterDriverSettings(new TPrinterDriverSettings(printerDriverSettings));
			Xls.PrintPaperSize = (TPaperSize)paperSize;
		}

		public byte[] GetPrinterDriverSettings()
		{
			byte[] result = null;

			Xls.ActiveSheet = 1;
			var driverSettings = Xls.GetPrinterDriverSettings();
			byte[] printerDriverSettings = driverSettings == null ? null : driverSettings.GetData();

			if (printerDriverSettings != null)
			{
				result = new byte[printerDriverSettings.Length + 2];
				printerDriverSettings.CopyTo(result, 2);
				Int16 paperSize = (Int16)Xls.PrintPaperSize;
				result[0] = (byte)(paperSize / 256);
				result[1] = (byte)(paperSize % 256);
			}
			return result;
		}
	}
}
