using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CertificateLineWrapper : ICertificateLine
	{
		public CertificateLineWrapper(int lineNum, JobComInvoiceLine invoiceLine)
		{
			quarantineExDocLine = Argument.NotNull(invoiceLine.QuarantineExDocLine, "invoiceLine.QuarantineExDocLine");
			LineNumber = lineNum;
		}

		public ZInt LineNumber { get; private set; }

		public ZDecimal NetQuantity => quarantineExDocLine.QL_NetQuantity;

		public ZString NetQuantityUnit => quarantineExDocLine.QL_NetQuantityUnit;

		public ZString ProductCode => quarantineExDocLine.ProductCode;

		public ZString ProductDescription => quarantineExDocLine.QL_SendHCDesc ? quarantineExDocLine.InvoiceLine.JI_Description : ZString.Empty;

		public ZString AdditionalProductDescription => quarantineExDocLine.QL_AddtionalProductDescription;

		public ZString[] ExtraCertificates => !quarantineExDocLine.QL_ExtraCertificate.IsEmpty ? new[] { quarantineExDocLine.QL_ExtraCertificate } : System.Array.Empty<ZString>();

		public ZDecimal PackQuantity => new ZDecimal(quarantineExDocLine.QL_OuterPackCount);

		public ZString PackType => quarantineExDocLine.QL_OuterPackType;

		public IEnumerable<IRFPNumber> RFPNumbers
		{
			get
			{
				quarantineExDocLine.InvoiceLine.RFPNumbers.Sort(RFPNumber.Schema.ZA_RFPNumber, ListSortDirection.Ascending);
				var lastRFPNumber = ZString.Empty;

				foreach (RFPNumber number in quarantineExDocLine.InvoiceLine.RFPNumbers)
				{
					if (number.ZA_RFPNumber != lastRFPNumber)
					{
						lastRFPNumber = number.ZA_RFPNumber;
						yield return new RFPNumberWrapper(number, quarantineExDocLine.InvoiceLine.RFPNumbers, GetContainers());
					}
				}
			}
		}

		IEnumerable<IRFPContainer> GetContainers()
		{
			foreach (CusContainerInvoiceLinePivot pivot in quarantineExDocLine.InvoiceLine.ContainersPivot)
			{
				var container = pivot.Container;
				if (container != null)
				{
					yield return new RFPContainer
					{
						ContainerNumber = container.CO_ContainerNumber,
						Seal = container.CO_Seal
					};
				}
			}
		}

		readonly QuarantineExDocLine quarantineExDocLine;

		class RFPContainer : IRFPContainer
		{
			public ZString ContainerNumber { get; set; }
			public ZString Seal { get; set; }
		}
	}
}
