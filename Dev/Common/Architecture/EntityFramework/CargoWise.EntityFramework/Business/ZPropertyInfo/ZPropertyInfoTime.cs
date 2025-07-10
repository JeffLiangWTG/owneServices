using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoTime : ZPropertyInfo<ZTime>
	{
		internal ZPropertyInfoTime(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor)
			: base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoTime(BusinessObject bizObj, string name)
			: this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZTime.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZTime parsedResult;

			bool result = ZTime.TryParseExact(stringValue, out parsedResult, ZTime.TimeFormat);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
