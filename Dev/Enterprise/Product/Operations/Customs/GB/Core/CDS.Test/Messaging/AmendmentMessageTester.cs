using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	internal class AmendmentMessageTester
	{
		public AmendmentMessageTester(ZString initialXML, ZString comparisonXML)
		{
			this.initialXML = initialXML;
			this.comparisonXML = comparisonXML;
		}

		WCOJobDeclarationMessageSendingObject GetMessageSendingObject()
		{
			var factory = new BusinessObjectFactory();
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(factory);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var sendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault(x => x.Header.PK.Equals(entryHeader.PK));

			return sendingObject;
		}

		public ZString RunTest()
		{
			var namespacesToKeep = new List<XNamespace>
									{ "urn:wco:datamodel:WCO:Declaration_DS:DMS:2" };
			var helper = new AmendmentMessageHelperForTest(null);
			var objectToSend = GetMessageSendingObject();
			var diffGram = helper.GetDiffGramExposed(initialXML, comparisonXML, namespacesToKeep);
			objectToSend.AmendmentDetails = new AmendmentDetails(diffGram, initialXML, comparisonXML);
			var amendment = helper.GetAmendmentsExposed(objectToSend);
			var actualXmlDoc = amendment.Xml.ToString();

			return actualXmlDoc;
		}

		readonly ZString initialXML;
		readonly ZString comparisonXML;
	}
}
