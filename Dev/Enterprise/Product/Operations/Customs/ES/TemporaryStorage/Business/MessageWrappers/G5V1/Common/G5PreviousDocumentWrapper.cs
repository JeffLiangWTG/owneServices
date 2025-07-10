using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5PreviousDocumentWrapper : IG5PreviousDocument
	{
		public G5PreviousDocumentWrapper(ZString customsOffice, TemporaryStoragePreviousDocument doc)
		{
			document = Argument.NotNull(doc, nameof(doc));
			customsOfficeIsES = customsOffice.StartsWith(Core.Constants.CountryCodes.Spain);
		}
		readonly TemporaryStoragePreviousDocument document;
		readonly ZBool customsOfficeIsES;

		public IG5PreviousTSD PreviousTSD => previousTSD ??= customsOfficeIsES ? new G5PreviousTSDWrapper(document) : null;
		G5PreviousTSDWrapper previousTSD;

		public ICommonDocumentGoodsItemId PreviousGeneric => previousGeneric ??= customsOfficeIsES ? null : new G5GenericPreviousDocumentWrapper(document);
		G5GenericPreviousDocumentWrapper previousGeneric;
	}
}
