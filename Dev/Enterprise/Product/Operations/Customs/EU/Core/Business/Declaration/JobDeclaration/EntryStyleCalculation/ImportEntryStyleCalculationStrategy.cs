using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using CodeTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ImportEntryStyleCalculationStrategy : DeclarationTypeBasedEntryStyleCalculationStrategy
	{
		public ImportEntryStyleCalculationStrategy(BusinessObjectFactory factory, ZString declarationCountryCode, RefCountry originCountry, IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider) : base(factory, declarationCountryCode, originCountry, fallbackInfoProvider)
		{
		}

		protected override ZString DefaultEntryStyle => EntryStyleListImport.Codes.ImportNormal;

		protected override ZString CalculateBasedOn15And17CodeTypes(BusinessObjectFactory factory, ZString declarationCountryCode, ZString originCountryCode)
		{
			var result = ZString.Empty;
			var codeTypes = new ZString[] { CodeTypes.Code.Code_CO15, CodeTypes.Code.Code_IM15, CodeTypes.Code.Code_EU15 };
			var list = ZZRefCusCodeListCombined.Loader.LoadByCode(factory, declarationCountryCode, codeTypes, originCountryCode, ZDateTime.Today).Select(x => x.ZZD_CodeType);

			if (list.Contains(CodeTypes.Code.Code_CO15) && !list.Contains(CodeTypes.Code.Code_IM15) && !list.Contains(CodeTypes.Code.Code_EU15))
			{
				result = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			}
			else if (list.Contains(CodeTypes.Code.Code_IM15) && !list.Contains(CodeTypes.Code.Code_CO15) && !list.Contains(CodeTypes.Code.Code_EU15))
			{
				result = EntryStyleListImport.Codes.ImportNormal;
			}
			else if (list.Contains(CodeTypes.Code.Code_EU15) && !list.Contains(CodeTypes.Code.Code_IM15) && !list.Contains(CodeTypes.Code.Code_CO15))
			{
				result = EntryStyleListImport.Codes.ImportFromEFTAMember;
			}
			return result;
		}
	}
}
