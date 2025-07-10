using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("Count = {fields.Count}")]
	public sealed partial class OperationalActionFieldSupporterList : ICollection<OperationalActionFieldSupporter>, ICollection
	{
		public OperationalActionFieldSupporterList(Type rootType, string workflowType = null)
		{
			this.rootType = rootType;
			this.workflowType = workflowType;
		}

		public OperationalActionFieldSupporter this[string field]
		{
			get
			{
				OperationalActionFieldSupporter result;
				if (!fields.TryGetValue(field, out result))
				{
					PropertyInfo[] path = ReflectionHelper.FieldTextToPath(rootType, field);
					if (path != null)
					{
						result = fieldGenerator.CreateField(path);
						if (result != null)
						{
							fields.Add(result.Field, result);
						}
					}
					else if (!string.IsNullOrEmpty(field) && !string.IsNullOrEmpty(workflowType))
					{
						result = fieldGenerator.CreateField(field, workflowType);
					}
				}
				return result;
			}
		}

		public Type GetFieldType(string field)
		{
			var infoArr = ReflectionHelper.FieldTextToPath(rootType, field);
			return infoArr[infoArr.Length - 1].PropertyType;
		}

		#region ICollection<OperationalActionFieldList> Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		int ICollection<OperationalActionFieldSupporter>.Count
		{
			get { return fields.Count; }
		}

		void ICollection<OperationalActionFieldSupporter>.Clear()
		{
			throw new NotSupportedException();
		}

		void ICollection<OperationalActionFieldSupporter>.Add(OperationalActionFieldSupporter item)
		{
			throw new NotSupportedException();
		}

		bool ICollection<OperationalActionFieldSupporter>.Contains(OperationalActionFieldSupporter item)
		{
			return fields.ContainsKey(item.Field);
		}

		void ICollection<OperationalActionFieldSupporter>.CopyTo(OperationalActionFieldSupporter[] array, int arrayIndex)
		{
			fields.Values.CopyTo(array, arrayIndex);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection<OperationalActionFieldSupporter>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<OperationalActionFieldSupporter>.Remove(OperationalActionFieldSupporter item)
		{
			throw new InvalidOperationException();
		}

		#endregion

		#region IEnumerable<OperationalActionFieldList> Members

		public IEnumerator<OperationalActionFieldSupporter> GetEnumerator()
		{
			return fields.Values.GetEnumerator();
		}

		#endregion

		#region ICollection Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		int ICollection.Count
		{
			get { return fields.Count; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)fields.Values).CopyTo(array, index);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot
		{
			get { return ((ICollection)fields).SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		readonly Type rootType;
		readonly string workflowType;
		readonly Dictionary<string, OperationalActionFieldSupporter> fields = new Dictionary<string, OperationalActionFieldSupporter>(new FieldNameComparer());
		readonly OperationalActionFieldGenerator fieldGenerator = new OperationalActionFieldGenerator();
	}
}
