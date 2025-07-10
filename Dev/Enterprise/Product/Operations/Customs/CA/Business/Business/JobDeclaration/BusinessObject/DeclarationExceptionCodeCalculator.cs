using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class DeclarationExceptionCodeCalculator
	{
		public DeclarationExceptionCodeCalculator(Guid companyPK)
		{
			currentTime = ZDateTime.Now;
			currentUtcTime = ZDateTime.UtcNow;
			currentUtcDate = currentTime.Date;
			timeInOttawa = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", ZDateTime.UtcNow.ToDateTime());
			b3AcceptedButNotReportedOnDN = CACustomsDataRegistry.Instance.B3AcceptedButNotReportedOnDN.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			b3NoResponseThreshold = CACustomsDataRegistry.Instance.B3NoResponseThreshold.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			releaseNoResponseThreshold = CACustomsDataRegistry.Instance.ReleaseNoResponseThreshold.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			parsNotreleasedOceanThreshold = CACustomsDataRegistry.Instance.PARSNotreleasedOceanThreshold.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			parsNotreleasedAirThreshold = CACustomsDataRegistry.Instance.PARSNotreleasedAirThreshold.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			parsNotReleasedHighwayRailAndOtherThreshold = CACustomsDataRegistry.Instance.PARSNotReleasedHighwayRailAndOtherThreshold.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			postArrivalNotReleased = CACustomsDataRegistry.Instance.PostArrivalNotReleased.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		public void SetDeclarationException(JobDeclaration declaration, BusinessObjectFactory factory)
		{
			if (declaration == null || declaration.IsExport || declaration.CA_DeclarationException == CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy)
			{
				return;
			}

			int threadHoldFortransport = 0;
			if (declaration.CA_K84AccountingDate.IsValid)
			{
				declaration.CA_DeclarationException = ZString.Empty;
			}
			else if (b3AcceptedButNotReportedOnDN > 0 && Is010ConditionMatched(declaration, currentUtcTime, factory))
			{
				declaration.CA_DeclarationException = CAExceptionCodeList.Codes.EntryLodgedAndAcceptedNotReportedONDN;
			}
			else if (b3NoResponseThreshold > 0 && Is020ConditionMatched(declaration, currentUtcTime, factory))
			{
				declaration.CA_DeclarationException = CAExceptionCodeList.Codes.EntrySentButNoResponse;
			}
			else if (Is030ConditionMatched(declaration, timeInOttawa))
			{
				if (!declaration.CA_CSAEntry)
				{
					declaration.CA_DeclarationException = CAExceptionCodeList.Codes.ReleasedNotEntryConfirmed;
				}
				else
				{
					declaration.CA_DeclarationException = ZString.Empty;
				}
			}
			else if (releaseNoResponseThreshold > 0 && Is040ConditionMatched(declaration, currentUtcTime, factory))
			{
				declaration.CA_DeclarationException = CAExceptionCodeList.Codes.ReleaseAndIIDSentNoResponse;
			}
			else if (IsThreadFortransportModeGreateThanZero(declaration.JE_TransportMode, out threadHoldFortransport) && Is050ConditionMatchedHasParsETA(declaration))
			{
				var dateInCharge = declaration.TimeAtPortOfDischarge;
				if (dateInCharge.IsValid && declaration.JE_DateOfFirstArrival < dateInCharge.AddHours(-threadHoldFortransport))
				{
					declaration.CA_DeclarationException = CAExceptionCodeList.Codes.NoTimelyRelease;
				}
				else
				{
					declaration.CA_DeclarationException = ZString.Empty;
				}
			}
			else if (postArrivalNotReleased > 0 && Is050ConditionMatched(declaration, currentUtcDate, factory))
			{
				declaration.CA_DeclarationException = CAExceptionCodeList.Codes.NoTimelyRelease;
			}
			else
			{
				declaration.CA_DeclarationException = ZString.Empty;
			}
		}

		bool Is010ConditionMatched(JobDeclaration declaration, ZDateTime currentDate, BusinessObjectFactory factory)
		{
			var b3EntryHeader = declaration.B3EntryHeader;
			var result = b3EntryHeader != null && b3EntryHeader.IsClearedB3CorCAD &&
						 !declaration.CA_K84StatementDate.IsValid;

			if (result)
			{
				var lastestMessage = LoadEffectiveEdiMessage(factory, b3EntryHeader, EDIMessage.Direction.Receive, b3EntryHeader.CH_EntryStatus);
				var lastestDate = lastestMessage != null ? lastestMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;
				result = lastestDate.Date <
						 currentDate.AddWorkingDaysFromNearestWorkingDay(-b3AcceptedButNotReportedOnDN,
							 declaration.CAHolidaysApplicableToThisDeclaration).Date;
			}
			return result;
		}

		bool Is020ConditionMatched(JobDeclaration declaration, ZDateTime currenttime, BusinessObjectFactory factory)
		{
			var b3EntryHeader = declaration.B3EntryHeader;
			var result = b3EntryHeader != null && !b3EntryHeader.IsClearedB3CorCAD &&
						 b3EntryHeader.CH_Status == MessageStatusList.Codes.AwaitingOriginal;
			if (result)
			{
				var lastestMessage = LoadEffectiveEdiMessage(factory, b3EntryHeader, EDIMessage.Direction.Transmit);
				var lastestDate = lastestMessage != null ? (lastestMessage.EM_HeldUntilDate.IsValid ? lastestMessage.EM_HeldUntilDate : lastestMessage.EM_SystemCreateTimeUtc) : ZDateTime.Empty;
				result = lastestDate < currenttime.AddHours(-b3NoResponseThreshold);
			}
			return result;
		}

		static bool Is030ConditionMatched(JobDeclaration declaration, ZDateTime timeInOttawa)
		{
			var b3EntryHeader = declaration.B3EntryHeader;
			return b3EntryHeader != null && !b3EntryHeader.IsClearedB3CorCAD &&
				   declaration.JE_MessageType == JobMessageTypeList.Codes.Import &&
				   declaration.JE_EntryAuthorisationDate.IsValid &&
				   declaration.CA_EstimatedPaymentDueDate <
				   timeInOttawa.AddWorkingDaysFromNearestWorkingDay(1, declaration.CAHolidaysApplicableToThisDeclaration).Date;
		}

		bool Is040ConditionMatched(JobDeclaration declaration, ZDateTime currenttime, BusinessObjectFactory factory)
		{
			var result = declaration.ReleaseEntryHeader != null &&
						 (declaration.ReleaseEntryHeader.CH_Status == MessageStatusList.Codes.AwaitingOriginal ||
						  declaration.ReleaseEntryHeader.CH_Status == MessageStatusList.Codes.AwaitingChange ||
						  declaration.ReleaseEntryHeader.CH_Status == MessageStatusList.Codes.AwaitingDelete);
			if (result)
			{
				var lastestMessage = LoadEffectiveEdiMessage(factory, declaration.ReleaseEntryHeader, EDIMessage.Direction.Transmit);
				var lastestDate = lastestMessage != null ? lastestMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;
				result = lastestDate < currenttime.AddHours(-releaseNoResponseThreshold);
			}
			return result;
		}

		bool Is050ConditionMatchedHasParsETA(JobDeclaration declaration)
		{
			return declaration.JE_EntryStatus != EntryStatusList.Codes.Cancelled &&
				   declaration.JE_MessageType == JobMessageTypeList.Codes.Import &&
				   (declaration.CA_ServiceOption == ServiceOptions.Codes.PARS ||
					declaration.CA_ServiceOption == ServiceOptions.Codes.PARSOGD ||
					declaration.CA_ServiceOption == ACROSSServiceOptions.Codes.IID)
				   && !declaration.JE_EntryAuthorisationDate.IsValid
				   && declaration.JE_DateOfFirstArrival.IsValid;
		}

		bool Is050ConditionMatched(JobDeclaration declaration, ZDate currentDate, BusinessObjectFactory factory)
		{
			var result = declaration.JE_EntryStatus != EntryStatusList.Codes.Cancelled &&
						 declaration.JE_MessageType == JobMessageTypeList.Codes.Import &&
						 (declaration.CA_ServiceOption == ServiceOptions.Codes.ReplaceRMDwithAQ ||
						  declaration.CA_ServiceOption == ServiceOptions.Codes.RMDOGD)
						 && !declaration.JE_EntryAuthorisationDate.IsValid;
			if (result)
			{
				var lastestMessage = LoadEffectiveEdiMessage(factory, declaration.ReleaseEntryHeader, EDIMessage.Direction.Transmit);
				var lastestDate = lastestMessage != null ? lastestMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;
				result = lastestDate.Date < currentDate.AddDays(-postArrivalNotReleased);
			}
			return result;
		}

		int GetThreadFortransportMode(ZString jeTransportMode)
		{
			switch (jeTransportMode)
			{
				case TransportTypeList.Codes.Sea:
					return parsNotreleasedOceanThreshold;
				case TransportTypeList.Codes.Air:
					return parsNotreleasedAirThreshold;
				default:
					return parsNotReleasedHighwayRailAndOtherThreshold;
			}
		}
		bool IsThreadFortransportModeGreateThanZero(ZString jeTransportMode, out int threadHoldFortransport)
		{
			threadHoldFortransport = GetThreadFortransportMode(jeTransportMode);
			return threadHoldFortransport > 0;
		}

		EDIMessage LoadEffectiveEdiMessage(BusinessObjectFactory factory, CusEntryHeader entryHeader, string transmit, string subtype = "")
		{
			if (entryHeader == null)
			{
				return null;
			}

			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_MessageType, entryHeader.CH_MessageType);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, transmit);
			if (!subtype.IsNullOrEmpty())
			{
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, subtype);
			}
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			query.OrderBy = AutoEDIMessage.Schema.EM_SystemCreateTimeUtc + " DESC";
			var message = factory.LoadTop1<EDIMessage>(query);
			return message;
		}

		readonly ZDateTime currentTime;
		readonly ZDateTime currentUtcTime;
		readonly ZDate currentUtcDate;
		readonly ZDateTime timeInOttawa;
		readonly ZInt b3AcceptedButNotReportedOnDN;
		readonly ZInt b3NoResponseThreshold;
		readonly ZInt releaseNoResponseThreshold;
		readonly ZInt parsNotreleasedOceanThreshold;
		readonly ZInt parsNotreleasedAirThreshold;
		readonly ZInt parsNotReleasedHighwayRailAndOtherThreshold;
		readonly ZInt postArrivalNotReleased;
	}
}
