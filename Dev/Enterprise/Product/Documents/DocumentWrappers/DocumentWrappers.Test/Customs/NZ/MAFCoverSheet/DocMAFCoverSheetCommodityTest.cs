using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.Testing
{
	sealed class DocMAFCoverSheetCommodityTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DocMAFCoverSheetCommodity(Commodity, Factory);
		}

		#region Container

		NZDocsMAFCSCommodity Commodity
		{
			get
			{
				if (fCommodity == null)
				{
					fCommodity = CoverSheet.Commodities.AddNew();
					fCommodity.D1_CommodityOrSpecies = "ZOMG A COMMODITY";
					fCommodity.D1_MeasureWithUnit = "Measure";
					fCommodity.D1_QuantityWithUnit = "Quantity";
				}
				return fCommodity;
			}
		}
		NZDocsMAFCSCommodity fCommodity;

		#endregion

		#region CoverSheet
		NZDocsMAFCoverSheet CoverSheet
		{
			get
			{
				if (fCoverSheet == null)
				{
					fCoverSheet = new NZDocsMAFCoverSheet(TestDataBuilder.GetMAFMessaging(Declaration));
				}
				return fCoverSheet;
			}
		}
		NZDocsMAFCoverSheet fCoverSheet;
		#endregion

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion
	}
}
