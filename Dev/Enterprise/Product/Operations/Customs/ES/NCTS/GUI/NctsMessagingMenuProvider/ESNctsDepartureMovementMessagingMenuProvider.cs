using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class ESNctsDepartureMovementMessagingMenuProvider : NctsDepartureMovementMessagingMenuProvider
	{
		public ESNctsDepartureMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsMovementForm nctsMovementForm)
			: base(header)
		{
			ParentForm = nctsMovementForm;
		}

		public ESNctsDepartureMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, EU.NCTS.DataTransfer.NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			return base.CreateMenuItems().Concat(new[] {
				downloadTADMenuItem = new ZMenuItem(ResString.GetMultilingualString("CA68ECB6-F802-4227-AD39-C28F7470F578", "Download TAD (Transit Accompanying Document)"), DownloadTADClick),
				viewOnCustomsWebsite = new ZMenuItem(ResString.GetMultilingualString("FEA0AC88-77C2-4CB2-A5E2-57E58ED6E303", "View on Customs Website"), ViewOnCustomsWebsiteClick),
				editClearanceInfo = new ZMenuItem(ResString.GetMultilingualString("E3BDE56D-14FD-4E33-B955-A0289E2AA726", "Update Clearance info"), EditClearanceInfoClick)
			});
		}
		ZMenuItem downloadTADMenuItem;
		ZMenuItem viewOnCustomsWebsite;
		ZMenuItem editClearanceInfo;

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility(downloadTADMenuItem, () => CanDownloadTADMenuItem());
			SetMenuItemVisibility(viewOnCustomsWebsite, () => CanViewOnCustomsWebsite());
			SetMenuItemVisibility(editClearanceInfo, () => CanEditClearanceInfo());
		}

		ZBool CanDownloadTADMenuItem() => !Header.MovementReferenceNumber.IsEmpty;
		ZBool CanViewOnCustomsWebsite() => ((NctsHeader)Header).CanLaunchNctsUrl();
		ZBool CanEditClearanceInfo() => !Header.MovementReferenceNumber.IsEmpty;

		protected override void SendMessageToNctsCore(NctsMessageFunctionSet messageFunction)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(messageFunction, (NctsHeader)Header, ParentForm);
			commonMenuProvider.ESSendMessageToNcts();
		}

		void DownloadTADClick(object sender, EventArgs e)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider((NctsHeader)Header);
			commonMenuProvider.DownloadTAD();
		}

		void ViewOnCustomsWebsiteClick(object sender, EventArgs ev)
		{
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider((NctsHeader)Header);
			commonMenuProvider.LaunchCustomsWebsite();
		}

		void EditClearanceInfoClick(object sender, EventArgs e)
		{
			UpdateClearanceInfoWithUserInput();
		}

		void UpdateClearanceInfoWithUserInput()
		{
			var cusEntryNumberSelected = CusEntryNumber.LoadOrCreate(Header, CusEntryNumberTypes.Spain.ClearanceCSV, Header.CountryCode);
			var clearanceInfo = ClearanceInfo.LoadNew(cusEntryNumberSelected);
			using (var updateCSVForm = GetEditClearanceInfoForm(clearanceInfo))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(updateCSVForm) == DialogResult.OK)
				{
					var validationResult = ValidateClearanceInfoData(clearanceInfo);
					if (validationResult.IsEmpty)
					{
						UpdateClearanceInfoAndTriggerDocRequests(clearanceInfo);
					}
					else
					{
						Globals.Message.Show(validationResult);
					}
				}
			}
		}

		ZString ValidateClearanceInfoData(ClearanceInfo clearanceInfo)
		{
			var validationError = new ZStringBuilder();

			if (clearanceInfo.ClearanceNumber.IsEmpty)
			{
				validationError.Append(ResString.GetMultilingualString("97AE2185-8307-4C29-AF9B-BCAA69C16DBF", "The Clearance Number must not be empty", clearanceInfo.ClearanceNumber));
			}

			if (!ClearanceInfo.CSVCodeIsValidOrEmpty(clearanceInfo.ClearanceNumber))
			{
				validationError.Append(ResString.GetMultilingualString("657A7F57-946C-4696-877F-8747E3AA7E79", "{0} format is incorrect. Please fill with a correct Clearance Number", clearanceInfo.ClearanceNumber));
			}

			if (clearanceInfo.ArrivalLimitDate.IsEmpty)
			{
				validationError.Append(ResString.GetMultilingualString("6DF883E1-16AC-4D2A-AA03-87E75893D109", "The arrival limit date must not be empty"));
			}

			if (clearanceInfo.ClearanceDate.IsEmpty)
			{
				validationError.Append(ResString.GetMultilingualString("1AC61773-94ED-4195-8AA9-6E27ECB51EDD", "The clearance date date must not be empty"));
			}

			if (clearanceInfo.ArrivalLimitDate <= clearanceInfo.ClearanceDate)
			{
				validationError.Append(ResString.GetMultilingualString("6042DB67-E6CC-4B97-B7EC-88F723F76DC4", "Arrival limit date must be later than Clearance date"));
			}

			return validationError.ToStringWithNewLineBetweenAppends();
		}

		void UpdateClearanceInfoAndTriggerDocRequests(ClearanceInfo clearanceInfo)
		{
			var factory = new BusinessObjectFactory();
			var newFactoryNctsHeader = factory.Load<NctsHeader>(Header.PK);

			newFactoryNctsHeader.SetClearanceInfoClearanceNumber(clearanceInfo.ClearanceNumber);
			newFactoryNctsHeader.SetClearanceInfoClearanceDate(clearanceInfo.ClearanceDate);
			newFactoryNctsHeader.SetClearanceInfoArrivalLimitDate(clearanceInfo.ArrivalLimitDate);

			var messagesSentCount = 0;
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(newFactoryNctsHeader);
			if (commonMenuProvider.CheckBeforeSendingMissingDocuments(out var messageSenderObject))
			{
				messagesSentCount = SendNCTSDepartureDocumentRequestToCustoms(((ICertificateProvider)messageSenderObject).CertificateName, newFactoryNctsHeader);
			}

			try
			{
				factory.Save();
				ShowClearanceUpdateAndDocCaptureRequestSendingSuccessMessage(messagesSentCount);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		int SendNCTSDepartureDocumentRequestToCustoms(ZString certificateName, NctsHeader nctsheader)
		{
			int messagesSentCount = ZInt.Zero;
			var mrnCode = nctsheader.MovementReferenceNumber;
			if (!mrnCode.IsEmpty && !certificateName.IsEmpty)
			{
				var nctsDocRequest = new NCTSDepartureDocumentRequest(nctsheader, certificateName);
				messagesSentCount += nctsDocRequest.RequestMissingDocuments();
			}
			return messagesSentCount;
		}

		void ShowClearanceUpdateAndDocCaptureRequestSendingSuccessMessage(int messagesSentCount)
		{
			Globals.Message.Show(ResString.GetMultilingualString("09732C9D-483B-4B2D-B89B-9A89FB670A8A", "{0} Document Capture request(s) created", messagesSentCount.ToString(Culture.Current)));
		}

		protected virtual EditClearanceInfoForm GetEditClearanceInfoForm(ClearanceInfo clearanceInfo) => new EditClearanceInfoForm(clearanceInfo);
	}
}
