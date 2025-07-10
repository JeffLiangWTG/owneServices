using System;
using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business
{
	public class RowSegmentDefinitionAndData : Tuple<int, IZType>
	{
		public RowSegmentDefinitionAndData(int order, IZType value)
			: base(order, value)
		{
		}

		public int Order
		{
			get { return Item1; }
		}

		public IZType Value
		{
			get { return Item2; }
		}
	}
}
