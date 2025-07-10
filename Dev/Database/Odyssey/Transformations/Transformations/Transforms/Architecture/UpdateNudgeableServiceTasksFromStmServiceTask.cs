using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture;

public class UpdateNudgeableServiceTasksFromStmServiceTask : DataTransformation
{
	public override string UserDescription => "Update Nudgeable Service Task having a schedule time less than 15 minutes to 15 minutes from dbo.StmScheduleTask table.";

	protected override void OfflinePostUpgradeTransform()
	{
		var sqlCmd = @"
UPDATE dbo.StmServiceTask
SET 
 SST_Configuration.modify('replace value of (/ScheduleConfig/NextRunTimeCalculatorMinutes/@Period)[1] with xs:positiveInteger(15)'),
 SST_SystemLastEditTimeUtc = GetUtcDate(),
 SST_SystemLastEditUser = '~BP'
WHERE 
 SST_ServiceTaskCode IN ('CMP', 'EJO', 'EPT', 'EKR', 'MXP', 'PLS', 'AMM', 'GNE', 'EHO', 'KMQ', 'UYR', 'TWS', 'XMS', 'MCM', 'XTC', 'ILP', 'ZU4', 'ELV', 'HRV', 'API', 'MXR', 'US2', 'POB', 'FDD', 'FRI', 'HPQ', 'IES', 'ABI', 'ADE', 'ETW', 'GCI', 'EMC', 'LWK', 'AUA', 'CGN', 'ECL', 'NCS', 'BRS', 'DXP', 'CNS', 'CAS', 'PDD', 'DSP', 'DIS', 'ACS', 'HLP', 'REN', 'CCS', 'IEE', 'WPI', 'CWM', 'MXS', 'ARP', 'HKS', 'TXO', 'NCR', 'USS', 'DMI', 'BAV', 'US3', 'ZRP', 'CNR', 'EMS', 'DMS', 'NDS', 'ERQ', 'EMP', 'UAI', 'UEM', 'ECN', 'DOD', 'AVS', 'GEP', 'CAM', 'DXS', 'DEV', 'ITS', 'ECR', 'SRB', 'UMI', 'GGR', 'AUP', 'NCP', 'CHS', 'MCP', 'FRF', 'PCM', 'PRD', 'CHR', 'JCD', 'CRQ', 'TCH', 'EMX', 'ETR', 'CWS', 'TGP', 'USR', 'TRS', 'EPL', 'EBR', 'ARR', 'PJQ', 'ESR', 'ISF', 'VLD', 'R12', 'SPC', 'UAS', 'EPA', 'OMS', 'ERO', 'EMO', 'PLI', 'FRR', 'EAM', 'PHS', 'RET', 'MAP', 'UMK', 'PMS', 'PES', 'EFJ', 'ZCR', 'CNO', '#@2', 'CAE', 'ALC', 'EIN', 'ICS', 'CCV', 'AUI', 'XES', 'XTO', 'UMR', 'BRI', 'DET', 'ICR', 'DED', 'ARS', 'CIS', 'BER', 'UEI', 'DMP', 'ITP', 'EPH', 'CTR', 'USI', 'EHU', 'BEP', 'ECO', 'CDR', 'EMY', 'SYS', 'EDO', '#@1', 'ZCS', 'AUS', 'GCP', 'PLP', 'CDG', 'UMS', 'EDD', 'SCP', 'AUD', 'APW', 'EMR', 'CLP', 'NMI', 'AMI', 'BGC', 'DEI', 'IEM', 'IET', 'CPS', 'AMP', 'EUP', 'ORM', 'MUG', 'KRI', 'USP', 'CI2', 'UAM', 'ERS', 'RPN', 'WPN', 'BDD', 'ADS', 'ASC', 'GGS', 'UMP', 'EHG', 'DAP', 'CMR', 'DAS', 'NLS', 'CDS', 'UYP', 'SCR', 'KRO', 'IAN', 'HKP', 'CHP', 'SCS', 'AUZ', 'EDE', 'GVU', 'CCR', 'ZTR', 'ZCP', 'USE', 'DPM', 'EUY', 'EVN', 'DEE', 'ESS', 'EMU', 'DQP', 'UEO', 'RPR', 'EIT', 'TWC', 'SGI', 'NLR', 'US1', 'AMO', 'ESA', 'TRP', 'IER', 'TRR', 'EAR', 'ESM', 'BES', 'EWS', 'UYS', 'R11', 'ITR', 'AAP', 'BRP', 'NLP', 'WRQ', 'ZWP', 'LWM', 'CNP', 'TEL', 'APA', 'AUB', 'UCT', 'UXB', 'EIL', 'GVM', 'DYN')
 AND SST_Configuration.exist('/ScheduleConfig/NextRunTimeCalculatorMinutes') = 1
 AND CAST(SST_Configuration.value('(/ScheduleConfig/NextRunTimeCalculatorMinutes/@Period)[1]', 'int') AS INT) < 15
;

UPDATE dbo.StmServiceTask
SET 
 SST_Configuration.modify('replace value of (/ScheduleConfig/NextRunTimeCalculatorSeconds/@Period)[1] with xs:positiveInteger(15)'),
 SST_SystemLastEditTimeUtc = GetUtcDate(),
 SST_SystemLastEditUser = '~BP'
WHERE 
 SST_ServiceTaskCode IN ('CMP', 'EJO', 'EPT', 'EKR', 'MXP', 'PLS', 'AMM', 'GNE', 'EHO', 'KMQ', 'UYR', 'TWS', 'XMS', 'MCM', 'XTC', 'ILP', 'ZU4', 'ELV', 'HRV', 'API', 'MXR', 'US2', 'POB', 'FDD', 'FRI', 'HPQ', 'IES', 'ABI', 'ADE', 'ETW', 'GCI', 'EMC', 'LWK', 'AUA', 'CGN', 'ECL', 'NCS', 'BRS', 'DXP', 'CNS', 'CAS', 'PDD', 'DSP', 'DIS', 'ACS', 'HLP', 'REN', 'CCS', 'IEE', 'WPI', 'CWM', 'MXS', 'ARP', 'HKS', 'TXO', 'NCR', 'USS', 'DMI', 'BAV', 'US3', 'ZRP', 'CNR', 'EMS', 'DMS', 'NDS', 'ERQ', 'EMP', 'UAI', 'UEM', 'ECN', 'DOD', 'AVS', 'GEP', 'CAM', 'DXS', 'DEV', 'ITS', 'ECR', 'SRB', 'UMI', 'GGR', 'AUP', 'NCP', 'CHS', 'MCP', 'FRF', 'PCM', 'PRD', 'CHR', 'JCD', 'CRQ', 'TCH', 'EMX', 'ETR', 'CWS', 'TGP', 'USR', 'TRS', 'EPL', 'EBR', 'ARR', 'PJQ', 'ESR', 'ISF', 'VLD', 'R12', 'SPC', 'UAS', 'EPA', 'OMS', 'ERO', 'EMO', 'PLI', 'FRR', 'EAM', 'PHS', 'RET', 'MAP', 'UMK', 'PMS', 'PES', 'EFJ', 'ZCR', 'CNO', '#@2', 'CAE', 'ALC', 'EIN', 'ICS', 'CCV', 'AUI', 'XES', 'XTO', 'UMR', 'BRI', 'DET', 'ICR', 'DED', 'ARS', 'CIS', 'BER', 'UEI', 'DMP', 'ITP', 'EPH', 'CTR', 'USI', 'EHU', 'BEP', 'ECO', 'CDR', 'EMY', 'SYS', 'EDO', '#@1', 'ZCS', 'AUS', 'GCP', 'PLP', 'CDG', 'UMS', 'EDD', 'SCP', 'AUD', 'APW', 'EMR', 'CLP', 'NMI', 'AMI', 'BGC', 'DEI', 'IEM', 'IET', 'CPS', 'AMP', 'EUP', 'ORM', 'MUG', 'KRI', 'USP', 'CI2', 'UAM', 'ERS', 'RPN', 'WPN', 'BDD', 'ADS', 'ASC', 'GGS', 'UMP', 'EHG', 'DAP', 'CMR', 'DAS', 'NLS', 'CDS', 'UYP', 'SCR', 'KRO', 'IAN', 'HKP', 'CHP', 'SCS', 'AUZ', 'EDE', 'GVU', 'CCR', 'ZTR', 'ZCP', 'USE', 'DPM', 'EUY', 'EVN', 'DEE', 'ESS', 'EMU', 'DQP', 'UEO', 'RPR', 'EIT', 'TWC', 'SGI', 'NLR', 'US1', 'AMO', 'ESA', 'TRP', 'IER', 'TRR', 'EAR', 'ESM', 'BES', 'EWS', 'UYS', 'R11', 'ITR', 'AAP', 'BRP', 'NLP', 'WRQ', 'ZWP', 'LWM', 'CNP', 'TEL', 'APA', 'AUB', 'UCT', 'UXB', 'EIL', 'GVM', 'DYN')
 AND SST_Configuration.exist('/ScheduleConfig/NextRunTimeCalculatorSeconds') = 1
 AND CAST(SST_Configuration.value('(/ScheduleConfig/NextRunTimeCalculatorSeconds/@Period)[1]', 'int') AS INT) < 900
;

UPDATE dbo.StmServiceTask
SET 
 SST_Configuration = CAST(REPLACE(CAST(SST_Configuration AS NVARCHAR(MAX)), 'NextRunTimeCalculatorSeconds', 'NextRunTimeCalculatorMinutes') AS XML),
 SST_SystemLastEditTimeUtc = GetUtcDate(),
 SST_SystemLastEditUser = '~BP'
WHERE 
 SST_ServiceTaskCode IN ('CMP', 'EJO', 'EPT', 'EKR', 'MXP', 'PLS', 'AMM', 'GNE', 'EHO', 'KMQ', 'UYR', 'TWS', 'XMS', 'MCM', 'XTC', 'ILP', 'ZU4', 'ELV', 'HRV', 'API', 'MXR', 'US2', 'POB', 'FDD', 'FRI', 'HPQ', 'IES', 'ABI', 'ADE', 'ETW', 'GCI', 'EMC', 'LWK', 'AUA', 'CGN', 'ECL', 'NCS', 'BRS', 'DXP', 'CNS', 'CAS', 'PDD', 'DSP', 'DIS', 'ACS', 'HLP', 'REN', 'CCS', 'IEE', 'WPI', 'CWM', 'MXS', 'ARP', 'HKS', 'TXO', 'NCR', 'USS', 'DMI', 'BAV', 'US3', 'ZRP', 'CNR', 'EMS', 'DMS', 'NDS', 'ERQ', 'EMP', 'UAI', 'UEM', 'ECN', 'DOD', 'AVS', 'GEP', 'CAM', 'DXS', 'DEV', 'ITS', 'ECR', 'SRB', 'UMI', 'GGR', 'AUP', 'NCP', 'CHS', 'MCP', 'FRF', 'PCM', 'PRD', 'CHR', 'JCD', 'CRQ', 'TCH', 'EMX', 'ETR', 'CWS', 'TGP', 'USR', 'TRS', 'EPL', 'EBR', 'ARR', 'PJQ', 'ESR', 'ISF', 'VLD', 'R12', 'SPC', 'UAS', 'EPA', 'OMS', 'ERO', 'EMO', 'PLI', 'FRR', 'EAM', 'PHS', 'RET', 'MAP', 'UMK', 'PMS', 'PES', 'EFJ', 'ZCR', 'CNO', '#@2', 'CAE', 'ALC', 'EIN', 'ICS', 'CCV', 'AUI', 'XES', 'XTO', 'UMR', 'BRI', 'DET', 'ICR', 'DED', 'ARS', 'CIS', 'BER', 'UEI', 'DMP', 'ITP', 'EPH', 'CTR', 'USI', 'EHU', 'BEP', 'ECO', 'CDR', 'EMY', 'SYS', 'EDO', '#@1', 'ZCS', 'AUS', 'GCP', 'PLP', 'CDG', 'UMS', 'EDD', 'SCP', 'AUD', 'APW', 'EMR', 'CLP', 'NMI', 'AMI', 'BGC', 'DEI', 'IEM', 'IET', 'CPS', 'AMP', 'EUP', 'ORM', 'MUG', 'KRI', 'USP', 'CI2', 'UAM', 'ERS', 'RPN', 'WPN', 'BDD', 'ADS', 'ASC', 'GGS', 'UMP', 'EHG', 'DAP', 'CMR', 'DAS', 'NLS', 'CDS', 'UYP', 'SCR', 'KRO', 'IAN', 'HKP', 'CHP', 'SCS', 'AUZ', 'EDE', 'GVU', 'CCR', 'ZTR', 'ZCP', 'USE', 'DPM', 'EUY', 'EVN', 'DEE', 'ESS', 'EMU', 'DQP', 'UEO', 'RPR', 'EIT', 'TWC', 'SGI', 'NLR', 'US1', 'AMO', 'ESA', 'TRP', 'IER', 'TRR', 'EAR', 'ESM', 'BES', 'EWS', 'UYS', 'R11', 'ITR', 'AAP', 'BRP', 'NLP', 'WRQ', 'ZWP', 'LWM', 'CNP', 'TEL', 'APA', 'AUB', 'UCT', 'UXB', 'EIL', 'GVM', 'DYN')
 AND SST_Configuration.exist('/ScheduleConfig/NextRunTimeCalculatorSeconds') = 1
 AND CAST(SST_Configuration.value('(/ScheduleConfig/NextRunTimeCalculatorSeconds/@Period)[1]', 'int') AS INT) = 15
;";
		Db.Connection.ExecuteNonQuery(sqlCmd);
	}
}
