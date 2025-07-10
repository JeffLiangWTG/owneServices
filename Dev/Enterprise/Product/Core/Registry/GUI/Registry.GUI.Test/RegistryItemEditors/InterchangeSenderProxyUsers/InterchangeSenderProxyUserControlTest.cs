using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(InterchangeSenderProxyUserControl))]
	sealed class InterchangeSenderProxyUserControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new InterchangeSenderProxyUserControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((InterchangeSenderProxyUserControl)control).ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new InterchangeSenderProxyUserCollection();
			collection.AddNew();
			return collection;
		}
	}
}
