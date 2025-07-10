using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class ImportEntryLineWrapper : NonPersistentBusinessObject
	{
		public ImportEntryLineWrapper(IImportEntryLine entryLine, ZDecimal uSDRate, ZString entryLinePackTitle, ZBool isFirstItem)
		{
			this.EntryLine = entryLine;
			this.isFirstItem = isFirstItem;
			this.uSDRate = uSDRate;
			this.entryLinePackTitle = entryLinePackTitle;
		}
		public IImportEntryLine EntryLine { get; }
		public ZBool isFirstItem { get; }
		readonly ZDecimal uSDRate;
		readonly ZString entryLinePackTitle;
		public ZDecimal CustomsValueUSD
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (EntryLine.CustomsValueKRW > 0 && uSDRate > 0)
				{
					result = Utilities.Round(EntryLine.CustomsValueKRW / uSDRate, 0);
				}
				return result;
			}
		}

		public ZString FirstEntryLinePackTitle
		{
			get
			{
				if (!firstEntryLinePackTitle.HasValue)
				{
					firstEntryLinePackTitle = isFirstItem ? entryLinePackTitle : ZString.Empty;
				}
				return firstEntryLinePackTitle.Value;
			}
		}
		ZString? firstEntryLinePackTitle;
	}
}
