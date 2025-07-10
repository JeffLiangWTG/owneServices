using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentAcceptabilityBandValidation : AutoBMComponentAcceptabilityBandValidation
	{
		public BMComponentAcceptabilityBandValidation(AutoBMComponentAcceptabilityBand parent)
			: base(parent)
		{
		}

		new BMComponentAcceptabilityBand Parent
		{
			get { return (BMComponentAcceptabilityBand)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateFilterStrips();
		}

		#region BAB_Name

		protected override void CheckBAB_Name()
		{
			base.CheckBAB_Name();

			MandatoryValidation.CheckEntered(Parent.BAB_NameInfo);
		}

		#endregion

		#region BAB_FC_Component

		protected override void CheckBAB_FC_Component()
		{
			base.CheckBAB_FC_Component();

			ListValidation.ErrorIfInvalidPK(Parent.BAB_FC_ComponentInfo);
		}

		#endregion

		#region Boundary Values

		protected override void CheckBAB_CautionLowerBound()
		{
			base.CheckBAB_CautionLowerBound();
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.BAB_CautionLowerBoundInfo, Parent.BAB_GoodLowerBoundInfo);
		}

		protected override void CheckBAB_GoodLowerBound()
		{
			base.CheckBAB_GoodLowerBound();
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.BAB_GoodLowerBoundInfo, Parent.BAB_ExcellentLowerBoundInfo);
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.BAB_GoodLowerBoundInfo, Parent.BAB_CautionLowerBoundInfo);
		}

		protected override void CheckBAB_ExcellentLowerBound()
		{
			base.CheckBAB_ExcellentLowerBound();
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.BAB_ExcellentLowerBoundInfo, Parent.BAB_ExcellentUpperBoundInfo);
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.BAB_ExcellentLowerBoundInfo, Parent.BAB_GoodLowerBoundInfo);
		}

		protected override void CheckBAB_ExcellentUpperBound()
		{
			base.CheckBAB_ExcellentUpperBound();
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.BAB_ExcellentUpperBoundInfo, Parent.BAB_ExcellentLowerBoundInfo);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.BAB_ExcellentUpperBoundInfo, Parent.BAB_GoodUpperBoundInfo);
		}

		protected override void CheckBAB_GoodUpperBound()
		{
			base.CheckBAB_GoodUpperBound();
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.BAB_GoodUpperBoundInfo, Parent.BAB_ExcellentUpperBoundInfo);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.BAB_GoodUpperBoundInfo, Parent.BAB_CautionUpperBoundInfo);
		}

		protected override void CheckBAB_CautionUpperBound()
		{
			base.CheckBAB_CautionUpperBound();
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.BAB_CautionUpperBoundInfo, Parent.BAB_GoodUpperBoundInfo);
		}

		#endregion

		#region BAB_Type

		protected override void CheckBAB_Type()
		{
			base.CheckBAB_Type();

			MandatoryValidation.CheckEntered(Parent.BAB_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BAB_TypeInfo);
		}

		#endregion

		#region BAB_SqlText

		protected override void CheckBAB_SqlText()
		{
			base.CheckBAB_SqlText();
			if (!Parent.IsSqlDisabled && Parent.Lookups.Types.ContainsCode(Parent.BAB_Type))
			{
				MandatoryValidation.CheckEntered(Parent.BAB_SqlTextInfo);

				if (!Parent.BAB_SqlText.IsEmpty)
				{
					var parameters = new AcceptabilityBandSqlBuilderParameters(Parent);
					using (var completeCommand = GetCommandForColumnValidation(parameters))
					{
						parameters.IsQueryForValidation = true;
						using (var commandColumnValidation = GetCommandForColumnValidation(parameters))
						{
							//TODO: Change the SQL validation, avoid CheckValidStatementWithOptionalColumns because it cause: Deprecated feature: SET FMTONLY ON; -> https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/166254
							SqlValidation.CheckValidStatementWithOptionalColumns(Parent.BAB_SqlTextInfo, completeCommand, commandColumnValidation, sqlTextRequiredColumns, sqlTextOptionalColumns);

							if (!Parent.IsFilterStripsDisabled && !Regex.IsMatch(Parent.BAB_SqlText, string.Format(CultureInfo.InvariantCulture, "from\\s+{0}", BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName), RegexOptions.IgnoreCase))
							{
								Parent.BAB_SqlTextInfo.AddError(Res.GetString("d36e397b-a981-4c2d-b80b-3f7ae13557dd", "Must select data from the '{0}' result set.", BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName));
							}
						}
					}
				}

				var hasChanges = Parent.BAB_SqlTextInfo.HasChanges || (!Parent.IsInDatabase && !Parent.BAB_SqlText.IsEmpty);

				if (hasChanges && !Env.Security.AcceptabilityBandModifyQuery.IsAllowed)
				{
					Parent.BAB_SqlTextInfo.AddError(Res.GetString("ca54b30b-9b55-4fa0-800b-cdfc44c04b20", "You do not have permission to modify Acceptability Band SQL."));
				}
			}
		}

		internal DbCommand GetCommandForColumnValidation()
		{
			var parameters = new AcceptabilityBandSqlBuilderParameters(Parent)
			{
				IsQueryForValidation = true,
			};
			return GetCommandForColumnValidation(parameters);
		}

		DbCommand GetCommandForColumnValidation(AcceptabilityBandSqlBuilderParameters parameters)
		{
			return Parent.GetAcceptabilityBandSqlCommand(parameters, Db.NewExtraConnectionToMainDbWithReaderCredentials());
		}

		readonly string[] sqlTextRequiredColumns = { BMComponentAcceptabilityBand.ValueColumnName, BMComponentAcceptabilityBand.ComponentColumnName, BMComponentAcceptabilityBand.ReleaseGroupColumnName };
		readonly string[] sqlTextOptionalColumns = { BMComponentAcceptabilityBand.AdditionalAggregatorColumnName };

		#endregion

		#region BAB_FiltersBySection

		protected override void CheckBAB_FiltersBySection()
		{
			base.CheckBAB_FiltersBySection();

			if (Parent.BAB_FiltersBySection)
			{
				if (Parent.BAB_Type == AcceptabilityBandTypes.Codes.SQL)
				{
					Parent.BAB_FiltersBySectionInfo.AddError(Res.GetString("8c3dc523-c83f-433b-bf31-efd3f493b4ca", "Filtering by board section cannot be used for SQL Acceptability Bands. Please use AGR instead."));
				}
			}
		}

		#endregion

		#region ValidateFilterStrips

		void ValidateFilterStrips()
		{
			RelatedModuleFiltersHelper.ValidateFilterStrips(Parent.FilterRule);
			RelatedModuleFiltersHelper.ValidateFilterStrips(Parent.SupersetItemsFilterRule);
		}

		#endregion
	}
}
