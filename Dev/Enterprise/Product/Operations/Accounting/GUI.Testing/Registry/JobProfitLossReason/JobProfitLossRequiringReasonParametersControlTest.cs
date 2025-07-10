using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobProfitLossRequiringReasonParametersControl))]
	class JobProfitLossRequiringReasonParametersControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new JobProfitLossRequiringReasonParameters();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			JobProfitLossRequiringReasonParametersControl castedControl = (JobProfitLossRequiringReasonParametersControl)control;
			return castedControl.MarginThresholdPositiveProfitCalcEdit.ReadOnly && castedControl.JobStatusControl.ReadOnly;
		}
	}
}
