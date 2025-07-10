using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(NGTRegistryItemControl))]
	sealed class NGTRegistryItemControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new NGT
			{
				Password = "1"
			};
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((NGTRegistryItemControl)control).CurrentDataItem?.ReadOnly ?? false;
		}
	}
}
