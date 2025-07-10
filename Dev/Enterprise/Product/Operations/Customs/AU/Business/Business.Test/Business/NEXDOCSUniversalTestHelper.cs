using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCSUniversalTestHelper
	{
		public static void SetUp()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType("NTRTT", "NEXDOCS Treatment Code");
			helper.CreateNewOrGetExistingCusCodeType("NPCKT", "NEXDOCS Pack Type");
			helper.CreateNewOrGetExistingCusCodeType("NPERT", "NEXDOCS Related Export Permit Authority");
			helper.CreateNewOrGetExistingCusCodeType("NPKGT", "NEXDOCS Related Package Type");
			helper.CreateNewOrGetExistingCusCodeType("NNOCT", "NEXDOCS Nature of Commodity");
			helper.CreateNewOrGetExistingCusCodeType("NPRST", "NEXDOCS Preserve Type");
			helper.CreateNewOrGetExistingCusCodeType("NUOMV", "NEXDOCS Unit of Measurement");
			helper.CreateNewOrGetExistingCusCodeType("NSUPP", "NEXDOCS Supplementary Code");

			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var date3 = new ZDateTime(2019, 3, 3);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "D HEAT", "DRY HEAT", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NTRTT", "TR4", "Test treatment", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "CORE", "CORE", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "FUNGIC", "FUNGIC COOL", date1, date2);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", "PO", "Pods", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NPCKT", "PU", "TRAY PACK", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", "PT", "PALLET", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", "CS", "BONE-IN CARCASE QUARTERS AND SIDES", date1, date2);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NPERT", "WEA", "WHEAT EXPORT AUTHORITY", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NPERT", "CSH", "DEPT OF HEALTH SERVICES - DRUG TREA", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPERT", "WBC", "AUSTRALIAN WINE AND BRANDY CORPORAT", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPERT", "HBE", "DEPT. OF HEALTH SERVICES - BLOOD EX", date1, date2);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NPKGT", "PT", "PALLET", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NPKGT", "SP", "SHATTER PACK", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPKGT", "TY", "TANK, CYLINDRICAL", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPKGT", "VI", "VIALS", date1, date2);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NNOCT", "AQ", "Aquaculture", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NNOCT", "BP", "Animal By-Product", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NNOCT", "CS", "Carcass-Side", date1, date3);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", "H", "cooked", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NPRST", "M", "comminuted", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", "D", "dried", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", "UR", "unrefrigerated", date1, date2);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NUOMV", "BIL", "billion", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NUOMV", "BLD", "DRY BARREL", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NUOMV", "BLL", "BARREL", date1, date3);

			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", "EK", "MORE EU BALAI PRODUCT", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NSUPP", "E", "ELECTRICAL STIMULATION", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", "X", "BARROW PORK - SINGAPORE ONLY", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", "DM", "MANUFACTURING GRADE", date1, date2);

			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "H", "AMF", "PO", "EK");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "M", "AMF", "PU", "E");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "D", "AMF", "PT", "X");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "UR", "AMF", "CS", "DM");
			factory.Save();
		}
	}
}
