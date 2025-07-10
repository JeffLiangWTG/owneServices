using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobProfitLossReasonCodeControl))]
	class JobProfitLossReasonCodeControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new JobProfitLossReasonCodeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((JobProfitLossReasonCodeControl)control).JobProfitLossReasonCodeGrid_ForTestOnly.ReadOnly;
		}
	}
}
