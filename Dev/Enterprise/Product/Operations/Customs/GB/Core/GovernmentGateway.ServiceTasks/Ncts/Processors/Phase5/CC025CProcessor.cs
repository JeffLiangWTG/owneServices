using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc025c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC025CProcessor : NctsBaseProcessor<Cc025CType>
	{
		public CC025CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Goods Release Notification";

		protected override ZString LRN => null;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override ZString NoteForUnableToFindALinkedBusinessObject => ZString.Format("The processing of the message with interchange failed because the message could not be linked to a NCTS declaration with MRN {0}.", MRN);

		protected override string GetMessageId(Cc025CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc025CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc025CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewArrivalStatus(Cc025CType messageObject)
		{
			var customsStatus = ZString.Empty;
			switch (messageObject.TransitOperation.ReleaseIndicator)
			{
				case Constants.ReleaseIndicator.FullRelease:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
					break;
				case Constants.ReleaseIndicator.PartialRelease:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease;
					break;
				case Constants.ReleaseIndicator.NoRelease:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
					break;
				case Constants.ReleaseIndicator.PartialReleaseClosed:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
					break;
			}
			return customsStatus;
		}

		protected override void AfterUpdateNCTSHeader(Cc025CType messageObject)
		{
			var moveHeader = NctsHeaderItem.ArrivalMovementHeader;

			foreach (var houseConsignment in messageObject.Consignment)
			{
				foreach (var consignmentItem in houseConsignment.ConsignmentItem)
				{
					foreach (var packaging in consignmentItem.Packaging)
					{
						var package = RetrievePackageBySequencesForArrivalDeclaration(NctsHeaderItem, int.Parse(houseConsignment.SequenceNumber), int.Parse(consignmentItem.DeclarationGoodsItemNumber), int.Parse(packaging.SequenceNumber));
						if (package != null)
						{
							package.B5_UnitsReleased = short.Parse(packaging.NumberOfPackages);
						}
					}
				}
			}

			if (messageObject.TransitOperation != null)
			{
				NctsHeaderItem.MovementReferenceEntryNumber.CE_IssueDate = messageObject.TransitOperation.ReleaseDate;
			}
			NctsHeaderItem.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTimeOffset.Now, reference: moveHeader.BM_CustomsStatus));
			NctsHeaderItem.LockFileIfEnabledByConfiguration(ZString.Format("The tab 'Unloading Remarks' and its depending tabs were locked when message 'Goods Release Notification' was received."), EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks);
		}

		static CusInvPack RetrievePackageBySequencesForArrivalDeclaration(NctsHeader header, int houseConsignmentSequence, int houseConsignmentItemDeclarationSequence, int packageSequence)
		{
			var billSequence = houseConsignmentSequence.ToString();
			return header
				.Bills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == billSequence)
				?.ArrivalGoodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == houseConsignmentItemDeclarationSequence)
				?.Packages.Cast<NctsPackage>().FirstOrDefault(p => p.B5_SequenceNumber == packageSequence);
		}

		protected override ZString GetMessageInterpretation(Cc025CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			if (messageObject.TransitOperation != null)
			{
				switch (messageObject.TransitOperation.ReleaseIndicator)
				{
					case Constants.ReleaseIndicator.FullRelease:
						note.Append("All Goods are released for transit upon arrival. The movement is closed.");
						break;
					case Constants.ReleaseIndicator.PartialRelease:
						note.Append("Goods are partially released.");
						break;
					case Constants.ReleaseIndicator.PartialReleaseClosed:
						note.Append("Goods are partially released. The movement is closed.");
						break;
					case Constants.ReleaseIndicator.NoRelease:
						note.Append("No release of Goods.");
						break;
				}

				if (messageObject.TransitOperation.ReleaseIndicator != Constants.ReleaseIndicator.NoRelease)
				{
					foreach (var houseConsignment in messageObject.Consignment)
					{
						note.Append($"{houseConsignment.SequenceNumber}) House Bill: {(houseConsignment.ReleaseType == Constants.ReleaseType.PartialRelease ? "partial release" : "full release")}");
						foreach (var consignmentItem in houseConsignment.ConsignmentItem)
						{
							note.Append($"{consignmentItem.DeclarationGoodsItemNumber}) Item: {(consignmentItem.ReleaseType == Constants.ReleaseType.PartialRelease ? "partial release" : "full release")}");
							foreach (var package in consignmentItem.Packaging)
							{
								note.Append($"-- {package.NumberOfPackages} {package.TypeOfPackages} with the marks and numbers '{package.ShippingMarks}' are released");
							}
						}
					}
				}
			}

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Arrival;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
