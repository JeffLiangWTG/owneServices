using System;
using System.Linq;
using System.Xml;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	public class AzureCustomPolicyFile : CustomPolicyFileBase
	{
		public override void UpdatePolicyFileCore(XmlDocument xmlDocument, string companyCode, string systemUniqueIdentifier, string validTokenIssuerPrefix, bool isRollback)
		{
			if (xmlDocument == null)
			{
				throw new ArgumentNullException(nameof(xmlDocument));
			}

			var xmlNamespaceManager = BuildXmlNamespaceManager(xmlDocument);

			var clientIdToCompanyCode = xmlDocument.SelectSingleNode("//az:ClaimsTransformation[@Id='ClientIdToCompanyCode']", xmlNamespaceManager);
			var inputParameters = clientIdToCompanyCode?.SelectSingleNode("az:InputParameters", xmlNamespaceManager);
			var tenantNode = inputParameters?.SelectSingleNode($"az:InputParameter[@Id='{systemUniqueIdentifier}']", xmlNamespaceManager);
			if (tenantNode != null)
			{
				if (isRollback)
				{
					inputParameters.RemoveChild(tenantNode);
				}
				else
				{
					if (tenantNode.Attributes != null)
					{
						tenantNode.Attributes["Value"].Value = companyCode;
					}
				}
			}
			else if (!isRollback)
			{
				var inputParameter = xmlDocument.CreateElement("InputParameter", xmlNamespaceManager.LookupNamespace("az"));
				inputParameter.SetAttribute("Id", systemUniqueIdentifier);
				inputParameter.SetAttribute("DataType", "string");
				inputParameter.SetAttribute("Value", companyCode);
				inputParameters?.AppendChild(inputParameter);
			}

			var technicalProfile = xmlDocument.SelectSingleNode("//az:TechnicalProfile[@Id='Azure-OpenIdConnect']", xmlNamespaceManager);
			var validTokenIssuerPrefixes = technicalProfile?.SelectSingleNode("az:Metadata/az:Item[@Key='ValidTokenIssuerPrefixes']", xmlNamespaceManager);
			var originalTenants = validTokenIssuerPrefixes?.InnerText;
			var issueList = originalTenants?.Trim().Split(',').Select(i => i.Trim()).ToList();
			if (originalTenants != null && originalTenants.Contains(systemUniqueIdentifier))
			{
				if (isRollback)
				{
					issueList = issueList.Where(issuer => !issuer.Contains(systemUniqueIdentifier)).ToList();
				}
			}
			else if (!isRollback)
			{
				issueList?.Add($"https://login.microsoftonline.com/{systemUniqueIdentifier}");
			}

			const int indentSize = 2;
			var whitespacesForItemContent = new string(' ', indentSize * 7);
			var whitespacesForItemElement = new string(' ', indentSize * 6);

			var content = string.Join($",\r\n{whitespacesForItemContent}", issueList);

			if (validTokenIssuerPrefixes != null)
			{
				validTokenIssuerPrefixes.InnerText = $@"
{whitespacesForItemContent}{content}
{whitespacesForItemElement}";
			}
		}
	}
}
