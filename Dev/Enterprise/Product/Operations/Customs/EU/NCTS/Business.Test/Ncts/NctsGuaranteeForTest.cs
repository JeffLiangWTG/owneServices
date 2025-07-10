using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public class NctsGuaranteeForTest : NctsGuarantee
{
	public NctsGuaranteeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override GuaranteeApportionmentType GetApportionmentTypeCore() => _apportionmentType;

	public void SetupApportionmentType(GuaranteeApportionmentType apportionmentType)
	{
		_apportionmentType = apportionmentType;
	}

	GuaranteeApportionmentType _apportionmentType;
}
