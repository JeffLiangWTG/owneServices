using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Client.EDI.IssueManager.Business.StackLinesWeightsLogAutoAssigner;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests
{
	public class MachineLearningTeamAssignmentRetrieverTest : TestCaseWithFactory
	{
		public void TestIsEnabled()
		{
			var retriever = new MachineLearningTeamAssignmentRetriever();

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "   "))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, FakeApiUrl))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "   "))
			{
				Assert(!retriever.IsEnabled);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, FakeApiUrl))
			{
				Assert(retriever.IsEnabled);
			}
		}

		public void TestHttpMessageHandler()
		{
			var httpClientHandler = new MachineLearningTeamAssignmentRetriever().HttpMessageHandler as HttpClientHandler;

			AssertNotNull(httpClientHandler);
			Assert(httpClientHandler.PreAuthenticate);
			Assert(httpClientHandler.UseDefaultCredentials);
		}

		public void TestGetIssueAssignmentAsync_InvalidParameters()
		{
			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, FakeApiUrl))
			{
				var retriever = new MachineLearningTeamAssignmentRetriever();

				AssertNull(retriever.GetIssueAssignmentAsync(null, CreateAssignmentCandidates(), CreateLog()).Result);
				AssertNull(retriever.GetIssueAssignmentAsync(Array.Empty<StackLine>(), CreateAssignmentCandidates(), CreateLog()).Result);
				AssertNull(retriever.GetIssueAssignmentAsync(CreateStackLines(), null, CreateLog()).Result);
				AssertNull(retriever.GetIssueAssignmentAsync(CreateStackLines(), new List<AssignmentCandidate>(), CreateLog()).Result);
				AssertNull(retriever.GetIssueAssignmentAsync(CreateStackLines(), CreateAssignmentCandidates(), null).Result);
			}
		}

		public void TestGetIssueAssignmentAsync_Success()
		{
			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: null,
				expectedErrorReporterLastExceptionReportedType: null,
				expectedExceptionMessage: null,
				expectedAssigned: ExpectedAssignment,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiResponseCode: HttpStatusCode.OK,
				apiResponseJson: responseJson200Success);

			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: null,
				expectedErrorReporterLastExceptionReportedType: null,
				expectedExceptionMessage: null,
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiResponseCode: HttpStatusCode.OK,
				apiResponseJson: responseJson200Error);
		}

		public void TestGetIssueAssignmentAsync_NonSuccess_ServerEnd()
		{
			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: expectedErrorReporterLastMessageReported400Template,
				expectedErrorReporterLastExceptionReportedType: typeof(HttpRequestException),
				expectedExceptionMessage: "Response status code does not indicate success: 400 (Bad Request).",
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiResponseCode: HttpStatusCode.BadRequest,
				apiResponseJson: responseJson400);

			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: expectedErrorReporterLastMessageReported500Template,
				expectedErrorReporterLastExceptionReportedType: typeof(HttpRequestException),
				expectedExceptionMessage: "Response status code does not indicate success: 500 (Internal Server Error).",
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiResponseCode: HttpStatusCode.InternalServerError,
				apiResponseJson: responseJson500);

			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: expectedErrorReporterLastMessageReportedBadFormatTemplate,
				expectedErrorReporterLastExceptionReportedType: typeof(FormatException),
				expectedExceptionMessage: "Bad Json format of respond.",
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiResponseCode: HttpStatusCode.OK,
				apiResponseJson: responseJsonBadFormat);

			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: expectedErrorReporterLastMessageReportedInvalidAssignmentTemplate,
				expectedErrorReporterLastExceptionReportedType: null,
				expectedExceptionMessage: "Assignment does not exist in candidates.",
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiResponseCode: HttpStatusCode.OK,
				apiResponseJson: responseJsonInvalidAssignment);
		}

		public void TestGetIssueAssignmentAsync_NonSuccess_ClientEnd()
		{
			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: expectedErrorReporterLastMessageReportedTimeoutTemplate,
				expectedErrorReporterLastExceptionReportedType: typeof(TimeoutException),
				expectedExceptionMessage: "Request timeout error (5 seconds).",
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				exceptionToThrow: new TaskCanceledException());

			AssertGetIssueAssignmentAsync(
				expectedApiRequestJson: expectedRequestJsonTemplate,
				expectedErrorReporterLastMessageReported: expectedErrorReporterLastMessageReportedBadUrlTemplate,
				expectedErrorReporterLastExceptionReportedType: typeof(InvalidOperationException),
				expectedExceptionMessage: "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.",
				expectedAssigned: null,
				stackLines: CreateStackLines(),
				assignmentCandidates: CreateAssignmentCandidates(),
				log: CreateLog(),
				apiUrl: "This is a valid URI - IN YOUR DREAM!");
		}

		#region Implementation

		void AssertGetIssueAssignmentAsync(string expectedApiRequestJson, string expectedErrorReporterLastMessageReported, string expectedExceptionMessage, Type expectedErrorReporterLastExceptionReportedType, IssueAssignment expectedAssigned, StackLine[] stackLines, IEnumerable<AssignmentCandidate> assignmentCandidates, EdiHelpErrorLog log, HttpStatusCode? apiResponseCode = null, string apiResponseJson = null, string apiUrl = FakeApiUrl, Exception exceptionToThrow = null)
		{
			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apiUrl))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ApiTimeout))
			{
				var assignment = new MachineLearningTeamAssignmentRetriever
				{
					HttpMessageHandler = MachineLearningTeamAssignmentTestHelper.GetHttpMessageHandler(apiResponseCode, apiResponseJson, exceptionToThrow, assertSendAsyncAction: new Action<HttpRequestMessage>(httpRequestMessage =>
					{
						var requestJson = httpRequestMessage.Content.ReadAsStringAsync().Result;
						AssertEquals(apiUrl, httpRequestMessage.RequestUri.ToString());
						AssertEquals(GetExpectedRequestJson(expectedApiRequestJson, log.PK), requestJson);
					})),
				}.GetIssueAssignmentAsync(stackLines, assignmentCandidates, log).Result;

				AssertEquals(expectedAssigned, assignment);

				if (expectedAssigned != null)
				{
					AssertEquals(expectedAssigned.IsRetrievedFromAPI, assignment.IsRetrievedFromAPI);
				}

				if (!string.IsNullOrEmpty(expectedErrorReporterLastMessageReported))
				{
					AssertContains(GetExpectedErrorReporterLastMessage(expectedErrorReporterLastMessageReported, log.PK), ErrorReporter.LastMessageReported);
					var expectedErrorReportKey = $"MachineLearningTeamAssignment|API|{expectedExceptionMessage}";
					AssertEquals(ErrorReporter.LastKeyReported, expectedErrorReportKey);
				}
				else
				{
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				}

				if (expectedErrorReporterLastExceptionReportedType != null)
				{
					AssertType(expectedErrorReporterLastExceptionReportedType, ErrorReporter.LastExceptionReported);
				}
				else
				{
					AssertNull(ErrorReporter.LastExceptionReported);
				}
			}

			ErrorReporter.Clear();
		}

		const int ApiTimeout = 5;
		const string FakeApiUrl = "https://localhost/api/v1/assignment";

		IssueAssignment ExpectedAssignment => new IssueAssignment("PD1", "PA1", "MD1", isRetrievedFromApi: true);

		static string GetExpectedRequestJson(string template, ZGuid issuePk) => string.Format(template, issuePk.ToString());

		const string expectedRequestJsonTemplate = @"{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}";

		const string responseJson200Success = "{\"status\":\"success\",\"assignment\":{\"product\":\"PD1\",\"productArea\":\"PA1\",\"module\":\"MD1\"}}";
		const string responseJson200Error = "{\"status\":\"error\",\"message\":\"Not found.\"}";

		const string responseJson400 = "{\"status\":\"error\",\"message\":\"So your request is so bad.\"}";
		const string expectedErrorReporterLastMessageReported400Template = @"Response status code does not indicate success: 400 (Bad Request).
