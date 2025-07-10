using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsWrapper))]
sealed class TSCustomsNumberViewStmNumsWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidation()
	{
		var wrapper = (TSCustomsNumberViewStmNumsWrapper)GetNewWrapper(Factory);
		AssertType<TSCustomsNumberViewStmNumsWrapperValidation>(wrapper.Validation);
	}

	public void TestPrefix_Caption()
	{
		CombineAssertions(() =>
		{
			var wrapper = (TSCustomsNumberViewStmNumsWrapper)GetNewWrapper(Factory);
			AssertResourceStringData(wrapper.NumberPrefixInfo, "Prefix", "Prefix", "Prefix", "Prefix for Range");
		});
	}

	public void TestNextNumber_Caption()
	{
		CombineAssertions(() =>
		{
			var wrapper = (TSCustomsNumberViewStmNumsWrapper)GetNewWrapper(Factory);
			AssertResourceStringData(wrapper.SN_ValueForDisplayInfo, "Next Number", "Next Number", "Next Num.", "Next Number for Range");
		});
	}

	public void TestPadding_Caption()
	{
		CombineAssertions(() =>
		{
			var wrapper = (TSCustomsNumberViewStmNumsWrapper)GetNewWrapper(Factory);
			AssertResourceStringData(wrapper.NumberPaddingInfo, "Padding", "Padding", "Padding", "Padding for Range");
		});
	}

	public void TestSuffix_Caption()
	{
		CombineAssertions(() =>
		{
			var wrapper = (TSCustomsNumberViewStmNumsWrapper)GetNewWrapper(Factory);
			AssertResourceStringData(wrapper.NumberSuffixInfo, "Suffix", "Suffix", "Suffix", "Suffix for Range");
		});
	}

	public void TestIsActive_Caption()
	{
		CombineAssertions(() =>
		{
			var wrapper = (TSCustomsNumberViewStmNumsWrapper)GetNewWrapper(Factory);
			AssertResourceStringData(wrapper.IsActiveInfo, "Is Active?", "Is Active?", "Is Active?", "Is this configuration active?");
		});
	}

	public void TestProperties()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Code = "C1";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = ((ICustomsNumberViewStmNumsParent)premises).CustomsNumberProvider;
			stmNum.SN_Owner = premises.PK;
			stmNum.SN_Type = "TS";
			stmNum.SN_FountainName = "PRE@5@SU@N";
			stmNum.HasChanges = false;
			var wrapper = new TSCustomsNumberViewStmNumsWrapper(stmNum);
			AssertEquals("When loaded HasChanges", expected: false, wrapper.HasChanges);
			AssertEquals("When loaded Prefix", "PRE", wrapper.NumberPrefix);
			AssertEquals("When loaded Padding", 5, wrapper.NumberPadding);
			AssertEquals("When loaded Suffix", "SU", wrapper.NumberSuffix);
			AssertEquals("When loaded IsActive by default and not in DB", expected: true, wrapper.IsActive);

			wrapper.NumberPrefix = "BB";
			wrapper.NumberPadding = 2;
			wrapper.NumberSuffix = "YY";
			wrapper.IsActive = false;

			AssertEquals("FountainName after set dependant properties", "BB@2@YY@N", wrapper.SN_FountainName);

			stmNum.SN_FountainName = "AAA@8@ZZ@N";
			AssertEquals("When SN_FountainName directly changed HasChanges", expected: true, wrapper.HasChanges);
			AssertEquals("When SN_FountainName directly changed Prefix", "AAA", wrapper.NumberPrefix);
			AssertEquals("When SN_FountainName directly changed Padding", 8, wrapper.NumberPadding);
			AssertEquals("When SN_FountainName directly changed Suffix", "ZZ", wrapper.NumberSuffix);
			AssertEquals("When SN_FountainName directly changed IsActive", expected: false, wrapper.IsActive);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(ZArchitecture.Schema.ViewStmNumsSchema.SN_Owner, stmNum.SN_Owner);
			_ = query.AddToFilter(ZArchitecture.Schema.ViewStmNumsSchema.SN_Name, stmNum.SN_Name);
			stmNum = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNum.Provider = ((ICustomsNumberViewStmNumsParent)premises).CustomsNumberProvider;
			wrapper = new TSCustomsNumberViewStmNumsWrapper(stmNum);
			AssertEquals("When reloaded HasChanges", expected: false, wrapper.HasChanges);
			AssertEquals("When reloaded Prefix", "AAA", wrapper.NumberPrefix);
			AssertEquals("When reloaded Padding", 8, wrapper.NumberPadding);
			AssertEquals("When reloaded Suffix", "ZZ", wrapper.NumberSuffix);
			AssertEquals("When reloaded IsActive", expected: false, wrapper.IsActive);
		});
	}

	public void TestProperties_MaxValues()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Code = "123456789012345";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = ((ICustomsNumberViewStmNumsParent)premises).CustomsNumberProvider;
			stmNum.SN_Owner = premises.PK;
			stmNum.SN_Type = "TS";
			stmNum.HasChanges = false;
			var wrapper = new TSCustomsNumberViewStmNumsWrapper(stmNum)
			{
				NumberPrefix = "ASDFGHJKKL", NumberPadding = 10, NumberSuffix = "ASDFGHJKKL", IsActive = true
			};

			AssertEquals("MaxLength formatted FountainName", "ASDFGHJKKL@10@ASDFGHJKKL@Y", wrapper.SN_FountainName);
		});
	}

	public void TestDefaultValues()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Code = "123456789012345";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			_ = premises.NumberProvider.CustomsNumbers.AddNew();
			var wrapper = premises.NumberProvider.CustomsNumberWrappers[0];

			AssertEquals("Default IsActive is there is no other active", expected: true, wrapper.IsActive);
			AssertEquals("Default Type", "TS", wrapper.SN_Type);
			AssertEquals("Default Count", 1, wrapper.SN_Count.ToZInt());
			wrapper.NumberPrefix = "P";
			wrapper.NumberPadding = 1;
			wrapper.NumberSuffix = "S";

			Factory.Save();
			_ = premises.NumberProvider.CustomsNumbers.AddNew();
			var wrapper2 = premises.NumberProvider.CustomsNumberWrappers[1];

			AssertEquals("Default IsActive false when there is other active", expected: false, wrapper2.IsActive);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
		=> GetNewWrapper(Factory);

	static CustomsNumberViewStmNumsWrapper GetNewWrapper(BusinessObjectFactory factory)
	{
		var stmNumsProvider = factory.New<CusTempStorageRegPremises>().NumberProvider;
		var stmNums = stmNumsProvider.CustomsNumbers.AddNew();
		return stmNumsProvider.GetOrCreateWrapper(stmNums);
	}

	static void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}
}
