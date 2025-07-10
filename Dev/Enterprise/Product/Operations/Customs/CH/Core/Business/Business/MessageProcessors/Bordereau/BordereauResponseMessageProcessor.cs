using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public sealed class BordereauResponseMessageProcessor : BaseResponseMessageProcessor<IEdecBordereauDetail>
{
	public BordereauResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("CBEC3EDC-B06E-4A72-8B29-D0AD75150C79", "Customs Bordereau Message Response Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.BOR };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.BordereauResponse };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IEdecBordereauDetail xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IEdecBordereauDetail customsResponse)
	{
		var factory = message.Factory;

		if (message.EM_LinkedObject is GlbCompany company)
		{
			using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
			{
				var documentNumber = customsResponse.DocumentInformation.DocumentNumber;
				var documentDate = new ZDate(customsResponse.DocumentInformation.DocumentDateTime);
				var accountNumber = customsResponse.Account?.AccountNumber ?? string.Empty;
				if (!HasCustomsSummaryHeader(factory, documentNumber, documentDate))
				{
					var customsOffices = customsResponse.Details;
					var messageDetails = customsOffices.SelectMany(x => x.Detail);
					foreach (var detail in messageDetails)
					{
						var hasEDIMessage = HasCustomsOfficeDetailAnEDIMessage(factory, detail);
						var entryNumParent = FindEntryNumParent(message.Factory, detail.CustomsReference);
						if (entryNumParent != null && !hasEDIMessage)
						{
							CreateEvvRequest(entryNumParent, detail);
						}
					}
					CreateCustomsSummaryHeaderAndDetails(factory, documentNumber, documentDate, accountNumber, customsOffices);
				}
			}
		}
	}

	void CreateEvvRequest(IEDIMessageCollectionOwner entryNumParent, IBordereauCustomsOfficeDetail detail)
	{
		var messageSubType = BordereauChargeTypeList.GetMessageSubType(detail.DocumentType.DocumentTypeAbbreviation);
		var sendingObject = new EvvRequestSendingObject(entryNumParent, detail.CustomsReference, ZInt.ParseSafe(detail.CustomsDeclarationVersion, 0), messageSubType);
		var messageManager = MessageManagerFactory.CreateNew(sendingObject);
		if (messageManager != null)
		{
			entryNumParent.Messages.AddRange(messageManager.GenerateMessages());
		}
	}

	bool HasCustomsSummaryHeader(BusinessObjectFactory factory, ZString documentNumber, ZDate documentDate)
	{
		return new CustomsSummaryHeaderLoader(factory).LoadBorderau(documentNumber, documentDate) != null;
	}

	void CreateCustomsSummaryHeaderAndDetails(BusinessObjectFactory factory, ZString documentNumber, ZDate documentDate, ZString accountNumber, IEnumerable<IBordereauCustomsOffice> customsOffices)
	{
		var summaryHeader = factory.New<CustomsSummaryHeader>();
		summaryHeader.B2_StatementNumber = documentNumber;
		summaryHeader.B2_ProcessDate = documentDate;
		summaryHeader.B2_AccountNo = accountNumber;
		summaryHeader.B2_GC = GlbCompany.CurrentCompany.PK;

		var foundCustomsEntryNumbers = new Dictionary<string, bool>();

		foreach (var customsOffice in customsOffices)
		{
			foreach (var detail in customsOffice.Detail)
			{
				var summaryLine = summaryHeader.SummaryLines.AddNew();
				summaryLine.B3_EntryNum = $"{detail.CustomsReference}.{detail.CustomsDeclarationVersion}";
				summaryLine.B3_BrokerReference = detail.TraderReference ?? ZString.Empty;
				summaryLine.B3_Status = DecideSummaryLineStatus();
				summaryLine.LineCharge.B4_ChargeType = detail.DocumentType.DocumentTypeAbbreviation;
				summaryLine.LineCharge.B4_ReferenceNumber = customsOffice.CustomsOfficeNumber;
				summaryLine.LineCharge.B4_ChargeAmount = detail.Amount;

				string DecideSummaryLineStatus()
				{
					if (HasCustomsOfficeDetailAnEDIMessage(factory, detail))
					{
						return BordereauReceivedStatusList.Codes.Received;
					}

					return ExistsCustomsEntryNumber(CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(summaryLine.B3_EntryNum)) ? BordereauReceivedStatusList.Codes.Sent : BordereauReceivedStatusList.Codes.Skipped;
				}
			}

			bool ExistsCustomsEntryNumber(string entryNumberNoVersion)
			{
				if (!foundCustomsEntryNumbers.ContainsKey(entryNumberNoVersion))
				{
					foundCustomsEntryNumbers.Add(entryNumberNoVersion, factory.LoadTop1<CusEntryNumber>(CusEntryNumberHelper.GetEntryNumberQueryForHeaderOrShipment(entryNumberNoVersion)) != null);
				}
				return foundCustomsEntryNumbers[entryNumberNoVersion];
			}
		}
	}

	bool HasCustomsOfficeDetailAnEDIMessage(BusinessObjectFactory factory, IBordereauCustomsOfficeDetail detail)
	{
		var query = new ZQuery();
		query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeCodeList.Codes.EVV);
		query.AddToFilter(EDIMessageSchema.EM_MessageSubType, BordereauChargeTypeList.GetMessageSubType(detail.DocumentType.DocumentTypeAbbreviation));
		query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
		query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, detail.CustomsReference + "." + ZInt.ParseSafe(detail.CustomsDeclarationVersion, 0));
		return factory.LoadTop1<EDIMessage>(query) != null;
	}

	IEDIMessageCollectionOwner FindEntryNumParent(BusinessObjectFactory factory, ZString mrn)
	{
		var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryNumSchema.CE_ParentID);
		var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
		entryHeaderSubQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);
		var branchesSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
		branchesSubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
		declarationSubQuery.AddSubQuery(branchesSubQuery, JoinCondition.And);

		var query = new ZDBOnlyQuery(typeof(CusEntryNumber));
		query.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);
		query.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);

		query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Switzerland);
		query.AddToFilter(new ZQuery(CusEntryNumSchema.CE_EntryNum, mrn).AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, mrn + "."));
		query.AddToFilter(new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber).AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName));

		var entryNum = factory.LoadTop1<CusEntryNumber>(query);
		return entryNum?.Parent as IEDIMessageCollectionOwner;
	}
}
