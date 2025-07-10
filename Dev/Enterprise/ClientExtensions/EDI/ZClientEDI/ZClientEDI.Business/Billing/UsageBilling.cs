using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Common class for On Demand and STL billing
	/// </summary>
	public class UsageBilling : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UsageBilling(BusinessObjectFactory factory)
			: base(factory)
		{
			if (Globals.IsTest)
			{
				factoryForGenerate = factory;
			}

			InitDateRange();
		}

		#region Business Object Overrides

		// Tell the architecture that the class never has changes.
		// This avoids the Documents context menu item saying "Please save your record..."
		public override bool HasChanges
		{
			get { return false; }
			set { }
		}

		#endregion

		#region Init Date Range

		void InitDateRange()
		{
			ZDateTime lastMonth = ZDateTime.Now.AddMonths(-1);
			lastMonth = new ZDateTime(lastMonth.Year, lastMonth.Month, 1);
			DateTo = lastMonth.AddMonths(1).AddDays(-1);
		}

		#endregion

		#region Properties

		#region DateTo

		public ZDateTime DateTo
		{
			get { return dateTo; }
			set
			{
				SetNonPersistentPropertyValue(DateToInfo, ref dateTo, value);
				if (!IsValidationSuspended)
				{
					ValidateDateTo();
				}
			}
		}
		ZDateTime dateTo;

		public ZPropertyInfo DateToInfo
		{
			get { return GetZPropertyInfo(nameof(DateTo)); }
		}

		public void ValidateDateTo()
		{
			DateToInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DateToInfo);

			if (!DateToInfo.HasErrors())
			{
				if (!DateTo.IsValid)
				{
					DateToInfo.AddError("Please enter a valid date.");
				}
				else if (DateTo.Month == DateTo.AddDays(1).Month)
				{
					DateToInfo.AddError("Date must be a last day of the month.");
				}
			}
		}

		#endregion

		#region OrganisationPK

		[List("Organisations")]
		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganisationPK();
				}
			}
		}
		ZGuid organisationPK;

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return this.GetZPropertyInfo(nameof(OrganisationPK)); }
		}

		public void ValidateOrganisationPK()
		{
			OrganisationPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(OrganisationPKInfo);
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public EDIOrgHeader Organisation => Factory.Load<EDIOrgHeader>(OrganisationPK);

		#endregion

		#region EnterpriseCode

		[MaxLength(3)]
		public ZString EnterpriseCode
		{
			get { return enterpriseCode; }
			set
			{
				if (enterpriseCode != value)
				{
					CheckMaximumLength(EnterpriseCodeInfo, value);
					SetNonPersistentPropertyValue(EnterpriseCodeInfo, ref enterpriseCode, value);
				}
			}
		}
		ZString enterpriseCode;
		public ZPropertyInfo EnterpriseCodeInfo { get { return GetZPropertyInfo(nameof(EnterpriseCode)); } }

		#endregion

		#region Licence Mode

		[List("LicenceModeList")]
		public ZString LicenceMode
		{
			get { return licenceMode;  }
			set
			{
				SetNonPersistentPropertyValue(LicenceModeInfo, ref licenceMode, value);
				if (!IsValidationSuspended)
				{
					ValidateLicenceMode();
				}
			}
		}
		ZString licenceMode = LicenceModeConstants.Codes.Odpl;

		public ZPropertyInfo LicenceModeInfo
		{
			get { return this.GetZPropertyInfo(nameof(LicenceMode)); }
		}

		public void ValidateLicenceMode()
		{
			LicenceModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LicenceModeInfo);
			ListValidation.ErrorIfInvalidCode(LicenceModeInfo);
		}

		public static class LicenceModeConstants
		{
			public static class Codes
			{
				public const string Odpl = "ODM";
				public const string STL = "STL";
				public const string ALL = "ALL";
			}

			public static class Descriptions
			{
				public const string Odpl = "ODPL";
				public const string STL = "STL";
				public const string ALL = "ALL";
			}
		}

		public CodeDescriptionPairList LicenceModeList
		{
			get
			{
				CodeDescriptionPairList supportModeList = new CodeDescriptionPairList();

				supportModeList.AddPair(LicenceModeConstants.Codes.Odpl, LicenceModeConstants.Descriptions.Odpl);
				supportModeList.AddPair(LicenceModeConstants.Codes.STL, LicenceModeConstants.Descriptions.STL);
				supportModeList.AddPair(LicenceModeConstants.Codes.ALL, LicenceModeConstants.Descriptions.ALL);

				return supportModeList;
			}
		}

		#endregion

		#region Minimum Amount to Bill

		public static ZDecimal SystemMinimumAmountToBill
		{
			get { return EDIDataRegistry.Instance.MinimumAmountToBill.Value; }
		}

		public ZDecimal MinimumAmountToBill
		{
			get { return SystemMinimumAmountToBill; }
		}

		#endregion

		#region Back Post

		public ZBool IsBackPostAvailable
		{
			get { return isBackPostAvailable; }
		}
		ZBool isBackPostAvailable;

		internal void SetIsBackPostAvailableForTest(bool val)
		{
			isBackPostAvailable = val;
		}

		public ZPropertyInfo IsBackPostAvailableInfo
		{
			get { return GetZPropertyInfo(nameof(IsBackPostAvailable)); }
		}

		public ZBool IsBackPostAllowed
		{
			get { return isBackPostAllowed; }
			set
			{
				SetNonPersistentPropertyValue(IsBackPostAllowedInfo, ref isBackPostAllowed, value);
			}
		}
		ZBool isBackPostAllowed = true;

		public ZPropertyInfo IsBackPostAllowedInfo
		{
			get { return GetZPropertyInfo(nameof(IsBackPostAllowed)); }
		}

		public void UpdateIsBackPostAvailable(GlbBranch[] branches)
		{
			isBackPostAvailable = false;
			var today = ZDateTime.Today;
			var backPostDate = today.AddDays(-today.Day);
			if (backPostDate >= DateTo)
			{
				if (branches.Length > 0)
				{
					var companyPks = branches.Select(x => x.GB_GC).Distinct().ToArray();
					var periodCalc = new AccountingPeriodCalculator(branches[0].Factory);
					isBackPostAvailable = true;
					foreach (var companyPk in companyPks)
					{
						var period = periodCalc.GetFirstOpenPeriod(companyPk);
						if (period != null && period.AM_StartDate >= backPostDate)
						{
							isBackPostAvailable = false;
							break;
						}
					}
				}
			}

			IsBackPostAvailableInfo.RefreshBinding();
		}

		#endregion

		#endregion

		protected BusinessObjectFactory FactoryForGenerate
		{
			get { return factoryForGenerate ?? (factoryForGenerate = new BusinessObjectFactory() { RefreshEnabled = false }); }
			set { factoryForGenerate = value; }
		}
		BusinessObjectFactory factoryForGenerate;
	}
}

