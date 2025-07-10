using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.IE.NCTS.Business
{
	abstract class IE013AndIE015MessageProvider : NctsDepartureHeaderMessageProvider, IIE013AndIE015Header
	{
		public IE013AndIE015MessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ??= (NctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)NctsHeader.MovementHeader.CusAuthorizationUsages : NctsHeader.CusAuthorizationUsages)
			.Select(c => new AuthorisationProvider(c))
			.ToArray();
		IReadOnlyCollection<IAuthorisation> authorisations;

		public string DepartureOffice => MovementHeader.DepartureCustomsOffice?.OfficeCode;

		public string DestinationOffice => MovementHeader.DestinationCustomsOffice?.OfficeCode;

		public IReadOnlyCollection<ITransitOffice> TransitOffices => transitOffices ??= MovementHeader.TransitCustomsOfficeCodeList.Select(t => new TransitOfficeProvider(t)).ToArray();
		IReadOnlyCollection<ITransitOffice> transitOffices;

		public IReadOnlyCollection<string> ExitOffices => exitOffices ??= ShouldSendExitOffices ? MovementHeader.ExitForTransitCustomsOfficeCodeList.Select(o => o.OfficeCode.ToString()).ToArray() : Array.Empty<string>();
		IReadOnlyCollection<string> exitOffices;

		bool ShouldSendExitOffices
		{
			get
			{
				var result = false;
				var declartionType = MovementHeader.BM_InBondEntryType.ToUpperInvariant();
				if (declartionType != NctsPhase5DeclarationTypeList.Codes.TIR && declartionType != NctsPhase5DeclarationTypeList.Codes.T2)
				{
					var typeOfSecurity = MovementHeader.BM_TypeOfSecurity.ToUpperInvariant();
					result = typeOfSecurity != NctsTypeOfSecurityList.Codes.NON && typeOfSecurity != NctsTypeOfSecurityList.Codes.ENT;
				}
				return result;
			}
		}

		public IHolder HolderOfTheTransit => CachedValueHelper.GetValue(ref holderOfTheTransit, () => HolderOfTransitProcedureProvider.New(NctsHeader.Principal, MovementHeader.BM_InBondEntryType));
		CachedValue<IHolder> holderOfTheTransit;

		public IReadOnlyCollection<IIE013AndIE015Guarantee> Guarantees
		{
			get
			{
				if (guarantees is null)
				{
					guarantees = MovementHeader.Guarantees.Cast<NctsGuarantee>().Select(g => new IE013AndIE015GuaranteeProvider(g)).ToArray();
				}
				return guarantees;
			}
		}
		IReadOnlyCollection<IIE013AndIE015Guarantee> guarantees;

		public IIE013AndIE015Consignment Consignment => CachedValueHelper.GetValue(ref consignment, () => new IE013AndIE015Consignment(NctsHeader));
		CachedValue<IIE013AndIE015Consignment> consignment;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representative, () => CreateRepresentative());

		CachedValue<IRepresentative> representative;
		IRepresentative CreateRepresentative()
		{
			var representative = MovementHeader.Representative;
			return IE.Business.AES.RepresentativeProvider.New(representative, representative.OrganisationPK == NctsHeader.Principal?.OrganisationPK ? RepresentationTypeList.Codes._2Direct : RepresentationTypeList.Codes._3Indirect);
		}
	}
}
