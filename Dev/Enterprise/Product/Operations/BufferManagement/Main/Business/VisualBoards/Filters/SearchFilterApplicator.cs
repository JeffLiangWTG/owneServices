using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class SearchFilterApplicator : CardVisibilityFilter
	{
		public SearchFilterApplicator(SearchFilter filter)
		{
			this.filter = filter;
		}

		readonly SearchFilter filter;

		public override bool AllowMultiple
		{
			get { return filter.AllowMultiple; }
		}

		public override string FilterName
		{
			get { return filter.FilterName; }
		}

		public override bool RequiresUndo
		{
			get { return filter.RequiresUndo; }
			set { filter.RequiresUndo = value; }
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) =>
				{
					var task = cardContent.GetTask(factory);
					var workflow = cardContent.GetWorkflow(factory);
					if (task != null && workflow != null)
					{
						return IsMatchOnRow(task)
							|| IsMatchOnRow(workflow)
							|| workflow.Parent != null && IsMatchOnRow(((BusinessObject)workflow.Parent))
							|| IsMatchOnSearchableProperties(task, viewModel.Cache)
							|| IsMatchOnSearchableProperties(workflow, viewModel.Cache)
							|| IsMatchOnTags(cardContent)
							|| IsMatchOnVisibleProperties(cardContent);
					}
					else
					{
						return false;
					}
				};
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			cards.FetchForLoad(factory);
		}

		bool IsMatchOnTags(ICardContent cardContent)
		{
			var applicableTags = cardContent.ApplicableTagMagnitudes;

			if (applicableTags.Count == 0)
			{
				return false;
			}
			else
			{
				var tagMagnitudes = applicableTags.Select(s => cardContent.Definitions.MagnitudesCache[s]);
				var matchesTagCodeOrDescription = tagMagnitudes.Any(tm => filter.SearchRegex.IsMatch(tm.TGM_Code.ToString()))
					|| tagMagnitudes.Any(tm => filter.SearchRegex.IsMatch(tm.TGM_Description.ToString()));

				if (matchesTagCodeOrDescription)
				{
					return true;
				}
				else
				{
					return tagMagnitudes
						.Select(m => m.Definition)
						.Distinct()
						.Any(td => filter.SearchRegex.IsMatch(td.TGD_Code.ToString()));
				}
			}
		}

		bool IsMatchOnRow(BusinessObject bizo)
		{
			var row = ((INeedRow)bizo).Row;

			if (row == null)
			{
				return false;
			}

			for (int colIndex = 0; colIndex < row.ItemArray.Length; colIndex++)
			{
				var columnValue = row.ItemArray[colIndex];

				if (columnValue is bool)
				{
					bool boolValue = (bool)columnValue;
					if (boolValue && filter.SearchRegex.IsMatch(row.Table.Columns[colIndex].ColumnName))
					{
						return true;
					}
				}
				else
				{
					var stringValue = columnValue as string;
					if (stringValue != null && filter.SearchRegex.IsMatch(stringValue))
					{
						return true;
					}
				}
			}

			return false;
		}

		static string GetPropertyInfoString(BusinessObject bizo)
		{
			return bizo.GetType().Name;
		}

		static string GetPropertyCaptionString(PropertyInfo info)
		{
			return info.ReflectedType.Name + info.Name + "_Caption";
		}

		static string GetPropertyCaption(PropertyInfo info, PropertyCache cache)
		{
			return cache.GetCachedValue(ZGuid.Empty, GetPropertyCaptionString(info), () => DataBoundResourceStrings.GetDataForProperty(info).Caption);
		}

		bool IsMatchOnSearchableProperties(BusinessObject bizo, PropertyCache cache)
		{
			foreach (var property in GetSearchableProperties(bizo, cache))
			{
				if (property.PropertyType == typeof(ZBool))
				{
					if (filter.SearchRegex.IsMatch(GetPropertyCaption(property, cache)) && property.GetValue(bizo, null).Equals(true))
					{
						return true;
					}
				}
				else
				{
					var value = property.GetValue(bizo, null);
					if (value != null && filter.SearchRegex.IsMatch(value.ToString()))
					{
						return true;
					}
				}
			}

			return false;
		}

		static PropertyInfo[] GetSearchableProperties(BusinessObject bizo, PropertyCache cache)
		{
			return cache.GetCachedValue(ZGuid.Empty, GetPropertyInfoString(bizo), () =>
			{
				return bizo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(p => p.IsDefined(typeof(VisualBoardSearchableAttribute), true))
					.ToArray();
			});
		}

		protected virtual bool IsMatchOnVisibleProperties(ICardContent cardContent)
		{
			var factorylessCardContent = cardContent as FactorylessCardContent;

			if (factorylessCardContent != null)
			{
				foreach (var property in factorylessCardContent.CustomisedControlData.LineCaches)
				{
					var value = property.GetValue(factorylessCardContent);
					if (value != null && filter.SearchRegex.IsMatch(value.ToString()))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
