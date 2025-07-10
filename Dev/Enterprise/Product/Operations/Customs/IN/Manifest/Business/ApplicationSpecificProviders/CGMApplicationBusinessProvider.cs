using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMApplicationBusinessProvider : ApplicationBusinessProvider
{
	public override Type AsycudaManifestHeaderType => typeof(CGMAsycudaManifestHeader);

	public override MessagingProvider MessagingProvider => null;

	public override FeatureProvider FeatureProvider => new CGMFeatureProvider();

	protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.India };

	protected override IReadOnlyList<IManifestType> CreateManifestTypes()
	{
		var result = new List<IManifestType>();
		result.Add(new INManifestTypes().CGM);
		return result;
	}

	protected override IAsycudaManifestHeaderDataObjectWriter
		GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) =>
		new AsycudaManifestHeaderDataObjectWriter<CGMAsycudaManifestHeader>(manager);

	protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(
		IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) =>
		new AsycudaForCustomsDeclarationDataObjectWriter<CGMAsycudaBill, CGMAsycudaPack, AsycudaPackedItem>(manager, helper);
}
