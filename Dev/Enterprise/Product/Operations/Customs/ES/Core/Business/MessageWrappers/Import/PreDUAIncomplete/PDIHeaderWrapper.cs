using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PDIHeaderWrapper : IPDIHeader
	{
		public PDIHeaderWrapper(CusEntryHeader cusEntryHeader)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			declaration = entryHeader.Declaration;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public ZString CustomsOffice => declaration.JE_CustomsOffice.SubstringSafe(declaration.JE_CustomsOffice.Length - 6);

		public ZString ShipmentType => declaration.JE_MessageSubType;

		public ZInt TotalLinesNum => entryHeader.MergedLines.Count;

		public IImportImporterProvider Importer => CachedValueHelper.GetValue(ref importer, () => ImportImporterWrapper.New(declaration.ImporterDocumentaryAddress, declaration));
		CachedValue<IImportImporterProvider> importer;

		public IImportDeclarantPartyIdProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => ImportDeclarantPartyIdWrapper.New(declaration));
		CachedValue<IImportDeclarantPartyIdProvider> declarant;

		public ZString DeclarationEmail => declaration.DeclEmailAddr;

		public ZString OtherEmail => declaration.ZG_OtherEmailAddr;

		public ZString OriginCountry => declaration.JE_GoodsOrigin;

		public ZString GoodsLocation
		{
			get
			{
				var location = entryHeader.EntryInstruction.GoodsLocation.Address.AuthorisationNumber;
				return location.Length > 10 ? location.SubstringSafe(4, 4) : location.Left(4);
			}
		}
	}
}
