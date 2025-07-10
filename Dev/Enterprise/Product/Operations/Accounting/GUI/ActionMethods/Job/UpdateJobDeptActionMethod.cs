using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobDeptActionMethod : OperationalActionMethod
	{
		public UpdateJobDeptActionMethod() : base(new Guid("0c2e23fd-db98-4510-82cf-54498fe17511"))
		{
		}

		public override OperationalActionMethodApplicator
			NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateJobDeptActionMethodApplicator(factory);
		}

		public override string Name => ActionMethodName;

		public override string Description => ActionMethodName;

		string ActionMethodName => Res.GetString("04023b69-5f1c-440f-abdc-75b5bd31372e", "Update Job Department");

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateJobDeptUserControl();
		}
	}
}
