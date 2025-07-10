using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.GUI.Testing
{
	sealed class PBNUserControlTest : TestCaseWithFactory
	{
		public void TestCustomsDeclarationsGrid()
		{
			using (var form = new ASYCUDA.GUI.ManifestForm(Factory.New<AsycudaManifestHeader>()))
			{
				form.Show();
				var customsReferencesGrid = form.FindSingle<ZGrid>("CustomsReferencesGrid");
				AssertSequencesEqual(ExpectedOrderedColumns, customsReferencesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestTransitDeclarationsGrid()
		{
			using (var form = new ASYCUDA.GUI.ManifestForm(Factory.New<AsycudaManifestHeader>()))
			{
				form.Show();
				var transitReferencesGrid = form.FindSingle<ZGrid>("TransitReferencesGrid");
				AssertSequencesEqual(ExpectedOrderedColumns, transitReferencesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		string[] ExpectedOrderedColumns =>
		[
			PBNCustomsDeclarationItem.Schema.CSI_Code,
			PBNCustomsDeclarationItem.Schema.CSI_ReferenceNumber,
			PBNCustomsDeclarationItem.Schema.CSI_Status,
			PBNCustomsDeclarationItem.Schema.CSI_DateOfIssue,
			PBNCustomsDeclarationItem.Schema.CSI_RN_NKCountryCode,
		];
	}
}
