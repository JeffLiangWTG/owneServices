using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class AccChargeCodeListEditContainerTest : Enterprise.ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestReadOnly()
		{
			using (ZForm form = new ZForm())
			{
				AccChargeCodeListEditContainer control = GetNewControl();
				form.Controls.Add(control);
				form.Show();

				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("AccChargeCodeGrid.ReadOnly", false, control.AccChargeCodeGrid.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("AccChargeCodeGrid.ReadOnly", true, control.AccChargeCodeGrid.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("AccChargeCodeGrid.ReadOnly", false, control.AccChargeCodeGrid.ReadOnly);
			}
		}

		public void TestFieldValue()
		{
			using (ZForm form = new ZForm())
			{
				AccChargeCodeListEditContainer control = GetNewControl();
				form.Controls.Add(control);
				form.Show();

				AssertNull("Precondition: Data should be null.", control.Data);

				for (int i = 0; i < 2; i++)
				{
					var cc1 = Factory.NewWithValidTestData<AccChargeCode>();
					cc1.AC_Desc = String.Format("CC{0}", i);
					cc1.AC_GC = CompanyPK;
					Factory.Save();

					control.FieldValue = cc1.PK.ToString();

					var data = control.Data;
					AssertEquals("Data.AccChargeCodeList.Count", 1, data.AccChargeCodeList.Count);
					AssertEquals("Data.AccChargeCodeList[0].ChargeCode", cc1.PK, data.AccChargeCodeList[0].ChargeCode);

					AssertEquals("Data.AccChargeCodeList.CompanyPK", CompanyPK, data.AccChargeCodeList.CompanyPK);
					AssertEquals("Data.AccChargeCodeList.Factory", Factory, data.AccChargeCodeList.Factory);
				}
			}
		}

		public void TestFilter()
		{
			using (AccChargeCodeListEditContainer control = new AccChargeCodeListEditContainer(RegistryFindBoxFilter.None, Factory, CompanyPK))
			{
				control.FieldValue = "";
				AssertEquals("BusinessEntity.AccChargeCodeList.Filter", RegistryFindBoxFilter.None, control.Data.AccChargeCodeList.Filter);
			}

			using (AccChargeCodeListEditContainer control = new AccChargeCodeListEditContainer(RegistryFindBoxFilter.FreightChargeCode, Factory, CompanyPK))
			{
				control.FieldValue = "";
				AssertEquals("BusinessEntity.AccChargeCodeList.Filter", RegistryFindBoxFilter.FreightChargeCode, control.Data.AccChargeCodeList.Filter);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = Factory.NewWithValidTestData<GlbCompany>().PK.ToGuid();
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.MinimumSize = new Size(1024, 600);
			result.Size = new Size(1024, 600);
			result.CaptionRenderingEnabled = true;

			AccChargeCodeListEditContainer control = GetNewControl();
			result.Controls.Add(control);
			control.FieldValue = Factory.LoadTop1<AccChargeCode>(new ZQuery()).ToString();

			return result;
		}

		AccChargeCodeListEditContainer GetNewControl()
		{
			return new AccChargeCodeListEditContainer(RegistryFindBoxFilter.None, Factory, CompanyPK);
		}

		Guid CompanyPK;

		#endregion
	}
}
