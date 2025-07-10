using System;
using CargoWise.Types;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	public class EdiCertRequestContext
	{
		public ZString SystemIdentifier { get; set; }

		public override string ToString()
		{
			return FormattableString.Invariant($"System Identifier: {SystemIdentifier}");
		}
	}
}
