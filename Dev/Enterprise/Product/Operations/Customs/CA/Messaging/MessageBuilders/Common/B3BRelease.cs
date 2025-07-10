using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public class B3BRelease : IB3BRelease
	{
		public B3BRelease() { }

		public B3BRelease(ZString ccnNumber, ZDateTime dateOfRelease)
		{
			CargoControlNumber = ccnNumber;
			DateOfRelease = dateOfRelease;
		}

		#region Implementation of IB3BRelease

		public ZString CargoControlNumber { get; set; }
		public ZDateTime DateOfRelease { get; set; }

		#endregion
	}
}
