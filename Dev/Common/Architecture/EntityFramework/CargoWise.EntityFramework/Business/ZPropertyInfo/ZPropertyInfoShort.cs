using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoShort : ZPropertyInfo<ZShort>
	{
		internal ZPropertyInfoShort(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoShort(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZShort.Zero; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZShort parsedResult;

			bool result = ZShort.TryParse(stringValue, out parsedResult);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
