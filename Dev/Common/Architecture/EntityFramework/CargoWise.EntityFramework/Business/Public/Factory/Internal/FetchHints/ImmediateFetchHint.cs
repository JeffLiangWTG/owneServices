using System;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class ImmediateFetchHint : FetchHint, IImmediateHint
	{
		public ImmediateFetchHint(Type businessObjectType, SchemaColumn column, IZType value) : base(column, value)
		{
			string bizOTableName = BusinessObjectFactory.GetTableNameFromType(businessObjectType);
			string[] split = bizOTableName.Split('.');
			if (!split[split.Length - 1].Equals(column.TableName, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(
					"BusinessObjectType and Column must represent the same table" + System.Environment.NewLine +
					"BusinessObjectType : " + bizOTableName + System.Environment.NewLine +
					"Column : " + column.TableName);
			}
			this.BusinessObjectType = businessObjectType;
		}

		readonly Type BusinessObjectType;

		public override IQueryHashKey GetHashKeyObject()
		{
			EnumerableHashObject enumerableHashObject = new EnumerableHashObject();
			enumerableHashObject.AddExpandingEnumerable(base.GetHashKeyObject());
			enumerableHashObject.Add(BusinessObjectType);
			return enumerableHashObject;
		}

		#region IImmediateHint Members

		Type IImmediateHint.BusinessObjectType
		{
			get { return this.BusinessObjectType; }
		}

		#endregion
	}
}
