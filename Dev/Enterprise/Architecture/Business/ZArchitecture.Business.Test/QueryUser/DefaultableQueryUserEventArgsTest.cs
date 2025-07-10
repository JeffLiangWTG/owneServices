using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class DefaultableQueryUserEventArgsTest : TestCase
	{
		public void TestConstructor_YesNoCancel()
		{
			var contextSameVals = new DialogDefaultContext(dialogIdentifier: new ZGuid("46e7effa-5ef3-497d-916e-2f39c67c7534"),
															caption: "Continue?",
															buttons: ZMessageBoxButtons.YesNoCancel,
															icon: ZMessageBoxIcon.Warning,
															context: null,
															resultsNotToSave: new[] { ZDialogResult.No, ZDialogResult.Cancel },
															showCheckboxOnly: true);

			var args1 = new DefaultableQueryUserEventArgs(contextSameVals, "Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", defaultResponse: false);
			AssertEquals("Continue?", args1.Caption);
			AssertEquals("Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", args1.Message);
			AssertEquals("Postcondition: event args should be the same", contextSameVals, args1.Context);
			AssertEquals(false, args1.Response);

			var args2 = new DefaultableQueryUserEventArgs(contextSameVals, "Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", defaultResponse: true);
			AssertEquals("Continue?", args2.Caption);
			AssertEquals("Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", args2.Message);
			AssertEquals("Postcondition: event args should be the same", contextSameVals, args2.Context);
			AssertEquals(true, args2.Response);

			AssertExceptionThrown<ArgumentNullException>(() => new DefaultableQueryUserEventArgs(null, "test", false));
		}

		public void TestConstructor_YesNo()
		{
			var contextSameVals = new DialogDefaultContext(dialogIdentifier: new ZGuid("46e7effa-5ef3-497d-916e-2f39c67c7534"),
												caption: "Continue?",
												buttons: ZMessageBoxButtons.YesNo,
												icon: ZMessageBoxIcon.Warning,
												context: null,
												resultsNotToSave: new[] { ZDialogResult.No },
												showCheckboxOnly: true);

			var args1 = new DefaultableQueryUserEventArgs(contextSameVals, "Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", defaultResponse: false);
			AssertEquals("Postcondition: correct caption", "Continue?", args1.Caption);
			AssertEquals("Postcondition: correct message", "Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", args1.Message);
			AssertEquals("Postcondition: event args should be the same", contextSameVals, args1.Context);
			AssertEquals("Postcondition: correct default response", false, args1.Response);

			var args2 = new DefaultableQueryUserEventArgs(contextSameVals, "Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", defaultResponse: true);
			AssertEquals("Postcondition: correct caption", "Continue?", args2.Caption);
			AssertEquals("Postcondition: correct message", "Not all lines have been picked, please confirm you want to finalize the pick and all unpicked lines.", args2.Message);
			AssertEquals("Postcondition: event args should be the same", contextSameVals, args2.Context);
			AssertEquals("Postcondition: correct default response", true, args2.Response);

			AssertExceptionThrown<ArgumentNullException>(() => new DefaultableQueryUserEventArgs(null, "test", true));
		}
	}
}
