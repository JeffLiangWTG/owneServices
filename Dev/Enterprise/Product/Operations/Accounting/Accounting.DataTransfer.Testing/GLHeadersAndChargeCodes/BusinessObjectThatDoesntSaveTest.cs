using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	[TestedType(typeof(BusinessObjectThatDoesntSave))]
	class BusinessObjectThatDoesntSaveTest : NonPersistentBusinessObjectTestCase
	{
	}
}
