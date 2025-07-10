using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ARAPDefaultTaxRecognitionRuleControl))]
	public class ARAPDefaultTaxRecognitionRuleControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ARAPDefaultTaxRecognitionRuleCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ARAPDefaultTaxRecognitionRuleControl)control).ARAPDefaultTaxRecognitionRuleGrid.ReadOnly;
		}
	}
}
