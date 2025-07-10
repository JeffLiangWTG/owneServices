using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AuthorityToDealTest : D99BCUSRESInfoProviderTest
	{
		#region TestDocumentName

		public override void TestDocumentName()
		{
			CUSRESMessage message = new CUSRESMessage();
			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("DocumentName", "ATD", aTD.DocumentName);
		}

		#endregion

		#region SecurityCode

		public void TestSecurityCode()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup3 group3 = message.Group3.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.ComplianceCodeNumber;
			rFF.Reference.ReferenceIdentifier = "AAAANNPTY";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("Security Code", "AAAANNPTY", aTD.SecurityCode);
		}

		#endregion

		#region AuthorityToDealDateIssued

		public void TestAuthorityToDealDateIssued()
		{
			CUSRESMessage message = new CUSRESMessage();
			DTMSegment dTM = message.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = DateTimePeriodFunctionCodeQualifierList.ClearanceDateCustoms;
			dTM.DateTimePeriod.DateTimePeriodValue = "20050204";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("AuthorityToDealDateIssued", "04-Feb-05", aTD.AuthorityToDealDateIssued);
		}

		#endregion

		#region TotalNumberOfPacakges

		public void TestTotalNumberOfPacakges()
		{
			CUSRESMessage message = new CUSRESMessage();
			CNTSegment cNT = message.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.TotalNumberOfPackages;
			cNT.Control.ControlValue = "123";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("TotalNumberOfPacakges", "123", aTD.TotalNumberOfPacakges);
		}

		#endregion

		#region AuthorityToDealActionReason

		public void TestAuthorityToDealActionReason()
		{
			CUSRESMessage message = new CUSRESMessage();
			FTXSegment fTX = message.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.CustomsClearanceInstructionImport;
			fTX.TextLiteral.FreeTextValue1 = "Authority to deal action reason";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("AuthorityToDealActionReason", "Authority to deal action reason", aTD.AuthorityToDealActionReason);
		}

		#endregion

		#region Warehouse Establishment

		public void TestWarehouseEstablishmentIdentifier()
		{
			CUSRESMessage message = new CUSRESMessage();
			LOCSegment lOC = message.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.Warehouse;
			lOC.LocationIdentification.LocationNameCode = "FA67B";
			lOC.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("WarehouseEstablishmentIdentifier", "FA67B", aTD.WarehouseEstablishmentIdentifier);
		}

		#endregion

		#region Lines

		public void TestLines()
		{
			CUSRESMessage message = new CUSRESMessage();
			SegmentGroup13 group13 = message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("Line Count", 1, aTD.Lines.Length);
		}

		public void TestNoLines()
		{
			CUSRESMessage message = new CUSRESMessage();
			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("Line Count", 0, aTD.Lines.Length);
		}

		#endregion

		#region Customs Officer Name

		public void TestCustomsOfficerName()
		{
			CUSRESMessage message = new CUSRESMessage();
			NADSegment nAD = message.Group1.InstantiateAChildAndAddItToChildrenCollection().NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Customs;
			nAD.NameAndAddress.NameAndAddressLine3 = "Name";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("CustomsOfficerName", "Name", aTD.CustomsOfficerName);
		}

		#endregion

		#region Workgroup Name

		public void TestWorkgroupName()
		{
			CUSRESMessage message = new CUSRESMessage();
			NADSegment nAD = message.Group1.InstantiateAChildAndAddItToChildrenCollection().NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Customs;
			nAD.NameAndAddress.NameAndAddressLine2 = "Workgroup";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("WorkgroupName", "Workgroup", aTD.WorkgroupName);
		}

		#endregion

		#region Customs Officer State

		public void TestCustomsOfficerState()
		{
			CUSRESMessage message = new CUSRESMessage();
			NADSegment nAD = message.Group1.InstantiateAChildAndAddItToChildrenCollection().NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Customs;
			nAD.NameAndAddress.NameAndAddressLine1 = "State";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("CustomsOfficerState", "State", aTD.CustomsOfficerState);
		}

		#endregion

		#region Customs Officer Email Address

		public void TestCustomsOfficerEmailAddress()
		{
			CUSRESMessage message = new CUSRESMessage();
			COMSegment cOM = message.Group1.InstantiateAChildAndAddItToChildrenCollection().Group2.InstantiateAChildAndAddItToChildrenCollection().COM.InstantiateAChildAndAddItToChildrenCollection();
			cOM.CommunicationContact.CommunicationNumberCodeQualifier = CommunicationNumberCodeQualifierList.ElectronicMail;
			cOM.CommunicationContact.CommunicationNumber = "email@email";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("CustomsOfficerEmailAddress", "email@email", aTD.CustomsOfficerEmailAddress);
		}

		#endregion

		#region Customs Officer Fax Number

		public void TestCustomsOfficerFaxNumber()
		{
			CUSRESMessage message = new CUSRESMessage();
			COMSegment cOM = message.Group1.InstantiateAChildAndAddItToChildrenCollection().Group2.InstantiateAChildAndAddItToChildrenCollection().COM.InstantiateAChildAndAddItToChildrenCollection();
			cOM.CommunicationContact.CommunicationNumberCodeQualifier = CommunicationNumberCodeQualifierList.Telefax;
			cOM.CommunicationContact.CommunicationNumber = "98765432";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("CustomsOfficerFaxNumber", "98765432", aTD.CustomsOfficerFaxNumber);
		}

		#endregion

		#region Customs Officer Telephone Number

		public void TestCustomsOfficerTelephoneNumber()
		{
			CUSRESMessage message = new CUSRESMessage();
			COMSegment cOM = message.Group1.InstantiateAChildAndAddItToChildrenCollection().Group2.InstantiateAChildAndAddItToChildrenCollection().COM.InstantiateAChildAndAddItToChildrenCollection();
			cOM.CommunicationContact.CommunicationNumberCodeQualifier = CommunicationNumberCodeQualifierList.Telephone;
			cOM.CommunicationContact.CommunicationNumber = "98765432";

			AuthorityToDeal aTD = new AuthorityToDeal(message);
			AssertEquals("CustomsOfficerTelephoneNumber", "98765432", aTD.CustomsOfficerTelephoneNumber);
		}

		#endregion

		protected override CUSRESMessage GetCUSRESMessage()
		{
			return new CUSRESMessage();
		}

		protected override D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES)
		{
			return new AuthorityToDeal(cUSRES);
		}
	}
}
