using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.FR
{
	[TestedType(typeof(UpdateBM_EntryDateFromTP5IE028Message))]
	internal class UpdateBM_EntryDateFromTP5IE028MessageTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateBM_EntryDateFromTP5IE028Message();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update BM_EntryDate From TP5 028 Message._1] ON [dbo].[EDIMessage] ([EM_ApplicationCode], [EM_ReceiveTransmit], [EM_Status], [EM_MessageType], [EM_MessageSubType]) INCLUDE ([EM_LinkUniqueID], [EM_MessageData], [EM_SystemCreateTimeUtc]) WHERE ([EM_ApplicationCode]='FRC' AND [EM_ReceiveTransmit]='RCV' AND [EM_Status]='PRS' AND [EM_MessageType]='TP5' AND [EM_MessageSubType]='028') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update BM_EntryDate From TP5 028 Message._2] ON [dbo].[CusInBondHeader] ([BH_HeaderType], [BH_ApplicationCode]) WHERE ([BH_HeaderType]='D' AND [BH_ApplicationCode]='NC5') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			AssertEquals("EntryDate has been updated to the declarationAcceptanceDate of the last received 028 message.", new DateTime(2024, 06, 01, 0, 0, 0), TestConnection.ExecuteScalar($"SELECT BM_EntryDate FROM CusInBondMoveHeader WHERE BM_PK = '{cusInBondMoveHeaderDeparture1PK}'"));
			AssertEquals("EntryDate already has a value and will not be updated.", new DateTime(2022, 07, 30, 0, 0, 0), TestConnection.ExecuteScalar($"SELECT BM_EntryDate FROM CusInBondMoveHeader WHERE BM_PK = '{cusInBondMoveHeaderDeparture2PK}'"));
			AssertEquals("The EntryDate for phase 4 will not be updated.", DBNull.Value, TestConnection.ExecuteScalar($"SELECT BM_EntryDate FROM CusInBondMoveHeader WHERE BM_PK = '{cusInBondMoveHeaderPhase4DeparturePK}'"));
			AssertEquals("The EntryDate for arrival will not be updated.", DBNull.Value, TestConnection.ExecuteScalar($"SELECT BM_EntryDate FROM CusInBondMoveHeader WHERE BM_PK = '{cusInBondMoveHeaderArrivalPK}'"));
		}

		protected override void PrepareTestData()
		{
			var companyPK = Guid.NewGuid();
			var branchPK = Guid.NewGuid();

			var sql1 = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
				VALUES ('{0}', '{1}', '{2}', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
				VALUES ('{4}', '{5}', 'TFF', 'FR', '2022-07-30T00:00:00', 'E', '2022-07-30T00:00:00', 'E')",
				companyPK, "FR", "DFR", "CLASQUIN SA", branchPK, companyPK);
			using (var cmd1 = Db.Connection.Command(sql1))
			{
				cmd1.ExecuteNonQuery();
			}

			var departmentPK = TestDataCreator.CreateDepartment("DEP1");

			var messageData1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Message>
  <EnveloppeMessage>
    <schemaID>DELTAT_MESSAGE_CC028C</schemaID>
    <schemaVersion>1.0</schemaVersion>
    <partyId>FR34906904700018</partyId>
    <transactionId>WTLDFRFRM+DTD+0000007735</transactionId>
    <numseq>3</numseq>
  </EnveloppeMessage>
  <MessageBody>
    <CC028C xmlns=""http://ncts.dgtaxud.ec"">
      <messageSender xmlns="""">Token1</messageSender>
      <messageRecipient xmlns="""">Token1</messageRecipient>
      <preparationDateAndTime xmlns="""">2024-05-01T12:34:56Z</preparationDateAndTime>
      <messageIdentification xmlns="""">Token1</messageIdentification>
      <messageType xmlns="""">CC028C</messageType>
      <correlationIdentifier xmlns="""">Token1</correlationIdentifier>
      <TransitOperation xmlns="""">
        <LRN>LRN28</LRN>
        <MRN>MRN28</MRN>
        <declarationAcceptanceDate>2024-05-01</declarationAcceptanceDate>
      </TransitOperation>
      <CustomsOfficeOfDeparture xmlns="""">
        <referenceNumber>referen1</referenceNumber>
      </CustomsOfficeOfDeparture>
      <HolderOfTheTransitProcedure xmlns="""">
        <identificationNumber>identificationNu1</identificationNumber>
        <TIRHolderIdentificationNumber>TIRHolderIdentif1</TIRHolderIdentificationNumber>
        <name>name1</name>
        <Address>
          <streetAndNumber>streetAndNumber1</streetAndNumber>
          <postcode>postcode1</postcode>
          <city>city1</city>
          <country>c1</country>
        </Address>
      </HolderOfTheTransitProcedure>
    </CC028C>
  </MessageBody>
</Message>
";

			var messageData2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Message>
  <EnveloppeMessage>
    <schemaID>DELTAT_MESSAGE_CC028C</schemaID>
    <schemaVersion>1.0</schemaVersion>
    <partyId>FR34906904700018</partyId>
    <transactionId>WTLDFRFRM+DTD+0000007735</transactionId>
    <numseq>3</numseq>
  </EnveloppeMessage>
  <MessageBody>
    <CC028C xmlns=""http://ncts.dgtaxud.ec"">
      <messageSender xmlns="""">Token1</messageSender>
      <messageRecipient xmlns="""">Token1</messageRecipient>
      <preparationDateAndTime xmlns="""">2024-05-01T12:34:56Z</preparationDateAndTime>
      <messageIdentification xmlns="""">Token1</messageIdentification>
      <messageType xmlns="""">CC028C</messageType>
      <correlationIdentifier xmlns="""">Token1</correlationIdentifier>
      <TransitOperation xmlns="""">
        <LRN>LRN28</LRN>
        <MRN>MRN28</MRN>
        <declarationAcceptanceDate>2024-06-01</declarationAcceptanceDate>
      </TransitOperation>
      <CustomsOfficeOfDeparture xmlns="""">
        <referenceNumber>referen1</referenceNumber>
      </CustomsOfficeOfDeparture>
      <HolderOfTheTransitProcedure xmlns="""">
        <identificationNumber>identificationNu1</identificationNumber>
        <TIRHolderIdentificationNumber>TIRHolderIdentif1</TIRHolderIdentificationNumber>
        <name>name1</name>
        <Address>
          <streetAndNumber>streetAndNumber1</streetAndNumber>
          <postcode>postcode1</postcode>
          <city>city1</city>
          <country>c1</country>
        </Address>
      </HolderOfTheTransitProcedure>
    </CC028C>
  </MessageBody>
</Message>
";

			var interchangePK1 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "1", "RCV");
			var interchangePK2 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "2", "RCV");
			var interchangePK3 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "3", "RCV");
			var interchangePK4 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "4", "RCV");
			var interchangePK5 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "5", "RCV");

			var sql = $@"
				DECLARE @cusInBondHeaderDeparture1PK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDeparture2PK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderPhase4DeparturePK		UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderArrivalPK				UNIQUEIDENTIFIER = NEWID();

INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
				(@cusInBondHeaderDeparture1PK, '{branchPK}', 'BH01', 'NC5', 'D', '2022-07-30', 1, GetUtcDate(), 'KCH','KCH'),
				(@cusInBondHeaderDeparture2PK, '{branchPK}', 'BH02', 'NC5', 'D', '2022-12-15', 1, GetUtcDate(), 'KCH','KCH'),
				(@cusInBondHeaderPhase4DeparturePK, '{branchPK}', 'BH03', 'NCT', 'D', '2022-12-15', 1, GetUtcDate(), 'KCH','KCH'),
				(@cusInBondHeaderArrivalPK, '{branchPK}', 'BH04', 'NC5', 'A', '2023-03-02', 1, GetUtcDate(), 'KCH','KCH')

INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SystemCreateTimeUtc,	BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_ValuationDate, BM_SpecificCircumstance, BM_EntryDate) VALUES
				(@cusInBondMoveHeaderDeparture1PK,		@cusInBondHeaderDeparture1PK,		'2022-07-30', 'KCH', '2023-01-01 01:01:00', 'KCH', '2022-07-30', '1', NULL),
				(@cusInBondMoveHeaderDeparture2PK,		@cusInBondHeaderDeparture2PK,		'2023-07-30', 'KCH', '2023-01-01 01:01:00', 'KCH', '2022-07-30', '1', '2022-07-30'),
				(@cusInBondMoveHeaderPhase4DeparturePK,	@cusInBondHeaderPhase4DeparturePK,	'2023-07-30', 'KCH', '2023-01-01 01:01:00', 'KCH', '2022-07-30', '2', NULL),
				(@cusInBondMoveHeaderArrivalPK,			@cusInBondHeaderArrivalPK,			'2022-12-15', 'KCH', '2023-01-01 01:01:00', 'KCH', '2022-12-15', '1', NULL);

