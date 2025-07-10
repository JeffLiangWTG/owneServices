using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.FR
{
	[TestedType(typeof(UpdateZG_ConfirmedGuaranteeAmountFromDeltaGMessage))]
	internal class UpdateZG_ConfirmedGuaranteeAmountFromDeltaGMessageTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateZG_ConfirmedGuaranteeAmountFromDeltaGMessage();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update ZG_ConfirmedGuaranteeAmount From DeltaG Message._1] ON [dbo].[EDIMessage] ([EM_EI], [EM_LinkUniqueID]) WHERE ([EM_ApplicationCode]='FRC' AND [EM_ReceiveTransmit]='RCV' AND [EM_Status]='PRS' AND ([EM_MessageType] IN ('IMC', 'IMD', 'EXC', 'EXD'))) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update ZG_ConfirmedGuaranteeAmount From DeltaG Message._2] ON [dbo].[GenAddOnColumn] ([XA_ParentID]) WHERE ([XA_ParentTableCode]='JE' AND [XA_Name]='JE_DeltaMode' AND [XA_Data]='G1') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update ZG_ConfirmedGuaranteeAmount From DeltaG Message._3] ON [dbo].[GenAddOnColumn] ([XA_ParentID]) WHERE ([XA_ParentTableCode]='JE' AND [XA_Name]='JE_DeltaMode' AND [XA_Data]='G2') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update ZG_ConfirmedGuaranteeAmount From DeltaG Message._4] ON [dbo].[CusEntryHeader] ([CH_PK], [CH_JE]) WHERE ([CH_DataModel]='FR' AND [CH_EntryStatus]>='050') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update ZG_ConfirmedGuaranteeAmount From DeltaG Message._5] ON [dbo].[CusEntryHeader] ([CH_PK], [CH_JE]) WHERE ([CH_DataModel]='FR' AND [CH_EntryStatus]>='130') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		protected override void AssertTransformationResults()
		{
			AssertCusEntryHeaderConfirmedGuaranteeAmount(cusEntryHeaderPK1, "ConfirmedGuaranteeAmount", "10");
			AssertCusEntryHeaderConfirmedGuaranteeAmount(cusEntryHeaderPK2, "ConfirmedGuaranteeAmount", "");
			AssertCusEntryHeaderConfirmedGuaranteeAmount(cusEntryHeaderPK3, "ConfirmedGuaranteeAmount", "");
			AssertCusEntryHeaderConfirmedGuaranteeAmount(cusEntryHeaderPK4, "ConfirmedGuaranteeAmount", "");
			AssertCusEntryHeaderConfirmedGuaranteeAmount(cusEntryHeaderPK5, "ConfirmedGuaranteeAmount", "10");
			AssertCusEntryHeaderConfirmedGuaranteeAmount(cusEntryHeaderPK6, "ConfirmedGuaranteeAmount", "");
		}

		void AssertCusEntryHeaderConfirmedGuaranteeAmount(Guid pk, string addInfoEntries, string valueExpected)
		{
			var (selectBuilder, crossApplyBuilder) = FillSelectAndCrossApply("CH_AddInfo", addInfoEntries);

			var sql = $@"
SELECT {selectBuilder}
FROM CusEntryHeader
{crossApplyBuilder}
WHERE CH_PK = @pk";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, CusEntryInstructionSchema.PK);
				using (var reader = command.ExecuteReader())
				{
					Assert("Reader should have rows", ((SqlDataReader)reader).HasRows);
					reader.Read();
					AssertEquals("ConfirmedGuaranteeAmount expected to be equal", valueExpected, reader.GetValue<string>(addInfoEntries));
				}
			}
		}

		static (string selectBuilder, string crossApplyBuilder) FillSelectAndCrossApply(string addInfoColumnName, string addInfoEntries)
		{
			var selectBuilder = new StringBuilder();
			var crossApplyBuilder = new StringBuilder();

			if (selectBuilder.Length == 0)
			{
				selectBuilder.Append($"{addInfoEntries}.Value AS {addInfoEntries}");
			}
			else
			{
				selectBuilder.Append($", {addInfoEntries}.Value AS {addInfoEntries}");
			}

			crossApplyBuilder.AppendLine($"CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull({addInfoColumnName}, '{addInfoEntries}') AS {addInfoEntries}");

			return (selectBuilder.ToString(), crossApplyBuilder.ToString());
		}

		protected override void PrepareTestData()
		{
			var companyPK = Guid.NewGuid();

			var sql1 = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", companyPK, "FR", "DFR", "CLASQUIN SA");
			using (var cmd1 = Db.Connection.Command(sql1))
			{
				cmd1.ExecuteNonQuery();
			}

			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "FRANCE");
			var departmentPK = TestDataCreator.CreateDepartment("DEP1");

			declarationPK1 = Guid.NewGuid();
			declarationPK2 = Guid.NewGuid();
			declarationPK3 = Guid.NewGuid();
			declarationPK4 = Guid.NewGuid();
			declarationPK5 = Guid.NewGuid();
			declarationPK6 = Guid.NewGuid();

			cusEntryHeaderPK1 = Guid.NewGuid();
			cusEntryHeaderPK2 = Guid.NewGuid();
			cusEntryHeaderPK3 = Guid.NewGuid();
			cusEntryHeaderPK4 = Guid.NewGuid();
			cusEntryHeaderPK5 = Guid.NewGuid();
			cusEntryHeaderPK6 = Guid.NewGuid();

			var baseMessageDataWithMontant = @"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>
