using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl))]
	sealed class MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			control = (MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl)control;
			var localComputerAliasTextBoxReadOnly = control.FindSingleOrDefault<ZTextBox>(x => x.Name == "LocalComputerAliasTextBox").ReadOnly;
			var domainNameTextBoxReadOnly = control.FindSingleOrDefault<ZTextBox>(x => x.Name == "DomainNameTextBox").ReadOnly;
			var receivingIntervalIntEditReadOnly = control.FindSingleOrDefault<ZIntEdit>(x => x.Name == "ReceivingIntervalIntEdit").ReadOnly;
			var sendingIntervalIntEditReadOnly = control.FindSingleOrDefault<ZIntEdit>(x => x.Name == "SendingIntervalIntEdit").ReadOnly;
			var verboseCheckBoxReadOnly = control.FindSingleOrDefault<ZCheckBox>(x => x.Name == "VerboseCheckBox").ReadOnly;
			var failureNotificationGroupFindboxReadOnly = control.FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "FailureNotificationGroupFindbox").ReadOnly;
			var downTimeStartDateEditReadOnly = control.FindSingleOrDefault<ZDateEdit>(x => x.Name == "DownTimeStartDateEdit").ReadOnly;
			var downTimeEndDateEditReadOnly = control.FindSingleOrDefault<ZDateEdit>(x => x.Name == "DownTimeEndDateEdit").ReadOnly;

			return localComputerAliasTextBoxReadOnly && domainNameTextBoxReadOnly && receivingIntervalIntEditReadOnly && sendingIntervalIntEditReadOnly && verboseCheckBoxReadOnly && failureNotificationGroupFindboxReadOnly && downTimeStartDateEditReadOnly && downTimeEndDateEditReadOnly;
		}
	}
}
