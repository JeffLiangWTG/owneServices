using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks
{
	public class EMCSRequestDataProvider : GBCustomsRequestDataProviderBase
	{
		public EMCSRequestDataProvider(EMCSJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public ZString ServiceReference => declaration.Messages.NumberOfOutgoingMessages > 1 ? EMCSHelper.GetApplicationReferenceFromLastOutgoingMessage(declaration.Messages) : ZString.Empty;

		protected override ZString GatewayCore => "EMCS";

		protected override ZString JobNumberCore => declaration.JE_DeclarationReference;

		protected override ZString GetCredentialsKey() => null;

		protected override CredentialsSetting GetCredentialsSetting() => null;

		public new Credentials Credentials
		{
			get
			{
				return new Credentials
				{
					Key = declaration.JE_CustomsProfile
				};
			}
		}

		readonly EMCSJobDeclaration declaration;
	}
}
