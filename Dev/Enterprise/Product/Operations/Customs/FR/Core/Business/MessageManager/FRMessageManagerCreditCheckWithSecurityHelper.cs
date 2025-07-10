using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business
{
	public class FRMessageManagerCreditCheckWithSecurityHelper : MessageManagerCreditCheckWithSecurityHelper
	{
		public FRMessageManagerCreditCheckWithSecurityHelper(MessageSending.JobDeclarationMessageSendingObject objectToSend, ErrorCollector errorCollector)
			: base(objectToSend.Header.Declaration)
		{
			this.objectToSend = Argument.NotNull(objectToSend, "objectToSend cannot be null");
			this.entryHeader = Argument.NotNull(objectToSend.Header, "entryHeader cannot be null");
			this.declaration = Argument.NotNull(objectToSend.Header.Declaration, "declaration cannot be null");
			this.errorCollector = Argument.NotNull(errorCollector, "errorCollector cannot be null");
		}

		public static bool ShouldCheckCreditForThisDeclarationAndMessage(JobDeclaration declaration, CusEntryHeader entryHeader, ZString messageType)
		{
			var companyPK = declaration.RegistryCompanyPK;
			var branchPk = declaration.RegistryBranchPK;
			var shouldCheck = false;
			var shouldAlwaysCheck = FRCustomsDataRegistry.Instance.CheckAtEverySubmission.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);
			if (!shouldAlwaysCheck)
			{
				if (!messageType.IsEmpty)
				{
					var checkNotLodgedArrived = FRCustomsDataRegistry.Instance.CheckWhenSendingAnArrivedValidee.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);
					var checkPreLodgedArriving = FRCustomsDataRegistry.Instance.CheckWhenChangingAnAnticipateToAValidee.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);

					if (!shouldCheck && checkNotLodgedArrived && IsNotLodgedButArrived(entryHeader, messageType))
					{
						shouldCheck = true;
					}

					if (!shouldCheck && checkPreLodgedArriving && IsPreLodgedAndArriving(entryHeader, messageType))
					{
						shouldCheck = true;
					}
				}
			}
			else
			{
				shouldCheck = true;
			}

			return shouldCheck;
		}

		public bool WarnAboutCreditChecks()
		{
			var isAllowed = ShouldCheckCreditForThisDeclarationAndMessage(declaration, entryHeader, objectToSend.MessageType) ? this.IsCreditCheckOKToSend : this.IsDeniedPartyOKToSend;
			if (!isAllowed)
			{
				if (!this.IsCreditCheckDoneOutsideCW1)
				{
					errorCollector.AddError(this.ReasonForNotAllowed);
				}
			}
			return isAllowed;
		}

		static bool IsPreLodgedAndArriving(CusEntryHeader entryHeader, ZString messageType)
		{
			return entryHeader.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES050 && messageType == EntryActionCodeList.Codes.VAL;
		}

		static bool IsNotLodgedButArrived(CusEntryHeader entryHeader, ZString messageType)
		{
			return entryHeader.CH_EntryStatus.IsEmpty && messageType == EntryActionCodeList.Codes.VAL;
		}

		readonly JobDeclaration declaration;
		readonly CusEntryHeader entryHeader;
		readonly MessageSending.JobDeclarationMessageSendingObject objectToSend;
		readonly ErrorCollector errorCollector;
	}
}
