using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Internal
{
	public class StringLineCollection : NonPersistentBusinessObjectCollection<StringLine>
	{
		public StringLineCollection(StringArrayRegistryDataType dataType)
		{
			this.dataType = dataType;
		}

		public void Populate(string[] values)
		{
			foreach (string value in values)
			{
				AddNew().Value = value;
			}
		}

		public string[] ToStringArray()
		{
			List<string> result = new List<string>();
			foreach (StringLine element in this)
			{
				result.Add(element.Value);
			}
			return result.ToArray();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StringLine(dataType);
		}

		internal StringArrayRegistryDataType DataType
		{
			get { return dataType; }
		}

		readonly StringArrayRegistryDataType dataType;
	}
}
