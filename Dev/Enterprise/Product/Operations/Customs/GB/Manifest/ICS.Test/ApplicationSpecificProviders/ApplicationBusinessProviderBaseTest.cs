using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public abstract class ApplicationBusinessProviderBaseTest<T, THeader> : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<T, THeader>
		where T : ApplicationBusinessProviderBase
		where THeader : AsycudaManifestHeaderBase
	{
		public override void TestManifestTypes()
		{
			using (RegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				expectedManifestTypes = new ICSManifestTypes().All.Where(x => x.Code == ManifestType);
				base.TestManifestTypes();
			}

			using (RegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				expectedManifestTypes = Array.Empty<IManifestType>();
				base.TestManifestTypes();
			}
		}

		protected abstract BooleanRegistryItem RegistryItem { get; }
		protected abstract ZString ManifestType { get; }

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
		IEnumerable<IManifestType> expectedManifestTypes;
		protected override Type ExpectedMessagingProviderType => typeof(ICSMessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<EU.Manifest.Business.AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<EU.Manifest.Business.AsycudaBill, EU.Manifest.Business.AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);

		protected override THeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			result.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			result.AMA_ManifestType = ManifestType;
			return result;
		}
	}
}
