using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobStatusActionMethod : OperationalActionMethod
	{
		public UpdateJobStatusActionMethod() : base(new Guid("ee280bfe-c787-4dff-bdf0-e0558f369111"))
		{
		}

		public override OperationalActionMethodApplicator
			NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateJobStatusActionMethodApplicator(factory);
		}

		public override string Name => ActionMethodName;

		public override string Description => ActionMethodName;

		string ActionMethodName => Res.GetString("ab5403d1-0df2-4301-b7b1-e675fad6dbcc", "Update Job Status");

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateJobStatusUserControl();
		}
	}
}
