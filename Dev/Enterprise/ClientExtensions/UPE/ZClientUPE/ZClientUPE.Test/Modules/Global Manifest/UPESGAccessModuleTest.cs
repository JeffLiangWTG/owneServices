using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPESGAccessModule))]
	public class UPESGAccessModuleTest : Customs.SG.Access.GUI.Testing.ManifestModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
