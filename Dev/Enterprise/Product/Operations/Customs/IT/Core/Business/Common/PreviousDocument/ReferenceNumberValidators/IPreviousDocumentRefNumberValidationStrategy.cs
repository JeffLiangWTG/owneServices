using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

interface IPreviousDocumentRefNumberValidationStrategy
{
	void CheckReferenceNumber(ZPropertyInfoString numberPropertyInfo);
}
