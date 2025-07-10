using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(DocumentWrapperCollection<HBLCargoRegistrationInformationItemWrapper, HBLRegistrationInformationItemProvider>))]
sealed class DocumentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentWrapperCollection<HBLCargoRegistrationInformationItemWrapper, HBLRegistrationInformationItemProvider>>
{
	protected override DocumentWrapperCollection<HBLCargoRegistrationInformationItemWrapper, HBLRegistrationInformationItemProvider> GetCollectionToTest()
	{
		return new DocumentWrapperCollection<HBLCargoRegistrationInformationItemWrapper, HBLRegistrationInformationItemProvider>([], Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new HBLCargoRegistrationInformationItemWrapper(new HBLRegistrationInformationItemProvider(), 0);
	}
}
