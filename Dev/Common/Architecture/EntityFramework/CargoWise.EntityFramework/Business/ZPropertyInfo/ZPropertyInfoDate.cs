using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoDate : ZPropertyInfo<ZDate>
	{
		internal ZPropertyInfoDate(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor)
			: base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoDate(BusinessObject bizObj, string name)
			: this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZDate.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZDate parsedResult;

			bool result = ZDate.TryParseJulianDate(stringValue, out parsedResult);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
