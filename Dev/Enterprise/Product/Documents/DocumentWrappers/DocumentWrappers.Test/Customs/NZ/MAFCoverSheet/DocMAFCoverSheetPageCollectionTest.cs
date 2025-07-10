using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.Testing
{
	[TestedType(typeof(DocMAFCoverSheetPageCollection))]
	sealed class DocMAFCoverSheetPageCollectionTest : DocBaseWrapperCollectionTest<DocMAFCoverSheetPageCollection>
	{
		protected override DocMAFCoverSheetPageCollection GetNewDocumentWrapperCollection()
		{
			return new DocMAFCoverSheetPageCollection(Factory, CoverSheet);
		}

		protected override object GetNewObjectToWrap()
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

		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			DocMAFCoverSheetPage newWrapper = (DocMAFCoverSheetPage)GetNewObjectToWrap();
			collection.Add(newWrapper);
			return newWrapper;
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
