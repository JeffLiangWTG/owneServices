using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeader))]
sealed class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestRemainingPackagesQty()
	{
		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_Reference = "DDT123456";
		CombineAssertions(() =>
		{
			AssertEquals("RemainingPackagesQty should be 0 when register has no register lines.", 0, registerHeader.RemainingPackagesQty);
			var registerLine1 = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine1.SRL_PackagesRemaining = 10;
			var registerLine2 = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine2.SRL_PackagesRemaining = 20;
			AssertEquals("RemainingPackagesQty should be sum of register register lines SRL_PackagesRemaining.", 30, registerHeader.RemainingPackagesQty);
		});
	}

	public void TestPremises()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			AssertEquals("Premises empty when it does not join.", ZString.Empty, premises.SRP_Code);

			premises.SRP_Code = "AAA";
			AssertEquals("Premises AAA when it is joined.", "AAA", premises.SRP_Code);

			header.SRH_SRP_Premises = premises.PK;
			var cusTempStoragePromises = (CusTempStorageRegPremises)header.Premises;
			AssertEquals("Premises SRP_Code is Readonly", true, cusTempStoragePromises.SRP_CodeInfo.ReadOnly);
			AssertEquals("Premises SRP_CustomsLocation is Readonly", true, cusTempStoragePromises.SRP_CustomsLocationInfo.ReadOnly);
		});
	}

	public void TestAddNewRegisterTransaction()
	{
		var emptyRegisterHeader = Factory.New<CusTempStorageRegHeader>();
		emptyRegisterHeader.SRH_Reference = "EmptyRegister";
		emptyRegisterHeader.SRH_AppCode = "IST";

		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_Reference = "HEADER";
		registerHeader.SRH_AppCode = "IST";

		var registerLine1 = registerHeader.CusTempStorageRegLines.AddNew();
		registerLine1.SRL_LineNumber = 1;
		registerLine1.SRL_PackageType = "CTN";

		var oblTransaction = registerLine1.CusTempStorageRegLineTransactions.AddNew();
		oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		oblTransaction.SRT_InternalReferenceNumber = "InternalReference";
		oblTransaction.SRT_Reference = "CustomsReference";
		oblTransaction.SRT_GrossWeight = 2000m;
		oblTransaction.SRT_PackageQty = 100;
		AssertEquals("Prerequisite.", 100, registerLine1.SRL_PackagesRemaining);

		var registerLine2 = registerHeader.CusTempStorageRegLines.AddNew();
		registerLine2.SRL_LineNumber = 2;
		var transaction5 = registerLine2.CusTempStorageRegLineTransactions.AddNew();
		transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction5.SRT_InternalReferenceNumber = "InternalReference";
		transaction5.SRT_Reference = ZString.Empty;
		transaction5.SRT_GrossWeight = 2000m;
		transaction5.SRT_PackageQty = 10;

		var logger = new LoggingInformation();

		var transactionData1 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 1, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Updating Packages and Gross Weight transaction"
		};
		registerHeader.AddNewRegisterTransaction(logger, transactionData1.RegisterLineNo, transactionData1.CustomsReferenceNumber, transactionData1.ReferenceType, transactionData1.InternalReferenceNumber, transactionData1.InternalReferenceType, transactionData1.GrossMass, transactionData1.PackageQuantity, transactionData1.Comments);
		Factory.Save();

		var registerLine1Query = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
		_ = registerLine1Query.AddToFilter(CusTempStorageRegLineSchema.SRL_LineNumber, 1);
		var reloadedRegisterLine1 = Factory.Load<CusTempStorageRegLine>(registerLine1Query)[0];

		var registerLine2Query = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
		_ = registerLine2Query.AddToFilter(CusTempStorageRegLineSchema.SRL_LineNumber, 2);
		var reloadedRegisterLine2 = Factory.Load<CusTempStorageRegLine>(registerLine2Query)[0];

		CombineAssertions("First transaction data processing results", () =>
		{
			AssertEquals("A new transaction should have been added against registerLine1.", 2, reloadedRegisterLine1.CusTempStorageRegLineTransactions.Count);
			AssertEquals("No new transaction should have been added against registerLine2.", 1, reloadedRegisterLine2.CusTempStorageRegLineTransactions.Count);
			AssertEquals("New transaction Package Quantity should match transaction data.", 30, reloadedRegisterLine1.CusTempStorageRegLineTransactions.ElementAt(1).SRT_PackageQty);
			AssertEquals("New transaction GrossMass should match transaction data.", 6000m, registerLine1.CusTempStorageRegLineTransactions.ElementAt(1).SRT_GrossWeight);
			AssertEquals("New transaction InternalReference should match processed transaction InternalReference.", "InternalReference", reloadedRegisterLine1.CusTempStorageRegLineTransactions.ElementAt(1).SRT_InternalReferenceNumber);
			AssertEquals("New transaction Reference should match processed transaction CustomsReferenceNumber.", "CustomsReference", reloadedRegisterLine1.CusTempStorageRegLineTransactions.ElementAt(1).SRT_Reference);
			AssertEquals("New transaction ReferenceType should match processed transaction TransactionType.", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, reloadedRegisterLine1.CusTempStorageRegLineTransactions.ElementAt(1).SRT_ReferenceType);
			AssertEquals("New transaction Comments should match processed transaction Comments.", "Updating Packages and Gross Weight transaction", reloadedRegisterLine1.CusTempStorageRegLineTransactions.ElementAt(1).SRT_Comments);
			AssertEquals("SRL_PackagesRemaining should have been updated.", 130, reloadedRegisterLine1.SRL_PackagesRemaining);
			Assert("Processor log should have been updated.", logger.Logs.Any(x => x.ToString().Contains("Creation of a transaction for register HEADER, Line Number 1: Package Quantity 30, Gross Weight 6000.")));
		});

		var transactionData2 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 1, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "UnmatchingReference",
			GrossMass = 26000m,
			PackageQuantity = -140,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction with too much packages to withdraw"
		};
		registerHeader.AddNewRegisterTransaction(logger, transactionData2.RegisterLineNo, transactionData2.CustomsReferenceNumber, transactionData2.ReferenceType, transactionData2.InternalReferenceNumber, transactionData2.InternalReferenceType, transactionData2.GrossMass, transactionData2.PackageQuantity, transactionData2.Comments);
		Factory.Save();

		reloadedRegisterLine1 = Factory.Load<CusTempStorageRegLine>(registerLine1Query)[0];
		reloadedRegisterLine2 = Factory.Load<CusTempStorageRegLine>(registerLine2Query)[0];

		CombineAssertions("Second transaction data processing results", () =>
		{
			AssertEquals("No new transaction should have been added against registerLine1.", 2, reloadedRegisterLine1.CusTempStorageRegLineTransactions.Count);
			AssertEquals("No new transaction should have been added against registerLine2.", 1, reloadedRegisterLine2.CusTempStorageRegLineTransactions.Count);
		});
	}

	public void TestHumanReadableName()
	{
		var cusTempStorageRegHeader = (CusTempStorageRegHeader)GetNewBusinessObject();
		AssertEquals("SumA Register TEST", cusTempStorageRegHeader.HumanReadableName);
	}

	public void TestSRH_PreviousReferenceType_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.SRH_PreviousReferenceType)
			.WithCaption("Previous Reference Type");
	});

	public void TestSRH_Status_Attributes()
	{
		_ = AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.SRH_Status)
			.WithCaption("Status");
	}

	public void TestSRH_CustomsOffice_Attributes()
	{
		_ = AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.SRH_CustomsOffice)
			.WithCaption("Customs Office");
	}

	public void TestSRH_ArrivalDate_Attributes()
	{
		_ = AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.SRH_ArrivalDate)
			.WithCaption("Arrival Date");
	}

	public void TestSRH_PresentationDate_Attributes()
	{
		_ = AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.SRH_PresentationDate)
			.WithCaption("Presentation Date");
	}

	public void TestSRH_PreviousReference_Attributes()
	{
		_ = AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.SRH_PreviousReference)
			.WithCaption("Previous Reference Number");
	}

	public void TestOnSaving_PopulateSRH_InternalReference()
	{
		var newFactory = new BusinessObjectFactory();
		var premisesWithoutWrapper = newFactory.New<CusTempStorageRegPremises>();
		premisesWithoutWrapper.SRP_Code = "ZZZ";
		premisesWithoutWrapper.SRP_Description = "Desc1";
		var orgHeader1 = newFactory.New<OrgHeader>();
		orgHeader1.OH_Code = "ZZZ";
		var orgAddress1 = newFactory.New<OrgAddress>();
		orgAddress1.OA_OH = orgHeader1.PK;
		orgAddress1.OA_Address1 = "Address1";
		premisesWithoutWrapper.SRP_OA_PremisesAddress = orgAddress1.PK;

		var premises = newFactory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc2";
		var orgHeader2 = newFactory.New<OrgHeader>();
		orgHeader2.OH_Code = "AH3";
		var orgAddress2 = newFactory.New<OrgAddress>();
		orgAddress2.OA_OH = orgHeader2.PK;
		orgAddress2.OA_Address1 = "Address2";
		premises.SRP_OA_PremisesAddress = orgAddress2.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.NumberPadding = 3;
		wrapper1.NumberPrefix = "PP";
		wrapper1.NumberSuffix = "SS";
		wrapper1.SN_MinimumValue = 10;
		wrapper1.SN_Count = 2;
		newFactory.Save();

		header.SRH_SRP_Premises = premises.PK;
		CombineAssertions(() =>
		{
			Factory.Save();
			AssertEquals("PP010SS", header.SRH_InternalReference);

			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_AppCode = "123";
			header2.SRH_Reference = "Reference2";
			header2.SRH_SRP_Premises = premises.PK;

			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_AppCode = "123";
			header3.SRH_Reference = "Reference3";
			header3.SRH_InternalReference = "AH3";
			header3.SRH_SRP_Premises = premises.PK;

			var header4 = Factory.New<CusTempStorageRegHeader>();
			header4.SRH_AppCode = "123";
			header4.SRH_Reference = "Reference4";
			header4.SRH_SRP_Premises = premises.PK;

			var header5 = Factory.New<CusTempStorageRegHeader>();
			header5.SRH_AppCode = "123";
			header5.SRH_Reference = "Reference5";
			header5.SRH_SRP_Premises = premisesWithoutWrapper.PK;
			Factory.Save();

			AssertEquals("Correct first number", "PP010SS", header.SRH_InternalReference);
			AssertEquals("Correct next number", "PP011SS", header2.SRH_InternalReference);
			AssertEquals("If it was set previous to save it is not modified", "AH3", header3.SRH_InternalReference);
			AssertEquals("Empty for no number available", ZString.Empty, header4.SRH_InternalReference);
			AssertEquals("Empty for null wrapper", ZString.Empty, header5.SRH_InternalReference);
		});
	}

	public void TestGetEDocsProviderSupporter()
	{
		AssertType<EDocsProviderSupporter>(header.GetEDocsProviderSupporter());
	}

	public void TestDocumentSupporter()
	{
		var supporter = header.DocumentSupporter;
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegHeaderDocumentSupporter>("Type", supporter);
			AssertSame("Cached", supporter, header.DocumentSupporter);
		});
	}

	public void TestDocManagerInfo()
	{
		var manager = header.DocManagerInfo;
		CombineAssertions(() =>
		{
			AssertType<DocManagerInfo>("Type", manager);
			AssertSame("Cached", manager, header.DocManagerInfo);
		});
	}

	public void TestNoAuditLog()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		Factory.Save();

		header.SRH_Reference = "Test Reference";
		Factory.Save();

		header.Delete();
		Factory.Save();

		CombineAssertions("No audit log for CusTempStorageRegHeader", () =>
		{
			Assert("No ADD log when created", !header.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode));
			Assert("No EDT log when edited", !header.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode));
			Assert("No DEL log when deleted", !header.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.DeletedARecordInTheSystemCode));
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	static BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var cusTempStorageRegHeader = factory.New<CusTempStorageRegHeader>();
		cusTempStorageRegHeader.SRH_ArrivalDate = ZDate.Today;
		cusTempStorageRegHeader.SRH_PresentationDate = ZDate.Today;
		cusTempStorageRegHeader.SRH_AppCode = "123";
		cusTempStorageRegHeader.SRH_Status = "OK";
		cusTempStorageRegHeader.SRH_Reference = "TEST";
		return cusTempStorageRegHeader;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "123";
		header.SRH_Reference = "Reference";
	}
	CusTempStorageRegHeader header;
}
