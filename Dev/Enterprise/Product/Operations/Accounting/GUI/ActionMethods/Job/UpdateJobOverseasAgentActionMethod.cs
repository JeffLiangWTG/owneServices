using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobOverseasAgentActionMethod : OperationalActionMethod
	{
		public UpdateJobOverseasAgentActionMethod() : base(new Guid("94c211df-7e76-4e26-a33b-6fb09bbe112d"))
		{
		}

		public override OperationalActionMethodApplicator
			NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateJobOverseasAgentActionMethodApplicator(factory);
		}

		public override string Name => ActionMethodName;

		public override string Description => ActionMethodName;

		string ActionMethodName => Res.GetString("195e98d4-2a70-4a7c-abcd-ac9bb2238256", "Update Overseas Agent");

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateJobOverseasAgentUserControl();
		}
	}
}
