using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Enterprise.VisualBoards.Business
{
	public class SearchFilter : IBoardFilter, IUndoableFilter
	{
		public SearchFilter(string searchTerm)
		{
			rawSearchTerm = searchTerm.TrimEnd(' ');
			searchTerm = Regex.Escape(rawSearchTerm).Replace("\\*", ".*");
			searchRegex = new Regex(string.Format(CultureInfo.InvariantCulture, ".*{0}.*", searchTerm), RegexOptions.IgnoreCase);
		}

		readonly string rawSearchTerm;
		readonly Regex searchRegex;

		public string RawSearchTerm
		{
			get { return rawSearchTerm; }
		}

		public Regex SearchRegex
		{
			get { return searchRegex; }
		}

		#region IBoardFilter Members

		public bool AllowMultiple
		{
			get { return false; }
		}

		public string FilterName
		{
			get { return Res.GetString("687ed8c3-b892-4a66-adc5-ed830163d591", "Search for '{0}'", rawSearchTerm); }
		}

		public bool RequiresRedraw
		{
			get { return false; }
		}

		public bool RequiresRemoval { get; set; }

		public Action RestoreVisualStateAfterFilterRemovedAction { get; set; }

		#endregion

		#region IUndoableFilter Members

		public bool RequiresUndo { get; set; }

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			return obj is SearchFilter;
		}

		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode();
		}

		#endregion
	}
}
