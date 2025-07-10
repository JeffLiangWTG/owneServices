namespace Enterprise.LogShipping.Setup.GUI
{

	partial class TaskStatusControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskStatusControl));
			this.stateImageList = new System.Windows.Forms.ImageList(this.components);
			this.label = new System.Windows.Forms.Label();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// stateImageList
			// 
			this.stateImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("stateImageList.ImageStream")));
			this.stateImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.stateImageList.Images.SetKeyName(0, "Wrench.png");
			this.stateImageList.Images.SetKeyName(1, "Yes.gif");
			this.stateImageList.Images.SetKeyName(2, "Forward.ico");
			this.stateImageList.Images.SetKeyName(3, "YesGray.gif");
			this.stateImageList.Images.SetKeyName(4, "Error.ico");
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.label.Location = new System.Drawing.Point(24, 5);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(34, 15);
			this.label.TabIndex = 0;
			this.label.Text = "label";
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(4, 4);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(16, 16);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			// 
			// TaskStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.AutoSize = true;
			this.Controls.Add(this.pictureBox);
			this.Controls.Add(this.label);
			this.Name = "TaskStatusControl";
			this.Size = new System.Drawing.Size(72, 25);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ImageList stateImageList;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.PictureBox pictureBox;

	}
}