using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRMessageManagerBaseOnlyTest : TestCaseWithFactory
	{
		public void TestNotificationsAreNotDisplayedForABN()
		{
			var hawb = Factory.New<CusHAWB>();
			var testManager = new CMRMessageManagerForTest(hawb);

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111111111111");
			AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "");
			AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));
		}

		public void TestNotificationsAreDisplayedForABNOnOverride()
		{
			var hawb = Factory.New<CusHAWB>();
			var testManager = new CMRMessageManagerForTest(hawb);

			var currentCompanyProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			try
			{
				testManager.ShouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue = false;
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111111111111");
				AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "");
				AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));

				testManager.ShouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue = true;
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11 111 111 111");
				AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111111111111");
				AssertEquals(true, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
				AssertEquals(true, testManager.GetCommonNotificationsForSending().ContainsWarning("The Organisation Proxy of current company (" + GlbCompany.CurrentCompany.GC_Name + ") has not been entered. Please enter it in Company form."));

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = currentCompanyProxy;
				AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The Organisation Proxy of current company (" + GlbCompany.CurrentCompany.GC_Name + ") has not been entered. Please enter it in Company form."));
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "");
				AssertEquals(true, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				AssertEquals(false, testManager.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));
				AssertEquals(true, testManager.GetCommonNotificationsForSending().ContainsWarning("The Organisation Proxy of current company (" + GlbCompany.CurrentCompany.GC_Name + ") has not been entered. Please enter it in Company form."));
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = currentCompanyProxy;
			}
		}

		public void TestShouldSendMessagesInTestMode()
		{
			var hawb = Factory.New<CusHAWB>();
			var manager = new CMRMessageManagerForTest(hawb);
			Env.Registry.CMRTestMode = false;
			Assert(!manager.ShouldSendMessagesInTestMode);
			Env.Registry.CMRTestMode = true;
			Assert(manager.ShouldSendMessagesInTestMode);
		}

		public void TestNotificationsForCertificatesFactoryHits()
		{
			var certificatesHelper = ObjectFactory.New<Integration.Customs.AU.ICertificateManagerHelper>(Factory);
			certificatesHelper.CreateCustomsCertificates();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var mawb = otherFactory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();

			var manager1 = new CMRMessageManagerForTest(hawb1);
			_ = manager1.GetCommonNotificationsForSending();
			var initialHitcount = otherFactory.GetTableHitCount(RefSysConfigSchema.Constants.TableName);
			AssertGreaterThan(initialHitcount, 0);

			var manager2 = new CMRMessageManagerForTest(hawb2);
			_ = manager2.GetCommonNotificationsForSending();
			var updatedHitcount = otherFactory.GetTableHitCount(RefSysConfigSchema.Constants.TableName);

			AssertEquals(initialHitcount, updatedHitcount);
		}

		sealed class CMRMessageManagerForTest : CMRMessageManager
		{
			public CMRMessageManagerForTest(CusHAWBBase hawb)
			{
				this.hawb = hawb;
			}

			internal new MessageSendingNotificationCollection GetCommonNotificationsForSending() => base.GetCommonNotificationsForSending();

			internal new bool ShouldSendMessagesInTestMode => base.ShouldSendMessagesInTestMode;

			bool shouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue;
			internal bool ShouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue
			{
				get => shouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue;
				set => shouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue = value;
			}

			protected override bool ShouldValidateCurrentCompanyLocalBusNumCharacters => ShouldValidateCurrentCompanyLocalBusNumCharactersReturnTrue;

			internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusHAWBBase).Messages;

			internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { hawb.Calculator };

			internal override string GetStatus() => hawb.CS_CustomsStatus;

			internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CTOCusHAWBAIRCRAmendmentGenerator(bizo as CTOCusHAWB);

			internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new AIRCRMessageBuilder(bizo as CTOCusHAWB) };

			public override string MessageFriendlyName => "Test";

			public override BusinessObject BusinessObject => hawb;

			readonly CusHAWBBase hawb;
		}
	}
}
