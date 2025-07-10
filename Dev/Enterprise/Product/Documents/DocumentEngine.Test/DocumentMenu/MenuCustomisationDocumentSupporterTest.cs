using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;
using _Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	public abstract class MenuCustomisationDocumentSupporterTest : DocumentSupporterTest
	{
		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestRunningDocumentsShouldNotCauseException()
		{
			base.TestRunningDocumentsShouldNotCauseException();
		}

		public void TestWrapperKnowsAboutParentDocumentSupporter()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(_Constants.DataContext.MapGenericFreightJob, null);
			AssertEquals("Precondition: GetDocumentWrappers().Length", 1, wrappers.Length);
			MenuCustomisationDocumentSupporter.IWantToKnowMyParentDocumentSupporter wrapper = wrappers[0] as MenuCustomisationDocumentSupporter.IWantToKnowMyParentDocumentSupporter;
			AssertNotNull("DocumentWrapper returned should not be null when cast to IWantToKnowMyParentDocumentSupporter", wrapper);
			AssertEquals("ParentDocumentWrapper should be set", DocumentSupporter, wrapper.ParentDocumentSupporter);
		}

		public void TestBusinessContext()
		{
			AssertEquals("DocumentSupporter.BusinessContext", SupportedBusinessContext, DocumentSupporter.BusinessContext);
		}

		public void TestIsDataContextSupported()
		{
			AssertEquals("IsDataContextSupported(DataContext.MapGenericFreightJob)", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(_Constants.DataContext.MapGenericFreightJob)));
		}

		public void TestGetDocumentWrappers()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(_Constants.DataContext.MapGenericFreightJob, null);
			AssertEquals("GetDocumentWrappers().Length", 1, wrappers.Length);
			DocumentWrapper wrapper = wrappers[0];
			AssertNotNull("DocumentWrapper returned should not be null", wrapper);
			AssertEquals("wrapper.GetType().Name", "SchemaWrapper", wrapper.GetType().Name);
		}

		#region DocumentSupporter
		protected abstract MenuCustomisationDocumentSupporter DocumentSupporter { get; }
		protected abstract BusinessContext SupportedBusinessContext { get; }
		#endregion
	}
}
