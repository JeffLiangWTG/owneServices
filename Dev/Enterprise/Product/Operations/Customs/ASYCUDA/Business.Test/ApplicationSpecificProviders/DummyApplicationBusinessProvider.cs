using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class DummyApplicationBusinessProvider : ApplicationBusinessProvider
	{
		public DummyApplicationBusinessProvider(string manifestType, params string[] country)
		{
			supportedManifestType = manifestType;
			supportedCountry = country;
		}

		public ZGuid BizoPKPassed;

		public override bool SupportsAutoSendGlobalManifest(string manifestType) => true;

		public override IProcessor GetSendGlobalManifestProcessor(AsycudaManifestHeader header)
		{
			BizoPKPassed = header.PK;
			return new Mock<IProcessor>().Object;
		}

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			if (supportedManifestType == null)
			{
				return Array.Empty<IManifestType>();
			}
			return new[]
			{
					new ManifestType(
						supportedManifestType,
						"description",
						["AIR", "SEA"],
						applicationCode != null
							? [applicationCode]
							: [ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine],
						MessageLevel.Manifest
					)
				};
		}

		public override string ApplicationCode => applicationCode ?? base.ApplicationCode;
		string applicationCode;

		public DummyApplicationBusinessProvider WithApplicationCode(string applicationCode)
		{
			this.applicationCode = applicationCode;
			return this;
		}

		protected override IReadOnlyList<ZString> CreateCountryCodes() => supportedCountry.Select(c => new ZString(c)).ToList();

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) => new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) => new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		public override Type AsycudaManifestHeaderType => typeof(DummyAsycudaManifestHeader);

		public override MessagingProvider MessagingProvider => null;

		public override FeatureProvider FeatureProvider => new FeatureProviderForTest();

		readonly string supportedManifestType;
		readonly string[] supportedCountry;
	}
}
