using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[XmlSerializerAssembly("Enterprise.DocumentEngine.XmlSerializers")]
	[XmlRoot("ArrayOfColumnHeading")]
	public class ColumnHeadingCollection : AutoColumnHeadingCollection
	{
		public ColumnHeadingCollection()
		{
		}

		public ColumnHeadingCollection(ColumnHeading[] columnHeadings)
		{
			this.Capacity = this.Count + columnHeadings.Length;
			foreach (ColumnHeading heading in columnHeadings)
			{
				this.Add(heading);
			}
		}

		public void Remove(ColumnHeading columnHeading)
		{
			this.RemoveAt(this.index.IndexOf(columnHeading.DisplayLabel));
		}

		public bool Contains(string displayLabel)
		{
			return index.IndexOf(displayLabel) > -1;
		}

		public ColumnHeading[] ToArray()
		{
			ColumnHeading[] headings = new ColumnHeading[this.Count];
			for (int i = 0; i < this.Count; i++)
			{
				headings[i] = this[i];
			}
			return headings;
		}

		#region Cloning
		public ColumnHeadingCollection Clone()
		{
			ColumnHeadingCollection theClone = new ColumnHeadingCollection();
			theClone.Capacity = Count;
			foreach (ColumnHeading heading in this)
			{
				theClone.Add(heading.Clone());
			}
			ColumnHeading oldHeading;
			foreach (ColumnHeading heading in theClone)
			{
				oldHeading = this[heading.DisplayLabel];
				foreach (ColumnHeading referencingColumn in oldHeading.ReferencedBy)
				{
					if (referencingColumn.DisplayLabel == String.Empty)
					{
						heading.ReferencedBy.Add(referencingColumn); // this is Dummy ColumnHeading
					}
					else
					{
						heading.ReferencedBy.Add(theClone[referencingColumn.DisplayLabel]);
					}
				}
			}
			return theClone;
		}

		public ColumnHeadingCollection CloneVisible()
		{
			ColumnHeadingCollection theClone = new ColumnHeadingCollection();
			theClone.Capacity = this.Cast<ColumnHeading>().Count(x => !x.Hidden);
			foreach (ColumnHeading heading in this)
			{
				if (!heading.Hidden)
				{
					theClone.Add(heading.Clone());
				}
			}
			return theClone;
		}
		#endregion

		#region string indexer
		public ColumnHeading this[string displayLabel]
		{
			get
			{
				return this[index.IndexOf(displayLabel)];
			}
		}

		void MaintainIndex(int index, ColumnHeading value)
		{
			while (this.index.Count < index + 1)
			{
				this.index.Add("");
			}
			this.index[index] = value.DisplayLabel;
		}
		List<string> index = new List<string>();

		protected override void OnInsert(int index, object value)
		{
			base.OnInsert(index, value);
			MaintainIndex(index, (ColumnHeading)value);
		}

		protected override void OnClear()
		{
			base.OnClear();
			this.index = new List<string>();
		}

		protected override void OnRemove(int index, object value)
		{
			base.OnRemove(index, value);
			this.index.Remove(((ColumnHeading)value).DisplayLabel);
		}
		#endregion
	}
}
