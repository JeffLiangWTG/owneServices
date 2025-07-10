using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobProfitLossReasonActionMethod : OperationalActionMethod
	{
		public UpdateJobProfitLossReasonActionMethod() : base(new Guid("edc272e9-92c0-4dbf-a8c4-c3db07fa7523"))
		{
		}

		public override OperationalActionMethodApplicator
			NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateJobProfitLossReasonActionMethodApplicator(factory);
		}

		public override string Name => ActionMethodName;

		public override string Description => ActionMethodName;

		string ActionMethodName => Res.GetString("7e3cb02b-94ff-4524-8fc7-30fa79ae2601", "Updates Job Profit / Loss Reason Code");

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateJobProfitLossReasonUserControl();
		}
	}
}
