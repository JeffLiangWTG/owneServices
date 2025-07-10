using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESJobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public abstract class ESDocSADHLine : DocSADHLine
	{
		protected ESDocSADHLine(ESCusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		public new ESCusEntryLine EntryLine => (ESCusEntryLine)base.EntryLine;

		protected new ESJobDeclaration Declaration => (ESJobDeclaration)base.Declaration;

		public override ZString Box31PackagesAndDescriptionOfGoods => EntryLine.Box31CompleteText.Length > EntryLine.Box31MaxLength
																	? SeparateBoxesHelper.GetTextTrimmedBox31ForNormalLine(EntryLine.Box31CompleteText, EntryLine.Box31MaxLength)
																	: EntryLine.Box31CompleteText;

		const string WeightDecimalFormatSpain = "N3";

		const int roundNoUCC6AndWeightIsLessThanOneOrUCC6AndWeightIsGreaterThanOne = 3;

		protected override ZString Box35GrossWeightInKGCore
		{
			get
			{
				var grossWeight = EntryLine.EffectiveGrossWeight.InKilogramsSafe;
				var effectiveGrossWeight = grossWeight > 1 ? ImportWhenGrossWeightIsGreaterThanOne(grossWeight) : grossWeight.Round(roundNoUCC6AndWeightIsLessThanOneOrUCC6AndWeightIsGreaterThanOne);
				return effectiveGrossWeight == 0 ? ZString.Empty : effectiveGrossWeight.ToStringTrimZeros(WeightDecimalFormatSpain);
			}
		}

		ZDecimal ImportWhenGrossWeightIsGreaterThanOne(ZDecimal grossWeight) => EntryLine.Header.IsExportUCC6 || EntryLine.Header.IsH2Style ? grossWeight.Round(roundNoUCC6AndWeightIsLessThanOneOrUCC6AndWeightIsGreaterThanOne) : (ZDecimal)Math.Ceiling(grossWeight);

		public override ZString Box37Procedure => EntryLine.Box37ProcedureCompleteText;

		protected override ZString Box37_2ProcedureCore => EntryLine.Box37_2ProcedureCompleteText;

		protected override ZString Box38NetWeightInKGCore => EntryLine.EffectiveNetWeight.InKilogramsSafe.ToStringTrimZeros(WeightDecimalFormatSpain);

		protected override ZString Box41SupplementaryUnitsCore => EntryLine.SupplementaryQuantity == 0 ? string.Empty : Box41Format(EntryLine.SupplementaryQuantity) + " " + EntryLine.SupplementaryUQ.ConvertCargoWiseToES(EntryLine.Factory);

		protected override ZBool ShowBox41SupplementaryUnitsCore => true;
		protected override ZString Box41SupplementaryUQDescriptionCore => ZString.Empty;

		protected override string Box41Format(ZDecimal supplementaryQuantity) => supplementaryQuantity.ToStringTrimZeros(SupplementaryUnitsDecimalFormatSpain);
		protected abstract ZString SupplementaryUnitsDecimalFormatSpain { get; }

		public override ZString Box44AddInfoAndDocuments => EntryLine.Box44CompleteText.Length > Box44MaxLength
															? SeparateBoxesHelper.GetTextTrimmedBox44ForNormalLine(EntryLine.Box44CompleteText, Box44MaxLength)
															: EntryLine.Box44CompleteText;

		protected abstract int Box44MaxLength { get; }

		protected override DocSADHLineTaxCollection GetBox47TaxesCore() => new DocSADHLineTaxCollection(new List<IDocSADHLineTaxBoxSupporter>(), Factory);

		protected override ZString Box49WarehouseCore
		{
			get
			{
				var box49WarehouseCore = ZString.Empty;

				if (EntryLine.Header.EntryInstruction is CusEntryInstruction entryInstruction)
				{
					var toWarehouseCode = entryInstruction.ToWarehouseCode;
					var fromWarehouseCode = entryInstruction.FromWarehouseCode;

					if (!fromWarehouseCode.IsEmpty && toWarehouseCode.IsEmpty)
					{
						box49WarehouseCore = fromWarehouseCode;
					}
					else if (fromWarehouseCode.IsEmpty && !toWarehouseCode.IsEmpty)
					{
						box49WarehouseCore = toWarehouseCode;
					}
					else if (!fromWarehouseCode.IsEmpty && !toWarehouseCode.IsEmpty)
					{
						box49WarehouseCore = toWarehouseCode;
					}
				}

				return box49WarehouseCore;
			}
		}
	}
}
