using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public static class MessageProcessorHelper
{
	public static EDIMessage GetOutgoingMessageFromSessionId(this BusinessObjectFactory factory, ZGuid sessionId)
	{
		EDIMessage outgoingMessage = null;

		if (sessionId.IsValid)
		{
			var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sessionId);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeSubQuery, JoinCondition.And);
			outgoingMessage = factory.LoadTop1<EDIMessage>(messageQuery);
		}
		return outgoingMessage;
	}

	public static EDIMessage GetIncomingMessageFromApplicationReference(this BusinessObjectFactory factory, ZString applicationCode, ZString applicationReference, string messageType = null, string messageSubType = null)
	{
		return GetMessageFromApplicationReference(factory, applicationCode, applicationReference, EDIMessage.Direction.Receive, messageType, messageSubType);
	}

	public static EDIMessage GetOutgoingMessageFromApplicationReference(this BusinessObjectFactory factory, ZString applicationCode, ZString applicationReference, string messageType = null, string messageSubType = null)
	{
		return GetMessageFromApplicationReference(factory, applicationCode, applicationReference, EDIMessage.Direction.Transmit, messageType, messageSubType);
	}

	public static EDIMessage GetMessageFromApplicationReference(this BusinessObjectFactory factory, ZString applicationCode, ZString applicationReference, string direction, string messageType, string messageSubType)
	{
		var messageQuery = new ZQuery();
		messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCode);
		messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, applicationReference);
		messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, direction);

		if (!string.IsNullOrEmpty(messageType))
		{
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
		}

		if (!string.IsNullOrEmpty(messageSubType))
		{
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageSubType);
		}

		return factory.LoadTop1<EDIMessage>(messageQuery);
	}

	public static EDIInterchange GetOutgoingInterchangeFromSessionId(this BusinessObjectFactory factory, ZGuid sessionId)
	{
		var interchangeQuery = new ZQuery();
		interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sessionId);
		interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
		return factory.LoadTop1<EDIInterchange>(interchangeQuery);
	}

	public static BusinessObject FindCusEntryNumParent(GlbCompany company, ZString sourceEntryNum)
	{
		var factory = company.Factory;

		var firstDotIndex = sourceEntryNum.IndexOf('.');
		var mrn = firstDotIndex < 0 ? sourceEntryNum : sourceEntryNum.Substring(0, firstDotIndex);

		var branchesQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
		branchesQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);

		var cusInBondHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusEntryNumSchema.CE_ParentID);
		cusInBondHeaderQuery.AddSubQuery(branchesQuery, JoinCondition.And);

		var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
		jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_GC, company.PK);

		var cusEntryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryNumSchema.CE_ParentID);
		cusEntryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);

		var transitQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
		transitQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusInBondHeaderSchema.Constants.TableName);
		transitQuery.AddSubQuery(cusInBondHeaderQuery, JoinCondition.And);

		var importExportQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
		importExportQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
		importExportQuery.AddSubQuery(cusEntryHeaderQuery, JoinCondition.And);

		var query = new ZQuery(transitQuery, JoinCondition.Or, importExportQuery);
		query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Switzerland);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		var mrnQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrn);
		mrnQuery.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, mrn + ".");
		query.AddToFilter(mrnQuery);

		query.OrderBy = CusEntryNumSchema.Constants.CE_SystemCreateTimeUtc + OrderByClause.Descending;

		return factory.LoadTop1<CusEntryNumber>(query)?.Parent;
	}

	public static ZString ToZString(this string str, int maxLength) => new ZString(str).Left(maxLength);

	public static ZDateTime UtcToLocalBranchTime(this DateTime datetime) => new ZDateTime(datetime, DateTimeKind.Utc).ToLocalBranchTime();

	public static ZString AppendEntryNumVersion(this string mrnOrGdrn, string version) => $"{mrnOrGdrn}.{version}";
}
