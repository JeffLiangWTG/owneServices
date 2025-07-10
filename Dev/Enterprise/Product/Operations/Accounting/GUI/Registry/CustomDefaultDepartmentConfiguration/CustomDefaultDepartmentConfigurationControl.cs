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
	public partial class CustomDefaultDepartmentConfigurationControl : RegistryZUserControl, INotifications
	{
		public CustomDefaultDepartmentConfigurationControl()
		{
			InitializeComponent();
			CustomDepartmentChooser = new CustomDepartmentChooser(new BusinessObjectFactory());
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

		readonly ResourceStringData CaptionForValidationResult = Res.GetData("9577FBEF-22CB-426C-B3CD-43179F9C7AD8", "Default Department:");
		readonly ResourceStringData CaptionForTimeTakenResult = Res.GetData("7F8FE57D-CAFE-40E9-88D9-9AD2DAB80883", "Time Taken:");
		readonly CustomDepartmentChooser CustomDepartmentChooser;

		void EvaluateButton_Click(object sender, EventArgs e)
		{
			var watch = new Stopwatch();
			watch.Start();
#if !WINZOR
			ValidationResult = CustomDepartmentChooser.ValidateConfigWithJob(JobFindBox.Guid, ConfigTextBox.Rtf);
#else
			ValidationResult = CustomDepartmentChooser.ValidateConfigWithJob(JobFindBox.Guid, ConfigTextBox.Html);
#endif

			TimeTaken = watch.ElapsedMilliseconds;
			watch.Stop();
			ValidationResultLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}", CaptionForValidationResult.Caption, ValidationResult); // Validation Result
			TimeTakenResultLabel.Text = string.Format(CultureInfo.CurrentCulture, (NoResString)"{0} {1} ms.", CaptionForTimeTakenResult.Caption, TimeTaken); // Time Taken Result
		}

		void SetDepartmentDefaultingRulesLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProductionRulesEngineLinkManager.HandleProductionRulesEngineLinkClick(this, ProductionRuleSetAliases.AccountingDepartmentSelection);
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
