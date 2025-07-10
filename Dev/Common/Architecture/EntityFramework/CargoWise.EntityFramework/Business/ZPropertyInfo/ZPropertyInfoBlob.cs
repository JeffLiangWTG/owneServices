using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoBlob : ZPropertyInfo<ZBlob>
	{
		internal ZPropertyInfoBlob(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoBlob(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return true; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZBlob.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			Value = ZBlob.FromUTF8(stringValue);

			return true;
		}
	}
}
