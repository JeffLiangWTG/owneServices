namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	/// <summary>
	/// Subscriber Codes such as DOS and DTG are already in use, please avoid using them.
	/// Use the query below to check for other in use codes:
	///		SELECT SubscriberCode, Description
	///		FROM ediProd_Audit.biadmin.SubscriberControl
	///		WHERE SubscriberCode LIKE 'D%' AND Description NOT LIKE 'Data Science%'
	/// </summary>
	static class SubscriberCodes
	{
		public const string AccChargeCode = "DAC";
		public const string AccGLHeaderCode = "DAG";
		public const string AccTransactionHeaderSubscriberCode = "DAT";
		public const string BMComponentSubscriberCode = "DBC";
		public const string BMNCNShape = "DBS";
		public const string ClientCompanySubscriberCode = "DCC";
		public const string ClientInvoiceDeliverySubscriberCode = "DL9";
		public const string ClientLicencePriceHeaderSubscriberCode = "DL6";
		public const string ClientLicencePriceItemSubscriberCode = "DL7";
		public const string EdiBilledUsageSubscriberCode = "DBU";
		public const string EdiPriceUsageMappingSubscriberCode = "DPU";
		public const string GenCustomAddOnValueSubscriberCode = "DGV";
		public const string GenPivotSubscriberCode = "DGP";
		public const string GlbBranchSubscriberCode = "DGB";
		public const string GlbCapabilitySubscriberCode = "DGC";
		public const string GlbEmploymentHistoryCode = "DGE";
		public const string GlbEmploymentTeamCode = "DGT";
		public const string GlbGroupLinkSubscriberCode = "DGK";
		public const string GlbGroupSubscriberCode = "DGG";
		public const string GlbHolidaySubscriberCode = "DGH";
		public const string GlbResourceCapabilityPivotSubscriberCode = "DGR";
		public const string GlbSecuritySubscriberCode = "DGU";
		public const string GlbStaffHolidaySubscriberCode = "DGO";
		public const string GlbStaffManagerSubscriberCode = "DSM";
		public const string GlbStaffSubscriberCode = "DGS";
		public const string GlbWorkTimeSubscriberCode = "DGW";
		public const string GlbWorkPatternSubscriberCode = "DW1";
		public const string HelpErrorLogSubscriberCode = "DHE";
		public const string HelpErrorLogKeySubscriberCode = "DHK";
		public const string HelpErrorLogOccurrenceSubscriberCode = "DHO";
		public const string IncidentDiagnosticCriteriaPivotSubscriberCode = "DDP";
		public const string IncidentDiagnosticCriteriaSubscriberCode = "DDC";
		public const string IncidentMainSubscriberCode = "DIM";
		public const string IncidentManagementGroupMessageSubscriberCode = "DGM";
		public const string IncidentManagementGroupSubscriberCode = "DIG";
		public const string IncidentManagementLinkSubscriberCode = "DIL";
		public const string IncidentMetricsSubscriberCode = "DIE";
		public const string IncidentTriageDiagnosticCriteriaPivotSubscriberCode = "DTP";
		public const string IncidentTriageSubscriberCode = "DIT";
		public const string IncidentRequestSubscriberCode = "DIR";
		public const string JobConversationMessageSubscriberCode = "DJM";
		public const string JobConversationParticipantSubscriberCode = "DJP";
		public const string JobConversationSubscriberCode = "DJC";
		public const string LicenceCompanySubscriberCode = "DLC";
		public const string LicenceDatabaseSubscriberCode = "DLD";
		public const string LicenceEnterpriseSubscriberCode = "DLE";
		public const string LicenceHeaderSubscriberCode = "DLA";
		public const string OrgContactSubscriberCode = "DOC";
		public const string OrgHeaderSubscriberCode = "DOH";
		public const string OrgOpportunitySubscriberCode = "DOO";
		public const string OrgStaffAssignmentsSubscriberCode = "DO8";
		public const string ProcessEstimateLogSubscriberCode = "DPE";
		public const string ProcessHeaderLinkSubscriberCode = "DPL";
		public const string ProcessHeaderSubscriberCode = "DPH";
		public const string ProcessTasksSubscriberCode = "DPT";
		public const string RefUNLOCOUtcOffsetSubscriberCode = "DRU";
		public const string StmDataSubscriberCode = "DSD";
		public const string StmNoteSubscriberCode = "DST";
		public const string TagDefinitionSubscriberCode = "DTD";
		public const string TagLinkSubscriberCode = "DTL";
		public const string TagMagnitudeSubscriberCode = "DTM";
		public const string WorkItemSubscriberCode = "DWI";
		public const string WorkProjectSubscriberCode = "DWP";
	}
}