<Message>
  <EnveloppeMessage>
    <schemaID>MessageReponseCDecImp</schemaID>
    <schemaVersion>18122012</schemaVersion>
    <partyId>40218856900048</partyId>
    <transactionId>AAAAAAAAA+BBB+0000000001</transactionId>
    <numseq>0</numseq>
  </EnveloppeMessage>
  <ReponseDeclaration>
    <Entete>
      <refdec>1901204207</refdec>
      <refdos>9000-B00177613</refdos>
      <dateValidationDec>13/02/2019</dateValidationDec>
    </Entete>
    <ReponseDatas>
      <Notification>
        <Etat>
          <etat>BAE</etat>
          <etatDate>13/02/2019</etatDate>
          <etatHeure>04:04</etatHeure>
          <etatprec>VALIDE</etatprec>
          <etatprecDate>13/02/2019</etatprecDate>
          <etatprecHeure>04:04</etatprecHeure>
          <evenement>Expiration du timer</evenement>
        </Etat>
        <LiquidationGen>
          <montantcautionne>5539</montantcautionne>
          <montantnoncautionne>0</montantnoncautionne>
          <montantcautionnable>10</montantcautionnable>
          <montantTotalAPayer>5539</montantTotalAPayer>
          <montantAI2>0</montantAI2>
          <montantnonpercu>0</montantnonpercu>
        </LiquidationGen>
      </Notification>
    </ReponseDatas>
  </ReponseDeclaration>
</Message>
";

			var baseMessageDataWithoutMontant = @"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>
<Message>
  <EnveloppeMessage>
    <schemaID>MessageReponseCDecImp</schemaID>
    <schemaVersion>18122012</schemaVersion>
    <partyId>40218856900048</partyId>
    <transactionId>AAAAAAAAA+BBB+0000000001</transactionId>
    <numseq>0</numseq>
  </EnveloppeMessage>
  <ReponseDeclaration>
    <Entete>
      <refdec>1901204207</refdec>
      <refdos>9000-B00177613</refdos>
      <dateValidationDec>13/02/2019</dateValidationDec>
    </Entete>
    <ReponseDatas>
      <Notification>
        <Etat>
          <etat>BAE</etat>
          <etatDate>13/02/2019</etatDate>
          <etatHeure>04:04</etatHeure>
          <etatprec>VALIDE</etatprec>
          <etatprecDate>13/02/2019</etatprecDate>
          <etatprecHeure>04:04</etatprecHeure>
          <evenement>Expiration du timer</evenement>
        </Etat>
      </Notification>
    </ReponseDatas>
  </ReponseDeclaration>
</Message>
";

			var interchangePK1 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "00001.ANT.084521", "RCV");
			var interchangePK2 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "00002.VAL.084521", "RCV");
			var interchangePK3 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "00003.VAL.084521", "RCV");
			var interchangePK4 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 4, 1), "~BP", "XXX", "DEC", "00004.EEE.084521", "RCV");
			var interchangePK5 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "00005.BAE COMPLETE.084521", "RCV");
			var interchangePK6 = TestDataCreator.CreateEDIInterchange(branchPK, "FRC", new DateTime(2022, 3, 1), "~BP", "XXX", "DEC", "00006.BAE COMPLETE.084521", "RCV");

			var sql = $@"
