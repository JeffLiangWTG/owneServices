using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARContraController))]
	class ARContraControllerTest : ContraControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARContra;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return ARRow; }
		}

		protected override BusinessObject ParentTransactionHeaderRowNotSaved
		{
			get { return new BusinessObjectFactory().New(typeof(ARContraRow)); }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewReceivablesContra; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesContra; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesContra; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesContra; }
		}
	}
}
