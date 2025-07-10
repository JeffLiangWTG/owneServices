using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public class MailboxAndRemoteWebPrintClientCredentialsValidation : ZValidation
	{
		public MailboxAndRemoteWebPrintClientCredentialsValidation(MailboxAndRemoteWebPrintClientCredentials parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly MailboxAndRemoteWebPrintClientCredentials parent;

		public override Type AutoValidationType => typeof(MailboxAndRemoteWebPrintClientCredentialsValidation);

		public override void ValidateAll()
		{
			ValidateLocalComputerAlias();
			ValidateDomainName();
			ValidateReceivingInterval();
			ValidateSendingInterval();
			ValidateFailureNotificationGroup();
			ValidateDownTimeStart();
			ValidateDownTimeEnd();
		}

		public void ValidateLocalComputerAlias()
		{
			ValidateCalculatedProperty(parent.LocalComputerAliasInfo);
		}

		protected void CheckLocalComputerAlias()
		{
			if (string.IsNullOrEmpty(parent.LocalComputerAlias))
			{
				parent.LocalComputerAliasInfo.AddError(Res.GetString("3FDC8573-3D5D-429F-9852-D8D2E07F2F16", "Local Computer Alias cannot be empty."));
			}
		}

		public void ValidateDomainName()
		{
			ValidateCalculatedProperty(parent.DomainNameInfo);
		}

		protected void CheckDomainName()
		{
			if (string.IsNullOrEmpty(parent.DomainName))
			{
				parent.DomainNameInfo.AddError(Res.GetString("AC9550FC-FEBF-498E-BC7B-E8844E0FF90D", "Domain Name cannot be empty."));
			}
		}

		public void ValidateReceivingInterval()
		{
			ValidateCalculatedProperty(parent.ReceivingIntervalInfo);
		}

		protected void CheckReceivingInterval()
		{
			CheckInterval(parent.ReceivingIntervalInfo, 3, 10);
		}

		public void ValidateSendingInterval()
		{
			ValidateCalculatedProperty(parent.SendingIntervalInfo);
		}

		protected void CheckSendingInterval()
		{
			CheckInterval(parent.SendingIntervalInfo, 10, 180);
		}

		void CheckInterval(ZPropertyInfo info, int minValue, int maxValue)
		{
			var interval = (ZInt)info.Value;
			if (interval < minValue || interval > maxValue)
			{
				info.AddError(Res.GetString("1BA1E5D9-2057-4BB8-A760-649AFD21E656", "Enter a digit between {0} to {1}.", minValue, maxValue));
			}
		}

		public void ValidateFailureNotificationGroup()
		{
			ValidateCalculatedProperty(parent.FailureNotificationGroupInfo);
		}

		protected void CheckFailureNotificationGroup()
		{
			ListValidation.ErrorIfInvalidCode(parent.FailureNotificationGroupInfo);
		}

		public void ValidateDownTimeStart()
		{
			ValidateCalculatedProperty(parent.DownTimeStartInfo);
		}

		protected void CheckDownTimeStart()
		{
			CheckDownTime(parent.DownTimeStartInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(parent.DownTimeStartInfo, parent.DownTimeEndInfo);
		}

		public void ValidateDownTimeEnd()
		{
			ValidateCalculatedProperty(parent.DownTimeEndInfo);
		}

		protected void CheckDownTimeEnd()
		{
			CheckDownTime(parent.DownTimeEndInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(parent.DownTimeEndInfo, parent.DownTimeStartInfo);
		}

		void CheckDownTime(ZPropertyInfo targetInfo)
		{
			TypeValidation.CheckValidZTime(targetInfo);
			if (parent.DownTimeStart.IsValid && parent.DownTimeEnd.IsValid && parent.DownTimeEnd <= parent.DownTimeStart)
			{
				targetInfo.AddError(Res.GetString("B264D1AC-BC80-475C-B0E5-B439E63F1BEC", "End must be later than Start."));
			}
		}
	}
}
