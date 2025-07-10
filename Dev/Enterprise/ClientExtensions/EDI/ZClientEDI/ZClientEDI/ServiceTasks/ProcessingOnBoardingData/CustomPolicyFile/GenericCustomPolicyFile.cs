using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	public class GenericCustomPolicyFile : CustomPolicyFileBase
	{
		public override void UpdatePolicyFileCore(XmlDocument xmlDocument, string companyCode, string systemUniqueIdentifier, string validTokenIssuerPrefix, bool isRollback)
		{
			if (xmlDocument == null)
			{
				throw new ArgumentNullException(nameof(xmlDocument), $"{nameof(xmlDocument)} must not be null");
			}

			if (!isRollback && (string.IsNullOrEmpty(systemUniqueIdentifier) || string.IsNullOrEmpty(validTokenIssuerPrefix)))
			{
				throw new InvalidOperationException("The None-AzureIDP should provide client_id and validTokenIssuerPrefix");
			}

			if (string.IsNullOrEmpty(IdpServerName))
			{
				throw new InvalidOperationException($"{nameof(IdpServerName)} must not be empty");
			}

			var inputParameterId = $"WC_{companyCode}";
			var technicalProfileId = $"{companyCode}-OpenIdConnect";
			var claimsExchangeId = $"{companyCode}AccountExchange";

			var nsManager = BuildXmlNamespaceManager(xmlDocument);

			var inputParameterToRemove = GetInputParameterXmlNode(xmlDocument, nsManager, inputParameterId);
			inputParameterToRemove?.ParentNode?.RemoveChild(inputParameterToRemove);

			var technicalProfileToRemove = GetTechnicalProfileXmlNode(xmlDocument, nsManager, technicalProfileId);
			technicalProfileToRemove?.ParentNode?.RemoveChild(technicalProfileToRemove);

			var claimsProviderSelectionToRemove = GetClaimsProviderSelectionXmlNode(xmlDocument, nsManager, claimsExchangeId); claimsProviderSelectionToRemove?.ParentNode?.RemoveChild(claimsProviderSelectionToRemove);

			var claimsExchangeToRemove = GetClaimsExchangeXmlNode(xmlDocument, nsManager, claimsExchangeId);
			claimsExchangeToRemove?.ParentNode?.RemoveChild(claimsExchangeToRemove);

			if (!isRollback)
			{
				var metaDataUrl = $"{validTokenIssuerPrefix}/.well-known/openid-configuration";

				var domainLookupClaimsTransformation = xmlDocument.SelectSingleNode("//az:BuildingBlocks/az:ClaimsTransformations/az:ClaimsTransformation[@Id='DomainLookup']", nsManager);
				if (domainLookupClaimsTransformation != null)
				{
					var inputParameters = domainLookupClaimsTransformation.SelectSingleNode("az:InputParameters", nsManager);
					if (inputParameters != null)
					{
						var inputParameter = xmlDocument.CreateElement("InputParameter", nsManager.LookupNamespace("az"));
						inputParameter.SetAttribute("Id", inputParameterId);
						inputParameter.SetAttribute("DataType", "string");
						inputParameter.SetAttribute("Value", "true");
						inputParameters.AppendChild(inputParameter);
					}
				}

				var claimsProvider = xmlDocument.SelectSingleNode($@"//az:ClaimsProviders/az:ClaimsProvider[az:DisplayName='{IdpServerName}']", nsManager);
				if (claimsProvider != null)
				{
					var technicalProfiles = claimsProvider.SelectSingleNode("az:TechnicalProfiles", nsManager);
					if (technicalProfiles != null)
					{
						var existingTechnicalProfiles = technicalProfiles.ChildNodes;
						if (existingTechnicalProfiles.Count > 0)
						{
							var technicalProfileToClone = existingTechnicalProfiles[0];
							var newTechnicalProfile = (XmlElement)technicalProfileToClone.CloneNode(true);
							newTechnicalProfile.SetAttribute("Id", technicalProfileId);

							var displayName = newTechnicalProfile.SelectSingleNode("az:DisplayName", nsManager);
							if (displayName != null)
							{
								displayName.InnerText = companyCode;
							}

							var metadata = newTechnicalProfile.SelectSingleNode("az:Metadata", nsManager);
							if (metadata != null)
							{
								var claimValueOnWhichToEnable = metadata.SelectSingleNode("az:Item[@Key='ClaimValueOnWhichToEnable']", nsManager);
								if (claimValueOnWhichToEnable != null)
								{
									claimValueOnWhichToEnable.InnerText = inputParameterId;
								}

								var validTokenIssuerPrefixes = metadata.SelectSingleNode("az:Item[@Key='ValidTokenIssuerPrefixes']", nsManager);
								if (validTokenIssuerPrefixes != null)
								{
									validTokenIssuerPrefixes.InnerText = validTokenIssuerPrefix;
								}

								var metaData = metadata.SelectSingleNode("az:Item[@Key='METADATA']", nsManager);
								if (metaData != null)
								{
									metaData.InnerText = metaDataUrl;
								}

								var clientId = metadata.SelectSingleNode("az:Item[@Key='client_id']", nsManager);
								if (clientId != null)
								{
									clientId.InnerText = systemUniqueIdentifier;
								}
							}

							var outputClaims = newTechnicalProfile.SelectSingleNode("az:OutputClaims", nsManager);
							if (outputClaims != null)
							{
								var companyCodeNode = (XmlElement)outputClaims.SelectSingleNode("az:OutputClaim[@ClaimTypeReferenceId='company_code']", nsManager);
								companyCodeNode?.SetAttribute("DefaultValue", companyCode);
							}

							technicalProfiles.AppendChild(newTechnicalProfile);
						}
					}
				}

				var claimsProviderSelections = xmlDocument.SelectSingleNode("//az:OrchestrationSteps/az:OrchestrationStep[@Order='1']/az:ClaimsProviderSelections", nsManager);
				if (claimsProviderSelections != null)
				{
					var claimsProviderSelection = xmlDocument.CreateElement("ClaimsProviderSelection", nsManager.LookupNamespace("az"));
					claimsProviderSelection.SetAttribute("TargetClaimsExchangeId", claimsExchangeId);
					claimsProviderSelections.AppendChild(claimsProviderSelection);
				}

				var claimsExchanges = xmlDocument.SelectSingleNode("//az:OrchestrationSteps/az:OrchestrationStep[@Order='2']/az:ClaimsExchanges", nsManager);
				if (claimsExchanges != null)
				{
					var claimsExchange = xmlDocument.CreateElement("ClaimsExchange", nsManager.LookupNamespace("az"));
					claimsExchange.SetAttribute("Id", claimsExchangeId);
					claimsExchange.SetAttribute("TechnicalProfileReferenceId", technicalProfileId);
					claimsExchanges.AppendChild(claimsExchange);
				}
			}

			// update the endLine of IssuerList to CRLF
			var technicalProfile = xmlDocument.SelectSingleNode("//az:TechnicalProfile[@Id='Azure-OpenIdConnect']", nsManager);
			var azureIssuerPrefixes = technicalProfile?.SelectSingleNode("az:Metadata/az:Item[@Key='ValidTokenIssuerPrefixes']", nsManager);
			if (azureIssuerPrefixes != null)
			{
				azureIssuerPrefixes.InnerText = Regex.Replace(azureIssuerPrefixes.InnerText, "(?<!\r)\n", "\r\n");
			}
		}

		static XmlNode GetInputParameterXmlNode(XmlDocument xmlDocument, XmlNamespaceManager xmlNamespaceManager, string inputParameterId)
		{
			return xmlDocument.SelectSingleNode(
				$"//az:ClaimsTransformations/az:ClaimsTransformation[@Id='DomainLookup']/az:InputParameters/az:InputParameter[@Id='{inputParameterId}']",
				xmlNamespaceManager);
		}

		static XmlNode GetTechnicalProfileXmlNode(XmlDocument xmlDocument, XmlNamespaceManager xmlNamespaceManager, string technicalProfileId)
		{
			return xmlDocument.SelectSingleNode($"//az:ClaimsProviders/az:ClaimsProvider/az:TechnicalProfiles/az:TechnicalProfile[@Id='{technicalProfileId}']", xmlNamespaceManager);
		}

		static XmlNode GetClaimsProviderSelectionXmlNode(XmlDocument xmlDocument, XmlNamespaceManager xmlNamespaceManager, string claimsExchangeId)
		{
			return xmlDocument.SelectSingleNode($"//az:OrchestrationSteps/az:OrchestrationStep[@Order='1']/az:ClaimsProviderSelections/az:ClaimsProviderSelection[@TargetClaimsExchangeId='{claimsExchangeId}']", xmlNamespaceManager);
		}

		static XmlNode GetClaimsExchangeXmlNode(XmlDocument xmlDocument, XmlNamespaceManager xmlNamespaceManager, string claimsExchangeId)
		{
			return xmlDocument.SelectSingleNode($"//az:OrchestrationSteps/az:OrchestrationStep[@Order='2']/az:ClaimsExchanges/az:ClaimsExchange[@Id='{claimsExchangeId}']", xmlNamespaceManager);
		}

		protected virtual string IdpServerName => string.Empty;
	}
}
