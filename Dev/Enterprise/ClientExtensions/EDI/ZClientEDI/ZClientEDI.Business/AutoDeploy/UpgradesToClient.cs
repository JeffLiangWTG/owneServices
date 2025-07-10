using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.AutoDeploy.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public class UpgradesToClient : AutoUpgradesToClient, IComparable
	{
		public UpgradesToClient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoUpgradesToClient.Schema
		{
			public const string EnterpriseCode = "EnterpriseCode";
			public const string ClientSpecificCode = "ClientSpecificCode";
		}

		#endregion

		#region Overriden Properties

		public override ZString L1_CurrentStatus
		{
			get { return base.L1_CurrentStatus; }
			set
			{
				ZString oldValue = L1_CurrentStatus;
				base.L1_CurrentStatus = value;

				if (oldValue != value)
				{
					NotifyOnStatusChange();
				}
			}
		}

		#endregion

		#region RelatedBusinessObjects

		#region LicDatabase

		[RelatedBusinessObject("LicDatabase")]
		public override ZGuid L1_LD
		{
			get { return base.L1_LD; }
			set
			{
				base.L1_LD = value;
				OnLicDatabaseAssignment();
			}
		}

		public virtual LicenceDatabase LicDatabase
		{
			get { return Factory.Load<LicenceDatabase>(L1_LD); }
		}

		#endregion

		#region Build

		[RelatedBusinessObject("Build")]
		public override ZGuid L1_HL
		{
			get { return base.L1_HL; }
			set { base.L1_HL = value; }
		}

		public virtual ReleaseBuild Build
		{
			get { return Factory.Load<ReleaseBuild>(L1_HL); }
		}

		#endregion

		#region Organisation

		[RelatedBusinessObject("Organisation")]
		public override ZGuid L1_OH
		{
			get { return base.L1_OH; }
			set { base.L1_OH = value; }
		}

		public virtual EDIOrgHeader Organisation
		{
			get { return Factory.Load<EDIOrgHeader>(L1_OH); }
		}

		#endregion

		#region User

		public GlbStaff User
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, L1_GS_NKStaffCode); }
		}

		#endregion

		#endregion

		#region Properties

		#region L1_RequestedUpgradeMethod

		public override ZString L1_RequestedUpgradeMethod
		{
			get
			{
				return base.L1_RequestedUpgradeMethod;
			}
			set
			{
				base.L1_RequestedUpgradeMethod = value;
				AssignActualUpgradeMethod();
			}
		}

		#endregion

		#region EnterpriseCode

		public ZString EnterpriseCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (LicDatabase != null)
				{
					if (LicDatabase.LicHeadersForAllCompanies.Count > 0)
					{
						result = LicDatabase.LicHeadersForAllCompanies[0].Company.LicEnterprise.LE_EnterpriseCode;
					}
					else
					{
						result = LicDatabase.EnterpriseCode;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo EnterpriseCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EnterpriseCode); }
		}

		#endregion

		#region ClientSpecificCode

		public ZString ClientSpecificCode
		{
			get
			{
				ZString enterpriseCode = EnterpriseCode;
				ReleaseBuild build = Build;
				return (build != null) && build.IsSpecificForClient(enterpriseCode) ? enterpriseCode : ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region IComparable

		public int CompareTo(object obj)
		{
			var temp = obj as UpgradesToClient;

			if (temp != null)
			{
				return CompareTo(temp, true);
			}
			else
			{
				throw new ArgumentException("object is not an UpgradesToClient");
			}
		}

		int CompareTo(UpgradesToClient otherUpgrade, bool considerRequestedDateTime = true)
		{
			int result = this.L1_CurrentStatus.CompareTo(otherUpgrade.L1_CurrentStatus);
			if (result == 0)
			{
				result = Build.HL_ExeVersionDate.CompareTo(otherUpgrade.Build.HL_ExeVersionDate);
			}

			if (result == 0)
			{
				result = ClientSpecificCode.CompareTo(otherUpgrade.ClientSpecificCode);
			}

			if (result == 0)
			{
				result = Build.VersionNumber.CompareTo(otherUpgrade.Build.VersionNumber);
			}

			if (result == 0)
			{
				result = L1_ActualUpgradeMethod.CompareTo(otherUpgrade.L1_ActualUpgradeMethod);
			}

			if (considerRequestedDateTime && result == 0)
			{
				result = L1_RequestedDateTime.CompareTo(otherUpgrade.L1_RequestedDateTime);
			}

			return result;
		}

		#endregion

		#region Upgrades Grouping Support

		public bool CouldBeGroupedWith(UpgradesToClient otherUpgrade)
		{
			return otherUpgrade != null && CompareTo(otherUpgrade, false) == 0;
		}

		#endregion

		#region Implementation

		protected void OnLicDatabaseAssignment()
		{
			if (LicDatabase != null && LicDatabase.LicEnterprise != null && LicDatabase.LicEnterprise.Header != null)
			{
				L1_OH = LicDatabase.LicEnterprise.Header.PK;
			}
			AssignActualUpgradeMethod();
		}

		protected void AssignActualUpgradeMethod()
		{
			if (LicDatabase == null)
			{
				L1_ActualUpgradeMethod = ZString.Empty;
			}
			else
			{
				if (LicDatabase.IsUpgradeMethodSupported(L1_RequestedUpgradeMethod))
				{
					L1_ActualUpgradeMethod = L1_RequestedUpgradeMethod;
				}
				else
				{
					L1_ActualUpgradeMethod = LicDatabase.GetHighestSupportedUpgradeMethod();
				}
			}
		}

		protected void NotifyOnStatusChange()
		{
			if (User != null && !User.GS_EmailAddress.IsEmpty)
			{
				ZString subject = ZString.Empty;
				ZString messageBody = ZString.Empty;

				switch (L1_CurrentStatus)
				{
					case UpgradesToClientStatus.Codes.Failed:
						{
							subject = "Upgrade delivery failure notification";
							messageBody = ZString.Format("Sending upgrade {0} to the client {1}, server {2} using {3} failed at {4}",
								Build != null ? Build.PackageName : "", EnterpriseCode, LicDatabase != null ? LicDatabase.LD_ServerCode : ZString.Empty, L1_ActualUpgradeMethod, ZDateTime.Now);
							break;
						}
					case UpgradesToClientStatus.Codes.Blocked:
						{
							subject = "Blocked Upgrade notification";
							messageBody = ZString.Format("Sending upgrade {0} to the client {1}, server {2} using {3} was blocked at {4}",
								Build != null ? Build.PackageName : "", EnterpriseCode, LicDatabase != null ? LicDatabase.LD_ServerCode : ZString.Empty, L1_ActualUpgradeMethod, ZDateTime.Now);
							break;
						}
					case UpgradesToClientStatus.Codes.Received:
						{
							subject = "Upgrade delivery notification";
							messageBody = ZString.Format("Upgrade {0} was successfully delivered to the client {1}, server {2} using {3} at {4}",
								Build != null ? Build.PackageName : "", EnterpriseCode, LicDatabase != null ? LicDatabase.LD_ServerCode : ZString.Empty, L1_ActualUpgradeMethod, ZDateTime.Now);
							break;
						}
					default: break;
				}

				if (!subject.IsEmpty)
				{
					Env.OutgoingMailManager.CreateAndSaveSimple(subject, messageBody, User.GS_EmailAddress);
				}
			}
		}

		#endregion
	}
}

