using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ChildDummyBusinessObjectCollection : BusinessObjectCollection<ChildDummyBusinessObject>
	{
		public ChildDummyBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ChildDummyBusinessObject AddNew(
			string code,
			string description = "",
			int number = 0,
			decimal decimalValue = 0)
		{
			var result = AddNew();

			result.Z0_Code = code;
			result.Z0_Description = description;
			result.Z0_Number = number;
			result.Z0_Decimal = decimalValue;

			return result;
		}

		public ZString CollectionOwnProperty { get; set; } = "WhatEver";
	}
}
