using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Registry.Testing
{
	[TestedType(typeof(FallbackControl))]
	public class FallbackControlTest : RegistryZUserControlTestCase
	{
		public FallbackControlTest()
		{
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((FallbackControl)control).ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new FallbackSettings();
		}
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.France; }
		}
	}
}
