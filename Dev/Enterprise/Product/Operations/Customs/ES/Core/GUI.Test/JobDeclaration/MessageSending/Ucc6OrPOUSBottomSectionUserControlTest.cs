using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class Ucc6OrPOUSBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestCancellationDropEdit()
		{
			AssertType<ZDropEdit>(control.CodeDropEdit);
		}

		public void TestCancellationTextBox()
		{
			AssertType<ZTextBox>(control.ReasonTextBox);
		}

		public void TestCancellationVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "A";
			var entryHeader = (Business.Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.ZG_UCC6Version = 1;

			var messageSendingObjectParent = new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				grid.ListManager.Position = 0;
				Application.DoEvents();
				var action = (JobDeclarationMessageSendingObject)grid.ListManager.Current;

				CombineAssertions(() =>
				{
					action.MessageType = "EDC";
					var cancellationGroupBox = form.Controls.Find("CancellationGroupBox", true)[0];
					AssertEquals("Cancellation GroupBox is visible when Message Type is EDC", true, cancellationGroupBox.Visible);

					action.MessageType = "EDP";
					cancellationGroupBox = form.Controls.Find("CancellationGroupBox", true)[0];
					AssertEquals("Cancellation GroupBox is not visible when Message Type is not EDC", false, cancellationGroupBox.Visible);
				});
			}
		}

		public void TestActivateByOperatorDropEdit()
		{
			AssertType<ZDropEdit>(control.ActivateByOperatorDropEdit);
		}

		public void TestActivateByOperatorVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "A";
			var entryHeader = (Business.Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.ZG_UCC6Version = 1;

			var messageSendingObjectParent = new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				grid.ListManager.Position = 0;
				Application.DoEvents();
				var action = (JobDeclarationMessageSendingObject)grid.ListManager.Current;

				CombineAssertions(() =>
				{
					action.MessageType = "DCP";
					var activateByOperatorDropEdit = form.Controls.Find("ActivateByOperatorDropEdit", true)[0];
					AssertEquals("ActivateByOperatorDropEdit is visible when Message Type is DCP", true, activateByOperatorDropEdit.Visible);

					action.MessageType = "EDC";
					activateByOperatorDropEdit = form.Controls.Find("ActivateByOperatorDropEdit", true)[0];
					AssertEquals("ActivateByOperatorDropEdit is not visible when Message Type is not DCP or DSP", false, activateByOperatorDropEdit.Visible);

					action.MessageType = "DSP";
					activateByOperatorDropEdit = form.Controls.Find("ActivateByOperatorDropEdit", true)[0];
					AssertEquals("ActivateByOperatorDropEdit is visible when Message Type is DSP", true, activateByOperatorDropEdit.Visible);

					action.MessageType = "EDX";
					activateByOperatorDropEdit = form.Controls.Find("ActivateByOperatorDropEdit", true)[0];
					AssertEquals("ActivateByOperatorDropEdit is not visible when Message Type is not DCP or DSP", false, activateByOperatorDropEdit.Visible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					activateByOperatorDropEdit = form.Controls.Find("ActivateByOperatorDropEdit", true)[0];
					AssertEquals("ActivateByOperatorDropEdit is not visible when is EXP", false, activateByOperatorDropEdit.Visible);
				});
			}
		}

		public void TestSecurityDropEdit()
		{
			AssertType<ZDropEdit>(control.SecurityDropEdit);
		}

		public void TestSecurityVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "A";
			var entryHeader = (Business.Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.ZG_UCC6Version = 1;

			var messageSendingObjectParent = new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				grid.ListManager.Position = 0;
				Application.DoEvents();
				var action = (JobDeclarationMessageSendingObject)grid.ListManager.Current;

				CombineAssertions(() =>
				{
					action.MessageType = "EDP";
					var securityDropEdit = form.Controls.Find("SecurityDropEdit", true)[0];
					AssertEquals("SecurityDropEdit is visible when Message Type is EDP", true, securityDropEdit.Visible);

					action.MessageType = "EDC";
					securityDropEdit = form.Controls.Find("SecurityDropEdit", true)[0];
					AssertEquals("SecurityDropEdit is not visible when Message Type is not EDP, PDE ro EDM", false, securityDropEdit.Visible);

					action.MessageType = "PDE";
					securityDropEdit = form.Controls.Find("SecurityDropEdit", true)[0];
					AssertEquals("SecurityDropEdit is visible when Message Type is PDE", true, securityDropEdit.Visible);

					action.MessageType = "EDX";
					securityDropEdit = form.Controls.Find("SecurityDropEdit", true)[0];
					AssertEquals("SecurityDropEdit is not visible when Message Type is not EDP, PDE ro EDM", false, securityDropEdit.Visible);

					action.MessageType = "EDM";
					securityDropEdit = form.Controls.Find("SecurityDropEdit", true)[0];
					AssertEquals("SecurityDropEdit is visible when Message Type is EDM", true, securityDropEdit.Visible);
				});
			}
		}

		public void TestRequestDispatchDropEdit()
		{
			AssertType<ZDropEdit>(control.RequestDispatchDropEdit);
		}

		public void TestRequestDispatchVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "A";
			var entryHeader = (Business.Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.ZG_UCC6Version = 1;
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);

			var messageSendingObjectParent = new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				grid.ListManager.Position = 0;
				Application.DoEvents();
				var action = (JobDeclarationMessageSendingObject)grid.ListManager.Current;

				CombineAssertions(() =>
				{
					action.MessageType = "EDA";
					var requestDispatchDropEdit = form.Controls.Find("RequestDispatchDropEdit", true)[0];
					AssertEquals("RequestDispatchDropEdit is visible when Message Type is EDA", true, requestDispatchDropEdit.Visible);

					action.MessageType = "EDC";
					requestDispatchDropEdit = form.Controls.Find("RequestDispatchDropEdit", true)[0];
					AssertEquals("RequestDispatchDropEdit is not visible when Message Type is not EDA", false, requestDispatchDropEdit.Visible);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Ucc6OrPOUSBottomSectionUserControl();
		}
		Ucc6OrPOUSBottomSectionUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
