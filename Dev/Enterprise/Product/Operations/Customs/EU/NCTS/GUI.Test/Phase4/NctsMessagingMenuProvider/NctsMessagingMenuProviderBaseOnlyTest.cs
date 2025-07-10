using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	public sealed class NctsMessagingMenuProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCreateNewProvider()
		{
			var header = Factory.New<NctsHeader>();
			var helper = new NctsHeaderUniversalMessagingHelper();

			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				var provider = NctsMessagingMenuProvider.New(header, helper, nctsMovementForm);
				AssertType<NctsDepartureAndArrivalMovementMessagingMenuProvider>(provider);

				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				provider = NctsMessagingMenuProvider.New(header, helper, nctsMovementForm);
				AssertType<NctsArrivalMovementMessagingMenuProvider>(provider);

				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				provider = NctsMessagingMenuProvider.New(header, helper, nctsMovementForm);
				AssertType<NctsDepartureMovementMessagingMenuProvider>(provider);

				AssertNull("When NctsHeader parameter is null, result", NctsMessagingMenuProvider.New(null, helper, nctsMovementForm));
				AssertNull("When NctsHeaderUniversalMessagingHelper parameter is null, result", NctsMessagingMenuProvider.New(header, null, nctsMovementForm));
			}
		}

		public void TestPreSaveDeclaration()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();

			using (var nctsMovementForm = new ZForm(header))
			{
				var provider = new NctsMessagingMenuProviderForTest(header, nctsMovementForm);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = provider.PreSaveDeclarationExposed();

				AssertEquals("PreSaveDeclaration Result", true, result);
				AssertEquals("Ncts is saved", true, header.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				result = provider.PreSaveDeclarationExposed();
				AssertEquals("PreSaveDeclaration Result", true, result);
				AssertNull("No user message confirmation expected", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				header.BH_FTZMove = true;
				result = provider.PreSaveDeclarationExposed();
				AssertEquals("PreSaveDeclaration Result", false, result);
				AssertEquals("User message confirmation expected", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var providerWithNoParentForm = new NctsMessagingMenuProviderForTest(header, null);
			AssertExceptionThrown<InvalidOperationException>("Exception expected when provider does not have a valid Parent Form", () => providerWithNoParentForm.PreSaveDeclarationExposed());
		}

		class NctsMessagingMenuProviderForTest : NctsMessagingMenuProvider
		{
			public NctsMessagingMenuProviderForTest(NctsHeader header, ZForm parentForm) : base(header)
			{
				ParentForm = parentForm;
			}

			public override IEnumerable<ZMenuItem> CreateMenuItems() => Enumerable.Empty<ZMenuItem>();

			public bool PreSaveDeclarationExposed() => PreSaveDeclaration();
		}
	}
}
