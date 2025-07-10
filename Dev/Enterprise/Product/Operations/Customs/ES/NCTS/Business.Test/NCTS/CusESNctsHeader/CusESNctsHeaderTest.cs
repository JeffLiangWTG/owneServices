using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(CusESNctsHeader))]
	class CusESNctsHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCEN_SummaryType_Captions()
		{
			CombineAssertions(() =>
			{
				var dataForSummaryType = DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_SummaryTypeInfo);
				AssertEquals("Caption", "Summary Type", dataForSummaryType.Caption);
				AssertEquals("MediumCaption", "Summary Type", dataForSummaryType.MediumCaption);
				AssertEquals("ShortCaption", "Summary Type", dataForSummaryType.ShortCaption);
			});
		}

		public void TestCEN_SummaryType_ListAttribute()
		{
			var esHeader = NctsHeader.ESNctsHeader;
			AssertHasCustomAttribute(typeof(CusESNctsHeader), CusESNctsHeaderSchema.Constants.CEN_SummaryType, includesInherit: false,
				(CargoWise.ComponentModel.ListAttribute la) => la.ListDataSourceMember == "Lookups.SummaryTypeList");
		}

		public void TestCEN_AutomaticCompletion()
		{
			AssertEquals("Caption", "Automatic Completion", DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_AutomaticCompletionInfo).Caption);
		}

		public void TestCEN_AutomaticTranshipment()
		{
			AssertEquals("Caption", "Automatic Transhipment", DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_AutomaticTranshipmentInfo).Caption);
		}

		public void TestCEN_TIRArrival()
		{
			AssertEquals("Caption", "TIR Arrival", DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_TIRArrivalInfo).Caption);
		}

		public void TestCEN_TIRPartialUnloading()
		{
			AssertEquals("Caption", "TIR Partial Unloading", DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_TIRPartialUnloadingInfo).Caption);
		}

		public void TestCEN_TIRCarnetPage()
		{
			AssertEquals("Caption", "TIR Carnet Page", DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_TIRCarnetPageInfo).Caption);
		}

		public void TestCEN_PreviousSummaryDeclaration_Captions()
		{
			CombineAssertions(() =>
			{
				var dataForPrevSummary = DataBoundResourceStrings.GetDataForProperty(NctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclarationInfo);
				AssertEquals("Caption", "Previous Summary", dataForPrevSummary.Caption);
				AssertEquals("MediumCaption", "Previous Summary", dataForPrevSummary.MediumCaption);
				AssertEquals("ShortCaption", "Prev. Summary", dataForPrevSummary.ShortCaption);
			});
		}

		public void TestIsShowTIRArrivalDetails()
		{
			CombineAssertions(() =>
			{
				NctsHeader.ESNctsHeader.CEN_TIRArrival = ZBool.True;
				AssertEquals($"CEN_TIRArrival={NctsHeader.ESNctsHeader.CEN_TIRArrival}", true, NctsHeader.ESNctsHeader.IsShowTIRArrivalDetails);
				NctsHeader.ESNctsHeader.CEN_TIRArrival = ZBool.False;
				AssertEquals($"CEN_TIRArrival={NctsHeader.ESNctsHeader.CEN_TIRArrival}", false, NctsHeader.ESNctsHeader.IsShowTIRArrivalDetails);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.ESNctsHeader;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.ESNctsHeader;
		}

		NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = Factory.New<NctsHeader>());
		NctsHeader nctsHeader;
	}
}
