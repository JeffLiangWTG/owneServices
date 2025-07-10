using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business
{
	public abstract class ImportHeaderProvider : IImportHeader
	{
		protected ImportHeaderProvider(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Declaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
		}
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;

		protected BusinessObjectFactory Factory => EntryHeader.Factory;

		public string LocalReferenceNumber => EntryHeader.LocalReferenceNumber;

		protected string GetCW1OrCWPAuthorizationNumberFromDeclarantOrgHeader()
		{
			var result = string.Empty;
			var declarant = Declaration.Declarant;
			if (declarant != null)
			{
				result = CusAuthorisationHeader.Loader.GetAuthorisations(Declaration.Factory,
					Core.Constants.CountryCodes.Germany,
					new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP },
					ZDate.Today,
					declarant.Header.PK
				).FirstOrDefault()?.CPH_Number ?? string.Empty;
			}
			return result;
		}
	}
}
