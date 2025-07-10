using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RequireReasonForCLRItem))]
	sealed class RequireReasonForCLRItemTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new RequireReasonForCLRItem
		{
			Code = "123",
			Title = "12345",
			ClearingReason = "Dummy Description"
		};

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		public void TestIsMandatoryReadOnlyAndAutoSet()
		{
			var item = new RequireReasonForCLRItem();

			item.Code = "123";
			item.Title = "12345";
			item.IsMandatory = true;

			item.ClearingReason = "";
			AssertEquals(true, item.IsMandatory);
			AssertEquals(false, item.IsMandatoryInfo.ReadOnly);

			item.ClearingReason = "Dummy Description";
			AssertEquals(false, item.IsMandatory);
			AssertEquals(true, item.IsMandatoryInfo.ReadOnly);
		}

		public void TestValidateCode()
		{
			var itemCollection = new RequireReasonForCLRItemCollection();
			var item1 = new RequireReasonForCLRItem
			{
				Code = "ABC",
				Title = "T1"
			};

			var item2 = new RequireReasonForCLRItem
			{
				Code = "WJG",
				Title = "T2"
			};

			var item3 = new RequireReasonForCLRItem
			{
				Code = " ",
				Title = "T3"
			};

			var item4 = new RequireReasonForCLRItem
			{
				Code = "EFG",
				Title = "T4"
			};

			var item5 = new RequireReasonForCLRItem
			{
				Code = "  C",
				Title = "T5"
			};

			var item6 = new RequireReasonForCLRItem
			{
				Code = "OTH",
				Title = "T6"
			};

			var item7 = new RequireReasonForCLRItem
			{
				Code = "E3A",
				Title = "T7"
			};

			itemCollection.Add(item1);
			itemCollection.Add(item2);
			itemCollection.Add(item3);
			itemCollection.Add(item4);
			itemCollection.Add(item5);
			itemCollection.Add(item6);
			itemCollection.Add(item7);

			item2.Code = "EFG";
			item4.ValidateCode();

			AssertNoErrors(item1.CodeInfo);
			AssertHasErrors("There are duplicate Codes: EFG.", item2.CodeInfo);
			AssertHasErrors("Code should not be empty.", item3.CodeInfo);
			AssertHasErrors("There are duplicate Codes: EFG.", item4.CodeInfo);
			AssertHasErrors("Code length should be 3.", item5.CodeInfo);
			AssertHasErrors("'OTH' is the default Code for the use of inputting a reason not available in the registry.", item6.CodeInfo);
			AssertHasErrors("Code needs to be all capital letters.", item7.CodeInfo);
		}

		public void TestValidateTitle()
		{
			var item = new RequireReasonForCLRItem();
			item.Title = "123";
			AssertNoErrors("Precondition: TitleInfo should not have errors.", item.TitleInfo);

			item.Title = "";
			AssertHasErrors("Title should not be empty.", item.TitleInfo);
		}

		public void TestValidateIsMandatory()
		{
			var item = new RequireReasonForCLRItem();
			item.ClearingReason = "";
			item.IsMandatory = true;
			AssertNoErrors("Precondition: IsMandatoryInfo should not have errors.", item.IsMandatoryInfo);

			item.ClearingReason = "123 456";
			item.IsMandatory = true;
			AssertHasErrors("When Description is filled in, Mandatory should be unchecked.", item.IsMandatoryInfo);
		}

		public void TestValidateDescription()
		{
			var item = new RequireReasonForCLRItem();
			item.ClearingReason = "";
			AssertNoErrors("ClearingReasonInfo should not have errors if Clearing Reason is blank.", item.ClearingReasonInfo);

			item.ClearingReason = "12 34 5";
			AssertHasErrors("Clearing reason must be minimum six characters long.", item.ClearingReasonInfo);

			item.ClearingReason = "12345678";
			AssertHasErrors("Clearing reason must be minimum two words long.", item.ClearingReasonInfo);

			item.ClearingReason = "        12  ";
			AssertHasErrors("Clearing reason must be minimum two words and six characters long.", item.ClearingReasonInfo);

			item.ClearingReason = "123 456";
			AssertNoErrors("ClearingReasonInfo should not have errors if Clearing Reason is at least two words and six characters long.", item.ClearingReasonInfo);
		}
	}
}