INSERT INTO EDIMessage
	(EM_PK, EM_EI, EM_ApplicationCode, EM_MessageType, EM_MessageSubType, EM_Status, EM_ReceiveTransmit, EM_LinkTable, EM_LinkUniqueID, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_MessageData, EM_MessageNum, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
VALUES
	(NEWID(), '{interchangePK1}', 'FRC', 'TP5', '028', 'PRS', 'RCV', 'CusInBondHeader', @cusInBondHeaderDeparture1PK, '{branchPK}', '{departmentPK}', '2022-03-01', dbo.CLRCompressStringAsBytes('{messageData1}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK2}', 'FRC', 'TP5', '028', 'PRS', 'RCV', 'CusInBondHeader', @cusInBondHeaderDeparture1PK, '{branchPK}', '{departmentPK}', '2022-04-01', dbo.CLRCompressStringAsBytes('{messageData2}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK3}', 'FRC', 'TP5', '028', 'PRS', 'RCV', 'CusInBondHeader', @cusInBondHeaderDeparture2PK, '{branchPK}', '{departmentPK}', '2022-03-01', dbo.CLRCompressStringAsBytes('{messageData1}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK4}', 'FRC', 'TP5', '028', 'PRS', 'RCV', 'CusInBondHeader', @cusInBondHeaderPhase4DeparturePK, '{branchPK}', '{departmentPK}', '2022-03-01', dbo.CLRCompressStringAsBytes('{messageData1}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK5}', 'FRC', 'TP5', '028', 'PRS', 'RCV', 'CusInBondHeader', @cusInBondHeaderArrivalPK, '{branchPK}', '{departmentPK}', '2022-03-01', dbo.CLRCompressStringAsBytes('{messageData1}'), '1', '~BP', getutcdate(), '~BP');";

			var cmd = Db.Connection.Command(sql);
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
			cmd.AddParameter("@cusInBondMoveHeaderDeparture1PK", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderDeparture1PK);
			cmd.AddParameter("@cusInBondMoveHeaderDeparture2PK", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderDeparture2PK);
			cmd.AddParameter("@cusInBondMoveHeaderPhase4DeparturePK", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderPhase4DeparturePK);
			cmd.AddParameter("@cusInBondMoveHeaderArrivalPK", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderArrivalPK);
			cmd.ExecuteNonQuery();
		}

		Guid cusInBondMoveHeaderDeparture1PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDeparture2PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderPhase4DeparturePK = Guid.NewGuid();
		Guid cusInBondMoveHeaderArrivalPK = Guid.NewGuid();
	}
}
