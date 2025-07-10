using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.CustomsEntryHeaders))]
	public class CusEntryHeader : EU.Business.Declaration.CusEntryHeader
		, Integration.Customs.IE.ICusEntryHeader
		, IAISMessageAttachee
		, IAdditionalInfoCollectionProvider
		, ILRNProvider
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.CusEntryHeader.Schema
		{
		}

		public override bool ShouldLogEntryStatus => true;

		public override ZString EntryHeaderStatusDescription => CH_EntryStatus.IsEmpty ? ZString.Empty : base.EntryHeaderStatusDescription;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;
		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;
		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		public bool DuplicationPossible
		{
			get
			{
				var status = CH_EntryStatus;
				if (IsExport)
				{
					return IsStatusValidForDuplication(status, AESEntryStatusList.Codes.Cancelled, AESEntryStatusList.Codes.Refused, AESEntryStatusList.Codes.Rejected, AESEntryStatusList.Codes.DiversionRequestRejected);
				}
				else if (IsImport)
				{
					return IsStatusValidForDuplication(status, AISEntryStatusList.Codes.Cancelled, AISEntryStatusList.Codes.NotReleased, AISEntryStatusList.Codes.Rejected);
				}
				else
				{
					return false;
				}
			}
		}
		bool IsStatusValidForDuplication(ZString status, params ZString[] invalidStatuses) => !status.IsEmpty && !invalidStatuses.Contains(status);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			if (IsExport)
			{
				return new ExportCusEntryHeaderLookups(this);
			}
			else if (IsImport)
			{
				return new ImportCusEntryHeaderLookups(this);
			}
			else
			{
				return new CusEntryHeaderLookups(this);
			}
		}

		protected override bool IsLookupsCachedInBase => false;

		public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

		public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new IEnumerable<AdditionalInfo> AdditionalInfos => base.AdditionalInfos.Cast<AdditionalInfo>();
		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> GetAdditionalInfosToProcess()
		{
			foreach (JobComInvoiceHeader invoiceHeader in InvoiceHeaders)
			{
				foreach (AdditionalInfo document in invoiceHeader.AdditionalInfos)
				{
					yield return document;
				}
			}

			if (EntryInstruction is CusEntryInstruction instruction)
			{
				foreach (AdditionalInfo document in instruction.AdditionalInfos)
				{
					yield return document;
				}
			}
		}

		protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

		protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);

		public new IEnumerable<PreviousDocument> PreviousDocuments => base.PreviousDocuments.Cast<PreviousDocument>();
		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetPreviousDocumentsToProcess()
		{
			foreach (JobComInvoiceHeader invoiceHeader in InvoiceHeaders)
			{
				foreach (PreviousDocument document in invoiceHeader.PreviousDocuments)
				{
					yield return document;
				}
			}

			if (EntryInstruction is CusEntryInstruction instruction)
			{
				foreach (PreviousDocument document in instruction.PreviousDocuments)
				{
					yield return document;
				}
			}
		}

		protected override ZBool ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction => false;

		public new IEnumerable<SupportingDocument> SupportingDocuments => base.SupportingDocuments.Cast<SupportingDocument>();
		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocumentsToProcess()
		{
			foreach (JobComInvoiceHeader invoiceHeader in InvoiceHeaders)
			{
				foreach (SupportingDocument document in invoiceHeader.SupportingDocuments)
				{
					yield return document;
				}
			}

			if (EntryInstruction is CusEntryInstruction instruction)
			{
				foreach (SupportingDocument document in instruction.SupportingDocuments)
				{
					yield return document;
				}
			}
		}

		public string GetLRNAndSetIfNeeded() => new LRNGenerator(Factory, Declaration.Branch).GetLRNAndSetIfNeeded((ZPropertyInfoString)CH_BGMReferenceInfo, IsLastEDIMessageReceivedASyntaxError);

		bool IsLastEDIMessageReceivedASyntaxError()
		{
			var applicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
			var messageType = AISInterchangeTypeList.Codes.IM917;
			if (IsExport)
			{
				applicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
				messageType = AESIncomingMessageTypeList.Codes.IE917;
			}
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, CH_SystemCreateTimeUtc);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			var result = Factory.LoadTop1<EDIMessage>(query);
			return result != null && result.EM_MessageType.EqualsIgnoringCase(messageType);
		}

		public override ZBool ShouldSetUCRinBGMReferenceNumber => false;

		protected override ZString EntryNumberType => IsExport || IsImport ? new ZString(Common.CusEntryNumberTypes.Standard.MovementReferenceNumber) : base.EntryNumberType;

		internal (IDictionary<CusEntryLine, HashSet<CusEntryLine>> MainPackEntryLines, ISet<CusEntryLine> EntryLnesRelatedToMainPack) PackageRelatedEntryLines => Factory.GetValue(ref packageRelatedEntryLinesCached, () =>
		{
			var mainPackRelatedEntryLines = new Dictionary<CusEntryLine, HashSet<CusEntryLine>>();
			var entryLinesRelatedToMainPack = new HashSet<CusEntryLine>();
			if (IsExport)
			{
				var packageEntryLineMapped = new Dictionary<ZGuid, HashSet<CusEntryLine>>();
				var entryPackageLineMapped = new Dictionary<CusEntryLine, HashSet<ZGuid>>();
				var mainPackEntryLines = new HashSet<CusEntryLine>();
				var nonMainPackEntryLines = new HashSet<CusEntryLine>();
				foreach (CusEntryLine entryLine in MergedLines)
				{
					var invoiceLine = entryLine.RandomMainPackLineOrRandomLine;
					var isMainPack = invoiceLine.ZG_IsMainPack;
					var packagePKs = new HashSet<ZGuid>();
					invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().ForEach(x =>
					{
						var packagePK = x.CHC_CW;
						var entryLineMapped = packageEntryLineMapped.GetOrAdd(packagePK, () => new HashSet<CusEntryLine>());
						entryLineMapped.Add(entryLine);
						packagePKs.Add(packagePK);
					});
					entryPackageLineMapped.Add(entryLine, packagePKs);
					if (isMainPack)
					{
						mainPackEntryLines.Add(entryLine);
						var relatedEntryLines = new HashSet<CusEntryLine>();
						relatedEntryLines.Add(entryLine);
						mainPackRelatedEntryLines.Add(entryLine, relatedEntryLines);
					}
					else
					{
						nonMainPackEntryLines.Add(entryLine);
					}
				}
				if (mainPackEntryLines.Count > 0 && nonMainPackEntryLines.Count > 0)
				{
					mainPackEntryLines.ForEach(entryLine =>
					{
						var relatedEntryLines = mainPackRelatedEntryLines[entryLine];
						var packagePKs = entryPackageLineMapped[entryLine];
						packagePKs.ForEach(packagePK =>
						{
							if (packageEntryLineMapped.TryGetValue(packagePK, out var entryLineMapped))
							{
								entryLineMapped.Where(otherEntryLine => otherEntryLine != entryLine && nonMainPackEntryLines.Contains(otherEntryLine)).ForEach(otherEntryLine =>
								{
									relatedEntryLines.Add(otherEntryLine);
									entryLinesRelatedToMainPack.Add(otherEntryLine);
								});
							}
						});
					});
				}
			}
			return ((IDictionary<CusEntryLine, HashSet<CusEntryLine>>)mainPackRelatedEntryLines, (ISet<CusEntryLine>)entryLinesRelatedToMainPack);
		});
		CachedProperty<(IDictionary<CusEntryLine, HashSet<CusEntryLine>> MainPackRelatedEntryLines, ISet<CusEntryLine> EntryLnesRelatedToMainPack)> packageRelatedEntryLinesCached;

		public ZBool HasExportPresentationMessage => Factory.GetValue(ref hasOutgoingExportPresentationMessageCached, () =>
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.IECustomsExport);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, AESOutgoingMessageTypeList.Codes.ExportPresentation);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			return Factory.ExistsInDatabase(EDIMessage.Schema.TableName, query);
		});
		CachedProperty<ZBool> hasOutgoingExportPresentationMessageCached;

		public ZBool HasMultipleExportersViaLines => Factory.GetValue(ref hasMultipleExportersViaLinesCached, () =>
		{
			return MergedLines.Cast<CusEntryLine>()
				.Select(x => x.Consignor)
				.Where(x => GetMatchedOrgAddressOrNullIfUnmatched(x) != null)
				.Distinct()
				.IsCountMoreThan(1);
		});
		CachedProperty<ZBool> hasMultipleExportersViaLinesCached;

		static OrgAddress GetMatchedOrgAddressOrNullIfUnmatched(OrgAddress orgAddress)
		{
			var addressHeader = orgAddress?.Header;
			return (addressHeader == null || addressHeader.PK == Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation) ? null : orgAddress;
		}

		public bool HasInvoiceExporterAddress => Factory.GetValue(ref hasInvoiceExporterAddressCached, () => InvoiceHeaders.Any(x => x.ExporterAddress != null));
		CachedProperty<bool> hasInvoiceExporterAddressCached;

		protected override bool IsStatusChangedToClearedForLoggingCLREvent => CH_EntryStatus == EntryStatusList.Codes.Clear || CH_EntryStatus == AESEntryStatusList.Codes.ReleasedForExport;

		protected override bool ShouldLogCustomsClearedToEntryHeader => false; // to be logged on declaration only

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment => !Declaration.CustomsEntryHeaders.Any(x => x.CH_EntryStatus != EntryStatusList.Codes.Clear && x.CH_EntryStatus != AESEntryStatusList.Codes.ReleasedForExport);

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			LogLogicalStatusIfRequired();
			base.OnFactorySavingBeforeTransactionCore();
		}

		void LogLogicalStatusIfRequired()
		{
			if (!PK.IsEmpty)
			{
				if ((ZString)CH_StatusInfo.OriginalValue != CH_Status)
				{
					LogLogicalStatus();
				}
			}
		}

		void LogLogicalStatus()
		{
			Logs.AddNew(Events.MessageStatusChange, LogicalStatusEventReference, ZDateTimeOffset.Now);
		}

		ZString LogicalStatusEventReference => CH_Status;

		#region IAISMessageAttachee Members

		GlbStaff IMessageAttachee.CustomsAgent => Declaration?.CusAgent;
		IRelatedJob IMessageAttachee.RelatedJob => Declaration;
		ZString IMessageAttachee.LogicalStatus
		{
			get => CH_Status;
			set => CH_Status = value;
		}
		ZString IMessageAttachee.EntryStatus
		{
			get => CH_EntryStatus;
			set => CH_EntryStatus = value;
		}
		IEnumerable<Enterprise.Messaging.Business.EDIMessage> IMessageAttachee.Messages => Messages.Cast<Enterprise.Messaging.Business.EDIMessage>();

		void IAISMessageAttachee.SetCustomsRegistrationNumber(ZString crn)
		{
			CRN = crn;
		}

		void IAISMessageAttachee.SetEntryReleaseDate(ZDateTime releaseDate)
		{
			CH_EntryReleaseDate = releaseDate;
		}

		IRequestedDocumentsProvider IAISMessageAttachee.RequestedDocumentsProvider => EntryInstruction;

		void IAISMessageAttachee.PopulateConfirmedDutiesAndTaxes(IEnumerable<IGoodsItemProvider> goodsItems)
		{
			var linesDictionary = MergedLines.Cast<CusEntryLine>().ToDictionary(x => x.CL_LineNumber.ToString(), x => x);
			foreach (var goodsItem in goodsItems)
			{
				if (linesDictionary.TryGetValue(goodsItem.DeclarationGoodsItemNumber, out var cusEntryLine))
				{
					cusEntryLine.ConfirmedFees.RemoveAndDeleteAll();
					MessageProcessorHelper.PopulateConfirmedDutiesAndTaxes(cusEntryLine, goodsItem.TaxTypes);
				}
			}
		}

		#endregion
	}
}
