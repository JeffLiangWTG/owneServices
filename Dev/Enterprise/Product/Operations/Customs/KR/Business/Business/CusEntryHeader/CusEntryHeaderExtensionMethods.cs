using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public static class CusEntryHeaderExtensionMethods
	{
		public static BusinessObject[] GetDocumentDataSource(this CusEntryHeader entry, DataContextValue dataContextValue, string englishMenuName)
		{
			if (dataContextValue.FullDataContext == JobDeclarationDocumentSupporter.DataContexts.EXPEntryHeaderBO)
			{
				return new BusinessObject[] { new EntryDocumentWrapper(entry, entry.Factory).ExportEntryWrapper };
			}
			else if (dataContextValue.FullDataContext == JobDeclarationDocumentSupporter.DataContexts.LEXEntryHeaderBO)
			{
				return new BusinessObject[] { new EntryDocumentWrapper(entry, entry.Factory).LocalExportEntryWrapper };
			}
			else if (dataContextValue.FullDataContext == JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO)
			{
				return new BusinessObject[] { new EntryDocumentWrapper(entry, entry.Factory).ImportEntryWrapper };
			}
			else
			{
				switch (englishMenuName)
				{
					case JobDeclarationDocumentSupporter.MenuNames.RefundRequest:
						return Get5ULHeaderWrapperArray(entry).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate:
					case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English:
					case JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo:
						return GetExportEntryWrapperArray(entry, new string[] { ElectronicDocumentTypeList.Codes._830 }).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods:
					case JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate:
						return GetImportEntryWrapperArray(entry, new string[] { ElectronicDocumentTypeList.Codes._929 }).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.ApplicationofFTARate:
						return GetFTAWrapperArray(entry, new string[] { ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._DHR }).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase:
						return GetMessageWrapperArray(entry, ElectronicDocumentTypeList.Codes._5FV).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration:
						return GetExportAmendmentDetails(entry).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration:
						return GetLocalExportEntryWrapperArray(entry, new string[] { ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Codes._5DQ }).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration:
						return GetLocalExportAmendmentDetails(entry).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration:
						return GetExportCancellationDetails(entry).ToArray();
					case JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate:
						return entry.SubsequentMessageDetails.GOVCBRD72Messages.ToArray<EDIMessageWrapper>();
					case JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges:
						return GetIndividualStatementWrapperArray(entry).ToArray();
				}
				return System.Array.Empty<BusinessObject>();
			}
		}

		static CusEntrySnapshot GetLatestSnapshot(CusEntryHeader entry, string[] snapshotTypes)
		{
			CusEntrySnapshot snapshot = null;
			foreach (string snapshotType in snapshotTypes)
			{
				var tmpSnapshot = entry.Snapshots.GetLatestSnapshotIn(snapshotType);
				if (snapshot == null || tmpSnapshot != null && snapshot.CES_SystemCreateTimeUtc < tmpSnapshot.CES_SystemCreateTimeUtc)
				{
					snapshot = tmpSnapshot;
				}
			}

			if (snapshot == null)
			{
				var query = new ZDBOnlyQuery(typeof(CusEntrySnapshot));
				query.AddToFilter(CusEntrySnapshotSchema.CES_CH_EntryHeader, entry.PK);
				query.AddToFilter(CusEntrySnapshotSchema.CES_MessageType, SQLComparisonOperator.Contains, snapshotTypes);
				query.AddToFilter(CusEntrySnapshotSchema.CES_Status, EntrySnapshotStatus.Deleted);
				query.OrderBy = CusEntrySnapshotSchema.CES_SystemCreateTimeUtc.Name;

				snapshot = entry.Factory.Load<CusEntrySnapshot>(query).LastOrDefault();
			}

			return snapshot;
		}
		static IEnumerable<BusinessObject> Get5ULHeaderWrapperArray(CusEntryHeader entry)
		{
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5UL);
			if (snapshot != null)
			{
				yield return new EntrySnapshotWrapper(snapshot, entry.Factory).DataProvider5UL;
			}
		}
		static IEnumerable<BusinessObject> GetExportEntryWrapperArray(CusEntryHeader entry, string[] messageTypes)
		{
			var snapshot = GetLatestSnapshot(entry, messageTypes);
			if (snapshot != null)
			{
				yield return new EntrySnapshotWrapper(snapshot, entry.Factory).ExportEntryWrapper;
			}
			else
			{
				yield return new EntryDocumentWrapper(entry, entry.Factory).ExportEntryWrapper;
			}
		}
		static IEnumerable<BusinessObject> GetLocalExportEntryWrapperArray(CusEntryHeader entry, string[] messageTypes)
		{
			var snapshot = GetLatestSnapshot(entry, messageTypes);
			if (snapshot != null)
			{
				yield return new EntrySnapshotWrapper(snapshot, entry.Factory).LocalExportEntryWrapper;
			}
			else
			{
				yield return new EntryDocumentWrapper(entry, entry.Factory).LocalExportEntryWrapper;
			}
		}
		static IEnumerable<BusinessObject> GetImportEntryWrapperArray(CusEntryHeader entry, string[] messageTypes)
		{
			var snapshot = GetLatestSnapshot(entry, messageTypes);
			if (snapshot != null)
			{
				yield return new EntrySnapshotWrapper(snapshot, entry.Factory).ImportEntryWrapper;
			}
			else
			{
				yield return new EntryDocumentWrapper(entry, entry.Factory).ImportEntryWrapper;
			}
		}
		static IEnumerable<BusinessObject> GetIndividualStatementWrapperArray(CusEntryHeader entry)
		{
			return new IndividualStatementWrapperCollection(entry, entry.Factory).ToArray();
		}
		static IEnumerable<BusinessObject> GetFTAWrapperArray(CusEntryHeader entry, string[] messageTypes)
		{
			var snapshot = GetLatestSnapshot(entry, messageTypes);
			if (snapshot != null)
			{
				yield return new EntrySnapshotWrapper(snapshot, entry.Factory).FTAHeader;
			}
			else
			{
				yield return new EntryDocumentWrapper(entry, entry.Factory).FTAHeader;
			}
		}
		static IEnumerable<BusinessObject> GetMessageWrapperArray(CusEntryHeader entry, string messageType)
		{
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == messageType);
			if (messages != null)
			{
				foreach (var message in messages)
				{
					yield return new EDIMessageWrapper(message);
				}
			}
		}

		static IEnumerable<BusinessObject> GetExportCancellationDetails(CusEntryHeader entry)
		{
			var message = entry.Messages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._DKJ);
			if (message != null)
			{
				var exportCancellationDetails = new ExportCancellationDetails(message);
				exportCancellationDetails.Decorate(entry);
				yield return exportCancellationDetails;
			}
		}
		static IEnumerable<BusinessObject> GetExportAmendmentDetails(CusEntryHeader entry)
		{
			return entry.ExportAmendmentDetailsCollection;
		}

		static IEnumerable<BusinessObject> GetLocalExportAmendmentDetails(CusEntryHeader entry)
		{
			return entry.LocalExportAmendmentDetailsCollection;
		}

		public static EDIMessage GetIncomingMessage(this ZGuid entryPK, BusinessObjectFactory factory, ZString outgoingMessageNum, string incomingMessageType, string incomingMessageSubType = null, string incomingMessageMessageOwner = null)
		{
			if (outgoingMessageNum.IsEmpty && string.IsNullOrEmpty(incomingMessageSubType))
			{
				return null;
			}
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryPK);
			if (!outgoingMessageNum.IsEmpty)
			{
				query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, outgoingMessageNum);
			}
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, incomingMessageType);
			if (incomingMessageSubType != null)
			{
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, incomingMessageSubType);
			}
			if (incomingMessageMessageOwner != null)
			{
				query.AddToFilter(EDIMessageSchema.EM_MessageOwner, incomingMessageMessageOwner);
			}
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " " + OrderByClause.Descending;
			return factory.LoadTop1<EDIMessage>(query);
		}

		public static EDIMessage GetOutgoingMessage(this CusEntryHeader entry, ZString outgoingMessageNum, string outgoingMessageType)
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK);
			if (!outgoingMessageNum.IsEmpty)
			{
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, outgoingMessageNum);
			}
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, outgoingMessageType);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " " + OrderByClause.Descending;
			return entry.Factory.LoadTop1<EDIMessage>(query);
		}

		public static EDIMessage GetLatestR99Message(this CusEntryHeader entry, string outgoingMessageType)
		{
			return entry.PK.GetIncomingMessage(entry.Factory, ZString.Empty, ElectronicDocumentTypeList.Codes._R99, outgoingMessageType);
		}

		public static EDIMessage GetAcceptedOutgoingMessage(this CusEntryHeader entry, string outgoingMessageType)
		{
			var outgoingMessageNum = entry.GetLatestR99Message(outgoingMessageType)?.EM_ApplicationReference ?? "";
			return entry.GetOutgoingMessage(outgoingMessageNum, outgoingMessageType);
		}

		public static ZDecimal GetPenaltyExemptionAmount(this CusEntryHeader entry, EDIMessage message5FE)
		{
			var message5FK = entry.Messages.Cast<EDIMessage>().FirstOrDefault(x =>
									x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FK
								&& x.EM_MessageOwner == CustomsEntryStatusTypeList.Codes.ANT
								&& x.EM_ApplicationReference == message5FE.EM_MessageNum);

			var result = 0m;

			if (message5FK != null)
			{
				using (var reader = message5FK.GetEM_MessageTextReader())
				{
					result = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FK.Response>(reader).Declaration.GoodsShipment?.DutyTaxFee?.TotalTaxAmount?.Value ?? ZDecimal.Zero;
				}
			}
			return result;
		}

		public static ZDecimal GetAdditionalDutyOrTaxAmount(this CusEntryHeader entry, string dutyOrTaxCode, string versionNumber = "")
		{
			return entry?.Charges.Where(x => x.C1_RateOverrideReasonCode == versionNumber && x.C1_ChargeType == dutyOrTaxCode).Sum(x => x.C1_ChargeAmount) ?? ZDecimal.Zero;
		}

		public static ZDecimal GetTotalDutyOrTaxAmount(this CusEntryHeader entry, string dutyOrTaxCode)
		{
			return entry?.Charges.Where(x => x.C1_ChargeType == dutyOrTaxCode).Sum(x => x.C1_ChargeAmount) ?? ZDecimal.Zero;
		}

		public static void UpdateChargeAmount(this CusEntryHeader entry, string dutyOrTaxCode, decimal newTotalAmount)
		{
			var charges = entry.Charges.Where(x => x.C1_ChargeType == dutyOrTaxCode);
			var totalVersionedAmount = charges.Where(x => !x.C1_RateOverrideReasonCode.IsEmpty).Sum(x => x.C1_ChargeAmount);
			var difference = newTotalAmount - totalVersionedAmount;

			var nonVersionedCharge = charges.FirstOrDefault(x => x.C1_RateOverrideReasonCode.IsEmpty);
			if (nonVersionedCharge == null)
			{
				nonVersionedCharge = entry.Charges.AddNew();
				nonVersionedCharge.C1_ChargeType = dutyOrTaxCode;
			}

			nonVersionedCharge.C1_ChargeAmount = difference;
		}

		public static void SetChargeAmount(this CusEntryHeader entry, string dutyOrTaxCode, decimal amount, string versionNumber = "")
		{
			var charge = entry.Charges.Where(x => x.C1_RateOverrideReasonCode == versionNumber && x.C1_ChargeType == dutyOrTaxCode)?.FirstOrDefault();
			if (charge == null)
			{
				charge = entry.Charges.AddNew();
				charge.C1_ChargeType = dutyOrTaxCode;
			}
			charge.C1_ChargeAmount = amount;
		}

		public static void SetFeeAmount(this CusEntryLine entryLine, string dutyOrTaxCode, decimal newTotalAmount, decimal? rate = null)
		{
			var charges = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == dutyOrTaxCode);
			var totalVersionedAmount = charges.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			var difference = newTotalAmount - totalVersionedAmount;

			var nonVersionedCharge = charges.FirstOrDefault(x => x.CF_RateOverrideReasonCode.IsEmpty);
			if (nonVersionedCharge == null)
			{
				nonVersionedCharge = entryLine.Fees.AddNew() as CusEntryLineFee;
				nonVersionedCharge.CF_ChargeType = dutyOrTaxCode;
			}
			if (rate != null)
			{
				nonVersionedCharge.CF_Rate = rate.Value;
			}
			nonVersionedCharge.CF_ChargeAmount = difference;
		}

		public static void SetChargesAndLineFeesVersion(this CusEntryHeader entry)
		{
			var charges = entry.Charges.Where(x => x.C1_RateOverrideReasonCode.IsEmpty);
			var versionNumber = (entry.CH_VersionID + 1).ToString();
			foreach (var charge in charges)
			{
				charge.C1_RateOverrideReasonCode = versionNumber;
			}

			foreach (var entryLine in entry.MergedLines)
			{
				entryLine.SetFeesVersion(versionNumber);
			}
		}

		public static void SetFeesVersion(this CusEntryLine entryLine, string versionNumber)
		{
			var fees = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode.IsEmpty);
			foreach (var fee in fees)
			{
				fee.CF_RateOverrideReasonCode = versionNumber;
			}
		}

		public static void ResetChargesAndLineFeesVersionAndAmount(this CusEntryHeader entry, string versionNumber)
		{
			var redefineCharges = entry.Charges.Where(x => x.C1_RateOverrideReasonCode == versionNumber);
			foreach (var redefineCharge in redefineCharges.ToList())
			{
				var currentCharges = entry.Charges.Where(x => x.C1_RateOverrideReasonCode.IsEmpty && x.C1_ChargeType == redefineCharge.C1_ChargeType);
				if (currentCharges.Any())
				{
					ZDecimal currentAmount = currentCharges.Sum(x => x.C1_ChargeAmount);
					redefineCharge.C1_ChargeAmount += currentAmount;

					foreach (var currentCharge in currentCharges.ToList())
					{
						entry.Charges.RemoveAndDelete(currentCharge);
					}
				}
				redefineCharge.C1_RateOverrideReasonCode = ZString.Empty;
			}
			
			foreach (var entryLine in entry.MergedLines)
			{
				entryLine.ResetFeesVersionAndAmount(versionNumber);
			}
		}

		public static void ResetFeesVersionAndAmount(this CusEntryLine entryLine, string versionNumber)
		{
			var redefineFees = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode == versionNumber);
			foreach (var redefineFee in redefineFees.ToList())
			{
				var currentFees = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode.IsEmpty && x.CF_ChargeType == redefineFee.CF_ChargeType);
				if (currentFees.Any())
				{
					ZDecimal currentAmount = currentFees.Sum(x => x.CF_ChargeAmount);
					redefineFee.CF_ChargeAmount += currentAmount;

					foreach (var currentFee in currentFees.ToList())
					{
						entryLine.Fees.RemoveAndDelete(currentFee);
					}
				}
				redefineFee.CF_RateOverrideReasonCode = ZString.Empty;
			}
		}

		public static AmendmentSessionalData GetAmendmentSessionalDataWithMatchingVersionNumber(this CusEntryHeader entry, ZShort cw1VersionNumber)
		{
			var result = entry.EntryInstruction?.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.AmendmentCW1VersionNo == cw1VersionNumber);
			return result;
		}

		public static AmendmentSessionalData LoadAmendmentSessionalData(this CusEntryHeader entry, ZShort customsVersionNumber, ZDate submissionDate)
		{
			var cw1VersionNumber = entry.GetCW1VersionNumberFromCustoms5FEVersionNumber(customsVersionNumber, submissionDate);
			var result = entry.GetAmendmentSessionalDataWithMatchingVersionNumber(cw1VersionNumber);
			return result;
		}

		public static PenaltyExemptionSessionalData LoadPenaltyExemptionSessionalDataRelated5FE(this CusEntryHeader entry, ZShort versionNumber)
		{
			PenaltyExemptionSessionalData result = null;
			var amendmentSessionalData = entry.GetAmendmentSessionalDataWithMatchingVersionNumber(versionNumber);
			if (amendmentSessionalData != null)
			{
				result = amendmentSessionalData.ValidPenaltyExemptionSessionalData;
			}
			return result;
		}

		public static PenaltyExemptionSessionalData LoadPenaltyExemptionSessionalDataRelated5UA(this CusEntryHeader entry, ZShort versionNumber)
		{
			PenaltyExemptionSessionalData result = null;
			var amendmentSessionalData = entry.EntryInstruction?.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.PenaltyExemptionSessionalData.CSI_LineNo == versionNumber);
			if (amendmentSessionalData != null)
			{
				result = amendmentSessionalData.ValidPenaltyExemptionSessionalData;
			}
			return result;
		}
		public static RefundSessionalData GetOrCreateRefundSessionalDataOriginalSendable(this CusEntryHeader entry, string disbursementBillNumber)
		{
			RefundSessionalData result = null;

			if (entry.EntryInstruction != null)
			{
				result = entry.EntryInstruction?.RefundSessionalDataCollection.Cast<RefundSessionalData>().FirstOrDefault(x => x.CustomsDisbursementBill == disbursementBillNumber && x.CSI_DateOfIssue.IsEmpty);
				if (result == null)
				{
					result = entry.EntryInstruction.RefundSessionalDataCollection.AddNew();
					result.CSI_ReferenceNumber2 = disbursementBillNumber;
				}
			}
			return result;
		}

		public static CusEntryNumber GetMatchingRefundCusEntryNum(this CusEntryHeader entry, ZString refundDeclarationNumber, ZString disbursementBillNumber)
		{
			CusEntryNumber result = refundDeclarationNumber.IsEmpty ? null : entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL && x.CE_EntryNum == refundDeclarationNumber);

			result ??= entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL && x.CE_EntryNum.IsEmpty && x.CE_EntryLineReference == disbursementBillNumber && CustomsMessageStatusTypeList.IsOriginalMessageAllowed(x.CE_EntryStatus));
			
			return result;
		}
	}
}
