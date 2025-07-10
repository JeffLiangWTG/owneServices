using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsGuarantee))]
sealed class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
{
	public void TestCopyCustomsOfficeFromGuaranteeHeader()
	{
		var propertyInfo = typeof(NctsGuarantee).GetProperty("CopyCustomsOfficeFromGuaranteeHeader", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertEquals(false, propertyInfo.GetValue(Factory.New<NctsGuarantee>()));
	}

	public void TestCopyPW_BondFiledPort()
	{
		var guarantee = Factory.New<NctsGuarantee>();
		guarantee.PW_BondFiledPort = "filled";
		var guaranteeCopy = Factory.New<NctsGuarantee>();
		guaranteeCopy.CopyPersistentValuesFrom(guarantee);
		AssertEquals("copied", string.Empty, guaranteeCopy.PW_BondFiledPort);
		guaranteeCopy.PW_BondFiledPort = "filled";
		AssertEquals("setter", "filled", guaranteeCopy.PW_BondFiledPort);
	}

	public void TestDefaultLiabilityAmountWhenZeroDuties()
	{
		var propertyInfo = typeof(NctsGuarantee).GetProperty("Phase5DefaultLiabilityAmount", BindingFlags.NonPublic | BindingFlags.Instance);
		var nctsGuarantee = (NctsGuarantee)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
		AssertEquals(10000m, propertyInfo.GetValue(nctsGuarantee));
	}

	public void TestBondAmount_EmptiedWhenMadeReadOnly()
	{
		var nctsGuarantee = (NctsGuarantee)GetNewBusinessObject();
		nctsGuarantee.PW_BondNumber = "bondnumber";
		nctsGuarantee.PW_BondAmount = 1000;
		nctsGuarantee.PW_BondNumber = string.Empty;
		AssertEquals(ZDecimal.Zero, nctsGuarantee.PW_BondAmount);
	}

	public void TestValidationType()
	{
		var nctsGuarantee = (NctsGuarantee)GetNewBusinessObject();
		AssertType<NctsGuaranteeValidation>(nctsGuarantee.Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader.Guarantees.AddNew();
	}
}
