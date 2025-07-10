using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEntryLineCollection : NonPersistentBusinessObjectCollection<ExportEntryLine>
	{
		public ExportEntryLineCollection(CusEntryHeader entryHeader) : base(entryHeader?.Factory)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			AddRange(entryHeader.MergedLines.Select(x => new ExportEntryLine(x)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
