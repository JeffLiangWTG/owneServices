using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoDecimal : ZPropertyInfo<ZDecimal>
	{
		internal ZPropertyInfoDecimal(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoDecimal(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZDecimal.Zero; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZDecimal parsedResult;

			bool result = ZDecimal.TryParse(stringValue, out parsedResult);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
