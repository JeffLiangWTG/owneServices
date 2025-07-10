using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing;

[TestedType(typeof(SendH7ToCustomsApplicator))]
public class SendH7ToCustomsApplicatorTest : OperationalActionMethodApplicatorTest
{
	public void TestSubmitMessagesSuccessfully()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
		{
			var applicater = new SendH7ToCustomsApplicator();
			var logger = new DummyOperationalActionSectionLog();
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "IE1234";
			var bill1 = header1.Bills.AddNew();
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "IE2345";
			var bill2 = header2.Bills.AddNew();
			applicater.Apply(logger, new AsycudaManifestHeader[] { header1, header2 });

			CombineAssertions(() =>
			{
				AssertEquals("415", ((EDIMessage)bill1.Messages.Single()).EM_MessageType);
				AssertEquals("415", ((EDIMessage)bill2.Messages.Single()).EM_MessageType);
				Assert(logger.messages.Contains("SUCCESS: [HL IE1234] : Submit Succeeded."));
				Assert(logger.messages.Contains("SUCCESS: [HL IE2345] : Submit Succeeded."));
			});
		}
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new SendH7ToCustomsApplicator();
	}
}
