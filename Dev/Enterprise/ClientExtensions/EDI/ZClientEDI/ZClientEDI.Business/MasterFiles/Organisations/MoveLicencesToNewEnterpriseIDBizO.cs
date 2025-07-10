using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public interface IMoveLicencesToNewEnterpriseIDBizOCallbacks
	{
		void NotifyFailure(string serverCode);
		void NotifyError(string errorMessage);
		void NotifyTargetSameAsSource();
		void NotifyLicenceEnterpriseIDDoesNotExist(string id);
	}

	public class MoveLicencesToNewEnterpriseIDBizO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MoveLicencesToNewEnterpriseIDBizO(LicenceEnterprise licEnt, EDIOrgHeader orgHeader, IMoveLicencesToNewEnterpriseIDBizOCallbacks moveLicencesToNewEnterpriseIDBizOCallbacks)
			: base(licEnt.Factory)
		{
			licEntToCopyFrom = licEnt;
			currentOrgHeader = orgHeader;
			callbacks = moveLicencesToNewEnterpriseIDBizOCallbacks;
		}

		readonly IMoveLicencesToNewEnterpriseIDBizOCallbacks callbacks;

		#region MoveLicencesToNewEnterpriseID method

		public virtual void MoveLicencesToNewEnterpriseID()
		{
			LicenceCompany licCompany = currentOrgHeader.LicCompany;

			LicenceDatabase[] sourceLicDbList = licEntToCopyFrom.Databases
				.Where(sourceLicDb => licCompany.LicDatabases.Contains(sourceLicDb))
				.Cast<LicenceDatabase>().ToArray();

			if (Check_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes(sourceLicDbList) && ShouldMoveLicencesToNewEnterprise(sourceLicDbList))
			{
				licCompany.LC_LE = licEntToCopyTo.PK;
				List<LicenceDatabase> licDBsToDelete = new List<LicenceDatabase>(licEntToCopyFrom.Databases.Count);
				List<LicenceDatabase> licDBsToRemoveFromLicCompany = new List<LicenceDatabase>();

				foreach (LicenceDatabase sourceLicDb in sourceLicDbList)
				{
					LicenceDatabase existingTargetDb = GetExistingTargetLicDatabase(sourceLicDb);
					var licHeader = licCompany.GetHeader(sourceLicDb);
					if (!HasOtherCompaniesInstalled(sourceLicDb))
					{
						if (existingTargetDb == null)
						{
							sourceLicDb.LD_OH_WebAccessOrg = licEntToCopyTo.LE_OH;
							licEntToCopyFrom.Databases.Remove(sourceLicDb);
							sourceLicDb.LD_ServerCode = GetAutoAdjustedLD_ServerCode(sourceLicDb.LD_ServerCode);
							licEntToCopyTo.Databases.Add(sourceLicDb);
							sourceLicDb.Logs.AddNew(Events.MessageStatusChange, "LicenceDataBase enterpriseCode change from '" + licEntToCopyFrom.LE_EnterpriseCode + "' to '" + licEntToCopyTo.LE_EnterpriseCode + "'");
						}
						else
						{
							licDBsToDelete.Add(sourceLicDb);
							RelinkLicenceHeaderTo(licHeader, existingTargetDb);
							licDBsToRemoveFromLicCompany.Add(sourceLicDb);
						}
					}
					else
					{
						var targetLicDb = existingTargetDb ?? CreateNewTargetLicDatabase(sourceLicDb);
						RelinkLicenceHeaderTo(licHeader, targetLicDb);
						licDBsToRemoveFromLicCompany.Add(sourceLicDb);
					}
				}

				licCompany.LicDatabases.Load();
				foreach (LicenceDatabase sourceLicDb in licDBsToRemoveFromLicCompany)
				{
					if (licCompany.LicDatabases.Contains(sourceLicDb))
					{
						licCompany.LicDatabases.Remove(sourceLicDb);
					}
				}

				foreach (LicenceDatabase sourceLicDb in licDBsToDelete)
				{
					sourceLicDb.Delete();
				}

				foreach (LicenceDatabase targetLicDb in licCompany.LicDatabases)
				{
					targetLicDb.Validation.ValidateAll();
					licCompany.GetHeader(targetLicDb).RefreshBinding();
				}

				currentOrgHeader.ReloadLicEnterprise();
				currentOrgHeader.RefreshBinding();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				currentOrgHeader.Logs.AddNew(Events.EditedARecord, "moved Licences from enterprise ID '" + licEntToCopyFrom.LE_EnterpriseID + "' to enterprise ID '" + licEntToCopyTo.LE_EnterpriseID + "'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		LicenceDatabase CreateNewTargetLicDatabase(LicenceDatabase sourceLicDb)
		{
			LicenceDatabase result = licEntToCopyTo.Databases.AddNew();
			result.CopyPersistentValuesFrom(sourceLicDb);
			result.LD_ServerCode = GetAutoAdjustedLD_ServerCode(result.LD_ServerCode);
			result.LD_LE = licEntToCopyTo.PK;
			return result;
		}

		LicenceDatabase GetExistingTargetLicDatabase(LicenceDatabase sourceLicDb)
		{
			ZQuery query = new ZQuery(LicenceDatabaseSchema.LD_LE, licEntToCopyTo.PK);
			query.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_ServerCode, sourceLicDb.LD_ServerCode);
			query.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_PublicEmailAddressForUpdate, sourceLicDb.LD_PublicEmailAddressForUpdate);
			query.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.PK, SQLComparisonOperator.NotEqual, sourceLicDb.PK);
			LicenceDatabase[] queryResult = Factory.Load<LicenceDatabase>(query);
			return (queryResult.Length > 0) ? queryResult[0] : null;
		}

		void RelinkLicenceHeaderTo(LicenceHeader sourceLicHeader, LicenceDatabase targetLicDb)
		{
			LicenceCompany licCompany = sourceLicHeader.Company;

			if (!licCompany.LicDatabases.Contains(targetLicDb))
			{
				sourceLicHeader.Database.LicHeadersForAllCompanies.Remove(sourceLicHeader);
				sourceLicHeader.LA_LD = targetLicDb.PK;
				targetLicDb.LicHeadersForAllCompanies.Add(sourceLicHeader);
			}
			else
			{
				throw new InvalidOperationException("Organization already exists in target enterprise");
			}
		}

		protected virtual ZString GetAutoAdjustedLD_ServerCode(ZString serverCodeToAdjust)
		{
			ZString result = serverCodeToAdjust;

			if (!ServerCodeExists(licEntToCopyTo, result))
			{
				return result;
			}

			for (int i = 1; i < 10; i++)
			{
				result = serverCodeToAdjust.Substring(0, 2) + i.ToString(CultureInfo.InvariantCulture);
				if (!ServerCodeExists(licEntToCopyTo, result))
				{
					return result;
				}
			}

			for (int i = 10; i < 100; i++)
			{
				result = serverCodeToAdjust.Substring(0, 1) + i.ToString(CultureInfo.InvariantCulture);
				if (!ServerCodeExists(licEntToCopyTo, result))
				{
					return result;
				}
			}

			throw new InvalidOperationException("cannot adjust code");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "this functionality is for edi internal use only so should not cause any culture-specific problems")]
		protected virtual bool Check_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes(LicenceDatabase[] sourceLicDbList)
		{
			bool canCopy = true;
			foreach (LicenceDatabase sourceLicDb in sourceLicDbList)
			{
				try
				{
					GetAutoAdjustedLD_ServerCode(sourceLicDb.LD_ServerCode);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					canCopy = false;
					if (ex.Message == "cannot adjust code")
					{
						callbacks.NotifyFailure(sourceLicDb.LD_ServerCode);
					}
					else
					{
						throw;
					}
				}
			}
			return canCopy;
		}

		bool ShouldMoveLicencesToNewEnterprise(LicenceDatabase[] sourceLicDbList)
		{
			var result = true;
			foreach (var sourceLicDb in sourceLicDbList.Where(x => !x.LD_TenantID.IsEmpty))
			{
				if (HasOtherCompaniesInstalled(sourceLicDb) && GetExistingTargetLicDatabase(sourceLicDb) == null)
				{
					callbacks.NotifyError(
$@"Licence Database with the server code '{sourceLicDb.LD_ServerCode}' cannot be copied because it is linked by multiple organizations and it has a unique Tenant ID.
No Licences will be copied.
Please call the administrator to resolve this problem.");
					result = false;
					break;
				}
			}
			return result;
		}

		bool ServerCodeExists(LicenceEnterprise targetLicEnt, ZString serverCode)
		{
			ZQuery query = new ZQuery(LicenceDatabaseSchema.LD_LE, targetLicEnt.PK);
			query.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_ServerCode, serverCode);
			return (Factory.Load<LicenceDatabase>(query).Length > 0);
		}

		bool HasOtherCompaniesInstalled(LicenceDatabase sourceLicDb)
		{
			ZQuery query = new ZQuery(LicenceHeaderSchema.LA_LD, sourceLicDb.PK);
			query.AddToFilter(LicenceHeaderSchema.LA_LC, SQLComparisonOperator.NotEqual, currentOrgHeader.LicCompany.PK);
			return (Factory.Load<LicenceHeader>(query).Length > 0);
		}

		#endregion

		public LicenceEnterprise licEntToCopyFrom
		{
			get;
			set;
		}

		public LicenceEnterpriseCollection LicenceEnterpriseList
		{
			get
			{
				if (fLicenceEnterpriseList == null)
				{
					fLicenceEnterpriseList = new LicenceEnterpriseCollection(Factory, new ZQuery(LicenceEnterpriseSchema.PK, SQLComparisonOperator.NotEqual, licEntToCopyFrom.PK));
				}
				return fLicenceEnterpriseList;
			}
		}
		LicenceEnterpriseCollection fLicenceEnterpriseList;

		[BusinessObjectTestExclude]
		[CargoWise.ComponentModel.MaxLength(7)]
		public ZString LicenceEnterpriseID
		{
			get { return licEntToCopyTo != null ? licEntToCopyTo.LE_EnterpriseID : ZString.Empty; }
			set
			{
				CheckMaximumLength(LicenceEnterpriseIDInfo, value);

				var result = Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, value));
				if (result.Length == 1)
				{
					if (value != licEntToCopyFrom.LE_EnterpriseID)
					{
						licEntToCopyTo = result[0];
						licEntToCopyTo.HasChanges = true;
					}
					else
					{
						callbacks.NotifyTargetSameAsSource();
						licEntToCopyTo = null;
					}
				}
				else
				{
					callbacks.NotifyLicenceEnterpriseIDDoesNotExist(value);
					licEntToCopyTo = null;
				}

				RegisterEditableChildObject(licEntToCopyTo);

				LicenceEnterpriseIDInfo.RefreshBinding();
			}
		}

		protected LicenceEnterprise licEntToCopyTo
		{
			get;
			set;
		}

		public ZPropertyInfo LicenceEnterpriseIDInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceEnterpriseID)); }
		}

		EDIOrgHeader currentOrgHeader
		{
			get;
			set;
		}
	}
}
