using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStorageLinkPackageWrapper : DocBaseWrapper
{
	public static TemporaryStorageLinkPackageWrapper New(TemporaryStorageLinkPackage linkPackage, BusinessObjectFactory factoryToWrap) => new TemporaryStorageLinkPackageWrapper(linkPackage, factoryToWrap);

	public TemporaryStorageLinkPackageWrapper(object objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
	{
	}

	new TemporaryStorageLinkPackage ParentBusinessObject => (TemporaryStorageLinkPackage)base.ParentBusinessObject;

	public ZString Number => ParentBusinessObject.PackageNumber;
	public ZString Qty => ParentBusinessObject.PackQty.ToString();
	public ZString Type => ParentBusinessObject.Package.APA_PackUQ;
	public ZString MarksAndNumbers => ParentBusinessObject.Package.APA_MarksAndNumbers;
	public ZString Container => ParentBusinessObject.Package.Container?.ACN_ContainerNumber ?? ZString.Empty;
}
