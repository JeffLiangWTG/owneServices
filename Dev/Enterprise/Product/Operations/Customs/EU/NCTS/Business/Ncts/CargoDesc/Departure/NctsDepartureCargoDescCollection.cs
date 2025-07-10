using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDepartureCargoDescCollection<out T> : INctsCommonCargoDescCollection<T>
		where T : NctsDepartureCargoDesc
	{
		new T this[int index] { get; }
		ZDecimal InitialApportionedAmount { get; set; }
		bool CopyLastGoodsItemToNewLines { get; set; }
	}

	public class NctsDepartureCargoDescCollection<T> : NctsCommonCargoDescCollection<T>, INctsDepartureCargoDescCollection<T>
		where T : NctsDepartureCargoDesc
	{
		public NctsDepartureCargoDescCollection(NctsCommonMovementHeader movementHeader)
			: base(movementHeader)
		{
		}

		public NctsDepartureCargoDescCollection(NctsBill nctsBill)
			: base(nctsBill)
		{
			if (!nctsBill.Header?.IsDepartureMovement ?? false)
			{
				AdditionalFilter = new ZQuery { IsNoResultQuery = true };
			}
			EnableMaxCountValidation(nctsBill);
		}

		const int MaxGoodsItemsInTransitionPeriod = 999;
		const int MaxGoodsItemsOutsideTransitionPeriod = 1999;

		void EnableMaxCountValidation(NctsBill nctsBill)
		{
			if (nctsBill.Header is NctsHeader header)
			{
				if (header.IsInPhase5TransitionPeriod)
				{
					this.EnableMaxCountValidation(MaxGoodsItemsInTransitionPeriod, Res.GetString("A91EB0FB-57E2-4CE5-AEF2-F39DA527E2C7", "You may enter a maximum of {0} Goods Items.", MaxGoodsItemsInTransitionPeriod), warnAtHalfway: false);
				}
				else
				{
					this.EnableMaxCountValidation(MaxGoodsItemsOutsideTransitionPeriod, Res.GetString("8D04B87F-1537-41FB-9790-3EE5BEFD0976", "[G0005] You may enter a maximum of {0} Goods Items.", MaxGoodsItemsOutsideTransitionPeriod), warnAtHalfway: false);
				}
			}
		}

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var desc = newElement;

			using (desc.GetValidationSuspender())
			using (desc.SuspendSettingHasChanges())
			{
				if (Relationship.Master is NctsBill { IsDeleted: false, IsPhase5Departure: true } nctsBill)
				{
					desc.BY_RX_NKLinePriceCurrency = nctsBill.B0_RX_NKLinePriceCurrency;
				}
				if (Count == 0)
				{
					SetDefaultForFirstDesc(desc);
				}
				else if (Count > 0)
				{
					var previousDesc = this[Count - 1];

					SetDefaultFromPreviousDesc(previousDesc, desc);
				}
			}
		}

		protected virtual void SetDefaultForFirstDesc(T firstDesc)
		{
			firstDesc.BY_RN_NKCountryOfOrigin = firstDesc.Header is NctsHeader header && !header.IsPhase5 ? header.PortOfDispatch.Left(2) : ZString.Empty;
		}

		protected virtual void SetDefaultFromPreviousDesc(T previousDesc, T currentDesc)
		{
			currentDesc.BY_RN_NKCountryOfOrigin = previousDesc.BY_RN_NKCountryOfOrigin;

			CopyLastGoodsItemToNewLinesIfEnabled(currentDesc, previousDesc);
		}

		protected override void OnAdded(T businessObject)
		{
			base.OnAdded(businessObject);
			if (Parent is NctsBill nctsBill)
			{
				var header = nctsBill.Header;
				header?.Validation.ValidateMaxGoodsItemsForAllBills();
			}
		}

		public override void Delete(T businessObject)
		{
			if (businessObject.IsDeleted)
			{
				base.Delete(businessObject);
				return;
			}

			var header = businessObject.Bill?.Header;

			base.Delete(businessObject);

			if (header != null && !header.IsDeleted && !header.IsDeleting)
			{
				header.Validation.ValidateMaxGoodsItemsForAllBills();
			}
		}

		public ZDecimal InitialApportionedAmount { get; set; }

		protected override void OnLoadedIntoCollectionCore(T loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			if (Parent is NctsBill bill && bill.Header != null && !bill.Header.ReadOnly)
			{
				InitialApportionedAmount = InitialApportionedAmount + loadedObject.VatAmount + loadedObject.DutyAmount + loadedObject.AntiDumpingDutyAmount + loadedObject.CountervailingDutyAmount;
			}
		}

		protected void CopyLastGoodsItemToNewLinesIfEnabled(T newGoodsItem, T previousGoodsItem)
		{
			if (CopyLastGoodsItemToNewLines)
			{
				newGoodsItem.CopyPersistentValuesFrom(previousGoodsItem, new BusinessObjectCloneArgs(new[] { CusInBondCargoDescSchema.Constants.BY_ParentID, CusInBondCargoDescSchema.Constants.BY_LineNo, CusInBondCargoDescSchema.Constants.BY_NetWeightUnit, CusInBondCargoDescSchema.Constants.BY_GrossWeightUnit }));

				foreach (var supportingDocument in previousGoodsItem.SupportingDocuments.ToArray())
				{
					newGoodsItem.SupportingDocuments.Add(supportingDocument.Clone());
				}
				foreach (var additionalInfo in previousGoodsItem.AdditionalInfos.ToArray())
				{
					newGoodsItem.AdditionalInfos.Add(additionalInfo.Clone());
				}
				foreach (var package in previousGoodsItem.Packages.ToArray())
				{
					var newPack = (NctsPackage)package.Clone();
					newGoodsItem.Packages.Add(newPack);
					if (previousGoodsItem.Header.IsPhase5)
					{
						((NctsPackage)package).ContainersPivot.Cast<NctsCusInBondContainerPackageGenPivot>().ForEach(x => newPack.ContainersPivot.AddPivotFor(x.Container));
					}
				}
				foreach (var previousDocument in previousGoodsItem.PreviousDocuments.ToArray())
				{
					newGoodsItem.PreviousDocuments.Add(previousDocument.Clone());
				}
				foreach (var supplyChainActorReference in previousGoodsItem.CusSupplyChainActorReferences.ToArray())
				{
					newGoodsItem.CusSupplyChainActorReferences.Add(supplyChainActorReference.Clone());
				}
				foreach (var fee in previousGoodsItem.Fees.ToArray())
				{
					newGoodsItem.Fees.Add(fee.Clone());
				}
				foreach (var undg in previousGoodsItem.UNDGs.ToArray())
				{
					newGoodsItem.UNDGs.Add((UNDGDataItem)undg.Clone());
				}
				foreach (var docAddress in previousGoodsItem.DocAddresses.ToArray())
				{
					newGoodsItem.DocAddresses.Add(docAddress.Clone());
				}
				foreach (var additionalSupplementaryCode in previousGoodsItem.AdditionalSupplementaryCodes.ToArray())
				{
					newGoodsItem.AdditionalSupplementaryCodes.Add(additionalSupplementaryCode.Clone());
				}
			}
		}

		public bool CopyLastGoodsItemToNewLines { get; set; }
	}
}
