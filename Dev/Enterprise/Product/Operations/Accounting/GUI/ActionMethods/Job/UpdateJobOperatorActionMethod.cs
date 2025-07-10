using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobOperatorActionMethod : OperationalActionMethod
	{
		public UpdateJobOperatorActionMethod() : base(new Guid("d8c16983-884d-493a-ae2e-15e2164b33af"))
		{
		}

		public override OperationalActionMethodApplicator
			NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateJobOperatorActionMethodApplicator(factory);
		}

		public override string Name => ActionMethodName;

		public override string Description => ActionMethodName;

		string ActionMethodName => Res.GetString("3368baed-5bcf-4754-ba98-07d8d023eec4", "Assign Job To Another Operator");

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateJobOperatorUserControl();
		}
	}
}
