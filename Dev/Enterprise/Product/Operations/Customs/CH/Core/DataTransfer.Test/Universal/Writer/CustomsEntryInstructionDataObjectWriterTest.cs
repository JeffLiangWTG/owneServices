using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

sealed class CustomsEntryInstructionDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestAdditionalSupplyChainActors()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var actor1 = entryInstruction.SupplyChainActors.AddNew();
		actor1.CFR_Code = Customs.Business.SupplyChainActorRoleList.Codes.CS;
		actor1.CFR_Reference = "REF123";
		var address1 = Factory.NewWithValidTestData<OrgAddress>();
		address1.CompanyName = "Test Company 1";
		address1.Address1 = "123 Test Street";
		address1.Address2 = "Town";
		address1.Postcode = "A12B3C4";
		actor1.CFR_OA_Owner = address1.PK;
		actor1.CFR_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

		var actor2 = entryInstruction.SupplyChainActors.AddNew();
		actor2.CFR_Code = Customs.Business.SupplyChainActorRoleList.Codes.MF;
		actor2.CFR_Reference = "REF456";
		var address2 = Factory.NewWithValidTestData<OrgAddress>();
		address2.CompanyName = "Test Company 2";
		address2.Address1 = "456 Test Street";
		address2.Address2 = "City";
		address2.Postcode = "X98Y7Z6";
		actor2.CFR_OA_Owner = address2.PK;
		actor2.CFR_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

		var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryInstruction)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
		var dataObject = writer.GetDataObject(entryInstruction);

		var actorDataObjects = dataObject.CustomsReferenceCollection.Where(c => c.Type.Code.Value == Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor).OrderBy(c => c.Reference).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("EntryInstruction Supply Chain Actor count", 2, actorDataObjects.Length);
			AssertSupplyChainActor(actorDataObjects[0], "REF123", "Test Company 1");
			AssertSupplyChainActor(actorDataObjects[1], "REF456", "Test Company 2");
		});

		void AssertSupplyChainActor(CustomsReference actor, ZString reference, ZString companyName)
		{
			AssertEquals("Type.Code", Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, actor.Type.Code);
			AssertEquals("Reference", reference, actor.Reference);
			AssertEquals("Owner.CompanyName", companyName, actor.Owner.CompanyName);
			AssertNull("ReferencedEntityDescription", actor.ReferencedEntityDescription);
			AssertNull("IsOverridden", actor.IsOverridden);
			AssertNull("Order", actor.Order);
			AssertNull("DateCollection", actor.DateCollection);
		}
	}
}
