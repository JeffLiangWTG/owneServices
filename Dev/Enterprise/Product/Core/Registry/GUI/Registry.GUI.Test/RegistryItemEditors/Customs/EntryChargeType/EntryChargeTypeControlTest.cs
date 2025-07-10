using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeControl))]
	sealed class EntryChargeTypeControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EntryChargeTypeControl)control).EntryChargeTypesGrid.ReadOnly;
		}

		public void TestChargeCodeFindBoxModuleID()
		{
			using (EntryChargeTypeControl control = new EntryChargeTypeControl())
			{
				bool foundColumn = false;
				foreach (object columnStyle in control.EntryChargeTypesGrid.ColumnStyles)
				{
					ZGuidFindBoxColumnStyleInfo guidColumnStyle = columnStyle as ZGuidFindBoxColumnStyleInfo;
					if (guidColumnStyle != null && guidColumnStyle.ColumnName == "AC_ChargeCode")
					{
						AssertEquals("ModuleID", ModuleIDs.AccChargeCodeForRegistry, guidColumnStyle.ModuleID);
						foundColumn = true;
						break;
					}
				}
				AssertEquals("Charge code find box was not found.", true, foundColumn);
			}
		}
	}
}
