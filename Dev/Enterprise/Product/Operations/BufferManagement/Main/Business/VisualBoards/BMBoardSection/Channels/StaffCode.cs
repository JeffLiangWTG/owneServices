using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class StaffCode : IStaffCode
	{
		public StaffCode(ZGuid pk, ZString code)
		{
			PK = pk;
			Code = code;
		}

		public ZGuid PK { get; }
		public ZString Code { get; }
	}
}
