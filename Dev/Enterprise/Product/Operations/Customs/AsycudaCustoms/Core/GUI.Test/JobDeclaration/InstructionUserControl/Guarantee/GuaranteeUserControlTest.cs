using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.AsycudaCustoms.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class GuaranteeUserControlTest : TestCaseWithFactory
	{
		public void TestLinkButtonEnabled_PW_BondTypeInfo_ValueChanged()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.PW_BondType = MasterFiles.Business.GuaranteeBondTypeList.Codes.SingleTransaction;
					AssertEquals("No for SingleTransaction", false, button.Enabled);
				});
			}
		}

		public void TestLinkButtonEnabled_PW_CPH_GuaranteeInfo_ValueChanged()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.PW_CPH_Guarantee = ZGuid.Empty;
					AssertEquals("No for Empty PW_CPH_Guarantee", false, button.Enabled);
				});
			}
		}

		public void TestLinkButtonEnabled_PW_BondNumber2Info_ValueChanged()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.PW_BondNumber2 = ZString.Empty;
					AssertEquals("No for Empty PW_BondNumber2", false, button.Enabled);
				});
			}
		}

		public void TestLinkButtonEnabled_PW_BondEffectiveDateInfo_ValueChanged()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.PW_BondEffectiveDate = ZDateTime.Empty;
					AssertEquals("No for Empty PW_BondEffectiveDate", false, button.Enabled);
				});
			}
		}

		public void TestLinkButtonEnabled_PW_BondAmountInfo_ValueChanged()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.PW_BondAmount = ZDecimal.Zero;
					AssertEquals("No for Empty PW_BondAmount", false, button.Enabled);
				});
			}
		}

		public void TestLinkButtonEnabled_PW_StatusInfo_ValueChanged()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.PW_BondAmount = ZDecimal.Zero;
					guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
					AssertEquals("Yes for Empty PW_BondAmount and Linked", true, button.Enabled);
				});
			}
		}

		public void TestLinkButtonEnabled_ReleaseGuarantees_CollectionCountChange()
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var button = control.FindSingle<ZButton>("LinkButton");
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, button.Enabled);
					guarantee.Instruction.ReleaseGuarantees.AddNew();
					AssertEquals("No if release guarantee exists", false, button.Enabled);
				});
			}
		}

		public void TestLinkButton_Link_UserAgreesToSaveJobFirst() => AssertLink(true);

		public void TestLinkButton_Link_UserRejectsToSaveJobFirst() => AssertLink(false);

		public void TestLinkButton_Unlink_UserAgreesToSaveJobFirst() => AssertUnlink(true);

		public void TestLinkButton_Unlink_UserRejectsToSaveJobFirst() => AssertUnlink(false);

		public void TestCustomsCurrencyTextBox()
		{
			using (var control = new GuaranteeUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZTextBox>("CustomsCurrencyTextBox"));
			}
		}

		public void TestLinkButton()
		{
			using (var control = new GuaranteeUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZButton>("LinkButton"));
			}
		}

		public void TestGuaranteeGuidFindBox()
		{
			using (var control = new GuaranteeUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZGuidFindBox>("GuaranteeGuidFindBox"));
			}
		}

		public void TestBondTypeDropEdit()
		{
			using (var control = new GuaranteeUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZDropEdit>("BondTypeDropEdit"));
			}
		}

		public void TestStatusDropEdit()
		{
			using (var control = new GuaranteeUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZDropEdit>("StatusDropEdit"));
			}
		}

		public void TestAmountCalcEdit()
		{
			using (var control = new GuaranteeUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZCalcEdit>("AmountCalcEdit"));
			}
		}

		void AssertLink(bool doesUserAgreeToSaveJobFirst)
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidNotLinkedGuarantee();
			var declaration = guarantee.Instruction.JobDeclaration;
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				if (doesUserAgreeToSaveJobFirst)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				}
				control.FindSingle<ZButton>("LinkButton").PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Status updated", doesUserAgreeToSaveJobFirst ? GuaranteeStatusList.Codes.Linked : GuaranteeStatusList.Codes.NotLinked, guarantee.PW_Status);
					AssertEquals("Saved", doesUserAgreeToSaveJobFirst, !declaration.HasChanges);
				});
			}
		}

		void AssertUnlink(bool doesUserAgreeToSaveJobFirst)
		{
			var guarantee = new GuaranteeTestHelper(Factory).CreateValidLinkedGuarantee();
			var declaration = guarantee.Instruction.JobDeclaration;
			declaration.JE_GoodsDescription = "Goods";
			using (var form = new ZForm(guarantee.Instruction))
			using (var control = new GuaranteeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				if (doesUserAgreeToSaveJobFirst)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				}
				control.FindSingle<ZButton>("LinkButton").PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Status updated", doesUserAgreeToSaveJobFirst ? GuaranteeStatusList.Codes.NotLinked : GuaranteeStatusList.Codes.Linked, guarantee.PW_Status);
					AssertEquals("Saved", doesUserAgreeToSaveJobFirst, !declaration.HasChanges);
				});
			}
		}
	}
}
