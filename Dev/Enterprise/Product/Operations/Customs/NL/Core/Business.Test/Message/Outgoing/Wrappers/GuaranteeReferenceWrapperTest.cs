using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class GuaranteeReferenceWrapperTest : DataProviderTestCase<GuaranteeReferenceWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeReferenceWrapper(null, 2));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(2, wrapperLastGuarantee.SequenceNumeric);
	}

	public void TestAmount()
	{
		AssertEquals("Amount", 699m, Provider.Amount);
	}

	public void TestCurrency()
	{
		AssertEquals("Currency", "EUR", Provider.Currency);
	}

	public void TestId()
	{
		CombineAssertions(() =>
		{
			AssertEquals("ID", "GUARANTEEREF", Provider.Id);
			AssertEquals("ID", ZString.Empty, wrapperLastGuarantee.Id);
		});
	}

	public void TestAccessCode()
	{
		AssertEquals("Access Code", "1234", Provider.AccessCode);
	}

	public void TestReferenceId()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Reference ID", ZString.Empty, Provider.ReferenceId);
			AssertEquals("Reference ID", "GUARANTEEREF2", wrapperLastGuarantee.ReferenceId);
		});
	}

	public void TestGRN()
	{
		AssertEquals("GRN", string.Empty, Provider.GRN);
	}

	public void TestGuaranteeOffice()
	{
		AssertEquals("Guarantee Office", "NL001234", Provider.GuaranteeOffice);
	}

	protected override GuaranteeReferenceWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		var orgHeaderGuaranteeOffice = WrapperTestHelper.CreateOrgHeader(Factory, "Guarantee Office", "OFF", "321654");
		WrapperTestHelper.CreateAddress(orgHeaderGuaranteeOffice.Addresses, "OFF", "Dorpstaat 10", "Rotterdam", "1079CK");

		var declaration = Factory.New<JobDeclaration>();
		var bondDetail = WrapperTestHelper.CreateGuarantee(declaration, 699, "EUR", "GUARANTEEREF", "1234", "NL001234", "0", "OTH", orgHeaderGuaranteeOffice);
		wrapper = new GuaranteeReferenceWrapper(bondDetail, 1);
		var bondDetailLast = WrapperTestHelper.CreateGuarantee(declaration, 899, "EUR", "GUARANTEEREF2", "4321", "NL004321", "0", NLConstants.GuaranteeReferenceTypes.Guarantee, orgHeaderGuaranteeOffice);
		wrapperLastGuarantee = new GuaranteeReferenceWrapper(bondDetailLast, 2);
	}
	GuaranteeReferenceWrapper wrapper;
	GuaranteeReferenceWrapper wrapperLastGuarantee;
}
