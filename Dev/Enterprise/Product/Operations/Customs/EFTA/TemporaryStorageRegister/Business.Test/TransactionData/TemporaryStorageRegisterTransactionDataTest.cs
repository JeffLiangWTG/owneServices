using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TemporaryStorageRegisterTransactionData))]
sealed class TemporaryStorageRegisterTransactionDataTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return new TemporaryStorageRegisterTransactionData(Factory);
	}
}
