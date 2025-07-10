using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ViewMatchGroup))]
	public class ViewMatchGroupTestCase : EnterpriseBusinessObjectTestCase
	{
		// Not relevant for BizOs generated from views
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		public void TestViewMatchGroupDBView()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			ARInvoice aRINV1 = Factory.New<ARInvoice>();
			aRINV1.AH_OH = testOrg.PK;
			aRINV1.AH_IsCancelled = false;
			aRINV1.AH_InvoiceDate = new ZDateTime(2004, 4, 4);

			ARInvoice aRINV2 = Factory.New<ARInvoice>();
			aRINV2.AH_OH = testOrg.PK;
			aRINV2.AH_IsCancelled = true;
			((IMatching)aRINV2).CurrentMatchGroup.AddNew().AP_AH = aRINV2.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(aRINV2);
			aRINV2.AH_InvoiceDate = new ZDateTime(2004, 3, 3);

			TransactionMatchLink matchlink1 = ((IMatching)aRINV1).CurrentMatchGroup.AddNew();
			matchlink1.AP_AH = aRINV1.PK;
			matchlink1.AP_MatchGroupNum = "M00009999";
			matchlink1.AP_MatchDate = new ZDateTime(2004, 5, 5);

			TransactionMatchLink matchlink2 = ((IMatching)aRINV2).CurrentMatchGroup.AddNew();
			matchlink2.AP_AH = aRINV2.PK;
			matchlink2.AP_MatchGroupNum = "M00009888";
			matchlink2.AP_MatchDate = new ZDateTime(2004, 6, 6);

			aRINV1.AH_TransactionNum = "00009999";
			aRINV1.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRINV2.AH_TransactionNum = "00009888";
			aRINV2.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			ViewMatchGroupCollection matchGroups = new ViewMatchGroupCollection(Factory, new ZQuery());

			DbCommand command = Db.Connection.Command("SELECT * FROM dbo.ViewMatchGroup");

			ZString fieldNamesInTheView = ZString.Empty;

			try
			{
				using (var reader = command.ExecuteReader())
				{
					// use GetName like in Factory load
					for (int i = 0; i < reader.FieldCount; i++)
					{
						string columnName = reader.GetName(i);
						fieldNamesInTheView += columnName + System.Environment.NewLine;
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception("Exception thrown in the reader: " + System.Environment.NewLine + e.Message);
			}

			try
			{
				matchGroups.Load();
			}
			catch (Exception ex)
			{
				throw new Exception(fieldNamesInTheView, ex);
			}

			AssertEquals("Should be 1 ViewMatchGroup row in the view", 1, matchGroups.Count);
			ViewMatchGroup testViewMatchGroup = matchGroups[0];
			AssertEquals($"{AutoViewMatchGroup.Schema.MG_OH} should be TestOrg", testOrg.PK, testViewMatchGroup.MG_OH);
			AssertEquals($"{AutoViewMatchGroup.Schema.MG_TransactionNum} should be 00009999", "00009999", testViewMatchGroup.MG_TransactionNum);
			AssertEquals($"{AutoViewMatchGroup.Schema.MG_InvoiceDate} should be 4/4/2004", new ZDateTime(2004, 4, 4), testViewMatchGroup.MG_InvoiceDate);
		}
	}
}
