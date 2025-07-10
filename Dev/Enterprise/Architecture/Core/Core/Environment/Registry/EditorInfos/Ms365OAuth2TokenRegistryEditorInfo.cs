using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class Ms365OAuth2TokenRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get => typeof(Ms365OAuth2TokenRegistryDataType);
		}
	}

	[Serializable]
	public class Ms365OAuth2Token
	{
		public string Identifier { get; set; }
		public string User { get; set; }
		public byte[] Token { get; set; }
	}
}
