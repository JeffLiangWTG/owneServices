using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC013CCC015CDeclarationDataHeaderProvider : NctsDepartureHeaderProvider
	{
		readonly NctsDepartureMovementHeader depHeader;

		public CC013CCC015CDeclarationDataHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
			depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(depHeader));
		}

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = new TransitOperationProvider(nctsHeader));
		ITransitOperation transitOperation;

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ??= (nctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)nctsHeader.MovementHeader.CusAuthorizationUsages : nctsHeader.CusAuthorizationUsages)
			.OrderBy(cau => cau.AGC_Code)
			.ThenBy(cau => cau.AGC_Number)
			.Select((cau, index) => new AuthorisationProvider(cau, index + 1))
			.ToArray();
		IReadOnlyCollection<IAuthorisation> authorisations;

		public virtual IReadOnlyCollection<ICustomsOfficeOfTransit> CustomsOfficesOfTransit
		{
			get
			{
				var transitCustomsOfficeCodeList = nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.TransitCustomsOfficeCodeList : nctsHeader.TransitCustomsOfficeCodeList;
				return customsOfficesOfTransit ?? (customsOfficesOfTransit = transitCustomsOfficeCodeList.Select((co, index) => new CustomsOfficesOfTransitProvider(co.OfficeCode, co.ArrivalTime, index + 1)).ToArray<ICustomsOfficeOfTransit>());
			}
		}
		IReadOnlyCollection<ICustomsOfficeOfTransit> customsOfficesOfTransit;

		public IReadOnlyCollection<ICustomsOfficeOfExitForTransit> CustomsOfficesOfExitForTransit
		{
			get
			{
				var exitForTransitCustomsOfficeCodeList = nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.ExitForTransitCustomsOfficeCodeList : nctsHeader.ExitForTransitCustomsOfficeCodeList;
				return customsOfficesOfExitForTransit ?? (customsOfficesOfExitForTransit = exitForTransitCustomsOfficeCodeList.Select((co, index) => new CustomsOfficesOfExitForTransitProvider(co.OfficeCode, index + 1)).ToArray<ICustomsOfficeOfExitForTransit>());
			}
		}
		IReadOnlyCollection<ICustomsOfficeOfExitForTransit> customsOfficesOfExitForTransit;

		public virtual IReadOnlyCollection<IGuarantee> Guarantees => guarantees ?? (guarantees = nctsHeader.GetEffectiveGuarantees().Cast<Guarantee>().Select((pw, index) => new GuaranteeProvider(pw, index + 1)).ToArray<IGuarantee>());
		IReadOnlyCollection<IGuarantee> guarantees;

		public virtual IConsignmentType20 Consignment => consignment ?? (consignment = new NCTSConsignmentProvider(nctsHeader));
		IConsignmentType20 consignment;

		public override string MessageType => ZString.Empty;
	}
}
