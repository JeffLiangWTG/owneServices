using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DESREMConsignmentItemProvider : IDESREMConsignmentItem
	{
		public DESREMConsignmentItemProvider(NctsArrivalCargoDesc cargoDesc)
		{
			this.cargoDesc = Argument.NotNull(cargoDesc, nameof(cargoDesc));
		}
		readonly NctsArrivalCargoDesc cargoDesc;

		public int SequenceNumber => cargoDesc.BY_LineNo;

		public string DescriptionOfGoods
		{
			get
			{
				string result = null;
				if (UnloadedStateIsNEW)
				{
					result = cargoDesc.BY_Description;
				}
				else if (UnloadedStateIsDIF)
				{
					result = cargoDesc.UnloadedGoodsItem.BY_Description;
				}
				return result;
			}
		}

		public string CusCode
		{
			get
			{
				string result = null;
				var declaredValue = ((string)cargoDesc.BY_CusC4Number).ValueOrNullIfEmpty();
				var unloadedValue = ((string)cargoDesc.UnloadedGoodsItem?.BY_CusC4Number).ValueOrNullIfEmpty();
				if (UnloadedStateIsNEW)
				{
					result = declaredValue;
				}
				else if (UnloadedStateIsDIF && declaredValue != unloadedValue)
				{
					result = unloadedValue;
				}
				return result;
			}
		}

		public string HarmonizedSystemSubheadingCode
		{
			get
			{
				string result = null;
				var declaredValue = ((string)cargoDesc.BY_HarmonisedTariff).LeftOrNull(8);
				var unloadedValue = ((string)cargoDesc.UnloadedGoodsItem?.BY_HarmonisedTariff).LeftOrNull(8);
				if (UnloadedStateIsNEW)
				{
					result = declaredValue.LeftOrNull(6);
				}
				else if (UnloadedStateIsDIF && declaredValue != unloadedValue)
				{
					result = unloadedValue.LeftOrNull(6);
				}
				return result;
			}
		}

		public string CombinedNomenclatureCode
		{
			get
			{
				string result = null;
				var declaredValue = ((string)cargoDesc.BY_HarmonisedTariff).SubstringOrNull(6, 2);
				var unloadedValue = ((string)cargoDesc.UnloadedGoodsItem?.BY_HarmonisedTariff).SubstringOrNull(6, 2);
				if (UnloadedStateIsNEW && declaredValue != null && declaredValue.Length == 2)
				{
					result = declaredValue;
				}
				else if (UnloadedStateIsDIF && unloadedValue != null && unloadedValue.Length == 2 && declaredValue != unloadedValue)
				{
					result = unloadedValue;
				}
				return result;
			}
		}

		public decimal? GrossMass
		{
			get
			{
				decimal? result = null;
				var declaredValue = new ZWeight(cargoDesc.BY_GrossWeight, cargoDesc.BY_GrossWeightUnit).InKilogramsSafe.Round(3).Normalize();
				var unloadedValue = new ZWeight(cargoDesc.UnloadedGoodsItem?.BY_GrossWeight ?? ZDecimal.Zero, cargoDesc.UnloadedGoodsItem?.BY_GrossWeightUnit ?? ZString.Empty).InKilogramsSafe.Round(3).Normalize();
				if (UnloadedStateIsNEW)
				{
					result = declaredValue;
				}
				else if (UnloadedStateIsDIF && declaredValue != unloadedValue)
				{
					result = unloadedValue;
				}
				return result;
			}
		}

		public decimal? NetMass
		{
			get
			{
				decimal? result = decimal.Zero;
				var declaredValue = new ZWeight(cargoDesc.BY_NetWeight, cargoDesc.BY_NetWeightUnit).InKilogramsSafe.Round(6).Normalize();
				var unloadedValue = new ZWeight(cargoDesc.UnloadedGoodsItem?.BY_NetWeight ?? ZDecimal.Zero, cargoDesc.UnloadedGoodsItem?.BY_NetWeightUnit ?? ZString.Empty).InKilogramsSafe.Round(6).Normalize();
				if (cargoDesc.IsInPhase5TransitionPeriod)
				{
					declaredValue = declaredValue.Round(3).Normalize();
					unloadedValue = unloadedValue.Round(3).Normalize();
				}
				if (UnloadedStateIsNEW)
				{
					result = declaredValue;
				}
				else if (UnloadedStateIsDIF && declaredValue != unloadedValue)
				{
					result = unloadedValue;
				}
				return result > decimal.Zero ? result : null;
			}
		}

		public IReadOnlyCollection<IDESREMPackage> Packaging =>
			packaging ?? (packaging = (UnloadedStateIsNEW || UnloadedStateIsDIF)
					? cargoDesc.Packages
						.Cast<NctsPackage>()
						.Where(x => CargoWise.Common.IEnumerableExtensions.In(x.B5_TypeOfDifference, new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
						.Select(y => new DESREMPackageProvider(y))
						.ToArray()
					: Array.Empty<IDESREMPackage>());
		IReadOnlyCollection<IDESREMPackage> packaging;

		bool UnloadedStateIsNEW => CachedValueHelper.GetValue(ref unloadedStateIsNEW, () => cargoDesc.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW);
		CachedValue<bool> unloadedStateIsNEW;

		bool UnloadedStateIsDIF => CachedValueHelper.GetValue(ref unloadedStateIsDIF, () => cargoDesc.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF);
		CachedValue<bool> unloadedStateIsDIF;
	}
}
