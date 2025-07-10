using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.ICS.UniversalDataTransfer;
using Enterprise.Customs.GB.Registry;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.ICS
{
	public class ApplicationBusinessProviderSS : ApplicationBusinessProviderBase
	{
		public override Type AsycudaManifestHeaderType => typeof(Business.AsycudaManifestHeaderSS);

		public override IEnumerable<(ZString, ZString)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			return new[] { ((ZString)Core.Constants.CountryCodes.UnitedKingdom, (ZString)string.Format(@"{0} Great Britain ({1})", ICSManifestTypes.Codes.SAFETYANDSECURITY, ICSManifestTypes.Codes.SASGB)) };
		}

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
			=> new SASAsycudaManifestHeaderDataObjectWriter<Business.AsycudaManifestHeaderSS>(manager);

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
			=> new SASAsycudaManifestDataObjectReaderHelper(factory);

		protected override ZString ManifestType => ICSManifestTypes.Codes.SAS;

		protected override ZBool RegistryEnabled => GBCustomsDataRegistry.Instance.EnableSSGBManifest.Value;
	}
}
