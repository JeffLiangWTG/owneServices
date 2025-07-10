using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Utils;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Retrieve
{
	public class RetrieveHandlerTest : TransactionedTestCase
	{
		public void TestParseXml_WithSameCriteriaType()
		{
			string input = string.Empty;
			input += "<Organization xmlns=\"http://www.cargowise.com/Schemas/Universal\">";
			input += "	<CriteriaGroup Type=\"Key\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "	<CriteriaGroup Type=\"Key\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "</Organization>";
			var doc = XElement.Parse(input);

			var retriever = new RetrieveHandler();
			var result = retriever.GetCriteriaGroups(doc);

			AssertEquals(2, result.Count(item => item.Type == "Key"));
		}

		public void TestParseXml_WithMultipleCriteriaGroups()
		{
			var input = string.Empty;
			input += "<Organization xmlns=\"http://www.cargowise.com/Schemas/Universal\">";
			input += "	<CriteriaGroup Type=\"Score\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "	<CriteriaGroup Type=\"Partial\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "	<CriteriaGroup Type=\"Key\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "</Organization>";
			var doc = XElement.Parse(input);

			var retriever = new RetrieveHandler();
			var result = retriever.GetCriteriaGroups(doc);

			AssertEquals(1, result.Count(item => item.Type == "Key"));
			AssertEquals(1, result.Count(item => item.Type == "Score"));
			AssertEquals(1, result.Count(item => item.Type == "Partial"));
		}

		public void TestParseXml_WithSingleCriteriaGroup()
		{
			var input = string.Empty;
			input += "<Organization xmlns=\"http://www.cargowise.com/Schemas/Universal\">";
			input += "	<CriteriaGroup Type=\"Key\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "</Organization>";
			var doc = XElement.Parse(input);

			var retriever = new RetrieveHandler();
			var result = retriever.GetCriteriaGroups(doc);

			AssertEquals(1, result.Count(item => item.Type == "Key"));
		}

		public void TestParseXml_WithExternalEntity()
		{
			var input = string.Empty;
			input += "<CurrencyExchangeRate xmlns=\"http://www.cargowise.com/Schemas/Universal\">";
			input += "	<CriteriaGroup Type=\"Partial\">";
			input += "		<Criteria Entity=\"RefExchangeRate.RefCurrency\" FieldName=\"Code\">USD</Criteria>";
			input += "	</CriteriaGroup>";
			input += "</CurrencyExchangeRate>";
			var doc = XElement.Parse(input);

			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			CombineAssertions(delegate
			{
				var status = response.Status;
				AssertEquals(NativeResponseStatus.Accepted, status);

				var informations = response.Informations;
				Assert(informations.Any(i => i.Contains("1 matches found")));
			});
		}

#region Exceptional Case

		[ExpectNoExceptions]
		public void TestRetrieve_NoCriteriaGroup()
		{
			var input = string.Empty;
			input += "<Vessel>";
			input += "</Vessel>";
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			//<?xml version='1.0' encoding='utf-16'?>
			//<RetrieveDataResponse EntitySet='Vessel'>
			//  <Notifications>
			//    <Status>Rejected</Status>
			//    <Information>
			//      <Item>Error - Could not find valid criteria group</Item>
			//    </Information>
			//  </Notifications>
			//</RetrieveDataResponse>

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Could not find valid criteria group")));
		}

		[ExpectNoExceptions]
		public void TestRetrieve_InvalidCriteriaGroup()
		{
			var input = string.Empty;
			input += "<Vessel>";
			input += "	<Criteria Type=\"Partial\" Entity=\"RefVessel\" FieldName=\"Code\">Arnis</Criteria>";
			input += "</Vessel>";
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			//<?xml version='1.0' encoding='utf-16'?>
			//<RetrieveDataResponse EntitySet='Vessel'>
			//  <Notifications>
			//    <Status>Rejected</Status>
			//    <Information>
			//      <Item>Error - Could not find valid criteria group</Item>
			//    </Information>
			//  </Notifications>
			//</RetrieveDataResponse>

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Could not find valid criteria group")));
		}

		[ExpectNoExceptions]
		public void TestRetrieve_UnknownEntitySet()
		{
			var input = "<RetrieveDataRequest EntitySet=\"Crap\" />";
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			retriever.Execute(request, new DummyResponseFactory());
		}

		public void TestRetrieve_UnknownRetrieveType()
		{
			var input = string.Empty;
			input += "<Organization>";
			input += "	<CriteriaGroup Type=\"Blah\">";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "</Organization>";
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Unknown retrieve type - Blah")));
		}

		public void TestRetrieve_UnknownRetrieveType_EmptyType()
		{
			var input = string.Empty;
			input += "<Organization>";
			input += "	<CriteriaGroup>";
			input += "		<Criteria Entity=\"OrgHeader\" FieldName=\"PK\">24181EEA-D3E5-4AFE-8892-8684AB555879</Criteria>";
			input += "	</CriteriaGroup>";
			input += "</Organization>";
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			//<?xml version='1.0' encoding='utf-16'?>
			//<RetrieveDataResponse EntitySet='Organization'>
			//	<Notifications>
			//    <Status>Rejected</Status>
			//    <Information>
			//      <Item>Error - Empty retrieve type</Item>
			//    </Information>
			//  </Notifications>
			//</RetrieveDataResponse>

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Retrieve type is empty")));
		}

		public void TestRetrieve_NoCriterias()
		{
			var input = string.Empty;
			input += "<Organization>";
			input += "	<CriteriaGroup  Type=\"Key\"/>";
			input += "</Organization>";
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("No Criteria elements")));
		}

		public void TestRetrieve_ErrorCriteria()
		{
			var input =
@"<UNLOCO>
    <CriteriaGroup Type=""Key"">
    <Criteria Entity=""RefUNLOCO"" FieldName=""Codde"">FOO</Criteria>
    </CriteriaGroup>
</UNLOCO>";

			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Equals(@"Error - Key based retrieval only supports Primary or Candidate keys as field name. ""Codde"" is not a primary or candidate key for table ""RefUNLOCO""")));
			Assert(informations.Any(i => i.Equals("Information - 0 matches found.")));
		}

		public void TestRetrieve_EmptyEntitySets()
		{
			var retriever = new RetrieveHandler();
			var request = new Request { EntitySets = Array.Empty<XElement>() };
			var response = retriever.Execute(request, new DummyResponseFactory());

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Equals(@"Error - Body element must not be empty.")));
		}

		public void TestRetrieve_UserVisibleException_NoStackTrace()
		{
			var definitionFinder = new Mock<IDefinitionFinder>();
			var exception = new NativeXMLUserVisibleException("Message");
			definitionFinder.Setup(x => x.FindByEntitySetName(It.IsAny<string>())).Throws(exception);

			var input = CurrencyExchangeRateRequest;
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			retriever.DefinitionFinder = definitionFinder.Object;
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Message")));
			Assert(informations.All(e => !e.Contains(exception.StackTrace)));
			definitionFinder.VerifyAll();
		}

		public void TestRetrieve_OtherException_ShowStackTrace()
		{
			var definitionFinder = new Mock<IDefinitionFinder>();
			var exception = new Exception("Message");
			definitionFinder.Setup(x => x.FindByEntitySetName(It.IsAny<string>())).Throws(exception);

			var input = CurrencyExchangeRateRequest;
			var doc = XElement.Parse(input);
			var retriever = new RetrieveHandler();
			retriever.DefinitionFinder = definitionFinder.Object;
			var request = new Request { EntitySets = new[] { doc } };
			var response = retriever.Execute(request, new DummyResponseFactory());

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Message")));
			Assert(informations.Any(e => e.Contains(exception.StackTrace)));
			AssertContains("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
			definitionFinder.VerifyAll();
			ErrorReporter.Clear();
		}

		const string CurrencyExchangeRateRequest =
@"<CurrencyExchangeRate>
	<CriteriaGroup Type=""Partial"">
		<Criteria Table=""RefCurrency"" FieldName=""Code"">US%</Criteria>""
	</CriteriaGroup>
</CurrencyExchangeRate>";

#endregion

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
	}
}
