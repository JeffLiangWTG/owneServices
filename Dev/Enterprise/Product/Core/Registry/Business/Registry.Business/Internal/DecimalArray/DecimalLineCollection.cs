using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business.Internal
{
	public class DecimalLineCollection : NonPersistentBusinessObjectCollection<DecimalLine>
	{
		public DecimalLineCollection(DecimalArrayRegistryDataType dataType)
		{
			this.dataType = dataType;
		}

		public void Populate(decimal[] values)
		{
			foreach (decimal value in values)
			{
				AddNew().Number = value;
			}
		}

		public decimal[] ToDecimalArray()
		{
			List<Decimal> result = new List<decimal>();
			foreach (DecimalLine element in this)
			{
				result.Add(element.Number);
			}
			return result.ToArray();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DecimalLine(this);
		}

		internal DecimalArrayRegistryDataType DataType
		{
			get { return dataType; }
		}

		readonly DecimalArrayRegistryDataType dataType;
	}
}
