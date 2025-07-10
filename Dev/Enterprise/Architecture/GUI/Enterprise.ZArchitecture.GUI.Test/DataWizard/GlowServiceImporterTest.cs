using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.DataTransfer;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GlowServiceImporterTest : TestCase
	{
		public void TestStartImport_Response()
		{
			clientMock
				.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(response));

			var responseContent = JsonConvert.SerializeObject(new { Type = "Info", Message = "Committing to database", Progress = 90.0 });
			using (response.Content = new StringContent(responseContent))
			{
				importer.StartImportAsync(baseUri, mappingMock.Object, dataContent, log);

				AssertEquals(3, log.CountWithoutVerbose);

				AssertEquals("Starting file import process...", log.GetLogMessage(0));
				AssertEquals("Info", log.GetLogType(0));

				AssertEquals("Committing to database", log.GetLogMessage(1));
				AssertEquals("Info", log.GetLogType(1));

				AssertEquals("Data has been imported successfully.", log.GetLogMessage(2));
				AssertEquals("ImportFinished", log.GetLogType(2));
			}
		}

		public void TestStartImport_Response_WithErrorLog()
		{
			clientMock
				.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(response));

			var responseContent = JsonConvert.SerializeObject(new { Type = "Error", Message = "Value not provided in appropriate format." });
			using (response.Content = new StringContent(responseContent))
			{
				importer.StartImportAsync(baseUri, mappingMock.Object, dataContent, log);

				AssertEquals(2, log.CountWithoutVerbose);

				AssertEquals("Starting file import process...", log.GetLogMessage(0));
				AssertEquals("Info", log.GetLogType(0));

				AssertEquals("Value not provided in appropriate format.", log.GetLogMessage(1));
				AssertEquals("Error", log.GetLogType(1));
			}
		}

		public void TestStartImport_Response_WithInvalidLog()
		{
			clientMock
				.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(response));

			var responseContent = JsonConvert.SerializeObject(new { Description = "Some other text" });
			using (response.Content = new StringContent(responseContent))
			{
				importer.StartImportAsync(baseUri, mappingMock.Object, dataContent, log);

				AssertEquals(2, log.CountWithoutVerbose);

				AssertEquals("Starting file import process...", log.GetLogMessage(0));
				AssertEquals("Info", log.GetLogType(0));

				AssertEquals("Data has been imported successfully.", log.GetLogMessage(1));
				AssertEquals("ImportFinished", log.GetLogType(1));
			}
		}

		public void TestStartImport_ClientReturnsError400()
		{
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContentWithError, "Response from Glow service contains a non-successful status code 400 (Bad Request).");
		}

		public void TestStartImport_ClientReturnsError400_Response_WithInvalidJson()
		{
			response.StatusCode = HttpStatusCode.BadRequest;

			var responseContent = new StringContent("{ type = \"http://a.com/testtype\", detail = \"Some error message.\" }", Encoding.UTF8, "application/problem+json");
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Bad Request).");
		}

		public void TestStartImport_ClientReturnsError400_Response_WithInvalidMediaType()
		{
			response.StatusCode = HttpStatusCode.BadRequest;

			var responseContent = new StringContent(JsonConvert.SerializeObject(new { type = "http://a.com/testtype", detail = "Some error." }), Encoding.UTF8, "application/json");
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Bad Request).");
		}

		public void TestStartImport_ClientReturnsError400_BadData()
		{
			var problemDetails = "{\"type\":\"https://glow.wisetechglobal.com/data-import/bad-data\",\"status\":400,\"detail\":\"Oh no, bad data\",\"badData\":\"I am some bad data\"}";
			var responseContent = new StringContent(problemDetails, Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "Please fix the file and try again. File contains bad data: I am some bad data");
		}

		public void TestStartImport_ClientReturnsError403_WithNoUnauthorizedMappingPaths()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "Unauthorized mapping paths.",
				Type = ProblemType.UnauthorizedMappingPaths,
				Status = (int)HttpStatusCode.Forbidden
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.Forbidden;
			StartImport_ClientReturnsError(responseContent, "You do not have permission to perform this action. Contact your system administrator to be granted appropriate permissions.");
		}

		public void TestStartImport_ClientReturnsError403_WithUnauthorizedMappingPathsAndRelations()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "Unauthorized mapping paths.",
				Type = ProblemType.UnauthorizedMappingPaths,
				Status = (int)HttpStatusCode.Forbidden,
				Extensions = new Dictionary<string, object> { { "securityErrors", new[] {
					new MappingSecurityResult(false, string.Empty, new [] { "rel1" }),
					new MappingSecurityResult(false, "path1", new List<string>()),
					new MappingSecurityResult(true, "path2.path3", new [] { "rel2", "rel3" }), } } }
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.Forbidden;
			StartImport_ClientReturnsError(responseContent, @"Your permissions do not give importing rights to the following entities:
IDummyBizo
IDummyBizo/rel1
path1
path2.path3/rel2
path2.path3/rel3");
		}

		public void TestStartImport_ClientReturnsError400_FileFileHasInvalidExcelSheet()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "No Worksheet with name 'fooSheet' was found in file. Please make sure the file and mapping settings are correct.",
				Type = ProblemType.FileHasInvalidExcelSheet,
				Extensions = new Dictionary<string, object> { { "sheetName", "fooSheet" } },
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "No Worksheet with name 'fooSheet' was found in file. Please make sure the file and mapping settings are correct.");
		}

		public void TestStartImport_ClientReturnsError400_FileIsEncrypted()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "Excel file is encrypted.",
				Type = ProblemType.FileIsEncrypted,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "The selected Excel file is encrypted with a password and cannot be read. Please remove the password before importing.");
		}

		public void TestStartImport_ClientReturnsError400_FileIsInvalidExcel()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "some detail that shouldn't be used",
				Type = ProblemType.FileIsInvalidExcel,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "The selected Excel file is invalid or corrupted.");
		}

		public void TestStartImport_ClientReturnsError400_FileIsInvalidExcel_WithWrongContentType_FallsBackToReasonPhrase()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "some detail that shouldn't be used",
				Type = ProblemType.FileIsInvalidExcel,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/javascript");
			response.StatusCode = HttpStatusCode.BadRequest;
