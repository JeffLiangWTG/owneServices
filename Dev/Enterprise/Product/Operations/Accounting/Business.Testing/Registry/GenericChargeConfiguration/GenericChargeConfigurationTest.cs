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
	[TestedType(typeof(GenericChargeConfiguration))]
	public class GenericChargeConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			AssertNoErrors("Precondition: Note type shouldn't have error", BizObj.ChargePKInfo);
			BizObj.ChargePK = Setting.ChargePK;
			BizObj.ChargePK = new ZGuid();

			AssertHasError(BizObj.ChargePKInfo, "Please enter a Charge or GL Account.");

			BizObj.RunPreSaveValidation();

			var settings = new GenericChargeConfigurationCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			settings.Add(Setting);

			var charge2 = Factory.NewWithValidTestData<AccChargeCode>();
			charge2.AC_Code = "NA2";
			charge2.AC_GC = GlbCompany.CurrentCompany.PK;
			charge2.AC_ChargeType = "MRG";
			Factory.Save();
			var setting2 = settings.AddNew();
			setting2.ChargePK = charge2.PK;

			AssertNoErrors(setting2.ChargePKInfo);
			setting2.ChargePK = Setting.ChargePK;

			AssertHasNotifications(GenericChargeConfiguration.ErrorDuplicateCharge, setting2.ChargePKInfo);
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
			return new GenericChargeConfiguration(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return Setting;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GenericChargeConfiguration();
		}

		protected new GenericChargeConfiguration BizObj
		{
			get { return (GenericChargeConfiguration)base.BizObj; }
		}

		GenericChargeConfiguration setting;
		GenericChargeConfiguration Setting
		{
			get
			{
				if (setting == null)
				{
					setting = (GenericChargeConfiguration)GetBusinessObjectToClone();
					setting.ChargePK = Charge.PK;
				}

				return setting;
			}
		}

		BusinessObject charge;
		BusinessObject Charge
		{
			get
			{
				if (charge == null)
				{
					charge = Factory.NewWithValidTestData<AccChargeCode>();
					charge[AccChargeCodeSchema.AC_Code] = "NAB";
					charge[AccChargeCodeSchema.AC_GC] = GlbCompany.CurrentCompany.PK;
					charge[AccChargeCodeSchema.AC_ChargeType] = "MRG";
					Factory.Save();
				}
				return charge;
			}
		}
	}

	#endregion
}
