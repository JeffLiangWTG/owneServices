using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.Messaging.IE315;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm) { }

		public override ResourceString MenuCaption => ResString.GetMultilingualString("BBC6FBA1-5012-4FE9-801F-52AAC46836CD", "ICS Manifest");

		ZBool IsSafetyAndSecurity => isSafetyAndSecurity ?? (Header is AsycudaManifestHeaderSS);
		readonly ZBool? isSafetyAndSecurity;

		ICSEDIMessagingHelper Helper => new ICSEDIMessagingHelper((AsycudaManifestHeaderBase)Header);

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			var sendManifestText = ResString.GetMultilingualString("21C9D193-3CA9-4843-9F94-21B14C54A74D", "Send Manifest");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, sendManifestText, Header, SendManifest);

			if (IsSafetyAndSecurity)
			{
				var sendAmendmentText = ResString.GetMultilingualString("6029C3B3-A2BD-40EE-A837-3A570B14FB04", "Send Amendment");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, sendAmendmentText, Header, SendAmendment, preValidationDelegate: IsMandatoryAmendmentDataSupplied);
			}
			return menuItems.ToArray();
		}

		bool IsMandatoryAmendmentDataSupplied()
		{
			var result = true;

			if (Header is AsycudaManifestHeaderBase asycudaManifestHeader)
			{
				if (asycudaManifestHeader.RegistrationNumber.IsEmpty)
				{
					Globals.Message.Show(AmendmentWithNoMrn);
					result = false;
				}
			}

			return result;
		}

		void SendManifest()
		{
			if (Header is AsycudaManifestHeaderBase asycudaManifestHeader)
			{
				MultilingualString result = null;

				switch (asycudaManifestHeader.AMA_MessageStatus)
				{
					case ASYCUDA.Business.MessageStatusCodeList.Codes.NotSent:
					case ASYCUDA.Business.MessageStatusCodeList.Codes.Cancel:
					case ASYCUDA.Business.MessageStatusCodeList.Codes.Error:
					case ASYCUDA.Business.MessageStatusCodeList.Codes.Unknown:
					case "":
						if (Helper.SendMessageAndSave(GBMessageTypeList.Codes.New))
						{
							asycudaManifestHeader.Messages.Load();
							result = IsSafetyAndSecurity ? SuccessfulSSMessageSent : SuccessfulICSMessageSent;
						}
						break;
					case ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting:
						result = AwaitingManifest;
						break;
					default:
						result = ResString.GetMultilingualString("9BEE82DC-6E64-4F33-9584-48DCF8082A95", "Cannot send manifest from status '{0}'", asycudaManifestHeader.AMA_MessageStatus);
						break;
				}

				if (result != null)
				{
					Globals.Message.Show(result);
				}
			}
		}

		void SendAmendment()
		{
			if (Header is AsycudaManifestHeaderBase asycudaManifestHeader)
			{
				MultilingualString result = null;

				switch (asycudaManifestHeader.AMA_MessageStatus)
				{
					case ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting:
						result = AwaitingManifest;
						break;
					case ASYCUDA.Business.MessageStatusCodeList.Codes.Cancel:
						result = CancelledManifest;
						break;
					default:
						if (Helper.SendMessageAndSave(GBMessageTypeList.Codes.Amend))
						{
							asycudaManifestHeader.Messages.Load();
							result = IsSafetyAndSecurity ? SuccessfulSSMessageAmendment : SuccessfulICSMessageAmendment;
						}
						break;
				}

				if (result != null)
				{
					Globals.Message.Show(result);
				}
			}
		}

		static MultilingualString AwaitingManifest => ResString.GetMultilingualString("209F0AFD-8F11-4E99-BDB0-5C7A950E3B44", "Can’t be sent. Is waiting for a prior Customs response.");
		static MultilingualString CancelledManifest => ResString.GetMultilingualString("040FF57E-7E21-4533-8C79-FFBF082B7BC8", "Can’t be sent. The entire manifest was canceled.");
		static MultilingualString SuccessfulICSMessageSent => ResString.GetMultilingualString("5F22AAAC-EC5F-409B-82B2-D9582E5908E2", "ICS Manifest Message queued for sending.");
		static MultilingualString SuccessfulICSMessageAmendment => ResString.GetMultilingualString("B90798C6-E33F-41E3-A972-CF57D2A635A4", "Can’t be sent. ICS Amendment not implemented.");
		static MultilingualString SuccessfulSSMessageSent => ResString.GetMultilingualString("705F13C8-9813-4FC8-9DCF-01470512A1F5", "GB S&S Manifest Message queued for sending.");
		static MultilingualString SuccessfulSSMessageAmendment => ResString.GetMultilingualString("55B066F9-8742-4662-AC5B-886C453E6BF2", "GB S&S Amendment Message queued for sending.");
		static MultilingualString AmendmentWithNoMrn => ResString.GetMultilingualString("FB853B0F-712E-48B7-B2BE-7F2AC487AC68", "This manifest has no MRN and so there is nothing to amend. Do not send an amendment message.\r\n\r\nIf you are trying to resolve an initial submission failure, address the failure and then send an original message using the 'Send Manifest' option.  \r\n\r\nIf you are resolving a business rejection reported via message type 316 ('{0}'), address the failures and resend a new original message.", Enterprise.Customs.GB.ICS.CodeDescriptionPairLists.IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC316A);
	}
}
