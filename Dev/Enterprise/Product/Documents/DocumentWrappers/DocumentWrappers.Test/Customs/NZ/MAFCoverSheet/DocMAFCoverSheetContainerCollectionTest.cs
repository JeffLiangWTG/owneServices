using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.Testing
{
	[TestedType(typeof(DocMAFCoverSheetContainerCollection))]
	sealed class DocMAFCoverSheetContainerCollectionTest : DocBaseWrapperCollectionTest<DocMAFCoverSheetContainerCollection>
	{
		protected override DocMAFCoverSheetContainerCollection GetNewDocumentWrapperCollection()
		{
			AssertEquals("ZOMG A CONTAINER", Container.D2_ContainerNumber);
			DocMAFCoverSheetContainerCollection collection = new DocMAFCoverSheetContainerCollection(CoverSheet.Containers, 1, 5);
			return collection;
		}

		protected override object GetNewObjectToWrap()
		{
			return Container;
		}

		#region Container

		NZDocsMAFCSContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = CoverSheet.Containers.AddNew();
					fContainer.D2_ContainerNumber = "ZOMG A CONTAINER";
					fContainer.D2_IsFCL = true;
					fContainer.D2_IsLCL = false;
				}
				return fContainer;
			}
		}
		NZDocsMAFCSContainer fContainer;

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
