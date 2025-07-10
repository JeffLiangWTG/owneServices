using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalTariffLookups : ZLookups
	{
		public AdditionalTariffLookups(AdditionalTariff parent)
			: base(parent)
		{
		}

		public new AdditionalTariff Parent => (AdditionalTariff)base.Parent;

		public CodeDescriptionPairList LegalActSubjectList => Parent.GetLegalActSubjects();

		public CodeDescriptionPairList TariffTypeList => AdditionalTariff.GetTariffTypeListByLegalActSubject(Factory, Parent.Parent.EffectiveAssessmentDate, Parent.LegalActSubject);

		public ChildTariffViewCollection ChildTariffs
		{
			get
			{
				ZString dataGroupingCode = Core.Constants.CountryCodes.Brazil;
				var parentTariff = Parent.Parent.Tariff;

				var collection = ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Brazil,
														ZString.Empty, Parent.Parent.EffectiveAssessmentDate,
														Universal.Constants.TariffTypes.HarmonizedSystem, parentTariff);

				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Code", "Property", parentTariff, isRemovable: true));

				if (Parent.TariffType.IsEmpty)
				{
					ZInt instance = 1;
					foreach (var tariffType in new ChildTariffTypeList().GetAllCodesZString())
					{
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Type", "Property1", dataGroupingCode, ZArchitecture.Business.FilterOrCategory.Red, instance));
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Type", "Property2", tariffType, ZArchitecture.Business.FilterOrCategory.Red, instance));

						instance++;
					}
				}
				else
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Type", "Property1", dataGroupingCode, isRemovable: false));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Type", "Property2", Parent.TariffType, isRemovable: false));
				}

				return collection;
			}
		}
	}
}