API URL: https://localhost/api/v1/assignment
Request JSON:
{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}
Response JSON:
{{""status"":""error"",""message"":""So your request is so bad.""}}
";

		const string responseJson500 = "{\"status\":\"error\",\"message\":\"The server is on CR1!\"}";
		const string expectedErrorReporterLastMessageReported500Template = @"Response status code does not indicate success: 500 (Internal Server Error).
API URL: https://localhost/api/v1/assignment
Request JSON:
{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}
Response JSON:
{{""status"":""error"",""message"":""The server is on CR1!""}}
";

		const string responseJsonBadFormat = "{\"status\":\"success\",\"assignment\":\"This issue should be assigned to Satan.\"}";
		const string expectedErrorReporterLastMessageReportedBadFormatTemplate = @"Bad Json format of respond.
API URL: https://localhost/api/v1/assignment
Request JSON:
{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}
Response JSON:
{{""status"":""success"",""assignment"":""This issue should be assigned to Satan.""}}
";

		const string responseJsonInvalidAssignment = "{\"status\":\"success\",\"assignment\":{\"product\":\"XXX\",\"productArea\":\"YYY\",\"module\":\"ZZZ\"}}";
		const string expectedErrorReporterLastMessageReportedInvalidAssignmentTemplate = @"The assignment retrieved from the API does not exist in the candidates.
