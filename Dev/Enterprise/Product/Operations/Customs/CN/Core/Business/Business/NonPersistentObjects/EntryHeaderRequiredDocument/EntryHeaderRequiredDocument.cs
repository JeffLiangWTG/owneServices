using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class EntryHeaderRequiredDocument : NonPersistentBusinessObject, ICIQRequiredDocument
	{
		public EntryHeaderRequiredDocument(CIQRequiredDocument document) : base(document.Factory)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}
		readonly CIQRequiredDocument document;

		public ZString DocumentType => document.XC_DocumentType;

		public ZInt NumberOfOriginals => document.XC_NumberOfOriginals;

		public ZInt NumberOfCopies => document.XC_NumberOfCopies;

		public override string ToString()
		{
			return FormattableString.Invariant($"{DocumentType}:{NumberOfOriginals}/{NumberOfCopies}");
		}
	}
}
