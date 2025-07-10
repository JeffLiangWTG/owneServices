using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	/// <summary>
	/// These GUIDs are the PK of the StmMenuItem row that corresponds to a clickable item on the Documents menu. It allows us to simulate the user clicking this exact document. 
	/// Add your item/doc/template/pivot to the Stm* tables using the Documents>Customise menu of your bizO's form, save for shelf, then look for the NEW SU_PK that's added to documents.xml 
	/// </summary>
	public static class PrinterFromEdiMessageHelper_MenuKeys
	{
		public static class Chief
		{
			/// <summary>
			/// Airline delivery schedule. Printed when response to EAC/close request for master is approved by chief
			/// </summary>
			public static ZGuid AirlineDeliverySchedule { get { return new ZGuid("CFBB417E-F01A-44EF-AD9D-61FCCE2A19C9"); } }
		}

		public static class Ccsuk
		{
			/// <summary>
			/// Print advising that action was not yet completed. Negative, warning. Shed receives FSN for TAWB with CAC=CA
			/// </summary>
			public static ZGuid G2_AdviceOfCustomsAction { get { return new ZGuid("3B039D21-A657-4E2D-B181-B31276D78930"); } }

			/// <summary>
			/// Advice that removal request was approved. Positive, confirmation. Removal request receives CUSRES with CAC=CT or CW and NPR is set.
			/// </summary>
			public static ZGuid C1_AgentsTravellingCopyRemovalAuthority { get { return new ZGuid("340f22bd-a16b-4cda-a34e-6224d4403e0c"); } }
			public static ZGuid C1_AgentsTravellingCopyRemovalAuthorityREPRINT { get { return new ZGuid("4d125e8b-314e-4a54-8c48-f04c8fb2eeff"); } }

			/// <summary>
			/// Advice that further action is needed after removal was requested. Negative, warning. Removal request not immediately cleared (i.e. CAC=CA)
			/// </summary>
			public static ZGuid GR_AdviceOfSelectedRemovalRequest { get { return new ZGuid("2DC93876-EAC6-4D7A-957E-52661BD5AB3B"); } }

			/// <summary>
			/// Positive, confirmation. Fallback request acknowledged.
			/// </summary>
			public static ZGuid F2_FallbackEntryAcceptanceOutput { get { return new ZGuid("efec826e-9844-4edc-a3ef-90f4ac544701"); } }

			/// <summary>
			/// Positive, confirmation. Clearance granted and notified via FSN.
			/// </summary>
			public static ZGuid CC_CustomsClearance { get { return new ZGuid("d305a838-cf89-4eb9-92b6-a88ec4cb075d"); } }

			/// <summary>
			/// Printed by shed when transferring to another party upoon receipt of FSN/CU (tawb released)
			/// </summary>
			public static ZGuid TFM_TransferFreightManifest { get { return new ZGuid("09732DA5-7200-4847-B936-C38CE20148C3"); } }

			public static ZGuid RRA_ReleaseRemovalAuthority { get { return new ZGuid("83900439-CC43-4C6F-BAFF-4FDA4FDF87E7"); } }
			public static ZGuid RRA_ReleaseRemovalAuthorityReprint { get { return new ZGuid("8DA77E22-A7E5-4AFA-A35B-5F79CF7423A8"); } }

			public static ZGuid P5_InterAirportRemoval { get { return new ZGuid("66de3d20-a1a5-45d1-9b42-677e8249780b"); } }
		}

		public static class MCP
		{
			public static ZGuid PHS11 { get { return new ZGuid("19005a08-c3a1-478f-9b30-1843741cc50a"); } }
		}
	}
}
