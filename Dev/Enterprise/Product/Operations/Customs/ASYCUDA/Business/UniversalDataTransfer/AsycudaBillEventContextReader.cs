using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaBillEventContextReader
	{
		public AsycudaBillEventContextReader(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill");
		}

		protected readonly AsycudaBill bill;

		internal void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddEventContextValuesCore(values);
		}

		protected virtual void AddEventContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
		}
	}
}
