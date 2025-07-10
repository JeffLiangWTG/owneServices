using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public static class StorageDocsTestHelper
	{
		public static void AssertFileOrDocumentConfigured(BusinessObjectCollection edocsCollection, string fileType)
		{
			var document = (StorageDocsBase)edocsCollection.ToArray()[(edocsCollection.Count - 1)];
			Assertion.AssertEquals("Data type set", fileType, document.SC_DataType);
			Assertion.AssertGreaterThan<int>("Image data set", document.SC_ImageData.Length, 0);
		}
	}
}
