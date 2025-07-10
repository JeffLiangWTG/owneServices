using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoGuid : ZPropertyInfo<ZGuid>
	{
		internal ZPropertyInfoGuid(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoGuid(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return true; } // for Code Max Length
		}

		protected override IZType DefaultValueCore
		{
			get { return ZGuid.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			bool result = true;

			try
			{
				Value = new ZGuid(stringValue);
			}
			catch (System.FormatException) { result = false; }
			catch (ZTypeValueException) { result = false; }

			return result;
		}
	}
}
