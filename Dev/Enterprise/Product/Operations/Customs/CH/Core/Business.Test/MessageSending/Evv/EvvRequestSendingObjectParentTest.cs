using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvRequestSendingObjectParent))]
sealed class EvvRequestSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestFactory()
	{
		AssertSame(Factory, SendingObjectParent.Factory);
	}

	public void TestSetDefaults() => CombineAssertions(() =>
	{
		const string mrn = "123456789.1";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(mrn);
		var sendingObjectParent = new EvvRequestSendingObjectParent(entryHeader);

		AssertEquals("MRN default", "123456789", sendingObjectParent.Mrn);
		AssertEquals("MRN Version default", 1, sendingObjectParent.MrnVersion);
		AssertEquals("SendCustomsDuties default", true, sendingObjectParent.SendCustomsDuties);
		AssertEquals("SendVat default", true, sendingObjectParent.SendVat);
		AssertEquals("SendReimbursementCustomsDuties default", false, sendingObjectParent.SendReimbursementCustomsDuties);
		AssertEquals("SendReimbursementVat default", false, sendingObjectParent.SendReimbursementVat);
	});

	public void TestMrnReadOnly()
	{
		AssertEquals("ReadOnly", true, SendingObjectParent.MrnInfo.ReadOnly);
	}

	public void TestMrnVersion() => CombineAssertions(() =>
	{
		AssertEquals("Mrn Version MaxLength", 2, SendingObjectParent.MrnVersionInfo.MaxLength);
		AssertEquals("Mrn Version default value", 0, SendingObjectParent.MrnVersion);
	});

	public void TestSendingObjects()
	{
		CombineAssertions(() =>
		{
			SendingObjectParent.SendReimbursementCustomsDuties = true;
			SendingObjectParent.SendReimbursementVat = true;
			var sendingObjects = SendingObjectParent.SelectedSendingObjects.Cast<EvvRequestSendingObject>();

			AssertEquals("count", 4, sendingObjects.Count());

			AssertMessage(sendingObjects.ElementAt(0), 0, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, EvvDocumentType.Codes.TaxationDecisionCustomsDuties);
			AssertMessage(sendingObjects.ElementAt(1), 1, MessageSubTypeCodeList.Codes.TaxationDecisionVat, EvvDocumentType.Codes.TaxationDecisionVAT);
			AssertMessage(sendingObjects.ElementAt(2), 2, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, EvvDocumentType.Codes.RefundCustomsDuties);
			AssertMessage(sendingObjects.ElementAt(3), 3, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, EvvDocumentType.Codes.RefundVAT);

			void AssertMessage(EvvRequestSendingObject sendingObject, int sendingObjectNumber, ZString expectedMessageSubType, ZString expectedDocumentType)
			{
				AssertEquals($"SendingObject {sendingObjectNumber}, DocumentType", expectedDocumentType, sendingObject.DocumentType);
				AssertEquals($"SendingObject {sendingObjectNumber}, MessageSubType", expectedMessageSubType, sendingObject.MessageSubTypeForEDIMessage);
			}
		});
	}

	EvvRequestSendingObjectParent SendingObjectParent => sendingObjectParent ??= GetSendingObjectParent();
	EvvRequestSendingObjectParent sendingObjectParent;

	protected override BusinessObject GetNewBusinessObject() => GetSendingObjectParent();

	EvvRequestSendingObjectParent GetSendingObjectParent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new EvvRequestSendingObjectParent(entryHeader);
	}
}
