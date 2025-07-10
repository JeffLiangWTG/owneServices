using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Accounting.Export.Business.Testing
{
	public class AccountingTransactionDataObjectWriterStrategyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var defaultCtor = new AccountingTransactionDataObjectWriterStrategy();
			AssertSame(DefaultDataObjectWriterStrategy.Instance, defaultCtor.ParentStrategy);
			AssertSame(DefaultAccountingTransactionWriterStrategy.Instance, defaultCtor.AccountingStrategy);
			AssertEquals("None", defaultCtor.Context);

			var mockWriterStrategy = new Mock<IDataObjectWriterStrategy>();
			var withParentStrategy = new AccountingTransactionDataObjectWriterStrategy(parentStrategy: mockWriterStrategy.Object);
			AssertSame(mockWriterStrategy.Object, withParentStrategy.ParentStrategy);
			AssertSame(DefaultAccountingTransactionWriterStrategy.Instance, withParentStrategy.AccountingStrategy);
			AssertEquals("None", withParentStrategy.Context);

			var mockAccountingStrategy = new Mock<IAccountingTransactionWriterStrategy>();
			var withAccountingStrategy = new AccountingTransactionDataObjectWriterStrategy(accountingStrategy: mockAccountingStrategy.Object);
			AssertSame(DefaultDataObjectWriterStrategy.Instance, withAccountingStrategy.ParentStrategy);
			AssertSame(mockAccountingStrategy.Object, withAccountingStrategy.AccountingStrategy);
			AssertEquals("None", withAccountingStrategy.Context);

			var withContext = new AccountingTransactionDataObjectWriterStrategy(context: "SomeHelpfulContextText");
			AssertSame(DefaultDataObjectWriterStrategy.Instance, withContext.ParentStrategy);
			AssertSame(DefaultAccountingTransactionWriterStrategy.Instance, withContext.AccountingStrategy);
			AssertEquals("SomeHelpfulContextText", withContext.Context);
		}

		public void TestIsAllowSet_FallsBackToParentStrategy_WhenAccountingContextIsTrue()
		{
			var mockParentReturningTrue = new Mock<IDataObjectWriterStrategy>();
			mockParentReturningTrue.Setup(x => x.IsAllowSet(It.IsAny<string>())).Returns(true);
			var mockAccountingReturningTrue = new Mock<IAccountingTransactionWriterStrategy>();
			mockAccountingReturningTrue.Setup(x => x.IsAllowSetForAccountingContext(It.IsAny<string>())).Returns(true);
			var strategy = new AccountingTransactionDataObjectWriterStrategy(
				parentStrategy: mockParentReturningTrue.Object,
				accountingStrategy: mockAccountingReturningTrue.Object
			);

			Assert("When the Accounting Strategy returns true, the parent strategy should be used.", strategy.IsAllowSet("AnyField"));
			mockParentReturningTrue.Verify(x => x.IsAllowSet(It.IsAny<string>()), Times.AtLeastOnce());
			mockAccountingReturningTrue.Verify(x => x.IsAllowSetForAccountingContext(It.IsAny<string>()), Times.AtLeastOnce());

			var mockParentReturningFalse = new Mock<IDataObjectWriterStrategy>();
			mockParentReturningFalse.Setup(x => x.IsAllowSet(It.IsAny<string>())).Returns(false);
			var strategy2 = new AccountingTransactionDataObjectWriterStrategy(
				parentStrategy: mockParentReturningFalse.Object,
				accountingStrategy: mockAccountingReturningTrue.Object
			);

			Assert("When the Accounting Strategy returns true, the parent strategy should be used.", !strategy2.IsAllowSet("AnyField"));
			mockParentReturningFalse.Verify(x => x.IsAllowSet(It.IsAny<string>()), Times.AtLeastOnce());
			mockAccountingReturningTrue.Verify(x => x.IsAllowSetForAccountingContext(It.IsAny<string>()), Times.AtLeastOnce());
		}

		public void TestIsAllowSet_DoesNotUseParentStrategy_WhenAccountingContextIsFalse()
		{
			var mockParent = new Mock<IDataObjectWriterStrategy>();
			var mockAccountingReturningFalse = new Mock<IAccountingTransactionWriterStrategy>();
			mockAccountingReturningFalse.Setup(x => x.IsAllowSetForAccountingContext(It.IsAny<string>())).Returns(false);
			var strategy = new AccountingTransactionDataObjectWriterStrategy(
				parentStrategy: mockParent.Object,
				accountingStrategy: mockAccountingReturningFalse.Object
			);

			CombineAssertions("When the Accounting Strategy returns false, the parent strategy should not be called.", () =>
			{
				Assert(!strategy.IsAllowSet("AnyField"));
				mockAccountingReturningFalse.Verify(x => x.IsAllowSetForAccountingContext(It.IsAny<string>()), Times.AtLeastOnce());
				mockParent.Verify(x => x.IsAllowSet(It.IsAny<string>()), Times.Never());
			});
		}

		public void TestDefaultAccountingTransactionWriterStrategy_ReturnsTrueForAnyFieldOfXUT_ExceptBlacklist()
		{
			var strategy = new DefaultAccountingTransactionWriterStrategy();
			var allXUTFields = typeof(TransactionInfo).GetProperties().Select(pi => pi.Name).ToArray();
			AssertGreaterThan("Precondition: XUT has fields (and I haven't messed up the reflection code)", allXUTFields.Length, 0);

			var expectedFalseFields = new[] {
				"AuthorizationDetailCollection",
				"TransactionHeaderReferenceCollection"
			};
			var expectedTrueFields = allXUTFields.Except(expectedFalseFields);
			foreach (var field in allXUTFields)
			{
				var expectedResult = expectedTrueFields.Contains(field);
				AssertEquals($"Field '{field}' should return {expectedResult}", expectedResult, strategy.IsAllowSetForAccountingContext(field));
			}
		}

		public void TestFieldListDisallowedAccountingTransactionWriterStrategy_ReturnsFalseForDisallowedFields()
		{
			var strategy = new FieldListDisallowedAccountingTransactionWriterStrategy(new[] { "AuthorizationDetailCollection", "OtherField" });
			AssertEquals("SomeField is not disallowed and should return true", true, strategy.IsAllowSetForAccountingContext("SomeField"));
			AssertEquals("Blah is not disallowed and should return true", true, strategy.IsAllowSetForAccountingContext("Blah"));
			AssertEquals("Empty string is not disallowed and should return true", true, strategy.IsAllowSetForAccountingContext(""));
			AssertEquals("Null is not disallowed and should return true", true, strategy.IsAllowSetForAccountingContext(null));

			AssertEquals("AuthorizationDetailCollection is disallowed and should return false", false, strategy.IsAllowSetForAccountingContext("AuthorizationDetailCollection"));
			AssertEquals("OtherField is disallowed and should return false", false, strategy.IsAllowSetForAccountingContext("OtherField"));

			var nullAndEmptyStrategy = new FieldListDisallowedAccountingTransactionWriterStrategy(new[] { "", null });
			AssertEquals("Empty string should returns false when disallowed (as silly as that might be)", false, nullAndEmptyStrategy.IsAllowSetForAccountingContext(""));
			AssertEquals("Null should return false when disallowed (as silly as that might be)", false, nullAndEmptyStrategy.IsAllowSetForAccountingContext(null));

			var immutableStrategy = new FieldListDisallowedAccountingTransactionWriterStrategy(new[] { "AuthorizationDetailCollection", "OtherField" }.ToImmutableHashSet());
			AssertEquals("SomeField is not disallowed and should return true", true, immutableStrategy.IsAllowSetForAccountingContext("SomeField"));
			AssertEquals("AuthorizationDetailCollection is disallowed and should return false", false, immutableStrategy.IsAllowSetForAccountingContext("AuthorizationDetailCollection"));
		}

		public void TestFieldListDisallowedAccountingTransactionWriterStrategy_ReturnsTrueForAnyFieldOfXUT_WhenNoDisallowedFields()
		{
			var strategy = new FieldListDisallowedAccountingTransactionWriterStrategy(Array.Empty<string>());
			var allXUTFields = typeof(TransactionInfo).GetProperties().Select(pi => pi.Name).ToArray();
			AssertGreaterThan("Precondition: XUT has fields (and I haven't messed up the reflection code)", allXUTFields.Length, 0);
			foreach (var field in allXUTFields)
			{
				AssertEquals($"All fields should return true: {field}", true, strategy.IsAllowSetForAccountingContext(field));
			}
		}

		public void TestAllFieldsAllowedAccountingTransactionWriterStrategy_ReturnsTrueForAnyFieldOfXUT()
		{
			var strategy = new AllFieldsAllowedAccountingTransactionWriterStrategy();
			var allXUTFields = typeof(TransactionInfo).GetProperties().Select(pi => pi.Name).ToArray();
			AssertGreaterThan("Precondition: XUT has fields (and I haven't messed up the reflection code)", allXUTFields.Length, 0);
			foreach (var field in allXUTFields)
			{
				AssertEquals($"All fields should return true: {field}", true, strategy.IsAllowSetForAccountingContext(field));
			}
		}
	}
}
