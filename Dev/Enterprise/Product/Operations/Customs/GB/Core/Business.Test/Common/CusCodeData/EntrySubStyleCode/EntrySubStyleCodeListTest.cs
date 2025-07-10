using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(EntrySubStyleCodeList))]
	public class EntrySubStyleCodeListTest : BusinessObjectCollectionTestCase
	{
		[TestDate(2018, 9, 29)]
		public void TestEntrySubStyleCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("ENSUB", "Entry Sub Style");
			helper.CreateCusCodeType("CSTA", "Customs Status");
			helper.CreateCusCodeType("FSIS", "US FSIS Establishment Numbers");
			helper.CreateCusCodeList("CDS", "ENSUB", "A", "Standard customs declaration (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "B", "Simplified declaration on an occasional basis (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "C", "Simplified declaration with regular use (pre-authorised) (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "D", "Standard customs declaration (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "E", "Simplified declaration on an occasional basis (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "F", "Simplified declaration with regular use (pre-authorised) (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "J", "C21 (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "K", "C21 (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "X", "Supplementary declaration covered by types B and E (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "Y", "Supplementary declaration covered by types C and F (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("CDS", "ENSUB", "Z", "Supplementary declarations for Entry in Declarants Records (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));

			helper.CreateCusCodeList("CDS", "ENSUB", "G", "Test G", new ZDateTime(2018, 9, 30), new ZDateTime(2018, 10, 30));
			helper.CreateCusCodeList("CDS", "CSTA", "01", "Declaration has been legally accepted", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("EUN", "ENSUB", "Z", "Supplementary declarations for Entry in Declarants Records (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateCusCodeList("US", "FSIS", "P21469", "P21469", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));

			Factory.Save();

			var entrySubStyleList = new EntrySubStyleCodeList(Factory);
			entrySubStyleList.Load();

			AssertEquals(11, entrySubStyleList.Count);
			Assert(entrySubStyleList.Cast<ZZRefCusCodeListCombined>().All(x => x.ZZD_CodeType == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub));
			Assert(entrySubStyleList.Cast<ZZRefCusCodeListCombined>().All(x => x.ZZD_StartDate.IsInThePast()));
			Assert(entrySubStyleList.Cast<ZZRefCusCodeListCombined>().All(x => x.ZZD_EndDate.IsInTheFuture()));
			Assert(entrySubStyleList.Cast<ZZRefCusCodeListCombined>().All(x => x.ZZD_CountryOrGrouping == Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EntrySubStyleCodeList(Factory);
		}
	}
}
