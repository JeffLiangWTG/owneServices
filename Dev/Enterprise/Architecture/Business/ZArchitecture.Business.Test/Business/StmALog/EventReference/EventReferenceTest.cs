using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(EventReference))]
	sealed class EventReferenceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGenerateEventReference()
		{
			var eventReference1 = new EventReference(Events.CustomisableEvent00Code, ZString.Empty);
			eventReference1.FreeText = "TEST REFERENCE";
			eventReference1.ParameterCollection.Add(new Parameter(Events.CustomisableEvent00Code, "LOC", "AUSYD"));
			eventReference1.ParameterCollection.Add(new Parameter(Events.CustomisableEvent00Code, "FAC", "TERMINAL"));

			AssertEquals("TEST REFERENCE|FAC=TERMINAL|LOC=AUSYD", eventReference1.CompleteText);

			var eventReference2 = new EventReference(Events.CustomisableEvent00Code, "TESTING|RES=REASON");
			eventReference2.ParameterCollection.Add(new Parameter(Events.CustomisableEvent00Code, "LOC", "AUSYD"));
			eventReference2.ParameterCollection.Add(new Parameter(Events.CustomisableEvent00Code, "FAC", "TERMINAL"));
			AssertEquals("TESTING|FAC=TERMINAL|LOC=AUSYD|RES=REASON", eventReference2.CompleteText);
		}

		public void TestCompleteText()
		{
			var eventReference = new EventReference(Events.CustomisableEvent00Code, ZString.Empty);
			AssertEquals(ZString.Empty, eventReference.FreeText);
			AssertEquals(0, eventReference.ParameterCollection.Count);

			eventReference.CompleteText = "TESTING|LOC=AUSYD";
			AssertEquals("TESTING", eventReference.FreeText);
			AssertEquals(1, eventReference.ParameterCollection.Count);

			eventReference.CompleteText = "LCC";
			AssertEquals("LCC", eventReference.FreeText);
			AssertEquals(0, eventReference.ParameterCollection.Count);

			eventReference.CompleteText = "|LOC=AUSYD";
			AssertEquals(ZString.Empty, eventReference.FreeText);
			AssertEquals(1, eventReference.ParameterCollection.Count);
		}

		public void TestRunPreSaveValidation()
		{
			var eventReference1 = new EventReference(Events.CustomisableEvent00Code, "TESTING|LOC=AUSYD");
			eventReference1.RunPreSaveValidation();

			var param1 = eventReference1.ParameterCollection.Cast<Parameter>().FirstOrDefault();

			AssertNotNull(param1);
			AssertNoErrors(param1.CodeInfo);
			AssertNoErrors(param1.ParamValueInfo);

			var eventReference2 = new EventReference(Events.CustomisableEvent00Code, "TESTING|LOC=AUMEL");

			var param2 = new Parameter(Events.CustomisableEvent00Code);
			param2.Code = "LOC";
			param2.ParamValue = "AUMEL";

			eventReference2.ParameterCollection.Add(param2);
			eventReference2.RunPreSaveValidation();

			AssertNotNull("Precondition", param2);
			AssertHasError(param2.CodeInfo, "The parameter LOC has been duplicated and must be unique.");
			AssertNoErrors(param2.ParamValueInfo);
		}

		public void TestValidateFreeText()
		{
			var warningMessage = $@"In your expression '|AAAA=HI' has been recognized as a free text. 
If your intention is to enter this as a set of event parameters with values, you need to ensure the syntax is correct and the pipe character | is used at the start of each parameter.
A parameter code must not be longer than {Parameter.Schema.CodeMaxLength} characters.
Correct syntax is: Free Text|PAR=Value|PAR=Value etc.";
			var eventReference = new EventReference(Events.CustomisableEvent00Code, "|AAAA=HI");
			eventReference.RunPreSaveValidation();
			Assert(eventReference.FreeTextInfo.HasWarning(warningMessage));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EventReference(Events.CustomisableEvent00Code, string.Empty);
		}
	}
}
