using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementDatabaseWrapper : EdiCommissionAgreementTreeBizObjWrapper
	{
		public EdiCommissionAgreementDatabaseWrapper(EdiCommissionAgreementCustomization customization, LicenceDatabase licenceDatabase, IEnumerable<EdiCommissionAgreementCountryWrapper> children)
			: base(customization, children)
		{
			this.LicenceDatabase = licenceDatabase;
		}

		public readonly LicenceDatabase LicenceDatabase;

		ZGuid licenceDatabasePk
		{
			get { return LicenceDatabase != null ? LicenceDatabase.PK : ZGuid.Empty; }
		}

		#region Selected

		public override ZBool Selected
		{
			get { return base.Selected; }
			set
			{
				base.Selected = value;
				IncludeDatabaseUsage = value;
			}
		}

		#endregion

		#region Code

		public override ZString Code
		{
			get
			{
				if (LicenceDatabase == null)
				{
					return "[New Databases]";
				}
				else
				{
					return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", LicenceDatabase.LD_ServerCode, LicenceDatabase.LD_LicenceType);
				}
			}
		}

		#endregion

		#region Description

		public override ZString Description
		{
			get { return ""; }
		}

		#endregion

		#region ShouldAutoAdd

		public override ZBool ShouldAutoAdd
		{
			get { return GetMatchingCompanyAutoAddDatabases().Any(); }
			set
			{
				if (value)
				{
					if (!GetMatchingCompanyAutoAddDatabases().Any())
					{
						customization.CompanyAutoAddDatabases.AddNew(licenceDatabasePk);
					}
				}
				else
				{
					foreach (var companyDatabase in GetMatchingCompanyAutoAddDatabases().ToArray())
					{
						companyDatabase.Delete();
					}
				}
			}
		}

		public override ZBool ShouldAutoAdd_Enabled
		{
			get { return true; }
		}

		#endregion

		#region IncludeDatabaseUsage

		public override ZBool IncludeDatabaseUsage
		{
			get { return GetMatchingDatabasePivots().Any(); }
			set
			{
				if (value)
				{
					if (!GetMatchingDatabasePivots().Any())
					{
						customization.DatabasePivots.AddNew(licenceDatabasePk);
					}
				}
				else
				{
					foreach (var databasePivot in GetMatchingDatabasePivots().ToArray())
					{
						databasePivot.Delete();
					}
				}
			}
		}

		public override ZBool IncludeDatabaseUsage_Enabled
		{
			get { return true; }
		}

		#endregion

		IEnumerable<EdiCommissionAgreementCompanyAutoAddDatabase> GetMatchingCompanyAutoAddDatabases()
		{
			return customization.CompanyAutoAddDatabases.Where(x => x.EPD_LD == licenceDatabasePk);
		}

		IEnumerable<EdiCommissionAgreementDatabasePivot> GetMatchingDatabasePivots()
		{
			return customization.DatabasePivots.Where(x => x.EZD_LD == licenceDatabasePk);
		}
	}
}
