using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoDateTime : ZPropertyInfo<ZDateTime>
	{
		internal ZPropertyInfoDateTime(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoDateTime(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZDateTime.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZDateTime parsedResult;

			bool result = ZDateTime.TryParseISO8601Date(stringValue, out parsedResult);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
