using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;
using NumberRangeTypeList = Enterprise.Customs.EU.Business.NumberRangeTypeList;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TSCustomsNumberViewStmNumsEditorForm))]
	public sealed class TSCustomsNumberViewStmNumsEditorFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFountainNameTextBoxIsNotVisible()
		{
			using var form = (TSCustomsNumberViewStmNumsEditorForm)GetFormToBash();
			form.Show();
			var fountainNameTextBox = (ZTextBox)form.Controls.Find("FountainNameTextBox", searchAllChildren: true).Single();
			AssertEquals(expected: false, fountainNameTextBox.Visible);
		}

		public void TestPrefixTextBox()
		{
			using var form = (TSCustomsNumberViewStmNumsEditorForm)GetFormToBash();
			var prefixTextBox = form.PrefixTextBox;
			AssertNotNull("PrefixTextBox", prefixTextBox);
			AssertEquals("PrefixTextBox BindTo", "NumberPrefix", prefixTextBox.BindTo);
		}

		public void TestPaddingCalcEdit()
		{
			CombineAssertions(() =>
			{
				using var form = (TSCustomsNumberViewStmNumsEditorForm)GetFormToBash();
				var paddingCalcEdit = form.PaddingCalcEdit;
				AssertNotNull("PaddingCalcEdit", paddingCalcEdit);
				AssertEquals("PaddingCalcEdit BindTo", "NumberPadding", paddingCalcEdit.BindTo);
				AssertEquals("DecimalPlaces", 0, paddingCalcEdit.DecimalPlaces);
				AssertEquals("MaxValue", 10m, paddingCalcEdit.MaxValue);
				AssertEquals("AllowNegative", expected: false, paddingCalcEdit.AllowNegative);
			});
		}

		public void TestSuffixTextBox()
		{
			using var form = (TSCustomsNumberViewStmNumsEditorForm)GetFormToBash();
			var suffixTextBox = form.SuffixTextBox;
			AssertNotNull("SuffixTextBox", suffixTextBox);
			AssertEquals("SuffixTextBox BindTo", "NumberSuffix", suffixTextBox.BindTo);
		}

		public void TestIsActiveCheckBox()
		{
			using var form = (TSCustomsNumberViewStmNumsEditorForm)GetFormToBash();
			var isActiveCheckBox = form.IsActiveCheckBox;
			AssertNotNull("IsActiveCheckBox", isActiveCheckBox);
			AssertEquals("IsActiveCheckBox BindTo", "IsActive", isActiveCheckBox.BindTo);
		}

		protected override Form GetFormToBashCore()
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var orgHeaderPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			premises.AuthorizationOwner = orgHeaderPK;
			premises.AuthorizationNumber = "OH";

			var stmNumsProvider = premises.NumberProvider;
			var stmNums = stmNumsProvider.CustomsNumbers.AddNew();
			var wrapper = stmNumsProvider.GetOrCreateWrapper(stmNums);
			wrapper.SN_Type = NumberRangeTypeList.Codes.TemporaryStorageType;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var premisesLoaded = newFactory.Load<CusTempStorageRegPremises>(premises.PK);
			var providerLoaded = premisesLoaded.NumberProvider;
			var wrapperLoaded = (TSCustomsNumberViewStmNumsWrapper)providerLoaded.CustomsNumberWrappers.FirstOrDefault();
			var guiProvider = new TSCustomsNumberViewStmNumsGuiProvider(newFactory, Core.Constants.CountryCodes.Latvia, premisesLoaded.PK);
			return guiProvider.GetEditorForm(wrapperLoaded);
		}
	}
}
