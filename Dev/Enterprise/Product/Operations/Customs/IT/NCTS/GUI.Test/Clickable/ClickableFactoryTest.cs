using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(ClickableFactory))]
sealed class ClickableFactoryTest : TestCaseWithFactory
{
	public void TestCreateClickableIrildesRequestMenuItemComponent()
		=> AssertMenuItemComponent<ClickableIrildesRequestContext>(h
			=> ClickableFactory.CreateClickableIrildesRequestMenuItemComponent(h));

	public void TestCreateClickableNctsElectronicFolderStatusRequestMenuItemComponent()
		=> AssertMenuItemComponent<ClickableNctsElectronicFolderStatusRequestContext>(h
			=> ClickableFactory.CreateClickableNctsElectronicFolderStatusRequestMenuItemComponent(h));

	public void TestCreateClickableTransitAccompanyingDocumentRequestMenuItemComponent()
		=> AssertMenuItemComponent<ClickableTransitAccompanyingDocumentRequestContext>(h
			=> ClickableFactory.CreateClickableTransitAccompanyingDocumentRequestMenuItemComponent(h));

	void AssertMenuItemComponent<T>(Func<NctsHeader, ClickableMenuItemComponent> getMenuItemComponent)
	{
		AssertArgumentExceptionThrown<ArgumentNullException>("header", () => getMenuItemComponent(null));
		AssertMenuItemComponent<T>(getMenuItemComponent(nctsHeader));
	}

	void AssertMenuItemComponent<T>(ClickableMenuItemComponent menuItemComponent)
	{
		AssertNotNull(menuItemComponent);
		AssertType<ClickableMenuItemComponent>("Component type", menuItemComponent);
		AssertType<T>("Component context type", menuItemComponent.ClickableContext);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;
}
