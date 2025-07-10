using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Customs.Common.GUI.Res;

namespace Enterprise.Customs.Common.Module.Testing
{
	public class CusEntryNumberFilterStripHelperTest : TestCaseWithFactory
	{
		public void TestAddFilters()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			CusEntryNumberFilterStripHelper.AddFilters(filters, typeof(DummyBusinessObject));
			AssertNotNull("AdditionalReferenceNumber filter", filters[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumber]);
			Assert("AdditionalReferenceNumber filter", filters[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumber] is CusEntryNumTextFilter);
			AssertNotNull("AdditionalReferenceNumberIssueDate filter", filters[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate]);
			Assert("AdditionalReferenceNumberIssueDate filter", filters[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate] is CusEntryNumDateFilter);
		}

		public void TestFiltersShouldShowTranslationCorrectly()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				mockChs.Put("Customs|CusEntryNumberFilterStripHelper|AdditionalReferenceNumber", new ResourceStringData("Customs|CusEntryNumberFilterStripHelper|AdditionalReferenceNumber", "Translated Additional Reference Number"));
				mockChs.Put("Customs|CusEntryNumberFilterStripHelper|AdditionalReferenceNumberIssueDate", new ResourceStringData("Customs|CusEntryNumberFilterStripHelper|AdditionalReferenceNumberIssueDate", "Translated Additional Ref No Issue Date"));
				var filters = new ModuleFilterCollection();
				CusEntryNumberFilterStripHelper.AddFilters(filters, typeof(DummyBusinessObject));
				AssertEquals("AdditionalReferenceNumber filter description", "Translated Additional Reference Number", filters[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumber].MultilingualDescription);
				AssertEquals("AdditionalReferenceNumberIssueDate filter description", "Translated Additional Ref No Issue Date", filters[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate].MultilingualDescription);
			}
		}
	}
}
