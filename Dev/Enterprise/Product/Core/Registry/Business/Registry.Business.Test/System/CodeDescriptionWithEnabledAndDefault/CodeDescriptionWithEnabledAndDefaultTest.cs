using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithEnabledAndDefault))]
	sealed class CodeDescriptionWithEnabledAndDefaultTest : RegistryBusinessObjectTestCaseBase
	{
		#region Properties

		public void TestCode_ReadOnly()
		{
			var codeDescription = new CodeDescriptionWithEnabledAndDefault();

			codeDescription.IsSystemDefined = false;
			AssertEquals(false, codeDescription.CodeInfo.ReadOnly);

			codeDescription.IsSystemDefined = true;
			AssertEquals(true, codeDescription.CodeInfo.ReadOnly);
		}

		public void TestIsDefaultForBinding_SetsIsDefaultToFalseForOtherItemsInCollection()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection();
			var item1 = collection.AddNew();
			var item2 = collection.AddNew();
			var item3 = collection.AddNew();

			item1.IsDefaultForBinding = true;
			item2.IsDefaultForBinding = false;
			item3.IsDefaultForBinding = false;
			AssertEquals(true, item1.IsDefault);
			AssertEquals(false, item2.IsDefault);
			AssertEquals(false, item3.IsDefault);

			item2.IsDefaultForBinding = true;
			AssertEquals(false, item1.IsDefault);
			AssertEquals(true, item2.IsDefault);
			AssertEquals(false, item3.IsDefault);
		}

		#endregion

		#region ICanDelete

		public void TestCanDelete()
		{
			var codeDescription = new CodeDescriptionWithEnabledAndDefault();

			codeDescription.IsSystemDefined = false;
			AssertEquals(true, codeDescription.CanDelete);

			codeDescription.IsSystemDefined = true;
			AssertEquals(false, codeDescription.CanDelete);
			AssertEquals("This is system defined and cannot be deleted.", codeDescription.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CodeDescriptionWithEnabledAndDefault();
			result.Code = "AAA";
			result.Description = (NoResString)"AAA Desc";
			result.IsDefault = true;
			result.IsEnabled = true;
			result.IsSystemDefined = true;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
