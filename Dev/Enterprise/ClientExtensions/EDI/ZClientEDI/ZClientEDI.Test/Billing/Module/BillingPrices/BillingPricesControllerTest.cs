using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.BillingPrices.Testing
{
	[TestedType(typeof(BillingPricesController))]
	internal class BillingPricesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.BillingPrices;
		}

		public void TestShowEditForm()
		{
			ZString originalCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			ClientLicencePriceItem priceItem = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			Factory.Save();
			try
			{
				GlbCompany.CurrentCompany.GC_Code = "EDI";
				AssertNull("Last Shown form by default should be null", Controller.LastShownForm);
				Controller.ShowEditForm(priceItem);
				AssertEquals("The edit form should be opened and should be editable", ODisplayMode.Browse, Controller.LastShownForm.DisplayMode);
				Controller.LastShownForm.Dispose();
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_Code = originalCompanyCode;
			}
		}

		public new void TestNewForm()
		{
			ClientLicencePriceItem priceItem = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			Factory.Save();
			AssertControllerNotNull();
			try
			{
				Controller.ShowFormForNewEntity(priceItem);
				AssertNotNull("Last Shown form should not be empty", Controller.LastShownForm);
			}
			finally
			{
				Controller.LastShownForm.Dispose();
			}
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var mappings = new CodeDescriptionPairList();
			mappings.AddPair("Interface1", "PSQ 1000");
			EDIDataRegistry.Instance.ClientMappingBillingNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
			var price = base.GetBusinessObjectWithoutValidationErrors() as ClientLicencePriceItem;
			price.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			price.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			price.L7_Ref4 = "Interface1";
			return price;
		}
	}
}
