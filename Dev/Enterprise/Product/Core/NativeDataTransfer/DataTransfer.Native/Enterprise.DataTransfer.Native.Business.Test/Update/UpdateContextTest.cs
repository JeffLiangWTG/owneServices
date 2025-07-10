using System.Globalization;
using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateContextTest : TestCase
	{
		public class TestImportHandler : ImportHandler
		{
			public bool ErrorOccurWasCalled;
			public bool UnitSuccessWasCalled;
			public ExportImportRequest ExportImportRequest;
			public UpdateContext UpdateContent;

			public TestImportHandler(AncillaryImportServices sessionServices) : base(sessionServices)
			{
				UnitProcessSuccess = e => { UnitSuccessWasCalled = true; };
				ErrorOccur = (elem, ex) => { ErrorOccurWasCalled = true; };
			}

			// Hides the normal Import method to allow us to capture the UpdateContext and ExportImportRequest
			// variables for testing. This bypasses the real Import() method.
			public new void Import(Stream stream)
			{
				var requestDeserializer = RequestDeserializerBuilder.GetDeserializer(stream);
				ExportImportRequest = DeserializeRequest(requestDeserializer, stream);
				ValidateRequest(ExportImportRequest);

				UpdateContent = ConvertContext(ExportImportRequest.Settings);

				UpdateContent.AlwaysUseInternalPK = AlwaysUseProvidedPKs;

				ImportCore(ExportImportRequest, UpdateContent);
			}
		}

		public void TestUpdateContext_DefaultValue()
		{
			var context = new UpdateContext(new AncillaryImportServices(), new FactoryProvider());
			AssertNotNull(context.EntityInfo);
			AssertNotNull(context.InterceptorSettings);
		}

		public void TestFactoryProvider_RefreshDisabled()
		{
			var provider = new FactoryProvider();
			var factory = provider.GetNewFactory(Db.Connection);

			AssertEquals(false, factory.RefreshEnabled);
		}

		[UseSnapshotProtection]
		public void TestUpdateContext_Import_GoodFile()
		{
			UpdateContext_ImportHelper("Organization_CHIBEALAX.xml", true, false);
		}

		[UseSnapshotProtection]
		public void TestUpdateContext_Import_BadFile()
		{
			UpdateContext_ImportHelper("prolink_supplier_erroneous_data.xml", false, true);
		}

		public void TestGetOrCreateBehaviourRowFactorySavedFirst()
		{
			var context = new UpdateContext(new AncillaryImportServices(), new FactoryProvider());
			AssertNull(context.BehaviourRowFactorySavedFirst);
			var extra1 = context.GetOrCreateBehaviourRowFactorySavedFirst();
			var extra2 = context.GetOrCreateBehaviourRowFactorySavedFirst();
			AssertNotNull(extra1);
			AssertEquals(extra1, extra2);
			AssertNotEquals(context.RowFactory, extra1);
			AssertEquals(extra1, context.BehaviourRowFactorySavedFirst);
		}

		protected void UpdateContext_ImportHelper(string filename, bool expectedUnitSuccessCalled, bool expectedErrorOccurCalled)
		{
			var assemblyPath = string.Format(CultureInfo.InvariantCulture, "Enterprise.DataTransfer.Native.Business.TestFiles.{0}", filename);
			var xmlFileStream = GetType().Assembly.GetManifestResourceStream(assemblyPath);
			var ih = new TestImportHandler(new AncillaryImportServices());
			ih.Import(xmlFileStream);

			AssertEquals(expectedUnitSuccessCalled, ih.UnitSuccessWasCalled);
			AssertEquals(expectedErrorOccurCalled, ih.ErrorOccurWasCalled);
		}
	}
}
