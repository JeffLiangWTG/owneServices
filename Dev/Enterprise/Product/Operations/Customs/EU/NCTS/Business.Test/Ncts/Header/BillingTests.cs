using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsInvoicingSupporter))]
	sealed class BillingTests : MasterFiles.Business.Testing.JobInvoicingSupporterTest
	{
		public void TestGetChargePosterBehaviours()
		{
			AssertEquals("NCTS does not support any charge behaviour - change this as you will!", ChargePosterBehaviours.None, header.GetChargePosterBehaviours());
		}

		public void TestSetJobNumberFieldOnSaving()
		{
			AssertEquals("", header.BH_JobReference);
			((IJobHeaderParent)header).SetJobNumberFieldOnSaving();
			AssertEquals("NCT00000001", header.BH_JobReference);
			Factory.Save();
			AssertEquals("NCT00000001", header.BH_JobReference);
		}

		public void TestIntegrateWithAccountingIfRequired()
		{
			var integrationResult = header.IntegrateWithAccountingIfRequired(useNewFactory: false);
			AssertEquals(false, integrationResult.WasSuccessful);
			Factory.Save();
			integrationResult = header.IntegrateWithAccountingIfRequired(useNewFactory: false);
			AssertEquals(false, integrationResult.WasSuccessful);
			foreach (var stat in new ZString[] { NctsTransitStatusList.Codes.DeclarationAccepted, NctsTransitStatusList.Codes.DeclarationMrnAllocated, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture })
			{
				header.MovementHeader.BM_CustomsStatus = stat;
				integrationResult = header.IntegrateWithAccountingIfRequired(useNewFactory: false);
				AssertEquals("Accounting integration works for status " + stat, true, integrationResult.WasSuccessful);
			}
		}

		public void TestInvoicingSupporter()
		{
			AssertType<NctsInvoicingSupporter>(header.InvoicingSupporter);
		}

		public void TestEuNctsAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.EuNctsAudit, header.InvoicingSupporter.AuditSecurity);
			try
			{
				Env.Security.EuNctsAudit.IsAllowed = true;
				AssertEquals("Security, Audit", true, header.InvoicingSupporter.AuditSecurity.IsAllowed);
				Env.Security.EuNctsAudit.IsAllowed = false;
				AssertEquals("Without security, Audit", false, header.InvoicingSupporter.AuditSecurity.IsAllowed);
			}
			finally
			{
				Env.Security.EuNctsAudit.ClearOverriddenSecurityValue();
			}
		}

		public void TestEuNctsJobInvoicingSecurity()
		{
			AssertEquals("JobInvoicingSecurity", Env.Security.EuNctsJobInvoicing, header.InvoicingSupporter.JobInvoicingSecurity);
			try
			{
				Env.Security.EuNctsJobInvoicing.IsAllowed = true;
				AssertEquals("Security, JobInvoicing", true, header.InvoicingSupporter.JobInvoicingSecurity.IsAllowed);
				Env.Security.EuNctsJobInvoicing.IsAllowed = false;
				AssertEquals("Without security, JobInvoicing", false, header.InvoicingSupporter.JobInvoicingSecurity.IsAllowed);
			}
			finally
			{
				Env.Security.EuNctsJobInvoicing.ClearOverriddenSecurityValue();
			}
		}

		public void TestIControllerIDProvider()
		{
			var provider = (IControllerIDProvider)header;
			AssertEquals(ControllerIDs.Customs.EU.NctsMovementController, provider.ControllerID);
			AssertEquals(header.PK, provider.BusinessObjectPK);

			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals(ControllerIDs.Customs.EU.NctsMovementInShipmentController, provider.ControllerID);
			AssertEquals(header.PK, provider.BusinessObjectPK);

			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(ControllerIDs.Customs.EU.NctsMovementInConsolController, provider.ControllerID);
			AssertEquals(header.PK, provider.BusinessObjectPK);
		}

		public void TestWillIntegrateWithBillingOnlyCalledOnOnSuccessfulSave()
		{
			var header = Factory.New<NctsHeaderWithFakeInvoicingSupportForTest>();
			header.ShouldThrowOnSaving = false;
			Factory.Save();
			Assert("Should expect to integrate with billing upon successful save", header.WillIntegrateWithAccounting);

			var badHeader = Factory.New<NctsHeaderWithFakeInvoicingSupportForTest>();
			badHeader.ShouldThrowOnSaving = true;
			try
			{
				Factory.Save();
			}
			catch
			{
				Assert("Should NOT expect to integrate with billing upon failed save", !badHeader.WillIntegrateWithAccounting);
			}
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;

		class NctsHeaderWithFakeInvoicingSupportForTest : NctsHeader
		{
			public NctsHeaderWithFakeInvoicingSupportForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool WillIntegrateWithAccounting;
			public bool ShouldThrowOnSaving;

			public override void OnSaving()
			{
				if (ShouldThrowOnSaving)
				{
					throw new Exception("Cannot save fake for testing");
				}
				base.OnSaving();
			}
			protected override IAutoBillingResult IntegrateWithAccountingIfRequiredCore(bool useNewFactory)
			{
				WillIntegrateWithAccounting = true;
				return null;
			}
		}
	}
}
