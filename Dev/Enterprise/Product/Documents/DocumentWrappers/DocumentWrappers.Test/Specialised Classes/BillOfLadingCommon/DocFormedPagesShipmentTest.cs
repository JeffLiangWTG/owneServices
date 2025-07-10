using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocFormedPagesShipment))]
	sealed class DocFormedPagesShipmentTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocFormedPagesShipment();
		}

		#endregion
	}
}
