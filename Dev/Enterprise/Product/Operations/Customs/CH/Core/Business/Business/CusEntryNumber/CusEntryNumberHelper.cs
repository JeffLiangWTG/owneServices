using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public static class CusEntryNumberHelper
{
	public static string MovementReferenceNumberWithoutVersion(ZString movementReferenceNumber)
	{
		var indexOfDot = movementReferenceNumber.IndexOf('.');
		return indexOfDot < 0 ? movementReferenceNumber : movementReferenceNumber.Left(indexOfDot);
	}

	public static int? MovementReferenceNumberVersion(ZString movementReferenceNumber)
	{
		var indexOfDot = movementReferenceNumber.IndexOf('.');
		return indexOfDot < 0 ? null : (int?)ZInt.ParseSafe(movementReferenceNumber.SubstringSafe(indexOfDot + 1), 0);
	}

	public static ZQuery GetEntryNumberQueryForHeaderOrShipment(string entryNum)
	{
		var anyVersionEntryNum = MovementReferenceNumberWithoutVersion(entryNum);

		var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
		declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.Select(b => b.PK));
		var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryNumSchema.CE_ParentID);
		entryHeaderQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);

		var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
		jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GB, GlbCompany.CurrentCompany.Branches.Select(b => b.PK));
		var shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), CusEntryNumSchema.CE_ParentID);
		shipmentSubQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);

		var entryNumberQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
		entryNumberQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
		entryNumberQuery.AddSubQuery(shipmentSubQuery, JoinCondition.Or);
		entryNumberQuery.AddToFilter(new ZQuery(CusEntryNumSchema.CE_EntryNum, anyVersionEntryNum).AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, $"{anyVersionEntryNum}."));
		entryNumberQuery.AddToFilter(new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber).AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName));
		entryNumberQuery.OrderBy = CusEntryNumSchema.Constants.CE_ParentTable;

		return entryNumberQuery;
	}
}
