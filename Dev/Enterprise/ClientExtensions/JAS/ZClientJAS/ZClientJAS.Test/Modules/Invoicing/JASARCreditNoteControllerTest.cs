using System;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module
{
	[TestedType(typeof(JASARCreditNote))]
	class JASARCreditNoteControllerTest : ARCreditNoteControllerTest
	{
		protected override Type GetExpectedFormType()
		{
			return typeof(JASARCreditNoteForm);
		}

		protected override IDisposable PreventCreationOfCreditNotesRegistryConfiguration => null;
	}
}
