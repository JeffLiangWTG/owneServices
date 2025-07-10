using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	internal class GridColourSchemeValidation : StmModuleFilterValidation
	{
		public GridColourSchemeValidation(GridColourScheme scheme)
			: base(scheme)
		{
			Scheme = scheme;
		}

		readonly GridColourScheme Scheme;

		protected override void CheckS9_IsPublished()
		{
			if (Scheme.S9_IsPublished)
			{
				var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, Scheme.S9_FilterName);
				query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, Scheme.S9_ModuleID);
				query.AddToFilter(StmModuleFilterSchema.S9_IsPublished, true);
				query.AddToFilter(StmModuleFilterSchema.PK, SQLComparisonOperator.NotEqual, Scheme.PK);

				if (Scheme.Factory.Load<GridColourScheme>(query).Length > 0)
				{
					Scheme.S9_IsPublishedInfo.AddError(Res.GetString("18d03455-ce12-42bd-ac02-6f5f0a0a46ba", "Another scheme with the same name is already published for this module"));
				}
			}
		}

		protected override void CheckS9_FilterName()
		{
			base.CheckS9_FilterName();

			if (Scheme.S9_FilterName.Length == 0)
			{
				Scheme.S9_FilterNameInfo.AddError(Res.GetString("d7817eb1-3af4-46b3-a86a-d8935e2ec8fb", "Please enter a scheme name."));
			}
			else
			{
				var nameQuery = new ZQuery(StmModuleFilterSchema.S9_FilterName, Scheme.S9_FilterName);
				nameQuery.AddToFilter(StmModuleFilterSchema.PK, SQLComparisonOperator.NotEqual, Scheme.PK);
				var query = new ZQuery(StmModuleFilterSchema.S9_ModuleID, Scheme.S9_ModuleID);
				query.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);
				query.AddToFilter(nameQuery);

				var ownerQuery = new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, EnvProxy.Instance.CurrentUser.PK);
				ownerQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsPublished, true);
				ownerQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsSystem, true);

				query.AddToFilter(ownerQuery);
				var sameNameScheme = Scheme.Factory.LoadTop1<StmModuleFilter>(query);

				if (sameNameScheme != null)
				{
					Scheme.S9_FilterNameInfo.AddError(Res.GetString("068c62b0-791b-461d-aa5f-0626bbb84871", "This scheme name is already used, please enter a different scheme name."));
				}

				if (Scheme.S9_FilterName.StartsWith("[") || Scheme.S9_FilterName.EndsWith("]"))
				{
					Scheme.S9_FilterNameInfo.AddError(Res.GetString("0c7d77bf-7460-4e98-bf40-650a762aedcd", "The scheme name cannot start or end with square brackets."));
				}

				if (Scheme.S9_FilterName.EndsWith("*"))
				{
					Scheme.S9_FilterNameInfo.AddError(Res.GetString("d0e32676-3761-46f9-9ab0-e2667d9974e4", "The scheme name cannot end with *."));
				}
			}

			if (Scheme.ColourStrips.Count == 0)
			{
				Scheme.S9_FilterNameInfo.AddError(Res.GetString("e5486cd5-c722-405a-b6a9-5af9c2dd218c", "A color scheme needs to contain at least one rule."));
			}
		}

		#region PublishAcrossAllCompanies

		public void ValidatePublishAcrossAllCompanies()
		{
			ValidateCalculatedProperty(((GridColourScheme)Parent).PublishAcrossAllCompaniesInfo);
		}

		public void ValidateGridColourStrips()
		{
			Scheme.RemoveRowError(GridColourStripsErrorMessage);
			foreach (var colourStrip in Scheme.ColourStrips)
			{
				colourStrip.RunPreSaveValidation();
				if (colourStrip.HasErrors)
				{
					Scheme.AddRowError(GridColourStripsErrorMessage);
					break;
				}
			}
		}

		protected virtual void CheckPublishAcrossAllCompanies()
		{
			if (((GridColourScheme)Parent).PublishAcrossAllCompanies)
			{
				var publishGlobalGridColourSchemes = EnvProxy.Instance.Security.PublishGlobalGridColorSchemes;
				if (!publishGlobalGridColourSchemes.IsAllowedForAllBranches)
				{
					((GridColourScheme)Parent).PublishAcrossAllCompaniesInfo.AddError(publishGlobalGridColourSchemes.ErrorMessageForNotAllowed);
				}
			}
		}

		internal static string GridColourStripsErrorMessage => Res.GetString("AA027143-3C8A-4650-9C9B-618FDE7D4B46", "There are some errors in search rules, please correct these before saving.");

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePublishAcrossAllCompanies();
			ValidateGridColourStrips();
		}
	}
}
