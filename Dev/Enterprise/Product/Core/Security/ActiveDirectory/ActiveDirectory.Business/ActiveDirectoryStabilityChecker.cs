using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text.RegularExpressions;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Environment;

[assembly: StabilityChecker(ActiveDirectoryStabilityChecker.Description, ActiveDirectoryStabilityChecker.Code, typeof(Enterprise.Security.ActiveDirectory.ActiveDirectoryStabilityChecker))]
namespace Enterprise.Security.ActiveDirectory
{
	public class ActiveDirectoryStabilityChecker : IStabilityChecker
	{
		public const string Code = "ADC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Active Directory Stability Checker";

		public StabilityResult[] Check()
		{
			return Check(false);
		}

		public StabilityResult[] Check(bool forceCheck)
		{
			var result = new List<StabilityResult>();

			if (forceCheck || ActiveDirectoryRegistry.Instance.IsIntegrationEnabled)
			{
				var domainCredentialsCollection = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value;
				if (domainCredentialsCollection.Count == 0)
				{
					result.Add(new StabilityResult(StabilityResultLevel.Critical, Res.GetString("FD080A31-B91E-4E74-9ED0-CFAE21094737", "At least one Domain Credentials must be set in registry: {0}", ((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual)));
				}
				foreach (DomainCredentials domainCredentials in domainCredentialsCollection)
				{
					try
					{
						var isUserAndGroupOUValid = true;

						//1. Is User OU valid?
						if (!IsOrganisationalUnitValid(domainCredentials, domainCredentials.UserOrganisationalUnit, false))
						{
							result.Add(new StabilityResult(StabilityResultLevel.Critical, GetUserOUError(domainCredentials)));
							isUserAndGroupOUValid = false;
						}

						//2. Is Group OU valid?
						if ((ActiveDirectoryRegistry.Instance.EntitiesToSync == EntitiesToSync.UsersAndGroups) && !IsOrganisationalUnitValid(domainCredentials, domainCredentials.GroupOrganisationalUnit, false))
						{
							result.Add(new StabilityResult(StabilityResultLevel.Critical, GetGroupOUError(domainCredentials)));
							isUserAndGroupOUValid = false;
						}

						var tryingToSyncUsers = (ActiveDirectoryRegistry.Instance.SyncDirection == SyncDirection.TwoWay
							|| ActiveDirectoryRegistry.Instance.SyncMode == SyncMode.EnterpriseIsMaster);

						var tryingToSyncGroups = ActiveDirectoryRegistry.Instance.EntitiesToSync == EntitiesToSync.UsersAndGroups &&
							(ActiveDirectoryRegistry.Instance.SyncDirectionGroup == SyncDirection.TwoWay
							|| ActiveDirectoryRegistry.Instance.SyncMode == SyncMode.EnterpriseIsMaster);

						//Writable tests - when require write access and both User OU and Group OU are valid. Note: DirectorySearcherFactory can only return a writable DirectorySearcher when both User and Group OU are valid
						if (isUserAndGroupOUValid)
						{
							if (tryingToSyncUsers)
							{
								//3. Is default password matching current domain password policy? This also check if User OU is writable.
								if (!IsPasswordMatchingPolicy(domainCredentials))
								{
									result.Add(new StabilityResult(StabilityResultLevel.Critical, GetDefaultPasswordError(domainCredentials)));
								}
							}

							if (tryingToSyncGroups)
							{
								//4. Is Group OU writable?
								if ((ActiveDirectoryRegistry.Instance.EntitiesToSync == EntitiesToSync.UsersAndGroups) && !IsOrganisationalUnitValid(domainCredentials, domainCredentials.GroupOrganisationalUnit, true))
								{
									result.Add(new StabilityResult(StabilityResultLevel.Critical, GetGroupOUError(domainCredentials)));
								}
							}
						}
					}
					catch (NoDomainPrivilegeException)
					{
						result.Add(new StabilityResult(StabilityResultLevel.Critical, GetNoWritePrivilegeError(domainCredentials)));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result.Add(new StabilityResult(StabilityResultLevel.Exception, GetExceptionError(domainCredentials, ex)));
					}
				}
			}

			return result.ToArray();
		}

		public static string GetUserOUError(DomainCredentials domainCredentials) => Res.GetString("731ad57d-b7af-4522-85d9-765d3b582416", "The {0} for domain {1} in the registry setting '{2}' is pointing to an invalid Organizational Unit in Active Directory. New Staff records will not be synchronized with Active Directory. Please correct this registry setting.",
									domainCredentials.UserOrganisationalUnitInfo.HumanReadableName,
									domainCredentials.DomainName,
									((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual);

		public static string GetGroupOUError(DomainCredentials domainCredentials) => Res.GetString("e4839393-0e1c-4fed-8f1c-4ed432f1b89f", "The {0} for domain {1} in the registry setting '{2}' is pointing to an invalid Organizational Unit in Active Directory. New Group records will not be synchronized with Active Directory. Please correct this registry setting.",
								domainCredentials.GroupOrganisationalUnitInfo.HumanReadableName,
								domainCredentials.DomainName,
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual);

		public static string GetDefaultPasswordError(DomainCredentials domainCredentials) => Res.GetString("9c030e5e-66c1-479c-815f-89cfc8c16adc", "The {0} for domain {1} in the registry setting '{2}' is using a password that does not meet the current password policy requirements of the domain. Please correct this registry setting.",
								domainCredentials.DefaultPasswordInfo.HumanReadableName,
								domainCredentials.DomainName,
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual, domainCredentials.DomainName);

		public static string GetNoWritePrivilegeError(DomainCredentials domainCredentials) => Res.GetString("f16ed13b-5567-43e8-bdb8-1c7a09c9630d", "The user {0} defined in the registry setting '{1}' does not have write privileges to the Organizational Units for domain {2}. Active Directory Synchronization will not work.",
								domainCredentials.DomainUserName,
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual,
								domainCredentials.DomainName);

		public static string GetExceptionError(DomainCredentials domainCredentials, Exception ex) => Res.GetString("CAAA151B-FBC3-4619-A429-0E2763381680", @"Unexpected exception thrown when running {0} in domain {1}.\r\nException: {2}",
								Description,
								domainCredentials.DomainName,
								ex.Message);

		bool IsOrganisationalUnitValid(IDomainCredentials domainCredentials, string organisationalUnitPath, bool requireOUWritePrivilege)
		{
			try
			{
				var searcher = GetDirectorySearcher(domainCredentials, requireOUWritePrivilege);
				searcher.FindOrganisationalUnit(organisationalUnitPath);
			}
			catch (InvalidOUException)
			{
				return false;
			}
			catch (DirectoryServicesException ex) when (ex.Find<InvalidOUException>() != null)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		///  Checks if a password matches the current domain password policy 
		/// </summary>
		/// <param name="domainCredentials">Domain Credentials contains password to be checked</param>
		/// <returns>true if the password matches the current domain policy. false if the password doesn't match the policy or if the matching cannot be evaluated because no user OU is set in the registry</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		bool IsPasswordMatchingPolicy(IDomainCredentials domainCredentials)
		{
			IUserDirectoryEntry adUserEntry = null;
			try
			{
				var searcher = GetDirectorySearcher(domainCredentials, true);
				var userOU = searcher.FindOrganisationalUnit(domainCredentials.UserOrganisationalUnit);

				//clean up if directory entry wasn't deleted properly on previous run
				CleanUpLeftoverADSCTempUsers(searcher);

				adUserEntry = (IUserDirectoryEntry)userOU.CreateNewChild(ADStabilityCheckerUser, DirectoryObjectType.User, searcher);
				adUserEntry.CommitChanges();
				adUserEntry.SetPassword(domainCredentials.DefaultPassword);
			}
			catch (PasswordDoesNotMatchPolicyException)
			{
				return false;
			}
			catch (InvalidOUException)
			{
				return false;
			}
			catch (DirectoryServicesException ex) when (ex.Find<InvalidOUException>() != null)
			{
				return false;
			}
			finally
			{
				DeleteADUser(adUserEntry);
			}

			return true;

			void CleanUpLeftoverADSCTempUsers(IDirectorySearcher searcher)
			{
				var users = searcher.FindMatchingUsers(ADSCTempUserPrefix + "*", SearchScope.Subtree, ResultMatchingMode.AllowWildcards, domainCredentials.UserOrganisationalUnit);
				if (users != null)
				{
					foreach (var user in users)
					{
						// ADSC temporary user always created as inactive, Win2KName prefixed with "ADSC" and guid, 20 characters long in total
						if (user.IsActive || !Regex.IsMatch(user.Win2KName, "^ADSC(?i)[0-9A-F]{16}"))
						{
							continue;
						}

						try
						{
							var entry = user.GetDirectoryEntry();
							var creationDate = (DateTime)entry[ADAttributes.WhenCreated];
							if (DateTime.UtcNow.Subtract(creationDate).TotalHours > 1)
							{
								DeleteADUser(entry);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// This is to handle a rare case of racing condition where the AD user already deleted by another thread but still return in the Find above because DCs are not in sync
							// We don't care any exception here as it will be rerun in the next round.
						}
					}
				}
			}

			void DeleteADUser(IUserDirectoryEntry entry)
			{
				try
				{
					entry?.Delete();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// We don't care about any exception here as it shouldn't interrupt the process.
					// This is to handle any AD server exception during the final clean up
					// If the user couldn't be deleted, the next run will clean it up automatically.
				}
			}
		}

		IDirectorySearcher GetDirectorySearcher(IDomainCredentials domainCredentials, bool requireOUWritePrivilege)
		{
			return ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(domainCredentials, requireOUWritePrivilege);
		}

		const string ADSCTempUserPrefix = "ADSC"; //For backward compatibility, never change this prefix. If it's needed to be changed, keep this one and introduce a new one. This old one still need to be kept to clean up existing ADSC users.

		readonly string ADStabilityCheckerUser = (ADSCTempUserPrefix + ZGuid.NewZGuid()).Replace("-", "").Substring(0, 20);
	}
}
