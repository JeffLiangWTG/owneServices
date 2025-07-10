using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class AccTaxRateListEditContainerTest : Enterprise.ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		[RequiresSTA]
		public void TestReadOnly()
		{
			using (ZForm form = new ZForm())
			{
				AccTaxRateListEditContainer control = GetNewControl();
				form.Controls.Add(control);
				form.Show();

				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("AccTaxRateGrid.ReadOnly", false, control.AccTaxRateGrid.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("AccTaxRateGrid.ReadOnly", true, control.AccTaxRateGrid.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("AccTaxRateGrid.ReadOnly", false, control.AccTaxRateGrid.ReadOnly);
			}
		}

		public void TestFieldValue()
		{
			using (ZForm form = new ZForm())
			{
				AccTaxRateListEditContainer control = GetNewControl();
				form.Controls.Add(control);
				form.Show();

				AssertNull("Precondition: Data should be null.", control.Data);

				for (int i = 0; i < 2; i++)
				{
					ZGuid guid = ZGuid.NewZGuid();
					control.FieldValue = guid.ToString();

					var data = control.Data;
					AssertEquals("Data.AccTaxRateList.Count", 1, data.AccTaxRateList.Count);
					AssertEquals("Data.AccTaxRateList[0].TaxRate", guid, data.AccTaxRateList[0].TaxRate);

					AssertEquals("Data.AccTaxRateList.CompanyPK", CompanyPK, data.AccTaxRateList.CompanyPK);
					AssertEquals("Data.AccTaxRateList.Factory", Factory, data.AccTaxRateList.Factory);
				}
			}
		}

		public void TestFilter()
		{
			using (AccTaxRateListEditContainer control = new AccTaxRateListEditContainer(RegistryFindBoxFilter.None, Factory, CompanyPK))
			{
				control.FieldValue = "";
				AssertEquals("BusinessEntity.AccTaxRateList.Filter", RegistryFindBoxFilter.None, control.Data.AccTaxRateList.Filter);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = Guid.NewGuid();
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.MinimumSize = new Size(1024, 600);
			result.Size = new Size(1024, 600);
			result.CaptionRenderingEnabled = true;

			AccTaxRateListEditContainer control = GetNewControl();
			result.Controls.Add(control);
			control.FieldValue = Factory.LoadTop1<AccTaxRate>(new ZQuery()).ToString();

			return result;
		}

		AccTaxRateListEditContainer GetNewControl()
		{
			return new AccTaxRateListEditContainer(RegistryFindBoxFilter.None, Factory, CompanyPK);
		}

		Guid CompanyPK;

		#endregion
	}
}
