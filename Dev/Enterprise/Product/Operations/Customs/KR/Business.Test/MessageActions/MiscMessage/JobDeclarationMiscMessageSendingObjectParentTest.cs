using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationMiscMessageSendingObjectParent))]
	sealed class JobDeclarationMiscMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
		}

		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new JobDeclarationMiscMessageSendingObjectParent(declaration1, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var objectParent2 = new JobDeclarationMiscMessageSendingObjectParent(declaration2, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			AssertEquals(2, objectParent2.SendingObjectsCollection.Count);
		}

		public void TestObjectsToSend()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var objectParent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			Assert(!objectParent.HasAnyObjectToSend);
			objectParent.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals(1, objectParent.ObjectsToSend.Count());
			Assert(objectParent.HasAnyObjectToSend);
			AssertEquals(entry, objectParent.ObjectsToSend.Single().Header);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry2.MergedLines.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_SecondaryPreference = "A093000004";

			var object5FNParent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			Assert(!object5FNParent.HasAnyObjectToSend);
			object5FNParent.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(0, object5FNParent.ObjectsToSend.Count());

			object5FNParent.SendingObjectsCollection[2].MessageSendingEntryLines[0].ShouldSend = true;
			Assert(object5FNParent.HasAnyObjectToSend);
			AssertEquals(1, object5FNParent.ObjectsToSend.Count());
			AssertEquals(entry2, object5FNParent.ObjectsToSend.Single().Header);
		}
	}
}
