using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZDummyForm : ZForm
	{
		public ZCalcEdit CalcEdit;
		public ZTextBox TextBox;
		public ZGrid Grid;
		public ZTabControl TabControl;
		public Enterprise.Core.Forms.ZPostingButtonsUserControl SaveUserControl;
		private readonly System.ComponentModel.Container components;
		public ZTabPage TabPage1;
		public ZTabPage TabPage2;

		public ZDummyForm()
		{
		}

		public ZDummyForm(IBusiness entity)
			: base(entity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveUserControl);
			if (OverrideDefaultAddPreviousNextValue)
			{
				AutoAddPreviousNextButtons = OverridenAddPreviousNextValue;
			}

			FormLoadedWithArgs += (s, e) => LoadedFormArgs = e.Args;
		}

		public new void FireSaved()
		{
			base.FireSaved();
		}

		public override string FormCaption
		{
			get { return "ZDummyForm"; }
		}

		Dictionary<IBusiness, ZString> dependentObjects;
		public Dictionary<IBusiness, ZString> DependentObjects
		{
			get
			{
				if (dependentObjects == null)
				{
					dependentObjects = new Dictionary<IBusiness, ZString>();
				}
				return dependentObjects;
			}
		}

		protected override Dictionary<IBusiness, ZString> GetListOfBizObjectsToCheckEditing()
		{
			var result = base.GetListOfBizObjectsToCheckEditing();
			foreach (var obj in DependentObjects)
			{
				result.Add(obj.Key, obj.Value);
			}
			return result;
		}

		protected override bool IsAnotherUserEditing(Semaphores.Common.ISemaphoreInfo userAction)
		{
			if (IsAnotherUserEditing_CompareUserIdOnly)
			{
				return userAction.OwnerSession.UserPk != EnvProxy.Instance.CurrentUser.PK;
			}
			else
			{
				return base.IsAnotherUserEditing(userAction);
			}
		}

		public bool IsAnotherUserEditing_CompareUserIdOnly = true;

		public new MenuItem ActionsMenuItem
		{
			get { return base.ActionsMenuItem; }
		}

		public int DeleteCoreCallCount;
		protected override void DeleteCore()
		{
			DeleteCoreCallCount++;
			base.DeleteCore();
		}

		public new void Delete()
		{
			base.Delete();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			TabPage1 = new ZTabPage();
			TabPage1.Name = "Number1";
			TabPage2 = new ZTabPage();
			TabPage2.Name = "Number2";
			TopLevelTabControl.TabPages.Add(TabPage1);
			TopLevelTabControl.TabPages.Add(TabPage2);
			CaptionResourceString = Res.GetData("ZZ", "Dummy");
		}

		public static bool OverrideDefaultAddPreviousNextValue;
		public static bool OverridenAddPreviousNextValue = true;
		static bool throwSqlExceptionWhenSetVisible;

		public static IDisposable ThrowSqlExceptionWhenSetVisible()
		{
			throwSqlExceptionWhenSetVisible = true;
			return new DisposableAction(() => throwSqlExceptionWhenSetVisible = false);
		}

		public static void ResetPreviousNextOverrideValues()
		{
			OverrideDefaultAddPreviousNextValue = false;
			OverridenAddPreviousNextValue = true;
		}

		public void SendMouseMove(MouseEventArgs e)
		{
			OnMouseMove(e);
		}

		public void SendClick(EventArgs e)
		{
			OnClick(e);
		}

		public IEnumerable<string> FormArgsToPersistOnClose_ForTest { get; set; }

		protected internal override IEnumerable<string> GetFormArgsToPersistOnClose()
		{
			return FormArgsToPersistOnClose_ForTest;
		}

		public IEnumerable<string> LoadedFormArgs { get; private set; }

		public new void HandleApplyPostingButtonClickUnsafe(bool closeOnSave)
		{
			base.HandleApplyPostingButtonClickUnsafe(closeOnSave);
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			return ContinueWithDelete.Yes;
		}

		protected override void SetVisibleCore(bool value)
		{
			if (throwSqlExceptionWhenSetVisible)
			{
				var error = SqlExceptionBuilder.CreateSqlError(1222, 1, 1, Db.Connection.ServerName, "Lock request time out period exceeded.", "", 1);
				throw SqlExceptionBuilder.CreateSqlException(error);
			}
			base.SetVisibleCore(value);
		}

		#region Auto

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			var zGridTextBoxColumnInfo1 = new ZTextBoxColumnStyleInfo();
			var zGridCalcEditColumnInfo1 = new ZCalcEditColumnStyleInfo();
			this.CalcEdit = new ZCalcEdit();
			this.TextBox = new ZTextBox();
			this.Grid = new ZGrid();
			this.TabControl = new ZTabControl();
			this.SaveUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 285, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(272);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(273);
			// 
			// CalcEdit
			// 
			this.CalcEdit.BindTo = "Z0_Number";
			this.CalcEdit.Decimals = 2;
			this.CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.CalcEdit.Name = "CalcEdit";
			this.CalcEdit.TabIndex = 1;
			this.CalcEdit.Text = "ZCALCEDIT1";
			this.CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TextBox
			// 
			this.TextBox.BindTo = "Z0_Description";
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.TextBox.Name = "TextBox";
			this.TextBox.TabIndex = 2;
			this.TextBox.Text = "ZTEXTBOX1";
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.BindTo = "Collection";
			this.Grid.CaptionVisible = false;

			#region Column Initialisation

			zGridTextBoxColumnInfo1.ColumnName = "Z0_Description";
			zGridCalcEditColumnInfo1.ColumnName = "Z0_Number";
			this.Grid.ColumnStyles.Add(zGridTextBoxColumnInfo1);
			this.Grid.ColumnStyles.Add(zGridCalcEditColumnInfo1);

			#endregion

			this.Grid.EnableToolTips = false;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 16, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 224, true);
			this.Grid.TabIndex = 3;
			// 
			// TabControl
			// 
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 96, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 184, true);
			this.TabControl.TabIndex = 4;
			// 
			// SaveUserControl
			// 
			this.SaveUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 248, true);
			this.SaveUserControl.Name = "SaveUserControl";
			this.SaveUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.SaveUserControl.TabIndex = 5;
			// 
			// ZTestForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 308, true);
			this.Controls.Add(this.SaveUserControl);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.CalcEdit);
			this.Name = "ZTestForm";
			this.Text = "TestForm";
			this.Controls.SetChildIndex(this.CalcEdit, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.Grid, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.SaveUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		#endregion
	}
}