#pragma warning disable WTG2007 // ReasonPhrase for testing
			response.ReasonPhrase = "Some other reason";
#pragma warning restore WTG2007 // ReasonPhrase for testing
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Some other reason).");
		}

		public void TestStartImport_ClientReturnsError400_FileTooOld()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "Excel file version is too old.",
				Type = ProblemType.FileTooOld,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "The selected Excel file version is not supported. Please select an Excel file with version 97/2000/XP/2003 or superior.");
		}

		public void TestStartImport_ClientReturnsError400_FileTooOld_WithWrongContentType_FallsBackToReasonPhrase()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "I am an old file",
				Type = ProblemType.FileTooOld,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/javascript");
			response.StatusCode = HttpStatusCode.BadRequest;
#pragma warning disable WTG2007 // ReasonPhrase for testing
			response.ReasonPhrase = "Some other reason";
#pragma warning restore WTG2007 // ReasonPhrase for testing
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Some other reason).");
		}

		public void TestStartImport_ClientReturnsError400_StrictOOXMLExcelFileNotSupported()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "This detail should not be used",
				Type = ProblemType.StrictOOXMLExcelFileNotSupported,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "Your import data file is in an unsupported Excel format. Please save your file in 'Excel Workbook (*.xlsx)' format and then try again.");
		}

		public void TestStartImport_ClientReturnsError400_StrictOOXMLExcelFileNotSupported_WithWrongContentType_FallsBackToReasonPhrase()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "This detail should not be used",
				Type = ProblemType.StrictOOXMLExcelFileNotSupported,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/javascript");
			response.StatusCode = HttpStatusCode.BadRequest;
#pragma warning disable WTG2007 // ReasonPhrase for testing
			response.ReasonPhrase = "Some other reason";
