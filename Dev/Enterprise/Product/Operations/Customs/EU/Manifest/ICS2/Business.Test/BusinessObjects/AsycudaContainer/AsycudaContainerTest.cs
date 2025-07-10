using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

[TestedType(typeof(AsycudaContainer))]
sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidation()
	{
		var container = (AsycudaContainer)GetNewBusinessObject();
		AssertType<AsycudaContainerValidation>(container.Validation);
	}

	public void TestACN_IsShipperOwnedCaption()
	{
		var container = (AsycudaContainer)GetNewBusinessObject();
		var captionData = DataBoundResourceStrings.GetDataForProperty(container.ACN_IsShipperOwnedInfo);

		AssertEquals("Caption", "Shipper's Own Container", captionData.Caption);
		AssertEquals("MediumCaption", "Shipper's Own Cont.", captionData.MediumCaption);
		AssertEquals("ShortCaption", "SOC", captionData.ShortCaption);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObjectForDeleteTest(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var container = header.Containers.AddNew();
		return container;
	}
}
