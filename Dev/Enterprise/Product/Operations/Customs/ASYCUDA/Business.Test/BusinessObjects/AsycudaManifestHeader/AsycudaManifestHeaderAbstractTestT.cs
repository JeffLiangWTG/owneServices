using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestsSubclassesOf(typeof(AsycudaManifestHeader))]
	public abstract class AsycudaManifestHeaderAbstractTest : ManifestBase.Testing.AsycudaManifestHeaderTest
	{
		public void TestAMA_NatureChangeAfterManifestChange()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var appbusprovider = header.ApplicationBusinessProvider;

			if (appbusprovider != null)
			{
				var manifestTypes = appbusprovider.ManifestTypes;
				foreach (var manifestType in manifestTypes)
				{
					header.AMA_ManifestType = ZString.Empty;
					header.AMA_Nature = ZString.Empty;
					header.AMA_ManifestType = manifestType.Code;
					if (manifestType.ManifestNatures != null)
					{
						if (manifestType.ManifestNatures.Count == 1)
						{
							AssertEquals(manifestType.ManifestNatures[0].Code, header.AMA_Nature);
						}
						else
						{
							AssertEquals(ZString.Empty, header.AMA_Nature);
						}
					}
				}
			}
			Assert(true);
		}

		public void TestAMA_NatureChangeAfterTransportModeChange()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var appbusprovider = header.ApplicationBusinessProvider;

			if (appbusprovider != null)
			{
				var manifestTypes = appbusprovider.ManifestTypes;

				if (manifestTypes.Count != 0)
				{
					var manifestType = manifestTypes[0];
					if (manifestType.ManifestNatures != null)
					{
						header.AMA_ManifestType = manifestType.Code;
						header.AMA_TransportMode = ZString.Empty;
						header.AMA_Nature = ZString.Empty;
						header.AMA_TransportMode = manifestType.ApplicableTransportModes.FirstOrDefault();

						if (manifestType.ManifestNatures.Count == 1 && (header.FeatureProvider?.AllowDefaultingOfNature ?? true))
						{
							AssertEquals(manifestType.ManifestNatures[0].Code, header.AMA_Nature);
						}
						else
						{
							AssertEquals(ZString.Empty, header.AMA_Nature);
						}
					}
				}
			}
			Assert(true);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
