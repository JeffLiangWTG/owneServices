using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	abstract class DependencyMenuItemBaseTest : WorkflowRelationshipMenuItemBaseTest
	{
		protected sealed override ZToolStripMenuItem GetRootMenuItem_StandardDirection()
		{
			return GetMenuItem(workflow2_2, "Pre-requisite Workflows");
		}

		protected sealed override ZToolStripMenuItem GetRootMenuItem_OppositeDirection()
		{
			return GetMenuItem(workflow1_1, "Dependent Workflows");
		}
		protected override string LinkTypeUnderTest => ProcessHeaderLinkTypeList.Codes.Dependency;
		protected override string ConflictingLinkType => ProcessHeaderLinkTypeList.Codes.ParentChild;
		protected override string OpenMenuItemName_StandardDirection => "Open Pre-requisite Item";
		protected override string OpenMenuItemName_OppositeDirection => "Open Dependent Item";
		protected override string EditMenuItemName => "Edit Dependency Link";

		protected override string IndirectRelationshipName_StandardDirection => "pre-requisite";
		protected override string IndirectRelationshipName_OppositeDirection => "dependency";

		protected override ProcessHeader InvalidRecordForStandardDirectionLinking => jobHeader2;
		protected override ProcessHeader InvalidRecordForOppositeDirectionLinking => jobHeader1;

		protected override IEnumerable<ProcessHeader> ValidRecordsForStandardDirectionLinking
		{
			get
			{
				yield return jobHeader1;
				yield return workflow1_1;
			}
		}

		protected override IEnumerable<ProcessHeader> ValidRecordsForOppositeDirectionLinking
		{
			get
			{
				yield return jobHeader2;
				yield return workflow2_2;
			}
		}

		protected override string InvalidLink_ImmediateErrorMessage_StandardDirection => null;

		protected override IEnumerable<string> InvalidLink_ValidationErrorMessage_StandardDirection => new[] {
			@"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!
Organization (WRONGSYD) - Job Organization (H5ZX52PAMCOI) is complete.
Organization (WRONGSYD) - The bend and snap",
			@"This is a dependency link between workflows that are also involved in a Parent-Child relationship."
		};

		protected override string InvalidLink_ImmediateErrorMessage_OppositeDirection => null;

		protected override IEnumerable<string> InvalidLink_ValidationErrorMessage_OppositeDirection => new[] {
			@"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - Job Organization (XVBQP68SIYXQ) is complete.
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!
Organization (WRONGSYD) - The bend and snap",
			@"This is a dependency link between workflows that are also involved in a Parent-Child relationship."
		};
	}

	abstract class ParentChildMenuItemBaseTest : WorkflowRelationshipMenuItemBaseTest
	{
		protected sealed override ZToolStripMenuItem GetRootMenuItem_StandardDirection()
		{
			return GetMenuItem(workflow2_2, "Child Workflows");
		}

		protected sealed override ZToolStripMenuItem GetRootMenuItem_OppositeDirection()
		{
			return GetMenuItem(workflow1_1, "Parent Workflows");
		}

		protected override string LinkTypeUnderTest => ProcessHeaderLinkTypeList.Codes.ParentChild;
		protected override string ConflictingLinkType => ProcessHeaderLinkTypeList.Codes.Dependency;
		protected override string OpenMenuItemName_StandardDirection => "Open Child Item";
		protected override string OpenMenuItemName_OppositeDirection => "Open Parent Item";
		protected override string EditMenuItemName => "Edit Parent-Child Link";

		protected override string IndirectRelationshipName_StandardDirection => "child";
		protected override string IndirectRelationshipName_OppositeDirection => "parent";

		protected override ProcessHeader InvalidRecordForStandardDirectionLinking => workflow1_2;
		protected override ProcessHeader InvalidRecordForOppositeDirectionLinking => workflow2_1;

		protected override IEnumerable<ProcessHeader> ValidRecordsForStandardDirectionLinking
		{
			get
			{
				yield return jobHeader1;
			}
		}

		protected override IEnumerable<ProcessHeader> ValidRecordsForOppositeDirectionLinking
		{
			get
			{
				yield return jobHeader2;
			}
		}

		protected override void AssertOppositeDirectionWorkflowsNowLinked()
		{
			AssertMultilineASCIIEquals("Workflows can only have one parent, so adding a new parent is not valid.",
				@"This link is invalid so cannot be added.

Error - FP_FH_HeaderFrom: The 'from' workflow already has a parent.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertIsNowLinkedAndPersistedAfterSaved(workflow1_1, workflow3, shouldBeLinked: false, shouldBePersisted: false);
		}

		protected override string InvalidLink_ImmediateErrorMessage_StandardDirection => @"This link is invalid so cannot be added.

Error - FP_FH_HeaderFrom: The 'from' workflow already has a parent.
Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.";

		protected override IEnumerable<string> InvalidLink_ValidationErrorMessage_StandardDirection
		{
			get
			{
				return new[] { @"The 'from' workflow already has a parent.",
					@"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!",
					@"These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow." };
			}
		}

		protected override string InvalidLink_ImmediateErrorMessage_OppositeDirection => @"This link is invalid so cannot be added.

Error - FP_FH_HeaderFrom: The 'from' workflow already has a parent.
Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.";

		protected override IEnumerable<string> InvalidLink_ValidationErrorMessage_OppositeDirection
		{
			get
			{
				return new[] { @"The 'from' workflow already has a parent.",
					@"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - The bend and snap",
					@"Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow." };
			}
		}
	}

	abstract class WorkflowRelationshipMenuItemBaseTest : BMSGUITestCase
	{
		#region Menu Structure

		public void TestMenuStructure_StandardDirection()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();

			AssertEquals(5, rootMenuItem.DropDownItems.Count);

			AssertEquals("Add new...", rootMenuItem.DropDownItems[0].Text);
			AssertType<ToolStripSeparator>(rootMenuItem.DropDownItems[1]);
			AssertEquals("Remove", rootMenuItem.DropDownItems[2].Text);
			AssertEquals(OpenMenuItemName_StandardDirection, rootMenuItem.DropDownItems[3].Text);
			AssertEquals(EditMenuItemName, rootMenuItem.DropDownItems[4].Text);

			AssertSubMenuStructure_StandardDirection((ZToolStripMenuItem)rootMenuItem.DropDownItems[2]);
			AssertSubMenuStructure_StandardDirection((ZToolStripMenuItem)rootMenuItem.DropDownItems[3]);
			AssertSubMenuStructure_StandardDirection((ZToolStripMenuItem)rootMenuItem.DropDownItems[4]);
		}

		public void TestMenuStructure_OppositeDirection()
		{
			var rootMenuItem = GetRootMenuItem_OppositeDirection();

			AssertEquals(5, rootMenuItem.DropDownItems.Count);

			AssertEquals("Add new...", rootMenuItem.DropDownItems[0].Text);
			AssertType<ToolStripSeparator>(rootMenuItem.DropDownItems[1]);
			AssertEquals("Remove", rootMenuItem.DropDownItems[2].Text);
			AssertEquals(OpenMenuItemName_OppositeDirection, rootMenuItem.DropDownItems[3].Text);
			AssertEquals(EditMenuItemName, rootMenuItem.DropDownItems[4].Text);

			AssertSubMenuStructure_OppositeDirection((ZToolStripMenuItem)rootMenuItem.DropDownItems[2]);
			AssertSubMenuStructure_OppositeDirection((ZToolStripMenuItem)rootMenuItem.DropDownItems[3]);
			AssertSubMenuStructure_OppositeDirection((ZToolStripMenuItem)rootMenuItem.DropDownItems[4]);
		}

		#endregion

		#region Add New

		#region Standard Direction

		public void TestAddNewMenuItem_StandardDirection_WhenInvalidRelationshipChosen()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var addMenuItem = rootMenuItem.DropDownItems[0];

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(InvalidRecordForStandardDirectionLinking))
			{
				PerformClick(addMenuItem);

				if (InvalidLink_ImmediateErrorMessage_StandardDirection == null)
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertIsNowLinked(jobHeader2, workflow2_2, true);

					var link = GetLink(Factory, jobHeader2, workflow2_2);
					AssertNotNull(link);
					Assert(workflow2_2.IsRegisteredEditableChildObject(link));

					workflow2_2.RunPreSaveValidation(); // workflow2_2 is the wokflow the operation is performed on

					AssertLinkHasErrors(jobHeader2, workflow2_2, InvalidLink_ValidationErrorMessage_StandardDirection);
					AssertIsPersistedAfterSaved(jobHeader2, workflow2_2, false);
				}
				else
				{
					AssertMultilineASCIIEquals(InvalidLink_ImmediateErrorMessage_StandardDirection, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertIsNowLinkedAndPersistedAfterSaved(jobHeader2, workflow2_2, shouldBeLinked: false, shouldBePersisted: false);
				}
			}
		}

		public void TestAddNewMenuItem_StandardDirection_WhenValidRelationshipChosen()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var addMenuItem = rootMenuItem.DropDownItems[0];

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow3))
			{
				var startingPrereqCount = workflow2_2.NumberOfOpenPrerequisitesUpTheTree;

				PerformClick(addMenuItem);

				if (LinkTypeUnderTest == ProcessHeaderLinkTypeList.Codes.Dependency && ShouldUpdateNumberOfOpenPrereqsAfterActions)
				{
					AssertEquals(startingPrereqCount + 1, workflow2_2.NumberOfOpenPrerequisitesUpTheTree);
				}

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsNowLinkedAndPersistedAfterSaved(workflow3, workflow2_2, shouldBeLinked: true, shouldBePersisted: true);
			}
		}

		public void TestAddNewMenuItem_StandardDirection_WhenMultipleValidRelationshipChosen()
		{
			invalidLink1.Delete();
			invalidLink2.Delete();

			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var addMenuItem = rootMenuItem.DropDownItems[0];
			var validRecords = ValidRecordsForStandardDirectionLinking.ToArray();

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(validRecords))
			{
				PerformClick(addMenuItem);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsNowLinkedAndPersistedAfterSaved(validRecords.First(), workflow2_2, shouldBeLinked: true, shouldBePersisted: true);

				foreach (var additionalLinkedRecord in validRecords.Skip(1))
				{
					AssertIsNowLinked(additionalLinkedRecord, workflow2_2, true);
				}
			}
		}

		#endregion

		#region Opposite Direction

		public void TestAddNewMenuItem_OppositeDirection_WhenInvalidRelationshipChosen()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var rootMenuItem = GetRootMenuItem_OppositeDirection();
			var addMenuItem = rootMenuItem.DropDownItems[0];

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(InvalidRecordForOppositeDirectionLinking))
			{
				PerformClick(addMenuItem);

				if (InvalidLink_ImmediateErrorMessage_OppositeDirection == null)
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertIsNowLinked(workflow1_1, jobHeader1, true);

					var link = GetLink(Factory, workflow1_1, jobHeader1);
					AssertNotNull(link);
					Assert(workflow1_1.IsRegisteredEditableChildObject(link));

					workflow1_1.RunPreSaveValidation(); // workflow1_1 is the wokflow the operation is performed on

					AssertLinkHasErrors(workflow1_1, jobHeader1, InvalidLink_ValidationErrorMessage_OppositeDirection);
					AssertIsPersistedAfterSaved(workflow1_1, jobHeader1, false);
				}
				else
				{
					AssertMultilineASCIIEquals(InvalidLink_ImmediateErrorMessage_OppositeDirection, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertIsNowLinkedAndPersistedAfterSaved(workflow1_1, jobHeader1, shouldBeLinked: false, shouldBePersisted: false);
				}
			}
		}

		public void TestAddNewMenuItem_OppositeDirection_WhenValidRelationshipChosen()
		{
			invalidLink1.Delete();
			invalidLink2.Delete();

			var rootMenuItem = GetRootMenuItem_OppositeDirection();
			var addMenuItem = rootMenuItem.DropDownItems[0];

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow3))
			{
				PerformClick(addMenuItem);

				AssertOppositeDirectionWorkflowsNowLinked();
			}
		}

		protected virtual void AssertOppositeDirectionWorkflowsNowLinked()
		{
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertIsNowLinkedAndPersistedAfterSaved(workflow1_1, workflow3, shouldBeLinked: true, shouldBePersisted: true);
		}

		public void TestAddNewMenuItem_OppositeDirection_WhenMultipleValidRelationshipChosen()
		{
			invalidLink1.Delete();
			invalidLink2.Delete();

			var rootMenuItem = GetRootMenuItem_OppositeDirection();
			var addMenuItem = rootMenuItem.DropDownItems[0];
			var validRecords = ValidRecordsForOppositeDirectionLinking.ToArray();

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(validRecords))
			{
				PerformClick(addMenuItem);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsNowLinkedAndPersistedAfterSaved(workflow1_1, validRecords.First(), shouldBeLinked: true, shouldBePersisted: true);

				foreach (var additionalLinkedRecord in validRecords.Skip(1))
				{
					AssertIsNowLinked(workflow1_1, additionalLinkedRecord, true);
				}
			}
		}

		#endregion

		protected abstract ProcessHeader InvalidRecordForStandardDirectionLinking { get; }
		protected abstract ProcessHeader InvalidRecordForOppositeDirectionLinking { get; }

		protected abstract IEnumerable<ProcessHeader> ValidRecordsForStandardDirectionLinking { get; }
		protected abstract IEnumerable<ProcessHeader> ValidRecordsForOppositeDirectionLinking { get; }

		protected abstract string InvalidLink_ImmediateErrorMessage_StandardDirection { get; }

		protected abstract IEnumerable<string> InvalidLink_ValidationErrorMessage_StandardDirection { get; }

		protected abstract string InvalidLink_ImmediateErrorMessage_OppositeDirection { get; }

		protected abstract IEnumerable<string> InvalidLink_ValidationErrorMessage_OppositeDirection { get; }

		#endregion

		#region Remove Relationship
		public void TestRemoveMenuItem_StandardDirection()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var removeMenuItem = (ZToolStripMenuItem)rootMenuItem.DropDownItems[2];

			PerformClick(removeMenuItem.DropDownItems[0]);
			AssertLinkDeleted(link21_22, true);
			AssertLinkDeleted(link12_21, false);
			AssertLinkDeleted(link11_12, false);

			AssertMultilineASCIIEquals("", @"Would you like to delete this link?

From:	Organization (WRONGSYD) - The bend and snap
To:	Organization (WRONGSYD) - it works every time!", UnitTestUserNotification.Instance.LastMessage.Text);

			PerformClick(removeMenuItem.DropDownItems[2]);
			AssertLinkDeleted(link12_21, true);
			AssertLinkDeleted(link11_12, false);

			AssertMultilineASCIIEquals("", @"This is an indirect relationship to a different workflow. Would you like to delete this link anyway?

From:	Organization (DIDNOTHING) - and snap!
To:	Organization (WRONGSYD) - The bend and snap", UnitTestUserNotification.Instance.LastMessage.Text);

			PerformClick(removeMenuItem.DropDownItems[3]);
			AssertLinkDeleted(link11_12, true);

			AssertMultilineASCIIEquals("", @"This is an indirect relationship to a different workflow. Would you like to delete this link anyway?

From:	Organization (DIDNOTHING) - You bend...
To:	Organization (DIDNOTHING) - and snap!", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRemoveMenuItem_OppositeDirection()
		{
			var rootMenuItem = GetRootMenuItem_OppositeDirection();
			var removeMenuItem = (ZToolStripMenuItem)rootMenuItem.DropDownItems[2];

			PerformClick(removeMenuItem.DropDownItems[0]);
			AssertLinkDeleted(link11_12, true);
			AssertLinkDeleted(link12_21, false);
			AssertLinkDeleted(link21_22, false);

			AssertMultilineASCIIEquals("", @"Would you like to delete this link?

From:	Organization (DIDNOTHING) - You bend...
To:	Organization (DIDNOTHING) - and snap!", UnitTestUserNotification.Instance.LastMessage.Text);

			PerformClick(removeMenuItem.DropDownItems[2]);
			AssertLinkDeleted(link12_21, true);
			AssertLinkDeleted(link21_22, false);

			AssertMultilineASCIIEquals("", @"This is an indirect relationship to a different workflow. Would you like to delete this link anyway?

From:	Organization (DIDNOTHING) - and snap!
To:	Organization (WRONGSYD) - The bend and snap", UnitTestUserNotification.Instance.LastMessage.Text);

			PerformClick(removeMenuItem.DropDownItems[3]);
			AssertLinkDeleted(link21_22, true);

			AssertMultilineASCIIEquals("", @"This is an indirect relationship to a different workflow. Would you like to delete this link anyway?

From:	Organization (WRONGSYD) - The bend and snap
To:	Organization (WRONGSYD) - it works every time!", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Open

		public void TestOpenMenuItem()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var openMenuItem = (ZToolStripMenuItem)rootMenuItem.DropDownItems[3];

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNull(form);
			}

			PerformClick(openMenuItem.DropDownItems[3]);

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				Application.DoEvents();

				AssertNotNull(form);
				AssertEquals("DIDNOTHING", form.Organisation.OH_Code);

				var workflowsGrid = form.FindAll<WorkflowsUserControl>().Single().WorkflowsGrid;

				AssertSamePK(workflow1_1, workflowsGrid.GetCurrent());

				PerformClick(openMenuItem.DropDownItems[2]);
				Application.DoEvents();

				AssertSamePK(workflow1_2, workflowsGrid.GetCurrent());
			}
		}

		public void TestOpenMenuItem_WhenNoSecurity()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var openMenuItem = (ZToolStripMenuItem)rootMenuItem.DropDownItems[3];

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNull(form);
			}

			Env.Security.OrganisationCRMSecurity.DisableCRMSecurityForTesting(false);
			Env.Security.OrganisationModify.IsAllowed = false;
			Env.Security.OrganisationView.IsAllowed = false;

			PerformClick(openMenuItem.DropDownItems[3]);

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNull(form);
			}

			AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> View", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Edit

		public void TestEditMenuItem()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var editMenuItem = (ZToolStripMenuItem)rootMenuItem.DropDownItems[4];

			var form = Application.OpenForms.OfType<ProcessHeaderLinkForm>().SingleOrDefault();

			AssertNull(form);
			PerformClick(editMenuItem.DropDownItems[3]);

			using (form = Application.OpenForms.OfType<ProcessHeaderLinkForm>().SingleOrDefault())
			{
				AssertNotNull(form);

				var link = (ProcessHeaderLink)form.DataSource;
				AssertEquals("You bend... (DIDNOTHING)", link.FromHeaderDescription);
				AssertEquals("and snap! (DIDNOTHING)", link.ToHeaderDescription);
			}
		}

		public void TestEditMenuItem_WhenNoSecurity()
		{
			var rootMenuItem = GetRootMenuItem_StandardDirection();
			var editMenuItem = (ZToolStripMenuItem)rootMenuItem.DropDownItems[4];

			using (var form = Application.OpenForms.OfType<ProcessHeaderLinkForm>().SingleOrDefault())
			{
				AssertNull(form);
			}

			Env.Security.WorkflowDependenciesEdit.IsAllowed = false;
			Env.Security.WorkflowDependenciesView.IsAllowed = false;

			PerformClick(editMenuItem.DropDownItems[3]);

			using (var form = Application.OpenForms.OfType<ProcessHeaderLinkForm>().SingleOrDefault())
			{
				AssertNull(form);
			}

			AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Workflow & Process -> Job Workflows -> Workflow Dependencies -> View", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Assertions

		void AssertSubMenuStructure_StandardDirection(ZToolStripMenuItem menuItem)
		{
			AssertEquals(4, menuItem.DropDownItems.Count);
			AssertStartsWith("", "Organization (WRONGSYD) - The bend and snap", menuItem.DropDownItems[0].Text);
			AssertType<ToolStripSeparator>(menuItem.DropDownItems[1]);
			AssertStartsWith("", "Organization (DIDNOTHING) - and snap!", menuItem.DropDownItems[2].Text);
			AssertEquals($"Organization (DIDNOTHING) - You bend... [indirect {IndirectRelationshipName_StandardDirection}]", menuItem.DropDownItems[3].Text);
		}

		void AssertSubMenuStructure_OppositeDirection(ZToolStripMenuItem menuItem)
		{
			AssertEquals(4, menuItem.DropDownItems.Count);
			AssertStartsWith("", "Organization (DIDNOTHING) - and snap!", menuItem.DropDownItems[0].Text);
			AssertType<ToolStripSeparator>(menuItem.DropDownItems[1]);
			AssertEquals($"Organization (WRONGSYD) - The bend and snap [indirect {IndirectRelationshipName_OppositeDirection}]", menuItem.DropDownItems[2].Text);
			AssertEquals($"Organization (WRONGSYD) - it works every time! [indirect {IndirectRelationshipName_OppositeDirection}]", menuItem.DropDownItems[3].Text);
		}

		protected abstract string IndirectRelationshipName_StandardDirection { get; }
		protected abstract string IndirectRelationshipName_OppositeDirection { get; }

		void AssertLinkDeleted(ProcessHeaderLink link, bool deleted)
		{
			AssertEquals($"The link should{(deleted ? string.Empty : "not")} be deleted", deleted, link.IsDeleted);

			var newFactory = Factory.CreateNewFactory();
			var loadedLink = newFactory.Load<ProcessHeaderLink>(link.PK);

			if (ShouldSaveAfterActions)
			{
				if (deleted)
				{
					AssertNull(loadedLink);
				}
				else
				{
					AssertNotNull(loadedLink);
				}
			}
			else
			{
				AssertNotNull(loadedLink);

				Factory.Save();
				AssertEquals(deleted, loadedLink.IsDeleted);
			}
		}

		protected void AssertIsNowLinked(ProcessHeader from, ProcessHeader to, bool shouldBeLinked)
		{
			if (shouldBeLinked)
			{
				if (LinkTypeUnderTest == ProcessHeaderLinkTypeList.Codes.Dependency)
				{
					AssertIsPrerequisite(from, to);
				}
				else
				{
					AssertIsParent(from, to);
				}
			}
			else
			{
				if (LinkTypeUnderTest == ProcessHeaderLinkTypeList.Codes.Dependency)
				{
					AssertIsNotPrerequisite(from, to);
				}
				else
				{
					AssertIsNotParent(from, to);
				}
			}
		}

		protected void AssertIsPersistedAfterSaved(ProcessHeader from, ProcessHeader to, bool shouldBePersisted)
		{
			var newFactory = Factory.CreateNewFactory();
			var loadedLink = GetLink(newFactory, from, to);

			if (ShouldSaveAfterActions)
			{
				if (shouldBePersisted)
				{
					AssertNotNull(loadedLink);
				}
				else
				{
					AssertNull(loadedLink);
				}
			}
			else
			{
				AssertNull(loadedLink);

				if (shouldBePersisted)
				{
					Factory.Save();
					loadedLink = GetLink(newFactory, from, to);

					AssertNotNull(loadedLink);
				}
			}
		}

		protected void AssertIsNowLinkedAndPersistedAfterSaved(ProcessHeader from, ProcessHeader to, bool shouldBeLinked, bool shouldBePersisted)
		{
			AssertIsNowLinked(from, to, shouldBeLinked);
			AssertIsPersistedAfterSaved(from, to, shouldBePersisted);
		}

		protected void AssertLinkHasErrors(ProcessHeader from, ProcessHeader to, IEnumerable<string> expectedErrors)
		{
			var link = GetLink(Factory, from, to);
			link.RunPreSaveValidation();

			foreach (var expectedError in expectedErrors)
			{
				AssertHasError(link.FP_FH_HeaderFromInfo, expectedError);
				AssertHasError(link.FP_FH_HeaderToInfo, expectedError);
			}
		}

		protected ProcessHeaderLink GetLink(BusinessObjectFactory factory, ProcessHeader from, ProcessHeader to)
		{
			var query = new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, from.PK);
			query.AddToFilter(ProcessHeaderLinkSchema.FP_FH_HeaderTo, to.PK);
			query.AddToFilter(ProcessHeaderLinkSchema.FP_LinkType, LinkTypeUnderTest);

			return factory.LoadTop1<ProcessHeaderLink>(query);
		}

		#endregion

		#region Implementation

		void PerformClick(ToolStripItem menuItem)
		{
			menuItem.PerformClick();
		}

		static ProcessHeaderLink CreateLink(ProcessHeader headerFrom, ProcessHeader headerTo, string linkType)
		{
			switch (linkType)
			{
				case ProcessHeaderLinkTypeList.Codes.Dependency:
					return headerFrom.GetOrCreateDependencyLink(headerTo);

				case ProcessHeaderLinkTypeList.Codes.ParentChild:
					return headerFrom.GetOrCreateLinkToParent(headerTo);

				default:
					throw new ArgumentException("Invalid argument", nameof(linkType));
			}
		}

		protected virtual bool ShouldUpdateNumberOfOpenPrereqsAfterActions => true;

		protected abstract bool ShouldSaveAfterActions { get; }

		protected abstract ZToolStripMenuItem GetRootMenuItem_StandardDirection();
		protected abstract ZToolStripMenuItem GetRootMenuItem_OppositeDirection();

		protected abstract ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName);

		protected abstract string LinkTypeUnderTest { get; }
		protected abstract string ConflictingLinkType { get; }

		protected abstract string OpenMenuItemName_StandardDirection { get; }
		protected abstract string OpenMenuItemName_OppositeDirection { get; }
		protected abstract string EditMenuItemName { get; }

		protected override void SetUp()
		{
			base.SetUp();

			disposables = new DisposableList(10);

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			jobHeader1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader3.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			((OrgHeader)jobHeader1.Parent).OH_Code = "DIDNOTHING";
			((OrgHeader)jobHeader2.Parent).OH_Code = "WRONGSYD";

			workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "You bend...");
			workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "and snap!");

			workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "The bend and snap");
			workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "it works every time!");

			workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Whoever said orange was the new pink was seriously disturbed");

			BMSTestHelper.CreateTask(workflow1_1);
			BMSTestHelper.CreateTask(workflow1_2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			BMSTestHelper.CreateTask(workflow2_1);
			BMSTestHelper.CreateTask(workflow2_2);
			BMSTestHelper.CreateTask(workflow3);

			link11_12 = CreateLink(workflow1_1, workflow1_2, LinkTypeUnderTest);
			link12_21 = CreateLink(workflow1_2, workflow2_1, LinkTypeUnderTest);
			link21_22 = CreateLink(workflow2_1, workflow2_2, LinkTypeUnderTest);
			invalidLink1 = CreateLink(workflow2_2, workflow1_2, ConflictingLinkType);
			invalidLink2 = CreateLink(workflow1_1, workflow2_1, ConflictingLinkType);

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		protected VisualBoardTestConfig config;

		protected ProcessJobHeader jobHeader1, jobHeader2, jobHeader3;
		protected ProcessHeader workflow1_1, workflow1_2, workflow2_1, workflow2_2, workflow3;
		protected ProcessHeaderLink link11_12, link12_21, link21_22;
		protected ProcessHeaderLink invalidLink1, invalidLink2;

		protected DisposableList disposables;

		#endregion
	}
}
