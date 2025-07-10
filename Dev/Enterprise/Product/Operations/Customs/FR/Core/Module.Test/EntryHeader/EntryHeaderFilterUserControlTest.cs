using System.Collections.Generic;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Module.EntryHeader.Testing
{
	public class EntryHeaderFilterUserControlTest : EU.Module.Testing.EntryHeaderFilterUserControlTest
	{
		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>()
				{
					EntryHeaderFilterUserControl.Schema.CustomsProfile,
					EntryHeaderFilterUserControl.Schema.DeltaMode,
					"IsDeltaDStepOneSentOK",
					"IsDeltaDStepTwoSentOK",
					"IsDeltaDStepTwoSentOKButZeroLiquidation",
					CusEntryHeader.Schema.FRCustomsFallbackNumber,
					"DeltaGFallbackStatus",
					"AssessmentDate",
					CusEntryHeader.Schema.CH_TriggeringPointForValidation,
					EntryHeaderFilterUserControl.Schema.ExportExitType,
					CusEntryHeader.Schema.CorrelationID
				};
				list.AddRange(base.FilteredGridColumns);
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				var list = base.ColumnNamesInSortOrder;
				list.AddRange(new List<string>()
				{
					EntryHeaderFilterUserControl.Schema.CustomsProfile,
					EntryHeaderFilterUserControl.Schema.DeltaMode,
					"IsDeltaDStepOneSentOK",
					"IsDeltaDStepTwoSentOK",
					"IsDeltaDStepTwoSentOKButZeroLiquidation",
					CusEntryHeader.Schema.FRCustomsFallbackNumber,
					"DeltaGFallbackStatus",
					"AssessmentDate",
					CusEntryHeader.Schema.CH_TriggeringPointForValidation,
					EntryHeaderFilterUserControl.Schema.ExportExitType,
					CusEntryHeader.Schema.CorrelationID
				});
				return list;
			}
		}

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(jobDeclaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}
	}
}
