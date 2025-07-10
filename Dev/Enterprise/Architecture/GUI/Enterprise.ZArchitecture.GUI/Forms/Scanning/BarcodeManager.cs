using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Scanning
{
	public class BarcodeManager
	{
		public event EventHandler<BarcodeScanEventArgs> NonSystemBarcodeScanned;

		public void AddBarcode(ZString barcode, Action actionToPerform)
		{
			Argument.NotNullOrEmpty(barcode, "barcode");
			Argument.NotNull(actionToPerform, "actionToPerform");

			Actions.Add(barcode, actionToPerform);
		}

		public bool ExecuteBarcode(ZString barcode)
		{
			Action action;
			var trimmedBarcode = barcode.Trim();
			var barcodeHandled = Actions.TryGetValue(trimmedBarcode, out action);

			if (barcodeHandled)
			{
				action();
			}
			else if (NonSystemBarcodeScanned != null)
			{
				// give consumer an opportunity to handle the scan
				var args = new BarcodeScanEventArgs(trimmedBarcode);
				NonSystemBarcodeScanned(this, args);
				barcodeHandled = args.Handled;
			}

			return barcodeHandled;
		}

		Dictionary<ZString, Action> Actions
		{
			get { return actions ?? (actions = new Dictionary<ZString, Action>()); }
		}

		Dictionary<ZString, Action> actions;
	}
}
