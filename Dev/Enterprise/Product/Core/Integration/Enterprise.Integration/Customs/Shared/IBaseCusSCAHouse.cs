using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IBaseCusSCAHouse
			{
				ZGuid PK { get; }
				ZGuid CA_CB { get; set; }
				ZString CA_ConsigneeName { get; set; }
				ZString CA_ConsigneeState { get; set; }
				ZString CA_ConsigneeSuburb { get; set; }
				ZString CA_ConsigneeAddress1 { get; set; }
				ZString CA_ConsigneeAddress2 { get; set; }
				ZString CA_ConsigneePostcode { get; set; }
			}
		}
	}
}
