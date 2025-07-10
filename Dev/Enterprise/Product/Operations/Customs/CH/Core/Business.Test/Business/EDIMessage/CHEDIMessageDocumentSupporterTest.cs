using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHEDIMessageDocumentSupporter))]
public class CHEDIMessageDocumentSupporterTest : DocumentSupporterTest
{
	public void TestListOfSupportedDataContexts() => CombineAssertions(() =>
	{
		var documentSupporter = CreateDocumentSupporter();
		var dataContexts = documentSupporter.ListOfSupportedDataContexts.Cast<CodeDescriptionPair>().Select(c => c.Code);

		AssertCollectionContains(".EVVTaxationDocument", CHEDIMessageDocumentSupporter.EVVTaxationDocument, dataContexts);
		AssertCollectionContains(".EVVRefundDocument", CHEDIMessageDocumentSupporter.EVVRefundDocument, dataContexts);
		AssertCollectionContains(".EVVValidationDocument", CHEDIMessageDocumentSupporter.EVVValidationDocument, dataContexts);
	});

	public void TestGetBODocDataProvidersForVAT()
	{
		var message = CreateMessage();
		message.EM_MessageText = TestingData.InputEvvResponseVAT();
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);

		var dataProviders = documentSupporter.GetBODocDataProviders(new DataContextValue(CHEDIMessageDocumentSupporter.EVVTaxationDocument), null);

		AssertEquals(1, dataProviders.Length);
		AssertType<EVVTaxationDocumentWrapper>(dataProviders[0]);
	}

	public void TestGetBODocDataProvidersForDuties()
	{
		var message = CreateMessage();
		message.EM_MessageText = TestingData.InputEvvResponseDTY();
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);

		var dataProviders = documentSupporter.GetBODocDataProviders(new DataContextValue(CHEDIMessageDocumentSupporter.EVVTaxationDocument), null);

		AssertEquals(1, dataProviders.Length);
		AssertType<EVVTaxationDocumentWrapper>(dataProviders[0]);
	}

	public void TestGetBODocDataProvidersForRefundVAT()
	{
		var message = CreateMessage();
		message.EM_MessageText = TestingData.InputEvvResponseRefundVAT();
		message.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		message.EM_MessageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat;
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);

		var dataProviders = documentSupporter.GetBODocDataProviders(new DataContextValue(CHEDIMessageDocumentSupporter.EVVRefundDocument), null);

		AssertEquals(1, dataProviders.Length);
		AssertType<EVVRefundDocumentWrapper>(dataProviders[0]);
	}

	public void TestGetBODocDataProvidersForRefundCustomsDuties()
	{
		var message = CreateMessage();
		message.EM_MessageText = TestingData.InputEvvResponseRefundCustomsDuties();
		message.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		message.EM_MessageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties;
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);

		var dataProviders = documentSupporter.GetBODocDataProviders(new DataContextValue(CHEDIMessageDocumentSupporter.EVVRefundDocument), null);

		AssertEquals(1, dataProviders.Length);
		AssertType<EVVRefundDocumentWrapper>(dataProviders[0]);
	}

	public void ODocDataProvidersForTaxationValidation()
	{
		var message = CreateMessage();
		message.EM_MessageText = TestingData.InputEvvResponseDTY();
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);

		var dataProviders = documentSupporter.GetBODocDataProviders(new DataContextValue(CHEDIMessageDocumentSupporter.EVVValidationDocument), null);

		AssertEquals(1, dataProviders.Length);
		AssertType<EvvSignatureDocumentWrapper>(dataProviders[0]);
	}

	public void ODocDataProvidersForRefundValidation()
	{
		var message = CreateMessage();
		message.EM_MessageText = TestingData.InputEvvResponseRefundCustomsDuties();
		message.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		message.EM_MessageSubType = MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties;
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);

		var dataProviders = documentSupporter.GetBODocDataProviders(new DataContextValue(CHEDIMessageDocumentSupporter.EVVValidationDocument), null);

		AssertEquals(1, dataProviders.Length);
		AssertType<EvvSignatureDocumentWrapper>(dataProviders[0]);
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => CreateMessage();

	CHEDIMessage CreateMessage()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		return (CHEDIMessage)entryHeader.Messages.AddNew();
	}

	CHEDIMessageDocumentSupporter CreateDocumentSupporter()
	{
		var message = CreateMessage();
		var documentSupporter = new CHEDIMessageDocumentSupporter(message);
		return documentSupporter;
	}
}
