using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalHeaderForADAW))]
	public class GLJournalHeaderForADAWTest : GLJournalEnvironmentForADAWTest
	{
		public void TestPropertyValues()
		{
			BizObj.JournalType = "GJL";
			BizObj.PostPeriod = "202201";
			BizObj.ReverseOrEndPeriod = "202202";
			BizObj.JournalDescription = "June 22 Accrual Journal";
			BizObj.PresentationCategory = "Test";
			BizObj.GLJournalLines.AddNew();

			AssertEquals("Header Type", "H", BizObj.HeaderType);
			AssertEquals("Transaction Type", "GJL", BizObj.JournalType);
			AssertEquals("Period", "202201", BizObj.PostPeriod);
			AssertEquals("Reversing Period", "202202", BizObj.ReverseOrEndPeriod);
			AssertEquals("Journal Description", "June 22 Accrual Journal", BizObj.JournalDescription);
			AssertEquals("Presentation Category", "Test", BizObj.PresentationCategory);
			AssertEquals("GLJournal Lines", 1, BizObj.GLJournalLines.Count);
		}

		public override void TestProperties()
		{
			base.TestProperties();

			CombineAssertions(() =>
			{
				AssertNotNull("JournalType Property", BizObj.FindPropertyInfo(nameof(BizObj.JournalType)));
				AssertNotNull("PostPeriod Property", BizObj.FindPropertyInfo(nameof(BizObj.PostPeriod)));
				AssertNotNull("ReverseOrEndPeriod Property", BizObj.FindPropertyInfo(nameof(BizObj.ReverseOrEndPeriod)));
				AssertNotNull("JournalDescription Property", BizObj.FindPropertyInfo(nameof(BizObj.JournalDescription)));
				AssertNotNull("PresentationCategory Property", BizObj.FindPropertyInfo(nameof(BizObj.PresentationCategory)));
				AssertNotNull("PostDate Property", BizObj.FindPropertyInfo(nameof(BizObj.PostDate)));
				AssertNotNull("ReverseOrEndDate Property", BizObj.FindPropertyInfo(nameof(BizObj.ReverseOrEndDate)));
			});
		}

		public void TestMappingFieldMustHaveProperty()
		{
			var type = typeof(IGLJournalHeader);
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				if (property.CanWrite)
				{
					AssertNotNull($"Mapping field {property.Name} should have property.", BizObj.FindPropertyInfo(property.Name));
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLJournalHeaderForADAW(Factory, HeaderType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			HeaderType = "H";
			BizObj = new GLJournalHeaderForADAW(Factory, HeaderType);
			BizObj.RunPreSaveValidation();
		}

		string HeaderType;
		GLJournalHeaderForADAW BizObj;

		#endregion
	}
}
