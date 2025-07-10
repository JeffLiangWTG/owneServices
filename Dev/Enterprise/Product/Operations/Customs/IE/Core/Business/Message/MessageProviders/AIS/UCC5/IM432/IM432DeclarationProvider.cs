using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM432DeclarationProvider : IIM432DeclarationType, IIM432PartiesType
	{
		public IM432DeclarationProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;

		public IIM432PartiesType Parties => this;

		public decimal GrossMass => entryHeader.TotalInvoiceLinesGrossWeightInKG;

		public string LRN => entryHeader.CH_BGMReference;

		public string CustomsOfficeLodgement => declaration.JE_CustomsOffice;

		public string PresentationCustomsOffice => declaration.PresentationCustomsOffice;

		public string GoodsPresentationPerson => declaration.DeclarantOrgAddress?.GetEORI();

		IRepresentative representativeCache;
		public IRepresentative Representative => representativeCache ??= RepresentativeProvider.New(declaration);

		IReadOnlyCollection<IAuthorisationHolder> authorisationHolderCache;
		public IReadOnlyCollection<IAuthorisationHolder> AuthorisationHolder =>
			authorisationHolderCache ?? (authorisationHolderCache = instruction.CusAuthorizationUsages.Select(AuthorisationHolderProvider.New).ToArray());
	}
}
