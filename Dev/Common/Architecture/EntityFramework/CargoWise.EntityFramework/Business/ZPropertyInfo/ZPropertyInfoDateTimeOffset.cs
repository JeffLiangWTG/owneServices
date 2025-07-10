using System.ComponentModel;
using System.Globalization;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoDateTimeOffset : ZPropertyInfo<ZDateTimeOffset>
	{
		internal ZPropertyInfoDateTimeOffset(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoDateTimeOffset(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZDateTimeOffset.Empty; }
		}

		protected override bool HasChangesCore
			=> base.HasChangesCore || HasOffsetComponentChanged;

		bool HasOffsetComponentChanged
			=> Value.IsValid
				&& OriginalValue.IsValid
				&& Value.Offset != ((ZDateTimeOffset)OriginalValue).Offset;

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZDateTimeOffset parsedResult;

			bool result = ZDateTimeOffset.TryParseForUpdate(Value, stringValue, out parsedResult, CultureInfo.CurrentCulture);
			if (result)
			{
				Value = parsedResult;
			}

			return result;
		}
	}
}
