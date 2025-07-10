using System;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		protected override Type TestNumbersUserControl() => typeof(NumbersUserControl);

		public void TestInlandModeOfTransportDropEdit()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertEquals("BindTo", "JE_TransportModeInland", userControl.InlandModeOfTransportDropEdit.BindTo);
			}
		}

		public void TestRepresentativeAddressControl()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertEquals("BindTo", "JE_OA_Representative", userControl.RepresentativeAddressControl.BindTo);
			}
		}

		public void TestDeclarantOfficeAddressControl()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertEquals("BindTo", "JE_OA_DeclarantAddress", userControl.DeclarantOfficeAddressControl.BindTo);
			}
		}

		public void TestJE_ManifestNumberTextBox()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var manifestNumberTextBox = userControl.JE_ManifestNumberTextBox;
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", "JE_ManifestNumber", manifestNumberTextBox.BindTo);
					AssertEquals("JE_ManifestNumberTextBox Visibility", true, manifestNumberTextBox.Visible);
				});
			}
		}

		public void TestJE_GoodsOriginCodeFindBox()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var goodsOriginCodeFindBox = userControl.JE_GoodsOriginCodeFindBox;
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", "JE_GoodsOrigin", goodsOriginCodeFindBox.BindTo);
					AssertEquals("JE_GoodsOriginCode Visibility", true, goodsOriginCodeFindBox.Visible);
				});
			}
		}

		public void TestDutyPayerGuidFindBox()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertEquals("BindTo", "JE_OH_DutyPayer", userControl.DutyPayerGuidFindBox.BindTo);
			}
		}

		public void TestJE_MessageSubTypeBoundDropDownEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var userControl = new JobDeclarationUserControl())
			{
				userControl.JobDeclaration = declaration;
				AssertEquals("JE_DeclarationType", userControl.JE_MessageSubTypeBoundDropDownEdit.BindTo);
				AssertEquals("CusEntryInstruction.Lookups.StyleList", userControl.JE_MessageSubTypeBoundDropDownEdit.BindToList);
				AssertEquals("JE_DeclarationType Visibility", !declaration.AreMultipleEntryInstructionsAllowed, userControl.JE_MessageSubTypeBoundDropDownEdit.Visible);
			}
		}

		public void TestBondedWarehouseVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals(false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
			}
		}

		public void TestRemoveBondedWarehouseDocAddressControl()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = userControl.OrganisationsTopPanel.FindSingle<ZDocAddressControl>("BondedWarehouseDocAddressControl");
				AssertEquals("BondedWarehouseDocAddressControl is removed", false, control.Visible);
			}
		}

		public void TestJE_ValuationDateDateEdit()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertEquals("BindTo", "JE_ValuationDate", userControl.JE_ValuationDateDateEdit.BindTo);
			}
		}
	}
}
