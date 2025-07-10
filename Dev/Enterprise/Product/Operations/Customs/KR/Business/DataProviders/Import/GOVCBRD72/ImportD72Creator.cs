using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportD72Creator
	{
		public ImportD72Header Create(CusEntryHeader entry, ExtendReExportDateMessageSendingObject messageSendingObject, int versionNumber)
		{
			var importD72Data = new ImportD72Header();
			importD72Data.ImportDeclarationNumber = entry.EntryNumber;
			importD72Data.SequenceNo = versionNumber;
			if (messageSendingObject.CurrentReExportScheduledDate.IsValid)
			{
				importD72Data.BeforeReExportScheduledDate = messageSendingObject.CurrentReExportScheduledDate.ToDateTime();
			}
			var declaration = entry.Declaration;
			importD72Data.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			importD72Data.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			importD72Data.DeclarantType = declaration.IsSelfDeclaringOwner ? Constants.DeclarantType.GOVCBRD72.Importer : Constants.DeclarantType.GOVCBRD72.Broker;
			if (messageSendingObject.NewReExportDate.IsValid)
			{
				importD72Data.AfterReExportScheduledDate = messageSendingObject.NewReExportDate.ToDateTime();
			}
			importD72Data.ReasonDescription = messageSendingObject.ReasonDescription;

			var entryLineDataList = new List<ImportD72Line>();
			var orderMessageLines = messageSendingObject.MessageSendingInvoiceLines.Cast<MessageSendingInvoiceLine>().OrderBy(x => x.EntryLineNo).ThenBy(x => x.InvoiceLineNo);

			foreach (MessageSendingInvoiceLine messageLine in orderMessageLines)
			{
				var entryLineData = new ImportD72Line();
				entryLineData.EntryLineNo = messageLine.EntryLineNo;
				entryLineData.AmountCurrency = messageLine.AmountCurrency;
				entryLineData.DetailLineNo = messageLine.InvoiceLineNo;
				entryLineData.HSDescription = messageLine.HSDescription;
				entryLineData.ItemDescription = messageLine.ItemDescription;
				entryLineData.QuantityUnit = messageLine.UQ;
				entryLineData.Remark = messageLine.Remark;
				entryLineData.Quantity = messageLine.Quantity;
				entryLineData.Amount = messageLine.LinePrice;
				entryLineDataList.Add(entryLineData);
			}
			importD72Data.Lines = entryLineDataList.ToArray();

			PopulateImportD72Declarant(declaration, importD72Data);
			return importD72Data;
		}
		void PopulateImportD72Declarant(JobDeclaration declaration, ImportD72Header importD72Data)
		{
			var orgAddress = importD72Data.DeclarantType == Constants.DeclarantType.GOVCBRD72.Broker ? declaration.BrokerAddress : declaration.ImporterAddress;
			if (orgAddress != null)
			{
				importD72Data.Declarant = new Organisation(RoleType.Declarant)
				{
					CompanyName = orgAddress.CompanyName,
					RepresentativeName = orgAddress.Header.GetRepresentativeName(),
					AddressLine1 = orgAddress.Address1,
					AddressLine2 = orgAddress.Address2,
					Postcode = orgAddress.Postcode,
					BuildingNumber = orgAddress.GetBuildingNumber(),
					RoadNameCode = orgAddress.GetRoadNameCode(),
					IsIndividual = orgAddress.Header.GetIsIndividual(),
				};

				var firstID = orgAddress.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted();
				if (firstID != null)
				{
					importD72Data.Declarant.SetRegistrationIDNumbers(new IDNumberAndType[] { firstID });
				}
			}
		}
	}
}
