using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(AccTaxGLMovement))]
	public class AccTaxGLMovementTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 4, 1)]
		public void TestPeriod()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var testObject = Factory.NewWithValidTestData<AccTaxGLMovement>();
			testObject.ATM_Date = new ZDate(2020, 10, 31);
			AssertEquals(202010, testObject.ATM_Period);

			testObject.ATM_Date = new ZDate(2020, 3, 1);
			AssertEquals(202003, testObject.ATM_Period);
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		public void TestRoundingAmountCorrectly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chile))
			{
				var glMovement = Factory.NewWithValidTestData<AccTaxGLMovement>();
				glMovement.ATM_Amount = 9.99M;

				AssertEquals("Rounded up correctly", 10M, glMovement.ATM_Amount);
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected override BusinessObject GetNewBusinessObject()
		{
			var glMovement = Factory.NewWithValidTestData<AccTaxGLMovement>();
			glMovement.TaxTransaction.SubstituteGLMovementProcessor_ForTestOnly(new Mock<IGLMovementProcessor>().Object);

			return glMovement;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		[SuspendCriticalValidation]
		public void TestDelete()
		{
			ErrorReporter.Clear();

			var glMovement1 = Factory.NewWithValidTestData<AccTaxGLMovement>();
			Assert("Precondition: The GL Movement created is not in database", !glMovement1.IsInDatabase);

			glMovement1.Delete();

			Assert(glMovement1.IsDeleted);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			var glMovement2 = Factory.NewWithValidTestData<AccTaxGLMovement>();
			((INeedRow)glMovement2).Row.AcceptChanges();
			Assert("Precondition: The GL Movement created is in database", glMovement2.IsInDatabase);

			glMovement2.Delete();

			AssertEquals("AccTaxGLMovementInDBCannotBeDeleted", ErrorReporter.LastKeyReported);
			AssertContains($@"GL Movement in database cannot be deleted.
	PK = {glMovement2.PK}
	Type = AccTaxGLMovement", ErrorReporter.LastMessageReported);
			Assert(!glMovement2.IsDeleted);
			ErrorReporter.Clear();
		}
	}
}
