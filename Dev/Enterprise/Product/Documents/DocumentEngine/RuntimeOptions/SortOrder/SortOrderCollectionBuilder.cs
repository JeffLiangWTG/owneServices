using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class SortOrderCollectionBuilder
	{
		public SortOrderCollectionBuilder(ExcelWorkSheet sortSheet)
		{
			Sheet = sortSheet;
		}

		public void Build()
		{
			Errors.Clear();
			SortOrderCollection.Clear();
			try
			{
				Root = new StringTreeBuilder(Sheet).GetTree();
				BuildSortOrdersFromTree();
			}
			catch (TemplateDefinitionException ex)
			{
				Errors.Add(new ReportProcessingError(Res.GetString("7bedc1b1-546b-4407-8fb0-ee2daf867ebd", "Error Building Sort Orders: {0}", ex.Message),
									ex.CellReference, ReportProcessingErrorSeverity.Error));
			}
		}

		protected void BuildSortOrdersFromTree()
		{
			bool seenExplicitDefault = false;

			foreach (StringTreeNode sortOrderDef in Root.Children)
			{
				try
				{
					SortOrder newOrder = SortOrderCollection.Add(sortOrderDef.Value, sortOrderDef.Child().Value);
					if (sortOrderDef.Child().Children.Count == 1 && sortOrderDef.Child().Children[0].Value.Equals((NoResString)"default", StringComparison.OrdinalIgnoreCase))
					{
						if (!seenExplicitDefault)
						{
							newOrder.Selected = true;
							seenExplicitDefault = true;
						}
						else
						{
							throw new TemplateDefinitionException("Only one default sort order may be specified", sortOrderDef.Child().Children[0].CellReference);
						}
					}
				}
				catch (TemplateDefinitionException ex)
				{
					Errors.Add(new ReportProcessingError(Res.GetString("d29e0fc6-60e9-42e7-b979-fbe3df2cfd54", "Error Building Sort Orders from Tree: {0}", ex.Message),
										ex.CellReference, ReportProcessingErrorSeverity.Error));
				}
			}

			if (SortOrderCollection.Count > 0 && !seenExplicitDefault)
			{
				SortOrderCollection[0].Selected = true;
			}

			SortOrderCollection.UpdateDefaultOrder();
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

		protected SortOrderCollection fSortOrderCollection = new SortOrderCollection();
		public SortOrderCollection SortOrderCollection
		{
			get
			{
				return HasErrors ? new SortOrderCollection() : fSortOrderCollection;
			}
		}
	}
}
