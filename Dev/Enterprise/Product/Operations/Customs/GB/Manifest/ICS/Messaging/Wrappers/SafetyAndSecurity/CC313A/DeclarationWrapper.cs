using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC313A;
using Enterprise.Customs.GB.ICS.Business;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC313A
{
	public class DeclarationWrapper : DeclarationWrapperSS, IDeclaration
	{
		public DeclarationWrapper(AsycudaManifestHeaderBase manifest) : base(manifest, Constants.MessageType)
		{
		}

		public static class Constants
		{
			public const string MessageType = "CC313A";
		}

		#region Header
		public IHeader Header => header ??= new HeaderWrapper(manifest, utcDateTime);
		IHeader header;
		#endregion
	}
}

