using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SIHeaderCreator
	{
		public Import5SIHeader Create(CusEntryHeader entry)
		{
			var import5SIHeaderData = new Import5SIHeader();
			import5SIHeaderData.ImportDeclarationNumber = entry.EntryNumber;
			import5SIHeaderData.DeclarationCustomsOffice = entry.Declaration.JE_CustomsOffice;
			import5SIHeaderData.DeclarationCustomsDivision = entry.Declaration.JE_CustomsDivision;

			var import5SIMailItemIDsList = new List<Import5SILine>();
			foreach (var invoiceHeader in entry.InvoiceHeaders())
			{
				foreach (var cusSupportingInfo in invoiceHeader.Parcels.Cast<CusSupportingInfo>())
				{
					var import5SIMailItemIDs = new Import5SILine();

					import5SIMailItemIDs.ParcelCustomsNumber = cusSupportingInfo.CSI_ReferenceNumber;
					import5SIMailItemIDs.ParcelNumber = cusSupportingInfo.CSI_ReferenceNumber2;
					import5SIMailItemIDs.DeliveryType = cusSupportingInfo.CSI_Code;

					import5SIMailItemIDsList.Add(import5SIMailItemIDs);
				}
			}
			import5SIHeaderData.MailItemIDs = import5SIMailItemIDsList.Count > 0 ? import5SIMailItemIDsList.ToArray() : null;
			return import5SIHeaderData;
		}
	}
}
