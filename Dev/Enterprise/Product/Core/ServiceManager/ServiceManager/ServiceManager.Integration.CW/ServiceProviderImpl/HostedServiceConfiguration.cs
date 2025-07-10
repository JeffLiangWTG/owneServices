using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.CW;

namespace ServiceManager.Integration.CW
{
	public class HostedServiceConfiguration
	{
		readonly Lazy<MethodInfo[]> nudgingMethods;
		readonly Lazy<MethodInfo[]> requirementMethods;

		public HostedServiceConfiguration(IHostedServiceAttribute hostedServiceAttribute)
		{
			ServiceAttribute = hostedServiceAttribute;

			nudgingMethods = new Lazy<MethodInfo[]>(() =>
			{
				return SafeType == null
					? Array.Empty<MethodInfo>()
					: SafeType
						.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
						.Where(m => m.IsDefined(typeof(HostedServiceNudgedAttribute), inherit: false)
									&& m.ReturnType == typeof(bool)
									&& m.GetParameters().Length == 0)
						.ToArray();
			});

			requirementMethods = new Lazy<MethodInfo[]>(() =>
			{
				return SafeType == null
					? Array.Empty<MethodInfo>()
					: SafeType
						.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
						.Where(method =>
							((
								method.IsDefined(typeof(HostedServiceRequirementAttribute), inherit: false)
								&& method.ReturnType.Equals(typeof(string)))
							|| (
								method.IsDefined(typeof(HostedServiceRequirementsAttribute), inherit: false)
								&& method.ReturnType.Equals(typeof(string[]))))
							&& method.GetParameters().Length == 0)
						.ToArray();
			});
		}

		public bool SatisfiesRequirements() => SatisfiesRequirements(out _);

		public string[] CheckSatisfiesRequirementsForCurrentBranch(IEnumerable<IGlbCompany> activeCompanies)
		{
			_ = activeCompanies ?? throw new ArgumentNullException(nameof(activeCompanies));

			if (SafeType == null)
			{
				return new[] { "The service task type has been configured incorrectly." };
			}

			if (!HasRequirements)
			{
				return Array.Empty<string>();
			}

			var failureDescriptions = new List<string>();
			failureDescriptions.AddRange(CheckRequiresCompanyInCountryIsSatisfied(activeCompanies));
			failureDescriptions.AddRange(RequirementMethodsNotSatisfied());
			return failureDescriptions.ToArray();
		}

		public bool SatisfiesRequirements(out string? branchCode)
		{
			branchCode = null;

			if (SafeType == null)
			{
				return false;
			}

			if (!HasRequirements)
			{
				return true;
			}

			var activeCompanies = GlbCompany.GetActiveCompanies();
			var hasRequiresCompanyInCountryAndCanRunInAnyBranch = !string.IsNullOrEmpty(ServiceAttribute.RequiresCompanyInCountry) && ServiceAttribute.CanRunInAnyBranch;
			if (hasRequiresCompanyInCountryAndCanRunInAnyBranch && CheckRequiresCompanyInCountryIsSatisfied(activeCompanies).Length > 0)
			{
				return false;
			}

			foreach (var company in activeCompanies)
			{
				foreach (var branch in company.Branches.Where(x => x.GB_IsActive).OrderBy(x => x.GB_Code))
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						if (RequirementMethodsNotSatisfied().Length > 0)
						{
							break;
						}

						if (hasRequiresCompanyInCountryAndCanRunInAnyBranch || CheckRequiresCompanyInCountryIsSatisfied(activeCompanies).Length == 0)
						{
							branchCode = branch.GB_Code;
							return true;
						}
					}
				}
			}

