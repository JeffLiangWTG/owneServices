using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
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

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case CusEntryHeader.Schema.GSTAmount:
						Factory.AddFetchHint(CusEntryLineSchema.CL_CH, BusinessObject.PK);
						break;
					case CusEntryHeader.Schema.TransitPermitCount:
						Factory.AddFetchHint(CusEntryInstructionSchema.PK, BusinessObject.CH_CEI_Instruction);
						Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, BusinessObject.CH_CEI_Instruction);
						break;
					case CusEntryHeader.Schema.Completed:
					case CusEntryHeader.Schema.EarliestExpiryDate:
					case CusEntryHeader.Schema.Expired:
					case CusEntryHeader.Schema.TransitPermitInTransitCount:
						Factory.AddFetchHint(CusEntryInstructionSchema.PK, BusinessObject.CH_CEI_Instruction);
						Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, BusinessObject.CH_CEI_Instruction);
						Factory.AddFetchHint(CusInBondMoveHeaderSchema.BM_BH, BusinessObject.EntryInstruction?.CusInBondPermitsHeaders.header.PK ?? ZGuid.Empty);
						break;
				}
			}
			base.FetchForViewCore(columns);
		}
	}
}
