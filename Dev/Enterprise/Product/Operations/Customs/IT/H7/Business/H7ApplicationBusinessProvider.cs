using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.H7.Business;

public class H7ApplicationBusinessProvider : EU.H7.Business.H7ApplicationBusinessProvider
{
	public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

	protected override Type MessageSendingObjectType => typeof(MessageSendingObject);

	protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.Italy };
}

