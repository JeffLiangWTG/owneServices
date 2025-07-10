using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARAdjustmentNoteController))]
	public class ARAdjustmentNoteControllerTest : InvoicingBaseControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARAdjustmentNote;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(ARAdjustmentNote);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(GUI.AdjustmentNoteForm);
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesAdjustmentNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesAdjustmentNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}
	}
}
