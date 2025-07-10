using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NctsMessageStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMessageStatusList;
using NctsTransitStatusList = Enterprise.Customs.EU.NCTS.Business.NctsTransitStatusList;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class ESNctsArrivalMovementMessagingMenuProvider : NctsArrivalMovementMessagingMenuProvider
	{
		public ESNctsArrivalMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsMovementForm nctsMovementForm)
			: base(header)
		{
			ParentForm = nctsMovementForm;
		}

		public ESNctsArrivalMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, EU.NCTS.DataTransfer.NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		protected override void SendMessageToNctsCore(NctsMessageFunctionSet messageFunction)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(messageFunction, (NctsHeader)Header, ParentForm);
			commonMenuProvider.ESSendMessageToNcts();
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			return base.CreateMenuItems().Concat(new[] { downloadTADMenuItem = new ZMenuItem(Constants.MenuProviderCaptions.DownloadTAD, DownloadTADClick),
				sendCombinedArrivalAndDepartureMenuItem = new ZMenuItem(ResString.GetMultilingualString("C461B342-D5E0-49A0-AE06-DA9A171466F1", "Send &Combined Arrival and Departure"), SendArrivalAndDepartureMessageClick),
				viewOnCustomsWebsite = new ZMenuItem(Constants.MenuProviderCaptions.ViewOnCustomsWebsite, ViewOnCustomsWebsiteClick),
				createEXSDeclaration = new ZMenuItem(Constants.MenuProviderCaptions.CreateEXSDeclaration, CreateEXSDeclarationClick) });
		}
		ZMenuItem downloadTADMenuItem;
		ZMenuItem sendCombinedArrivalAndDepartureMenuItem;
		ZMenuItem viewOnCustomsWebsite;
		ZMenuItem createEXSDeclaration;

		void SendArrivalAndDepartureMessageClick(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(
				delegate
				{
					if (Header.IsArrivalTabReadOnly)
					{
						if (Globals.Message.ShowConfirmation(
							Res.GetString("DF18B093-5B75-4229-B6FD-274EEF491510", "Are you sure you want to resend this message?"),
							Res.GetString("80C22EF3-F1D5-462C-A97E-C004AB911A68", "Resend Message"),
							Res.GetString("060591FB-A2F8-4DB9-9CB3-D1F2340B12EC", "yes"),
							MessageBoxIcon.Question) != DialogResult.OK)
						{
							return;
						}
					}
					var userResult = DialogResult.OK;
					var matchingDepartureResults = Header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
					if (matchingDepartureResults.result == NctsHeader.DepartureRecordFindResult.FoundByMatchingMrn)
					{
						userResult = Globals.Message.ShowConfirmation(
							Res.GetString("18C18611-4B67-4FF3-85ED-529AB3E5F5F9", "This arrival does not have an attached departure, but {0} was found with a matching MRN. Use this departure’s data for TNN?", matchingDepartureResults.departureHeaderFound.BH_JobReference),
							Res.GetString("2337B9C2-32FB-4762-8D68-0C2FFE2209CD", "Use this departure MRN"),
							Res.GetString("7F801413-3FBB-43AB-8A0C-0C005537E20D", "yes"),
							MessageBoxIcon.Question);
					}
					else if (matchingDepartureResults.result == NctsHeader.DepartureRecordFindResult.NothingFound)
					{
						userResult = Globals.Message.ShowConfirmation(
							Res.GetString("152A4859-3AEB-4373-A011-6D4D7191035D", $"This arrival does not have an attached departure, and no other departure record was found with a matching MRN. Generate new departure for TNN?"),
							Res.GetString("116AB9F3-49E2-47B4-A141-8394CFCEDEE4", "Generate new departure"),
							Res.GetString("D410F0F8-72E0-48E9-8766-4D1ACFF37105", "yes"),
							MessageBoxIcon.Question);
						if (userResult == DialogResult.OK)
						{
							Header.TurnArrivalIntoDepartureAndArrival();
						}
					}
					else if (matchingDepartureResults.result == NctsHeader.DepartureRecordFindResult.Unknown)
					{
						userResult = DialogResult.Cancel;
					}

					var validResultForSend = new List<EU.NCTS.Business.NctsHeader.DepartureRecordFindResult>() { NctsHeader.DepartureRecordFindResult.AlreadyCombinedDepartureAndArrival, NctsHeader.DepartureRecordFindResult.FoundByMatchingMrn };
					if (validResultForSend.Contains(matchingDepartureResults.result) && userResult == DialogResult.OK)
					{
						SendMessageToNcts(new NctsMessageFunctionSet.CombinedArrivalAndDepartureMessage());
					}
				});
		}

		void ViewOnCustomsWebsiteClick(object sender, EventArgs ev)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider((NctsHeader)Header);
			commonMenuProvider.LaunchCustomsWebsite();
		}

		void CreateEXSDeclarationClick(object sender, EventArgs ev)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider((NctsHeader)Header);
			commonMenuProvider.CreateEXSDeclaration();
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility(downloadTADMenuItem, () => CanDownloadTADMenuItem());
			SetMenuItemVisibility(sendCombinedArrivalAndDepartureMenuItem, () => CanSendCombinedArrivalAndDeparture());
			SetMenuItemVisibility(viewOnCustomsWebsite, () => CanViewOnCustomsWebsite());
			SetMenuItemVisibility(createEXSDeclaration, () => CanCreateEXSDeclaration());
		}

		ZBool CanDownloadTADMenuItem() => !Header.MovementReferenceNumber.IsEmpty;

		void DownloadTADClick(object sender, EventArgs e)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider((NctsHeader)Header);
			commonMenuProvider.DownloadTAD();
		}

		protected override ZBool CanSendArrivalMessage
		{
			get
			{
				return Header.EffectiveMessageStatus != NctsMessageStatusList.Codes.ArrivalNotificationSent
					&& sendableDeclarationStatuses.Contains(Header.ArrivalMovementHeader.BM_CustomsStatus);
			}
		}

		ZBool CanSendCombinedArrivalAndDeparture()
		{
			return Header.EffectiveMessageStatus != NctsMessageStatusList.Codes.ArrivalNotificationSent
				&& sendableDeclarationStatuses.Contains(Header.ArrivalMovementHeader.BM_CustomsStatus);
		}

		ZBool CanViewOnCustomsWebsite()
		{
			NctsHeader nctsHeader = (NctsHeader)Header;
			return nctsHeader.CanLaunchNctsUrl();
		}

		ZBool CanCreateEXSDeclaration()
		{
			NctsHeader nctsHeader = (NctsHeader)Header;
			return acceptedStatusesToCreateEXSDeclaration.Contains(nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
		}

		readonly ZString[] sendableDeclarationStatuses = new ZString[] { NctsTransitStatusList.Codes.Unknown, NctsTransitStatusList.Codes.DeclarationRejected };

		readonly ZString[] acceptedStatusesToCreateEXSDeclaration = new ZString[] { NctsTransitStatusList.Codes.UnloadingPermissionGranted, NctsTransitStatusList.Codes.GoodsWrittenOff, NctsTransitStatusList.Codes.GoodsUnderCustomsControl };
	}
}
