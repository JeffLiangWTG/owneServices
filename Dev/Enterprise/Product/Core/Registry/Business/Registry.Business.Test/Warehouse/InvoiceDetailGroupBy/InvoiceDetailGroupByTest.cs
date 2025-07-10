using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceDetailGroupBy))]
	class InvoiceDetailGroupByTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestSetCustomDefaultValuesCore()
		{
			InvoiceDetailGroupBy item = new InvoiceDetailGroupBy();
			AssertEquals("JTY", item.Group1);
			AssertEquals("", item.Group2);
			AssertEquals("", item.Group3);
		}

		public void TestValidateGroup1()
		{
			AssertNoErrors("Precondition: Group1 should not have errors.", BizObj.Group1Info);

			BizObj.Group1 = "!@#";
			AssertHasError(BizObj.Group1Info, "Enter a valid selection.");

			BizObj.Group1 = BizObj.GroupByList[0].Code;
			AssertNoErrors(BizObj.Group1Info);

			BizObj.Group1 = "";
			AssertNoErrors(BizObj.Group1Info);
		}

		public void TestValidateGroup2()
		{
			AssertNoErrors("Precondition: Group1 should not have errors.", BizObj.Group2Info);

			BizObj.Group2 = "!@#";
			AssertHasError(BizObj.Group2Info, "Enter a valid selection.");

			BizObj.Group2 = BizObj.GroupByList[0].Code;
			AssertNoErrors(BizObj.Group2Info);

			BizObj.Group2 = "";
			AssertNoErrors(BizObj.Group2Info);
		}

		public void TestValidateGroup3()
		{
			AssertNoErrors("Precondition: Group3 should not have errors.", BizObj.Group3Info);

			BizObj.Group3 = "!@#";
			AssertHasError(BizObj.Group3Info, "Enter a valid selection.");

			BizObj.Group3 = BizObj.GroupByList[0].Code;
			AssertNoErrors(BizObj.Group3Info);

			BizObj.Group3 = "";
			AssertNoErrors(BizObj.Group3Info);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Group1 = "!@#";
			BizObj.Group2 = "!@#";
			BizObj.Group3 = "!@#";

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.Group1Info);
			AssertHasErrors(BizObj.Group2Info);
			AssertHasErrors(BizObj.Group3Info);
		}

		public void TestGroupByList()
		{
			AssertEquals("InvoiceGroupingSettingList.Count", 10, BizObj.GroupByList.Count);
			AssertEquals("Charge Code", BizObj.GroupByList.GetDescriptionFromCode("CCO"));
			AssertEquals("Product Code", BizObj.GroupByList.GetDescriptionFromCode("PRD"));
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new InvoiceDetailGroupBy();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new InvoiceDetailGroupBy BizObj
		{
			get { return (InvoiceDetailGroupBy)base.BizObj; }
		}

		#endregion
	}
}
