using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

abstract class AdditionalReferenceBase : TestCaseWithFactory
{
	public abstract void TestConstructor();
	public abstract void TestReferenceNumber();
	public abstract void TestReferenceType();
	protected abstract IAdditionalReference CreateWrapper();
}

abstract class PreviousDocumentBase : TestCaseWithFactory
{
	public abstract void TestConstructor();
	public abstract void TestReferenceNumber();
	public abstract void TestDocumentType();
	public abstract void TestComplementOfInformation();
	protected abstract IPreviousDocument CreateWrapper();
}

abstract class SupportingDocumentBase : TestCaseWithFactory
{
	public abstract void TestConstructor();
	public abstract void TestDocumentType();
	public abstract void TestReferenceNumber();
	public abstract void TestItemNumber();
	public abstract void TestComplementOfInformation();
	protected abstract ISupportingDocument CreateWrapper();
}

abstract class TransportDocumentBase : TestCaseWithFactory
{
	public abstract void TestConstructor();
	public abstract void TestReferenceNumber();
	public abstract void TestDocumentType();
	protected abstract ITransportDocument CreateWrapper();
}

abstract class LocationOfGoodsBase : TestCaseWithFactory
{
	public abstract void TestConstructor();
	public abstract void TestTypeOfLocation();
	public abstract void TestQualifier();
	public abstract void TestAuthorisationNumber();
	public abstract void TestAdditionalIdentifier();
	public abstract void TestCustomsOffice();
	public abstract void TestAddress();
	public abstract void TestContact();
	protected abstract ILocationOfGoods CreateWrapper();
}

abstract class ConsignmentItemPreviousDocumentBase : TestCaseWithFactory
{
	public abstract void TestConstructor();
	public abstract void TestReferenceNumber();
	public abstract void TestDocumentType();
	public abstract void TestComplementOfInformation();
	public abstract void TestPackageType();
	public abstract void TestNumberOfPackages();
	public abstract void TestUnitOfQuantity();
	public abstract void TestQuantity();
	public abstract void TestGoodsItemIdentifier();
	protected abstract IConsignmentItemPreviousDocument CreateWrapper();
}
