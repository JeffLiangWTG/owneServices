using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	public class ApplicationBusinessProviderTest : ApplicationBusinessProviderBaseTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		protected override BooleanRegistryItem RegistryItem => GBCustomsDataRegistry.Instance.EnableIcsManifest;
		protected override ZString ManifestType => ICSManifestTypes.Codes.ICS;
	}
}
