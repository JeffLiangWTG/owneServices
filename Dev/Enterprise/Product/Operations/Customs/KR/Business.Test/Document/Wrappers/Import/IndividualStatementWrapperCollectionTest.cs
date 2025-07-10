using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(IndividualStatementWrapperCollection))]
	sealed class IndividualStatementWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IndividualStatementWrapperCollection>
	{
		protected override IndividualStatementWrapperCollection GetCollectionToTest() => new IndividualStatementWrapperCollection(Factory.New<CusEntryHeader>(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234522000001M";
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "1234123456789012345";
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = entryNum.CE_EntryNum;

			var wrapper = new IndividualStatementWrapper(statement);
			wrapper.Decorate(entry);
			return wrapper;
		}
	}
}
