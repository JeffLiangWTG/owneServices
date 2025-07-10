using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Common
{
	public class DefaultBrokerAndCredentialValidation : ZValidation
	{
		public DefaultBrokerAndCredentialValidation(DefaultBrokerAndCredential parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DefaultBrokerAndCredential parent;

		public override void ValidateAll()
		{
			ValidateDefaultBrokerCode();
			ValidateDefaultCredentialAIR();
			ValidateDefaultCredentialSEA();
		}

		public override Type AutoValidationType => typeof(DefaultBrokerAndCredentialValidation);

		public void ValidateDefaultBrokerCode()
		{
			ValidateCalculatedProperty(parent.DefaultBrokerCodeInfo);
		}

		protected void CheckDefaultBrokerCode()
		{
			var targetInfo = parent.DefaultBrokerCodeInfo;
			if (targetInfo.Value.IsEmpty)
			{
				targetInfo.AddError(Res.GetString("378F566F-4DBF-42FA-A07C-2E8A4F1FCCE1", "Default Broker is required."));
			}
			if (!string.IsNullOrEmpty(parent.DefaultBrokerCode) && GlbStaffWrapper.Get(parent.DefaultBroker)?.PasswordCollection.Count < 1)
			{
				targetInfo.AddMessageError(Res.GetString("94979B40-63A6-4B7B-AC6F-0A6DBAD1A921", "The Default Broker you have selected does not have a valid credential. To add a valid credential, press F3 to visit the Staff screen, navigate to the Credentials tab, and add a CUS – Customs code."));
			}
		}

		public void ValidateDefaultCredentialSEA()
		{
			ValidateCalculatedProperty(parent.DefaultCredentialSEAInfo);
		}

		protected void CheckDefaultCredentialSEA()
		{
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("CD76F8C0-FE05-491C-BDFF-59EAFFE3CD5A", "Select a valid credential from the list."), parent.DefaultCredentialSEAInfo);
		}

		public void ValidateDefaultCredentialAIR()
		{
			ValidateCalculatedProperty(parent.DefaultCredentialAIRInfo);
		}

		protected void CheckDefaultCredentialAIR()
		{
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("5DCFBB9B-6063-475E-B55B-0F7D39D1709C", "Select a valid credential from the list."), parent.DefaultCredentialAIRInfo);
		}
	}
}
