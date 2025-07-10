using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class DeclarationTypeBasedEntryStyleCalculationStrategy : IEntryStyleCalculationStrategy
	{
		protected DeclarationTypeBasedEntryStyleCalculationStrategy(BusinessObjectFactory factory, ZString declarationCountryCode, RefCountry originCountry, IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.declarationCountryCode = declarationCountryCode;
			this.originCountry = originCountry;
			this.fallbackInfoProvider = Argument.NotNull(fallbackInfoProvider, nameof(fallbackInfoProvider));
		}
		readonly BusinessObjectFactory factory;
		readonly ZString declarationCountryCode;
		readonly RefCountry originCountry;
		readonly IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider;

		ZString IEntryStyleCalculationStrategy.Calculate()
		{
			if (originCountry == null)
			{
				return fallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment();
			}

			var entryStyle = CalculateBasedOn15And17CodeTypes(factory, declarationCountryCode, originCountry.RN_Code);
			if (entryStyle.IsEmpty)
			{
				entryStyle = CalculateBasedOnFallbackProcedure(originCountry);
			}
			return entryStyle;
		}

		ZString CalculateBasedOnFallbackProcedure(RefCountry originCountry)
		{
			ZString entryStyle;
			var originCountryCode = originCountry.RN_Code;
			if (originCountryCode == declarationCountryCode)
			{
				entryStyle = fallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment();
			}
			else if (IsNotUCC6 && originCountry.IsACountryEligibleToACommonTransitProcedure() && !factory.IsMemberOfEU(originCountryCode))
			{
				entryStyle = fallbackInfoProvider.GetEntrySubStyleForCommonTransit(originCountry);
			}
			else if (originCountry.IsASpecialTerritoryOfTheCommunity() || (factory.IsMemberOfEU(originCountryCode) && originCountry.HasSpecialTerritoriesOfTheCommunity()))
			{
				entryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			}
			else if (factory.IsMemberOfEU(originCountryCode))
			{
				entryStyle = fallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment();
			}
			else
			{
				entryStyle = DefaultEntryStyle;
			}
			return entryStyle;
		}

		protected abstract ZString CalculateBasedOn15And17CodeTypes(BusinessObjectFactory factory, ZString declarationCountryCode, ZString originCountryCode);

		protected abstract ZString DefaultEntryStyle { get; }

		protected bool IsNotUCC6 => CachedValueHelper.GetValue(ref isNotUCC6, () => !(fallbackInfoProvider is JobDeclaration declaration) || !declaration.IsUCC6);
		CachedValue<bool> isNotUCC6;
	}
}
