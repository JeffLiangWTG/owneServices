using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoBool : ZPropertyInfo<ZBool>
	{
		public ZPropertyInfoBool(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoBool(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZBool.False; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			bool result = stringValue.ToUpper() == "Y" || stringValue.ToUpper() == "N";

			if (result)
			{
				Value = new ZBool(stringValue);
			}

			return result;
		}
	}
}
