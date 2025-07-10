using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	sealed class MeasurementProvider : IMeasurement
	{
		public MeasurementProvider(CusEntryInstruction instruction, string type)
		{
			Argument.NotNull(instruction, nameof(instruction));
			this.parent = instruction;
			this.type = type;
		}

		public MeasurementProvider(JobComInvoiceLine invoiceLine, string type)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.parent = invoiceLine;
			this.type = type;
		}

		public MeasurementProvider(CusEntryLine entryLine, string type)
		{
			Argument.NotNull(entryLine, nameof(entryLine));
			this.parent = entryLine;
			this.type = type;
		}

		readonly BusinessObject parent;
		readonly string type;

		public decimal Quantity
		{
			get
			{
				return (parent, type) switch
				{
					(CusEntryInstruction instruction, nameof(CusEntryHeaderMessageProvider.Quantity)) => instruction.CEI_CargoQuantity,
					(CusEntryInstruction instruction, nameof(CusEntryHeaderMessageProvider.GrossWeight)) => instruction.CEI_CustomsWeight,
					(CusEntryInstruction instruction, nameof(CusEntryHeaderMessageProvider.Volume)) => instruction.CEI_Volume,
					(CusEntryLine entryLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity1)) => entryLine.CustomsQuantity1,
					(CusEntryLine entryLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity2)) => entryLine.CustomsQuantity2,
					(JobComInvoiceLine invoiceLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity1)) => invoiceLine.JI_CustomsQuantity,
					(JobComInvoiceLine invoiceLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity2)) => invoiceLine.JI_CustomsSecondQuantity,
					_ => decimal.Zero,
				};
			}
		}

		public string Unit
		{
			get
			{
				return (parent, type) switch
				{
					(CusEntryInstruction instruction, nameof(CusEntryHeaderMessageProvider.Quantity)) => instruction.CEI_CargoQuantityUnit,
					(CusEntryInstruction instruction, nameof(CusEntryHeaderMessageProvider.GrossWeight)) => instruction.CEI_CustomsWeightUnit,
					(CusEntryInstruction instruction, nameof(CusEntryHeaderMessageProvider.Volume)) => instruction.CEI_VolumeUnit,
					(CusEntryLine entryLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity1)) => entryLine.CustomsQuantityUnit1,
					(CusEntryLine entryLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity2)) => entryLine.CustomsQuantityUnit2,
					(JobComInvoiceLine invoiceLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity1)) => invoiceLine.JI_CustomsUnitQty,
					(JobComInvoiceLine invoiceLine, nameof(GoodsItemProvider) + "." + nameof(GoodsItemProvider.Quantity2)) => invoiceLine.JI_CustomsSecondUnitQty,
					_ => string.Empty
				};
			}
		}
	}
}
