using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class DeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	[GuiTest]
	public void TestCanSendMessage_CheckDeniedParty()
	{
		SetMessageSendingEnvironment();

		var declarationForTest = Factory.NewWithValidTestData<JobDeclarationForTest>();
		declarationForTest.DPSFreightMovementRestricted = false;
		declarationForTest.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration = declarationForTest;
		Factory.Save();

		var messageSendingObjectParent = (IMessageSendingObjectParent)GetNewBusinessObject();

		CombineAssertions(() =>
		{
			declarationForTest.DPSFreightMovementRestricted = true;
			AssertEquals("Has customs registration number", "Unable to submit message due to Denied Party Screening cancellation.", messageSendingObjectParent.CanSendMessage());
		});
	}

	protected abstract void SetMessageSendingEnvironment();

	public abstract void TestCanSendMessage_CheckMessageSendingEnvironment();

	protected abstract string MessageType { get; }

	protected virtual JobDeclaration GetNewJobDeclaration(BusinessObjectFactory factory)
	{
		var jobDeclaration = factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return jobDeclaration;
	}

	protected JobDeclaration Declaration => declaration ?? (declaration = GetNewJobDeclaration(Factory));
	JobDeclaration declaration;

	class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			DPSPartiesForTesting = System.Array.Empty<ScreeningParty>();
		}

		public bool DPSFreightMovementRestricted { get; set; }
		public ScreeningParty[] DPSPartiesForTesting { get; set; }

		protected override bool IsDPSFreightMovementRestrictedCore() => DPSFreightMovementRestricted;
		protected override ScreeningParty[] GetScreeningPartiesCore() => DPSPartiesForTesting;
	}
}
