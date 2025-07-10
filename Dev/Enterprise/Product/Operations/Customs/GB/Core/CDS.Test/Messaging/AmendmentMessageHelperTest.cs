using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(AmendmentMessageHelper))]
	class AmendmentMessageHelperTest : AmendmentMessageHelperAbstractTest<AmendmentMessageHelper>
	{
		public void NamespacesToKeep_ShouldContainCustomNamespace()
		{
			var helper = new AmendmentMessageHelperForTest(null);

			var result = helper.NamespacesToKeepExposed;

			AssertNotNull("NamespacesToKeep should return a non-null list.", result);
			AssertEquals("NamespacesToKeep should contain exactly one namespace.", 1, result.Count);
			AssertEquals("NamespacesToKeep should contain the correct namespace.", "urn:wco:datamodel:WCO:Declaration_DS:DMS:2", result[0].NamespaceName);
		}

		protected override string InitialXML => "Enterprise.Customs.GB.CDS.Testing.Messaging.AmendmentAdditionalInformation.xml";

		protected override string ExpectedXML => "Enterprise.Customs.GB.CDS.Testing.Messaging.AmendmentAdditionalInformationExpected.xml";

		protected override string InitialXMLForSendChanges => "<Root><Declaration><ID>InitialDeclaration</ID></Declaration></Root>";

		protected override string NewXMLForSendChanges => "<Root><Declaration><ID>NewDeclaration</ID></Declaration></Root>";

		protected override string ExpectedXMLForSendChanges => "<Declaration><ID>NewDeclaration</ID></Declaration>";

		protected override AmendmentMessageHelper MessageHelper => AmendmentMessageHelper.Instance;

		protected override WCOJobDeclarationMessageSendingObject MessageSendingObject
		{
			get
			{
				var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
				MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

				entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

				var invoice = entryHeader.Declaration.Invoices[0];
				var invoiceLine = invoice.InvoiceLines[0];
				invoiceLine.JI_Tariff = "1234567890";
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_LinePrice = 200;

				var container3 = entryHeader.Declaration.CusContainers.AddNew();
				container3.CO_ContainerNumber = "CNT789";

				var container4 = entryHeader.Declaration.CusContainers.AddNew();
				container4.CO_ContainerNumber = "CNT999";

				entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
				entryHeader.CH_CustomsMessageRemarks = "Amending";
				entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
				var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
				var sendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault(x => x.Header.PK.Equals(entryHeader.PK));

				return sendingObject;
			}
		}
	}
}
