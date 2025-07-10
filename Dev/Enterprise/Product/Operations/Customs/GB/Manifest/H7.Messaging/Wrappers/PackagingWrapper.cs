using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Messaging;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class PackagingWrapper : IPackaging
	{
		PackagingWrapper()
		{
		}

		public static PackagingWrapper New(ZString typeCode, ZDecimal quantity, ZString marksNumbersID)
		{
			return new PackagingWrapper
			{
				TypeCode = typeCode,
				Quantity = quantity,
				MarksNumbersID = marksNumbersID
			};
		}

		public ZString TypeCode { get; private set; }

		public ZDecimal Quantity { get; private set; }

		public ZString MarksNumbersID { get; private set; }
	}
}
