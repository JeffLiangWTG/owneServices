using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JXCWarningMessageBox : ZMessageBox
	{
		public static DialogResult ShowDialog(JXCWarningInfoCollector warningInfoCollector)
		{
			if (warningInfoCollector == null)
			{
				throw new ArgumentNullException(nameof(warningInfoCollector));
			}

			return ZFormModaliser.ShowDialogAndDispose(new JXCWarningMessageBox(warningInfoCollector));
		}

		JXCWarningMessageBox(JXCWarningInfoCollector warningInfoCollector)
			: base("There are JXC warnings that need to be corrected before JXC message can be exported", "Cannot Export JXC Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
		{
			InitializeComponent();

			this.WarningInfoCollector = warningInfoCollector;

			InitialFormHeight = Height;
			SetupWarningDetails();
		}

		void SetupWarningDetails()
		{
			DetailsButton = new ZButton();
			DetailsButton.Font = new Font("Arial", 8.50F); // for the arrows
			DetailsButton.Text = ShowDetailsText;
			ControlDpiScalingHelper.SetTop(ref DetailsButton, TextBox.Bottom, false);
			ControlDpiScalingHelper.SetWidth(ref DetailsButton, 92, true);
			ControlDpiScalingHelper.SetLeft(ref DetailsButton, this.Width - DetailsButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(20), false);
			ControlDpiScalingHelper.SetTop(ref DetailsButton, Button1.Top, false);
			DetailsButton.Click += delegate(object sender, EventArgs e) { HandleShowDetailsButtonClick(); };
			Controls.Add(DetailsButton);

			DetailsGroupBox = new ZGroupBox();
			DetailsGroupBox.Text = "JXC Warning List";
			DetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(FormPadding, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(DetailsButton.Bottom) + 15);
			DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ClientRectangle.Width) - (FormPadding * 2), DetailsGroupBoxHeight);

			WarningTreeView = new ZTreeView();
			WarningTreeView.ReadOnly = true;
			WarningTreeView.BeforeSelect += delegate(object sender, TreeViewCancelEventArgs e) { e.Cancel = true; };
			WarningTreeView.Dock = DockStyle.Fill;
			DetailsGroupBox.Controls.Add(WarningTreeView);
			HideDetails();

			Controls.Add(DetailsGroupBox);

			ControlDpiScalingHelper.SetLeft(ref Button1, DetailsButton.Left - Button1.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(FormPadding), false);
			ControlDpiScalingHelper.SetTop(ref Button1, DetailsButton.Top, false);
			Button1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		}

		void SetupTreeView()
		{
			WarningTreeView.BeginUpdate();

			WarningTreeView.BeforeSelect += new TreeViewCancelEventHandler(WarningTreeView_BeforeSelect);
			foreach (JXCWarningInfo warning in WarningInfoCollector)
			{
				string bizOKey = warning.BizObj.PK.ToStringKey();
				var bizONode = FindNode(WarningTreeView.Nodes, bizOKey) ?? WarningTreeView.Nodes.Add(bizOKey, warning.BizObj.HumanReadableName);

				var propertyNode = FindNode(bizONode.Nodes, warning.Info.Name) ?? bizONode.Nodes.Add(warning.Info.Name, warning.Info.HumanReadableName);

				if (!propertyNode.Nodes.ContainsKey(warning.WarningMessage))
				{
					propertyNode.Nodes.Add(warning.WarningMessage, warning.WarningMessage);
				}
			}

			WarningTreeView.EndUpdate();
		}

		void WarningTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			e.Cancel = true;
		}

		TreeNode FindNode(TreeNodeCollection nodes, string nodeKey)
		{
			TreeNode[] foundNodes = nodes.Find(nodeKey, false);
			return (foundNodes.Length > 0) ? foundNodes[0] : null;
		}

		void HandleShowDetailsButtonClick()
		{
			if (DetailsShown)
			{
				HideDetails();
			}
			else
			{
				ShowDetails();
			}
		}

		void HideDetails()
		{
			DetailsButton.Text = ShowDetailsText;
			DetailsGroupBox.Visible = false;
			ControlDpiScalingHelper.SetHeight(this, InitialFormHeight, false);
			DetailsShown = false;
		}

		void ShowDetails()
		{
			if (!TreeViewSetup)
			{
				SetupTreeView();
				TreeViewSetup = true;
			}

			DetailsButton.Text = HideDetailsText;
			DetailsGroupBox.Visible = true;
			ControlDpiScalingHelper.SetHeight(this, InitialFormHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(DetailsGroupBoxHeight + FormPadding * 2), false);
			DetailsShown = true;
		}

		const string ShowDetailsText = "Show Details\u25BC";
		const string HideDetailsText = "Hide Details\u25B2";
		const int FormPadding = 5;
		const int DetailsGroupBoxHeight = 200;

		public readonly JXCWarningInfoCollector WarningInfoCollector;
		internal bool TreeViewSetup;
		internal bool DetailsShown;
		internal int InitialFormHeight;
		internal ZButton DetailsButton;
		internal ZTreeView WarningTreeView;
		internal ZGroupBox DetailsGroupBox;
	}
}
