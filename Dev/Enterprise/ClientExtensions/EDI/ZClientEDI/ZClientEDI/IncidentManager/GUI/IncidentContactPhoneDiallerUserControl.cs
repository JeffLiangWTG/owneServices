using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentContactPhoneDiallerUserControl : ContactPhoneDiallerUserControl
	{
		public IncidentContactPhoneDiallerUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			InitializeIncident();
		}

		#region Incident

		IncidentMainBase incident;

		void InitializeIncident()
		{
			var form = FindForm() as ZForm;
			if (form != null)
			{
				incident = form.BusinessEntity as IncidentMainBase;
				if (incident != null)
				{
					incident.IM_OA_BranchAddressInfo.ValueChanged += Incident_BranchAddressInfo_ValueChanged;
				}
			}
		}

		void Incident_BranchAddressInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshControls();
		}

		#endregion

		#region DialInfo

		protected override ContactPhoneDialInfoBuilder GetNewDialInfoBuilder()
		{
			return new IncidentPhoneDialInfoBuilder(incident);
		}

		class IncidentPhoneDialInfoBuilder : ContactPhoneDialInfoBuilder
		{
			public IncidentPhoneDialInfoBuilder(IncidentMainBase incident)
			{
				this.incident = incident;
			}

			readonly IncidentMainBase incident;

			protected override PhoneDialInfo GetOfficePhoneDialInfo(OrgContact contact, OrgHeader org)
			{
				return GetContactBranchPhoneDialInfo(contact) ?? GetIncidentBranchPhoneDialInfo() ?? GetContactOrgPhoneDialInfo(org);
			}

			PhoneDialInfo GetContactBranchPhoneDialInfo(OrgContact contact)
			{
				if (contact != null)
				{
					var contactBranch = contact.BranchAddress;
					if (contactBranch != null)
					{
						var contactBranchPhone = contactBranch.OA_Phone;
						if (!contactBranchPhone.IsEmpty)
						{
							return new PhoneDialInfo(contactBranchPhone.ToString(), OfficeDescription);
						}
					}
				}

				return null;
			}

			PhoneDialInfo GetIncidentBranchPhoneDialInfo()
			{
				var incidentBranch = incident != null ? incident.BranchAddress : null;
				if (incidentBranch != null)
				{
					var incidentBranchPhone = incidentBranch.OA_Phone;
					if (!incidentBranchPhone.IsEmpty)
					{
						return new PhoneDialInfo(incidentBranchPhone.ToString(), OfficeDescription);
					}
				}

				return null;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (incident != null)
			{
				incident.IM_OA_BranchAddressInfo.ValueChanged -= Incident_BranchAddressInfo_ValueChanged;
			}

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
