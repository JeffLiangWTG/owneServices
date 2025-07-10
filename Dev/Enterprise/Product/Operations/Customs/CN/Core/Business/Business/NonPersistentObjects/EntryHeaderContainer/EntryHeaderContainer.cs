using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class EntryHeaderContainer : NonPersistentBusinessObject, ICusContainer
	{
		public EntryHeaderContainer(CusEntryHeader entryHeader, CusContainer cusContainer) : base(cusContainer.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.cusContainer = Argument.NotNull(cusContainer, nameof(cusContainer));
		}
		readonly CusEntryHeader entryHeader;
		readonly CusContainer cusContainer;

		public ZString ContainerNumber => cusContainer.CO_ContainerNumber;

		public ZString ContainerCode => cusContainer.ContainerCode;

		public ZString ContainerCodeDescription => cusContainer.ContainerCodeDescription;

		public ZBool IsLessContainer => cusContainer.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.LCL;

		public ZString IsLessContainerDesc => ConfirmationTypeList.GetDescriptionFromValue(IsLessContainer);

		public ZDecimal TareWeightInKG
		{
			get
			{
				if (!fTareWeightInKG.HasValue)
				{
					var tareWeightToCalculate = cusContainer.JobContainer.JC_Calc_TareWeight;
					if (tareWeightToCalculate.IsEmpty)
					{
						tareWeightToCalculate = cusContainer.JobContainer.JC_TareWeight;
					}
					fTareWeightInKG = Core.Constants.Weight.Convert(tareWeightToCalculate, cusContainer.JobContainer.JC_GrossWeightUQ, Core.Constants.Weight.Kilograms, false);
				}
				return fTareWeightInKG.Value;
			}
		}
		ZDecimal? fTareWeightInKG;

		public IEnumerable<ZShort> LinkedEntryLineNos
		{
			get
			{
				if (fLinkedEntryLineNos == null)
				{
					fLinkedEntryLineNos = new List<ZShort>();
					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
						{
							var linkedToThisContainer = invoiceLine.ContainersPivot.IsNullOrEmpty() || invoiceLine.ContainersPivot.Contains(cusContainer);
							if (linkedToThisContainer)
							{
								fLinkedEntryLineNos.Add(entryLine.EntryLineNo);
								break;
							}
						}
					}
				}
				return fLinkedEntryLineNos;
			}
		}
		List<ZShort> fLinkedEntryLineNos;

		public ZString LinkedEntryLineNosAsString => LinkedEntryLineNos.JoinAsString();
	}
}
