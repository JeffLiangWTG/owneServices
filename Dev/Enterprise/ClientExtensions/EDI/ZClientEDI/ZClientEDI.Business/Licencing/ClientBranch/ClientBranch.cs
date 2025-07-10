using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientBranch : AutoClientBranch
	{
		public ClientBranch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgAddress Address => Factory.Load<OrgAddress>(LCB_OA);

		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(LCB_LD);

		[RelatedBusinessObject("Database")]
		public override ZGuid LCB_LD
		{
			get => base.LCB_LD;
			set => base.LCB_LD = value;
		}

		#region Sync

		static readonly MutexID ImportClientBranchMutexID = new MutexID("ClientBranch", "Ensure same client branch only be imported once.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static bool Sync(BusinessObjectFactory factory, LicenceDatabase database, List<BranchReport> branchReports, int maxTries = 3, int millisecondsBetweenTries = 5000)
		{
			var mutex = CreateAndLockImportMutex(database.PK, maxTries, millisecondsBetweenTries);
			if (mutex == null)
			{
				return false;
			}

			var databaseInSyncFactory = factory.Load<LicenceDatabase>(database.PK);

			using (mutex)
			{
				SyncInternal(factory, databaseInSyncFactory, branchReports);
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static ZGlobalMutex CreateAndLockImportMutex(ZGuid databasePk, int maxTries, int millisecondsBetweenTries)
		{
			ZGlobalMutex result = null;
			int failureCount = 0;
			do
			{
				var mutex = new ZGlobalMutex(ImportClientBranchMutexID, databasePk.ToString());
				try
				{
					if (mutex.Lock())
					{
						result = mutex;
						mutex = null;
						break;
					}
				}
				catch (SqlLockLostException)
				{
				}
				finally
				{
					if (mutex != null)
					{
						try
						{
							((IDisposable)mutex).Dispose();
						}
						catch (Exception ex) when (!ex.IsCriticalException()) { }
					}
				}

				failureCount++;
				if (failureCount < maxTries)
				{
					Thread.Sleep(millisecondsBetweenTries);
				}
			}
			while (failureCount < maxTries);

			return result;
		}

		static void SyncInternal(BusinessObjectFactory factory, LicenceDatabase database, List<BranchReport> branchReports)
		{
			var clientBranchList = factory.Load<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_LD, database.PK)).ToList();
			var clientBranchByPk = clientBranchList.ToDictionary(x => x.LCB_ClientPK);
			var clientBranchByOrgAddress = clientBranchList.Where(x => !x.LCB_OA.IsEmpty).ToDictionary(x => x.LCB_OA);
			var org = database.WebAccessOrg;
			var addressCodesMap = org.Addresses.Cast<OrgAddress>().ToDictionary(x => x.OA_Code.ToString(), x => x.PK, StringComparer.OrdinalIgnoreCase);

			foreach (var report in branchReports)
			{
				if (!clientBranchByPk.TryGetValue(report.PK, out ClientBranch clientBranch))
				{
					clientBranch = factory.New<ClientBranch>();
					clientBranch.LCB_LD = database.PK;
				}

				clientBranch.PopulateFromVersionReport(report);
				clientBranch.CreateOrUpdateOrgAddress(report, clientBranchByOrgAddress, addressCodesMap, org);

				if (!clientBranchByOrgAddress.ContainsKey(clientBranch.LCB_OA))
				{
					clientBranchByOrgAddress.Add(clientBranch.LCB_OA, clientBranch);
				}
			}

			factory.Save();
		}

		void PopulateFromVersionReport(BranchReport report)
		{
			LCB_IsActive = report.IsActive;
			LCB_Code = report.Code;
			LCB_LCC_Code = report.CompanyCode;
			LCB_Name = report.BranchName;
			LCB_ClientPK = report.PK;
		}

		void CreateOrUpdateOrgAddress(BranchReport report, Dictionary<ZGuid, ClientBranch> clientBranchByOrgAddress, Dictionary<string, ZGuid> addressCodesMap, OrgHeader org)
		{
			if (org == null || string.IsNullOrEmpty(report.Address1))
			{
				return;
			}

			OrgAddress address = Address;

			if (address == null || address.IsMainAddress)
			{
				address = org.Addresses.Cast<OrgAddress>().FirstOrDefault(x => IsSameAddress(report, x) && !x.IsMainAddress);
				if (address != null && clientBranchByOrgAddress.TryGetValue(address.PK, out ClientBranch linkedBranch))
				{
					if (linkedBranch.PK != this.PK)
					{
						address = null;
					}
				}
			}

			if (report.IsActive)
			{
				if (address != null && address.OA_OH != org.PK)
				{
					address.OA_IsActive = false;
					address = null;
				}

				if (address == null)
				{
					address = org.Addresses.AddNew();
				}
			}

			if (address != null)
			{
				address.IsSuspendingDeduplication = true;

				LCB_OA = address.PK;

				if (address.OA_IsActive != report.IsActive)
				{
					address.OA_IsActive = report.IsActive;
				}
				address.OA_Address1 = report.Address1;
				address.OA_Address2 = report.Address2;

				if (!string.IsNullOrEmpty(report.City))
				{
					address.OA_City = report.City;
				}

				if (!string.IsNullOrEmpty(report.State))
				{
					address.OA_State = report.State;
				}

				if (!string.IsNullOrEmpty(report.PostCode))
				{
					address.OA_PostCode = report.PostCode;
				}

				address.OA_RN_NKCountryCode = report.CountryCode;

				if (address.OA_RL_NKRelatedPortCode != report.Unloco)
				{
					address.OA_RL_NKRelatedPortCode = report.Unloco;
				}

				address.OA_CompanyNameOverride = report.CompanyName;
				address.OA_Code = GetUniqueAddressCode(addressCodesMap, report.Code, report.BranchName, org, address);
				address.OA_ValidationStatus = GetValidStatus(report.ValidationStatus);

				if (!address.IsAddressOfType(OrgAddressType.Office))
				{
					address.AddAddressType(OrgAddressType.Office);
					address.HasChanges = true;
				}

				if (address.HasChanges)
				{
					this.HasChanges = true;
				}
			}
			else
			{
				LCB_OA = ZGuid.Empty;
			}
		}

		static bool IsSameAddress(BranchReport report, OrgAddress address)
		{
			return IsSameAddressPart(report.Address1, address.OA_Address1)
				&& IsSameAddressPart(report.Address2, address.OA_Address2)
				&& IsSameAddressPart(report.City, address.OA_City)
				&& IsSameAddressPart(report.State, address.OA_State)
				&& IsSameAddressPart(report.PostCode, address.OA_PostCode)
				&& IsSameAddressPart(report.Unloco, address.OA_RL_NKRelatedPortCode)
				&& IsSameAddressPart(report.CountryCode, address.OA_RN_NKCountryCode);
		}

		static bool IsSameAddressPart(ZString branchAddressPart, ZString orgAddressPart)
		{
			return branchAddressPart.EqualsIgnoringCase(orgAddressPart) || branchAddressPart.IsEmpty || orgAddressPart.IsEmpty;
		}

		public static ZString GetUniqueAddressCode(Dictionary<string, ZGuid> addressCodesMap, string branchCode, string branchName, OrgHeader org, OrgAddress address)
		{
			var branchCodePart = FormattableString.Invariant($" ({branchCode})");
			var branchNamePart = new ZString(branchName).SubstringSafe(0, OrgAddress.Schema.OA_CodeMaxLength - branchCodePart.Length);
			var result = branchNamePart + branchCodePart;

			if (addressCodesMap.ContainsKey(result) && addressCodesMap[result] != address.PK)
			{
				const int maximumSuffixNumber = 99;
				for (int i = 1; i <= maximumSuffixNumber; i++)
				{
					branchCodePart = FormattableString.Invariant($"_{i} ({branchCode})");
					branchNamePart = new ZString(branchName).SubstringSafe(0, OrgAddress.Schema.OA_CodeMaxLength - branchCodePart.Length);
					result = branchNamePart + branchCodePart;
					if (!addressCodesMap.ContainsKey(result))
					{
						break;
					}
				}
			}

			if (!addressCodesMap.ContainsKey(result))
			{
				addressCodesMap.Add(result, address.PK);
			}

			return result;
		}

		static string GetValidStatus(string branchValidationStatus)
		{
			return branchValidationStatus == AddressValidationStatus.Invalid || branchValidationStatus == AddressValidationStatus.ToBeVerified ? AddressValidationStatus.ManuallyVerified : branchValidationStatus;
		}

		#endregion
	}
}
