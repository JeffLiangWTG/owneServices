using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class SendCancellationMessageOperationalActionMethod : FrOperationActionMethod, IApplicatorValidationSupport
	{
		public SendCancellationMessageOperationalActionMethod()
			: base(new ZGuid("92579D6B-D244-4D54-BBAD-D12D20AD0D1F"))
		{
		}

		public override string Name => Res.GetString("8290519C-1AA2-4302-BFCF-3382A940D4E5", "Send Cancellation Message");

		public override string Description => Res.GetString("645D43B1-A833-462F-AE66-AF9304AE0E5C", "Send Cancellation Message");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendCancellationMessageApplicator(factory, this);
		}

		public override IComponent NewGuiControl()
		{
			if (IsValid)
			{
				return new SendCancellationMessageOperationalApplicatorControl();
			}

			throw new NotSupportedException();
		}

		public override bool HasControl => IsValid;

		public override bool RunWithoutUI => !IsValid;

		public bool IsValid { get; set; }

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(new FilterIsDeltaIEEnabledForImportsOrExportsConstraint().Name, new string[1] { "Y" });
			return result;
		}
	}
}
