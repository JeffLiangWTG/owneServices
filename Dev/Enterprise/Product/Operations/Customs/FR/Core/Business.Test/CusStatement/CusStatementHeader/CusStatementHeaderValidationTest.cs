using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	public class CusStatementHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckB2_BranchDesignation()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_BranchDesignationInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(statement.B2_BranchDesignationInfo, StatementEntryTypeList.Codes.DCG, StatementEntryTypeList.Codes.Import);
		}

		public void TestCheckB2_OH_Importer()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			ValidationTestHelper.AssertErrorIfInvalidPK(statement.B2_OH_ImporterInfo, ZGuid.BrettsGuid, importer.PK);
			ValidationTestHelper.AssertErrorIfNotEntered(statement.B2_OH_ImporterInfo);
		}

		public void TestCheckB2_StatementType()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_StatementTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(statement.B2_StatementTypeInfo, "X", StatementPeriodicityList.Codes.Day);
		}

		public void TestCheckB2_PeriodStartDate()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_PeriodStartDateInfo);
		}

		public void TestCheckB2_PeriodEndDate()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_PeriodEndDateInfo);
		}

		public void TestCheckB2_EntryFilerCode()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_EntryFilerCodeInfo);

			statement.EntryNumber = ZString.Empty;
			AssertEquals(false, statement.B2_EntryFilerCodeInfo.ReadOnly);

			statement.B2_EntryFilerCode = "XXXX";
			AssertNoMessageError("If there is no valid options, user should be able to enter anything.", statement.B2_EntryFilerCodeInfo, ListValidation.InvalidCodeMessageError);

			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.TEN, "B92F8A8B");

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			statement.B2_OH_Importer = importer.PK;
			statement.B2_EntryFilerCode = "YYYY";
			AssertHasMessageError("There is one option in the list, user must choose that one.", statement.B2_EntryFilerCodeInfo, ListValidation.InvalidCodeMessageError);

			statement.B2_EntryFilerCode = "DGI002";
			AssertNoMessageError(statement.B2_EntryFilerCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB2_PaymentType()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_PaymentTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(statement.B2_StatementTypeInfo, "X", MethodOfPaymentList.Codes.M);
		}

		public void TestCheckB2_ImporterCustomsID()
		{
			AssertPropertyIsMandatoryWhenNotReadOnly(statement.B2_ImporterCustomsIDInfo);
		}

		public void TestCheckB2_CheckNo()
		{
			statement.B2_StatementNumber = "ABC123";
			ValidationTestHelper.AssertFieldIsNotMandatory(statement.B2_CheckNoInfo);

			statement.B2_StatementNumber = ZString.Empty;
			statement.B2_PaymentType = MethodOfPaymentList.Codes.A;
			ValidationTestHelper.AssertFieldIsNotMandatory(statement.B2_CheckNoInfo);

			// mandatory only when editable and payment type is R.
			statement.B2_PaymentType = MethodOfPaymentList.Codes.R;
			ValidationTestHelper.AssertErrorIfNotEntered(statement.B2_CheckNoInfo);
		}

		void AssertPropertyIsMandatoryWhenNotReadOnly(ZPropertyInfo propertyInfo)
		{
			statement.EntryNumber = ZString.Empty;
			AssertEquals($"Property {propertyInfo.Name} should be editable when EntryNumber is empty.", false, propertyInfo.ReadOnly);
			ValidationTestHelper.AssertErrorIfNotEntered(propertyInfo);

			statement.EntryNumber = "ABC123";
			AssertEquals($"Property {propertyInfo.Name} should be read only when EntryNumber is not empty.", true, propertyInfo.ReadOnly);
			ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
		}

		CusStatementHeader statement;
	}
}
