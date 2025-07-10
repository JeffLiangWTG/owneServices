using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDatabaseInfo))]
	internal sealed class StorageDatabaseInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StorageDatabaseInfo();
		}

		#endregion
	}
}
