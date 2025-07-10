using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocBillOfLadingFormedPage))]
	sealed class DocBillOfLadingFormedPageTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocBillOfLadingFormedPage();
		}

		#endregion
	}
}
