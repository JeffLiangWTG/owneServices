using System;
using System.Diagnostics;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.ProductionRules.GUI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.GUI.Res;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CustomDefaultBranchConfigurationControl : RegistryZUserControl, INotifications
	{
		public CustomDefaultBranchConfigurationControl()
		{
			InitializeComponent();
			CustomBranchChooser = new CustomBranchChooser(new BusinessObjectFactory());
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ConfigTextBox.ReadOnly = !Env.CurrentUser.IsSupportUser;
			EvaluateButton.ReadOnly = readOnly;
			JobFindBox.ReadOnly = readOnly;
		}

		ZString ValidationResult { get; set; }

		[DecimalPlaces(2)]
		ZDecimal TimeTaken { get; set; }

		readonly ResourceStringData CaptionForValidationResult = Res.GetData("958CA79A-63B6-4B11-B870-6913FDC7BF10", "Default Branch:");
		readonly ResourceStringData CaptionForTimeTakenResult = Res.GetData("DD97D1A5-E577-4A76-811D-A565C5E607D8", "Time Taken:");
		readonly CustomBranchChooser CustomBranchChooser;

		void EvaluateButton_Click(object sender, EventArgs e)
		{
			var watch = new Stopwatch();
			watch.Start();
#if !WINZOR
			ValidationResult = CustomBranchChooser.ValidateConfigWithJob(JobFindBox.Guid, ConfigTextBox.Rtf);
#else
			ValidationResult = CustomBranchChooser.ValidateConfigWithJob(JobFindBox.Guid, ConfigTextBox.Html);
#endif
			TimeTaken = watch.ElapsedMilliseconds;
			watch.Stop();
			ValidationResultLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}", CaptionForValidationResult.Caption, ValidationResult); // Validation Result
			TimeTakenResultLabel.Text = string.Format(CultureInfo.CurrentCulture, (NoResString)"{0} {1} ms.", CaptionForTimeTakenResult.Caption, TimeTaken); // Time Taken Result
		}

		void SetBranchDefaultingRulesLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProductionRulesEngineLinkManager.HandleProductionRulesEngineLinkClick(this, ProductionRuleSetAliases.AccountingBranchSelection);
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		void Notify(INotification notification)
		{
			if (notification.Type.EnumValueName == CargoWise.ComponentModel.NotificationType.Information.EnumValueName)
			{
				Globals.Message.ShowInformation(notification.Message);
			}
			else if (notification.Type.EnumValueName == CargoWise.ComponentModel.NotificationType.Warning.EnumValueName)
			{
				Globals.Message.ShowWarning(notification.Message);
			}
			else
			{
				Globals.Message.ShowError(notification.Message);
			}
		}

		#endregion
	}
}
