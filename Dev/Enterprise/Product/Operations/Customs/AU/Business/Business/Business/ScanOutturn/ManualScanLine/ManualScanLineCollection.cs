using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ManualScanLineCollection : NonPersistentBusinessObjectCollection<ManualScanLine>
	{
		public ManualScanLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			barcodeList = new List<string>();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ManualScanLine();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!(bizOAdded is ManualScanLine))
			{
				throw new InvalidOperationException();
			}

			var line = (ManualScanLine)bizOAdded;
			if (!ContainsBarcode(line.Barcode))
			{
				barcodeList.Add(line.Barcode);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (!(bizO is ManualScanLine))
			{
				throw new InvalidOperationException();
			}
			var line = (ManualScanLine)bizO;
			if (ContainsBarcode(line.Barcode))
			{
				barcodeList.Remove(line.Barcode);
			}
		}

		readonly List<string> barcodeList;

		public bool ContainsBarcode(string barcode)
		{
			return barcodeList.Contains(barcode);
		}

		public int CountNumberOfManualScansByBarcode(string barcode)
		{
			return this.Cast<ManualScanLine>().Count(line => line.Barcode == barcode);
		}
	}
}
