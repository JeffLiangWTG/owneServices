using System;
using CargoWise.BrandManager;

namespace Enterprise.Faxing.Integration
{
	[Serializable]
	public class FaxRouterException32Bit : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "The string literal is safe to use in this context and does not need to be externalized.")]
		public FaxRouterException32Bit() : base(BrandingFactory.Instance.ProductName + " 64-bit does not support Fax Routing please switch to " + BrandingFactory.Instance.ProductName + " 32-bit") { } // SupressCodeSmell Reason = exception only shown to some users
#if NETFRAMEWORK
		protected FaxRouterException32Bit(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
