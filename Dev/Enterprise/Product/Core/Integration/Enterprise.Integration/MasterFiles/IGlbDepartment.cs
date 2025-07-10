using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IGlbDepartment
	{
		ZGuid PK { get; }
		ZString GE_Code { get; set; }
		ZString GE_Desc { get; set; }
		ZBool GE_Sea { get; set; }
		ZBool GE_Air { get; set; }
		ZBool GE_Import { get; set; }
		ZBool GE_Export { get; set; }
	}
}
