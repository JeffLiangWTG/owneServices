using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.Common.CodeMappings
{
	public class CodeMappingCollection : IEnumerable<CodeMapping>
	{
		public CodeMappingCollection(IEnumerable<CodeMapping> collection)
		{
			this.collection = collection;
		}
		readonly IEnumerable<CodeMapping> collection;

		public CodeMapping this[string table, string propertyName]
		{
			get
			{
				return collection.FirstOrDefault(m => m.TableName == table && m.PropertyName == propertyName);
			}
		}

		#region IEnumerable<CodeMapping> Members

		public IEnumerator<CodeMapping> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
