using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC315A;
using AsycudaManifestHeaderBase = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderBase;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC315A
{
	public class DeclarationWrapper : DeclarationWrapperSS, IDeclaration
	{
		public DeclarationWrapper(AsycudaManifestHeaderBase manifest) : base(manifest, Constants.MessageType)
		{
		}

		public static class Constants
		{
			public const string MessageType = "CC315A";
		}

		#region Header
		public IHeader Header => header ??= new HeaderWrapper(manifest, utcDateTime);
		IHeader header;
		#endregion
	}
}

