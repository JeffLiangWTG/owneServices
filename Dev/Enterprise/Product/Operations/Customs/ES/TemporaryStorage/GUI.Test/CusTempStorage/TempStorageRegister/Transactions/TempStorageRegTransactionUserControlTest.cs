using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

sealed class TempStorageRegTransactionUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceType()
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = "REF1";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		regHeader.SRH_SRP_Premises = premises.PK;
		var line = regHeader.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		using var form = new ZForm(line);
		using var control = new TempStorageRegTransactionUserControl();
		form.Controls.Add(control);
		form.Show();

		AssertType<CusTempStorageRegLine>("BindingSource is RegLine", control.BindingSource.DataSource);
	}

	public void TestTransactionsFilterControl() => CombineAssertions(() =>
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = "REF1";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		regHeader.SRH_SRP_Premises = premises.PK;

		var line1 = regHeader.CusTempStorageRegLines.AddNew();
		var line2 = regHeader.CusTempStorageRegLines.AddNew();
		line1.SRL_LineNumber = 1;
		using var form = new ZForm(line1);
		using var control = new TempStorageRegTransactionUserControl();
		form.Controls.Add(control);
		form.Show();

		var transactionsFilterControl = control.TransactionsFilterControl;
		AssertNotNull("TransactionsFilterControl is not null", transactionsFilterControl);
		AssertType<TempStorageRegTransactionFilterControl>(transactionsFilterControl);
		AssertSame("Collection updated", line1.CusTempStorageRegLineTransactionsForFilter, transactionsFilterControl.GridCollection);

		control.SetDataBinding(line2, string.Empty);
		AssertSame("Collection updated", line2.CusTempStorageRegLineTransactionsForFilter, transactionsFilterControl.GridCollection);
	});
}