			return false;
		}

		public Type? SafeType
		{
			get
			{
				if (string.IsNullOrEmpty(ServiceAttribute.TypeName) || string.IsNullOrEmpty(ServiceAttribute.TypeAssemblyName))
				{
					return null;
				}
				return Type.GetType(ServiceAttribute.TypeName + "," + ServiceAttribute.TypeAssemblyName, throwOnError: false);
			}
		}

		string[] CheckRequiresCompanyInCountryIsSatisfied(IEnumerable<IGlbCompany> activeCompanies)
		{
			var requiresCompanyInCountry = ServiceAttribute.RequiresCompanyInCountry;
			if (string.IsNullOrEmpty(requiresCompanyInCountry))
			{
				return Array.Empty<string>();
			}

			var countries = new HashSet<ZString>(
				requiresCompanyInCountry
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => (ZString)x.Trim().ToUpperInvariant())
				.Where(x => !x.IsEmpty)
			);
			if (countries.Count == 0)
			{
				return Array.Empty<string>();
			}

			var failureDescriptions = new List<string>();
			var currentCompany = GlbCompany.CurrentCompany;
			var currentBranch = GlbBranch.CurrentBranch;

			if (currentCompany != null && currentBranch != null)
			{
				if (ServiceAttribute.CanRunInAnyBranch)
				{
					var activeCompanyCountryCodes = new HashSet<ZString>(activeCompanies.Select(x => x.GC_RN_NKCountryCode));
					if (!countries.Overlaps(activeCompanyCountryCodes))
					{
						failureDescriptions.Add(FormattableString.Invariant($"No active company exists for any of the required countries. The configured list of the required countries for the task={requiresCompanyInCountry}."));
					}
				}
				else
				{
					var currentCompanyCountryCode = currentCompany.GC_RN_NKCountryCode;
					if (!countries.Contains(currentCompanyCountryCode))
					{
						failureDescriptions.Add(FormattableString.Invariant($"The current company country={currentCompanyCountryCode} was not in the configured list of the required countries for the task={requiresCompanyInCountry}."));
					}

					var currentBranchCountryCode = currentBranch.GB_RN_NKCountryCode.ToString();
					if (!countries.Contains(currentBranchCountryCode))
					{
						failureDescriptions.Add(FormattableString.Invariant($"The current branch country={currentBranchCountryCode} was not in the configured list of the required countries for the task={requiresCompanyInCountry}."));
					}
				}
			}
			else
			{
				if (currentCompany == null)
				{
					failureDescriptions.Add(FormattableString.Invariant($"The {nameof(GlbCompany.CurrentCompany)} is not set. Required countries for the task {requiresCompanyInCountry}."));
				}
				if (currentBranch == null)
				{
					failureDescriptions.Add(FormattableString.Invariant($"The {nameof(GlbBranch.CurrentBranch)} is not set. Required countries for the task {requiresCompanyInCountry}."));
				}
			}

			return failureDescriptions.ToArray();
		}

		bool HasRequirements => !string.IsNullOrEmpty(ServiceAttribute.RequiresCompanyInCountry) || requirementMethods.Value.Length > 0;

		string[] RequirementMethodsNotSatisfied()
		{
			if (requirementMethods.Value.Length == 0)
			{
				return Array.Empty<string>();
			}

			var requirementCheckTimeLimit = SharedRegistry.Instance.ServiceTaskRequirementCheckTimeLimit;
			var failureDescriptions = new List<string>();

			foreach (var method in requirementMethods.Value)
			{
				try
				{
					failureDescriptions.AddRange(InvokeRequirementCheckMethodWithTimeout(method, requirementCheckTimeLimit));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);
					failureDescriptions.Add($"{ex.Message} reported while checking requirement method {method.Name}.");
				}
			}

			return failureDescriptions.ToArray();
		}

		IEnumerable<string> InvokeRequirementCheckMethodWithTimeout(MethodInfo method, TimeSpan failureTimeout)
		{
			object? result = null;
			bool hadErrors = false;
			Exception? threadCriticalException = null;

			using var methodInvokeReady = new ManualResetEventSlim();
			using var methodInvokeCompleted = new ManualResetEventSlim();

			var thread = new Thread(() =>
			{
				using (DisposableActionForDbConnection())
				{
					methodInvokeReady.Set();
					try
					{
						result = method.Invoke(null, Array.Empty<object>());
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleException(ex.InnerException ?? ex);
						hadErrors = true;
					}
					catch (Exception ex)
					{
						threadCriticalException = ex.InnerException ?? ex;
					}
					finally
					{
						methodInvokeCompleted.Set();
					}
				}
			});

			thread.Name = method.Name;
			thread.Start();

			methodInvokeReady.Wait();

			if (!methodInvokeCompleted.Wait(failureTimeout))
			{
				var timeLimitError = $"Time limit ({failureTimeout.TotalSeconds}s) exceeded while checking requirement method {method.Name}.";
				var exception = new ServiceTaskRequirementCheckTimeoutException(ServiceAttribute, method.Name, failureTimeout.TotalSeconds);
				ErrorReporter.ReportDeveloperExceptionOnce(exception.Message, $"Service task {ServiceAttribute.Code}: " + timeLimitError, exception);
				return new string[] { timeLimitError };
			}

			if (threadCriticalException != null)
			{
				ExceptionDispatchInfo.Capture(threadCriticalException).Throw();
			}

			if (hadErrors)
			{
				return new[] { $"Exception reported while checking requirement method {method.Name}." };
			}

			if (method.ReturnType.Equals(typeof(string)))
			{
				var convertedResult = (string)result!;

				if (!string.IsNullOrEmpty(convertedResult))
				{
					return new[] { convertedResult };
				}
				else
				{
					return Enumerable.Empty<string>();
				}
			}
			else
			{
				return ((string[])result!);
			}
		}

		public IHostedServiceAttribute ServiceAttribute { get; }

		public bool IsConfiguredForNudging
		{
			get
			{
				if (nudgingMethods.Value.Length == 0)
				{
					return false;
				}

				foreach (var method in nudgingMethods.Value)
				{
					try
					{
						var isConfiguredToNudge = (bool)method.Invoke(null, [])!;
						if (!isConfiguredToNudge)
						{
							return false;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleException(ex);
						return false;
					}
				}
				return true;
			}
		}

		protected virtual IDisposable DisposableActionForDbConnection()
		{
			return Db.DisposableActionForDbConnection();
		}

		static void HandleException(Exception ex)
		{
			ErrorReporter.ReportOnce(ex.Message, ex);
		}
	}
}
