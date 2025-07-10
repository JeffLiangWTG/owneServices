using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.Shared
{
	partial class SharedJobMessageTypeList
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "This class is being inherited.")]
		public class MoreCodes
		{
			public const string AdvanceShippingNotice = "ASN";
		}

		public static class MoreDescriptions
		{
			public static MultilingualString AdvanceShippingNotice { get { return ResString.GetMultilingualString("JobMessageTypeList|AdvanceShippingNotice", "Advance Shipping Notice"); } }
		}
	}
}
