using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Integration;
using WTG.TestHelpers;
using SnailTestAttribute = NUnit.Framework.SnailTestAttribute;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Test
{
	public class RetrievePerformanceTest : TestCaseWithFactory
	{
		[SnailTest]
		public void TestRetrieveOrganization_2101Contacts_DoesNotThrowSqlExceptionOfTooManyParameters()
		{
			var actualImportLog = Import("Architecture/content/NativeDataTransfer/TestFiles/Organization_CARSHIUS_Import_2101Contacts.xml");
			var expectedImportLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes";
			AssertContains(expectedImportLog, actualImportLog);

			Export("Architecture/content/NativeDataTransfer/TestFiles/Organization_CARSHIUS_Retrieve.xml", out var responseStatus, out var actualExportLog);

			CombineAssertions(() =>
			{
				AssertNotContains("The incoming request has too many parameters.", actualExportLog);
				AssertContains("1 matches found", actualExportLog);
				AssertEquals(NativeResponseStatus.Accepted, responseStatus);
			}
			);
		}

		#region Implementation

		string Import(string fileName)
		{
			var xml = ReadFileContent(fileName);
			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			return GetLogs().Trim();
		}

		void Export(string fileName, out NativeResponseStatus responseStatus, out string log)
		{
			var xml = ReadFileContent(fileName);
			var root = XElement.Parse(xml);
			var entityElement = root.Elements().ToArray()[0].Elements().ToArray()[0];   //Skip <Native> and <Body>

			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { entityElement } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			responseStatus = response.Status;

			var informations = response.Informations;
			log = (informations.Any()) ? string.Join("\r\n", informations) : string.Empty;
		}

		string ReadFileContent(string fileName)
		{
			var reader = new StreamReader(AssetsHelper.FetchTestAsset(fileName));
			return reader.ReadToEnd();
		}

		string GetLogs()
		{
			return string.Join("\r\n", dummyLogger.Buffer.Logs().Select(log => log.Message).ToArray());
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			dummyLogger.Error("Test error: " + ex.Message);
		}

		void SetupData()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('7b9a1f02-ae57-4e12-902c-64ef364c191c', 'SSL', 'AU company1', 'AU', 'AUD')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('96b76135-2438-47c6-9e5e-308a47947f82', 'JAX', 'AU company2', 'AU', 'AUD')");
		}

		class DummyResponseFactory : IResponseFactory
		{
			public Response GetNewResponse()
			{
				return new Response_Universal();
			}

			public IXmlSerializer GetResponseSerializer()
			{
				return new ObjectXmlSerializer<Response_Universal>();
			}

			public XNamespace NameSpace
			{
				get { return ReferenceDataXMLForDeSerialize.NameSpace_Universal; }
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupImporter();
			SetupData();
		}

		void SetupImporter()
		{
			var session = new AncillaryImportServices();
			dummyLogger = session.Logger as MemoryLogger;
			importHandler = new ImportHandler(session);
			importHandler.ErrorOccur = ErrorOccur;
		}

		MemoryLogger dummyLogger;
		ImportHandler importHandler;

		#endregion
	}
}