#pragma warning restore WTG2007 // ReasonPhrase for testing
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Some other reason).");
		}

		public void TestStartImport_ClientReturnsError400_FileIsEmpty()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "The selected file contains no data.",
				Type = ProblemType.FileIsEmpty,
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "The selected file contains no data.");
		}

		public void TestStartImport_ClientReturnsError400_EmptyResponseContent_FallsBackToReasonPhrase()
		{
			var responseContent = new StringContent(string.Empty, Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Bad Request).");
		}

		public void TestStartImport_ClientReturnsError400_UnknownType_FallsBackToReasonPhrase()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "Unknown",
				Type = "SomeUnknownType",
			};
			var responseContent = new StringContent(problemDetails.Serialize(), Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
#pragma warning disable WTG2007 // ReasonPhrase for testing
			response.ReasonPhrase = "Some other reason";
#pragma warning restore WTG2007 // ReasonPhrase for testing
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Some other reason).");
		}

		public void TestStartImport_ClientReturnsError400_WithInvalidJson_FallsBackToReasonPhrase()
		{
			var responseContent = new StringContent("{\"type\": I am a bad json,\"detail\":\"hi\"}", Encoding.UTF8, "application/problem+json");
			response.StatusCode = HttpStatusCode.BadRequest;
			StartImport_ClientReturnsError(responseContent, "Response from Glow service contains a non-successful status code 400 (Bad Request).");
		}

		public void TestStartImport_ClientReturnsError413()
		{
			response.StatusCode = HttpStatusCode.RequestEntityTooLarge;
			StartImport_ClientReturnsError(responseContentWithError, "The file that you have chosen is too large and could cause performance issues if used in your application. Please choose a smaller file.");
		}

		public void TestStartImport_ClientReturnsError415()
		{
			response.StatusCode = HttpStatusCode.UnsupportedMediaType;
			StartImport_ClientReturnsError(responseContentWithError, "Response from Glow service contains a non-successful status code 415 (Unsupported Media Type).");
		}

		public void TestStartImport_ClientReturnsErrorOther400Error()
		{
			response.StatusCode = HttpStatusCode.NotFound;
			StartImport_ClientReturnsError(responseContentWithError, "Response from Glow service contains a non-successful status code 404 (Not Found).");
		}

		public void TestStartImport_ClientReturnsNonSuccessCode()
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			StartImport_ClientReturnsError(new StringContent(@"<blah/>"), "Response from Glow service contains a non-successful status code 500 (Internal Server Error).");
		}

		void StartImport_ClientReturnsError(StringContent responseContent, string expectedLastMessage)
		{
			clientMock
				.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(response));

			using (response.Content = responseContent)
			{
				importer.StartImportAsync(baseUri, mappingMock.Object, dataContent, log);

				AssertEquals(2, log.CountWithoutVerbose);
				AssertEquals("Starting file import process...", log.GetLogMessage(0));
				AssertEquals("Info", log.GetLogType(0));

				AssertEquals(expectedLastMessage, log.GetLogMessage(1));
				AssertEquals("Error", log.GetLogType(1));
			}
		}

		public void TestNullResponseThrowsException()
		{
			response = null;
			AssertExceptionThrown<ArgumentNullException>(() => importer.StartImportAsync(baseUri, mappingMock.Object, dataContent, log));
		}

		GlowServiceImporter importer;
		HttpResponseMessage response;
		StringContent responseContentWithError;
		Uri baseUri;
		Uri requestUri;
		Mock<IDataTransferMapping> mappingMock;
		MultipartContent dataContent;
		Mock<IGlowServiceClient> clientMock;
		Mock<IHubsClient> hubsClientMock;
		GlowLog log;

		protected override void SetUp()
		{
			base.SetUp();

			baseUri = new Uri("https://address/");
			requestUri = new Uri(baseUri.ToString() + $"api/datatransfer/import?entityName=IDummyBizo&key={Guid.Empty}&mappingPK={Guid.Empty}");
			mappingMock = new Mock<IDataTransferMapping>();
			mappingMock.Setup(m => m.Name).Returns("test mapping");
			mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
			mappingMock.Setup(m => m.PK).Returns(Guid.Empty);
			dataContent = new MultipartContent();

			log = new GlowLog();

			hubsClientMock = new Mock<IHubsClient>();
			hubsClientMock.Setup(h => h.StartAsync(baseUri, Guid.Empty, new Action<string, string, decimal>(log.AppendLog))).Returns(Task.FromResult<object>(null));

			response = new HttpResponseMessage();
			clientMock = new Mock<IGlowServiceClient>();
			responseContentWithError = new StringContent(JsonConvert.SerializeObject(new { type = "http://a.com/testtype", detail = "Some error message." }), Encoding.UTF8, "application/problem+json");

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(baseUri)).Returns(clientMock.Object);

			ObjectFactory.Substitute(hubsClientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			importer = new GlowServiceImporter();
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
			response?.Dispose();
		}
	}
}
