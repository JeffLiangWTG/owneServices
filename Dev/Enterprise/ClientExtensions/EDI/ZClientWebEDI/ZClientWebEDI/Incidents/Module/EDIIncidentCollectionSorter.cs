using System;
using System.ComponentModel;

using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI.Module
{
	/// <summary>
	/// CollectionSorter that is aware of Priorities
	/// </summary>
	public class EDIIncidentWebCollectionSorter : WebCollectionSorter
	{
		public EDIIncidentWebCollectionSorter(string sortProperty, ListSortDirection direction)
			: base(sortProperty, direction)
		{
		}

		public override int Compare(object x, object y)
		{
			return (SortProperty == IncidentMainSchema.IM_Priority.Name) ? ComparePriority(x, y) : base.Compare(x, y);
		}

		protected virtual int ComparePriority(object x, object y)
		{
			int xval = GetPriorityValue(x);
			int yval = GetPriorityValue(y);
			int result = 0;
			if (xval < yval)
			{
				result = 1;
			}
			else if (xval > yval)
			{
				result = -1;
			}
			return (Direction == ListSortDirection.Ascending) ? result : -result;
		}

		protected int GetPriorityValue(object obj)
		{
			int result;
			IncidentMainBase incident = obj as IncidentMainBase;
			string val = (incident != null) ? incident.IM_Priority.ToString() : "";
			Int32.TryParse(val.Substring(2, 1), out result);
			return result;
		}
	}
}
