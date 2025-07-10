using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARPaymentProcessingModule))]
	public class ARPaymentProcessingModuleTest : PaymentProcessingModuleTest
	{
		protected override string AR_AP => "AR";

		protected override SecurityCheckpoint FirstApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingFirstApproval; }
		}

		protected override SecurityCheckpoint SecondApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingSecondApproval; }
		}

		protected override SecurityCheckpoint ThirdApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingThirdApproval; }
		}

		protected override SecurityCheckpoint CancelApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingCancelApproval; }
		}

		protected override SecurityCheckpoint PostCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingPost; }
		}

		protected override SecurityCheckpoint NewCashPaymentCheckPoint
		{
			get { return Env.Security.NewReceivablesPaymentCash; }
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ARPaymentProcessing;
		}

		protected override PaymentApprovalWithAuthorisation GetNewPaymentApproval()
		{
			return GetNewPaymentApproval(Factory);
		}

		protected override PaymentApprovalWithAuthorisation GetNewPaymentApproval(BusinessObjectFactory factory)
		{
			ARPaymentApprovalWithAuthorisation approval = factory.New<ARPaymentApprovalWithAuthorisation>();
			approval.AV_OH = TestOrgHeader.PK;
			approval.AV_Amount = 10000m;
			return approval;
		}

		protected override PaymentProcessingModule GetNewModule()
		{
			return new ARPaymentProcessingModuleForTest();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			ARPaymentApprovalWithAuthorisation approval1 = Factory.New<ARPaymentApprovalWithAuthorisation>();
			approval1.AV_OH = TestOrgHeader.PK;
			collection.Add(approval1);

			ARPaymentApprovalWithAuthorisation approval2 = Factory.New<ARPaymentApprovalWithAuthorisation>();
			approval2.AV_OH = TestOrgHeader.PK;
			collection.Add(approval2);

			ARPaymentApprovalWithAuthorisation approval3 = Factory.New<ARPaymentApprovalWithAuthorisation>();
			approval3.AV_OH = TestOrgHeader.PK;
			collection.Add(approval3);

			ARPaymentApprovalWithAuthorisation approval4 = Factory.New<ARPaymentApprovalWithAuthorisation>();
			approval4.AV_OH = TestOrgHeader.PK;
			collection.Add(approval4);
		}

		protected override void SelectBusinessObjects(PaymentProcessingModule module, BusinessObject[] businessObjects)
		{
			ARPaymentProcessingModuleForTest moduleAsAR = module as ARPaymentProcessingModuleForTest;
			moduleAsAR.SetfSelectedBusinessObjects(businessObjects);
		}

		protected class ARPaymentProcessingModuleForTest : ARPaymentProcessingModule
		{
			protected override BusinessObject[] SelectedBusinessObjects
			{
				get { return fSelectedBusinessObjects ?? base.SelectedBusinessObjects; }
			}

			BusinessObject[] fSelectedBusinessObjects;

			public void SetfSelectedBusinessObjects(BusinessObject[] newfSelectedBusinessObjects)
			{
				fSelectedBusinessObjects = newfSelectedBusinessObjects;
			}
		}
	}
}
