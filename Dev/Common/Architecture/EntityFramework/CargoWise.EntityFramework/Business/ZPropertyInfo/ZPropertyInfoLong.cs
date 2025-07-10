using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoLong : ZPropertyInfo<ZLong>
	{
		internal ZPropertyInfoLong(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor)
		{ }

		public ZPropertyInfoLong(BusinessObject bizObj, string name) : this(bizObj, name, null)
		{ }

		public override bool SupportsMaxLength => false;

		protected override IZType DefaultValueCore => new ZLong();

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			bool result = ZLong.TryParse(stringValue, out var parsedResult);
			if (result)
			{
				Value = parsedResult;
			}
			return result;
		}
	}
}
