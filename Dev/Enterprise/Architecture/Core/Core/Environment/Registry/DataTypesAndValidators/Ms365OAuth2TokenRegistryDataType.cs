using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class Ms365OAuth2TokenRegistryDataType : RegistryDataTypeWithJsonSerializer<Ms365OAuth2Token>
	{
		public Ms365OAuth2TokenRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, new Ms365OAuth2Token())
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new Ms365OAuth2TokenRegistryEditorInfo();
		}

		protected override Ms365OAuth2Token CloneValue(Ms365OAuth2Token value)
		{
			Ms365OAuth2Token clonedToken = null;
			if (value != null)
			{
				clonedToken = new Ms365OAuth2Token
				{
					Identifier = value.Identifier,
					Token = value.Token,
					User = value.User
				};
			}

			return clonedToken;
		}
	}
}
