using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.MCP.MessageBuilders
{
	public class UCNMessageBuilder
	{
		public UCNMessageBuilder(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public (ClaimUcnEDIMessage Message, string ResultText) Build()
		{
			var ediMessage = declaration.Factory.New<ClaimUcnEDIMessage>();
			ediMessage.EM_MessageSubType = Constants.EDIMessageSubTypes.SendIslMessage;
			ediMessage.EM_MessageOwner = declaration.JE_CustomsProfile.SubstringSafe(0, EDIMessage.Schema.EM_MessageOwnerMaxLength);
			ediMessage.EM_GB = declaration.JE_GB;
			var success = true;
			var resultText = MessageCreationSuccess;
			var containerCount = declaration.CusContainers.Count;
			var credentialsFound = CheckAvailableCredentials(ediMessage.EM_MessageOwner);
			if (!credentialsFound)
			{
				success = false;
				resultText = ZString.Format(CredentialsNotFoundError, declaration.JE_CustomsProfile);
			}
			if (credentialsFound && containerCount > 0)
			{
				ediMessage.EM_ApplicationReference = declaration.CusContainers[0].CO_ContainerNumber;
				if (containerCount == 1)
				{
					var container = declaration.CusContainers[0];
					var mode = declaration.JE_ContainerMode;
					if (mode != Core.Constants.ContainerModes.FCL && mode != Core.Constants.ContainerModes.LCL)
					{
						mode = container.CO_FCL_LCL_AIR;
					}
					if (mode == Core.Constants.ContainerModes.FCL)
					{
						ediMessage.EM_MessageText = $":CSN~{container.CO_ContainerNumber}~~~~~~}}";
						container.CO_MessageStatus = ContainerStatusCodesList.Codes.ContainerClaimMadeFclMode;
					}
					else
					if (mode == Core.Constants.ContainerModes.LCL)
					{
						ediMessage.EM_MessageText = $":CSN~{container.CO_ContainerNumber}~Y~{container.CO_Calc_TotalPackages}~{container.CO_Weight}~~~}}";
						container.CO_MessageStatus = ContainerStatusCodesList.Codes.ContainerClaimMadeLclMode;
					}
					else
					{
						success = false;
						resultText = FunctionNotSupportedError;
					}
				}
				else
				{
					ediMessage.EM_MessageText = $":CSN~~~~~{declaration.JE_MasterBill}~Y~}}";
					foreach (var container in declaration.CusContainers)
					{
						container.CO_MessageStatus = ContainerStatusCodesList.Codes.ContainerClaimMadeAmalgamateMode;
					}
				}
			}
			if (success)
			{
				declaration.Messages.Add(ediMessage);
			}
			else
			{
				ediMessage.Delete();
			}

			return (ediMessage, resultText);
		}

		bool CheckAvailableCredentials(ZString badge)
		{
			using (DisposableEnvironment.ForBranch(declaration.JE_GB.ToGuid()))
			{
				var validCredential = GBCustomsDataRegistry.Instance.McpIslWebServiceCredentialsSet.GetValueWithoutFallback(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty)
					.Cast<McpIslCredentialsSetting>().FirstOrDefault(x => x.McpIslCompanyCode == badge);
				return validCredential != null;
			}
		}

		const string CredentialsNotFoundError = "No MCP ISL credential was found for company {0} for this declaration’s branch. Please ensure a credential record is supplied in the registry using the correct branch and company (badge) code.";
		const string FunctionNotSupportedError = "This function is only supported for FCL and LCL modes";
		const string MessageCreationSuccess = "CSN message created successfully";
	}
}
