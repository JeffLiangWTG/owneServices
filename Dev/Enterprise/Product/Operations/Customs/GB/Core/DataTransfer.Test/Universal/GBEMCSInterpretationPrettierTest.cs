using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class GBEMCSInterpretationPrettierTest : TestCaseWithFactory
	{
		public void TestCreatePrettyMessageInterpretationFromXmlRejectionMessage()
		{
			var xmlEvent = new XmlEventDeserializer().Parse(XmlFailureResponseEvent);
			AssertNotNull("Pre-Req: Universal Event", xmlEvent);
			var response = GBEMCSEventParentFinder.GetResponseTextXml(XmlFailureResponseTextDecoded, xmlEvent);
			AssertNotNull("Pre-Req: EMCSResponse", response);
			var pretty = prettier.CreatePrettyRejectionInterpretationFromXml(response);
			AssertEquals("Xml interpretation", ExpectedPrettyHTMLInterpretationXml, pretty);
		}

		public void TestCreatePrettyMessageInterpretationFromJsonRejectionMessage()
		{
			var response = GBEMCSEventParentFinder.GetJsonResponse(JsonFailureResponseTextDecoded);
			AssertNotNull("Pre-Req: EMCSResponse", response);
			var pretty = prettier.CreatePrettyRejectionInterpretationFromJson(response);
			AssertEquals("Json interpretation", ExpectedPrettyHTMLInterpretationJson, pretty);

			response = GBEMCSEventParentFinder.GetJsonResponse(JsonFailureResponseTextDecodedV2);
			AssertNotNull("Pre-Req: EMCSResponse V2", response);
			pretty = prettier.CreatePrettyRejectionInterpretationFromJson(response);
			AssertEquals("Json interpretation V2", ExpectedPrettyHTMLInterpretationJsonV2, pretty);
		}

		public void TestCreatePrettyPVTResponseInterpretationFromJson_Valid()
		{
			var pretty = CreatePrettyPVTResponseInterpretationFromJson(isTraderValid: true, isProductValid: true);
			var expected = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
				"<h3>Response to Pre-Validate Trader Query</h3><h4>Query:</h4>" +
				"<ul><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td>Trader ID</td><td>123456789</td></tr><tr><td>Type</td><td>UK Record</td></tr>" +
				"<tr><td>Excise codes</td><td>2203000100,2203001000</td></tr></table></ul><h4>Response:</h4>" +
				"<ul>Trader is <span style=\"color:green\">valid</span> for these products</ul>";

			AssertEquals(expected, pretty);
		}

		public void TestCreatePrettyPVTResponseInterpretationFromJson_Invalid()
		{
			var pretty = CreatePrettyPVTResponseInterpretationFromJson(isTraderValid: true, isProductValid: false);
			var expected = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
				"<h3>Response to Pre-Validate Trader Query</h3><h4>Query:</h4>" +
				"<ul><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td>Trader ID</td><td>123456789</td></tr><tr><td>Type</td><td>UK Record</td></tr>" +
				"<tr><td>Excise codes</td><td>2203000100,2203001000</td></tr></table></ul><h4>Response:</h4>" +
				"<ul>Trader is <span style=\"color:red\">invalid</span> for these products</ul>";

			AssertEquals(expected, pretty);

			pretty = CreatePrettyPVTResponseInterpretationFromJson(isTraderValid: false, isProductValid: false);
			AssertEquals(expected, pretty);
		}

		string CreatePrettyPVTResponseInterpretationFromJson(bool isTraderValid, bool isProductValid)
		{
			var request = @"{
  ""exciseTraderValidationRequest"": {
    ""exciseTraderRequest"": {
      ""exciseRegistrationNumber"": ""123456789"",
      ""entityGroup"": ""UK Record"",
      ""validateProductAuthorisationRequest"": [{
          ""product"": {
            ""exciseProductCode"": ""2203000100""
          }
        }, {
          ""product"": {
            ""exciseProductCode"": ""2203001000""
          }
        }
      ]
    }
  }
}";
			string response;
			if (isTraderValid && isProductValid)
			{
				response = @"{
  ""exciseTraderValidationResponse"": {
    ""validationTimestamp"": ""2024-05-31T12:34:56+01:00"",
    ""exciseTraderResponse"": [{
        ""exciseRegistrationNumber"": ""123456789"",
        ""entityGroup"": ""UK Record"",
        ""validTrader"": true,
        ""traderType"": ""1"",
        ""validateProductAuthorisationResponse"": {
          ""valid"": true
        }
      }
    ]
  }
}";
			}
			else if (!isTraderValid)
			{
				response = @"{
  ""validationTimestamp"": ""2024-05-31T12:34:56+01:00"",
  ""exciseTraderResponse"": [{
      ""exciseRegistrationNumber"": ""123456789"",
      ""entityGroup"": ""UK Record"",
      ""validTrader"": false,
      ""errorCode"": ""6"",
      ""errorText"": ""Not Found""
    }
  ]
}";
			}
			else
			{
				response = @"{
  ""validationTimeStamp"": ""2024-05-31T12:34:56+01:00"",
  ""exciseTraderResponse"": [{
      ""exciseRegistrationNumber"": ""123456789"",
      ""entityGroup"": ""UK Record"",
      ""validTrader"": true,
      ""traderType"": ""1"",
      ""validateProductAuthorisationResponse"": {
        ""valid"": false,
        ""productError"": [{
            ""exciseProductCode"": ""2203000100"",
            ""errorCode"": ""1"",
            ""errorText"": ""Unrecognised EPC""
          }, {
            ""exciseProductCode"": ""2203001000"",
            ""errorCode"": ""2"",
            ""errorText"": ""Unauthorised EPC""
          }
        ]
      }
    }
  ]
}";
			}
			return prettier.CreatePrettyPVTResponseInterpretationFromJson(request, response);
		}

		protected override void SetUp()
		{
			base.SetUp();
			prettier = new GBEMCSInterpretationPrettier();
		}

		GBEMCSInterpretationPrettier prettier;
		static readonly string XmlFailureResponseEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>EMCSJobDeclaration</Type>
              <Key>E00000944</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2023-05-22T12:59:28+10:00</EventTime>
        <EventType>MRJ</EventType>
        <DataContext>
          <DataSource>
            <DataProvider>EMCS</DataProvider>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>1DEDA5DE-495A-4874-A824-B118F03C2C44</Value>
          </Context>
          <Context>
            <Type>Error</Type>
            <Value>schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema</Value>
          </Context>
          <Context>
            <Type>ErrorSummary</Type>
            <Value>Message not accepted</Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>PHNvYXBlbnY6RW52ZWxvcGUgeG1sbnM6c29hcGVudj0iaHR0cDovL3d3dy53My5vcmcvMjAwMy8wNS9zb2FwLWVudmVsb3BlIj4NCiAgPHNvYXBlbnY6Qm9keT4NCiAgICA8SE1SQ1NPQVBSZXNwb25zZSB4bWxucz0iaHR0cDovL3d3dy5pbmxhbmRyZXZlbnVlLmdvdi51ay9TT0FQL1Jlc3BvbnNlLzIiPg0KICAgICAgPEVycm9yUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly93d3cuZ292dGFsay5nb3YudWsvQ00vZXJyb3JyZXNwb25zZSIgU2NoZW1hVmVyc2lvbj0iMi4wIj4NCiAgICAgICAgPEFwcGxpY2F0aW9uPg0KICAgICAgICAgIDxNZXNzYWdlQ291bnQ+MTU8L01lc3NhZ2VDb3VudD4gDQogICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwNTI8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5FbGVtZW50ICdxMTpTZWFsSW5mb3JtYXRpb24nIG11c3Qgb25seSBoYXZlIHZhbGlkIHRleHQgYXMgaXRzIGNvbnRlbnQ8L1RleHQ+IA0KICAgICAgICAgIDxMb2NhdGlvbj4vdG5zOkVudmVsb3BlWzFdL3RuczpCb2R5WzFdL2llOklFODE1WzFdL2llOkJvZHlbMV0vaWU6U3VibWl0dGVkRHJhZnRPZkVBREVTQURbMV0vaWU6Qm9keUVhZEVzYWRbMV0vaWU6UGFja2FnZVsxXS9pZTpTZWFsSW5mb3JtYXRpb25bMV08L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy1jb21wbGV4LXR5cGUuMi4yOiBFbGVtZW50ICdxMTpTZWFsSW5mb3JtYXRpb24nIG11c3QgaGF2ZSBubyBlbGVtZW50IFtjaGlsZHJlbl0sIGFuZCB0aGUgdmFsdWUgbXVzdCBiZSB2YWxpZC48L0RldmVsb3Blck1lc3NhZ2U+IA0KICAgICAgICAgICAgPC9NZXNzYWdlcz4NCiAgICAgICAgICA8L0FwcGxpY2F0aW9uPg0KICAgICAgICA8L0Vycm9yPg0KICAgICAgICA8RXJyb3I+DQogICAgICAgICAgPFJhaXNlZEJ5PkNoUklTPC9SYWlzZWRCeT4gDQogICAgICAgICAgPE51bWJlcj40MDAxPC9OdW1iZXI+IA0KICAgICAgICAgIDxUeXBlPnNjaGVtYTwvVHlwZT4gDQogICAgICAgICAgPFRleHQ+TWlzc2luZyBhdHRyaWJ1dGUgb24gZWxlbWVudCAncTE6U2VhbEluZm9ybWF0aW9uJzwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkJvZHlbMV0vaWU6SUU4MTVbMV0vaWU6Qm9keVsxXS9pZTpTdWJtaXR0ZWREcmFmdE9mRUFERVNBRFsxXS9pZTpCb2R5RWFkRXNhZFsxXS9pZTpQYWNrYWdlWzFdL2llOlNlYWxJbmZvcm1hdGlvblsxXS9AbGFuZ3VhZ2U8L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy1jb21wbGV4LXR5cGUuNDogQXR0cmlidXRlICdsYW5ndWFnZScgbXVzdCBhcHBlYXIgb24gZWxlbWVudCAncTE6U2VhbEluZm9ybWF0aW9uJy48L0RldmVsb3Blck1lc3NhZ2U+IA0KICAgICAgICAgICAgPC9NZXNzYWdlcz4NCiAgICAgICAgICA8L0FwcGxpY2F0aW9uPg0KICAgICAgICA8L0Vycm9yPg0KICAgICAgICA8RXJyb3I+DQogICAgICAgICAgPFJhaXNlZEJ5PkNoUklTPC9SYWlzZWRCeT4gDQogICAgICAgICAgPE51bWJlcj40MDg1PC9OdW1iZXI+IA0KICAgICAgICAgIDxUeXBlPnNjaGVtYTwvVHlwZT4gDQogICAgICAgICAgPFRleHQ+VmFsdWUgJycgZG9lc24ndCBoYXZlIHRoZSBjb3JyZWN0IGZvcm1hdDwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkJvZHlbMV0vaWU6SUU4MTVbMV0vaWU6Qm9keVsxXS9pZTpTdWJtaXR0ZWREcmFmdE9mRUFERVNBRFsxXS9pZTpCb2R5RWFkRXNhZFsxXS9pZTpQYWNrYWdlWzFdL2llOlNlYWxJbmZvcm1hdGlvblsxXTwvTG9jYXRpb24+IA0KICAgICAgICAgIDxBcHBsaWNhdGlvbj4NCiAgICAgICAgICAgIDxNZXNzYWdlcz4NCiAgICAgICAgICAgICAgPERldmVsb3Blck1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcnIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnLnsxLDM1MH0nIGZvciB0eXBlICdTZWFsSW5mb3JtYXRpb25UeXBlJy48L0RldmVsb3Blck1lc3NhZ2U+IA0KICAgICAgICAgICAgPC9NZXNzYWdlcz4NCiAgICAgICAgICA8L0FwcGxpY2F0aW9uPg0KICAgICAgICA8L0Vycm9yPg0KICAgICAgICA8RXJyb3I+DQogICAgICAgICAgPFJhaXNlZEJ5PkNoUklTPC9SYWlzZWRCeT4gDQogICAgICAgICAgPE51bWJlcj40MDg1PC9OdW1iZXI+IA0KICAgICAgICAgIDxUeXBlPnNjaGVtYTwvVHlwZT4gDQogICAgICAgICAgPFRleHQ+VmFsdWUgJycgZG9lc24ndCBoYXZlIHRoZSBjb3JyZWN0IGZvcm1hdDwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkJvZHlbMV0vaWU6SUU4MTVbMV0vaWU6Qm9keVsxXS9pZTpTdWJtaXR0ZWREcmFmdE9mRUFERVNBRFsxXS9pZTpCb2R5RWFkRXNhZFsxXS9pZTpQYWNrYWdlWzFdL2llOkNvbW1lcmNpYWxTZWFsSWRlbnRpZmljYXRpb25bMV08L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy1wYXR0ZXJuLXZhbGlkOiBWYWx1ZSAnJyBpcyBub3QgZmFjZXQtdmFsaWQgd2l0aCByZXNwZWN0IHRvIHBhdHRlcm4gJy57MSwzNX0nIGZvciB0eXBlICdDb21tZXJjaWFsU2VhbElkZW50aWZpY2F0aW9uVHlwZScuPC9EZXZlbG9wZXJNZXNzYWdlPiANCiAgICAgICAgICAgIDwvTWVzc2FnZXM+DQogICAgICAgICAgPC9BcHBsaWNhdGlvbj4NCiAgICAgICAgPC9FcnJvcj4NCiAgICAgICAgPEVycm9yPg0KICAgICAgICAgIDxSYWlzZWRCeT5DaFJJUzwvUmFpc2VkQnk+IA0KICAgICAgICAgIDxOdW1iZXI+NDA2NTwvTnVtYmVyPiANCiAgICAgICAgICA8VHlwZT5zY2hlbWE8L1R5cGU+IA0KICAgICAgICAgIDxUZXh0PkludmFsaWQgY29udGVudCBmb3VuZCBhdCBlbGVtZW50ICdxMTpDb21tZXJjaWFsU2VhbElkZW50aWZpY2F0aW9uJzwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkJvZHlbMV0vaWU6SUU4MTVbMV0vaWU6Qm9keVsxXS9pZTpTdWJtaXR0ZWREcmFmdE9mRUFERVNBRFsxXS9pZTpCb2R5RWFkRXNhZFsxXS9pZTpQYWNrYWdlWzFdL2llOkNvbW1lcmNpYWxTZWFsSWRlbnRpZmljYXRpb25bMV08L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy10eXBlLjMuMS4zOiBUaGUgdmFsdWUgJycgb2YgZWxlbWVudCAncTE6Q29tbWVyY2lhbFNlYWxJZGVudGlmaWNhdGlvbicgaXMgbm90IHZhbGlkLjwvRGV2ZWxvcGVyTWVzc2FnZT4gDQogICAgICAgICAgICA8L01lc3NhZ2VzPg0KICAgICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDwvRXJyb3I+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwNjU8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5JbnZhbGlkIGNvbnRlbnQgZm91bmQgYXQgZWxlbWVudCAncTE6UXVhbnRpdHknPC9UZXh0PiANCiAgICAgICAgICA8TG9jYXRpb24+L3RuczpFbnZlbG9wZVsxXS90bnM6Qm9keVsxXS9pZTpJRTgxNVsxXS9pZTpCb2R5WzFdL2llOlN1Ym1pdHRlZERyYWZ0T2ZFQURFU0FEWzFdL2llOkJvZHlFYWRFc2FkWzFdL2llOlF1YW50aXR5WzFdPC9Mb2NhdGlvbj4gDQogICAgICAgICAgPEFwcGxpY2F0aW9uPg0KICAgICAgICAgICAgPE1lc3NhZ2VzPg0KICAgICAgICAgICAgICA8RGV2ZWxvcGVyTWVzc2FnZT5jdmMtdHlwZS4zLjEuMzogVGhlIHZhbHVlICcwJyBvZiBlbGVtZW50ICdxMTpRdWFudGl0eScgaXMgbm90IHZhbGlkLjwvRGV2ZWxvcGVyTWVzc2FnZT4gDQogICAgICAgICAgICA8L01lc3NhZ2VzPg0KICAgICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDwvRXJyb3I+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwODU8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5WYWx1ZSAnMCcgZG9lc24ndCBoYXZlIHRoZSBjb3JyZWN0IGZvcm1hdDwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkJvZHlbMV0vaWU6SUU4MTVbMV0vaWU6Qm9keVsxXS9pZTpTdWJtaXR0ZWREcmFmdE9mRUFERVNBRFsxXS9pZTpCb2R5RWFkRXNhZFsxXS9pZTpRdWFudGl0eVsxXTwvTG9jYXRpb24+IA0KICAgICAgICAgIDxBcHBsaWNhdGlvbj4NCiAgICAgICAgICAgIDxNZXNzYWdlcz4NCiAgICAgICAgICAgICAgPERldmVsb3Blck1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcwJyBpcyBub3QgZmFjZXQtdmFsaWQgd2l0aCByZXNwZWN0IHRvIHBhdHRlcm4gJ1sxLTldXGR7MCwxNH18KFsxLTldXGR7MCwxM318MClcLlswLTldfChbMS05XVxkezAsMTJ9fDApXC5cZFswLTldfChbMS05XVxkezAsMTF9fDApXC5cZFxkWzAtOV0nIGZvciB0eXBlICdRdWFudGl0eVR5cGUnLjwvRGV2ZWxvcGVyTWVzc2FnZT4gDQogICAgICAgICAgICA8L01lc3NhZ2VzPg0KICAgICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDwvRXJyb3I+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwNjU8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5JbnZhbGlkIGNvbnRlbnQgZm91bmQgYXQgZWxlbWVudCAncTE6Rmlyc3RUcmFuc3BvcnRlclRyYWRlcic8L1RleHQ+IA0KICAgICAgICAgIDxMb2NhdGlvbj4vdG5zOkVudmVsb3BlWzFdL3RuczpCb2R5WzFdL2llOklFODE1WzFdL2llOkJvZHlbMV0vaWU6U3VibWl0dGVkRHJhZnRPZkVBREVTQURbMV0vaWU6Rmlyc3RUcmFuc3BvcnRlclRyYWRlclsxXTwvTG9jYXRpb24+IA0KICAgICAgICAgIDxBcHBsaWNhdGlvbj4NCiAgICAgICAgICAgIDxNZXNzYWdlcz4NCiAgICAgICAgICAgICAgPERldmVsb3Blck1lc3NhZ2U+Y3ZjLWNvbXBsZXgtdHlwZS4yLjQuYTogSW52YWxpZCBjb250ZW50IHdhcyBmb3VuZCBzdGFydGluZyB3aXRoIGVsZW1lbnQgJ3ExOkZpcnN0VHJhbnNwb3J0ZXJUcmFkZXInLiBPbmUgb2YgJ3sidXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpJRTgxNTpWMy4wMSI6RGVsaXZlcnlQbGFjZUN1c3RvbXNPZmZpY2UsICJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODE1OlYzLjAxIjpDb21wZXRlbnRBdXRob3JpdHlEaXNwYXRjaE9mZmljZX0nIGlzIGV4cGVjdGVkLjwvRGV2ZWxvcGVyTWVzc2FnZT4gDQogICAgICAgICAgICA8L01lc3NhZ2VzPg0KICAgICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDwvRXJyb3I+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwNjU8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5JbnZhbGlkIGNvbnRlbnQgZm91bmQgYXQgZWxlbWVudCAncTE6UmVmZXJlbmNlT2ZUYXhXYXJlaG91c2UnPC9UZXh0PiANCiAgICAgICAgICA8TG9jYXRpb24+L3RuczpFbnZlbG9wZVsxXS90bnM6Qm9keVsxXS9pZTpJRTgxNVsxXS9pZTpCb2R5WzFdL2llOlN1Ym1pdHRlZERyYWZ0T2ZFQURFU0FEWzFdL2llOlBsYWNlT2ZEaXNwYXRjaFRyYWRlclsxXS9pZTpSZWZlcmVuY2VPZlRheFdhcmVob3VzZVsxXTwvTG9jYXRpb24+IA0KICAgICAgICAgIDxBcHBsaWNhdGlvbj4NCiAgICAgICAgICAgIDxNZXNzYWdlcz4NCiAgICAgICAgICAgICAgPERldmVsb3Blck1lc3NhZ2U+Y3ZjLXR5cGUuMy4xLjM6IFRoZSB2YWx1ZSAnNjknIG9mIGVsZW1lbnQgJ3ExOlJlZmVyZW5jZU9mVGF4V2FyZWhvdXNlJyBpcyBub3QgdmFsaWQuPC9EZXZlbG9wZXJNZXNzYWdlPiANCiAgICAgICAgICAgIDwvTWVzc2FnZXM+DQogICAgICAgICAgPC9BcHBsaWNhdGlvbj4NCiAgICAgICAgPC9FcnJvcj4NCiAgICAgICAgPEVycm9yPg0KICAgICAgICAgIDxSYWlzZWRCeT5DaFJJUzwvUmFpc2VkQnk+IA0KICAgICAgICAgIDxOdW1iZXI+NDA4NTwvTnVtYmVyPiANCiAgICAgICAgICA8VHlwZT5zY2hlbWE8L1R5cGU+IA0KICAgICAgICAgIDxUZXh0PlZhbHVlICc2OScgZG9lc24ndCBoYXZlIHRoZSBjb3JyZWN0IGZvcm1hdDwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkJvZHlbMV0vaWU6SUU4MTVbMV0vaWU6Qm9keVsxXS9pZTpTdWJtaXR0ZWREcmFmdE9mRUFERVNBRFsxXS9pZTpQbGFjZU9mRGlzcGF0Y2hUcmFkZXJbMV0vaWU6UmVmZXJlbmNlT2ZUYXhXYXJlaG91c2VbMV08L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy1wYXR0ZXJuLXZhbGlkOiBWYWx1ZSAnNjknIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnW0EtWl17Mn1bYS16QS1aMC05XXsxMX0nIGZvciB0eXBlICdFeGNpc2VOdW1iZXJUeXBlJy48L0RldmVsb3Blck1lc3NhZ2U+IA0KICAgICAgICAgICAgPC9NZXNzYWdlcz4NCiAgICAgICAgICA8L0FwcGxpY2F0aW9uPg0KICAgICAgICA8L0Vycm9yPg0KICAgICAgICA8RXJyb3I+DQogICAgICAgICAgPFJhaXNlZEJ5PkNoUklTPC9SYWlzZWRCeT4gDQogICAgICAgICAgPE51bWJlcj40MDY1PC9OdW1iZXI+IA0KICAgICAgICAgIDxUeXBlPnNjaGVtYTwvVHlwZT4gDQogICAgICAgICAgPFRleHQ+SW52YWxpZCBjb250ZW50IGZvdW5kIGF0IGVsZW1lbnQgJ3ExOlRyYWRlckV4Y2lzZU51bWJlcic8L1RleHQ+IA0KICAgICAgICAgIDxMb2NhdGlvbj4vdG5zOkVudmVsb3BlWzFdL3RuczpCb2R5WzFdL2llOklFODE1WzFdL2llOkJvZHlbMV0vaWU6U3VibWl0dGVkRHJhZnRPZkVBREVTQURbMV0vaWU6Q29uc2lnbm9yVHJhZGVyWzFdL2llOlRyYWRlckV4Y2lzZU51bWJlclsxXTwvTG9jYXRpb24+IA0KICAgICAgICAgIDxBcHBsaWNhdGlvbj4NCiAgICAgICAgICAgIDxNZXNzYWdlcz4NCiAgICAgICAgICAgICAgPERldmVsb3Blck1lc3NhZ2U+Y3ZjLXR5cGUuMy4xLjM6IFRoZSB2YWx1ZSAnMTIzNCcgb2YgZWxlbWVudCAncTE6VHJhZGVyRXhjaXNlTnVtYmVyJyBpcyBub3QgdmFsaWQuPC9EZXZlbG9wZXJNZXNzYWdlPiANCiAgICAgICAgICAgIDwvTWVzc2FnZXM+DQogICAgICAgICAgPC9BcHBsaWNhdGlvbj4NCiAgICAgICAgPC9FcnJvcj4NCiAgICAgICAgPEVycm9yPg0KICAgICAgICAgIDxSYWlzZWRCeT5DaFJJUzwvUmFpc2VkQnk+IA0KICAgICAgICAgIDxOdW1iZXI+NDA4NTwvTnVtYmVyPiANCiAgICAgICAgICA8VHlwZT5zY2hlbWE8L1R5cGU+IA0KICAgICAgICAgIDxUZXh0PlZhbHVlICcxMjM0JyBkb2Vzbid0IGhhdmUgdGhlIGNvcnJlY3QgZm9ybWF0PC9UZXh0PiANCiAgICAgICAgICA8TG9jYXRpb24+L3RuczpFbnZlbG9wZVsxXS90bnM6Qm9keVsxXS9pZTpJRTgxNVsxXS9pZTpCb2R5WzFdL2llOlN1Ym1pdHRlZERyYWZ0T2ZFQURFU0FEWzFdL2llOkNvbnNpZ25vclRyYWRlclsxXS9pZTpUcmFkZXJFeGNpc2VOdW1iZXJbMV08L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy1wYXR0ZXJuLXZhbGlkOiBWYWx1ZSAnMTIzNCcgaXMgbm90IGZhY2V0LXZhbGlkIHdpdGggcmVzcGVjdCB0byBwYXR0ZXJuICdbQS1aXXsyfVthLXpBLVowLTldezExfScgZm9yIHR5cGUgJ0V4Y2lzZU51bWJlclR5cGUnLjwvRGV2ZWxvcGVyTWVzc2FnZT4gDQogICAgICAgICAgICA8L01lc3NhZ2VzPg0KICAgICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDwvRXJyb3I+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwNjU8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5JbnZhbGlkIGNvbnRlbnQgZm91bmQgYXQgZWxlbWVudCAnTWVzc2FnZUlkZW50aWZpZXInPC9UZXh0PiANCiAgICAgICAgICA8TG9jYXRpb24+L3RuczpFbnZlbG9wZVsxXS90bnM6Qm9keVsxXS9pZTpJRTgxNVsxXS9pZTpIZWFkZXJbMV0vdG1zOk1lc3NhZ2VJZGVudGlmaWVyWzFdPC9Mb2NhdGlvbj4gDQogICAgICAgICAgPEFwcGxpY2F0aW9uPg0KICAgICAgICAgICAgPE1lc3NhZ2VzPg0KICAgICAgICAgICAgICA8RGV2ZWxvcGVyTWVzc2FnZT5jdmMtY29tcGxleC10eXBlLjIuNC5hOiBJbnZhbGlkIGNvbnRlbnQgd2FzIGZvdW5kIHN0YXJ0aW5nIHdpdGggZWxlbWVudCAnTWVzc2FnZUlkZW50aWZpZXInLiBPbmUgb2YgJ3sidXJuOnB1YmxpY2lkOi06RUM6REdUQVhVRDpFTUNTOlBIQVNFNDpUTVM6VjMuMDEiOlRpbWVPZlByZXBhcmF0aW9ufScgaXMgZXhwZWN0ZWQuPC9EZXZlbG9wZXJNZXNzYWdlPiANCiAgICAgICAgICAgIDwvTWVzc2FnZXM+DQogICAgICAgICAgPC9BcHBsaWNhdGlvbj4NCiAgICAgICAgPC9FcnJvcj4NCiAgICAgICAgPEVycm9yPg0KICAgICAgICAgIDxSYWlzZWRCeT5DaFJJUzwvUmFpc2VkQnk+IA0KICAgICAgICAgIDxOdW1iZXI+NDA2NTwvTnVtYmVyPiANCiAgICAgICAgICA8VHlwZT5zY2hlbWE8L1R5cGU+IA0KICAgICAgICAgIDxUZXh0PkludmFsaWQgY29udGVudCBmb3VuZCBhdCBlbGVtZW50ICdoOkNvbnNpZ25vcklkJzwvVGV4dD4gDQogICAgICAgICAgPExvY2F0aW9uPi90bnM6RW52ZWxvcGVbMV0vdG5zOkhlYWRlclsxXS9lbWNzLWluZm8taGVhZGVyOkVNQ1NJbmZvWzFdL2VtY3MtaW5mby1oZWFkZXI6Q29uc2lnbm9ySWRbMV08L0xvY2F0aW9uPiANCiAgICAgICAgICA8QXBwbGljYXRpb24+DQogICAgICAgICAgICA8TWVzc2FnZXM+DQogICAgICAgICAgICAgIDxEZXZlbG9wZXJNZXNzYWdlPmN2Yy10eXBlLjMuMS4zOiBUaGUgdmFsdWUgJzEyMycgb2YgZWxlbWVudCAnaDpDb25zaWdub3JJZCcgaXMgbm90IHZhbGlkLjwvRGV2ZWxvcGVyTWVzc2FnZT4gDQogICAgICAgICAgICA8L01lc3NhZ2VzPg0KICAgICAgICAgIDwvQXBwbGljYXRpb24+DQogICAgICAgIDwvRXJyb3I+DQogICAgICAgIDxFcnJvcj4NCiAgICAgICAgICA8UmFpc2VkQnk+Q2hSSVM8L1JhaXNlZEJ5PiANCiAgICAgICAgICA8TnVtYmVyPjQwODU8L051bWJlcj4gDQogICAgICAgICAgPFR5cGU+c2NoZW1hPC9UeXBlPiANCiAgICAgICAgICA8VGV4dD5WYWx1ZSAnMTIzJyBkb2Vzbid0IGhhdmUgdGhlIGNvcnJlY3QgZm9ybWF0PC9UZXh0PiANCiAgICAgICAgICA8TG9jYXRpb24+L3RuczpFbnZlbG9wZVsxXS90bnM6SGVhZGVyWzFdL2VtY3MtaW5mby1oZWFkZXI6RU1DU0luZm9bMV0vZW1jcy1pbmZvLWhlYWRlcjpDb25zaWdub3JJZFsxXTwvTG9jYXRpb24+IA0KICAgICAgICAgIDxBcHBsaWNhdGlvbj4NCiAgICAgICAgICAgIDxNZXNzYWdlcz4NCiAgICAgICAgICAgICAgPERldmVsb3Blck1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcxMjMnIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnW0EtWl17Mn1bYS16QS1aMC05XXsxMX0nIGZvciB0eXBlICcjQW5vblR5cGVfQ29uc2lnbm9ySWRFTUNTSW5mbycuPC9EZXZlbG9wZXJNZXNzYWdlPiANCiAgICAgICAgICAgIDwvTWVzc2FnZXM+DQogICAgICAgICAgPC9BcHBsaWNhdGlvbj4NCiAgICAgICAgPC9FcnJvcj4NCiAgICAgIDwvRXJyb3JSZXNwb25zZT4NCiAgICA8L0hNUkNTT0FQUmVzcG9uc2U+DQogIDwvc29hcGVudjpCb2R5Pg0KPC9zb2FwZW52OkVudmVsb3BlPg==</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>";
		static readonly string XmlFailureResponseTextDecoded = @"<soapenv:Envelope xmlns:soapenv=""http://www.w3.org/2003/05/soap-envelope"">
  <soapenv:Body>
    <HMRCSOAPResponse xmlns=""http://www.inlandrevenue.gov.uk/SOAP/Response/2"">
      <ErrorResponse xmlns=""http://www.govtalk.gov.uk/CM/errorresponse"" SchemaVersion=""2.0"">
        <Application>
          <MessageCount>15</MessageCount> 
        </Application>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4052</Number> 
          <Type>schema</Type> 
          <Text>Element 'q1:SealInformation' must only have valid text as its content</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.2.2: Element 'q1:SealInformation' must have no element [children], and the value must be valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4001</Number> 
          <Type>schema</Type> 
          <Text>Missing attribute on element 'q1:SealInformation'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]/@language</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.4: Attribute 'language' must appear on element 'q1:SealInformation'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,350}' for type 'SealInformationType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:CommercialSealIdentification[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,35}' for type 'CommercialSealIdentificationType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:CommercialSealIdentification'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:CommercialSealIdentification[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '' of element 'q1:CommercialSealIdentification' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:Quantity'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Quantity[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '0' of element 'q1:Quantity' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '0' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Quantity[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '0' is not facet-valid with respect to pattern '[1-9]\d{0,14}|([1-9]\d{0,13}|0)\.[0-9]|([1-9]\d{0,12}|0)\.\d[0-9]|([1-9]\d{0,11}|0)\.\d\d[0-9]' for type 'QuantityType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:FirstTransporterTrader'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:FirstTransporterTrader[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.2.4.a: Invalid content was found starting with element 'q1:FirstTransporterTrader'. One of '{""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"":DeliveryPlaceCustomsOffice, ""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"":CompetentAuthorityDispatchOffice}' is expected.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:ReferenceOfTaxWarehouse'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:PlaceOfDispatchTrader[1]/ie:ReferenceOfTaxWarehouse[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '69' of element 'q1:ReferenceOfTaxWarehouse' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '69' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:PlaceOfDispatchTrader[1]/ie:ReferenceOfTaxWarehouse[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '69' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type 'ExciseNumberType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:TraderExciseNumber'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:ConsignorTrader[1]/ie:TraderExciseNumber[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '1234' of element 'q1:TraderExciseNumber' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '1234' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:ConsignorTrader[1]/ie:TraderExciseNumber[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '1234' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type 'ExciseNumberType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'MessageIdentifier'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Header[1]/tms:MessageIdentifier[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.2.4.a: Invalid content was found starting with element 'MessageIdentifier'. One of '{""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13"":TimeOfPreparation}' is expected.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'h:ConsignorId'</Text> 
          <Location>/tns:Envelope[1]/tns:Header[1]/emcs-info-header:EMCSInfo[1]/emcs-info-header:ConsignorId[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '123' of element 'h:ConsignorId' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '123' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Header[1]/emcs-info-header:EMCSInfo[1]/emcs-info-header:ConsignorId[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '123' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type '#AnonType_ConsignorIdEMCSInfo'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
      </ErrorResponse>
    </HMRCSOAPResponse>
  </soapenv:Body>
</soapenv:Envelope>";
		static readonly string ExpectedPrettyHTMLInterpretationXml = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><h3>EMCS Response Errors</h3><h4>Summary: Message not accepted</h4><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4052</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Element 'q1:SealInformation' must only have valid text as its content</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]</td></tr><tr><td>Message</td><td>cvc-complex-type.2.2: Element 'q1:SealInformation' must have no element [children], and the value must be valid.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4001</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Missing attribute on element 'q1:SealInformation'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]/@language</td></tr><tr><td>Message</td><td>cvc-complex-type.4: Attribute 'language' must appear on element 'q1:SealInformation'.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4085</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Value '' doesn't have the correct format</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]</td></tr><tr><td>Message</td><td>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,350}' for type 'SealInformationType'.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4085</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Value '' doesn't have the correct format</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:CommercialSealIdentification[1]</td></tr><tr><td>Message</td><td>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,35}' for type 'CommercialSealIdentificationType'.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'q1:CommercialSealIdentification'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:CommercialSealIdentification[1]</td></tr><tr><td>Message</td><td>cvc-type.3.1.3: The value '' of element 'q1:CommercialSealIdentification' is not valid.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'q1:Quantity'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Quantity[1]</td></tr><tr><td>Message</td><td>cvc-type.3.1.3: The value '0' of element 'q1:Quantity' is not valid.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4085</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Value '0' doesn't have the correct format</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Quantity[1]</td></tr><tr><td>Message</td><td>cvc-pattern-valid: Value '0' is not facet-valid with respect to pattern '[1-9]\d{0,14}|([1-9]\d{0,13}|0)\.[0-9]|([1-9]\d{0,12}|0)\.\d[0-9]|([1-9]\d{0,11}|0)\.\d\d[0-9]' for type 'QuantityType'.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'q1:FirstTransporterTrader'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:FirstTransporterTrader[1]</td></tr><tr><td>Message</td><td>cvc-complex-type.2.4.a: Invalid content was found starting with element 'q1:FirstTransporterTrader'. One of '{""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"":DeliveryPlaceCustomsOffice, ""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"":CompetentAuthorityDispatchOffice}' is expected.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'q1:ReferenceOfTaxWarehouse'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:PlaceOfDispatchTrader[1]/ie:ReferenceOfTaxWarehouse[1]</td></tr><tr><td>Message</td><td>cvc-type.3.1.3: The value '69' of element 'q1:ReferenceOfTaxWarehouse' is not valid.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4085</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Value '69' doesn't have the correct format</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:PlaceOfDispatchTrader[1]/ie:ReferenceOfTaxWarehouse[1]</td></tr><tr><td>Message</td><td>cvc-pattern-valid: Value '69' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type 'ExciseNumberType'.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'q1:TraderExciseNumber'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:ConsignorTrader[1]/ie:TraderExciseNumber[1]</td></tr><tr><td>Message</td><td>cvc-type.3.1.3: The value '1234' of element 'q1:TraderExciseNumber' is not valid.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4085</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Value '1234' doesn't have the correct format</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:ConsignorTrader[1]/ie:TraderExciseNumber[1]</td></tr><tr><td>Message</td><td>cvc-pattern-valid: Value '1234' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type 'ExciseNumberType'.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'MessageIdentifier'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Header[1]/tms:MessageIdentifier[1]</td></tr><tr><td>Message</td><td>cvc-complex-type.2.4.a: Invalid content was found starting with element 'MessageIdentifier'. One of '{""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13"":TimeOfPreparation}' is expected.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content found at element 'h:ConsignorId'</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Header[1]/emcs-info-header:EMCSInfo[1]/emcs-info-header:ConsignorId[1]</td></tr><tr><td>Message</td><td>cvc-type.3.1.3: The value '123' of element 'h:ConsignorId' is not valid.</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>ChRIS</td></tr><tr><td>Error Number</td><td>4085</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Value '123' doesn't have the correct format</td></tr><tr><td>Location</td><td>/tns:Envelope[1]/tns:Header[1]/emcs-info-header:EMCSInfo[1]/emcs-info-header:ConsignorId[1]</td></tr><tr><td>Message</td><td>cvc-pattern-valid: Value '123' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type '#AnonType_ConsignorIdEMCSInfo'.</td></tr></table></ul><br>";
		static readonly string JsonFailureResponseTextDecoded = @"{
    ""dateTime"": ""2023-12-06T11:34:13.191992"",
    ""debugMessage"": ""Error while parsing &lt;q1:Header xmlns:q1=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13\""&gt;&lt;MessageSender xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;NDEA.GB&lt;/MessageSender&gt;&lt;MessageRecipient xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;NDEA.GB&lt;/MessageRecipient&gt;&lt;DateOfPreparation xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;2023-12-05&lt;/DateOfPreparation&gt;&lt;MessageIdentifier xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;48&lt;/MessageIdentifier&gt;&lt;CorrelationIdentifier xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;48&lt;/CorrelationIdentifier&gt;&lt;TimeOfPreparation xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;10:59:21&lt;/TimeOfPreparation&gt;&lt;/q1:Header&gt;: parser error \""'{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation' expected but {urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier found\"" while parsing /{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}IE815/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}Header/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageSender{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageRecipient{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}DateOfPreparation{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}CorrelationIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation\n   ^"",
    ""emcsCorrelationId"": ""00000000-0000-0000-0000-000000000000"",
    ""message"": ""Not valid IE815 message"",
	""errors"": [
        {
            ""errorCode"": 8081,
            ""errorMessage"": ""The Identity of Transport Units must be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit."",
            ""location"": ""/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]"",
            ""value"": null
        },
        {
            ""errorCode"": 8082,
            ""errorMessage"": ""The Identity of Transport Units must not be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit."",
            ""location"": ""/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]"",
            ""value"": ""Unknown unit""
        }
    ]
}";
		static readonly string JsonFailureResponseTextDecodedV2 = @"{
    ""dateTime"": ""2023-12-06T11:34:13.191992"",
    ""debugMessage"": ""Error while parsing &lt;q1:Header xmlns:q1=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13\""&gt;&lt;MessageSender xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;NDEA.GB&lt;/MessageSender&gt;&lt;MessageRecipient xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;NDEA.GB&lt;/MessageRecipient&gt;&lt;DateOfPreparation xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;2023-12-05&lt;/DateOfPreparation&gt;&lt;MessageIdentifier xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;48&lt;/MessageIdentifier&gt;&lt;CorrelationIdentifier xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;48&lt;/CorrelationIdentifier&gt;&lt;TimeOfPreparation xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;10:59:21&lt;/TimeOfPreparation&gt;&lt;/q1:Header&gt;: parser error \""'{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation' expected but {urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier found\"" while parsing /{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}IE815/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}Header/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageSender{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageRecipient{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}DateOfPreparation{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}CorrelationIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation\n   ^"",
    ""correlationId"": ""00000000-0000-0000-0000-000000000000"",
    ""message"": ""Not valid IE815 message"",
    ""validatorResults"": [
        {
			""errorCategory"": ""business"",
            ""errorType"": 8081,
            ""errorReason"": ""The Identity of Transport Units must be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit."",
            ""errorLocation"": ""/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]"",
            ""originalAttributeValue"": null
        },
        {
			""errorCategory"": ""business"",
            ""errorType"": 8082,
            ""errorReason"": ""The Identity of Transport Units must not be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit."",
            ""errorLocation"": ""/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]"",
            ""originalAttributeValue"": ""Unknown unit""
        }
    ]
}";

		static readonly string ExpectedPrettyHTMLInterpretationJson =   @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Message</td></tr><tr><td>Not valid IE815 message</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Debug Message</td></tr><tr><td>Error while parsing &lt;q1:Header xmlns:q1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13""&gt;&lt;MessageSender xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;NDEA.GB&lt;/MessageSender&gt;&lt;MessageRecipient xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;NDEA.GB&lt;/MessageRecipient&gt;&lt;DateOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;2023-12-05&lt;/DateOfPreparation&gt;&lt;MessageIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;48&lt;/MessageIdentifier&gt;&lt;CorrelationIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;48&lt;/CorrelationIdentifier&gt;&lt;TimeOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;10:59:21&lt;/TimeOfPreparation&gt;&lt;/q1:Header&gt;: parser error ""'{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation' expected but {urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier found"" while parsing /{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}IE815/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}Header/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageSender{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageRecipient{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}DateOfPreparation{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}CorrelationIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation<br>   ^</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Date-Time</td></tr><tr><td>2023-12-06T11:34:13.191992</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Correlation ID</td></tr><tr><td>00000000-0000-0000-0000-000000000000</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Error Code</td><td>8081</td></tr><tr><td>Error Message</td><td>The Identity of Transport Units must be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit.</td></tr><tr><td>Location</td><td>/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]</td></tr><tr><td>Value</td><td>&nbsp;</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Error Code</td><td>8082</td></tr><tr><td>Error Message</td><td>The Identity of Transport Units must not be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit.</td></tr><tr><td>Location</td><td>/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]</td></tr><tr><td>Value</td><td>Unknown unit</td></tr></table></BR>";
		static readonly string ExpectedPrettyHTMLInterpretationJsonV2 = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Message</td></tr><tr><td>Not valid IE815 message</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Debug Message</td></tr><tr><td>Error while parsing &lt;q1:Header xmlns:q1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13""&gt;&lt;MessageSender xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;NDEA.GB&lt;/MessageSender&gt;&lt;MessageRecipient xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;NDEA.GB&lt;/MessageRecipient&gt;&lt;DateOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;2023-12-05&lt;/DateOfPreparation&gt;&lt;MessageIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;48&lt;/MessageIdentifier&gt;&lt;CorrelationIdentifier xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;48&lt;/CorrelationIdentifier&gt;&lt;TimeOfPreparation xmlns=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""&gt;10:59:21&lt;/TimeOfPreparation&gt;&lt;/q1:Header&gt;: parser error ""'{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation' expected but {urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier found"" while parsing /{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}IE815/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}Header/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageSender{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageRecipient{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}DateOfPreparation{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}CorrelationIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation<br>   ^</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Date-Time</td></tr><tr><td>2023-12-06T11:34:13.191992</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Correlation ID</td></tr><tr><td>00000000-0000-0000-0000-000000000000</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Error Code</td><td>business 8081</td></tr><tr><td>Error Message</td><td>The Identity of Transport Units must be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit.</td></tr><tr><td>Location</td><td>/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]</td></tr><tr><td>Value</td><td>&nbsp;</td></tr></table></BR><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Error Code</td><td>business 8082</td></tr><tr><td>Error Message</td><td>The Identity of Transport Units must not be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit.</td></tr><tr><td>Location</td><td>/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]</td></tr><tr><td>Value</td><td>Unknown unit</td></tr></table></BR>";
	}
}
