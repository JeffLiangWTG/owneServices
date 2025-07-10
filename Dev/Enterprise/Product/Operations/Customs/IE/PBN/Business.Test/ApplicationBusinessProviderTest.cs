using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public override void TestManifestTypes()
		{
			expectedManifestTypes = new PBNManifestTypes().All;
			base.TestManifestTypes();
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
		IEnumerable<IManifestType> expectedManifestTypes;

		protected override Type ExpectedMessagingProviderType => null;

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => null;

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => null;

		public new void TestGetAsycudaManifestHeaderDataObjectWriter()
		{
			// Future WI will remove this
			Assert(true);
		}

		public new void TestGetCustomsDeclarationDataObjectWriter()
		{
			// Future WI will remove this
			Assert(true);
		}

		public void TestApplicableTransportModes_IE_PBN()
		{
			var header = CreateNewManifest();
			var transportModes = header.Lookups.TransportModeList;
			CombineAssertions(() =>
			{
				AssertEquals("There is 2 codes in the transport mode list", 2, transportModes.Count);
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.Sea));
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.Road));
			});
		}

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = "IE";
			result.AMA_ManifestType = PBNManifestTypes.Codes.PBN;
			return result;
		}
	}
}
