using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class StlLicenceControl : ZUserControl
	{
		public StlLicenceControl()
		{
			InitializeComponent();
			showExpiredPricesCheckBox.ReadOnlyChanged += ShowExpiredPricesCheckBox_ReadOnlyChanged;
			showExpiredSettingsCheckBox.ReadOnlyChanged += ShowExpiredSettingsCheckBox_ReadOnlyChanged;
		}

		void ShowExpiredSettingsCheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			// Prevent checkbox from ever being readonly. It only affects the display.
			if (showExpiredSettingsCheckBox.ReadOnly)
			{
				showExpiredSettingsCheckBox.ReadOnly = false;
			}
		}

		void ShowExpiredPricesCheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			// Prevent checkbox from ever being readonly. It only affects the display.
			if (showExpiredPricesCheckBox.ReadOnly)
			{
				showExpiredPricesCheckBox.ReadOnly = false;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SplitterState.Persist(settingsSplitContainer);
			SplitterState.Persist(pricesSettingsSplitter);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var licHeader = CurrentDataItem as LicenceHeader;
			var db = licHeader != null && !licHeader.IsDeleted ? licHeader.Database : null;
			if (db != null)
			{
				db.LD_IsBilledPerCompanyInfo.ValueChanged -= LD_IsBilledPerCompanyInfo_ValueChanged;
			}

			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var licHeader = CurrentDataItem as LicenceHeader;
			var db = licHeader != null && !licHeader.IsDeleted ? licHeader.Database : null;
			if (db != null)
			{
				ProductionServiceLabel.Visible = db.LD_LicenceType != DatabaseTypes.Codes.Production;
				priceCurrencyBox.Visible = db.LD_IsBilledPerCompany;
				db.LD_IsBilledPerCompanyInfo.ValueChanged += LD_IsBilledPerCompanyInfo_ValueChanged;
			}
		}

		void LD_IsBilledPerCompanyInfo_ValueChanged(object sender, EventArgs e)
		{
			var licHeader = CurrentDataItem as LicenceHeader;
			var db = licHeader != null && !licHeader.IsDeleted ? licHeader.Database : null;
			if (db != null)
			{
				priceCurrencyBox.Visible = db.LD_IsBilledPerCompany;

				if (db.LD_IsBilledPerCompany && db.LD_IsBilledPerCompanyInfo.HasChanges)
				{
					if (db.ActiveLicHeadersForAllCompanies.OfType<LicenceHeader>().Any(x => x.PK != licHeader.PK && x.LA_RX_NKPriceCurrency.IsEmpty))
					{
						Globals.Message.ShowInformation(@"The Price Currency entered here will be set to all organizations attached to this database.
Edit the other organizations manually if they require a different currency.");
					}
				}
			}
		}

		void newButton_Click(object sender, EventArgs e)
		{
			if (IsSecurityCheckpointAllowed())
			{
				var strip = new ContextMenuStrip();

				foreach (ICodeDescription type in BillingConstants.LicenceSetting.GetTypeList())
				{
					var item = strip.Items.Add(type.Description, null, AddSetting);
					item.Tag = type.Code;
				}
				var button = sender as Control;
				strip.Show(button, 0, button.Height);
			}
		}

		protected void AddSetting(object sender, EventArgs e)
		{
			LicenceHeader licHeader = CurrentDataItem as LicenceHeader;
			if (licHeader != null)
			{
				var db = licHeader.Database;
				var sortedObjs = db.LicenceSettingsForBinding.ToList();
				var item = (ToolStripItem)sender;
				var typeCode = (string)item.Tag;
				var type = EdiLicenceSettingTypeDecider.GetTypeByCode(typeCode);
				var setting = (EdiLicenceSetting)db.Factory.New(type);
				sortedObjs.Add(setting);

				var template = GetLastSettingByType(db.Factory, typeCode);
				if (template != null)
				{
					setting.LS9_GE_Department1 = template.LS9_GE_Department1;
					setting.LS9_GE_Department2 = template.LS9_GE_Department2;
					setting.LS9_Name = template.LS9_Name;
				}

				setting.LS9_LD = db.PK;
				setting.LS9_Type = typeCode;
				db.LicenceSettingsForBinding.Add(setting);

				//new line appears at the bottom of the list and preserve the original list order.
				db.LicenceSettingsForBinding.ApplySort(Comparer<EdiLicenceSetting>.Create((x, y) => sortedObjs.IndexOf(x).CompareTo(sortedObjs.IndexOf(y))) as IComparer<EdiLicenceSetting>);

				int index = SettingsGrid.ListManager.List.IndexOf(setting);
				SettingsGrid.ListManager.Position = index;
			}
		}

		EdiLicenceSetting GetLastSettingByType(BusinessObjectFactory factory, string typeCode)
		{
			var query = new ZQuery(EdiLicenceSettingSchema.LS9_Type, typeCode);
			query.AddToFilter(EdiLicenceSettingSchema.LS9_SystemCreateUser, Env.CurrentUser.Initials);
			query.OrderBy = EdiLicenceSettingSchema.Constants.LS9_SystemCreateTimeUtc + OrderByClause.Descending;
			var result = factory.LoadTop1<EdiLicenceSetting>(query);

			if (result == null)
			{
				query = new ZQuery(EdiLicenceSettingSchema.LS9_Type, typeCode);
				query.OrderBy = EdiLicenceSettingSchema.Constants.LS9_SystemCreateTimeUtc + OrderByClause.Descending;
				result = factory.LoadTop1<EdiLicenceSetting>(query);
			}

			return result;
		}

		bool IsSecurityCheckpointAllowed()
		{
			bool result = EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed;
			if (!result)
			{
				EDISecurityCheckpoints.OrgLicenceBilling.ShowError();
			}

			return result;
		}
	}
}

