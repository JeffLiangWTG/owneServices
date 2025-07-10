using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotEDIMessageCollection))]
	class CusStorageDocPivotEDIMessageCollectionTest : BusinessObjectCollectionViewTestCase<CusStorageDocPivotEDIMessageCollection>
	{
		protected override CusStorageDocPivotEDIMessageCollection GetCollectionToTest()
		{
			return new CusStorageDocPivotEDIMessageCollection(loadedPivot, entryHeader.Messages);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var newMessage = Factory.NewWithValidTestData<EDIMessage>();
			entryHeader.Messages.Add(newMessage);
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = loadedPivot.PK;
			messagePivot.XX_Relation1TableCode = loadedPivot.TablePrefix;
			messagePivot.XX_Relation2ID = newMessage.PK;
			messagePivot.XX_Relation2TableCode = newMessage.TablePrefix;
			Factory.Save();
			return newMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			var newPivot = entryHeader.EDocPivotCollection.AddNew();
			newPivot.CSD_DocType = "T1";
			Factory.Save();
			loadedPivot = new BusinessObjectFactory().Load<CusStorageDocPivot>(newPivot.PK);

			message = Factory.NewWithValidTestData<EDIMessage>();
			entryHeader.Messages.Add(message);
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = loadedPivot.PK;
			messagePivot.XX_Relation1TableCode = loadedPivot.TablePrefix;
			messagePivot.XX_Relation2ID = message.PK;
			messagePivot.XX_Relation2TableCode = message.TablePrefix;
			Factory.Save();
		}
		EDIMessage message;
		CusStorageDocPivot loadedPivot;
		CusEntryHeader entryHeader;
	}
}
