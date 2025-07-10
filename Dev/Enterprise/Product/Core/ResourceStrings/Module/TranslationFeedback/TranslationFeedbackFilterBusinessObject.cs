using CargoWise.Integration;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Module
{
	public class TranslationFeedbackFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddDefaultApplicationFilter(filters);

			FilterCategory categoriesAndStatus = new FilterCategory(ResString.GetMultilingualString("ae39389f-3aab-4dc0-be6d-50f851a61822", "Categories and Status"));

			var filter = filters.AddTextFilter("Status", StmTranslationFeedbackSchema.XT_Status, new TranslationFeedbackStatusList());
			filter.MultilingualDescription = ResString.GetMultilingualString("61022c8e-cdb1-4e1f-997b-27ed601ac3b3", "Status");
			filter.Category = categoriesAndStatus;
			filter.Visibility = FilterVisibility.AlwaysVisible;

			var languages = new CodeDescriptionPairList(OLookUpEditType.Language);
			foreach (ICodeDescription language in languages.ToArray())
			{
				if (Res.IsSystemDefinedEnglish(language.Code))
				{
					languages.Remove(language);
				}
			}
			filter = filters.AddTextFilter("Language", StmTranslationFeedbackSchema.XT_Language, languages);
			filter.MultilingualDescription = ResString.GetMultilingualString("974dd979-77fd-4389-9da1-5326ac70c19e", "Language");
			filter.Category = categoriesAndStatus;
			filter.Visibility = FilterVisibility.AlwaysVisible;
			if (Res.CurrentLanguage != Res.DefaultLanguage)
			{
				filter.DefaultProperty = Res.CurrentLanguage;
			}

			filters.AddTextFilter("Source Text", StmTranslationFeedbackSchema.XT_Source).MultilingualDescription = ResString.GetMultilingualString("7bc58db5-f191-43b0-a2b1-5962ab8c200e", "Source Text");
			filters.AddTextFilter("Original Translation", StmTranslationFeedbackSchema.XT_OriginalTranslation).MultilingualDescription = ResString.GetMultilingualString("7f043618-bdf5-4565-8eb5-7f1d0be71a5b", "Original Translation");
			filters.AddTextFilter("Suggested Translation", StmTranslationFeedbackSchema.XT_SuggestedTranslation).MultilingualDescription = ResString.GetMultilingualString("bc655971-256a-4cd8-8fb7-a07f2e42c429", "Suggested Translation");

			if (TranslationFeedbackConfiguration.IsMasterDatabase)
			{
				filters.AddTextFilter("Staff", StmTranslationFeedbackSchema.XT_ClientStaffInitial).MultilingualDescription = ResString.GetMultilingualString("3917fcda-2e24-4864-ae5b-9990b62a3d0d", "Staff Code");

				FilterCategory companyCatgory = new FilterCategory(ResString.GetMultilingualString("2a610c85-c1c2-49a4-b8c4-fdaa33d6fd1f", "Company"));
				filter = filters.AddTextFilter("Enterprise Code", StmTranslationFeedbackSchema.XT_EnterpriseCode);
				filter.MultilingualDescription = ResString.GetMultilingualString("013d950e-4b38-4586-80f2-edb9ba84cf6e", "Enterprise Code");
				filter.Category = companyCatgory;

				filter = filters.AddTextFilter("Database Code", StmTranslationFeedbackSchema.XT_DatabaseCode);
				filter.MultilingualDescription = ResString.GetMultilingualString("5558704f-eb18-4add-9834-e08682c169cc", "Database Code");
				filter.Category = companyCatgory;

				filter = filters.AddTextFilter("Company Code", StmTranslationFeedbackSchema.XT_CompanyCode);
				filter.MultilingualDescription = ResString.GetMultilingualString("d4dbb6e0-8032-46f6-a266-f1481a1e087f", "Company Code");
				filter.Category = companyCatgory;
			}

			return filters;
		}

		protected void AddDefaultApplicationFilter(ModuleFilterCollection filters)
		{
			var applicationFilter = filters.AddTextFilter("Application", StmTranslationFeedbackSchema.XT_Application, new TranslationFeedbackApplicationsList());
			applicationFilter.Category = FilterCategories.Other;
			applicationFilter.MultilingualDescription = ResString.GetMultilingualString("8f014596-c973-4088-b7f1-b3e65aa4323f", "Application");
			applicationFilter.Property = TranslationFeedbackApplicationsList.Codes.Cargowise;
			applicationFilter.Visibility = FilterVisibility.AlwaysApplied;
		}
	}
}
