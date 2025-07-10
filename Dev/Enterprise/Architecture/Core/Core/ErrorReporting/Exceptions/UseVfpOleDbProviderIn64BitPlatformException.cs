using System;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class UseVfpOleDbProviderIn64BitPlatformException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public UseVfpOleDbProviderIn64BitPlatformException() : base(Constants.ProductName + " 64-bit does not support Visual Foxpro OLE DB Provider. Please use " + Constants.ProductName + " 32-bit to use Visual Foxpro OLE DB Provider.") { }

#if NETFRAMEWORK
		protected UseVfpOleDbProviderIn64BitPlatformException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
