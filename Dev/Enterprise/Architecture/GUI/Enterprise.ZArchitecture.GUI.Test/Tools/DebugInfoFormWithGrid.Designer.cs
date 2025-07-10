using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Forms.FormDebugInfo;

namespace Enterprise.Core.Forms.Test
{
	partial class DebugInfoFormWithGrid
	{
		new void InitializeComponent()
		{
			ZDropEditColumnStyleInfo editColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			this.OtherTextBox = new ZTextBox();
			this.ItemsGrid = new ZGrid();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.OtherTextBox.SuspendLayout();
			this.ItemsGrid.SuspendLayout();
			this.SuspendLayout();
			this.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			//
			// OtherTextBox
			//
			this.BindingSource.SetBindingMember(this.OtherTextBox, "Z0_Description");
			// ItemsGrid
			//
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "Collection");
			editColumnStyleInfo1.CharacterCasing = CharacterCasing.Normal;
			editColumnStyleInfo1.ColumnName = "Z0_ChildOnly";
			editColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			this.ItemsGrid.ColumnStyles.Add(editColumnStyleInfo1);
			//
			// DebugInfoFormWithGrid
			//
			this.Controls.Add(this.ItemsGrid);
			this.Controls.Add(this.OtherTextBox);
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(432, 380);
			((ISupportInitialize)(this.ItemsGrid)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
