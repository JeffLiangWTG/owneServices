using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IJobConsolCost : IChargeWithChargeCode
	{
		string GetJobConsolCostInfo();

		ZPropertyInfo E6_AH_APInvoiceInfo { get; }

		ZDecimal E6_OSGSTAmount_Calc { get; }

		bool ShouldBeReadOnlyWhenPosted { get; }
	}

	public interface IChargeWithChargeCode
	{
		ZGuid PK { get; }
		string AC_Code { get; }
		string AC_Desc { get; }
		bool IsDeleted { get; }
	}
}
