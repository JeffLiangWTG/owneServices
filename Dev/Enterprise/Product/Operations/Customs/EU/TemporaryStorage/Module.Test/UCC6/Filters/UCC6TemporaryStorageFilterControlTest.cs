using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	sealed class UCC6TemporaryStorageFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumns()
		{
			using (var form = new ZForm())
			{
				var filterControl = new UCC6TemporaryStorageFilterControl(new TemporaryStorageHeaderCollection(Factory), new UCC6TemporaryStorageFilterStripBusinessObject());
				form.Controls.Add(filterControl);
				form.Show();

				var columnStyles = filterControl.Grid.ColumnStyles;
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_JobReference, "Job #");
				AssertColumn(columnStyles, nameof(TemporaryStorageHeader.LRN), "LRN");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_DateAtCustomsOffice, "Presentation Date");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_CustomsOffice, "Customs Office");
				AssertColumn(columnStyles, nameof(TemporaryStorageHeader.PresentationCustomsOffice), "Presentation Customs Office");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_GB, "Branch");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_OA_Declarant, "Declarant");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_OA_Presenter, "Presenter");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_OA_Representative, "Representative");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_OA_Carrier, "Carrier");
				AssertColumn(columnStyles, nameof(TemporaryStorageHeader.PlaceOfUnloading), "Place of Unloading");
				AssertColumn(columnStyles, nameof(TemporaryStorageHeader.CRN), "CRN");
				AssertColumn(columnStyles, nameof(TemporaryStorageHeader.MRN), "MRN");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_MessageType, "Message Type");
				AssertColumn(columnStyles, nameof(TemporaryStorageHeader.CustomsStatus), "Customs Status");
				AssertColumn(columnStyles, AsycudaManifestHeaderSchema.Constants.AMA_TransportMode, "Transportation Mode");
			}

			void AssertColumn(ArrayList columnStyles, string columnName, string columnCaption)
			{
				var message = $"{columnName} should be found in Grid.ColumnStyles";
				Assert(message, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == columnName && info.CaptionResourceString.Caption == columnCaption));
			}
		}
	}
}
