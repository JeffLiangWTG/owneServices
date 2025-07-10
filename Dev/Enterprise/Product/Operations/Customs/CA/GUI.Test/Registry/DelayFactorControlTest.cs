using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(DelayFactorControl))]
	sealed class DelayFactorControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
			=> new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 7, DelayIntervalTypeCodes.Codes.Default);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			var bizo = control.CurrentDataItem as DelayFactorRegistryBusinessObject;
			if (bizo != null)
			{
				result = bizo.ReadOnly;
			}
			return result;
		}
	}
}
