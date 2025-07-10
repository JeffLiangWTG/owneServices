using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class NFeEntryExportObjectCollection : NonPersistentBusinessObjectCollection<NFeEntryExportObject>
	{
		public NFeEntryExportObjectCollection(JobDeclaration declaration)
		: base(declaration.Factory)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Creating of new elements is not allowed.");
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			var entryHeaders = declaration.ActiveEntryHeaders.FormalEntries.ToList();

			foreach (var removedEntry in this.Cast<NFeEntryExportObject>().Where(x => !entryHeaders.Contains(x.EntryHeader)).ToArray())
			{
				RemoveAndDelete(removedEntry);
			}

			AddRange(entryHeaders.Where(x => FindByEntryHeader(x) == null).Select(x => NFeEntryExportObject.New(x)));

			foreach (var entryExportObject in this.Cast<NFeEntryExportObject>())
			{
				entryExportObject.Lines.Load();
			}
		}

		NFeEntryExportObject FindByEntryHeader(CusEntryHeader entryHeader) => this.Cast<NFeEntryExportObject>().FirstOrDefault(x => x.EntryHeader == entryHeader);
	}
}
