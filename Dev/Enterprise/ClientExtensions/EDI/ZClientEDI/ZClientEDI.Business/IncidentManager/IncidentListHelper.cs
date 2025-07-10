using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentListHelper
	{
		public IncidentListHelper(AutoIncidentMain incident)
		{
			this.Incident = incident;
		}

		#region Group Dependent Staff List

		public GlbStaffCollection ActiveGroupDependentStaffList
		{
			get
			{
				if (fActiveGroupDependentStaffList == null)
				{
					ZQuery filter = new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True);
					fActiveGroupDependentStaffList = new GlbStaffCollection(Factory, filter);
				}

				return fActiveGroupDependentStaffList;
			}
		}

		public GlbStaffCollection GroupDependentStaffList
		{
			get
			{
				if (fGroupDependentStaffList == null)
				{
					fGroupDependentStaffList = new GlbStaffCollection(Factory);
				}

				return fGroupDependentStaffList;
			}
		}

		GlbStaffCollection fActiveGroupDependentStaffList;
		GlbStaffCollection fGroupDependentStaffList;

		#endregion

		#region Organisation Contact List

		public OrgContactDependentCollection OrganisationContactList
		{
			get
			{
				if (fOrganisationContactList == null || CurrentOrganisationPK != ClientPK)
				{
					CurrentOrganisationPK = ClientPK;
					fOrganisationContactList = GetNewOrganisationContactList();
				}

				return fOrganisationContactList;
			}
		}

		ZGuid CurrentOrganisationPK;
		OrgContactDependentCollection fOrganisationContactList;

		protected virtual OrgContactDependentCollection GetNewOrganisationContactList()
		{
			return (Client == null) ? new OrgContactDependentCollection(Factory) : new OrgContactDependentCollection(Client, Factory);
		}

		#endregion

		#region Module Lists

		#region Enterprise

		public CodeDescriptionPairList ProductList
		{
			get
			{
				if (fProductList == null)
				{
					fProductList = new EnterpriseModuleList();
				}

				return fProductList;
			}
		}

		CodeDescriptionPairList fProductList;

		#endregion

		#region Deliverance

		#region Deliverance Module List

		/// <summary>
		/// This is the OLD list which is only used in the OLD deliverance support incident module.
		/// Will be removed when we remove the old incident system.
		/// </summary>

		public CodeDescriptionPairList DeliveranceModuleList
		{
			get
			{
				if (fDeliveranceModuleList == null)
				{
					fDeliveranceModuleList = new CodeDescriptionPairList();

					fDeliveranceModuleList.AddPair("DAR", "Deliv Acc Receivable");
					fDeliveranceModuleList.AddPair("DAP", "Deliv Acc Payable");
					fDeliveranceModuleList.AddPair("DCB", "Deliv Cashbook");
					fDeliveranceModuleList.AddPair("DGL", "Deliv Gen Ledger");
					fDeliveranceModuleList.AddPair("DJC", "Deliv Job Costing");
					fDeliveranceModuleList.AddPair("DIF", "Deliv Import Freight");
					fDeliveranceModuleList.AddPair("DEF", "Deliv Export Freight");
					fDeliveranceModuleList.AddPair("DOR", "Deliv Order Tracking");
					fDeliveranceModuleList.AddPair("DLC", "Deliv Landed Costing");
					fDeliveranceModuleList.AddPair("DCU", "Deliv Customs/Tariff");
					fDeliveranceModuleList.AddPair("DTR", "Deliv Transport");
					fDeliveranceModuleList.AddPair("DNZ", "Deliv ediBroker");
					fDeliveranceModuleList.AddPair("DBW", "Deliv Bonded Warehouse");
					fDeliveranceModuleList.AddPair("DWH", "Deliv Warehousing");
					fDeliveranceModuleList.AddPair("DED", "Deliv Edifice");
					fDeliveranceModuleList.AddPair("DAC", "Deliv Air Cargo Autom.");
					fDeliveranceModuleList.AddPair("DSC", "Deliv Sea Cargo Autom.");
					fDeliveranceModuleList.AddPair("DEX", "Deliv ECN - Exit");
					fDeliveranceModuleList.AddPair("DSE", "Deliv System Error");
					fDeliveranceModuleList.AddPair("DMI", "Deliv Miscellaneous");
					fDeliveranceModuleList.AddPair("DEM", "Deliv Email");
					fDeliveranceModuleList.AddPair("DFX", "Deliv Faxing/EFax");
					fDeliveranceModuleList.AddPair("DMN", "Deliv Maintanence");
					fDeliveranceModuleList.AddPair("DRA", "Deliv Autorating");
					fDeliveranceModuleList.AddPair("DCO", "Deliv Comms Server");
					fDeliveranceModuleList.AddPair("DND", "Deliv New Docs");
					fDeliveranceModuleList.AddPair("DEO", "Deliv EOM");
					fDeliveranceModuleList.AddPair("DET", "Deliv EagleTrac");
					fDeliveranceModuleList.AddPair("DML", "Deliv MailLink");
				}

				return fDeliveranceModuleList;
			}
		}

		CodeDescriptionPairList fDeliveranceModuleList;

		#endregion

		#endregion

		#endregion

		#region Release Build List

		public ReleaseBuildCollection ReleaseBuildList
		{
			get
			{
				if (fReleaseBuildList == null)
				{
					fReleaseBuildList = new ReleaseBuildCollection(Factory);
				}

				return fReleaseBuildList;
			}
		}

		ReleaseBuildCollection fReleaseBuildList;

		#endregion

		#region Staff Dependent Group List

		public GlbGroupCollection ActiveStaffDependentGroupList
		{
			get
			{
				if (fActiveStaffDependentGroupList == null)
				{
					ZQuery filter = new ZQuery(GlbGroupSchema.GG_IsActive, ZBool.True);
					fActiveStaffDependentGroupList = new GlbGroupCollection(Factory, filter);
				}

				return fActiveStaffDependentGroupList;
			}
		}

		public GlbGroupCollection StaffDependentGroupList
		{
			get
			{
				if (fStaffDependentGroupList == null)
				{
					fStaffDependentGroupList = new GlbGroupCollection(Factory);
				}

				return fStaffDependentGroupList;
			}
		}

		GlbGroupCollection fActiveStaffDependentGroupList;
		GlbGroupCollection fStaffDependentGroupList;

		#endregion

		#region Staff List

		public GlbStaffCollection StaffList
		{
			get
			{
				if (fStaffList == null)
				{
					fStaffList = new GlbStaffCollection(Factory);
				}

				return fStaffList;
			}
		}

		GlbStaffCollection fStaffList;

		#endregion

		#region Error Log (Issue) List

		public HelpErrorLogCollection ErrorLogList
		{
			get
			{
				if (fErrorLogList == null)
				{
					fErrorLogList = new HelpErrorLogCollection(Factory);
				}

				return fErrorLogList;
			}
		}

		HelpErrorLogCollection fErrorLogList;

		#endregion

		#region Program Area List

		public CodeDescriptionPairList ProgramAreaList
		{
			get
			{
				if (fProgramAreaList == null)
				{
					fProgramAreaList = IncidentConstants.GetProgramAreaList();
				}

				return fProgramAreaList;
			}
		}

		CodeDescriptionPairList fProgramAreaList;

		#endregion

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return Incident != null ? Incident.Factory : null; }
		}

		readonly AutoIncidentMain Incident;

		OrgHeader Client
		{
			get { return Incident != null ? Incident.Client : null; }
		}

		ZGuid ClientPK
		{
			get { return Client != null ? Client.PK : ZGuid.Empty; }
		}

		#endregion
	}
}

