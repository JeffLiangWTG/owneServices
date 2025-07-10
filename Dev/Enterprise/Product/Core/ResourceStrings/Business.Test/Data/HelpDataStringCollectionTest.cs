using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(HelpDataStringCollection))]
	sealed class HelpDataStringCollectionTest : NonPersistentBusinessObjectCollectionTestCase<HelpDataStringCollection>
	{
		#region Implementation

		protected override HelpDataStringCollection GetCollectionToTest()
		{
			return new HelpDataStringCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HelpDataString();
		}

		#endregion
	}
}
