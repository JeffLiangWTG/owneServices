using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	class JobDeclarationFetchStrategy : EU.Business.FetchStrategies.JobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void FetchForBindCore()
		{
			base.FetchForBindCore();
			AddFetchHints();
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			AddFetchHints();
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			AddFetchHints();
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			AddFetchHints();
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			AddFetchHints();
		}

		void AddFetchHints()
		{
			Factory.AddFetchHint(CusAddInfoSchema.Instance, new ZQuery(CusAddInfoSchema.B7_Type, Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties), new ZQuery(CusAddInfoSchema.B7_ParentID, BusinessObject.PK));
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			var mainQueryRelatedDeclarations = new ZQuery(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
			Factory.AddFetchHint(GenPivotSchema.Instance, mainQueryRelatedDeclarations);
		}

		protected override bool IsCusEntryInstructionRelatedColumn(string columnName)
		{
			return base.IsCusEntryInstructionRelatedColumn(columnName) || columnName == JobDeclaration.Schema.JE_DeclarationType;
		}

		protected override bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			return base.IsCusEntryHeaderRelatedColumn(columnName) || columnName == JobDeclaration.Schema.ZG_ImportClearanceStatusICS
				|| columnName == JobDeclaration.Schema.JE_GBRouteOfEntry
				|| columnName == JobDeclaration.Schema.ZG_StyleOfEntrySOE
				|| columnName == JobDeclaration.Schema.JE_GBRouteOfEntryDescription
				|| columnName == JobDeclaration.Schema.ZG_StyleOfEntrySOEDescription
				|| columnName == JobDeclaration.Schema.ZG_ImportClearanceStatusICSDescription
				|| columnName == JobDeclaration.Schema.JE_EntryStatusDescription;
		}
	}
}
