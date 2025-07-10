using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class GroupByCollectionBuilder
	{
		public GroupByCollectionBuilder(ExcelWorkSheet groupBySheet)
		{
			Sheet = groupBySheet;
		}

		public void Build()
		{
			Errors.Clear();
			GroupByCollection.Clear();

			try
			{
				Root = new StringTreeBuilder(Sheet).GetTree();
				BuildGroupBysFromTree();
			}
			catch (TemplateDefinitionException ex)
			{
				Errors.Add(new ReportProcessingError(Res.GetString("cda475af-8ebe-4e99-90f2-1475de2487fb", "Error Building {0}: {1}", "GroupBys", ex.Message)
									, ex.CellReference
									, ReportProcessingErrorSeverity.Error));
			}
		}

		protected void BuildGroupBysFromTree()
		{
			bool seenExplicitDefault = false;

			foreach (StringTreeNode groupByDef in Root.Children)
			{
				try
				{
					string fieldList = groupByDef.Child().Value;
					if (fieldList.Equals((NoResString)"none", StringComparison.OrdinalIgnoreCase))
					{
						fieldList = "";
					}

					GroupBy newOrder = GroupByCollection.Add(groupByDef.Value, fieldList);
					if (groupByDef.Child().Children.Count == 1 && groupByDef.Child().Children[0].Value.Equals((NoResString)"default", StringComparison.OrdinalIgnoreCase))
					{
						if (!seenExplicitDefault)
						{
							newOrder.Selected = true;
							seenExplicitDefault = true;
						}
						else
						{
							throw new TemplateDefinitionException("Only one default GroupBy may be specified", groupByDef.Child().Children[0].CellReference);
						}
					}
				}
				catch (TemplateDefinitionException ex)
				{
					Errors.Add(new ReportProcessingError(Res.GetString("685e9e13-2921-465d-b113-a22437755f34", "Error Building {0} from Tree: {1}", "GroupBys", ex.Message),
										ex.CellReference, ReportProcessingErrorSeverity.Error));
				}
			}

			if (GroupByCollection.Count > 0 && !seenExplicitDefault)
			{
				GroupByCollection[0].Selected = true;
			}

			GroupByCollection.UpdateDefaultGroupBy();
		}

		protected ExcelWorkSheet Sheet;
		protected StringTreeNode Root;

		protected List<IReportProcessingError> fErrors = new List<IReportProcessingError>();
		public List<IReportProcessingError> Errors
		{
			get
			{
				return fErrors;
			}
		}

		public bool HasErrors
		{
			get
			{
				return Errors.Count != 0;
			}
		}

		protected GroupByCollection fGroupByCollection = new GroupByCollection();
		public GroupByCollection GroupByCollection
		{
			get
			{
				return HasErrors ? new GroupByCollection() : fGroupByCollection;
			}
		}
	}
}
