using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ElectronicMessagingCertificateExpiryDateCheckServiceTask.Code,
	"E-Reporting Certificate/Token Expiry Date Check Service",
	"ACC",
	typeof(ElectronicMessagingCertificateExpiryDateCheckServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	IsScheduleReadOnly = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day"
	)]

namespace Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck
{
	public class ElectronicMessagingCertificateExpiryDateCheckServiceTask : ServiceProviderImpl
	{
		public const string Code = "ECE";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging only")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var utcNow = ZDateTime.UtcNow;
			ServiceLogger.Information($"E-Reporting Certificate/Token Expiry Date Check Service is running at {utcNow:yyyy-MM-dd HH:mm:ss}.");

			var activeCompanies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true))
				.Where(company => company.GC_Code != GlbCompany.DemoCompanyCode && company.HasActiveBranch)
				.OrderBy(x => x.GC_Code)
				.ToArray();

			var countryObjectFactoryDic = new Dictionary<string, ICountryEInvoicingObjectFactory>();
			foreach (var company in activeCompanies)
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();
				if (!countryObjectFactoryDic.TryGetValue(company.GC_RN_NKCountryCode, out var countryObjectFactory))
				{
					countryObjectFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(company.GC_RN_NKCountryCode);
					countryObjectFactoryDic[company.GC_RN_NKCountryCode] = countryObjectFactory;
				}

				if (countryObjectFactory == null || countryObjectFactory.Credentials == null || countryObjectFactory.Credentials.PasswordType != PasswordTypesList.Codes.EIM)
				{
					continue;
				}

				try
				{
					Processor.AlertForAlmostExpiryCredentials(company, countryObjectFactory.Credentials, utcNow, ServiceLogger,
						countryObjectFactory.GetElectronicMessagingNotificationEmailCreator(),
						(IElectronicMessagingNotificationQueryProvider)countryObjectFactory);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Error($"Fail error for '{company.GC_Code}' - {ex.Message}.");
				}
			}
			ServiceLogger.Information("E-Reporting Certificate/Token Expiry Date Check Service is completed.");
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		IElectronicMessagingCertificateExpiryDateCheckProcessor Processor => processor ?? (processor = ObjectFactory.Get<IElectronicMessagingCertificateExpiryDateCheckProcessor>());
		IElectronicMessagingCertificateExpiryDateCheckProcessor processor;
	}
}
