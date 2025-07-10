using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADHeaderCommonWrapperTest<THeaderWrapper> : TestCaseWithFactory
	where THeaderWrapper : IHeaderCommon
{
	public void TestAnnualProgressiveNumber()
	{
		AssertEquals("<<MSGNO PLACEHOLDER>>", sadHeaderWrapper.AnnualProgressiveNumber);
	}

	public void TestAuthorizationNo()
	{
		jobDeclaration.ZG_AuthorisationNumber = "123456D";
		AssertEquals(sadHeaderWrapper.AuthorizationNo, "123456");
		jobDeclaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertEquals(sadHeaderWrapper.AuthorizationNo, ZString.Empty);
	}

	public void TestAuthorizationCIN()
	{
		jobDeclaration.ZG_AuthorisationNumber = "123456D";
		AssertEquals(sadHeaderWrapper.AuthorizationCIN, "D");
		jobDeclaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertEquals(sadHeaderWrapper.AuthorizationNo, ZString.Empty);
	}

	public void TestTotalItems()
	{
		entryHeader.MergedLines.AddNew();
		entryHeader.MergedLines.AddNew();
		entryHeader.MergedLines.AddNew();
		AssertEquals(3, sadHeaderWrapper.TotalItems);
	}

	public void TestAcceptanceDate()
	{
		entryInstruction.CEI_DateForDuty = new ZDate(2019, 07, 24);
		AssertEquals(new ZDate(2019, 07, 24), sadHeaderWrapper.AcceptanceDate);
		entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 06, 03, 16, 30, 45);
		AssertEquals(new ZDate(2019, 06, 03), sadHeaderWrapper.AcceptanceDate);

		entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
		AssertEquals(ZDate.Empty, sadHeaderWrapper.AcceptanceDate);
		entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
		AssertEquals(ZDate.Invalid, sadHeaderWrapper.AcceptanceDate);
	}

	public void TestDeclaration()
	{
		CombineAssertions(() =>
		{
			var declaration = sadHeaderWrapper.Declaration;
			AssertNotNull(declaration);
			AssertEquals(GetExpectedDeclarationType(), declaration.GetType());
		});
	}

	protected abstract Type GetExpectedDeclarationType();

	public void TestConsignor()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = supplier.Addresses.AddNew();
		jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = address.PK;
		address.CompanyName = "IKEA";
		address.Address1 = "MAIN";
		address.Address2 = "ADDRESS";
		address.Postcode = "4000";
		address.City = "ABCEXPMEL";
		address.OA_RN_NKCountryCode = "ZA";

		CombineAssertions(() =>
		{
			var consignor = sadHeaderWrapper.Consignor;
			AssertNotNull(consignor);
			AssertType<SADTraderWrapper>(consignor);
			AssertEquals("IT", consignor.IdCountryCode);
			AssertEquals("385040449", consignor.ID);
			AssertEquals("IKEA", consignor.Name);
			AssertEquals("MAIN ADDRESS", consignor.Address);
			AssertEquals("4000", consignor.Postcode);
			AssertEquals("ABCEXPMEL", consignor.City);
			AssertEquals("ZA", consignor.CountryCode);
		});
	}

	public void TestConsignee()
	{
		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = importer.Addresses.AddNew();
		jobDeclaration.ImporterDeliveryAddress.E2_OA_Address = address.PK;
		address.CompanyName = "YKK MEDITERRANEO SPA";
		address.Address1 = "ZONA IND. CAMPOLUNGO";
		address.Postcode = "63100";
		address.City = "ASCOLI PICENO";
		address.OA_RN_NKCountryCode = "IT";
		CombineAssertions(() =>
		{
			var consignee = sadHeaderWrapper.Consignee;
			AssertNotNull(consignee);
			AssertType<SADTraderWrapper>(consignee);
			AssertEquals("IT", consignee.IdCountryCode);
			AssertEquals("385040449", consignee.ID);
			AssertEquals("YKK MEDITERRANEO SPA", consignee.Name);
			AssertEquals("ZONA IND. CAMPOLUNGO", consignee.Address);
			AssertEquals("63100", consignee.Postcode);
			AssertEquals("ASCOLI PICENO", consignee.City);
			AssertEquals("IT", consignee.CountryCode);
		});
	}

	public void TestDeclarantTrader()
	{
		CombineAssertions(() =>
		{
			var declarantTrader = sadHeaderWrapper.DeclarantTrader;
			AssertNotNull(declarantTrader);
			AssertType<SADDeclarantTraderWrapper>(declarantTrader);
		});
	}

	public abstract void TestCountryOfDispatch();

	public abstract void TestTermsOfDelivery();

	public abstract void TestTransactionData();

	public void TestDeferredPayment()
	{
		CombineAssertions(() =>
		{
			var deferredPayment = sadHeaderWrapper.DeferredPayment;
			AssertNotNull(deferredPayment);
			AssertType<SADDeferredPaymentWrapper>(deferredPayment);
		});
	}

	public abstract void TestWarehouseIdentification();

	public abstract void TestCountryOfDestination();

	public abstract void TestIsContainerizedTransport();

	public abstract void TestTransportModeAtBorder();

	public abstract void TestInlandTransportMode();

	public void TestDateLimitOfTemporaryOperation()
	{
		AssertEquals(ZDate.Empty, sadHeaderWrapper.DateLimitOfTemporaryOperation);
		entryInstruction.ZG_TempProcLimitDate = new ZDate(2019, 07, 25);
		AssertEquals(new ZDate(2019, 07, 25), sadHeaderWrapper.DateLimitOfTemporaryOperation);
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		AssertEquals(ZDate.Empty, sadHeaderWrapper.DateLimitOfTemporaryOperation);
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Invalid;
		AssertEquals(ZDate.Invalid, sadHeaderWrapper.DateLimitOfTemporaryOperation);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => GetHeaderWrapper(null));
		AssertNoExceptionThrown(() => GetHeaderWrapper(entryHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		SetDeclarationMessageType(jobDeclaration);
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
	}

	protected abstract THeaderWrapper GetHeaderWrapper(CusEntryHeader entryHeader);

	protected virtual void SetDeclarationMessageType(JobDeclaration declaration) { }

	protected JobDeclaration jobDeclaration;
	protected CusEntryHeader entryHeader;
	protected CusEntryInstruction entryInstruction;
	protected THeaderWrapper sadHeaderWrapper;
}
