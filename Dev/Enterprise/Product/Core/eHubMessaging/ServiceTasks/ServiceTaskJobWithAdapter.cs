using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class ServiceTaskJobWithAdapter : MessageProcessingJob
	{
		readonly IAdaptorFactory adaptorFactory;

		bool companyLevelExceptionOccured;

		protected ServiceTaskJobWithAdapter(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory = null)
			: base(serviceTaskSupport, notifier)
		{
			this.adaptorFactory = adaptorFactory ?? new GatewayAdaptorFactory();
		}

		protected virtual IAdaptorFactory CreateDynamicAdaptorFactory() => null;

		internal override void ExecuteInternal()
		{
			companyLevelExceptionOccured = false;
			base.ExecuteInternal();
		}

		internal virtual bool PreProcessingCheckPassed()
		{
			if (CurrentCompany == null)
			{
				Notifier.Notify(new WarningNotification(Res.GetString("4E35C196-E864-48C8-A827-D6FC5717F553", "Could not find any suitable companies to process messages.")));
				return false;
			}

			if (CurrentCompanySettings == null)
			{
				Notifier.Notify(new WarningNotification(Res.GetString("9aa72e5a-3eb0-4b19-a391-2bcf14bf7162", "Messaging settings not found for {0}.", CurrentCompany.GC_Code)));
				return false;
			}

			if (!CurrentCompanySettings.PasswordExists)
			{
				HandlePasswordPreProcessingCheckFail();
				return false;
			}

			return true;
		}

		protected virtual void HandlePasswordPreProcessingCheckFail()
		{
			Notifier.Notify(new WarningNotification(Res.GetString("A7B7B1B8-4B89-44D3-8543-09835155A7FF", "This {0} system is not registered with CargoWise eHub server. Please run EHI service task to complete the registration.", BrandingFactory.Instance.ProductName)));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		protected IeHubAdapter CreateAdapter()
		{
			try
			{
				var adapter = (CreateDynamicAdaptorFactory() ?? adaptorFactory).Create(ClientID, Password, Notifier, ServerAddress);
				if (adapter != null)
				{
					NotifyVerbose(Res.GetString("8fd72219-6361-4bf9-8901-3673f6fd9972", "Create adapter({0}): {1}", ServerAddress, Res.GetString("9DBDFE9B-C8D4-4F22-B903-47B692E30E1F", "SUCCESS")));
					return adapter;
				}
				NotifyVerbose(Res.GetString("8fd72219-6361-4bf9-8901-3673f6fd9972", "Create adapter({0}): {1}", ServerAddress, Res.GetString("7386E470-E5E4-493B-A5C1-327EE5F9385F", "FAIL")));
				throw new CreateAdapterException();
			}
			catch (TypeInitializationException ex)
			{
				if (ex.Message.Contains("System.Net.ServicePointManager")) // not ideal to use msg, but no other option
				{
					NotifyVerbose(Res.GetString("8fd72219-6361-4bf9-8901-3673f6fd9972", "Create adapter({0}): {1}", ServerAddress, Res.GetString("7386E470-E5E4-493B-A5C1-327EE5F9385F", "FAIL")));
					throw new CreateAdapterException("Environmental Error", ex);
				}
				throw;
			}
		}

		protected virtual string ClientID => CurrentCompany.LicenceKeyIdentifier;
		protected virtual string Password => CurrentCompanySettings.GetPassword();

		#region Companies

		protected IEnumerable<GlbCompany> Companies => ServiceTaskSupport.CompanySettingsManager.Companies;
		protected IEnumerable<GlbCompany> CompaniesThatShouldBeServiced => Companies.Where(CompanyShouldBeServiced);

		internal virtual GlbCompany CurrentCompany
		{
			get { return currentCompany; }
			set
			{
				currentCompany = value;
				CurrentCompanySettings = currentCompany != null ? ServiceTaskSupport.CompanySettingsManager.GetSetting(currentCompany) : null;
			}
		}

		GlbCompany currentCompany;

		internal ICompanySettings CurrentCompanySettings { get; private set; }

		protected void NotifyExecutingForCompany()
		{
			if (CurrentCompany != null)
			{
				NotifyVerbose(Res.GetString("B4148D43-D6C9-45E2-ABE3-D1C1C2BA6322", "Executing for Company {0}.", CurrentCompany.GC_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "exception message check")]
		protected bool HandleCompanyLevelException(Exception ex)
		{
			companyLevelExceptionOccured = true;
			if (ex is SqlLockLostException)
			{
				Notifier.Notify(new WarningNotification(Res.GetString("6dcc26b7-4b26-4edc-8f70-4d9f0d5e1a0a", "Exclusive lock for company was lost.")));
				return true;
			}

			if (ex is MessageSecurityException)
			{
				if (ex.IsExceptionPresentIncludingInner<FaultException>((NoResString)"ClientID or Password invalid."))
				{
					ServiceTaskSupport.ReportKnownException(ex, LogMessages.LoginFailureMessage);
				}
				else
				{
					Notifier.AddError(ex.Message + System.Environment.NewLine + ex);
				}
				return true;
			}

			if (ex is RegistrationException)
			{
				if (ex.Message.Contains((NoResString)"Please ignore the following message:")) // This is part of an exception message reported by eHub
				{
					var separator = new[] { "Please ignore " };  // This is part of an exception message reported by eHub
					var trimmedMessages = ex.Message.Split(separator, StringSplitOptions.None);
					var logMessage = trimmedMessages.Length > 0 ? trimmedMessages[0] : ex.Message;

					Notifier.Add(new WarningNotification(logMessage));
				}
				else
				{
					Notifier.AddError(ex.Message + System.Environment.NewLine + ex);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}
				return true;
			}

			return false;
		}

		protected virtual bool CompanyShouldBeServiced(GlbCompany company)
		{
			if (IsDeveloperSystem())
			{
				Notifier.AddWarning(Res.GetString("51C6201B-03F8-42EF-BB16-6482A4ABB091", "Skipping company '{0}' because it has a test license. To connect to eHub you must have a license that exists in ediProd.", company.GC_Code));
				return false;
			}

			if (!company.IsLicenceKeyIdentifierValid)
			{
				ServiceTaskSupport.ReportKnownException(new ConfigurationErrorsException(), Res.GetString("4D6FD004-AB62-46FC-85C2-9081B0DA0408", "Skipping company '{0}' because it has an invalid license.", company.GC_Code));
				return false;
			}

			return true;
		}

		protected virtual bool IsDeveloperSystem()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return "EDI".Equals(registrationKey.EnterpriseCode) && "DAT".Equals(registrationKey.ServerCode);
		}

		protected bool HandleCompanyLevelException(Exception ex, ref DisposableList companyLocks)
		{
			if (ex is SqlLockLostException)
			{
				companyLocks.Dispose();
				companyLocks = new DisposableList(0);
			}

			return HandleCompanyLevelException(ex);
		}

		#endregion // Companies

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		internal AdapterType ServiceAdapterType => adaptorFactory.AdapterType;

		public override bool NextExecuteIterationIsScheduled => base.NextExecuteIterationIsScheduled && !companyLevelExceptionOccured;
	}
}
