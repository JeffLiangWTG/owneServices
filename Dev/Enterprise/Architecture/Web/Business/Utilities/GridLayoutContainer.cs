using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public class GridLayoutContainer : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string SelectedAvailableColumn = "SelectedAvailableColumn";
			public const string SelectedLayoutColumn = "SelectedLayoutColumn";
		}

		#endregion

		public GridLayoutContainer(StringCollectionX allColumnHeaders, string currentLayout, string defaultLayout, string requiredColumns, string excludedGroupMembers)
			: base(new BusinessObjectFactory())
		{
			fExcludedGroupMembers = excludedGroupMembers;
			fRequiredColumns = requiredColumns;
			fAllColumns = GetAllColumns(allColumnHeaders, requiredColumns);
			fDefaultLayout = GetColumnsLayout(defaultLayout);
			fCurrentLayout = GetColumnsLayout(currentLayout);
			fLayoutForRollback = new GridLayoutElementCollection();
			fLayoutForRollback.AddRange(fCurrentLayout);
		}

		public string CurrentLayoutString
		{
			get { return LayoutToString(CurrentLayout); }
		}

		readonly string fExcludedGroupMembers;
		readonly string fRequiredColumns;

		#region Properties To Bind To

		[CargoWise.ComponentModel.MaxLength(250)]
		public ZString SelectedAvailableColumn
		{
			get { return fSelectedAvailableColumn; }
			set
			{
				if (fSelectedAvailableColumn != value)
				{
					CheckMaximumLength(SelectedAvailableColumnInfo, value);
					fSelectedAvailableColumn = value;
					SelectedAvailableColumnInfo.RefreshBinding();
				}
			}
		}
		ZString fSelectedAvailableColumn;

		public virtual ZPropertyInfo SelectedAvailableColumnInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedAvailableColumn); }
		}

		[CargoWise.ComponentModel.MaxLength(250)]
		public ZString SelectedLayoutColumn
		{
			get { return fSelectedLayoutColumn; }
			set
			{
				if (fSelectedLayoutColumn != value)
				{
					CheckMaximumLength(SelectedLayoutColumnInfo, value);
					fSelectedLayoutColumn = value;
					SelectedLayoutColumnInfo.RefreshBinding();
				}
			}
		}
		ZString fSelectedLayoutColumn;

		public virtual ZPropertyInfo SelectedLayoutColumnInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedLayoutColumn); }
		}

		#endregion

		#region Lists To Bind To List

		public GridLayoutElementCollection AvailableColumns
		{
			get
			{
				if (fAvailableColumns == null)
				{
					fAvailableColumns = GetAvailableColumns();
				}
				return fAvailableColumns;
			}
		}
		GridLayoutElementCollection fAvailableColumns;

		protected GridLayoutElementCollection GetAvailableColumns()
		{
			GridLayoutElementCollection result = new GridLayoutElementCollection();
			string[] excluded = fExcludedGroupMembers.Split(',');

			for (int i = 0; i < AllColumns.Count; i++)
			{
				GridLayoutElement element = AllColumns[i];

				if (!CurrentLayout.Contains(element) && !((IList)excluded).Contains(i.ToString()))
				{
					result.Add(element);
				}
			}
			result.Sort(GridLayoutElement.Schema.HeaderText, System.ComponentModel.ListSortDirection.Ascending);
			return result;
		}

		public GridLayoutElementCollection CurrentLayout
		{
			get { return fCurrentLayout; }
		}

		readonly GridLayoutElementCollection fCurrentLayout;

		#endregion

		#region All and Default Columns, Original Layout for rollback

		public GridLayoutElementCollection AllColumns
		{
			get { return fAllColumns; }
		}

		readonly GridLayoutElementCollection fAllColumns;

		public GridLayoutElementCollection DefaultLayout
		{
			get { return fDefaultLayout; }
		}

		readonly GridLayoutElementCollection fDefaultLayout;

		readonly GridLayoutElementCollection fLayoutForRollback;

		#endregion

		public void AddToLayout()
		{
			int columnNumber = -1;
			if (int.TryParse(SelectedAvailableColumn, out columnNumber))
			{
				GridLayoutElement elementToAdd = FindElement(AvailableColumns, columnNumber);
				if (elementToAdd != null)
				{
					int currentAvailable = ((IList)AvailableColumns).IndexOf(elementToAdd);
					CurrentLayout.Add(elementToAdd);

					SelectedLayoutColumn = elementToAdd.ColumnNumber.ToString();
					AvailableColumns.Remove(elementToAdd);

					SelectedAvailableColumn = GetAdjustedSelection(AvailableColumns, currentAvailable);
				}
			}
		}

		public void RemoveFromLayout()
		{
			int columnNumber = -1;
			if (CurrentLayout.Count > 1 && int.TryParse(SelectedLayoutColumn, out columnNumber) && columnNumber > 0)
			{
				GridLayoutElement elementToRemove = FindElement(CurrentLayout, columnNumber);
				if (elementToRemove != null)
				{
					int current = ((IList)CurrentLayout).IndexOf(elementToRemove);
					CurrentLayout.Remove(elementToRemove);

					AvailableColumns.Add(elementToRemove);
					AvailableColumns.Sort(GridLayoutElement.Schema.HeaderText, System.ComponentModel.ListSortDirection.Ascending);

					SelectedAvailableColumn = elementToRemove.ColumnNumber.ToString();

					SelectedLayoutColumn = GetAdjustedSelection(CurrentLayout, current);
				}
			}
		}

		public void RestoreDefault()
		{
			CurrentLayout.RemoveAll();
			CurrentLayout.AddRange(DefaultLayout);

			fAvailableColumns = null;
		}

		public void RollbackChanges()
		{
			CurrentLayout.RemoveAll();
			CurrentLayout.AddRange(fLayoutForRollback);

			fAvailableColumns = null;
		}

		public void MoveSelectedUp()
		{
			MoveSelected(MovingDirection.MoveUp);
		}

		public void MoveSelectedDown()
		{
			MoveSelected(MovingDirection.MoveDown);
		}

		ZString GetAdjustedSelection(GridLayoutElementCollection elements, int selectionIndex)
		{
			ZString result = ZString.Empty;

			if (elements.Count > 0)
			{
				result = selectionIndex < elements.Count ?
					elements[selectionIndex].ColumnNumber.ToString() :
					elements[elements.Count - 1].ColumnNumber.ToString();
			}

			return result;
		}

		protected enum MovingDirection
		{
			MoveUp,
			MoveDown
		}

		protected void MoveSelected(MovingDirection direction)
		{
			int columnNumber = -1;
			if (int.TryParse(SelectedLayoutColumn, out columnNumber))
			{
				GridLayoutElement elementToMove = FindElement(CurrentLayout, columnNumber);
				if (elementToMove != null)
				{
					int current = ((IList)CurrentLayout).IndexOf(elementToMove);
					if ((direction == MovingDirection.MoveUp && current > 0) ||
						(direction == MovingDirection.MoveDown && current < (CurrentLayout.Count - 1)))
					{
						CurrentLayout.Remove(elementToMove);
						int newIndex = direction == MovingDirection.MoveUp ? current - 1 : current + 1;

						((IList)CurrentLayout).Insert(newIndex, elementToMove);
					}
				}
			}
		}

		#region Implementation

		#region GetColumnsLayout

		GridLayoutElementCollection GetAllColumns(StringCollectionX allColumnHeaders, string requiredColumns)
		{
			GridLayoutElementCollection result = new GridLayoutElementCollection();
			string[] required = requiredColumns.Split(',');

			for (int i = 0; i < allColumnHeaders.Count; i++)
			{
				GridLayoutElement element = result.AddNew();
				element.HeaderText = allColumnHeaders[i];
				element.ColumnNumber = ((IList)required).Contains(i.ToString()) ? -(i + 1) : i + 1;
			}

			return result;
		}

		GridLayoutElementCollection GetColumnsLayout(string currentLayoutString)
		{
			GridLayoutElementCollection result = new GridLayoutElementCollection();

			List<string> layout = new List<string>(currentLayoutString.Split(','));
			foreach (string requiredColumn in fRequiredColumns.Split(','))
			{
				if (!layout.Contains(requiredColumn))
				{
					layout.Add(requiredColumn);
				}
			}

			string[] exclusions = fExcludedGroupMembers.Split(',');

			foreach (string columnNumber in layout)
			{
				int i = GetColumnIndexFromLayout(columnNumber);
				if ((i != 0) && !((IList)exclusions).Contains(columnNumber))
				{
					GridLayoutElement element = FindElement(AllColumns, i);
					if (element != null)
					{
						result.Add(element);
					}
				}
			}

			return result;
		}

		#endregion

		string LayoutToString(GridLayoutElementCollection layout)
		{
			StringBuilder result = new StringBuilder();

			foreach (GridLayoutElement element in layout)
			{
				int number = element.ColumnNumber < 0 ? -(element.ColumnNumber + 1) : element.ColumnNumber - 1;
				result.Append(number.ToString() + ",");
			}

			return result.ToString().TrimEnd(',');
		}

		GridLayoutElement FindElement(GridLayoutElementCollection collection, int columnNumber)
		{
			foreach (GridLayoutElement element in collection)
			{
				if (element.ColumnNumber == columnNumber)
				{
					return element;
				}
			}

			return null;
		}

		#endregion

		int GetColumnIndexFromLayout(string layoutIndex)
		{
			string[] required = fRequiredColumns.Split(',');
			int i = -1;
			int.TryParse(layoutIndex, out i);
			int result = ((IList)required).Contains(layoutIndex) ? -(i + 1) : i + 1;

			return result;
		}
	}
}
