using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.CDSResponse.Testing
{
	public class ResponseTests : TestCaseWithFactory
	{
		public void TestSerializeDeserialize()
		{
			var cdsResponseEDIMessage = Factory.New<CDSResponseEDIMessage>();
			cdsResponseEDIMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>LRN123456789000-S0001000</FunctionalReferenceID>
    <ID>18GBJCM3USAFD2WD51</ID>
  </Declaration>
</Response>";
			var rsp = cdsResponseEDIMessage.MessageDataObject;
			AssertEquals("01", rsp.GetFunctionCode());
			AssertEquals(new ZDateTime(2018, 7, 27, 12, 12, 12), rsp.GetIssueDate());
			AssertEquals("18GBJCM3USAFD2WD51", rsp.GetMovementReferenceNumber());
			AssertEquals("LRN123456789000-S0001000", rsp.GetDeclarationFunctionalReferenceID());
		}

		public void TestSerializeDeserialize_102()
		{
			var cdsResponseEDIMessage = Factory.New<CDSResponseEDIMessage>();
			cdsResponseEDIMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""102"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">2018-07-27 12:12:12</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>18GBJCM3USAFD2WD51</ID>
  </Declaration>
</Response>";
			var rsp = cdsResponseEDIMessage.MessageDataObject;
			AssertEquals("01", rsp.GetFunctionCode());
			AssertEquals(new ZDateTime(2018, 7, 27, 12, 12, 12), rsp.GetIssueDate());
			AssertEquals("18GBJCM3USAFD2WD51", rsp.GetMovementReferenceNumber());
			AssertEquals("8GB123456789000-S0001000", rsp.GetDeclarationFunctionalReferenceID());
		}

		public void TestGetIssueDate()
		{
			var response = new Response
			{
				IssueDateTime = new ResponseIssueDateTimeType
				{
					Item = new ResponseIssueDateTimeTypeDateTimeString
					{
						Value = "2018-07-27 12:12:12"
					}
				}
			};
			AssertEquals(new ZDateTime(2018, 7, 27, 12, 12, 12), response.GetIssueDate());
		}

		public void TestGetMovementReferenceNumber()
		{
			var response = new Response
			{
				Declaration = new ResponseDeclaration
				{
					ID = new DeclarationIdentificationIDType1
					{
						Value = "123"
					}
				}
			};
			AssertEquals("123", response.GetMovementReferenceNumber());
		}

		public void TestGetFunctionalReferenceID()
		{
			var response = new Response
			{
				Declaration = new ResponseDeclaration
				{
					FunctionalReferenceID = new DeclarationFunctionalReferenceIDType1
					{
						Value = "8GB123456789000-S0001000"
					}
				}
			};
			AssertEquals("8GB123456789000-S0001000", response.GetDeclarationFunctionalReferenceID());
		}

		public void TestGetFunctionCode()
		{
			var response = new Response
			{
				FunctionCode = new ResponseFunctionCodeType
				{
					Value = "01"
				}
			};
			AssertEquals("01", response.GetFunctionCode());
		}

		public void TestGetResponseFunction()
		{
			var response = new Response
			{
				FunctionCode = new ResponseFunctionCodeType
				{
					Value = "01"
				}
			};
			AssertType<ResponseFunction.DeclarationAccepted>(response.GetResponseFunction());
		}

		public void TestHasErrors()
		{
			var response = new Response();

			CombineAssertions(() =>
			{
				AssertEquals("Null - No Error", false, response.HasErrors());

				response.Error = System.Array.Empty<ResponseError>();
				AssertEquals("Empty - No Error", false, response.HasErrors());

				response.Error = new ResponseError[] { new ResponseError() };
				AssertEquals("Not Empty - Has Error", true, response.HasErrors());
			});
		}
	}
}
