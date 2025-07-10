using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientLicenceHeaderEx : AutoClientLicenceHeaderEx
	{
		public ClientLicenceHeaderEx(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public LicenceHeader LicHeader
		{
			get { return Factory.Load<LicenceHeader>(L0_LA); }
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("LicHeader")]
		public override ZGuid L0_LA
		{
			get { return base.L0_LA; }
			set { base.L0_LA = value; }
		}

		#region NextAndCurrentMaintenancePercent

		/// <summary>
		/// If the user sets the next percent
		/// then for consistency the current percent should be set to match.
		/// </summary>
		public ZDecimal NextAndCurrentMaintenancePercent
		{
			get { return base.L0_NextMaintenancePercent; }
			set
			{
				base.L0_NextMaintenancePercent = value;
				base.L0_CurrentMaintenancePercent = value;
			}
		}

		public virtual ZPropertyInfo NextAndCurrentMaintenancePercentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.L0_NextMaintenancePercent, x => L0_NextMaintenancePercentInfo); }
		}

		#endregion

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			bool result = false;
			if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
			{
				result = true;
			}
			else
			{
				switch (property.Name)
				{
					case ClientLicenceHeaderExSchema.Constants.L0_NextMaintenancePercent:
					case ClientLicenceHeaderExSchema.Constants.L0_NextNewSeatMaintenancePercent:
						result = L0_FixedMaintenanceAmount != 0m;
						break;

					default:
						break;
				}
			}

			return result;
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			L0_NextNewSeatMaintenancePercent = 30m;
			L0_LastNewSeatMaintenancePercent = 30m;
		}

		#region Log Changes

		public override void OnSaving()
		{
			base.OnSaving();
			if (LicHeader != null)
			{
				BillingInvoicingHelper.SynchronizeChild(LicHeader, this, ClientLicenceHeaderExSchema.L0_LA);
				LogChanges();
			}
		}

		void LogChanges()
		{
			if (IsInDatabase)
			{
				if (L0_LastMaintenancePercentInfo.HasChanges ||
					L0_LastNewSeatMaintenancePercentInfo.HasChanges ||
					L0_NextMaintenancePercentInfo.HasChanges ||
					L0_NextNewSeatMaintenancePercentInfo.HasChanges)
				{
					ZStringBuilder builder = new ZStringBuilder("Maintenance");
					LogHelper.BuildLog(builder, " | Last%:", L0_LastMaintenancePercentInfo);
					LogHelper.BuildLog(builder, " | LastNew%:", L0_LastNewSeatMaintenancePercentInfo);
					LogHelper.BuildLog(builder, " | Next%:", L0_NextMaintenancePercentInfo);
					LogHelper.BuildLog(builder, " | NextNew%:", L0_NextNewSeatMaintenancePercentInfo);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					LicHeader.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (L0_FixedMaintenanceAmountInfo.HasChanges ||
					L0_RX_NKFixedMaintenanceCurrencyInfo.HasChanges)
				{
					ZStringBuilder builder = new ZStringBuilder("Maintenance Fixed");
					LogHelper.BuildLog(builder, " | Amount:", L0_FixedMaintenanceAmountInfo);
					LogHelper.BuildLog(builder, " | Currency:", L0_RX_NKFixedMaintenanceCurrencyInfo);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					LicHeader.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		#endregion
	}
}

