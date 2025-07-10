using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Utils;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	class EDIMenuConsolidationTest : Customs.GUI.Testing.EDIMenuConsolidationTest<EDIMenuConsolidationTest.ConsolidationTestMenu, JobDeclaration>
	{
		protected override ConsolidationTestMenu GetEdiMenu => new ConsolidationTestMenu();

		protected override JobDeclaration GetDeclaration
		{
			get
			{
				var declaration = JobDeclarationTest.CreateSendableDeclaration(Factory);
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				return declaration;
			}
		}

		protected override Func<ConsolidationTestMenu, IEnumerable<MenuItem>> QueuedForConsolidationDisablesMenuItems => SendingMenuItemsFunc;

		protected override Func<ConsolidationTestMenu, IEnumerable<MenuItem>> QueuedForConsolidationPromptsOnSubmitMenuItems => SendingMenuItemsFunc;

		Func<ConsolidationTestMenu, IEnumerable<MenuItem>> SendingMenuItemsFunc => menu => new[]
		{
			menu.CMRSendPreLodgeMenuItem, menu.CMRSendLodgeWithPayMenuItem,
			menu.CMRSendLodgeWithoutPayMenuItem, menu.SendPaymentMenuItem,
			menu.CMRSendAmendmentMenuItem, menu.sendWithdrawalMenuItem, menu.throwAwayMergedLinesMenuItem
		};

		protected override Func<JobDeclaration, IList> GetMergedLinesFunc => (dec => dec.CustomsEntryHeaders.SelectMany(x => x.MergedLines.Cast<CusEntryLine>()).ToList());
		protected override Action<JobDeclaration> MakeDeclarationMessageErrorAction => jobDeclaration => jobDeclaration.JE_RL_NKPortOfArrival = "AUXXX";
		protected override string MessageError => "Port Of Arrival: This port code is invalid.";
		protected override Action<ConsolidationTestMenu> SubmitDeclarationClick => menu => menu.CMRSendPreLodgeMenuItem.PerformClick();
		protected override Action<ConsolidationTestMenu, bool> SetRefuseLockForTesting => (menu, value) => menu.ConsolidatedEntryMenuProvider.RefuseLockForTesting = value;
		protected override bool SupportsRemoveFromConsolidation => true;

		public void TestQueueForConsolidationShowsOnlyForValidDecTypes()
		{
			var declaration = GetDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new ConsolidationTestMenu())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				Assert("Pre-condition", !queueForConsolidationMenuItem.Visible);

				menu.Declaration = declaration;
				Assert("Queue for Consolidation menu option should be visible", queueForConsolidationMenuItem.Visible);

				declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
				menu.RefreshMenu();
				Assert("Queue for Consolidation menu option should not be visible for this message type", !queueForConsolidationMenuItem.Visible);

				declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
				menu.RefreshMenu();
				Assert("Queue for Consolidation menu option should also not be visible for this message type", !queueForConsolidationMenuItem.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
				menu.RefreshMenu();
				Assert("Queue for Consolidation menu option should not be visible for this declaration type", !queueForConsolidationMenuItem.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
				menu.RefreshMenu();
				Assert("Queue for Consolidation menu option should be visible/enabled again", queueForConsolidationMenuItem.Visible);
			}
		}

		public sealed class ConsolidationTestMenu : EDIMenu
		{
			protected override ConsolidatedEntryMenuProvider GetConsolidatedEntryMenuProvider() => new ConsolidationMenuProviderForTest(Form);
			public new ConsolidationMenuProviderForTest ConsolidatedEntryMenuProvider => (ConsolidationMenuProviderForTest)base.ConsolidatedEntryMenuProvider;
		}

		public sealed class ConsolidationMenuProviderForTest : AUConsolidatedEntryMenuProvider
		{
			public ConsolidationMenuProviderForTest(ZForm parentForm) : base(parentForm)
			{
			}

			public bool RefuseLockForTesting { get; set; }

			protected override SqlApplicationLock LockDeclarationForConsolidation() => RefuseLockForTesting ? null : base.LockDeclarationForConsolidation();
		}
	}
}
