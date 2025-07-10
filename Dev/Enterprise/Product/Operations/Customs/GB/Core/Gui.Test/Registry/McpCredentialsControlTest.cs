using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(CredentialsControl))]
	class CredentialsControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CredentialsControl)control).CredentialsGrid.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CredentialsSettingCollection();
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}
	}
}
