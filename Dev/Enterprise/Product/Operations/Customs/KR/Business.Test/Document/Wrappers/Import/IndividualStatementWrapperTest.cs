using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(IndividualStatementWrapper))]
	sealed class IndividualStatementWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEmptyFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_GB = ZGuid.Empty;
			var entry = declaration.ActiveEntryHeaders.AddNew();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

			var wrapper = new IndividualStatementWrapper(statement);
			wrapper.Decorate(entry);

			AssertEquals(ZDecimal.Zero, wrapper.TotalGrossWeightInKG);
			AssertEquals(ZInt.Zero, wrapper.TotalPackQty);
			AssertEquals(ZDateTime.Empty, wrapper.DeclarationDate);
			AssertEquals(null, wrapper.Declarant);
			AssertEquals(ZString.Empty, wrapper.DeclarantCompanyName);
			AssertEquals(ZString.Empty, wrapper.DeclarantPhoneNumber);
			AssertEquals(ZString.Empty, wrapper.HouseBillNumber);
			AssertEquals(ZString.Empty, wrapper.FormattedImportDeclarationNumber);
			AssertEquals(ZString.Empty, wrapper.CustomsOfficeName);
			AssertEquals(ZString.Empty, wrapper.FormattedStatementNumberFirstLine);
			AssertEquals(ZString.Empty, wrapper.FormattedStatementNumberSecondLine);
		}
		protected override BusinessObject GetNewBusinessObject() => new IndividualStatementWrapper(Factory.New<CusStatementHeader>());
	}
}
