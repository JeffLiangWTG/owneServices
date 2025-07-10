using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class OrganisationDetailsUserControl : ZUserControl
	{
		public OrganisationDetailsUserControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(DetailsGroupBox);
		}

		OrgImpAddInfo parent
		{
			get { return DataSource as OrgImpAddInfo; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (parent != null)
			{
				parent.ZO_IsGSTDirectPaymentInfo.ValueChanged -= ZO_IsGSTDirectPaymentInfo_ValueChanged;
				parent.ZO_IsImporterDirectPaymentInfo.ValueChanged -= ZO_IsImporterDirectPaymentInfo_ValueChanged;
				parent.ZO_IsLVSImporterDirectPaymentInfo.ValueChanged -= ZO_IsLVSImporterDirectPayment_ValueChanged;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (parent != null)
			{
				parent.ZO_IsGSTDirectPaymentInfo.ValueChanged += ZO_IsGSTDirectPaymentInfo_ValueChanged;
				parent.ZO_IsImporterDirectPaymentInfo.ValueChanged += ZO_IsImporterDirectPaymentInfo_ValueChanged;
				parent.ZO_IsLVSImporterDirectPaymentInfo.ValueChanged += ZO_IsLVSImporterDirectPayment_ValueChanged;
			}
		}

		void ZO_IsGSTDirectPaymentInfo_ValueChanged(object sender, EventArgs e)
		{
			if (parent.ZO_IsGSTDirectPayment)
			{
				Globals.Message.Show(Res.GetString("d9ac8bb1-4776-4290-8e89-1da225a99030", @"You have selected 'GST Direct' for this client.
Unless the payment party is overridden on specific jobs the following applies:
The GST Direct (G) flag will be sent in Entry entries. This flag will be shown on the DN.
Auto-rating of GST may be suppressed depending on the setting of the 'Auto-Rate GST Direct Amounts' check box on this form.
A separate charge code may be nominated for direct GST (Registry->Autorating->Charge Codes->Customs->Canada->Disbursement Charge Code Override).
Direct GST amounts will appear in a separate column on the K84/DN reports.
Note this will only apply to subsequent jobs and jobs already completed will not be affected."));
			}
		}

		void ZO_IsImporterDirectPaymentInfo_ValueChanged(object sender, EventArgs e)
		{
			if (parent.ZO_IsImporterDirectPayment)
			{
				Globals.Message.Show(Res.GetString("0FF26B03-DBD9-46B8-8B83-8E407754AC31", @"You have selected 'Importer Direct' for this client, i.e. Import Lodged Security with Customs.
Unless the payment party is overridden on specific jobs the following applies:
The Importer-Lodged-Security (I) flag will be sent in Entry entries, with your Account Security Code or the importers Account Security Code, if specified.
Details will be returned in your K84/DN but will appear under a separate section on the K84 and will not be included in the Broker totals, or on a DN under the importers BN with the “I” flag indicated.
Auto-rating of Duty, Taxes and Fees may be suppressed depending on the setting of the ‘Auto Rate Duty and GST direct amounts’ check box on this form.
Note this will only apply to subsequent jobs and jobs already completed will not be affected."));
			}
		}

		void ZO_IsLVSImporterDirectPayment_ValueChanged(object sender, EventArgs e)
		{
			if (parent.ZO_IsLVSImporterDirectPayment)
			{
				Globals.Message.Show(Res.GetString("0AAEBB9F-478A-4ABB-855D-F1B2BE6F440C", @"You have selected 'Importer Direct' for this client, i.e. Import Lodged Security with Customs.
Unless the payment party is overridden on specific jobs the following applies:
The Importer-Lodged-Security (I) flag will be sent in Entry entries, with your Account Security Code or the importers Account Security Code, if specified.
Details will be returned in your K84/DN but will appear under a separate section on the K84 and will not be included in the Broker totals, or on a DN under the importers BN with the “I” flag indicated.
Auto-rating of Duty, Taxes and Fees may be suppressed depending on the setting of the ‘Auto Rate Duty and GST direct amounts’ check box on this form.
Note this will only apply to subsequent jobs and jobs already completed will not be affected."));
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
	}
}
