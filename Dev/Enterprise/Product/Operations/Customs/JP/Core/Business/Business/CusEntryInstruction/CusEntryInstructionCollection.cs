using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.JP.Business
{
	public class CusEntryInstructionCollection : CusEntryInstructionCollection<CusEntryInstruction>, ISequenceNumberHeader
	{
		public CusEntryInstructionCollection(JobDeclaration master)
			: base(master)
		{
		}

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this;

		public ShortSequenceNumberGenerator SeqNumberGenerator => seqNumberGenerator ?? (seqNumberGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator seqNumberGenerator;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var declaration = Master;
			var goodsDescription = declaration.JE_GoodsDescription;
			if (child is CusEntryInstruction entryInstruction)
			{
				if (!goodsDescription.IsEmpty)
				{
					entryInstruction.CEI_GoodsDescription = goodsDescription.Left(AutoJPCusEntryInstruction.Schema.CEI_GoodsDescriptionMaxLength);
				}

				DefaultCargoQuantity(declaration, entryInstruction);
				DefaultGrossWeightAndVolume(declaration, entryInstruction);
			}
		}

		void DefaultGrossWeightAndVolume(BaseJobDeclaration declaration, CusEntryInstruction entryInstruction)
		{
			var totalWeightUnit = declaration.JE_TotalWeightUnit;
			if (!totalWeightUnit.IsEmpty)
			{
				entryInstruction.CEI_GrossWeightUnit = totalWeightUnit;
				var remainingWeight = declaration.JE_TotalWeight - this.ToList<CusEntryInstruction>().Sum(x => Core.Constants.Weight.ConvertSafe(x.CEI_GrossWeight, x.CEI_GrossWeightUnit, totalWeightUnit));
				entryInstruction.CEI_GrossWeight = Math.Max(remainingWeight, 0);
			}

			var totalVolumeUnit = declaration.JE_TotalVolumeUnit;
			if (!declaration.JE_TotalVolumeUnit.IsEmpty)
			{
				entryInstruction.CEI_VolumeUnit = totalVolumeUnit;
				var remainingVolume = declaration.JE_TotalVolume - this.ToList<CusEntryInstruction>().Sum(x => Core.Constants.Volume.ConvertSafe(x.CEI_Volume, x.CEI_VolumeUnit, totalVolumeUnit));
				entryInstruction.CEI_Volume = Math.Max(remainingVolume, 0);
			}
		}

		void DefaultCargoQuantity(BaseJobDeclaration declaration, CusEntryInstruction entryInstruction)
		{
			var totalNoOfPacksPackType = declaration.JE_TotalNoOfPacksPackType;
			var refPack = CusRefPacksHelper.LoadRefPack(entryInstruction.Factory, totalNoOfPacksPackType, RPTypeList.Codes.DeclarationTotal, Core.Constants.CountryCodes.Japan);
			if (refPack != null)
			{
				var cargoQuantityUnit = refPack.RP_CustomsPack;
				var cargoQuantityFactor = refPack.RP_ConversionFactor;
				if (cargoQuantityFactor > 0)
				{
					var remainingQuantity = Math.Max(declaration.JE_TotalNoOfPacks - this.Cast<CusEntryInstruction>().Sum(c => c.CEI_CargoQuantityUnit == cargoQuantityUnit ? c.CEI_CargoQuantity / cargoQuantityFactor : 0), 0);
					entryInstruction.CEI_CargoQuantity = remainingQuantity * cargoQuantityFactor;
				}
				entryInstruction.CEI_CargoQuantityUnit = cargoQuantityUnit.Left(AutoJPCusEntryInstruction.Schema.CEI_CargoQuantityUnitMaxLength);
			}
		}
	}
}
