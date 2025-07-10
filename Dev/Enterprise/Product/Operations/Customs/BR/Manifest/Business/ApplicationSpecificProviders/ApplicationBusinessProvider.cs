using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.BR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Brazil };

		public override MessagingProvider MessagingProvider => null;

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			var result = new List<IManifestType> { };

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Brazil)
			{
				if (BRCustomsDataRegistry.Instance.EnableUCRManifest.Value)
				{
					result.Add(new BRManifestTypes().MUCR);
				}
				if (BRCustomsDataRegistry.Instance.EnableMercanteSystem.Value)
				{
					result.Add(new BRManifestTypes().MER);
				}
			}

			return result;
		}

		protected override IAsycudaManifestHeaderDataObjectWriter
			GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) =>
			new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(
			IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) =>
			new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
	}
}
