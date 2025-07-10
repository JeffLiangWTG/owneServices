using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.GB.GVMS.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.GVMS
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.UnitedKingdom };

		public override IEnumerable<(ZString, ZString)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			return new[] { ((ZString)Core.Constants.CountryCodes.UnitedKingdom, (ZString)GVMSManifestType.Descriptions.GoodsVehicleMovementSystemGvms) };
		}

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			return new GVMSManifestType().All;
		}
		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();
		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
			=> new GVMSAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			=> new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);

		protected override AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new GVMSAsycudaManifestHeaderDataObjectWriterHelper((AsycudaManifestHeader)header);
		}

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode) => new GVMSAsycudaManifestDataObjectReaderHelper(factory);
	}
}
