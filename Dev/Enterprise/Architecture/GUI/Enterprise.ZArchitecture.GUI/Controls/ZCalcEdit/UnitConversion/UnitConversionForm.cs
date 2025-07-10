using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.ZArchitecture.GUI
{
	partial class UnitConversionForm : ZChildForm
	{
		public UnitConversionForm(UnitConversion unitConversion, ZCalcEditCore creatingCalcEdit)
			: base(unitConversion)
		{
			InitializeComponent();

			this.unitConversion = unitConversion;
			this.creatingCalcEdit = creatingCalcEdit;

			Setup();
		}

		readonly UnitConversion unitConversion;
		readonly ZCalcEditCore creatingCalcEdit;

		const int gapBetweenDynamicControls = 6;
		const int distanceFromDropEditToCalcEdits = 100;
		const int calcEditWidth = 64;

		void Setup()
		{
			var originalGroupBoxHeight = fromGroupBox.Height + toGroupBox.Height;

			SetupCalcEdits(fromGroupBox, fromDropEdit, false);
			SetupCalcEdits(toGroupBox, toDropEdit, true);

			ControlDpiScalingHelper.SetTop(ref toGroupBox, fromGroupBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(gapBetweenDynamicControls), false);

			ControlDpiScalingHelper.SetWidth(this, fromGroupBox.Left + fromGroupBox.Right, false);
			ControlDpiScalingHelper.SetHeight(this, this.Height + (fromGroupBox.Height + toGroupBox.Height) - originalGroupBoxHeight, false);

			cancelButton.Click += (s, e) => Close();
			okButton.Click += CommitChanges;
		}

		void SetupCalcEdits(ZGroupBox groupBox, ZDropEdit unitDropEdit, bool readOnly)
		{
			var x = unitDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(distanceFromDropEditToCalcEdits);
			var nextY = unitDropEdit.Top;

			foreach (var descriptor in unitConversion.AmountDescriptors)
			{
				var calcEdit = new ZCalcEdit();
				calcEdit.Size = ControlDpiScalingHelper.NewScaledSize(calcEditWidth, 20, true);
				calcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(x, nextY, false);
				calcEdit.TextAlign = HorizontalAlignment.Right;
				calcEdit.Decimals = creatingCalcEdit.Decimals;
				calcEdit.MaxValue = creatingCalcEdit.MaxValue;
				calcEdit.ReadOnly = readOnly || descriptor.IsReadOnly;
				calcEdit.CaptionResourceString = DataBoundResourceStrings.GetDataForProperty(descriptor);
				var prefix = readOnly ? UnitConversion.ToPrefix : UnitConversion.FromPrefix;
				var propertyName = prefix + descriptor.Name;
				calcEdit.SetDataBinding(unitConversion.UnitAmountContainer, propertyName);
				calcEdit.GetExtension<NotificationExtension>().Notifications = new NotificationCollection(unitConversion.UnitAmountContainer.FindPropertyInfo(propertyName).Notifications);

#if DEBUG
				Testing.MissingResourceStringChecker.ExcludeFromTest(calcEdit); // Caption resource string is set above, but it may contain empty caption and repotred by basher test
#endif

				groupBox.Controls.Add(calcEdit);

				nextY = calcEdit.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(gapBetweenDynamicControls);
			}

			groupBox.Size = ControlDpiScalingHelper.NewScaledSize(x + ControlDpiScalingHelper.ScaleToCurrentDpiX(calcEditWidth + gapBetweenDynamicControls), nextY, false);
			ControlDpiScalingHelper.SetHeight(ref groupBox, nextY, false);
		}

		void CommitChanges(object sender, EventArgs e)
		{
			unitConversion.ValidateAll();

			if (unitConversion.HasErrors)
			{
				Globals.Message.Show(Res.GetString("f5330b27-1b63-40ec-a9e4-b92cac6e99e6", "Please fix all errors."));
			}
			else
			{
				unitConversion.CommitConversion();
				Close();
			}
		}
	}
}
