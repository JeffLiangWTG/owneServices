using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExtendedHoursRequestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWithErrorIfNotEnteredInCustomsOffice()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			parent.CustomsOffice = ZString.Empty;
			AssertHasErrorContaining(parent.CustomsOfficeInfo, MandatoryValidation.MustBeEntered);

			SetCusCodeData();
			parent.CustomsOffice = "033";
			AssertNoErrorContaining(parent.CustomsOfficeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestWithMessageErrorIfNotEnteredInDepartment()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			parent.Department = ZString.Empty;
			AssertHasMessageErrorContaining(parent.DepartmentInfo, MandatoryValidation.YouHaveNotEntered);

			SetCusCodeData();
			parent.Department = "10";
			AssertNoMessageErrorContaining(parent.DepartmentInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMeesgeErrorIfInvalidCodeInCustomesOffice()
		{
			SetCusCodeData();
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			parent.CustomsOffice = "X";
			AssertHasMessageErrorContaining(parent.CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			parent.CustomsOffice = "033";
			AssertNoMessageError(parent.CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestWithMeesgeErrorIfInvalidCodeInDepartment()
		{
			SetCusCodeData();
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			parent.Department = "X";
			AssertHasMessageErrorContaining(parent.DepartmentInfo, ListValidation.InvalidCodeMessageError);

			parent.Department = "10";
			AssertNoMessageError(parent.DepartmentInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2022, 10, 14)]
		public void TestWithMessageErrorIfNotEnteredInStartDate()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			parent.StartDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(parent.StartDateInfo, MandatoryValidation.YouHaveNotEntered);

			parent.StartDate = new ZDateTime(2021, 01, 01);
			AssertHasMessageErrorContaining(parent.StartDateInfo, "The 'Start Period' must be greater than or equal to today's date.");

			parent.StartDate = new ZDateTime(2022, 10, 14);
			AssertNoMessageErrorContaining(parent.StartDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.StartDateInfo, "The 'Start Period' must be greater than or equal to today's date.");
		}

		public void TestWithCheckDateIsAfterAnotherDateInEndDate()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			parent.StartDate = new ZDateTime(2021, 01, 01);
			parent.EndDate = new ZDateTime(2020, 12, 31);
			AssertHasMessageErrorContaining(parent.EndDateInfo, "The 'End Period' must be greater than the 'Start Period'.");

			parent.EndDate = new ZDateTime(2021, 01, 01);
			AssertHasMessageErrorContaining(parent.EndDateInfo, "The 'End Period' must be greater than the 'Start Period'.");

			parent.EndDate = new ZDateTime(2021, 01, 02);
			AssertNoMessageErrorContaining(parent.EndDateInfo, "The 'End Period' must be greater than the 'Start Period'.");
		}

		public void TestBranchPKMandatory()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, company.PK);
			parent.BranchPK = ZGuid.Empty;
			AssertHasErrorContaining(parent.BranchPKInfo, MandatoryValidation.MustBeEntered);

			parent.BranchPK = ZGuid.Invalid;
			AssertNoErrorContaining(parent.BranchPKInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(parent.BranchPKInfo, ListValidation.InvalidCodeError);

			parent.BranchPK = GlbBranch.CurrentBranch.PK;
			AssertHasErrorContaining("GlbBranch.CurrentBranch.PK does not belong to the passed-in company", parent.BranchPKInfo, ListValidation.InvalidCodeError);

			parent.BranchPK = branch.PK;
			AssertNoErrorContaining(parent.BranchPKInfo, ListValidation.InvalidCodeError);
		}

		public void TestWithErrorIfNotEnteredUnipassId()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			parent.Validation.ValidateMessageType();
			AssertHasErrorContaining(parent.MessageTypeInfo, "Please enter a UNIPASS Declarant ID in the registry.");

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");

			parent.Validation.ValidateMessageType();
			AssertNoErrorContaining(parent.MessageTypeInfo, "Please enter a UNIPASS Declarant ID in the registry.");
		}

		void SetCusCodeData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "033", "양산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Factory.Save();
		}
	}
}
