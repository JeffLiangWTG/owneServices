using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(UsersAuthorizedToReopenClosedPeriods))]
	public class UsersAuthorizedToReopenClosedPeriodsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			AssertNoErrors("Precondition: Note type shouldn't have error", BizObj.StaffPKInfo);
			BizObj.StaffPK = new ZGuid();

			AssertHasError(BizObj.StaffPKInfo, "Please enter a value.");

			BizObj.RunPreSaveValidation();

			UsersAuthorizedToReopenClosedPeriodsCollection settings = new UsersAuthorizedToReopenClosedPeriodsCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			settings.Add(Setting);

			UsersAuthorizedToReopenClosedPeriods setting2 = settings.AddNew();

			AssertNoErrors(setting2.StaffPKInfo);
			setting2.StaffPK = Setting.StaffPK;

			AssertHasNotifications(setting2.ErrorDuplicateGlobalStaff, setting2.StaffPKInfo);
		}

		#region Implementation

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new UsersAuthorizedToReopenClosedPeriods(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return Setting;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UsersAuthorizedToReopenClosedPeriods();
		}

		protected new UsersAuthorizedToReopenClosedPeriods BizObj
		{
			get { return (UsersAuthorizedToReopenClosedPeriods)base.BizObj; }
		}

		UsersAuthorizedToReopenClosedPeriods setting;
		UsersAuthorizedToReopenClosedPeriods Setting
		{
			get
			{
				if (setting == null)
				{
					setting = (UsersAuthorizedToReopenClosedPeriods)GetBusinessObjectToClone();
					setting.StaffPK = Staff.PK;
				}

				return setting;
			}
		}

		BusinessObject staff;
		BusinessObject Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.NewWithValidTestData<GlbStaff>();
					staff[GlbStaffSchema.GS_FullName] = "Spanch Bob";
					staff[GlbStaffSchema.GS_LoginName] = "Square.Pants";
					Factory.Save();
				}
				return staff;
			}
		}
	}

	#endregion
}
