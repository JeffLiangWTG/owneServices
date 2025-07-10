using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IRefCarrierConfig
			{
				IEnumerable<ZString> MandatoryAttributes { get; }
				ZBool IsSetDefaultCarrierType { get; }
			}
		}
	}
}