API URL: https://localhost/api/v1/assignment
Request JSON:
{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}
Response JSON:
{{""status"":""success"",""assignment"":{{""product"":""XXX"",""productArea"":""YYY"",""module"":""ZZZ""}}}}
";

		const string expectedErrorReporterLastMessageReportedTimeoutTemplate = @"Request timeout error (5 seconds).
API URL: https://localhost/api/v1/assignment
Request JSON:
{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}
Response JSON:
(empty)
";

		const string expectedErrorReporterLastMessageReportedBadUrlTemplate = @"An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.
API URL: This is a valid URI - IN YOUR DREAM!
Request JSON:
{{
  ""stackLines"": [
    {{
      ""assembly"": ""Assembly1.dll"",
      ""type"": ""Type1"",
      ""method"": ""Method1()"",
      ""parameters"": ""param a1, param b1"",
      ""fullStackLine"": ""StackLine1\r\nXXXXX\r\nXXXXX""
    }},
    {{
      ""assembly"": ""Assembly2.dll"",
      ""type"": ""Type2"",
      ""method"": ""Method2()"",
      ""parameters"": ""param a2, param b2"",
      ""fullStackLine"": ""StackLine2\r\nXXXXX\r\nXXXXX""
    }}
  ],
  ""assignmentCandidates"": [
    {{
      ""weight"": 0.9,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD1"",
        ""productArea"": ""PA1"",
        ""module"": ""MD1""
      }}
    }},
    {{
      ""weight"": 0.8,
      ""assembly"": ""Assembly1.dll"",
      ""assignment"": {{
        ""product"": ""PD2"",
        ""productArea"": ""PA2"",
        ""module"": ""MD2""
      }}
    }},
    {{
      ""weight"": 0.7,
      ""assembly"": ""Assembly2.dll"",
      ""assignment"": {{
        ""product"": ""PD3"",
        ""productArea"": ""PA3"",
        ""module"": ""MD3""
      }}
    }}
  ],
  ""issuePk"": ""{0}"",
  ""issueNumber"": ""00000001"",
  ""exceptionType"": ""System.OutOfMemoryException"",
  ""exceptionMessage"": ""Very serious exception."",
  ""exceptionSource"": ""CargoWiseOne.exe"",
  ""firstReported"": ""2024-02-01T11:24:03.110Z"",
  ""lastReported"": ""2024-02-03T15:44:11.596Z""
}}
Response JSON:
(empty)
";

		static string GetExpectedErrorReporterLastMessage(string template, ZGuid issuePk) => string.Format(template, issuePk);

		StackLine[] CreateStackLines()
		{
			return new StackLine[]
			{
				new StackLine("Assembly1.dll", "Type1", "Method1()", "param a1, param b1", @"StackLine1
XXXXX
XXXXX"),
				new StackLine("Assembly2.dll", "Type2", "Method2()", "param a2, param b2", @"StackLine2
XXXXX
XXXXX"),
			};
		}

		IEnumerable<AssignmentCandidate> CreateAssignmentCandidates()
		{
			return new List<AssignmentCandidate>
			{
				new AssignmentCandidate
				{
					Weight = 0.9,
					Assembly = "Assembly1.dll",
					Assignment = new IssueAssignment("PD1", "PA1", "MD1"),
				},
				new AssignmentCandidate
				{
					Weight = 0.8,
					Assembly = "Assembly1.dll",
					Assignment = new IssueAssignment("PD2", "PA2", "MD2"),
				},
				new AssignmentCandidate
				{
					Weight = 0.7,
					Assembly = "Assembly2.dll",
					Assignment = new IssueAssignment("PD3", "PA3", "MD3"),
				},
			};
		}

		EdiHelpErrorLog CreateLog()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			log.HE_LogType = "EXC";
			log.HE_IssueNumber = "00000001";
			log.HE_ExceptionType = typeof(OutOfMemoryException).ToString();
			log.HE_ExceptionSource = "CargoWiseOne.exe";
			log.HE_ExceptionMessage = "Very serious exception.";
			log.HE_FirstReported = new ZDateTime(2024, 2, 1, 11, 24, 3, 110);
			log.HE_FirstProcessed = new ZDateTime(2024, 2, 2, 3, 9, 43, 58);
			log.HE_LastReported = new ZDateTime(2024, 2, 3, 15, 44, 11, 596);

			return log;
		}

		#endregion
	}
}
