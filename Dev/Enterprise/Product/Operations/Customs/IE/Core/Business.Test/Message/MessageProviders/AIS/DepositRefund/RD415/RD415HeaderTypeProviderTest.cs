using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415HeaderTypeProviderTest : DataProviderTestCase<RD415HeaderTypeProvider>
	{
		public void TestIRD415HeaderType()
		{
			Assert("Should implement IRD415HeaderType", Provider is IRD415HeaderType);
		}

		public void TestApplicationReferenceId()
		{
			AssertEquals(AISOutboundEDIMessage.RD415ApplicationReferenceIdPlaceHolder, Provider.ApplicationReferenceId);
		}

		[TestDate(2024, 5, 12)]
		public void TestDate()
		{
			AssertEquals(new System.DateTime(2024, 5, 12), Provider.Date);
		}

		public void TestApplicant()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "D007");
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			AssertEquals("Declarant EORI", "IED007", Provider.Applicant);
		}

		protected override RD415HeaderTypeProvider GetProvider()
		{
			SetUpTestData();
			return new RD415HeaderTypeProvider(sendingAction);
		}

		void SetUpTestData()
		{
			if (sendingAction == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				sendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
			}
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		DepositRefundApplicationMessageSendingAction sendingAction;
	}
}
