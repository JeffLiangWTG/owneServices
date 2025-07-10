using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public partial class AutoRatingPriorityControl : ZUserControl
	{
		public AutoRatingPriorityControl()
		{
			InitializeComponent();
			CreateMappers();
			OrderedPriorities = new List<string>();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Priorities
		{
			get => string.Join(",", OrderedPriorities);
			set => PopulateListControl(value);
		}

		#region Implementation

		Hashtable CodeMapper;

		#region Setting Priorities

		void PopulateListControl(string priorityString)
		{
			OrderedPriorities.Clear();

			var prioritiesArray = priorityString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			OrderedPriorities.AddRange(prioritiesArray.Select(p => p.ToUpperInvariant()));

			RebuildList();
		}

		void CreateMappers()
		{
			CodeMapper = new Hashtable
			{
				{ RateEntrySchema.TI_RH_NKCommodityCode.Name.ToUpper(), Res.GetString("7c50cb54-39b5-43a8-89f6-6d44ebac2663", "COMMODITY CODE") },
				{ RateEntrySchema.TI_RS_NKServiceLevel_NI.Name.ToUpper(), Res.GetString("a1d4fed6-872d-41a4-ac71-155c86fd6704", "SERVICE LEVEL") },
				{ RateEntrySchema.TI_OH_TransportProvider.Name.ToUpper(), Res.GetString("be786f08-df4a-4878-8f2e-f933d483b328", "SHIPPING PROVIDER") },
				{ RateEntrySchema.TI_ViaLRC.Name.ToUpper(), Res.GetString("3cb0b630-95f5-48d0-9aad-4703d688a7b4", "TRANS-SHIPMENT PORT") },
				{ RateEntrySchema.TI_HBLDeliveryMode.Name.ToUpper(), Res.GetString("1657f46e-17c8-4dec-be49-155d9ff5c847", "HBL Delivery Mode") }
			};
		}

		#endregion

		#region Changing Priorities

		void MoveUpButton_Click(object sender, System.EventArgs e)
		{
			MovePriority(-1);
		}

		void MoveDown_Click(object sender, System.EventArgs e)
		{
			MovePriority(1);
		}

		void MovePriority(int increment)
		{
			var selectedIndex = PriorityListBox.SelectedIndex;

			if (PriorityListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("c6efcc63-379c-4bc9-84d7-21d89d7d8119", "There are no priorities in the list."));
			}
			else if (selectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("82d07e01-2fcb-42a5-82ac-b78e204045bb", "You have not selected a priority."));
			}
			else if (PriorityListBox.SelectedIndices.Count > 1)
			{
				Globals.Message.ShowError(Res.GetString("897d5628-adf9-4b90-acd9-3d7ac47d5fc5", "You can select only one priority at a time."));
			}
			else if (selectedIndex == PriorityListBox.Items.Count - 1 && increment > 0)
			{
				Globals.Message.ShowError(Res.GetString("b0043bb0-b327-4b31-8721-7095bde93245", "This priority is already at the bottom of the list."));
			}
			else if (selectedIndex == 0 && increment < 0)
			{
				Globals.Message.ShowError(Res.GetString("8d08b734-4949-4f51-8957-f34c369e9d45", "This priority is already at the top of the list."));
			}
			else
			{
				var selectedItem = OrderedPriorities[selectedIndex];
				OrderedPriorities.RemoveAt(selectedIndex);

				var newIndex = selectedIndex + increment;
				OrderedPriorities.Insert(newIndex, selectedItem);

				RebuildList();
				PriorityListBox.SelectedIndex = newIndex;
			}
		}

		void RebuildList()
		{
			PriorityListBox.Items.Clear();
			foreach (var priority in OrderedPriorities)
			{
				var displayedText = CodeMapper[priority] ?? string.Empty;
				PriorityListBox.Items.Add(displayedText);
			}
		}

		readonly List<string> OrderedPriorities;

		#endregion

		#endregion
	}
}

// Tests are in AutoRatingPriorityRegistryItemEditor
