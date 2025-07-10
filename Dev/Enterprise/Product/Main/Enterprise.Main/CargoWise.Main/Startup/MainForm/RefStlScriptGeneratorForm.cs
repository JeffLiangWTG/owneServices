using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.MainForm
{
	internal class StlViewer
	{
		public StlViewer(IStlScript script)
		{
			this.Script = script;
		}

		public IStlScript Script { get; private set; }

		public override string ToString()
		{
			return Script.Name;
		}

		public override bool Equals(object obj)
		{
			return (obj is StlViewer scriptViewer) && scriptViewer.ToString().Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode() => ToString().GetHashCode();
		public static bool operator ==(StlViewer obj1, StlViewer obj2) => obj1.Equals(obj2);
		public static bool operator !=(StlViewer obj1, StlViewer obj2) => !obj1.Equals(obj2);
	}

	public partial class RefStlScriptGeneratorForm : ZChildForm
	{
		public RefStlScriptGeneratorForm()
		{
			InitializeComponent();
			var scriptsList = RefStlScriptGenerator.TestInstancesOfAllScripts.Select(s => new StlViewer(s)).OrderBy(s => s.ToString()).ToList();
			allScripts.DataSource = scriptsList;
			selectedScripts.DataSource = new List<StlViewer>();
		}

		void addButton_Click(object sender, EventArgs e)
		{
			var item = (StlViewer)this.allScripts.SelectedItem;
			var previouslySelected = (List<StlViewer>)selectedScripts.DataSource;
			if (!previouslySelected.Contains(item))
			{
				var newList = new List<StlViewer>(new[] { item });
				newList.AddRange(previouslySelected);
				selectedScripts.DataSource = newList;
				selectedScripts.SelectedIndex = 0;
			}
		}

		void removeButton_Click(object sender, EventArgs e)
		{
			var selected = (List<StlViewer>)selectedScripts.DataSource;
			selected.Remove((StlViewer)this.selectedScripts.SelectedItem);
			selectedScripts.DataSource = new List<StlViewer>(selected);
			if (selected.Count > 0)
			{
				selectedScripts.SelectedIndex = 0;
			}
		}

		void generateButton_Click(object sender, EventArgs e)
		{
			var selected = (List<StlViewer>)selectedScripts.DataSource;
			var script = RefStlScriptGenerator.GenerateInsertScript(selected.Select(s => s.Script).ToArray());
			SafeClipboard.SetText(script);
			UpdateStatusBar(Res.GetString("39FC4D13-B88D-47B1-A50C-F9CDED80D98C", "Script Copied to Clipboard"), ComponentModel.NotificationType.Information);
		}

		void displayPropertiesButton_Click(object sender, EventArgs e)
		{
			var selected = (List<StlViewer>)selectedScripts.DataSource;
			var collectorsForDisplay = new StlCollectorForDisplayCollection();

			foreach (var stlViewer in selected)
			{
				var script = stlViewer.Script;
				collectorsForDisplay.Add(new StlCollectorForDisplay
				{
					FeatureCode = script.Code,
					RoleName = script.Role,
					ModuleName = script.Module,
					FunctionName = script.Function,
					FeatureName = script.Feature,
					DataGranularity = RefStlScriptHelper.DataGrainToCode(script.StlGrain),
					CompanyCode = script.Company,
					BranchCode = script.Branch,
					TransactionDateUtc = script.TransactionDateUtc,
					CreatingUserCode = script.User,
					GuidReference = script.GuidReference,
					BillingReference1 = script.Reference1,
					BillingReference2 = script.Reference2,
					BillingReference3 = script.Reference3,
					BillingReference4 = script.Reference4,
					AdditionalRefs = script.AdditionalRefs,
					TransactionCount = script.BillableCount,
					PreparationScript = script.Preparation,
					FromClause = script.From,
					WhereClause = script.Where,
					WithOptionRecompile = script.WithRecompile,
					UsedInBilling = script.IsMandatoryForMilestones,
					ActiveOn = script.ActiveOn,
					MinCW1Version = script.MinCW1Version,
					MaxCW1Version = script.MaxCW1Version,
					DateType = RefStlScriptHelper.DateTypeToCode(script.DateType),
					CollectionStartDateUtc = RefStlScriptHelper.DateToCode(script.CollectionStartDateUtc)
				});
			}
			new StlCollectorForm(collectorsForDisplay).Show();
		}
	}
}
