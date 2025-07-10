using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoInt : ZPropertyInfo<ZInt>
	{
		internal ZPropertyInfoInt(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoInt(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return new ZInt(); }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZInt parsedResult;

			bool result = ZInt.TryParse(stringValue, out parsedResult);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
