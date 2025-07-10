using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class AllocateToOrganisationForm : ZChildForm
	{
		public AllocateToOrganisationForm(ComPayRegisteredOrganisation comPayLine)
			: base(comPayLine)
		{
			this.comPayLine = comPayLine;
			comPayLine.SetReadOnlyIncludingChildren(true);
			Text = Res.GetString("ca6f598b-030b-4578-b6ec-347db119c025", "Allocate to Organization ({0})", comPayLine.ECN);
		}

		ZArchitecture.ZTextBox ClientNameTextBox;
		ZDateEdit RegistrationDateDateEdit;
		ZArchitecture.ZTextBox RelatedOrganisationsTextBox;
		ZArchitecture.ZTextBox FaxTextBox;
		ZArchitecture.ZTextBox PhoneTextBox;
		ZArchitecture.ZTextBox CountryTextBox;
		ZArchitecture.ZTextBox PostcodeTextBox;
		ZArchitecture.ZTextBox StateTextBox;
		ZArchitecture.ZTextBox SuburbTextBox;
		ZArchitecture.ZTextBox TerminalCodeTextBox;
		ZArchitecture.ZTextBox ABNTextBox;
		ZArchitecture.ZTextBox Address2TextBox;
		ZArchitecture.ZTextBox Address1TextBox;
		ZArchitecture.ZTextBox ECNTextBox;

		ComPayRegisteredOrganisation comPayLine;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		protected Business.PeriodManagement.NewYearPeriodSettings NewYearSettings;
		ZGroupBox AllocateGroupBox;
		ZButton AllocateButton;
		ZButton CloseButton;
		ZGuidFindBox OrgFindBox;
		readonly System.ComponentModel.Container components;

		#region System stuff
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
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

		#endregion

		void AllocateButton_Click(object sender, EventArgs e)
		{
			if (!comPayLine.CusCode.IsDeleted && comPayLine.CusCode.OK_OH.IsValid)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZString aUCode = Core.Constants.CountryCodes.Australia;
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_OH, comPayLine.CusCode.OK_OH);
				query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.eNettRegistrationNumber);
				query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_RN_NKCodeCountry, aUCode);
				OrgCusCode orgCusCodeInDatabase = factory.LoadTop1<OrgCusCode>(query);
				if (orgCusCodeInDatabase == null)
				{
					comPayLine.CusCode.OK_CustomsRegNo = comPayLine.ECN.ToString();
					comPayLine.CusCode.OK_RN_NKCodeCountry = aUCode;
					comPayLine.CusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
					comPayLine.CusCode.Factory.Save();
					comPayLine.OrgHeaderPKs.Add(comPayLine.CusCode.OK_OH);
					Globals.Message.Show(Res.GetString("490F5FFC-8A19-4f10-B9D2-F5BE3BEACE61", "Organization was successfully allocated"));
				}
				else
				{
					DialogResult result = Globals.Message.Show(Res.GetString("970245b0-c35d-4096-9e0f-dc5fbfc8b517",
@"Do you want to overwrite the existing ENE code already configured against this organization?
This organization code already has a ComPay registration saved against it."), "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);

					if (result == DialogResult.Yes)
					{
						orgCusCodeInDatabase.OK_CustomsRegNo = comPayLine.ECN.ToString();
						comPayLine.OrgHeaderPKs.Add(orgCusCodeInDatabase.OK_OH);
						factory.Save();
						Globals.Message.Show(Res.GetString("C9D64497-1E67-4c74-80F6-95E6D86BD934", "Allocation for organization was updated"));
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("efbec137-3060-47ed-b59b-d31ceb9c708f", "Please enter the organization"));
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

