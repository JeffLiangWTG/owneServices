using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationProcessRelatedProvider : IDeclarationProcessRelated
	{
		public DeclarationProcessRelatedProvider(CusEntryNumber entryNumber)
		{
			this.entryNumber = Argument.NotNull(entryNumber, nameof(entryNumber));
		}

		public static DeclarationProcessRelatedProvider New(CusEntryNumber entryNumber) => entryNumber == null ? null : new DeclarationProcessRelatedProvider(entryNumber);

		readonly CusEntryNumber entryNumber;

		public string ReferenceTypeCode => ProcessRelatedTypeList.MapToCustomsCode(entryNumber.CE_EntryType);
		public string ReferenceNumber => entryNumber.CE_EntryNum;
	}
}


