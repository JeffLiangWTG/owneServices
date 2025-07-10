using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.ICS.UniversalDataTransfer;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	[TestedType(typeof(ApplicationBusinessProviderSS))]
	public class ApplicationBusinessProviderSSTest : ApplicationBusinessProviderBaseTest<ApplicationBusinessProviderSS, AsycudaManifestHeaderSS>
	{
		public void TestDescription()
		{
			var provider = new ApplicationBusinessProviderSS();
			var oneAndOnlyManifest = provider.GetManifestDescriptions(null, null, null).First();
			AssertEquals("Should be GB S&S with an ampersand", "Safety and Security Great Britain (S&S GB)", oneAndOnlyManifest.Item2);
		}

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(SASAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeaderSS>);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(SASAsycudaManifestDataObjectReaderHelper);
		protected override BooleanRegistryItem RegistryItem => GBCustomsDataRegistry.Instance.EnableSSGBManifest;
		protected override ZString ManifestType => ICSManifestTypes.Codes.SAS;
	}
}
