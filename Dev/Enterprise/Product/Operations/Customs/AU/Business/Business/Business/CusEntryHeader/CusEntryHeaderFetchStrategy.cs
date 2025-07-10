using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business;
class CusEntryHeaderFetchStrategy : Customs.Business.FetchStrategies.CusEntryHeaderFetchStrategy
{
	public CusEntryHeaderFetchStrategy(CusEntryHeader entry)
		: base(entry)
	{
	}

	protected new CusEntryHeader BusinessObject
	{
		get { return (CusEntryHeader)base.BusinessObject; }
	}

	protected override void FetchForLoadCore()
	{
		base.FetchForLoadCore();
		Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
		Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
	}

	protected override void FetchForViewCore(TableColumn[] columns)
	{
		base.FetchForViewCore(columns);

		foreach (var column in columns)
		{
			var columnName = column.ColumnName;
			switch (columnName)
			{
				case CusEntryHeader.Schema.AQISContainerCharges:
				case CusEntryHeader.Schema.AQISProcessingCharge:
				case CusEntryHeader.Schema.AQISServicePaymentAmount:
				case CusEntryHeader.Schema.DeclarationProcessingCharge:
				case CusEntryHeader.Schema.EntryFee:
				case CusEntryHeader.Schema.MessageFee:
				case CusEntryHeader.Schema.OtherEntryCharge:
				case CusEntryHeader.Schema.ScreenFreeCharge:
				case CusEntryHeader.Schema.TotalAmountPayable:
				case CusEntryHeader.Schema.TotalPayableAdmin:
				case CusEntryHeader.Schema.TradegateGST:
				case CusEntryHeader.Schema.WoodLevy:
				case CusEntryHeader.Schema.WoodLevyIncludingWHEstimate:
					Factory.AddFetchHint(CusEntryHeaderChargesSchema.C1_ClusterKey, BusinessObject.CH_ClusterKey);
					break;
				case CusEntryHeader.Schema.ATDSecurityCode:
					Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
					break;
				case CusEntryHeader.Schema.CustomsFactor:
				case CusEntryHeader.Schema.TAndI:
					Factory.AddFetchHint(CusEntryLineSchema.CL_ClusterKey, BusinessObject.CH_ClusterKey);
					break;
				case CusEntryHeader.Schema.DutyAmount:
				case CusEntryHeader.Schema.DutyAmountIncludingWHEstimate:
				case CusEntryHeader.Schema.GSTAmountIncludingWHEstimate:
				case CusEntryHeader.Schema.LCTAmount:
				case CusEntryHeader.Schema.LCTAmountIncludingWHEstimate:			
				case CusEntryHeader.Schema.TotalSecurityConcession:
				case CusEntryHeader.Schema.TotalSecurityLiability:
				case CusEntryHeader.Schema.WETAmount:
				case CusEntryHeader.Schema.WETAmountIncludingWHEstimate:
					Factory.AddFetchHint(CusEntryLineSchema.CL_ClusterKey, BusinessObject.CH_ClusterKey);
					Factory.AddFetchHint(CusEntryLineFeeSchema.CF_ClusterKey, BusinessObject.CH_ClusterKey);
					break;
			}
		}
	}
}
