using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class ErrorsHelperTests : TestCase
	{
		public void TestErrorResponses_Missing()
		{
			const string messageText = "<xyz></xyz>";
			var helper = new ErrorsHelper(messageText, "//xyz");
			AssertEquals(0, helper.ErrorResponses.Count);
		}

		public void TestErrorResponses_Empty()
		{
			const string messageText = "<errorResponse><errors/></errorResponse>";
			var helper = new ErrorsHelper(messageText, "//errorResponse");
			AssertEquals(0, helper.ErrorResponses.Count);
		}

		public void TestErrorResponses_Error()
		{
			const string messageText = "<xyz><errors><error><code>x</code><message>:message</message></error></errors></xyz>";
			var helper = new ErrorsHelper(messageText, "//xyz");
			AssertEquals(1, helper.ErrorResponses.Count);
			AssertEquals("message", helper.ErrorResponses[0].DisplayError);
		}

		public void TestErrorResponses_Errors()
		{
			const string messageText = @"<errorResponse>
      <code>BAD_REQUEST</code>
      <message>Payload is not valid according to schema</message>
      <errors>
        <error>
          <code>xml_validation_error</code>
          <message>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.*[^\s].*' for type '#AnonType_SupervisingOfficeIdentificationIDType'.</message>
        </error><error>
          <code>xml_validation_error</code>
          <message>cvc-complex-type.2.2: Element 'ID' must have no element [children], and the value must be valid.</message>
        </error>
		<error>
         <code>xml_validation_error</code>
         <message>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '[A-Z]{3}' for type 'ISO3AlphaCurrencyCodeContentType'.</message>
      </error>
      <error>
         <code>xml_validation_error</code>
         <message>cvc-attribute.3: The value '' of attribute 'currencyID' on element 'StatisticalValueAmount' is not valid with respect to its type, 'ISO3AlphaCurrencyCodeContentType'.</message>
      </error>
      <error>
         <code>xml_validation_error</code>
         <message>cvc-pattern-valid: We recognise the category but cannot parse the rest of it - do not explode.</message>
      </error>
      <error>
         <code>xml_validation_error</code>
         <message>whatever-category: We do not even recognise the category so have no idea what to expect here - but still do not explode.</message>
      </error>
      </errors>
    </errorResponse>";
			var helper = new ErrorsHelper(messageText, "//errorResponse");
			AssertEquals(6, helper.ErrorResponses.Count);
			AssertContainsExactElementsInExactOrder(helper.ErrorResponses.Select(x => x.DisplayError), new string[] {
				"Supervising office, coded - Value '' is not facet-valid with respect to pattern '.*[^\\s].*'",
				"Element 'ID' must have no element [children], and the value must be valid.",
				"Value '' is not facet-valid with respect to pattern '[A-Z]{3}'",
				"The value '' of attribute 'currencyID' on element 'StatisticalValueAmount' is not valid with respect to its type, 'ISO3AlphaCurrencyCodeContentType'.",
				"We recognise the category but cannot parse the rest of it - do not explode.",
				"We do not even recognise the category so have no idea what to expect here - but still do not explode."
			});
		}
	}
}
