using System;
using System.Collections.Specialized;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class NameEventHandlerCollection : NameObjectCollectionBase
	{
		public NameEventHandlerCollection()
		{
		}

		#region Indexers

		public EventHandler this[string key]
		{
			get { return (EventHandler)BaseGet(key); }
		}

		public EventHandler this[int i]
		{
			get { return (EventHandler)BaseGet(i); }
		}

		#endregion

		#region Implementation

		public void Add(string name, EventHandler handler)
		{
			if (name == null || handler == null)
			{
				ErrorReporter.ReportOnce(DeveloperErrorKey, DeveloperErrorMessage);
			}
			else
			{
				BaseAdd(name, handler);
			}
		}

		public void Clear()
		{
			BaseClear();
		}

		public void Remove(string name)
		{
			BaseRemove(name);
		}

		const string DeveloperErrorKey = "NameEventHandlerCollection";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer error message")]
		protected internal const string DeveloperErrorMessage = "NameEventHandlerCollection does not take in nulls, unlike its parent...Dont give it nulls or this will happen to you...";

		#endregion
	}
}
