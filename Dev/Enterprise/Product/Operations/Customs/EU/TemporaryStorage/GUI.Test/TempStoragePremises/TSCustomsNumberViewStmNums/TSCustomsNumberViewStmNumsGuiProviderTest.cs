using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TSCustomsNumberViewStmNumsGuiProvider))]
	public sealed class TSCustomsNumberViewStmNumsGuiProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetEditorForm()
		{
			var provider = Premises.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper = provider.CustomsNumberWrappers[0];
			using var form = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetEditorForm(wrapper);
			AssertEquals(typeof(TSCustomsNumberViewStmNumsEditorForm), form.GetType());
		}

		public void TestGetUserControl()
		{
			var provider = Premises.NumberProvider;
			using var userControl = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetUserControl();
			AssertEquals(typeof(TSCustomsNumberViewStmNumsUserControl), userControl.GetType());
		}

		CusTempStorageRegPremises Premises => premises ??= Factory.New<CusTempStorageRegPremises>();
		CusTempStorageRegPremises premises;

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
			=> new TSCustomsNumberViewStmNumsGuiProvider(Factory, Core.Constants.CountryCodes.Latvia, Premises.PK);
	}
}
