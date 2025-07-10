using System;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module
{
	[TestedType(typeof(JASARAdjustmentNote))]
	class JASARAdjustmentNoteControllerTest : ARAdjustmentNoteControllerTest
	{
		protected override Type GetExpectedFormType()
		{
			return typeof(JASARAdjustmentNoteForm);
		}
	}
}
