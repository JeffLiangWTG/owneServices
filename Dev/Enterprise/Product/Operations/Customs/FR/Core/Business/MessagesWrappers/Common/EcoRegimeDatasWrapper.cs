using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class EcoRegimeDatasWrapper : IEcoRegimeDatas
	{
		public EcoRegimeDatasWrapper(CusEntryLine entryLine)
		{
			itemEntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		public IEnumerable<IEcoRegimeDeclDatas> DecEcos => decEcos ?? (decEcos = GetDecEcos().ToArray());
		IEcoRegimeDeclDatas[] decEcos;

		public ZDecimal GuaranteeAmount => itemEntryLine.SpecificRegimeGuaranteeAmount;

		public sbyte? NumberDaysOfDischarge
		{
			get
			{
				var capturedValue = itemEntryLine.SpecificRegimeNumberDaysOfDischarge;
				if (capturedValue > 99 || capturedValue < 2)
				{
					capturedValue = sbyte.MinValue;
				}
				return (sbyte?)capturedValue;
			}
		}

		protected IEnumerable<IEcoRegimeDeclDatas> GetDecEcos()
		{
			List<IEcoRegimeDeclDatas> decEcos = new List<IEcoRegimeDeclDatas>();

			foreach (PreviousDocument previousDoc in itemEntryLine.Declaration.PreviousDocuments)
			{
				if (previousDoc.CSI_Code == PreviousDocumentCodeList.Codes.CLE)
				{
					decEcos.Add(new EcoRegimeDeclDatasWrapper(previousDoc));
				}
			}

			foreach (JobComInvoiceLine invoiceLine in itemEntryLine.InvoiceLines)
			{
				if (invoiceLine.PreviousInbondMovement != null)
				{
					decEcos.Add(new EcoRegimeDeclDatasInbondMovementWrapper(invoiceLine.PreviousInbondMovement));
				}
			}

			return decEcos;
		}

		readonly CusEntryLine itemEntryLine;
	}
}
