using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Grid
{
	public class GridRowFinderBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GridRowFinderBusinessObject(ZGrid grid)
			: base()
		{
			this.Grid = grid;
			isDirty = true;
			grid.ListManager.ListChanged += new ListChangedEventHandler((o, e) =>
			{
				//ItemChanged on a non-readonly grid is raised both for changing selected row AND for changing a value, with as far as I can tell otherwise identical parameters too...
				//So we have to check if the row index is changing to decide if this is likely a 'row changed' or 'value changed' event.
				if (e.ListChangedType == ListChangedType.ItemChanged)
				{
					if (!Grid.ReadOnly && oldCurrentRowIndex == Grid.CurrentRowIndex && e.NewIndex == Grid.CurrentRowIndex)
					{
						InvalidateSearch();
					}
				}
				else
				{
					InvalidateSearch();
				}

				oldCurrentRowIndex = Grid.CurrentRowIndex;
			});
		}

		public event EventHandler OnBeforeSearch;

		int oldCurrentRowIndex;

		public void DoSearch(bool isNext = true, bool explicitlyDeselect = false)
		{
			//commit unsaved changes to 'Test to Search for' and any open grid edit
			OnBeforeSearch?.Invoke(this, new EventArgs());
			this.Grid.EndEdit();
			((IMenuParent)Grid).BeforeProcessingMenuItem();

			//rebuild cache and set initial currentIndex
			if (isDirty)
			{
				//PERFORMANCE: this part could be async to improve responsivity, if needed
				isDirty = false;
				currentIndex = -1;
				matchingRows = new List<int>();
				isRowMatching = new Dictionary<int, int>();

				for (var i = 0; i < Grid.ListManager.List.Count; ++i)
				{
					var bizO = Grid.ListManager.List[i] as BusinessObject;
					if (bizO != null && MatchesSearch(Grid.ListManager, bizO, i))
					{
						matchingRows.Add(i);
						isRowMatching.Add(i, matchingRows.Count - 1);
					}
				}

				if (!SearchFromSelectedRow && matchingRows.Count > 0)
				{
					currentIndex = isNext ? -1 : matchingRows.Count; //will be modified by 1 soon 
				}

				UpdateSearchLabel();
			}

			//edge case - no results
			if (matchingRows.Count == 0)
			{
				return;
			}

			//find next current row
			if (SearchFromSelectedRow)
			{
				var j = Grid.CurrentRowIndex;
				if (isRowMatching.ContainsKey(Grid.CurrentRowIndex))
				{
					currentIndex = isRowMatching[j] + (isNext ? 1 : -1);
				}
				else
				{
					//PERFORMANCE: if this is slow, we could cache the final result of doing this for each row when we build cache. But that's probably not needed? Row count should be in 10ks at most
					for (var i = 0; i < Grid.ListManager.List.Count; ++i)
					{
						if (isNext)
						{
							++j;
							if (j == Grid.ListManager.List.Count)
							{
								j -= Grid.ListManager.List.Count;
							}
						}
						else
						{
							--j;
							if (j < 0)
							{
								j += Grid.ListManager.List.Count;
							}
						}
						if (isRowMatching.ContainsKey(j))
						{
							currentIndex = isRowMatching[j];
							break;
						}
					}
				}
			}
			else
			{
				currentIndex = currentIndex + (isNext ? 1 : -1);
			}

			if (currentIndex < 0)
			{
				currentIndex += matchingRows.Count;
			}
			if (currentIndex >= matchingRows.Count)
			{
				currentIndex -= matchingRows.Count;
			}

			//on non-readonly grids you always get the 'new old rows stay selected' behaviour, not sure why.
			if (explicitlyDeselect || !Grid.ReadOnly)
			{
				Grid.UnSelectAll();
			}

			//select new current row
			if (SelectAllRows)
			{
				currentIndex = matchingRows.Count - 1;
				Grid.CurrentRowIndex = matchingRows[currentIndex]; //this de-selects previously selected rows UNLESS ctrl is held (or unless it's editable, then we need an explicit UnSelectAll();). not unit testable for some reason...
				foreach (var index in matchingRows)
				{
					Grid.Select(index);
				}
			}
			else
			{
				Grid.CurrentRowIndex = matchingRows[currentIndex]; //same as previous comment
				Grid.Select(Grid.CurrentRowIndex);
			}

			UpdateSearchLabel();
			return;
		}

		void UpdateSearchLabel()
		{
			SearchLabel = Res.GetString("ed1c50cd-ae1a-4806-ad7a-7ed2afd0c5ea", "{0} out of {1}", currentIndex + 1, matchingRows.Count);
		}

		bool MatchesSearch(CurrencyManager manager, BusinessObject bizO, int index)
		{
			foreach (var colStyle in Grid.TableStyles[0].GridColumnStyles.OfType<ZGridColumnStyle>())
			{
				if (colStyle != null && ShouldSearchColumn(colStyle))
				{
					object colValue = null;
					try
					{
						colValue = bizO[colStyle.MappingName];
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						colValue = colStyle.GetValueAsString(manager, index);
					}

					if (SearchByType(colValue, colStyle, bizO))
					{
						return true;
					}
				}
			}

			return false;
		}

		bool ShouldSearchColumn(ZGridColumnStyle colStyle) => (ColumnsToSearch[colStyle.HeaderText] != null && ColumnsToSearch[colStyle.HeaderText].Value);

		bool SearchByType(object colValue, ZGridColumnStyle colStyle, BusinessObject bizO)
		{
			if ((colValue is ZString || colValue is string) && colValue.ToString().Contains(TextToSearchFor, MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}

			if ((colValue is ZInt || colValue is int) && colValue.ToString().Equals(TextToSearchFor))
			{
				return true;
			}

			if (colValue is MultilingualString multilingualColValue)
			{
				//attempt both current language search and invariant-culture english search
				if (
					multilingualColValue.ToString(Res.CurrentLanguage).Contains(TextToSearchFor, MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)
					||
					multilingualColValue.ToString(Res.DefaultLanguage).Contains(TextToSearchFor, MatchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)
					 )
				{
					return true;
				}
			}
			else if (colValue is ZGuid || colValue is Guid || ZGuid.TryParse(colValue, out ZGuid zGuid))
			{
				var propertyInfo = ZPropertyInfoRetriever.GetZPropertyInfo(colStyle, bizO);
				if (propertyInfo == null)
				{
					return false;
				}
				var guidColCodeValue = RelatedBusinessObjectAttribute.GetCodeForGuid(propertyInfo);
				if (guidColCodeValue.Contains(TextToSearchFor, MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		internal int currentIndex;
		internal List<int> matchingRows;
		Dictionary<int, int> isRowMatching;

		readonly ZGrid Grid;

		internal bool isDirty;

		public void InvalidateSearch()
		{
			isDirty = true;
		}

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString TextToSearchFor
		{
			get { return fTextToSearchFor; }
			set
			{
				if (fTextToSearchFor != value)
				{
					InvalidateSearch();
					CheckMaximumLength(TextToSearchForInfo, value);
					SetNonPersistentPropertyValue(TextToSearchForInfo, ref fTextToSearchFor, value);
				}
			}
		}

		public ZPropertyInfo TextToSearchForInfo
		{
			get { return GetZPropertyInfo(nameof(TextToSearchFor)); }
		}

		ZString fTextToSearchFor;

		public ZBool MatchCase
		{
			get
			{
				return fMatchCase;
			}
			set
			{
				if (fMatchCase != value)
				{
					InvalidateSearch();
					SetNonPersistentPropertyValue(MatchCaseInfo, ref fMatchCase, value);
				}
			}
		}
		ZBool fMatchCase;

		public ZPropertyInfo MatchCaseInfo
		{
			get { return GetZPropertyInfo(nameof(MatchCase)); }
		}

		public ZBool SearchFromSelectedRow
		{
			get
			{
				return fSearchFromSelectedRow;
			}
			set
			{
				if (fSearchFromSelectedRow != value)
				{
					SetNonPersistentPropertyValue(SearchFromSelectedRowInfo, ref fSearchFromSelectedRow, value);
				}
			}
		}
		ZBool fSearchFromSelectedRow;

		public ZPropertyInfo SearchFromSelectedRowInfo
		{
			get { return GetZPropertyInfo(nameof(SearchFromSelectedRow)); }
		}

		public ZBool SelectAllRows
		{
			get
			{
				return fSelectAllRows;
			}
			set
			{
				if (fSelectAllRows != value)
				{
					SetNonPersistentPropertyValue(SelectAllRowsInfo, ref fSelectAllRows, value);
				}
			}
		}
		ZBool fSelectAllRows;

		public ZPropertyInfo SelectAllRowsInfo
		{
			get { return GetZPropertyInfo(nameof(SelectAllRows)); }
		}

		public ZString SearchLabel
		{
			get
			{
				return fSearchLabel;
			}
			set
			{
				if (fSearchLabel != value)
				{
					SetNonPersistentPropertyValue(SearchLabelInfo, ref fSearchLabel, value);
				}
			}
		}
		ZString fSearchLabel;

		public ZPropertyInfo SearchLabelInfo
		{
			get { return GetZPropertyInfo(nameof(SearchLabel)); }
		}

		public void PossiblyRefreshColumnsToSearch()
		{
			if (fColumnsToSearch == null)
			{ return; }
			var i = 0;
			foreach (DataGridColumnStyle colStyle in Grid.TableStyles[0].GridColumnStyles)
			{
				if (IsGridRowFindSupported(colStyle))
				{
					if (i >= fColumnsToSearch.Count || fColumnsToSearch[i].Description != colStyle.HeaderText)
					{
						RefreshColumnsToSearch();
						return;
					}
					++i;
				}
			}
		}

		void RefreshColumnsToSearch()
		{
			InvalidateSearch();
			fColumnsToSearch.Clear();

			if (Grid.TableStyles.Count > 0)
			{
				foreach (DataGridColumnStyle colStyle in Grid.TableStyles[0].GridColumnStyles)
				{
					if (IsGridRowFindSupported(colStyle))
					{
						fColumnsToSearch.AddNew(colStyle.HeaderText, true);
					}
				}
			}
		}

		public void ToggleSearchColumns()
		{
			InvalidateSearch();
			foreach (var column in ColumnsToSearch)
			{
				column.Value = !column.Value;
			}
		}

		public ZBoolDescriptionPairList ColumnsToSearch
		{
			get
			{
				if (fColumnsToSearch == null)
				{
					fColumnsToSearch = new ZBoolDescriptionPairList();

					RefreshColumnsToSearch();
				}

				fColumnsToSearch.OnListChanged += new ZBoolDescriptionPairChangedEventHandler((e) => { InvalidateSearch(); });
				fColumnsToSearch.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler((e) => { InvalidateSearch(); });

				return fColumnsToSearch;
			}
		}

		ZBoolDescriptionPairList fColumnsToSearch;

		internal virtual bool IsGridRowFindSupported(DataGridColumnStyle colStyle)
		{
			if (colStyle is ZGridColumnStyle zStyle && zStyle.IsSensitiveValue)
			{
				return false;
			}

			if (colStyle.PropertyDescriptor != null)
			{
				return supportedPropertyType.Contains(colStyle.PropertyDescriptor.PropertyType.Name);
			}

			if (Grid.ListManager != null && Grid.ListManager.Count > 0)
			{
				var propertyInfo = Grid.ListManager.List[0].GetType().GetProperty(colStyle.MappingName);
				if (propertyInfo != null)
				{
					var propertyType = propertyInfo.PropertyType;
					return supportedPropertyType.Contains(propertyType.Name);
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Supported property type")]
		readonly HashSet<string> supportedPropertyType = new HashSet<string> { "ZString", "string", "ZInt", "int", "ZGuid", "Guid", "MultilingualString" };
	}
}
