using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public class Phase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
	{
		public Phase5MessagingMenuProvider(NctsHeader header) : base(header) { }

		public Phase5MessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper) : base(header, ntcsHeaderUniversalMessagingHelper) { }

		public new NctsHeader Header => (NctsHeader)base.Header;

		protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
		{
			foreach (var menuItem in base.CreateMenuItemsCore())
			{
				yield return menuItem;
			}
			if (Header.IsDepartureMovement)
			{
				yield return QueryOnGuarantee;
				yield return GuaranteeAccessCodes;
				yield return UploadSupportingDocuments;
			}
		}

		ZMenuItem QueryOnGuarantee => queryOnGuarantee ??= new ZMenuItem(ResString.GetMultilingualString("DDDA5894-95CA-4D1A-83E2-D4149359BF46", "Query on Guarantee"), QueryOnGuaranteeClick);
		ZMenuItem queryOnGuarantee;

		ZMenuItem GuaranteeAccessCodes => guaranteeAccessCodes ??= new ZMenuItem(ResString.GetMultilingualString("AB26585B-69B5-47C0-97B0-8D1DD12A1CBB", "Guarantee Access Codes"), GuaranteeAccessCodesClick);
		ZMenuItem guaranteeAccessCodes;

		ZMenuItem UploadSupportingDocuments => uploadSupportingDocuments ??= new ZMenuItem(ResString.GetMultilingualString("C529308C-661C-47C1-90AA-284CBAF096AD", "Upload Supporting Documents"), UploadSupportingDocumentsClick);
		ZMenuItem uploadSupportingDocuments;

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility(QueryOnGuarantee, () => CanSendQueryOnGuarantee);
			SetMenuItemVisibility(GuaranteeAccessCodes, () => CanSendGuaranteeAccessCodes);
		}

		protected override void SendToCustomsCore(ZMenuItem menuItem)
		{
			if (Header.Company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired())
			{
				base.SendToCustomsCore(menuItem);
			}
		}

		protected override EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormCore(EU.NCTS.Business.NctsHeaderMessageSendingObjectParent messageSendingobjectParent) => new MessageSendingForm(messageSendingobjectParent);

		bool CanSendQueryOnGuarantee => true;

		bool CanSendGuaranteeAccessCodes => true;

		void QueryOnGuaranteeClick(object sender, EventArgs e)
		{
			var message = Res.GetString("85CDB3B4-0326-4727-95B8-B4704DF00D06", "Please provide Guarantee details in the Guarantees Grid to send Query message.");
			if (CheckBeforeSending(sender, HasGuarantees, message))
			{
				QueryOnGuaranteeForm.ShowForm(Header);
			}
		}

		void UploadSupportingDocumentsClick(object sender, EventArgs e)
		{
			var message = Res.GetString("F63EE04E-0BFE-4F9E-A9AD-C40E8FECD7A2", "The Supporting Documents message cannot be sent before the declaration has an MRN.");
			if (CheckBeforeSending(sender, HasMrn, message))
			{
				DocumentsSendingForm.ShowForm(Header);
			}
		}

		void GuaranteeAccessCodesClick(object sender, EventArgs e)
		{
			var message = Res.GetString("8E4FD71F-F2F7-45BC-9AB9-C5EB9FA7546C", "Please provide valid Guarantee details in the Guarantees Grid to send Guarantee Access code message.");
			if (CheckBeforeSending(sender, HasValidGuarantees, message))
			{
				GuaranteeAccessCodesUserSelectionForm.ShowForm(Header);
			}
		}

		bool HasGuarantees() => Header.MovementHeader.Guarantees.Count > 0;

		bool HasValidGuarantees() => Header.MovementHeader.Guarantees.Cast<NctsGuarantee>().Select(nctsGuarantee => nctsGuarantee.CusGuarantee).Any();

		bool HasMrn() => !Header.MovementReferenceNumber.IsEmpty;

		bool CheckBeforeSending(object sender, Func<bool> predicate, string message)
			=> sender is ZMenuItem menuItem && PreSaveHeader(menuItem) && Header.Company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired() && CheckAdditionalConditions(predicate, message);

		bool CheckAdditionalConditions(Func<bool> predicate, string message)
		{
			if (predicate is null || predicate())
			{
				return true;
			}
			else
			{
				Globals.Message.ShowInformation(message);
				return false;
			}
		}
	}
}
