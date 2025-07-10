using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CK.Manifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDAManifest.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> ApplicableCountryCodesCore(Directions direction, ZString transportMode, string manifestStyle = null)
			=> CountryHelper.SupportedAsycudaCountryCodesList(factory).Where(x => x.ZZD_Code == Core.Constants.CountryCodes.CookIslands && x.HasAttribute(manifestStyle)).Select(x => x.ZZD_Code).ToList();

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.CookIslands };

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) => new AsycudaManifestHeaderDataObjectWriter<ASYCUDAManifest.Business.AsycudaManifestHeader>(manager);

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode) => new AsycudaManifestDataObjectReaderHelper(countryCode, factory);
	}
}
