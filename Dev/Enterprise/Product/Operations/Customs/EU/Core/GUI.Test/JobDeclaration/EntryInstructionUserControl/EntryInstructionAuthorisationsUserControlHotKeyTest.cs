#if !WINZOR
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryInstructionAuthorisationsUserControlHotKeyTest : TestCaseWithFactory
	{
		public void TestAuthorisationUsageUpdate()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var config = GetEnableAdHocTrueConfiguration();
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControlForAdditionalTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				EmulateF5KeyDown(control);

				CombineAssertions("Updated authorisation usage", () =>
				{
					var authorisationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault();
					AssertNotNull("A new authorization should be produced as result of action", authorisationUsage);
					AssertEquals("Number of authorizations should be 1", 1, entryInstruction.CusAuthorizationUsages.Count);
					AssertEquals("Owner", "OWNER", authorisationUsage.Owner.OH_Code);
					AssertEquals("AGC_Code", "BLA", authorisationUsage.AGC_Code);
					Assert("AGC_CPH_Authorization should have a value", !authorisationUsage.AGC_CPH_Authorization.IsEmpty);
				});
			}
		}

		public void TestF5HotKeyRegistration()
		{
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				AssertEquals("F5 should not be registered when AdHoc is not enabled", false, control.AuthorisationsGrid.Hotkeys.IsRegistered(Keys.F5));
			}

			var config = GetEnableAdHocTrueConfiguration();
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				AssertEquals("F5 should be registered when AdHoc is enabled", true, control.AuthorisationsGrid.Hotkeys.IsRegistered(Keys.F5));
			}
		}

		public void TestF5KeyDownShowsDialogWhenNoAuthorisationNumberCaptured()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorisationUsage.AGC_Number = "12345";
			authorisationUsage.AGC_OH_Owner = owner.PK;
			authorisationUsage.AGC_Code = "BLA";
			AssertNull(authorisationUsage.RelatedAuthorisationHeader);

			var authorisationHeader = Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345";
			authorisationHeader.CPH_Type = "BLA";
			authorisationHeader.CPH_OH_PermitHolder = owner.PK;
			Factory.Save();
			var config = GetEnableAdHocTrueConfiguration();
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				Env.Security.FlagAuthorisationAdHoc.IsAllowed = true;
				EmulateF5KeyDown(control);
				AssertNull("Prerequisite : No temporary authorisation form shows with captured AGC_Number.", ZFormModaliser.LastFormShownDialogForTest);

				authorisationUsage.AGC_Number = ZString.Empty;
				AssertHasNotifications("Prerequisite", authorisationUsage.AGC_NumberInfo);
				EmulateF5KeyDown(control);
				AssertType<CusAuthorisationForm>("A temporary authorisation form should show when AGC_Number is empty.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestF5KeyDownShowsDialogDependingCapturedAuthorisationUsageHasARelatedAuthorisation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorisationUsage.AGC_Number = "12345";
			authorisationUsage.AGC_OH_Owner = owner.PK;
			authorisationUsage.AGC_Code = "BLA";
			AssertNull(authorisationUsage.RelatedAuthorisationHeader);

			var authorisationHeader = Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345";
			authorisationHeader.CPH_Type = "BLA";
			authorisationHeader.CPH_OH_PermitHolder = owner.PK;
			Factory.Save();

			var config = GetEnableAdHocTrueConfiguration();
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				Env.Security.FlagAuthorisationAdHoc.IsAllowed = true;

				AssertNotNull("Prerequisite: a related authorization was found", authorisationUsage.RelatedAuthorisationHeader);
				EmulateF5KeyDown(control);
				AssertNull("No temporary authorisation form should show when captured authorization usage has a related authorization.", ZFormModaliser.LastFormShownDialogForTest);

				authorisationHeader.CPH_Number = "6789";
				AssertNull("Prerequisite: no related authorization", authorisationUsage.RelatedAuthorisationHeader);
				EmulateF5KeyDown(control);
				AssertType<CusAuthorisationForm>("A temporary authorisation form should show when captured authorization usage has norelated authorization.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestF5KeyDownShowsDialogIfSecurityAllowsForIt()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			var config = GetEnableAdHocTrueConfiguration();
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				Env.Security.FlagAuthorisationAdHoc.IsAllowed = false;
				EmulateF5KeyDown(control);
				AssertNull("No temporary authorisation form should show when security does't allow it.", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("The FlagAuthorisationAdHoc error message should show.", Env.Security.FlagAuthorisationAdHoc.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.FlagAuthorisationAdHoc.IsAllowed = true;
				EmulateF5KeyDown(control);
				AssertType<CusAuthorisationForm>("A temporary authorisation form can show only when security allows it.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[RequiresSTA]
		public void TestF5KeyDownShowsDialogDependingActiveControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			var config = GetEnableAdHocTrueConfiguration();
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetAuthorizationCodeAsActiveControl();
				control.SetAuthorizationCodeAsActiveControl();
				EmulateF5KeyDown(control);
				AssertNull("No temporary authorisation form should not show when AGC_CPH_Authorization is not the active control.", ZFormModaliser.LastFormShownDialogForTest);

				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				EmulateF5KeyDown(control);
				AssertType<CusAuthorisationForm>("A temporary authorisation form can show only when AGC_CPH_Authorization is the active control.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		static void EmulateF5KeyDown(EntryInstructionAuthorisationsUserControlForTest control)
		{
			KeySender.PostKeyDown(control.AuthorisationsGrid, control.AuthorisationsGrid.Handle, Keys.F5);
			Application.DoEvents();
		}

		public class EntryInstructionAuthorisationsUserControlForTest : EntryInstructionAuthorisationsUserControl
		{
			public new ZGrid AuthorisationsGrid => base.AuthorisationsGrid;

			public void SetAuthorizationAsActiveControl()
			{
				ActiveControl = AuthorisationsGrid.Controls.OfType<TextBox>().ToArray().First(x => x.Name == "AGC_CPH_Authorization");
			}

			public void SetAuthorizationCodeAsActiveControl()
			{
				ActiveControl = AuthorisationsGrid.Controls.OfType<TextBox>().ToArray().First(x => x.Name == "AGC_Code");
			}
		}

		class EntryInstructionAuthorisationsUserControlForAdditionalTest : EntryInstructionAuthorisationsUserControlForTest
		{
			protected override void ShowTemporaryAuthorisationForm(Customs.Business.CusAuthorisationHeader tempAuthorisation)
			{
				var factory = new BusinessObjectFactory();

				var orgHeader = factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "OWNER";
				factory.Save();

				tempAuthorisation.CPH_IsAdHoc = true;
				tempAuthorisation.CPH_Number = "12345";
				tempAuthorisation.CPH_OH_PermitHolder = orgHeader.PK;
				tempAuthorisation.CPH_Type = "BLA";
				tempAuthorisation.Factory.Save();
			}
		}

		static Hashtable GetEnableAdHocTrueConfiguration()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected().Setup<bool>("EnableAdHocCore").Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);
			return config;
		}
	}
}
#endif
