using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Customs
{
	[TestedType(typeof(IncompleteEntry))]
	sealed class IncompleteEntryTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestSQLHasNotChanged()
		{
			var expectedText = @"--DROP FUNCTION IncompleteEntry
CREATE FUNCTION [dbo].[IncompleteEntry]
(
	@CurrentCompany UNIQUEIDENTIFIER,
	@TransportMode VARCHAR(3),
	@VoyageFlightDate DATETIME,
	@Agent UNIQUEIDENTIFIER,
	@ImporterABN VARCHAR(15),
	@DeclarationType VARCHAR(3),
	@PortOfArrival VARCHAR(5),
	@Supplier UNIQUEIDENTIFIER,
	@EntrySubmissionDateFrom SMALLDATETIME,
	@EntrySubmissionDateTo SMALLDATETIME,
	@JobCreatedDateFrom SMALLDATETIME,
	@JobCreatedDateTo SMALLDATETIME
)

RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
(
	SELECT
		Declaration.TransportMode AS TransportMode,
		Branch.BranchShortName AS Agent,
		Declaration.JobNumber AS JobNumber,
		Declaration.MessageSubType AS DeclarationType,
		Declaration.ExportDate AS VoyageFlightDate,
		Declaration.EstimatedArrivalDate AS DischargeDate,
		CASE WHEN
			(Declaration.TransportMode = 'SEA' and Declaration.ContainerMode = 'FCL')
		THEN
			(CASE WHEN
				(DocsAndCartage.FCLAvailable IS NULL OR JobVoyDest.AvailabilityDate < DocsAndCartage.FCLAvailable)
			THEN
				JobVoyDest.AvailabilityDate
			ELSE
				DocsAndCartage.FCLAvailable
			END)
		ELSE 
			(CASE WHEN 
				(
					DocsAndCartage.LCLAvailable IS NULL
				OR 
					(SELECT TOP 1 DepotAvailabilityDate
					FROM Sailing.BAS__JobSailing
 					WHERE JobVoyDestinationKey = JobVoyDest.JobVoyDestinationKey) < DocsAndCartage.LCLAvailable
				)
			THEN 
				(SELECT TOP 1 DepotAvailabilityDate
				FROM Sailing.BAS__JobSailing 
				WHERE JobVoyDestinationKey = JobVoyDest.JobVoyDestinationKey)
			ELSE
				DocsAndCartage.LCLAvailable
			END)
		END AS AvailableDate,
		CASE WHEN 
			(Declaration.TransportMode = 'SEA' and Declaration.ContainerMode = 'FCL')
		THEN
			(CASE WHEN
				(DocsAndCartage.FCLStorageCommences IS NULL OR JobVoyDest.StorageDate < DocsAndCartage.FCLStorageCommences)
			THEN
				JobVoyDest.StorageDate
			ELSE
				DocsAndCartage.FCLStorageCommences
			END)
		ELSE
			(CASE WHEN
				(
					DocsAndCartage.LCLStorageCommences IS NULL
				OR
					(SELECT TOP 1 DepotStorageDate
					FROM Sailing.BAS__JobSailing 
					WHERE JobVoyDestinationKey = JobVoyDest.JobVoyDestinationKey) < DocsAndCartage.LCLStorageCommences
				)
			THEN
				(SELECT TOP 1 DepotStorageDate
				FROM Sailing.BAS__JobSailing 
				WHERE JobVoyDestinationKey = JobVoyDest.JobVoyDestinationKey)
			ELSE
				DocsAndCartage.LCLStorageCommences
			END)
		END AS StorageDate,
		Declaration.PortOfArrival AS DischargePort,
		EntryNumber.EntryNum AS EntryNumber,
		Declaration.AgentsReference AS AgentReference,
		Declaration.OwnerReference AS OwnerReference,
		ImporterOrg.Code AS OwnerCode,
		Declaration.EntryStatus AS CustomsStatus,
		SupplierOrg.Code AS SupplierCode,
		EntryHeader.TotalPaid AS AmountPayable,
		OrgCusCode.CustomsRegNo AS ImporterABN,
		Declaration.PaymentMethod AS PaymentMethod, 
		CASE WHEN Declaration.EntrySubmittedDate IS NULL THEN 'N' ELSE 'Y' END AS IsEntrySubmitted
	FROM 
		Customs.BAS__Declaration Declaration
		LEFT JOIN Organization.BAS__Organization ImporterOrg  ON Declaration.ImporterKey = ImporterOrg.OrganizationKey
		LEFT JOIN Organization.BAS__OrgCusCode OrgCusCode ON ImporterOrg.OrganizationKey = OrgCusCode.OrganizationKey and OrgCusCode.CodeType = 'ABN'
		LEFT JOIN Organization.BAS__Organization SupplierOrg  ON Declaration.SupplierKey = SupplierOrg.OrganizationKey
		LEFT JOIN Customs.BAS__EntryHeader EntryHeader LEFT JOIN Customs.BAS__EntryNumber EntryNumber  ON EntryHeader.EntryHeaderKey = EntryNumber.EntryHeaderKey ON EntryHeader.DeclarationKey = Declaration.DeclarationKey
		LEFT JOIN Organization.BAS__Branch Branch ON Declaration.BranchKey = Branch.BranchKey
		LEFT JOIN Customs.BAS__DocumentAndCartage DocsAndCartage ON Declaration.DeclarationKey = DocsAndCartage.ParentDeclarationKey
		LEFT JOIN Sailing.BAS__JobVoyage Voyage 
				ON Declaration.Vessel = Voyage.Vessel
				AND Declaration.VoyageFlightNo = Voyage.VoyageFlight
				AND (Voyage.FlightDate =
					(SELECT TOP 1 FlightDate
					FROM Sailing.BAS__JobVoyage InnerVoyage 
					WHERE Voyage.VoyageFlight = InnerVoyage.VoyageFlight
					ORDER BY DATEDIFF(mm, Voyage.FlightDate, InnerVoyage.FlightDate)))
	LEFT JOIN Sailing.BAS__JobVoyDestination JobVoyDest ON JobVoyDest.JobVoyageKey = Voyage.JobVoyageKey AND JobVoyDest.PortOfDischarge = Declaration.PortOfArrival

	WHERE (ISNULL(Declaration.EntryStatus, '') <> 'FIN') 
	AND (Declaration.ApplicationCode = 'CMR')
	AND (Declaration.MessageType = 'IMP' OR Declaration.MessageType = 'EXW')
	AND (@CurrentCompany = Branch.CompanyID)
	AND (@TransportMode = '' OR Declaration.TransportMode = @TransportMode)
	AND (@Agent IS NULL OR Branch.BranchID = @Agent)
	AND (@ImporterABN = '' OR OrgCusCode.CustomsRegNo = @ImporterABN)
	AND (@DeclarationType = '' OR Declaration.MessageSubType = @DeclarationType)
	AND (@PortOfArrival IS NULL OR Declaration.PortOfArrival = @PortOfArrival)
	AND (@Supplier IS NULL OR Declaration.SupplierID = @Supplier)
	AND
	(
		((@EntrySubmissionDateFrom IS NULL OR CAST(Declaration.EntrySubmittedDate AS DATE) >= @EntrySubmissionDateFrom)
		AND
		(@EntrySubmissionDateTo IS NULL OR CAST(Declaration.EntrySubmittedDate AS DATE) <= @EntrySubmissionDateTo))
		OR
		(
			Declaration.EntrySubmittedDate IS NULL AND
			((@JobCreatedDateFrom IS NULL OR CAST(Declaration.SystemCreateTimeUtc AS DATE) >= @JobCreatedDateFrom)
			AND
			(@JobCreatedDateTo IS NULL OR CAST(Declaration.SystemCreateTimeUtc AS DATE) <= @JobCreatedDateTo))
		)
	)
)
";
			var sqlFunction = new IncompleteEntry();
			var actualText = sqlFunction.Text;
			AssertEquals("A version of this function exists in the Odyssey database(/CargoWise.DbUpgrader/src/Scripts/Scripts.Definitions/Customs/IncompleteEntry.sql), please update it.", expectedText, actualText);
		}

		public void TestFunctionalityOfTheFunction()
		{
			(Guid companyPK, _) = EDWTestDataCreator.CreateCompany("AU#", "AU", "AU");
			(_, long branchKey) = EDWTestDataCreator.CreateBranch(companyPK, "AU$", "AUMEL");

			var sql = $@"
DECLARE @DeclarationKey BIGINT
SELECT @DeclarationKey = ISNULL(MAX(DeclarationKey), 0)
FROM {Db.EdwDatabaseName}.Customs.BAS__Declaration

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__Declaration(DeclarationID, DeclarationKey, DataModel, JobNumber, BranchKey, CompanyID, ApplicationCode, MessageType)
values
(NEWID(), @DeclarationKey + 1, 'AU', 'BUS100TEST', '{branchKey}', '{companyPK}', 'CMR', 'IMP'),
(NEWID(), @DeclarationKey + 2, 'AU', 'BUS101TEST', '{branchKey}', '{companyPK}', 'BLT', 'EXW')
";

			using var command1 = Db.Connection.Command(sql);
			command1.ExecuteNonQuery();

			sql =
				$"select * FROM {Db.EdwDatabaseName}.dbo.IncompleteEntry('{companyPK}', '', NULL, NULL, '', '', NULL, NULL, NULL, NULL, NULL, NULL)";
			using var command2 = Db.Connection.Command(sql);
			using var reader = command2.ExecuteReader();

			AssertEquals(true, reader.Read());
			AssertEquals("BUS100TEST", reader["JobNumber"]);
			AssertEquals("N", reader["IsEntrySubmitted"]);
			AssertEquals(false, reader.Read());
		}
	}
}
