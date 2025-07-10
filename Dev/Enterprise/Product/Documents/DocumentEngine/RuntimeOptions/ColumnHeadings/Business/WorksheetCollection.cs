using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class WorksheetCollection : AutoWorksheetCollection
	{
		public Worksheet AddNew(string sheetName)
		{
			Worksheet newSheet = new Worksheet(sheetName);
			this.Add(newSheet);
			return newSheet;
		}

		public Worksheet AddNew(string sheetName, string title)
		{
			Worksheet newSheet = new Worksheet(sheetName, title);
			this.Add(newSheet);
			return newSheet;
		}

		public Worksheet this[string sheetname]
		{
			get
			{
				return this[index.IndexOf(sheetname)];
			}
		}

		public void Remove(Worksheet worksheet)
		{
			this.RemoveAt(this.index.IndexOf(worksheet.Name));
		}

		protected override void OnInsert(int index, object value)
		{
			base.OnInsert(index, value);
			MaintainStringIndex(index, (Worksheet)value);
		}

		protected override void OnRemove(int index, object value)
		{
			base.OnRemove(index, value);
			this.index.Remove(((Worksheet)value).Name);
		}

		protected override void OnClear()
		{
			base.OnClear();
			this.index = new List<string>();
		}

		void MaintainStringIndex(int index, Worksheet value)
		{
			if (value.Name.IsEmpty)
			{
				throw new DocumentEngineException("Worksheet to be added must have a value for name.");
			}
			else
			{
				while (this.index.Count < index + 1)
				{
					this.index.Add("");
				}
				this.index[index] = value.Name;
			}
		}
		List<string> index = new List<string>();

		public WorksheetCollection Clone()
		{
			WorksheetCollection theClone = new WorksheetCollection();
			foreach (Worksheet worksheet in this)
			{
				theClone.Add(worksheet.Clone());
			}
			return theClone;
		}

		public bool Contains(string sheetName)
		{
			return index.Contains(sheetName);
		}

		public bool IsEmpty
		{
			get
			{
				bool result = true;
				foreach (Worksheet sheet in this)
				{
					result = result && sheet.ColumnHeadings.Count == 0;
				}
				return result;
			}
		}
	}
}
