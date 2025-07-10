using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ITCustomsNumberViewStmNumsCompanyProvider))]
sealed class ITCustomsNumberViewStmNumsCompanyProviderTest : NonPersistentBusinessObjectTestCase
{
	public void TestGetProvider()
	{
		var provider = (CustomsNumberViewStmNumsCompanyProvider)CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Company.GC_RN_NKCountryCode, Company.PK);
		AssertEquals(typeof(ITCustomsNumberViewStmNumsCompanyProvider), provider.GetType());
		AssertEquals(true, object.ReferenceEquals(provider, Company.CustomsNumberProvider));
		AssertEquals(Core.Constants.CountryCodes.Italy, provider.CountryCode);
	}

	public void TestGetSetting()
	{
		var provider = Company.CustomsNumberProvider;
		var setting = provider.GetSetting(NumberRangeTypeList.Codes.EntrySummaryDeclaration);
		AssertEquals(true, object.ReferenceEquals(setting, provider.GetSetting(NumberRangeTypeList.Codes.EntrySummaryDeclaration)));
		AssertEquals(typeof(ITCustomsNumberViewStmNumsSetting), setting.GetType());
		AssertEquals(NumberRangeTypeList.Codes.EntrySummaryDeclaration, setting.RangeType);
	}

	public void TestGetEditorForm()
	{
		var provider = Company.CustomsNumberProvider;
		var stmNum = provider.CustomsNumbers.AddNew();
		var wrapper = provider.CustomsNumberWrappers[0];
		using (var form = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetEditorForm(wrapper))
		{
			AssertEquals(typeof(ITCustomsNumberViewStmNumsEditorForm), form.GetType());
		}
	}

	public void TestCustomsNumberWrappers()
	{
		var provider = Company.CustomsNumberProvider;
		AssertEquals(typeof(ITCustomsNumberViewStmNumsWrapperCollection), provider.CustomsNumberWrappers.GetType());
		AssertEquals(0, provider.CustomsNumberWrappers.Count);
		var stmNum = provider.CustomsNumbers.AddNew();
		AssertEquals(1, provider.CustomsNumberWrappers.Count);
		var wrapper = provider.CustomsNumberWrappers[0];
		AssertEquals(typeof(ITCustomsNumberViewStmNumsWrapper), wrapper.GetType());
	}

	public void TestGetUserControl()
	{
		var provider = Company.CustomsNumberProvider;
		using (var userControl = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetUserControl())
		{
			AssertEquals(typeof(ITCustomsNumberViewStmNumsUserControl), userControl.GetType());
		}
	}

	public void TestGetOrCreateWrapper()
	{
		var provider = Company.CustomsNumberProvider;
		AssertNull(provider.GetOrCreateWrapper(null));
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		var wrapper = provider.GetOrCreateWrapper(stmNum);
		AssertEquals(typeof(ITCustomsNumberViewStmNumsWrapper), wrapper.GetType());
		AssertEquals(true, object.ReferenceEquals(wrapper, provider.GetOrCreateWrapper(stmNum)));
	}

	public void TestGetNewLookups()
	{
		var provider = Company.CustomsNumberProvider;
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		var lookups = stmNum.Lookups;
		AssertEquals(typeof(CustomsNumberViewStmNumsLookups), lookups.GetType());
		stmNum.Provider = provider;
		lookups = stmNum.Lookups;
		AssertEquals(typeof(ITCustomsNumberViewStmNumsLookups), lookups.GetType());
	}

	[UseSnapshotProtection]
	public void TestGetNumberRangeDetail()
	{
		var provider = Company.CustomsNumberProvider;
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		stmNum.Provider = provider;
		stmNum.SN_Owner = Company.PK;
		stmNum.SN_Type = NumberRangeTypeList.Codes.EntrySummaryDeclaration;
		stmNum.SN_FountainName = "2017:12312122";
		stmNum.SN_MinimumValue = 100000;
		stmNum.SN_Count = 1000;
		Factory.Save();
		var range = stmNum.GetNumberRanges().First();
		AssertEquals("Range Type: ENS, Year: 2017, Applies To: 12312122", range.Detail);
	}

	GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
	GlbCompany company;

	protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
	{
		return new ITCustomsNumberViewStmNumsCompanyProvider(Factory, Company.PK);
	}
}
