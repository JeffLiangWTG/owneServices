using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZGuidFindBoxColumnStyleTestForm : ZForm
	{
		public ZGrid zGrid1;

		public ZGuidFindBoxColumnStyleTestForm(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public static bool AddGuidColumnFirst
		{
			get { return addGuidColumnFirst; }
			set { addGuidColumnFirst = value; }
		}

		static bool addGuidColumnFirst;

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			var zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			var zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			this.zGrid1 = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "Collection";
			this.zGrid1.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Collection";
			zGuidFindBoxColumnStyleInfo1.Caption = "GuidFindBox";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Z0_Guid";
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Collection";
			zCodeFindBoxColumnStyleInfo1.Caption = "Code";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Z0_Code";

			if (addGuidColumnFirst)
			{
				this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
				this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			}
			else
			{
				this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
				this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			}

			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = new System.Drawing.Point(16, 8);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = new System.Drawing.Size(528, 352);
			this.zGrid1.TabIndex = 0;
			// 
			// TestForm
			// 

			this.ClientSize = new System.Drawing.Size(560, 374);
			this.Controls.Add(this.zGrid1);
			this.Name = "TestForm";
			this.Text = "TestForm";
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
