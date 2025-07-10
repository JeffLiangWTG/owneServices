using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationWrapper : IDeclaration
	{
		DeclarationWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			this.jobDeclaration = entryHeader.Declaration;
			this.entryInstruction = entryHeader.EntryInstruction;
		}

		public static DeclarationWrapper NewOrNull(CusEntryHeader entryHeader) => entryHeader == null || entryHeader.Declaration == null || entryHeader.EntryInstruction == null ? null : new DeclarationWrapper(entryHeader);

		public string AcceptanceDateTime => string.Empty;

		public ICollection<IDeclarationAgent> Agent => agent ?? (agent = GetAgent());
		ICollection<IDeclarationAgent> agent;

		public ICollection<IDeclarationAgent> GetAgent()
		{
			var agentCollection = new List<IDeclarationAgent>();
			agentCollection.Add(DeclarationAgentWrapper.NewOrNull(jobDeclaration));
			return agentCollection;
		}

		public IIDType DeclarationOfficeID => IDTypeWrapper.NewOrNull(jobDeclaration.JE_CustomsOffice);

		public IDeclarationDmExtensions DmExtensions => DeclarationDmExtWrapper.NewOrNull(entryHeader);

		public ICollection<IDeclarationDutyTaxFee> DutyTaxFee => null;

		public ICollection<IDeclarationGoodsShipment> GoodsShipment => GetGoodsShipment();

		public IDeclarationGovernmentProcedure GovernmentProcedure => DeclarationGovernmentProcedureWrapper.NewOrNull(entryInstruction);

		public IIDType ID => IDTypeWrapper.NewOrNull(entryHeader.EntryNumber);

		public ICollection<IDeclarationImporter> Importer => importer ?? (importer = GetImporter());
		ICollection<IDeclarationImporter> importer;

		public ICollection<IDeclarationImporter> GetImporter()
		{
			var declarationImporterCollection = new List<IDeclarationImporter>();
			declarationImporterCollection.Add(DeclarationImporterWrapper.NewOrNull(jobDeclaration));
			return declarationImporterCollection;
		}

		public string IssueDateTime => string.Empty;

		public ICodeType TypeCode => CodeTypeWrapper.NewOrNull(MapMessageType(jobDeclaration.JE_MessageType));

		ICollection<IDeclarationGoodsShipment> GetGoodsShipment()
			=> entryHeader
				.InvoiceHeaders()
				.Cast<JobComInvoiceHeader>()
				.OrderBy(invoice => invoice.JZ_InvoiceDisplaySequence)
				.Select(invoice => DeclarationGoodsShipmentWrapper.NewOrNull(entryInstruction, invoice))
				.Cast<IDeclarationGoodsShipment>()
				.ToList();

		string MapMessageType(string messageType)
			=> messageType switch
			{
				Common.Shared.SharedJobMessageTypeList.Codes.Import => "1",
				_ => string.Empty
			};

		readonly JobDeclaration jobDeclaration;
		readonly CusEntryInstruction entryInstruction;
		readonly CusEntryHeader entryHeader;
	}
}
