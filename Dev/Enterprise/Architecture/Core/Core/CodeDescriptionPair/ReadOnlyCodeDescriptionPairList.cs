using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	public class ReadOnlyCodeDescriptionPairList : ICodeDescriptionPairList, ICodeDescriptionPairListIndexer
	{
		#region Construction

		public ReadOnlyCodeDescriptionPairList()
		{
		}

		public ReadOnlyCodeDescriptionPairList(ICodeDescriptionPairList listToClone)
		{
			CloneList(listToClone);
		}

		public ReadOnlyCodeDescriptionPairList(byte[] xmlByteArray)
		{
			ConvertFromXMLByteArray(xmlByteArray);
		}

		void CloneList(ICodeDescriptionPairList listToClone)
		{
			foreach (ICodeDescription element in listToClone)
			{
				ICodeDescription clone =
					(element is CodeElement) ?
					new CodeElement(element.PK, element.Code, element.Description) :
					new CodeDescriptionPair(element.Code, element is IMultilingualDescription ? ((IMultilingualDescription)element).MultilingualDescription : (NoResString)element.Description);

				Elements.Add(clone);
			}
		}

		public virtual object Clone()
		{
			return new ReadOnlyCodeDescriptionPairList(this);
		}

		#endregion

		#region Code/Description

		public string[] GetAllCodes()
		{
			return Elements.Select(x => x.Code).ToArray();
		}

		public ZString[] GetAllCodesZString()
		{
			return Elements.Select(x => new ZString(x.Code)).ToArray();
		}

		public bool ContainsOnly(params string[] codes)
		{
			if (codes.Length != Count)
			{
				return false;
			}
			foreach (string code in codes)
			{
				if (!ContainsCode(code))
				{
					return false;
				}
			}
			return true;
		}

		public bool ContainsCode(object code) => ContainsCodeCore(code);

		protected virtual bool ContainsCodeCore(object code)
		{
			var trimmedCode = code.ToString().TrimEnd();
			foreach (ICodeDescription element in this)
			{
				if (string.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns null if a match not found.
		/// </summary>
		public string GetDescriptionFromCode(string code)
		{
			string trimmedCode = code.Trim();
			foreach (ICodeDescription element in this)
			{
				if (string.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element.Description;
				}
			}
			return null;
		}

		public MultilingualString GetMultilingualDescriptionFromCode(string code)
		{
			string trimmedCode = code.Trim();
			foreach (ICodeDescription element in this)
			{
				if (string.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element is IMultilingualDescription ? ((IMultilingualDescription)element).MultilingualDescription : (NoResString)element.Description;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns null if a match not found.
		/// </summary>
		public string GetCodeFromDescription(string description)
		{
			string trimmedDescription = description.Trim();
			foreach (ICodeDescription element in this)
			{
				if (string.Equals(element.Description.Trim(), trimmedDescription, StringComparison.OrdinalIgnoreCase)
					|| (element is IMultilingualDescription multilingualElement && string.Equals(multilingualElement.MultilingualDescription.GetUnresolvedString(), description)))
				{
					return element.Code;
				}
			}
			return null;
		}

		public int IndexOfCode(object code)
		{
			string trimmedCode = code.ToString().Trim();
			for (int i = 0; i < Count; i++)
			{
				if (string.Equals(this[i].Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		public string DefaultCode
		{
			get { return defaultCode; }
			set { defaultCode = value; }
		}

		string defaultCode;

		#endregion

		#region ICodeDescriptionPairListIndexer Members

		public ICodeDescription this[ZGuid pk]
		{
			get
			{
				return pk.IsValid
					? this[pk.ToGuid()]
					: null;
			}
		}

		public ICodeDescription this[Guid pk]
		{
			get
			{
				if (pk == Guid.Empty)
				{
					return null;
				}
				return Elements.SingleOrDefault(element => element.PK != null && element.PK is ZGuid && (ZGuid)element.PK == pk);
			}
		}

		#endregion

		#region AsString

		public string CodesAsString
		{
			get
			{
				string result = "";
				foreach (ICodeDescription element in this)
				{
					result += element.Code + ", ";
				}
				if (result.Length >= 2)
				{
					result = result.Substring(0, result.Length - 2);
				}
				return result;
			}
		}

		public string ElementsAsString
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (ICodeDescription element in this)
				{
					builder.AppendFormat("{0} - {1}", element.Code, element.Description);
					builder.AppendLine();
				}
				return builder.ToString().Trim();
			}
		}

		#endregion

		#region To/From XML

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML constants")]
		public virtual byte[] ToXMLByteArray()
		{
			byte[] result;

			DataSet data = new DataSet();
			DataTable table = new DataTable();

			DataColumn codeColumn = new DataColumn("Code", typeof(string));
			DataColumn descColumn = new DataColumn("Description", typeof(string));

			table.Columns.Add(codeColumn);
			table.Columns.Add(descColumn);

			data.Tables.Add(table);

			foreach (ICodeDescription element in this)
			{
				DataRow row = table.NewRow();
				row[codeColumn] = element.Code;
				var multilingual = element as IMultilingualDescription;
				row[descColumn] = multilingual != null ? multilingual.MultilingualDescription.GetUnresolvedString() : element.Description;
				table.Rows.Add(row);
			}

			using (MemoryStream xmlStream = new MemoryStream())
			{
				data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
				result = xmlStream.ToArray();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected virtual void ConvertFromXMLByteArray(byte[] xmlBytes)
		{
			Elements.Clear();

			if (xmlBytes.Length > 0)
			{
				using (MemoryStream xmlStream = new MemoryStream(xmlBytes))
				{
					DataSet data = new DataSet();
					data.ReadXml(xmlStream, XmlReadMode.Auto);

					if (data.Tables.Count >= 1)
					{
						foreach (DataRow row in data.Tables[0].Rows)
						{
							string code = row[0] as string;
							string desc = row[1] as string;

							if (code == null)
							{
								code = "";
							}

							if (desc == null)
							{
								desc = "";
							}

							Elements.Add(new CodeDescriptionPair(code, desc));
						}
					}
				}
			}
		}

		#endregion

		#region ICollection Members

		public int Count
		{
			get { return Elements.Count; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public void CopyTo(Array array, int index)
		{
			Elements.ToArray().CopyTo(array, index);
		}

		public object SyncRoot
		{
			get { return Elements; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return Elements.GetEnumerator();
		}

		#endregion

		#region IList Members

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Contains(ICodeDescription value)
		{
			return Elements.Contains(value);
		}

		public ICodeDescription this[int index]
		{
			get { return Elements[index]; }
		}

		void IList.Clear()
		{
			throw new NotSupportedException("Cannot call IList.Clear() on a ReadOnlyCodeDescriptionPairList.");
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException("Cannot set IList[value] on a ReadOnlyCodeDescriptionPairList."); }
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("Cannot call IList.RemoveAt(index) on a ReadOnlyCodeDescriptionPairList.");
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException("Cannot call IList.Insert(index, value) on a ReadOnlyCodeDescriptionPairList.");
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException("Cannot call IList.Remove(value) on a ReadOnlyCodeDescriptionPairList.");
		}

		bool IList.Contains(object value)
		{
			return Contains((ICodeDescription)value);
		}

		int IList.IndexOf(object value)
		{
			return Elements.IndexOf((ICodeDescription)value);
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException("Cannot call IList.Add(value) on a ReadOnlyCodeDescriptionPairList.");
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		#endregion

		#region Elements List
		protected internal ReadOnlyCodeDescriptionList Elements
		{
			get { return elements; }
		}
		readonly ReadOnlyCodeDescriptionList elements = new ReadOnlyCodeDescriptionList();

		protected internal class ReadOnlyCodeDescriptionList : IList<ICodeDescription>
		{
			readonly List<ICodeDescription> codeDescriptions = new List<ICodeDescription>();

			public void Add(ICodeDescription item)
			{
				if (!(item is IBusiness) && item?.Code == null)
				{
					throw new ArgumentException("We should not be adding a null code to a description pair list.");
				}

				codeDescriptions.Add(item);
			}

			public void Clear()
			{
				codeDescriptions.Clear();
			}

			public bool Contains(ICodeDescription item)
			{
				return codeDescriptions.Contains(item);
			}

			public void CopyTo(ICodeDescription[] array, int arrayIndex)
			{
				codeDescriptions.CopyTo(array, arrayIndex);
			}

			public bool Remove(ICodeDescription item)
			{
				return codeDescriptions.Remove(item);
			}

			public int Count => codeDescriptions.Count;

			public bool IsReadOnly => false;

			public IEnumerator<ICodeDescription> GetEnumerator()
			{
				return codeDescriptions.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int IndexOf(ICodeDescription item)
			{
				return codeDescriptions.IndexOf(item);
			}

			public void Insert(int index, ICodeDescription item)
			{
				codeDescriptions.Insert(index, item);
			}

			public void RemoveAt(int index)
			{
				codeDescriptions.RemoveAt(index);
			}

			public ICodeDescription this[int index]
			{
				get => codeDescriptions[index];
				set => codeDescriptions[index] = value;
			}

			public void Sort(IComparer<ICodeDescription> comparer)
			{
				codeDescriptions.Sort(comparer);
			}

			public void Sort(Comparison<ICodeDescription> comparison)
			{
				codeDescriptions.Sort(comparison);
			}
		}
		#endregion

		#region GetHumanReadableListOfElements
		public string GetHumanReadableListOfElements()
		{
			return GetHumanReadableListOfElements(System.Environment.NewLine);
		}

		public string GetHumanReadableListOfElements(string delimiter)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (ICodeDescription pair in this)
			{
				result.Append(pair.Code.Trim() + " - " + pair.Description.Trim());
			}
			return result.ToStringWithDelimiterBetweenAppends(delimiter);
		}
		#endregion

		#region MaxCodeLength

		public int MaxCodeLength
		{
			get
			{
				// TODO: calculate dynamically as the list is changed instead of this slow loop every time.
				int maxLength = 0;
				foreach (ICodeDescription element in this)
				{
					if (element.Code.Length > maxLength)
					{
						maxLength = element.Code.Length;
					}
				}
				return maxLength;
			}
		}

		#endregion

		#region Equality

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				return true;
			}
			ReadOnlyCodeDescriptionPairList list = obj as ReadOnlyCodeDescriptionPairList;
			if (list == null)
			{
				return false;
			}
			foreach (ICodeDescription item in elements)
			{
				if (!list.Contains(item))
				{
					return false;
				}
			}
			foreach (ICodeDescription item in list)
			{
				if (!Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return elements.Count; // poor implementation, but don't expect anyone to hash lists
		}

		#endregion
	}
}
