using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.HK.Business.Testing
{
	[TestedType(typeof(TraxonMessage))]
	class TraxonMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Application Code", EDIMessage.ApplicationCodes.Traxon, message.EM_ApplicationCode);
				AssertEquals("TestMessage", false, message.EM_IsTestMessage);
			});
		}

		public void TestMessageNumberAllocation()
		{
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG82"))
			{
				CombineAssertions(() =>
				{
					AssertEquals("PreCondition Empty Message Number", ZString.Empty, message.EM_MessageNum);
					message.EM_MessageText = ExampleTraxonMessageText;
					Factory.Save();
					AssertNotNullOrEmpty("First Message: Message Number not Empty", message.EM_MessageNum);
					var message2 = Factory.New<TraxonMessage>();
					message2.EM_MessageText = ExampleTraxonMessageText;
					Factory.Save();
					int messageNumber1 = int.Parse(message.EM_MessageNum);
					int messageNumber2 = int.Parse(message2.EM_MessageNum);
					AssertGreaterThan("Message number comparison", messageNumber2, messageNumber1);
				});
			}
		}

		public void TestMessageNumberAllocationWithDifferentBranch()
		{
			var message = Factory.New<TraxonMessage>();
			AssertEquals("PreCondition message branch", GlbBranch.CurrentBranch.PK, message.EM_GB);
			Assert("PreCondition Empty Message Number", message.EM_MessageNum.IsEmpty);
			message.EM_MessageText = ExampleTraxonMessageText;
			Factory.Save();

			Assert("Message Number not Empty", !message.EM_MessageNum.IsEmpty);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "HK";
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = "HKHKG";
			Factory.Save();

			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG82"))
			{
				using (branch.SetAsTemporaryContext())
				{
					message = Factory.New<TraxonMessage>();
					AssertEquals("PreCondition message branch not current branch", branch.PK, message.EM_GB);
					Assert("PreCondition Empty Message Number", message.EM_MessageNum.IsEmpty);
					message.EM_MessageText = ExampleTraxonMessageText;
				}

				Factory.Save();
				Assert("Message Number is not Empty", !message.EM_MessageNum.IsEmpty);
			}
		}

		public void TestRemovalOfMessageNumberPlaceHolder()
		{
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG82"))
			{
				CombineAssertions(() =>
				{
					AssertEquals("PreCondition Empty Message Number", ZString.Empty, message.EM_MessageNum);
					message.EM_MessageText = ExampleTraxonMessageText;
					AssertContains("PreCondition Place Holder Exists in Message Text", EDIMessage.MessageNumberPlaceHolder, message.EM_MessageText);
					Factory.Save();
					AssertNotContains("Message Number Place Holder is Removed", EDIMessage.MessageNumberPlaceHolder, message.EM_MessageText);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<TraxonMessage>();
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
		}
		TraxonMessage message;

		const string ExampleTraxonMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSEXP:D:95A:UN+HMF68974393X160'BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'LOC+5+BNE'LOC+8+HKG'CNT+7:94.0:KGM'CNT+8:3'NAD+PK+1331929'TDT+13+CX102'DTM+132:001223:101'RFF+MWB:16068974393'CNT+10:1'CNI+1+3134-001'CNT+8:3'MEA+WT++KGM:94.0'LOC+5+BNE'LOC+8+HKG'NAD+CN++FX 8621310489+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'NAD+CZ++TE 8522345678+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+QLD+++AU'GDS+12'FTX+AAA+++3 CARTONS STC HOMETIME SAMPL'PAC'PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'MOA+43:1710.02:AUD'MOA+44:12.34:AUD'DOC+811:::3B003571077FDC'UNT+26+<<MSGNO PLACEHOLDER>>'";
	}
}
