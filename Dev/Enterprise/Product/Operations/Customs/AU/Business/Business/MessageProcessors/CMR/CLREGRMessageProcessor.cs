using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CLREGRMessageProcessor : CMRMessageResponseProcessor
	{
		public CLREGRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.CLREG, "Client Registration Response (CLREGR)")
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			var message = incomingMessage as CMRCLREGRMessage;
			if (message == null)
			{
				return false;
			}

			var linkedOrganization = (OrgHeader)message.EM_LinkedObject;

			if (linkedOrganization != null)
			{
				var orgheaderWrapper = new OrgHeaderWrapper(linkedOrganization);

				if (orgheaderWrapper.CLREGInfoProvider.ZA_ABN.IsEmpty)
				{
					ZString customsClientID = cUSRES.FTX[0].TextReference.FreeTextValueCode;

					if (!customsClientID.IsEmpty)
					{
						var existingCidCodeWithOrWithoutPremise = linkedOrganization.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
						if (existingCidCodeWithOrWithoutPremise == null)
						{
							var cidCodeWithoutPremise = linkedOrganization.CustomsCodes.AddNew();
							cidCodeWithoutPremise.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
							cidCodeWithoutPremise.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							cidCodeWithoutPremise.OK_CustomsRegNo = customsClientID;
						}
						else
						{
							var matchedAddress = FindMatchedAddressFromIncomingMessage(message, orgheaderWrapper.Messages);
							if (matchedAddress != null)
							{
								var cidCodeForMatchedAddress = linkedOrganization.CustomsCodes.GetOrgCusCodeForPremiseAddress(OrgCusCode.CodeTypes.CustomsClientID, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, matchedAddress.PK);
								if (cidCodeForMatchedAddress == null)
								{
									cidCodeForMatchedAddress = linkedOrganization.CustomsCodes.AddNew();
									cidCodeForMatchedAddress.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
									cidCodeForMatchedAddress.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
									cidCodeForMatchedAddress.OK_OA_PremisesAddress = matchedAddress.PK;
								}
								cidCodeForMatchedAddress.OK_CustomsRegNo = customsClientID;
							}
						}
					}
				}

				return true;
			}
			else
			{
				Logger.LogWarning("The incoming message is not responding to an organization. Can't continue.");
				return false;
			}
		}

		OrgAddress FindMatchedAddressFromIncomingMessage(CMRCLREGRMessage incomingMessage, EDIMessageCollection messages)
		{
			OrgAddress result = null;
			var linkedOrganization = (OrgHeader)incomingMessage.EM_LinkedObject;
			var outgoingMessage = incomingMessage.GetOutgoingMessageByBGMRefAndVersion(messages, CMRMessage.CMRMessageTypes.CLREG) as CMRCLREGMessage;
			if (outgoingMessage != null)
			{
				var outgoingMessageText = outgoingMessage.EM_FormattedMessageText;
				foreach (OrgAddress address in linkedOrganization.Addresses)
				{
					var bsn1 = CLREGInfoProvider.GetBsn1FromOrgAddress(address);
					var bsn2 = CLREGInfoProvider.GetBsn2FromOrgAddress(address);
					var bsnCity = CLREGInfoProvider.GetBsnCityFromOrgAddress(address);
					var bsnPostCode = CLREGInfoProvider.GetBsnPostCodeFromOrgAddress(address);
					var addressString = $"FTX+ATY+BA++{bsn1}:{bsn2}:{bsnCity}:{bsnPostCode}";
					if (outgoingMessageText.Contains(addressString, StringComparison.InvariantCultureIgnoreCase))
					{
						result = address;
						break;
					}
				}
			}

			return result;
		}
	}
}
