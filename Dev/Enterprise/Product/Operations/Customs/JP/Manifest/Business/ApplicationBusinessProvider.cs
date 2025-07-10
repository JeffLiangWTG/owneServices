using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => [(ZString)Core.Constants.CountryCodes.Japan];

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			var result = new List<IManifestType>();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Japan)
			{
				var jpManifestTypes = new JPManifestTypes();
				if (JPRegistry.Instance.EnableHCHForwarderManifest.Value)
				{
					result.Add(jpManifestTypes.HCH);
				}
				if (JPRegistry.Instance.EnableHDFForwarderManifest.Value)
				{
					result.Add(jpManifestTypes.HDF);
				}
				if (JPRegistry.Instance.EnableNVCForwarderManifest.Value)
				{
					result.Add(jpManifestTypes.NVC);
				}
				if (JPRegistry.Instance.EnableVANForwarderManifest.Value)
				{
					result.Add(jpManifestTypes.VAN);
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

		protected override IEnumerable<IManifestType> GetApplicableManifestTypesCore(Directions direction, string transportMode)
		{
			var manifestTypes = ManifestTypes;
			return (direction, transportMode) switch
			{
				(Directions.Import, TransportTypeList.Codes.Air) => manifestTypes.Where(x => x.Code == nameof(JPManifestTypes.HCH)),
				(Directions.Export, TransportTypeList.Codes.Air) => manifestTypes.Where(x => x.Code == nameof(JPManifestTypes.HDF)),
				(Directions.Import, TransportTypeList.Codes.Sea) => manifestTypes.Where(x => x.Code == nameof(JPManifestTypes.NVC)),
				(Directions.Export, TransportTypeList.Codes.Sea) => manifestTypes.Where(x => x.Code == nameof(JPManifestTypes.VAN)),
				_ => []
			};
		}
	}
}
