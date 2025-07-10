using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoByte : ZPropertyInfo<ZByte>
	{
		internal ZPropertyInfoByte(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoByte(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return new ZByte(); }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZByte parsedValue;

			bool result = ZByte.TryParse(stringValue, out parsedValue);
			if (result)
			{
				Value = parsedValue;
			}

			return result;
		}
	}
}
