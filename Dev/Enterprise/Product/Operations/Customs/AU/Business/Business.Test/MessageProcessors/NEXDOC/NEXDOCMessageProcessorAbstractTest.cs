using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestsSubclassesOf(typeof(NEXDOCMessageProcessor))]
	abstract class NEXDOCMessageProcessorAbstractTest : TestCaseWithFactory
	{
		protected GlbGroup emailGroup;
		protected EmbeddedResourceRetriever embeddedResourceRetriever;

		protected string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.BatchProcessor.TestFiles." + fileName;

		protected void SetupEmailGroups()
		{
			var factory = new BusinessObjectFactory();
			emailGroup = factory.New<GlbGroup>();
			var currentStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			currentStaffMember.GS_EmailAddress = "TEST@EDI.COM.AU";
			emailGroup.Staff.Add(currentStaffMember);
			factory.Save();

			AUCustomsDataRegistry.Instance.SendAQISAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailGroup.PK.ToGuid());
			AUCustomsDataRegistry.Instance.SendAQISErrors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.SendAQISErrorsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailGroup.PK.ToGuid());
			AUCustomsDataRegistry.Instance.SendAQISImpediments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.SendAQISImpedimentsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailGroup.PK.ToGuid());
		}

		protected EDIMessage CreateNEXDOCInterchangeAndMessageFromUniversalXML(ZString universalXMLText)
		{
			var nextMessageNumber = 0;
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(universalXMLText);

			var headerNode = xmlDoc.SelectSingleNode("//*[local-name()='UniversalInterchange']//*[local-name()='Header']");
			var bodyNode = xmlDoc.SelectSingleNode("//*[local-name()='UniversalInterchange']//*[local-name()='Body']");

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "NEX_EI_00" + nextMessageNumber++;
			interchange.EI_HeaderText = headerNode.OuterXml;
			interchange.EI_BodyText = bodyNode.OuterXml;
			interchange.EI_Status = EDIMessage.Status.Received;

			var message = interchange.ContainedMessages.AddNew();
			message.EM_MessageNum = "NEX_EM_00" + nextMessageNumber++;
			message.EM_MessageType = EDIMessage.ApplicationCodes.NEXDOCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = bodyNode.InnerXml;
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}
	}
}
