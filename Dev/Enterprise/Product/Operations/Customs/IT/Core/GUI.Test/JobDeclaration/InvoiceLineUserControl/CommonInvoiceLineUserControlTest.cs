using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

internal abstract class CommonInvoiceLineUserControlTest<T> : TestCaseWithFactory
	where T : EUInvoiceLineUserControl, IHasTaxTabPageExposedForTest, IHasPanelLayoutMembersExposedForTest
{
	public void TestUserControlType()
	{
		using (var form = new ZForm())
		using (var control = GetNewInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var previousDocument = control.Controls.Find("PreviousDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(ExpectedPreviousDocumentsUserControlType, previousDocument.UserControlType);
			var additionalInfos = control.Controls.Find("additionalInfosUserControl1", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(ExpectedAdditionalInfosUserControlType, additionalInfos.UserControlType);
		}
	}

	public void TestTaxTabPageVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = GetNewInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();
			var taxTabPage = control.TaxTabPageExposed;
			AssertNotNull(taxTabPage);
			Assert(!taxTabPage.TabVisible);
		}
	}

	public void TestDynamicLayoutApplied()
	{
		using (var control = GetNewInvoiceLineUserControl())
		{
			AssertEquals("DynamicLayoutApplied", true, control.DynamicLayoutAppliedExposed);
		}
	}

	public void TestHasDifferentPanelLayout()
	{
		using (var control = GetNewInvoiceLineUserControl())
		{
			AssertEquals("HasDifferentPanelLayout", true, control.HasDifferentPanelLayoutExposed);
		}
	}

	public void TestInvoiceLineDetailsPanelLayoutType()
	{
		using (var control = GetNewInvoiceLineUserControl())
		{
			AssertType("InvoiceLineDetailsPanelLayoutType", InvoiceLineDetailsPanelLayoutType, control.GetNewInvoiceLineDetailsPanelLayoutExposed());
		}
	}

	protected abstract T GetNewInvoiceLineUserControl();

	protected virtual Type ExpectedAdditionalInfosUserControlType => typeof(AdditionalInfosUserControl);

	protected virtual Type ExpectedPreviousDocumentsUserControlType => typeof(PreviousDocumentsUserControl);

	protected abstract Type InvoiceLineDetailsPanelLayoutType { get; }
}

internal interface IHasTaxTabPageExposedForTest
{
	ZTabPage TaxTabPageExposed { get; }
}

internal interface IHasPanelLayoutMembersExposedForTest
{
	ZBool DynamicLayoutAppliedExposed { get; }
	ZBool HasDifferentPanelLayoutExposed { get; }
	IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayoutExposed();
}
