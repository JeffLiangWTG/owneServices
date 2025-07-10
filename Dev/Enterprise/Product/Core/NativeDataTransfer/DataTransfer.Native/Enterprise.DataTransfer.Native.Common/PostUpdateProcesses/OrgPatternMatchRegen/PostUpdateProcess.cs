using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	class PostUpdateProcess : PostUpdateProcessHandler
	{
		internal PostUpdateProcess(IEntityContext context, IEntity rootEntity) : base(context, rootEntity)
		{
		}

		protected override bool ShouldRun
		{
			get
			{
				var statistics = Context.Statistics;
				return RootEntity != null && RootEntity.TableName == OrgHeaderSchema.Constants.TableName
					&& (statistics.GetEntityStatistics(OrgHeaderSchema.Constants.TableName).Entities.Count > 0
						|| statistics.GetEntityStatistics(OrgAddressSchema.Constants.TableName).Entities.Count > 0
						|| statistics.GetEntityStatistics(OrgCusCodeSchema.Constants.TableName).Entities.Count > 0
						|| statistics.GetEntityStatistics(OrgBrandOrRelatedNameSchema.Constants.TableName).Entities.Count > 0);
			}
		}

		protected override void UpdateCore()
		{
			var factory = new WrapperFactory(Context.RowFactory);
			var organisationWrapper = factory.Load<MatchingOrganisation>(RootEntity.InternalPK);
			if (organisationWrapper != null)
			{
				SetMatchingMainAddressPort(organisationWrapper, RootEntity);
				var dataManager = new PatternMatchDataManager(organisationWrapper, factory, Context.Connection);
				var patternMatchRebuilder = new PatternMatchReBuilder(dataManager);
				patternMatchRebuilder.Generate(null, null);
				SetUpdatedOrgAddressValidationStatus(Context.RowFactory, organisationWrapper);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		internal void SetMatchingMainAddressPort(MatchingOrganisation wrapper, IEntity rootEntity)
		{
			var child = rootEntity.Children.Where(x => x.EntityName == "OrgAddress");
			foreach (var address in child)
			{
				var addressCapability = address.Children.Where(x => x.EntityName == "OrgAddressCapability");
				foreach (var subCapability in addressCapability)
				{
					var isMain = subCapability.Properties.Any(x => x.Name == "IsMainAddress" && x.Value.Equals("true"));
					if (isMain)
					{
						var main = wrapper.Addresses.First(x => x.PK == address.InternalPK);
						main.OA_RL_NKRelatedPortCode = wrapper.OH_RL_NKClosestPort;
					}
				}
				break;
			}
		}

		internal void SetUpdatedOrgAddressValidationStatus(RowFactory factory, MatchingOrganisation organisation)
		{
			var updatedAddressesPKs = Context.Statistics.EntityAffected
				.Where(dbEntity => dbEntity.Action == DBEntity.DbAction.Update && dbEntity.Name == OrgAddressSchema.Constants.TableName)
				.Select(dbEntity => new ZGuid(dbEntity.PK)).ToList();
			var updatedOrgAddresses = organisation.Addresses.Where(address => updatedAddressesPKs.Contains(address.PK)).OfType<MatchingAddress>().ToList();
			var userCode = Env.CurrentUser != null ? new ZString(Env.CurrentUser.Initials) : ZString.Empty;
			foreach (var updatedOrgAddress in updatedOrgAddresses)
			{
				var originalStatus = updatedOrgAddress.OA_ValidationStatus;
				updatedOrgAddress.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
				var logParameters = new Dictionary<string, string>
				{
					[Params.Codes.New] = updatedOrgAddress.OA_ValidationStatus,
					[Params.Codes.Old] = originalStatus,
				};

				var row = factory.New(StmALogSchema.Constants.TableName);
				row[StmALogSchema.Constants.PK] = Guid.NewGuid();
				row[StmALogSchema.Constants.SL_IsEstimate] = false;
				row[StmALogSchema.Constants.SL_IsCancelled] = false;
				row[StmALogSchema.Constants.SL_FireWorkflow] = false;
				row[StmALogSchema.Constants.SL_Reference] = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(string.Empty, logParameters);
				row[StmALogSchema.Constants.SL_PostedTimeUtc] = ZDateTime.UtcNow.ToDateTime();
				row[StmALogSchema.Constants.SL_EventTime] = ZDateTime.Now.ToDateTime();
				row[StmALogSchema.Constants.SL_GS_NKUser] = userCode;
				row[StmALogSchema.Constants.SL_Table] = OrgAddressSchema.Constants.TableName;
				row[StmALogSchema.Constants.SL_Parent] = updatedOrgAddress.PK.ToGuid();
				row[StmALogSchema.Constants.SL_SE_NKEvent] = AutoEvents.StatusChange.Code;
				row.Table.Rows.Add(row);
			}
		}
	}
}
