using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailFieldCollection))]
	public class EmailFieldCollectionTest : EmailFieldCollectionTest<EmailFieldCollection>
	{
		protected override EmailFieldCollection GetCollectionToTest()
		{
			return new EmailFieldCollection();
		}
	}

	public abstract class EmailFieldCollectionTest<T> : RegistryBusinessObjectCollectionTemplateTestCase<T> where T : EmailFieldCollection
	{
		public void TestGetEmailFieldCountWithIndex()
		{
			AssertNotNull(Collection);

			EmailField field = Collection.AddNew();
			field.Index = "1";
			AssertEquals(1, Collection.GetEmailFieldCountWithIndex("1"));

			field = Collection.AddNew();
			field.Index = "1";
			AssertEquals(2, Collection.GetEmailFieldCountWithIndex("1"));
		}

		public void TestGetEmailFieldCountWithCode()
		{
			AssertNotNull(Collection);

			EmailField field = Collection.AddNew();
			field.Code = Core.Constants.EmailFormat.EmailFieldCodes.BranchCode;
			AssertEquals(1, Collection.GetEmailFieldCountWithCode(Core.Constants.EmailFormat.EmailFieldCodes.BranchCode));

			field = Collection.AddNew();
			field.Code = Core.Constants.EmailFormat.EmailFieldCodes.BranchCode;
			AssertEquals(2, Collection.GetEmailFieldCountWithCode(Core.Constants.EmailFormat.EmailFieldCodes.BranchCode));
		}

		#region Implementation
		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EmailField();
		}

		#endregion
	}
}
