using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public static class CMRReferenceFilesTestHelper
	{
		public static void InsertThesaurusWithDefaultData(BusinessObjectFactory factory)
		{
			var elementsToAdd = new[]
			{
				"Traditional medicine",
				"Medicine",
				"Gun",
				"Brandy",
				"Gas",
				"Pyrotechnic device",
				"Pharmaceutical",
				"Pill",
				"Plutonium",
				"Micro organism",
				"Cigar",
				"Household goods",
			};

			InsertThesaurusData(factory, elementsToAdd);
		}

		public static void InsertThesaurusData(BusinessObjectFactory factory, params string[] stopPhrases)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.SACTH, "CMR SAC Thesaurus", Core.Constants.CountryCodes.Australia);

			foreach (var stopPhrase in stopPhrases)
			{
				InsertThesaurusData(helper, stopPhrase);
			}
			factory.Save();
		}
		static void InsertThesaurusData(UniversalReferenceTestDataHelper helper, string descriptionAndCode) => InsertThesaurusData(helper, descriptionAndCode, descriptionAndCode);

		static void InsertThesaurusData(UniversalReferenceTestDataHelper helper, string description, string code)
		{
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.SACTH, code, description, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		}
	}
}
