using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class JobRequiredDocumentAddInfoFilterStripControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridFields()
		{
			var collection = new JobRequiredDocumentAddInfoCollection(Factory);
			var filterBO = new JobRequiredDocumentAddInfoFilterBusinessObject();

			using (var filterControl = new JobRequiredDocumentAddInfoFilterStripControl(collection, filterBO))
			{
				filterControl.Show();
				AssertNotNull("URN", filterControl.FilteredGrid.GetColumnStyle("EX_ReferenceNumber").Caption);
			}
		}
	}
}
