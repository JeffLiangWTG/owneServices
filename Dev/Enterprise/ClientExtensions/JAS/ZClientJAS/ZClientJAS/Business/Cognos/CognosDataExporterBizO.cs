using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosDataExporterBizO : JASDataExporterBizO
	{
		public CognosDataExporterBizO()
		{
			((IZPropertyInfoObsolete)ExportSummaryInfo).ReadOnly = true;
		}

		public override JASDataExporterBizOValidation Validation
		{
			get { return new CognosDataExporterBizOValidation(this); }
		}

		public virtual bool Export(ICognosNotificationSubscriber notificationSubscriber)
		{
			bool result = false;

			try
			{
				ZDateTime exportStartDateTime = PeriodCalculator.IsCurrentPeriod(EndingPeriod) ? ZDateTime.Now : PeriodCalculator.GetLastDayForPeriod(EndingPeriod);
				CognosExportDirector director = GetNewCognosExportDirector(notificationSubscriber);
				string tempFilePath = Path.Combine(Env.TempPath, string.Format("{0:yyMM}{1}.csv", exportStartDateTime, CurrentCompanyCode));
				result = director.Export(exportStartDateTime, tempFilePath);
				if (result)
				{
					result = DeliverCognosFile(tempFilePath, notificationSubscriber);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, "An error has occurred during export. Detail: " + ex.Message));
				ErrorReporter.ReportOnce("ZClientJAS.CognosDataExporterBizO.Export()", ex);
			}

			return result;
		}

		public ZString ExportSummary
		{
			get { return ExportSummaryBuilder.ToStringWithNewLineBetweenAppends(); }
		}

		public ZPropertyInfo ExportSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(ExportSummary)); }
		}

		public ZInt EndingPeriod
		{
			get { return fEndingPeriod; }
			set
			{
				if (fEndingPeriod != value)
				{
					SetNonPersistentPropertyValue(EndingPeriodInfo, ref fEndingPeriod, value);
					((CognosDataExporterBizOValidation)Validation).ValidateEndingPeriod();
				}
			}
		}

		public ZPropertyInfo EndingPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(EndingPeriod)); }
		}

		public AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory, GlbCompany.CurrentCompany);
				}
				return fPeriodCalculator;
			}
		}

		public void AppendExportSummary(string summaryText)
		{
			ExportSummaryBuilder.Append(summaryText);
			ExportSummaryInfo.RefreshBinding();
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override string EmailSubject
		{
			get { return "Cognos Export file from CargoWise One"; }
		}

		protected virtual CognosExportDirector GetNewCognosExportDirector(ICognosNotificationSubscriber notificationSubscriber)
		{
			return new CognosExportDirector(notificationSubscriber);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EndingPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
		}

		bool DeliverCognosFile(string tempFilePath, INotifications notificationSubscriber)
		{
			bool result = false;

			try
			{
				DeliverFiles(tempFilePath);
				result = true;
			}
			catch (IOException exception)
			{
				notificationSubscriber.Notify(new ErrorNotification(ErrorType.IOError, exception.Message));
			}
			catch (EmailSendFailedException exception)
			{
				notificationSubscriber.Notify(new ErrorNotification(ErrorType.ErrorSendingEmail, exception.Message));
			}

			return result;
		}

		ZString CurrentCompanyCode
		{
			get
			{
				JASOrgHeader orgProxyJAS = GlbCompany.CurrentCompany.OrgProxy as JASOrgHeader;
				return orgProxyJAS != null ? orgProxyJAS.NettingCode : ZString.Empty;
			}
		}

		ZStringBuilder ExportSummaryBuilder
		{
			get
			{
				if (fExportSummaryBuilder == null)
				{
					fExportSummaryBuilder = new ZStringBuilder();
				}
				return fExportSummaryBuilder;
			}
		}

		ZStringBuilder fExportSummaryBuilder;
		AccountingPeriodCalculator fPeriodCalculator;
		ZInt fEndingPeriod;

		#endregion
	}
}
