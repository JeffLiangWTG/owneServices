using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class IncidentClosureDispositionControl : RegistryZUserControl
	{
		protected List<IncidentClosureDispositionGridControl> Grids = new List<IncidentClosureDispositionGridControl>();

		public IncidentClosureDispositionControl()
		{
			InitializeComponent();
			Grids.Add(Grid1);
			Grids.Add(Grid2);
			Grids.Add(Grid3);
			Grids.Add(Grid4);
		}

		public IncidentClosureDispositionControl(IncidentClosureDispositionRegistryEditorInfo editorInfo)
			: this()
		{
			ZGroupBox[] groupboxArray = { zGroupBox1, zGroupBox2, zGroupBox3, zGroupBox4 };
			var captions = editorInfo.Captions.ToArray();

			for (int i = 0; i < groupboxArray.Length; i++)
			{
				if (i + 1 <= captions.Length)
				{
					Grids[i].SetLevel(
						(i <= 0) ? null : Grids[i - 1],
						(i >= captions.Length - 1) ? null : Grids[i + 1]);
					groupboxArray[i].GetExtension<ILabelCaptionRenderer>().Caption = captions[i];
					Grids[i].SetupColumns(editorInfo.BoolColumnCaption, true, editorInfo.IsCodeColumnVisible);
					if (!editorInfo.AreIsResolutionColumnsVisible[i])
					{
						Grids[i].RemoveIsResolutionColumn();
					}
				}
				else
				{
					groupboxArray[i].Visible = false;
				}
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			foreach (IncidentClosureDispositionGridControl grid in Grids)
			{
				grid.ReadOnly = readOnly;
			}
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return Grid1.ReadOnly;
			}
		}

#endif

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Ensure parents get set first
			foreach (IncidentClosureDispositionGridControl grid in Grids)
			{
				grid.SetDataBinding(dataSource, dataMember);
			}
			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
