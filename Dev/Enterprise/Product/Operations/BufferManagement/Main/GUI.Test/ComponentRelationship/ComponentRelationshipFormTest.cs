using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ComponentRelationshipForm))]
	class ComponentRelationshipFormTest : ZFormBasherTest
	{
		public void TestIsActiveControl_HasDescriptionText()
		{
			var componentRelationship = CreateALargeComponentRelationship();

			using (var form = new ComponentRelationshipForm(componentRelationship))
			{
				form.Show();
				form.FindSingle<ZTabPage>(p => p.Text == "Details").Select();
				Application.DoEvents();

				var isActiveCheckBox = form.FindAll<ZCheckBox>().First();
				var mainStatusBar = form.FindAll<ZStatusBar>().First();

				isActiveCheckBox.Select();

				form.Show();
				Application.DoEvents();

				AssertEquals("The description text in the main panel should explain the IsActive window", "Indicates whether this component relationship is available for use.", mainStatusBar.Panels[0].Text);
				AssertEquals("Is Active", isActiveCheckBox.Text);
			}
		}

		public void TestNameControl_HasDescriptionText()
		{
			var componentRelationship = CreateALargeComponentRelationship();

			using (var form = new ComponentRelationshipForm(componentRelationship))
			{
				form.Show();
				form.FindSingle<ZTabPage>(p => p.Text == "Details").Select();
				Application.DoEvents();

				var nameTextBox = form.FindAll<ZTextBox>().First();
				var mainStatusBar = form.FindAll<ZStatusBar>().First();

				nameTextBox.Select();

				form.Show();
				Application.DoEvents();

				AssertEquals("The description text in the main panel should explain the IsActive window", "Relationship Name", mainStatusBar.Panels[0].Text);
			}
		}

		public void TestSupportsEDocs()
		{
			var componentRelationship = CreateALargeComponentRelationship();

			using (var form = new ComponentRelationshipForm(componentRelationship))
			{
				form.Show();
				AssertNotNull(form.FindSingle<ZTabPage>(p => p.Text == "eDocs"));
			}
		}

		public void TestDbHits()
		{
			var newFactory = Factory.CreateNewFactory();

			var componentRelationship = CreateALargeComponentRelationship();
			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 2 },
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ StmDataSchema.Constants.TableName, 2 },
				{ RefDataGroupingSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true, thresholdForUnspecified: 5, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				var loadedComponentRelationship = newFactory.Load<ComponentRelationship>(componentRelationship.PK);

				using (var form = new ComponentRelationshipForm(loadedComponentRelationship))
				{
					form.Show();

					form.FindSingle<ZTabPage>(p => p.Text == "Details").Select();

					Application.DoEvents();
				}
			}
		}

		public void TestDbHitsValidateAll()
		{
			var newFactory = Factory.CreateNewFactory();

			var componentRelationship = CreateALargeComponentRelationship();
			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
			};

			var loadedComponentRelationship = newFactory.Load<ComponentRelationship>(componentRelationship.PK);

			using (var form = new ComponentRelationshipForm(loadedComponentRelationship))
			{
				form.Show();

				form.FindSingle<ZTabPage>(p => p.Text == "Details").Select();

				Application.DoEvents();

				var validateAllMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[2];
				AssertEquals("&Validate All", validateAllMenuItem.Text);

				using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true, thresholdForUnspecified: 5, ignoreHitsFromTablesCachedInUberFactory: true))
				{
					validateAllMenuItem.PerformClick();

					Application.DoEvents();
				}
			}
		}

		ComponentRelationship CreateALargeComponentRelationship()
		{
			const int numOfSystem = 5;
			const int numOfComponent = 15;

			var componentRelationship = Factory.NewWithValidTestData<ComponentRelationship>();
			var componentRelationshipLinks = componentRelationship.RelatedComponentLinks;

			for (var i = 0; i < numOfSystem; i++)
			{
				var system = BMSTestHelper.CreateSystem(Factory);
				for (var j = 0; j < numOfComponent / numOfSystem; ++j)
				{
					var component = BMSTestHelper.CreateBuffer(system, "Component " + j);
					var link = componentRelationshipLinks.AddNew();

					link.FL_FC_ComponentTo = component.PK;
				}
			}

			return componentRelationship;
		}

		protected override Form GetFormToBashCore()
		{
			return new ComponentRelationshipForm(Factory.New<ComponentRelationship>());
		}
	}
}
