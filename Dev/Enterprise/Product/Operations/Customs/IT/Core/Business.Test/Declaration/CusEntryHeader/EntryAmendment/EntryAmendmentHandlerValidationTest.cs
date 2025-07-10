using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using static Enterprise.Integration.Customs.IT;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryAmendmentHandlerValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMovementReferenceNumber_Entered()
	{
		var handler = new EntryAmendmentHandler(Factory);
		ValidationTestHelper.AssertErrorIfNotEntered(handler.MovementReferenceNumberInfo);
	}

	public void TestCheckMovementReferenceNumber_MaxLength()
	{
		CombineAssertions(() =>
		{
			const string message = "MRN length must be 18 characters";

			var handler = new EntryAmendmentHandler(Factory);
			handler.MovementReferenceNumber = "123456789";
			AssertHasError("Not 18 characters", handler.MovementReferenceNumberInfo, message);

			handler.MovementReferenceNumber = "123456789012345678";
			AssertNoError("18 characters", handler.MovementReferenceNumberInfo, message);
		});
	}

	public void TestCheckMovementReferenceNumber_Format()
	{
		CombineAssertions(() =>
		{
			const string message = "Please enter a MRN in the following format";

			var handler = new EntryAmendmentHandler(Factory);
			handler.MovementReferenceNumber = "123456789012345678";
			AssertHasMessageErrorContaining("Invalid format", handler.MovementReferenceNumberInfo, message);

			handler.MovementReferenceNumber = "24ITQ0B08AAB1911J3";
			AssertNoMessageErrorContaining("Valid format", handler.MovementReferenceNumberInfo, message);
		});
	}

	public void TestCheckMovementReferenceNumber_DuplicatedDeclaration()
	{
		CombineAssertions(() =>
		{
			const string mrn = "24ITQYG08AAB1956J4";

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.MovementReferenceNumberSetter(mrn);

			var handler = new EntryAmendmentHandler(Factory);
			handler.MovementReferenceNumber = mrn;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertHasWarning("Duplicated Declaration", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);

			handler.MovementReferenceNumber = "24ITQYG08AAB1956J5";
			handler.Validation.ValidateMovementReferenceNumber();
			AssertNoWarning("Different CE_EntryNum", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);

			var entryNumber = CusEntryNumber.Load<CusEntryNumber>(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Italy);

			entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			handler.MovementReferenceNumber = mrn;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertNoWarning("Different CE_EntryType", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);

			entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertNoWarning("Different CE_RN_NKCountryCode", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);

			var entryNumberOfAnotherBO = CusEntryNumber.LoadOrCreate<CusEntryNumber>(Factory.New<JobDeclaration>(), CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Italy);
			entryNumberOfAnotherBO.CE_EntryNum = mrn;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertNoWarning("Different CE_ParentTable", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);

			var nctsHeader = Factory.New<ICusInBondHeader>() as BusinessObject;
			var entryNumberNcts = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Italy);
			entryNumberNcts.CE_EntryNum = mrn;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertHasWarning("Duplicated NCTS", handler.MovementReferenceNumberInfo, DuplicatedNctsMessage);
		});
	}

	public void TestCheckMovementReferenceNumber_DuplicatedNctsHeader()
	{
		CombineAssertions(() =>
		{
			const string mrn = "24ITQYG08AAB1956J4";

			var nctsHeader = Factory.New<ICusInBondHeader>() as BusinessObject;
			var entryNumber = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Italy);
			entryNumber.CE_EntryNum = mrn;

			var handler = new EntryAmendmentHandler(Factory);
			handler.MovementReferenceNumber = mrn;
			AssertHasWarning("Duplicated NCTS", handler.MovementReferenceNumberInfo, DuplicatedNctsMessage);

			handler.MovementReferenceNumber = "24ITQYG08AAB1956J5";
			AssertNoWarning("Different CE_EntryNum", handler.MovementReferenceNumberInfo, DuplicatedNctsMessage);

			entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			handler.MovementReferenceNumber = mrn;
			AssertNoWarning("Different CE_EntryType", handler.MovementReferenceNumberInfo, DuplicatedNctsMessage);

			entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertNoWarning("Different CE_RN_NKCountryCode", handler.MovementReferenceNumberInfo, DuplicatedNctsMessage);

			var entryNumberOfAnotherBO = CusEntryNumber.LoadOrCreate<CusEntryNumber>(Factory.New<JobDeclaration>(), CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Italy);
			entryNumberOfAnotherBO.CE_EntryNum = mrn;
			handler.Validation.ValidateMovementReferenceNumber();
			AssertNoWarning("Different CE_ParentTable", handler.MovementReferenceNumberInfo, DuplicatedNctsMessage);

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.MovementReferenceNumberSetter(mrn);
			handler.Validation.ValidateMovementReferenceNumber();
			AssertHasWarning("Duplicated Declaration", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);
		});
	}

	public void TestCheckMovementReferenceNumber_InvalidAndDuplicated()
	{
		CombineAssertions(() =>
		{
			const string mrn = "24IT12345678901235";

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.MovementReferenceNumberSetter(mrn);

			var handler = new EntryAmendmentHandler(Factory);
			handler.MovementReferenceNumber = mrn;
			handler.Validation.ValidateMovementReferenceNumber();

			AssertHasWarning("Duplicated Declaration", handler.MovementReferenceNumberInfo, DuplicatedDeclarationMessage);
			AssertHasMessageError("Invalid last digit", handler.MovementReferenceNumberInfo, "MRN does not have a valid last digit. The last digit should be 4");
		});
	}

	public void TestCheckTotalEntryLines_Range()
	{
		CombineAssertions(() =>
		{
			const string errorMessage = "Total Entry Lines: enter a numeric value between 1 and 99999";

			var handler = new EntryAmendmentHandler(Factory);
			handler.Validation.ValidateTotalEntryLines();
			AssertHasError("TotalEntryLines not set", handler.TotalEntryLinesInfo, errorMessage);

			handler.TotalEntryLines = 0;
			AssertHasError("TotalEntryLines set to 0", handler.TotalEntryLinesInfo, errorMessage);

			handler.TotalEntryLines = 123456;
			AssertHasError("TotalEntryLines set to greater than 99999", handler.TotalEntryLinesInfo, errorMessage);

			handler.TotalEntryLines = 12345;
			AssertNoError("TotalEntryLines is within acceptable range", handler.TotalEntryLinesInfo, errorMessage);
		});
	}

	const string DuplicatedNctsMessage = "The MRN number entered is already present in another NCTS Departure Declaration";
	const string DuplicatedDeclarationMessage = "The MRN number entered is already present in an Entry of another Customs Declaration";
}
