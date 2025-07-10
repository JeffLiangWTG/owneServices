using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class ZPropertyInfoString : ZPropertyInfo<ZString>
	{
		internal ZPropertyInfoString(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoString(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return true; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZString.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			bool result = stringValue.Length <= MaxLength;

			if (result)
			{
				Value = stringValue;
			}

			return result;
		}
	}
}