INSERT INTO JobDeclaration
    (JE_PK, JE_DeclarationReference, JE_DataModel, JE_ApplicationCode, JE_EntryStatus, JE_MessageType, JE_GC, JE_GB, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES
    ('{declarationPK1}', 'B00001', 'FR', 'DG', '060', 'EXP', '{companyPK}', '{branchPK}', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
    ('{declarationPK2}', 'B00002', 'FR', 'DG', '040', 'EXP', '{companyPK}', '{branchPK}', 3, getutcdate(), '~BP', getutcdate(), '~BP'),
    ('{declarationPK3}', 'B00003', 'FR', 'DG', '060', 'EXP', '{companyPK}', '{branchPK}', 4, getutcdate(), '~BP', getutcdate(), '~BP'),
    ('{declarationPK4}', 'B00004', 'FR', 'DG', '060', 'EXP', '{companyPK}', '{branchPK}', 5, getutcdate(), '~BP', getutcdate(), '~BP'),
    ('{declarationPK5}', 'B00005', 'FR', 'DG', '150', 'EXP', '{companyPK}', '{branchPK}', 6, getutcdate(), '~BP', getutcdate(), '~BP'),
    ('{declarationPK6}', 'B00006', 'FR', 'DG', '060', 'EXP', '{companyPK}', '{branchPK}', 7, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO GenAddOnColumn
	(XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser)
VALUES
	(NEWID(), 'JE_DeltaMode', 'STR', 'G1', 'JE', '{declarationPK1}', GETDATE(), 'E', GETDATE(), 'E'),
	(NEWID(), 'JE_DeltaMode', 'STR', 'G1', 'JE', '{declarationPK2}', GETDATE(), 'E', GETDATE(), 'E'),
	(NEWID(), 'JE_DeltaMode', 'STR', 'G1', 'JE', '{declarationPK3}', GETDATE(), 'E', GETDATE(), 'E'),
	(NEWID(), 'JE_DeltaMode', 'STR', 'G1', 'JE', '{declarationPK4}', GETDATE(), 'E', GETDATE(), 'E'),
	(NEWID(), 'JE_DeltaMode', 'STR', 'G2', 'JE', '{declarationPK5}', GETDATE(), 'E', GETDATE(), 'E'),
	(NEWID(), 'JE_DeltaMode', 'STR', 'G2', 'JE', '{declarationPK6}', GETDATE(), 'E', GETDATE(), 'E');

INSERT INTO CusEntryHeader
	(CH_PK, CH_DataModel, CH_EntryStatus, CH_IsValid, CH_JE, CH_EntryReleaseDate, CH_MessageType, CH_Status, CH_ExitedStatus, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES
	('{cusEntryHeaderPK1}', 'FR', '060', 1, '{declarationPK1}', CAST(N'2022-02-01T00:00:00' AS SmallDateTime), 'DG', 'OK', '', 2, CAST(N'2022-01-01T00:00:00' AS SmallDateTime), '~BP', getutcdate(), '~BP'),
	('{cusEntryHeaderPK2}', 'FR', '040', 1, '{declarationPK2}', CAST(N'2022-02-01T00:00:00' AS SmallDateTime), 'DG', 'OK', '', 3, CAST(N'2022-01-01T00:00:00' AS SmallDateTime), '~BP', getutcdate(), '~BP'),
	('{cusEntryHeaderPK3}', 'FR', '060', 1, '{declarationPK3}', CAST(N'2022-02-01T00:00:00' AS SmallDateTime), 'DG', 'OK', '', 4, CAST(N'2022-01-01T00:00:00' AS SmallDateTime), '~BP', getutcdate(), '~BP'),
	('{cusEntryHeaderPK4}', 'FR', '060', 1, '{declarationPK4}', CAST(N'2022-02-01T00:00:00' AS SmallDateTime), 'DG', 'OK', '', 5, CAST(N'2023-12-01T00:00:00' AS SmallDateTime), '~BP', getutcdate(), '~BP'),
	('{cusEntryHeaderPK5}', 'FR', '150', 1, '{declarationPK5}', CAST(N'2022-02-01T00:00:00' AS SmallDateTime), 'DG', 'OK', '', 6, CAST(N'2022-01-01T00:00:00' AS SmallDateTime), '~BP', getutcdate(), '~BP'),
	('{cusEntryHeaderPK6}', 'FR', '060', 1, '{declarationPK6}', CAST(N'2022-02-01T00:00:00' AS SmallDateTime), 'DG', 'OK', '', 7, CAST(N'2022-01-01T00:00:00' AS SmallDateTime), '~BP', getutcdate(), '~BP');

INSERT INTO EDIMessage
	(EM_PK, EM_EI, EM_ApplicationCode, EM_MessageType, EM_MessageSubType, EM_Status, EM_ReceiveTransmit, EM_LinkTable, EM_LinkUniqueID, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_MessageData, EM_MessageNum, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
VALUES
	(NEWID(), '{interchangePK1}', 'FRC', 'IMC', 'IMC', 'PRS', 'RCV', 'CusEntryHeader', '{cusEntryHeaderPK1}', '{branchPK}', '{departmentPK}', CAST(N'2022-03-01T00:00:00' AS SmallDateTime), dbo.CLRCompressStringAsBytes('{baseMessageDataWithMontant}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK2}', 'FRC', 'IMC', 'IMC', 'PRS', 'RCV', 'CusEntryHeader', '{cusEntryHeaderPK2}', '{branchPK}', '{departmentPK}', CAST(N'2022-03-01T00:00:00' AS SmallDateTime), dbo.CLRCompressStringAsBytes('{baseMessageDataWithMontant}'), '1', '~BP', getutcdate(), '~BP'),	
	(NEWID(), '{interchangePK3}', 'FRC', 'IMC', 'IMC', 'PRS', 'RCV', 'CusEntryHeader', '{cusEntryHeaderPK3}', '{branchPK}', '{departmentPK}', CAST(N'2022-03-01T00:00:00' AS SmallDateTime), dbo.CLRCompressStringAsBytes('{baseMessageDataWithoutMontant}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK4}', 'FRC', 'IMC', 'IMC', 'PRS', 'RCV', 'CusEntryHeader', '{cusEntryHeaderPK4}', '{branchPK}', '{departmentPK}', CAST(N'2022-03-01T00:00:00' AS SmallDateTime), dbo.CLRCompressStringAsBytes('{baseMessageDataWithMontant}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK5}', 'FRC', 'IMC', 'IMC', 'PRS', 'RCV', 'CusEntryHeader', '{cusEntryHeaderPK5}', '{branchPK}', '{departmentPK}', CAST(N'2022-03-01T00:00:00' AS SmallDateTime), dbo.CLRCompressStringAsBytes('{baseMessageDataWithMontant}'), '1', '~BP', getutcdate(), '~BP'),
	(NEWID(), '{interchangePK6}', 'FRC', 'IMC', 'IMC', 'PRS', 'RCV', 'CusEntryHeader', '{cusEntryHeaderPK6}', '{branchPK}', '{departmentPK}', CAST(N'2022-03-01T00:00:00' AS SmallDateTime), dbo.CLRCompressStringAsBytes('{baseMessageDataWithMontant}'), '1', '~BP', getutcdate(), '~BP');";

			var cmd = Db.Connection.Command(sql);
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
			cmd.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
			cmd.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
			cmd.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
			cmd.AddParameter("@declarationPK3", SqlDbType.UniqueIdentifier, declarationPK3);
			cmd.AddParameter("@declarationPK4", SqlDbType.UniqueIdentifier, declarationPK4);
			cmd.AddParameter("@declarationPK5", SqlDbType.UniqueIdentifier, declarationPK5);
			cmd.AddParameter("@declarationPK6", SqlDbType.UniqueIdentifier, declarationPK6);
			cmd.AddParameter("@cusEntryHeaderPK1", SqlDbType.UniqueIdentifier, cusEntryHeaderPK1);
			cmd.AddParameter("@cusEntryHeaderPK2", SqlDbType.UniqueIdentifier, cusEntryHeaderPK2);
			cmd.AddParameter("@cusEntryHeaderPK3", SqlDbType.UniqueIdentifier, cusEntryHeaderPK3);
			cmd.AddParameter("@cusEntryHeaderPK4", SqlDbType.UniqueIdentifier, cusEntryHeaderPK4);
			cmd.AddParameter("@cusEntryHeaderPK5", SqlDbType.UniqueIdentifier, cusEntryHeaderPK5);
			cmd.AddParameter("@cusEntryHeaderPK6", SqlDbType.UniqueIdentifier, cusEntryHeaderPK6);

			cmd.ExecuteNonQuery();
		}

		Guid cusEntryHeaderPK1;
		Guid cusEntryHeaderPK2;
		Guid cusEntryHeaderPK3;
		Guid cusEntryHeaderPK4;
		Guid cusEntryHeaderPK5;
		Guid cusEntryHeaderPK6;
		Guid declarationPK1;
		Guid declarationPK2;
		Guid declarationPK3;
		Guid declarationPK4;
		Guid declarationPK5;
		Guid declarationPK6;
	}
}
