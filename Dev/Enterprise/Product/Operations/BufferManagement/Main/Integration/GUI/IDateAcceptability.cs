using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IDateAcceptability
	{
		ZString FH_DateAcceptability { get; }
		ZPropertyInfo FH_DateAcceptabilityInfo { get; }

		bool IsDeleted { get; }
	}
}
