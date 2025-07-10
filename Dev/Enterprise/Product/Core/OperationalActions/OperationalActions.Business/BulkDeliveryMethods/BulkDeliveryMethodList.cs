using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class BulkDeliveryMethodList : ReadOnlyCodeDescriptionPairList
	{
		public BulkDeliveryMethodList()
		{
			Elements.Add(new AutoBulkDeliveryMethod());
			Elements.Add(new ForcePrinterBulkDeliveryMethod());
			Elements.Add(new OnlyPrinterBulkDeliveryMethod());
			Elements.Add(new OnlyElectronicBulkDeliveryMethod());
		}

		public new BulkDeliveryMethod this[int index]
		{
			get { return (BulkDeliveryMethod)base[index]; }
		}

		public BulkDeliveryMethod this[string code]
		{
			get
			{
				int index = IndexOfCode(code);
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(code), code, "unknown delivery method");
				}

				return this[index];
			}
		}

		public bool UsesPrinter(string code)
		{
			int index = IndexOfCode(code);
			return index < 0 || this[index].UsesPrinter;
		}

		public bool AllowCoverNote(string code)
		{
			int index = IndexOfCode(code);
			return index >= 0 && this[index].AllowCoverNote;
		}

		public bool AllowDeliverDocumentsInOneEmail(string code)
		{
			int index = IndexOfCode(code);
			return index >= 0 && this[index].AllowDeliverDocumentsInOneEmail;
		}
	}
}
