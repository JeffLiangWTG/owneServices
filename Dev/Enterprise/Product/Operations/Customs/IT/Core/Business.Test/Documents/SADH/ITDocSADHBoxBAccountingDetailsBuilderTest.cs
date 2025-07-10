using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITDocSADHBoxBAccountingDetailsBuilderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when entryHeader parameter is null", () => new ITDocSADHBoxBAccountingDetailsBuilder(null));
	}

	public void TestRegistrationDataRow()
	{
		CombineAssertions("Assert Registration Data Row (first row)", () =>
		{
			var boxBAccountingDetailsBuilder = GetNewBuilder();
			AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 0));

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			AssertEquals("First Row", "REG. 4 T-95870G 01/06/2021", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 0));
		});
	}

	public void TestDeferralAccountRow()
	{
		CombineAssertions("Assert Deferral Account Row (second row)", () =>
		{
			var boxBAccountingDetailsBuilder = GetNewBuilder();
			AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 1));

			declaration.JE_DefermentAccountNumber = "123456A";
			AssertEquals("First row", "", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 0));
			AssertEquals("Second row", "ACCOUNT N. 123456A", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 1));

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			entryHeader.EntryPayInfos.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "T", totalAmount: 1m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "T");
			AssertEquals("When A93 has been entered, Second row", "ACCOUNT N. 123456A - A93 N. 45678", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 1));

			declaration.JE_DefermentAccountNumber = "";
			AssertEquals("When A93 has been entered, Second row", "A93 N. 45678", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 1));
		});
	}

	public void TestDutyAmountGRow()
	{
		CombineAssertions("Assert Duty Amount G Row (third row)", () =>
		{
			var boxBAccountingDetailsBuilder = GetNewBuilder();
			AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "EUR 0.00", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 2));

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			var entryPayInfoCollection = entryHeader.EntryPayInfos;
			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "T", totalAmount: 1m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "T");
			AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "EUR 0.00", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 2));

			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "G", totalAmount: 55555.55m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "G");
			AssertEquals("When A93 with Payment Type G has been entered, Third row", "EUR 55555.55 EXP. 30/06/2021", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 2));
		});
	}

	public void TestDutyAmountERow()
	{
		using (Enterprise.Customs.EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("Assert Duty Amount E Row (fourth row)", () =>
			{
				var boxBAccountingDetailsBuilder = GetNewBuilder();
				AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "EUR 0.00", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 3));

				Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
				var entryPayInfoCollection = entryHeader.EntryPayInfos;
				entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "G", totalAmount: 1m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "G");
				AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "EUR 0.00", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 3));

				entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "E", totalAmount: 222.55, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "E");
				AssertEquals("When A93 with Payment Type E has been entered, Fourth row", "EUR 222.55 EXP. 30/06/2021", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 3));
			});
		}
	}

	public void TestDutyAmountFRow()
	{
		CombineAssertions("Assert Duty Amount F Row (fourth row)", () =>
		{
			var boxBAccountingDetailsBuilder = GetNewBuilder();
			AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "EUR 0.00", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 3));

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			var entryPayInfoCollection = entryHeader.EntryPayInfos;
			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "G", totalAmount: 1m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "G");
			AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), "EUR 0.00", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 3));

			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "F", totalAmount: 222.55, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "F");
			AssertEquals("When A93 with Payment Type F has been entered, Fourth row", "EUR 222.55 EXP. 30/06/2021", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 3));
		});
	}

	public void TestIvistoInfoBlock()
	{
		SetUpIvistoReferenceData();

		CombineAssertions(() =>
		{
			var boxBAccountingDetailsBuilder = GetNewBuilder();
			AssertNotContains("When IVISTO EntryNumber is not linked", "Notifica Visto Uscire per l'esportazione", boxBAccountingDetailsBuilder.GetAccountingDetails());

			var ivisto = Factory.NewCusEntryNumber(entryHeader, CusEntryNumberConstants.EntryTypes.Ivisto, ZString.Empty, new ZDateTime(2022, 01, 01));
			ivisto.CE_EntryStatus = "EXC";
			ivisto.CE_EntryLineReference = "IT301000";
			AssertEquals("First Row of IVISTO block", "Notifica Visto Uscire per l'esportazione", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 4));
			AssertEquals("Second Row of IVISTO block", "Esito: Uscita conclusa     01/01/2022", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 5));
			AssertEquals("Third Row of IVISTO block", "Dog. di destino: IT301000     Ufficio delle Dogane di C", GetRowOfAccountingDetails(boxBAccountingDetailsBuilder, 6));
		});
	}

	public void TestGetAccountingDetails()
	{
		SetUpIvistoReferenceData();

		var boxBAccountingDetailsBuilder = GetNewBuilder();
		var expectedEmptyAccountingDetails = @"

EUR 0.00
EUR 0.00";
		AssertEquals($"Empty {nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails)}", expectedEmptyAccountingDetails, boxBAccountingDetailsBuilder.GetAccountingDetails());

		var ivisto = Factory.NewCusEntryNumber(entryHeader, CusEntryNumberConstants.EntryTypes.Ivisto, ZString.Empty, new ZDateTime(2022, 01, 01));
		ivisto.CE_EntryStatus = "EXC";
		ivisto.CE_EntryLineReference = "IT301000";
		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
		declaration.JE_DefermentAccountNumber = "123456A";
		var entryPayInfoCollection = entryHeader.EntryPayInfos;
		entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "G", totalAmount: 55555.55m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "G");
		entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "F", totalAmount: 222.55, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "F");

		var expectedAccountingDetails = @"REG. 4 T-95870G 01/06/2021
ACCOUNT N. 123456A - A93 N. 45678
EUR 55555.55 EXP. 30/06/2021
EUR 222.55 EXP. 30/06/2021
Notifica Visto Uscire per l'esportazione
Esito: Uscita conclusa     01/01/2022
Dog. di destino: IT301000     Ufficio delle Dogane di C";
		AssertEquals(nameof(ITDocSADHBoxBAccountingDetailsBuilder.GetAccountingDetails), expectedAccountingDetails, boxBAccountingDetailsBuilder.GetAccountingDetails());
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	#region Implementation

	ITDocSADHBoxBAccountingDetailsBuilder GetNewBuilder() => new ITDocSADHBoxBAccountingDetailsBuilder(entryHeader);

	ZString GetRowOfAccountingDetails(ITDocSADHBoxBAccountingDetailsBuilder builder, int rowIndex)
	{
		return builder.GetAccountingDetails().Split("\r\n").ElementAtOrDefault(rowIndex);
	}

	void SetUpIvistoReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT301000", "Ufficio delle Dogane di CIVITAVECCHIA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	#endregion
}
