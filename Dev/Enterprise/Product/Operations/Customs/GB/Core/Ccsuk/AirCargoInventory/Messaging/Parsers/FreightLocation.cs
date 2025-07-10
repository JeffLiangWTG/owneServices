using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class FreightLocation
	{
		public ZString Function { get; set; }

		public ZString LocationCode { get; set; }

		public ZString ShedOperator { get; set; }

		public ZString ShedPhysicalIdentity { get; set; }

		public bool IsEmpty
		{
			get
			{
				return (Function.IsEmpty && LocationCode.IsEmpty && ShedOperator.IsEmpty && ShedPhysicalIdentity.IsEmpty);
			}
		}

		public override string ToString()
		{
			if (!ShedOperator.IsEmpty || !ShedPhysicalIdentity.IsEmpty)
			{
				return string.Format("{0}={1}, Shed={2} {3}", new LocationTypes().GetDescriptionFromCode(Function), LocationCode, ShedOperator, ShedPhysicalIdentity);
			}
			else
			{
				return string.Format("{0}={1}", new LocationTypes().GetDescriptionFromCode(Function), LocationCode);
			}
		}
	}
}
