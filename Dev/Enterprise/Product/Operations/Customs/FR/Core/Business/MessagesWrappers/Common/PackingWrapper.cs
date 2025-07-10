using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class PackingWrapper : IPacking
	{
		public PackingWrapper(CusEntryLine currentEntryLine)
		{
			entryLine = Argument.NotNull(currentEntryLine, nameof(currentEntryLine));
			package = entryLine.Package;
			packageCount = package?.Count ?? 0;
			packageItemsCount = package?.ItemsCount.ToZInt() ?? 0;
			isUnpacked = package?.IsUnpacked ?? false;
		}

		public ZInt Count => isUnpacked ? ZInt.Zero : packageCount;

		public ZInt ItemsCount => isUnpacked ? packageItemsCount : ZInt.Zero;

		public ZString Type => package?.Type ?? ZString.Empty;

		public ZString MarksAndNos => package?.MarksAndNos ?? ZString.Empty;

		readonly CusEntryLine entryLine;
		readonly CusEntryLinePack package;
		readonly ZInt packageCount;
		readonly ZInt packageItemsCount;
		readonly bool isUnpacked;
	}
}
