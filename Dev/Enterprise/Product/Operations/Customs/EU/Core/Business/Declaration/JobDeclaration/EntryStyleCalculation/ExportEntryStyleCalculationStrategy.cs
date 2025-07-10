using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using CodeTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ExportEntryStyleCalculationStrategy : DeclarationTypeBasedEntryStyleCalculationStrategy
	{
		public ExportEntryStyleCalculationStrategy(BusinessObjectFactory factory, ZString declarationCountryCode, RefCountry originCountry, IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider) : base(factory, declarationCountryCode, originCountry, fallbackInfoProvider)
		{
		}

		protected override ZString DefaultEntryStyle => EntryStyleListExport.Codes.ExportNormal;

		protected override ZString CalculateBasedOn15And17CodeTypes(BusinessObjectFactory factory, ZString declarationCountryCode, ZString originCountryCode)
		{
			var result = ZString.Empty;
			var codeTypes = new ZString[] { CodeTypes.Code.Code_CO17, CodeTypes.Code.Code_EX17, CodeTypes.Code.Code_EU17 };
			var list = ZZRefCusCodeListCombined.Loader.LoadByCode(factory, declarationCountryCode, codeTypes, originCountryCode, ZDateTime.Today).Select(x => x.ZZD_CodeType);

			if (list.Contains(CodeTypes.Code.Code_CO17) && !list.Contains(CodeTypes.Code.Code_EX17) && !list.Contains(CodeTypes.Code.Code_EU17))
			{
				result = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			}
			else if (list.Contains(CodeTypes.Code.Code_EX17) && !list.Contains(CodeTypes.Code.Code_CO17) && !list.Contains(CodeTypes.Code.Code_EU17))
			{
				result = EntryStyleListExport.Codes.ExportNormal;
			}
			else if (IsNotUCC6 && list.Contains(CodeTypes.Code.Code_EU17) && !list.Contains(CodeTypes.Code.Code_EX17) && !list.Contains(CodeTypes.Code.Code_CO17))
			{
				result = EntryStyleListExport.Codes.ExportToEFTAMember;
			}
			return result;
		}
	}
}
