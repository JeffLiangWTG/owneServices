using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.ODPL.Test
{
	[TestedType(typeof(OdplModuleUsage))]
	internal class OdplModuleUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUnitCount()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", 0, odplModule.UnitCount);

			odplModule.StaffCount = 13;
			AssertEquals(13, odplModule.UnitCount);

			odplModule.FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(1, odplModule.UnitCount);

			odplModule.FeeType = BillingConstants.FeeType.Database;
			AssertEquals(1, odplModule.UnitCount);

			odplModule.StaffCount = 0;
			AssertEquals(0, odplModule.UnitCount);

			odplModule.FeeType = "XXX";
			odplModule.StaffCount = 10;
			AssertEquals(10, odplModule.UnitCount);
		}

		public void TestMixedUnitCount()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", 0, odplModule.MixedUnitCount);

			odplModule.StaffCount = 13;
			AssertEquals(13, odplModule.MixedUnitCount);

			odplModule.FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(1, odplModule.MixedUnitCount);

			odplModule.FeeType = BillingConstants.FeeType.Database;
			AssertEquals(1, odplModule.MixedUnitCount);

			odplModule.FeeType = "XXX";
			odplModule.PurchasedStaffCount = 13;
			AssertEquals(13, odplModule.MixedUnitCount);

			odplModule.PurchasedStaffCount = 14;
			AssertEquals(14, odplModule.MixedUnitCount);
		}

		public void TestAmount()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", 0m, odplModule.Amount);

			odplModule.UnitPrice = 2m;
			odplModule.StaffCount = 8;
			AssertEquals(16m, odplModule.Amount);

			odplModule.FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(2m, odplModule.Amount);
		}

		public void TestAmountRounding()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", 0m, odplModule.Amount);

			odplModule.UnitPrice = 1.1357m;
			odplModule.StaffCount = 10;
			AssertEquals("Amount rounded", 11.36m, odplModule.Amount);

			odplModule.UnitPrice = 1.1354m;
			odplModule.StaffCount = 10;
			AssertEquals("Amount rounded", 11.35m, odplModule.Amount);
		}

		public void TestLicenceUnitsAmount()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", 0m, odplModule.LicenceUnitsAmount);

			odplModule.LicenceUnits = 3;
			odplModule.StaffCount = 9;
			AssertEquals(27m, odplModule.LicenceUnitsAmount);

			odplModule.FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(3m, odplModule.LicenceUnitsAmount);
		}

		public void TestFeeTypeDescription()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", "", odplModule.FeeTypeDescription);

			var feeTypesList = BillingConstants.GetFeeTypeList();

			odplModule.FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(feeTypesList.GetDescriptionFromCode(odplModule.FeeType), odplModule.FeeTypeDescription);

			odplModule.ModuleCode = BillingConstants.CoreModuleCode;
			AssertEquals(feeTypesList.GetDescriptionFromCode(odplModule.FeeType), odplModule.FeeTypeDescription);

			odplModule.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage = true;
			AssertEquals(BillingConstants.FeeTypeDescriptions.RegisteredUser, odplModule.FeeTypeDescription);

			odplModule.ModuleCode = "FOR";
			odplModule.FeeType = BillingConstants.FeeType.NamedUser;
			AssertEquals(feeTypesList.GetDescriptionFromCode(odplModule.FeeType), odplModule.FeeTypeDescription);

			odplModule.FeeType = BillingConstants.FeeType.CoreUsers;
			AssertEquals(BillingConstants.FeeTypeDescriptions.RegisteredUser, odplModule.FeeTypeDescription);

			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_FeeType = BillingConstants.FeeType.DatabaseLanguageZ;
			priceItem.L7_Price = 16m;
			priceItem.L7_Order = 8;
			priceItem.L7_Description = "DatabaseLanguageZ - abc";
			priceItem.L7_Language = Core.SharedConstants.Languages.ChineseSimplified;
			priceItem.L7_ChargeBasis = "Database - 123";
			odplModule.PopulateFromPriceItem(priceItem);
			AssertEquals("Database - 123", odplModule.FeeTypeDescription);
		}

		public void TestShowOnSummary()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", false, odplModule.ShowOnSummary);

			odplModule.UnitPrice = 2m;
			odplModule.StaffCount = 8;
			AssertEquals(true, odplModule.ShowOnSummary);

			odplModule.StaffCount = 0;
			odplModule.PurchasedStaffCount = 4;
			AssertEquals("purchased seats only are shown", true, odplModule.ShowOnSummary);

			odplModule.StaffCount = 8;
			odplModule.UnitPrice = 0m;
			AssertEquals(false, odplModule.ShowOnSummary);

			odplModule.FeeType = BillingConstants.FeeType.Included;
			AssertEquals("Even when included modules have staffcount, not showing them", false, odplModule.ShowOnSummary);
		}

		public void TestIsProductionModule()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", true, odplModule.IsProductionModule);

			odplModule.ModuleCode = "COR";
			AssertEquals(true, odplModule.IsProductionModule);

			odplModule.ModuleCode = BillingConstants.NonProductionDatabase.CoreModuleCode;
			AssertEquals(false, odplModule.IsProductionModule);

			odplModule.ModuleCode = "XXX";
			AssertEquals(true, odplModule.IsProductionModule);
		}

		public void TestPopulateFromPriceItem()
		{
			OdplModuleUsage odplModule = new OdplModuleUsage(Factory);
			AssertEquals("Precondition", "", odplModule.FeeType);
			AssertEquals("Precondition", 0m, odplModule.UnitPrice);
			AssertEquals("Precondition", 0, odplModule.Order);
			AssertEquals("Precondition", "", odplModule.ModuleName);

			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_FeeType = BillingConstants.FeeType.Module;
			priceItem.L7_Price = 16m;
			priceItem.L7_Order = 8;
			priceItem.L7_Description = "Supa module";

			odplModule.PopulateFromPriceItem(priceItem);
			AssertEquals(BillingConstants.FeeType.Module, odplModule.FeeType);
			AssertEquals(16m, odplModule.UnitPrice);
			AssertEquals(8, odplModule.Order);
			AssertEquals("Supa module", odplModule.ModuleName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OdplModuleUsage(Factory);
		}

		#endregion
	}
}
