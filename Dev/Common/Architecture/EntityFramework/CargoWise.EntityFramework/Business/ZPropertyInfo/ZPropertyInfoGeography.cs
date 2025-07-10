using System.Collections.ObjectModel;
using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZPropertyInfoGeography : ZPropertyInfo<ZGeography>
	{
		internal ZPropertyInfoGeography(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor) : base(bizObj, name, propertyDescriptor) { }

		public ZPropertyInfoGeography(BusinessObject bizObj, string name) : this(bizObj, name, null) { }

		public ReadOnlyCollection<string> AllowedSpatialTypes
		{
			get
			{
				var attribute = PropertyDescriptor.Attributes[typeof(AllowSpatialTypesAttribute)] as AllowSpatialTypesAttribute;
				return attribute?.AllowedSpatialTypes;
			}
		}

		public override bool SupportsMaxLength
		{
			get { return false; }
		}

		protected override IZType DefaultValueCore
		{
			get { return ZGeography.Empty; }
		}

		protected override bool SetValueFromStringCore(ZString stringValue)
		{
			ZGeography parsedResult;
			bool result = ZGeography.TryParse(stringValue, out parsedResult);
			if (result)
			{
				Value = parsedResult;
			}
			return result;
		}
	}
}
