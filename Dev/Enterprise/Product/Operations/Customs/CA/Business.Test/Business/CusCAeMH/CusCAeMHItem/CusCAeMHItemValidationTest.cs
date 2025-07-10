using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFirstUNDG()
		{
			var lineItem = Factory.New<CusCAeMHItem>();
			var undgDataItem = lineItem.UNDGs.AddNew();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "AS";
			subs.DG_UNNO = "AS";
			subs.DG_MP = "M";

			lineItem.BX_IsDangerousInBulk = false;
			lineItem.FirstUNDG = ZGuid.Empty;
			lineItem.Validation.ValidateFirstUNDG();
			AssertNoMessageErrorContaining(lineItem.FirstUNDGInfo, MandatoryValidation.YouHaveNotEntered);

			lineItem.BX_IsDangerousInBulk = true;
			lineItem.Validation.ValidateFirstUNDG();
			AssertHasMessageErrorContaining(lineItem.FirstUNDGInfo, MandatoryValidation.YouHaveNotEntered);

			lineItem.FirstUNDG = subs.PK;
			lineItem.Validation.ValidateFirstUNDG();
			AssertNoMessageErrorContaining(lineItem.FirstUNDGInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBX_QuantityUQ()
		{
			item.BX_QuantityUQ = "XX";
			AssertHasMessageErrorContaining(item.BX_QuantityUQInfo, ListValidation.InvalidCodeMessageError);
			item.BX_QuantityUQ = ACROSSPackageTypes.Codes.PACKAGE;
			AssertNoMessageErrorContaining(item.BX_QuantityUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBX_QuantityUQ2()
		{
			item.BX_Quantity = 100m;
			item.BX_QuantityUQ = string.Empty;
			AssertHasMessageError(item.BX_QuantityUQInfo, "You have not entered Quantity UQ.");
			item.BX_QuantityUQ = ACROSSPackageTypes.Codes.PACKAGE;
			AssertNoMessageError(item.BX_QuantityUQInfo, "You have not entered Quantity UQ.");
		}

		public void TestCheckBX_Quantity()
		{
			item.BX_Quantity = 1m;
			item.BX_Quantity = 0m;
			AssertHasMessageErrorContaining(item.BX_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
			item.BX_Quantity = 1m;
			AssertNoMessageErrorContaining(item.BX_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBX_Description()
		{
			item.BX_Description = "DESCRIPTION";
			item.BX_Description = ZString.Empty;
			AssertHasMessageErrorContaining(item.BX_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			item.BX_Description = "DESCRIPTION";
			AssertNoMessageErrorContaining(item.BX_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		CusCAeMHItem item;
		protected override void SetUp()
		{
			base.SetUp();
			item = Factory.New<CusCAeMHMaster>().HouseBills.AddNew().Items.AddNew();
		}
	}
}
