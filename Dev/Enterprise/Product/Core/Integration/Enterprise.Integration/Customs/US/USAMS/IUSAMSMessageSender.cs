using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class USAMS
			{
				public interface IUSAMSMessageSender
				{
					Tuple<int, string> SendManifest(ZGuid headerPK, IEnumerable<USAMSManifestBillAmendment> amendments = null, bool canSendMessageErrorsAsWarnings = true);
				}
			}
		}
	}
}
