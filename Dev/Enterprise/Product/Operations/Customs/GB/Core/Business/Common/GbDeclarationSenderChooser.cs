using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Business
{
	public class GbDeclarationSenderChooser : IDeclarationMessageSender
	{
		public void Send(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendToCustoms, CusdecMessageFunction how)
		{
			Argument.NotNull(declaration, "declaration");
			IDeclarationMessageSender sender = null;

			BadgeCodeSetting badgeSetting = GetBadgeSetting(declaration, sendToCustoms);
			if (badgeSetting == null)
			{
				return; // error message already shown to user
			}
			if (badgeSetting.CSPCode == GatewayList.Codes.NES)
			{
				var shouldStillSend = CheckAlertsForEDCS(sendToCustoms);
				if (!shouldStillSend)
				{
					return;
				}
			}
			sender = GetSpecificCspSenderFromBadgeIfRegistryIsSufficientlyConfigured(badgeSetting, sendToCustoms);
			if (sender == null)
			{
				sendToCustoms.NotifyUserOfAnInvalidOperation(RegistryOptionsInsufficientlyConfiguredErrorMessage);
				return;
			}

			DoFinalSend(declaration, sendToCustoms, how, sender);
		}

		bool CheckAlertsForEDCS(ISendsMessagesToCustoms sendToCustoms)
		{
			var alertDate = GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.Value;
			if (alertDate > DateTime.MinValue && alertDate != ZDateTime.Empty && alertDate > ZDateTime.Now.AddHours(-6))
			{
				var alertString = GBCustomsDataRegistry.Instance.EdcsWtgAlertString.Value + "\r\n\r\n Send anyway?";
				return sendToCustoms.YesNoQuery(alertString, "EDCS has known problems");
			}
			return true;
		}

		protected virtual void DoFinalSend(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendToCustoms, CusdecMessageFunction how, IDeclarationMessageSender sender)
		{
			sender.Send(declaration, sendToCustoms, how);
		}

		BadgeCodeSetting GetBadgeSetting(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendToCustoms)
		{
			JobDeclaration euDec = (JobDeclaration)declaration;

			// Service task may be processing a received badge in the context of any branch
			BadgeCodeSetting badgeSetting = GBCustomsDataRegistry.Instance.BadgeCodes.Value.FindByBadgeCode(euDec.JE_CustomsProfile, euDec.JE_MessageType) ?? GBCustomsDataRegistry.Instance.BadgeCodes.GetValueWithoutFallback(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty).FindByBadgeCode(euDec.JE_CustomsProfile, euDec.JE_MessageType);
			if (badgeSetting == null || euDec.JE_CustomsProfile.IsEmpty)
			{
				sendToCustoms.NotifyUserOfAnInvalidOperation("Badge codes need to be supplied in the registry and then one selected on the declaration. \nWithout badge codes, the CSP cannot be determined. Ensure that the badges in the registry are set for the correct direction (IMP/EXP).");
				return null;
			}
			return badgeSetting;
		}

		IDeclarationMessageSender GetSpecificCspSenderFromBadgeIfRegistryIsSufficientlyConfigured(BadgeCodeSetting badgeSetting, ISendsMessagesToCustoms sendToCustoms)
		{
			IDeclarationMessageSender sender = null;
			Dictionary<string, IDeclarationMessageSender> dictionaryOfAvaiableSenders = GetListOfAvailableGbDeclarationSendersFromSpring();
			pentantSender = dictionaryOfAvaiableSenders["PENTANT"];
			mcpSender = dictionaryOfAvaiableSenders["MCP"];
			nesSender = dictionaryOfAvaiableSenders["NES"];
			cnsSender = dictionaryOfAvaiableSenders["CNS"];
			ccsukSender = dictionaryOfAvaiableSenders["CCSUK"];

			switch (badgeSetting.CSPCode)
			{
				case Enterprise.Customs.GB.Registry.GatewayList.Codes.CNS_CUSDECOnly:
					sender = CnsRegistrySufficientlyConfiguredForDirectMessaging ? cnsSender : null;
					break;

				case Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW:
					sender = CcsukRegistrySufficientlyConfiguredForDirectMessaging ? ccsukSender : null;
					break;

				case Enterprise.Customs.GB.Registry.GatewayList.Codes.MCP_CUSDECOnly:
					sender = McpRegistrySufficientlyConfiguredForDirectMessaging ? mcpSender : null;
					break;

				case Enterprise.Customs.GB.Registry.GatewayList.Codes.NES:
					sender = nesSender; // No need to choose, cos there is no URL
					break;

				case Enterprise.Customs.GB.Registry.GatewayList.Codes.Pentant:
					sender = pentantSender;
					break;

				default:
					sendToCustoms.NotifyUserOfAnInvalidOperation(RegistryOptionsInsufficientlyConfiguredErrorMessage);
					break;
			}
			return sender;
		}

		const string RegistryOptionsInsufficientlyConfiguredErrorMessage = "The registry options are not sufficiently configured for this CSP. Cannot send.";

		static Dictionary<string, IDeclarationMessageSender> GetListOfAvailableGbDeclarationSendersFromSpring()
		{
			Dictionary<string, IDeclarationMessageSender> lookupList = new Dictionary<string, IDeclarationMessageSender>();

			var compileTimeCheck = ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooserOptions>();  // this line is to ensure that the below line is correct
			Hashtable dictionary = (Hashtable)ObjectFactory.Get("GB.IDeclarationMessageSenderChooserOptions");
			foreach (DictionaryEntry entry in dictionary)
			{
				string key = (string)entry.Key;
				ObjectHandle valueHandle = (ObjectHandle)entry.Value;
				IDeclarationMessageSender value = (IDeclarationMessageSender)valueHandle.GetObject();
				lookupList.Add(key, value);
			}
			return lookupList;
		}

		internal bool McpRegistrySufficientlyConfiguredForDirectMessaging
		{
			get
			{
				ZString url = GB.Registry.GBCustomsDataRegistry.Instance.McpDestin8Url;
				return !url.IsEmpty;
			}
		}

		internal bool CnsRegistrySufficientlyConfiguredForDirectMessaging
		{
			get
			{
				ZString url = GB.Registry.GBCustomsDataRegistry.Instance.CnsUploadUrl;
				return !url.IsEmpty;
			}
		}

		internal bool CcsukRegistrySufficientlyConfiguredForDirectMessaging
		{
			get
			{
				return !((ZString)GB.Registry.GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.Value).IsEmpty;
			}
		}

		public string Badge { get; private set; }

		IDeclarationMessageSender pentantSender;
		IDeclarationMessageSender mcpSender;
		IDeclarationMessageSender nesSender;
		IDeclarationMessageSender cnsSender;
		IDeclarationMessageSender ccsukSender;
	}
}
