using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.H7.Business
{
	public sealed class H7ApplicationBusinessProvider : EU.H7.Business.H7ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(H7ManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.France };

		protected override Type MessageSendingObjectType => typeof(MessageSendingObject);

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.France;
	}
}

