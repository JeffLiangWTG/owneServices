using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface
{
	[Serializable]
	public class PODErrorType : ErrorType
	{
		internal protected PODErrorType(string name, string message)
			: base(name, message)
		{
		}

		internal protected PODErrorType(string message)
			: this(message, message)
		{
		}

		public static PODErrorType ImportError { get { return new PODErrorType("ImportError", Res.GetString("63F660B8-4E5E-45F3-B81E-9382C6C78172", "Import Error")); } }
	}
}
