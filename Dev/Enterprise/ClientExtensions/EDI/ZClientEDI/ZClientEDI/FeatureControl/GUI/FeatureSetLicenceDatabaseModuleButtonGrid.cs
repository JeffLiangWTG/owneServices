using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	public partial class FeatureSetLicenceDatabaseModuleButtonGrid : ZModuleButtonGrid
	{
		public FeatureSetLicenceDatabaseModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.LicenceDatabase;
			OnAttach += Attach;
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm((LicenceDatabase)selected);
		}

		FeatureControlSet FeatureSet => (FeatureControlSet)Form.BusinessEntity;

		protected void Attach(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			var hasChanges = false;

			foreach (var item in e.AttachedBusinessObjects.Cast<LicenceDatabase>())
			{
				if (item.LD_FCS_FeatureSet != FeatureSet.PK)
				{
					hasChanges = true;
					if (item.FeatureSet != null)
					{
						databasesToUpdateFeatureSet.Add(item, item.FeatureSet);
					}
				}

				item.LD_FCS_FeatureSet = FeatureSet.PK;
			}

			if (hasChanges)
			{
				FeatureSet.HasChanges = hasChanges;
			}
		}

		protected override void Detach(BusinessObject selected)
		{
			var detachedDatabase = (LicenceDatabase)selected;
			detachedDatabase.LD_FCS_FeatureSet = ZGuid.Empty;
			FeatureSet.Databases.Remove(detachedDatabase);
			databasesToUpdateFeatureSet.Remove(detachedDatabase);
		}

		public override void Refresh()
		{
			InnerGrid?.ListManager?.Refresh();
			base.UpdateButtonsReadOnly();
			base.Refresh();
		}

		#region Confirm update feature set

		readonly Dictionary<LicenceDatabase, FeatureControlSet> databasesToUpdateFeatureSet = new Dictionary<LicenceDatabase, FeatureControlSet>();

		public string GetFeatureSetUpdateConfirmationMessage()
		{
			if (databasesToUpdateFeatureSet.Count == 0)
			{
				return string.Empty;
			}

			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendLine("There are one or more Licence Databases are already linked to another Feature Set. Please confirm to continue linking to this Feature Set.");
			foreach (var database in databasesToUpdateFeatureSet)
			{
				messageBuilder.AppendLine($"Database {database.Key.LD_DatabaseNumber}: {database.Value.FCS_ProductName} => {FeatureSet.FCS_ProductName}");
			}
			return messageBuilder.ToString();
		}

		public void ClearFeatureSetUpdateConfirmationMessage() => databasesToUpdateFeatureSet.Clear();

		#endregion
	}
}
