using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestsSubclassesOf(typeof(CAMessageManager))]
	public abstract class CAMessageManagerTestCase : EDIFACTMessageManagerTestCase
	{
		public void TestResetDeclaration()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = false;
			var manager = GetMessageManager() as CAMessageManager;
			AssertResetDeclaration(manager, () => manager.ResetDeclaration(), false);

			Env.Security.CustomsResetToOriginal.IsAllowed = true;
			manager = GetMessageManager() as CAMessageManager;
			AssertResetDeclaration(manager, () => manager.ResetDeclaration(), true);
		}

		protected abstract void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed);

		public void TestCanSendThisMessageWithIsCreditCheckOKToSend()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var company = GlbCompany.GetCurrentCompany(Factory);
				var companyProxy = company.OrgProxy;
				companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "123456");

				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				supplier.CompanyData.OB_IsDebtor = true;

				supplier.MiscServ.OM_ARGlobalCreditApproved = true;
				supplier.MiscServ.OM_ARGlobalOnCreditHold = true;

				var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				jobDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				jobDeclaration.JE_OH_Supplier = supplier.PK;

				var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				cusEntryHeader.FillWithValidTestData();
				cusEntryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

				Factory.Save();

				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertCanSendThisMessage(cusEntryHeader, ZString.Empty);
				}

				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertCanSendThisMessage(cusEntryHeader, "Submit message with credit restriction canceled.");
				}

				var lvsDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				lvsDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				lvsDeclaration.JE_OH_Importer = supplier.PK;
				lvsDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				var invoiceHeader = lvsDeclaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 4, 10);
				var entryHeader = lvsDeclaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

				var line = entryHeader.MergedLines.AddNew();
				line.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
				line.CL_CustomsValue = 2499m;
				Factory.Save();

				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertCanSendThisMessage(entryHeader, "Submit message with credit restriction canceled.");
				}

				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertCanSendThisMessage(cusEntryHeader, ZString.Empty);
				}
			}
		}

		protected abstract void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage);

		public override void SetTestMode(bool testMode)
		{
			LicenceTypeChanger.SetSystemLicence(testMode ? DatabaseTypes.Codes.Test : DatabaseTypes.Codes.Production);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
		}
	}
}
