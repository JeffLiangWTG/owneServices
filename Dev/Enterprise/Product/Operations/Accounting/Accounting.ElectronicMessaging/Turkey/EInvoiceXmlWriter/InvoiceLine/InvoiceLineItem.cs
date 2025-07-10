using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class InvoiceLineItem
	{
		internal ItemType BuildInvoiceLineItem(PostingJournal line)
		{
			var item = new ItemType()
			{
				ID = new IDType() { Value = line.ChargeCode?.Code.ToString() ?? line.GLAccount.AccountCode.ToString() },
				Description = new DescriptionType() { Value = line.ChargeCode?.Description.ToString() ?? line.GLAccount.Description.ToString() }, //As legacy system
				Name = new NameType1() { Value = line.Description }, //As legacy system
				BrandName = new BrandNameType() { Value = line.Description }, //As legacy system
				ModelName = new ModelNameType() { Value = line.Description }, //As legacy system
				SellersItemIdentification = new ItemIdentificationType()
				{
					ID = new IDType() { Value = line.ChargeCode?.Code.ToString() ?? line.GLAccount.AccountCode.ToString() } //As legacy system
				}
			};
			return item;
		}
	}
}
