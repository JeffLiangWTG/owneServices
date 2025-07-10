using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class CashAdvanceControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(AccCashAdvanceRequestHeader);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			header.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.CAH_Ledger = "AR";
			header.CAH_RX_NKTransactionCurrency = "AUD";

			var line = Factory.New<AccCashAdvanceRequestLine>();
			line.CAL_GC_Company = GlbCompany.CurrentCompany.PK;
			line.CAL_CAH_RequestHeader = header.PK;
			line.CAL_LocalAmount = 1000;
			header.Lines.Add(line);
			Factory.Save();

			return header;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
		}

		public override void TestViewForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("ViewForm should be null", Controller.ShowViewForm(sourceEntity));
		}

		public override void TestEditForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("EditForm should be null", Controller.ShowEditForm(sourceEntity));
		}

		public override void TestDeleteForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(sourceEntity));
		}
	}
}
