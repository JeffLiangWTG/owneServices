using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public NctsDeepCloneStrategy(BusinessObject bizObjToClone, ZGuid clonedParentPK)
			: this(bizObjToClone, clonedParentPK, null)
		{
		}

		public NctsDeepCloneStrategy(BusinessObject bizObjToClone, ZGuid clonedParentPK, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(bizObjToClone, CloneType.DeepTemplateCopy, alternativeFactoryToInstantiateCloneIn)
		{
			this.clonedParentPK = clonedParentPK;
		}
		readonly ZGuid clonedParentPK;

		public override BusinessObject Clone()
		{
			var args = GetCloneArgs(bizObjToClone.GetType(), clonedParentPK, alternativeFactoryToInstantiateCloneIn);
			var result = Clone(args);
			if (IsTemplateCopy)
			{
				foreach (var dynamicValue in bizObjToClone.GetSystemDefinedValues())
				{
					result.SetSystemDefinedValue(dynamicValue.PropertyName, dynamicValue.Value);
				}

				foreach (var dynamicValue in bizObjToClone.GetUserDefinedValues())
				{
					result.SetUserDefinedValue(dynamicValue.PropertyName, dynamicValue.Value);
				}
			}

			if (bizObjToClone is IDocAddresses sourceAddresses
				&& result is IDocAddresses targetAddresses)
			{
				CopyJobDocAddresses(sourceAddresses, targetAddresses, result.PK);
			}

			return result;
		}

		static void CopyJobDocAddresses(IDocAddresses sourceAddresses, IDocAddresses targetAddresses, ZGuid parentPK)
		{
			targetAddresses.DocAddresses.RemoveAndDeleteAll();
			foreach (var address in sourceAddresses.DocAddresses.Cast<JobDocAddress>())
			{
				if (targetAddresses.SupportedAddressTypes.Contains(address.DocAddressType))
				{
					var clonedAddress = (JobDocAddress)new NctsDeepCloneStrategy(address, parentPK).Clone();
					using (clonedAddress.GetValidationSuspender())
					using (clonedAddress.SuspendSettingHasChanges())
					{
						targetAddresses.DocAddresses.Add(clonedAddress);
					}
					clonedAddress.HasChanges = !clonedAddress.IsEmpty;
				}
			}
		}

		static BusinessObjectCloneArgs GetCloneArgs(Type typeOfBusinessObjectToClone, ZGuid parentForeignKey, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			var createArgsData = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone, alternativeFactoryToInstantiateCloneIn, CompleteArgsForDeepTemplateCopy);
			BusinessObjectCloneArgs result = null;
			if (createArgsData != null)
			{
				(result, var parentForeignKeyColumnName) = createArgsData(alternativeFactoryToInstantiateCloneIn);
				if (!string.IsNullOrEmpty(parentForeignKeyColumnName))
				{
					result.AddValueOverride(typeOfBusinessObjectToClone, parentForeignKeyColumnName, parentForeignKey);
				}
			}
			return result ?? new BusinessObjectCloneArgs();
		}

		static Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)> GetCloneArgsRecursivelyUntilFindKey(Type typeOfBusinessObjectToClone, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn, Dictionary<Type, Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)>> completeArgs)
		{
			Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)> createArgsData = null;
			if (typeOfBusinessObjectToClone != null)
			{
				if (!completeArgs.TryGetValue(typeOfBusinessObjectToClone, out createArgsData))
				{
					createArgsData = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone.BaseType, alternativeFactoryToInstantiateCloneIn, completeArgs);
					if (createArgsData != null)
					{
						lock (completeArgs)
						{
							completeArgs.Add(typeOfBusinessObjectToClone, createArgsData);
						}
					}
				}
			}
			return createArgsData;
		}

		[ThreadStatic] static Dictionary<Type, Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)>> completeArgsForDeepTemplateCopy;

		static Dictionary<Type, Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)>> CompleteArgsForDeepTemplateCopy => completeArgsForDeepTemplateCopy ?? (completeArgsForDeepTemplateCopy = GetDeepTemplateCopyArgs());

		static BusinessObjectCloneArgs CreateBusinessObjectCloneArgs(BusinessObjectFactory factory, IEnumerable<string> columnNamesToExcludeFromCopy)
		{
			return factory == null
				? new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy, true)
				: new BusinessObjectCloneArgs(factory, columnNamesToExcludeFromCopy, null, true);
		}

		// List of supported cloneable NCTS bizOs and their excluded properties.
		static Dictionary<Type, Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)>> GetDeepTemplateCopyArgs() => new Dictionary<Type, Func<BusinessObjectFactory, (BusinessObjectCloneArgs args, string parentForeignKeyColumnName)>>
		{
			{
				typeof(NctsHeader),
				(factory) => (CreateBusinessObjectCloneArgs(factory,
					new[]
					{
						CusInBondHeaderSchema.Constants.BH_JobReference,
						CusInBondHeaderSchema.Constants.BH_CustomsProfile
					}), string.Empty)
			},
			{
				typeof(NctsCommonMovementHeader),
				(factory) => (CreateBusinessObjectCloneArgs(factory,
					new[]
					{
						CusInBondMoveHeaderSchema.Constants.BM_BH,
						CusInBondMoveHeaderSchema.Constants.BM_GS_NKCusAgent,
						CusInBondMoveHeaderSchema.Constants.BM_CustomsStatus,
						CusInBondMoveHeaderSchema.Constants.BM_PaperlessInbondNum,
						CusInBondMoveHeaderSchema.Constants.BM_Phase,
						CusInBondMoveHeaderSchema.Constants.BM_EntryDate,
						CusInBondMoveHeaderSchema.Constants.BM_BM_DepartureMovement,
						CusInBondMoveHeaderSchema.Constants.BM_WarehouseTransactionStatus,
						CusInBondMoveHeaderSchema.Constants.BM_ValuationDate,
					}), CusInBondMoveHeaderSchema.Constants.BM_BH)
			},
			{
				typeof(NctsArrivalMovementHeader),
				(factory) => (CreateBusinessObjectCloneArgs(factory,
					new[]
					{
						CusInBondMoveHeaderSchema.Constants.BM_BH,
						CusInBondMoveHeaderSchema.Constants.BM_GS_NKCusAgent,
						CusInBondMoveHeaderSchema.Constants.BM_CustomsStatus,
						CusInBondMoveHeaderSchema.Constants.BM_PaperlessInbondNum,
						CusInBondMoveHeaderSchema.Constants.BM_Phase,
						CusInBondMoveHeaderSchema.Constants.BM_EntryDate,
						CusInBondMoveHeaderSchema.Constants.BM_BM_DepartureMovement,
						CusInBondMoveHeaderSchema.Constants.BM_WarehouseTransactionStatus,
						CusInBondMoveHeaderSchema.Constants.BM_ValuationDate,
						CusInBondMoveHeaderSchema.Constants.BM_UnloadingDate,
						CusInBondMoveHeaderSchema.Constants.BM_NoChangesToReport,
						CusInBondMoveHeaderSchema.Constants.BM_StateOfSeals,
						CusInBondMoveHeaderSchema.Constants.BM_UnloadingCompleted,
						CusInBondMoveHeaderSchema.Constants.BM_UnloadingRemarks,
						CusInBondMoveHeaderSchema.Constants.BM_GrossWeight,
						CusInBondMoveHeaderSchema.Constants.BM_GrossWeightUQ,
						CusInBondMoveHeaderSchema.Constants.BM_GrossWeightUnloaded,
						CusInBondMoveHeaderSchema.Constants.BM_InlandTransportMode,
					}), CusInBondMoveHeaderSchema.Constants.BM_BH)
			},
			{
				typeof(NctsCommonCargoDesc),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusInBondCargoDescSchema.Constants.BY_ParentID }), CusInBondCargoDescSchema.Constants.BY_ParentID)
			},
			{
				typeof(NctsContainer),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusInBondContainerSchema.Constants.BC_ParentID }), CusInBondContainerSchema.Constants.BC_ParentID)
			},
			{
				typeof(NctsPackage),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusInvPackSchema.Constants.B5_ParentID }), CusInvPackSchema.Constants.B5_ParentID)
			},
			{
				typeof(JobDocAddress),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { JobDocAddressSchema.Constants.E2_ParentID }), JobDocAddressSchema.Constants.E2_ParentID)
			},
			{
				typeof(UNDGDataItem),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { UNDGDataItemSchema.Constants.DI_ParentID }), UNDGDataItemSchema.Constants.DI_ParentID)
			},
			{
				typeof(SupplementaryCode),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusCodeDataSchema.Constants.CY_ParentID }), CusCodeDataSchema.Constants.CY_ParentID)
			},
			{
				typeof(NctsCargoDescFee),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusInBondFeeSchema.Constants.BFE_BY }), CusInBondFeeSchema.Constants.BFE_BY)
			},
			{
				typeof(CusAuthorizationUsage),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusAuthorizationUsageSchema.Constants.AGC_ParentID }), CusAuthorizationUsageSchema.Constants.AGC_ParentID)
			},
			{
				typeof(NctsSupportingDocument),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusSupportingInfoSchema.Constants.CSI_ParentID }), CusSupportingInfoSchema.Constants.CSI_ParentID)
			},
			{
				typeof(CommonPreviousDocument),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusSupportingInfoSchema.Constants.CSI_ParentID }), CusSupportingInfoSchema.Constants.CSI_ParentID)
			},
			{
				typeof(NctsAdditionalInfo),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusSupportingInfoSchema.Constants.CSI_ParentID }), CusSupportingInfoSchema.Constants.CSI_ParentID)
			},
			{
				typeof(CountryOfRouting),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusCodeDataSchema.Constants.CY_ParentID }), CusCodeDataSchema.Constants.CY_ParentID)
			},
			{
				typeof(CusSupplyChainActorReference),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusReferenceSchema.Constants.CFR_ParentID }), CusReferenceSchema.Constants.CFR_ParentID)
			},
			{
				typeof(DepartureCusTransportMeans),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusTransportMeansSchema.Constants.TPM_ParentID }), CusTransportMeansSchema.Constants.TPM_ParentID)
			},
			{
				typeof(NctsBill),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusInBondBillSchema.Constants.B0_BH }), CusInBondBillSchema.Constants.B0_BH)
			},
			{
				typeof(NctsBillAdditionalDocument),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusSupportingInfoSchema.Constants.CSI_ParentID }), CusSupportingInfoSchema.Constants.CSI_ParentID)
			},
			{
				typeof(CusSupportingInfo),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusSupportingInfoSchema.Constants.CSI_ParentID }), CusSupportingInfoSchema.Constants.CSI_ParentID)
			},
			{
				typeof(NctsDepartureCargoDesc),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusInBondCargoDescSchema.Constants.BY_ParentID }), CusInBondCargoDescSchema.Constants.BY_ParentID)
			},
			{
				typeof(NctsPreviousDocument),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusSupportingInfoSchema.Constants.CSI_ParentID }), CusSupportingInfoSchema.Constants.CSI_ParentID)
			},
			{
				typeof(CusGoodsLocation),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusGoodsLocationSchema.Constants.CGL_ParentID }), CusGoodsLocationSchema.Constants.CGL_ParentID)
			},
			{
				typeof(NctsEuOfficeCode),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusCodeDataSchema.Constants.CY_ParentID }), CusCodeDataSchema.Constants.CY_ParentID)
			},
			{
				typeof(NctsGuarantee),
				(factory) => (CreateBusinessObjectCloneArgs(factory, new[] { CusBondDetailSchema.Constants.PW_ParentID }), CusBondDetailSchema.Constants.PW_ParentID)
			}
		};
	}
}
