using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ReferencesWrapper : IReferences
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ReferencesWrapper(CusEntryHeader entryHeader)
		{
			this.cusEntryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");
			this.declaration = Argument.NotNull(this.cusEntryHeader.Declaration, "JobDeclaration cannot be null");
		}

		#region Members
		public ZString OwnerDeclarationIdentification => cusEntryHeader?.CH_BGMReference ?? ZString.Empty;

		public ZString CusDeclarationNumber => cusEntryHeader?.EntryNumber ?? ZString.Empty;

		#endregion

		protected readonly CusEntryHeader cusEntryHeader;
		protected readonly JobDeclaration declaration;
	}
}
