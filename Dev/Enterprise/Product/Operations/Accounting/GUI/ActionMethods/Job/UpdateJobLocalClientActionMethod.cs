using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobLocalClientActionMethod : OperationalActionMethod
	{
		public UpdateJobLocalClientActionMethod() : base(new Guid("2960681e-d7ca-4a60-b019-39b93d78512b"))
		{
		}

		public override OperationalActionMethodApplicator
			NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateJobLocalClientActionMethodApplicator(factory);
		}

		public override string Name => ActionMethodName;

		public override string Description => ActionMethodName;

		string ActionMethodName => Res.GetString("8cb92da3-0bad-4160-9d05-90e531ebf166", "Update Local Client");

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateJobLocalClientUserControl();
		}
	}
}
