using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLineSubAccountForADAW))]
	public class GLJournalLineSubAccountForADAWTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertyValues()
		{
			BizObj.SubAccountType = "ORG";
			BizObj.SubAccountValue = "CARGOWSYD";

			AssertEquals("Sub Account Type", "ORG", BizObj.SubAccountType);
			AssertEquals("Sub Account Value", "CARGOWSYD", BizObj.SubAccountValue);
		}

		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("SubAccountType Property", BizObj.FindPropertyInfo(nameof(BizObj.SubAccountType)));
				AssertNotNull("SubAccountValue Property", BizObj.FindPropertyInfo(nameof(BizObj.SubAccountValue)));
			});
		}

		public void TestMappingFieldMustHaveProperty()
		{
			var type = typeof(IGLJournalLineSubAccount);
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
			return new GLJournalLineSubAccountForADAW(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizObj = new GLJournalLineSubAccountForADAW(Factory);
			BizObj.RunPreSaveValidation();
		}

		GLJournalLineSubAccountForADAW BizObj;

		#endregion
	}
}
