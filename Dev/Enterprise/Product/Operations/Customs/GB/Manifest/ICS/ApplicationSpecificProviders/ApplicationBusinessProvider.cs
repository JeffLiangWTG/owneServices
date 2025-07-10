using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.ICS
{
	public class ApplicationBusinessProvider : ApplicationBusinessProviderBase
	{
		public override Type AsycudaManifestHeaderType => typeof(Business.AsycudaManifestHeader);

		public override IEnumerable<(ZString, ZString)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			return new[] { ((ZString)Core.Constants.CountryCodes.UnitedKingdom, (ZString)"Northern Ireland ICS") };
		}

		protected override ZString ManifestType => ICSManifestTypes.Codes.ICS;

		protected override ZBool RegistryEnabled => GBCustomsDataRegistry.Instance.EnableIcsManifest.Value;
	}
}
