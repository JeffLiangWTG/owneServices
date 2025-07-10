using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.Testing
{
	sealed class DocMAFCoverSheetPageTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			NZDocsMAFCSContainer container1 = CoverSheet.Containers.AddNew();
			container1.D2_ContainerNumber = "CONTAINER1";
			container1.D2_IsFCL = true;
			container1.D2_IsLCL = false;

			NZDocsMAFCSCommodity commodity1 = CoverSheet.Commodities.AddNew();
			commodity1.D1_CommodityOrSpecies = "CUCKOO SQUEAKER";
			commodity1.D1_MeasureWithUnit = "1 ZOMG";
			commodity1.D1_QuantityWithUnit = "2 EPIX";
			return new DocMAFCoverSheetPage(Factory, new DocMAFCoverSheetCommodityCollection(CoverSheet.Commodities, 1, 5), new DocMAFCoverSheetContainerCollection(CoverSheet.Containers, 1, 5));
		}

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
