using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithSingleTrueCollection))]
	sealed class CodeDescriptionBoolWithSingleTrueCollectionTest : RegistryBusinessObjectCollectionTestCase<CodeDescriptionBoolWithSingleTrueCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CodeDescriptionBoolWithSingleTrueCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolWithSingleTrueCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolWithSingleTrue();
		}

		#endregion
	}
}
