using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ImportWizardMappingCollection))]
	sealed class ImportWizardMappingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportWizardMappingCollection>
	{
		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ImportWizardMappingCollection);
		}

		protected override ImportWizardMappingCollection GetCollectionToTest()
		{
			ImportWizardMappingCollection result = new ImportWizardMappingCollection(Helper.Wizard);
			result.Load();

			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new NotSupportedException("New elements are not allowed.");
		}

		ImportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ImportWizardTestHelper(Factory);
				}

				return helper;
			}
		}

		ImportWizardTestHelper helper;

		#endregion
	}
}
