using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationCommodityProvider : IDeclarationCommodity
	{
		public DeclarationCommodityProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public string ID => string.Empty;
		public string GoodsDescription => invoiceLine.FullGoodsDescription;
		public string ComplementaryDescription => invoiceLine.ComplementaryDescription;
		public decimal InvoiceAmount => invoiceLine.JI_Calc_FOB;
		public string TariffCode => invoiceLine.JI_Tariff;
		public int SequenceNumeric => invoiceLine.JI_LineNo;

		public IEnumerable<IDeclarationProductCharacteristic> ProductCharacteristics
		{
			get
			{
				if (productCharacteristic == null)
				{
					productCharacteristic = new List<IDeclarationProductCharacteristic>();
					foreach (var attribute in invoiceLine.Attributes.Cast<AttributeCusCodeData>().OrderBy(x => x.CY_Code))
					{
						productCharacteristic.Add(new DeclarationProductCharacteristicProvider(attribute));
					}
				}
				return productCharacteristic;
			}
		}

		List<IDeclarationProductCharacteristic> productCharacteristic;

		public IEnumerable<IDeclarationNFeInvoice> ReferenceInvoices
		{
			get
			{
				if (referenceInvoices == null)
				{
					referenceInvoices = new List<IDeclarationNFeInvoice>();
					foreach (var eletronicLogistic in invoiceLine.ElectronicLogisticInvoiceCollection.Cast<ElectronicLogisticInvoice>())
					{
						referenceInvoices.Add(new DeclarationEletronicLogisticInvoiceProvider(eletronicLogistic));
					}

					foreach (var complementaryLogistic in invoiceLine.ComplementaryLogisticInvoiceCollection.Cast<ComplementaryLogisticInvoice>())
					{
						referenceInvoices.Add(new DeclarationComplementaryLogisticInvoiceProvider(complementaryLogistic));
					}
				}
				return referenceInvoices;
			}
		}

		List<IDeclarationNFeInvoice> referenceInvoices;
	}
}
