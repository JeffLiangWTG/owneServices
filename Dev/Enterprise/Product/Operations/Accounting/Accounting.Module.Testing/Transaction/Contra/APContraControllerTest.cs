using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APContraController))]
	class APContraControllerTest : ContraControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APContra;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return APRow; }
		}

		protected override BusinessObject ParentTransactionHeaderRowNotSaved
		{
			get { return new BusinessObjectFactory().New(typeof(APContraRow)); }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewPayablesContra; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewPayablesContra; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesContra; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesContra; }
		}
	}
}
