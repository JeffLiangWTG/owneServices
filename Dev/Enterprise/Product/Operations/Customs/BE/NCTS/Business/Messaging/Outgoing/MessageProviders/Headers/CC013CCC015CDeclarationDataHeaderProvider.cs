using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.NCTS.Business.Messaging.MessageProviders.NCTS.Outgoing;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC013CCC015CDeclarationDataHeaderProvider : NctsDepartureHeaderProvider
	{
		readonly NctsDepartureMovementHeader depHeader;

		public CC013CCC015CDeclarationDataHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
			depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(depHeader));
		}

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = GetTransitOperation());
		ITransitOperation transitOperation;

		protected virtual ITransitOperation GetTransitOperation() => new TransitOperationProvider(nctsHeader);

		public IReadOnlyCollection<IAuthorization> Authorisations => authorisations ??= (nctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)nctsHeader.MovementHeader.CusAuthorizationUsages : nctsHeader.CusAuthorizationUsages)
			.OrderBy(cau => cau.AGC_Code)
			.ThenBy(cau => cau.AGC_Number)
			.Select((cau, index) => new AuthorizationProvider(cau, index + 1))
			.ToArray();
		IReadOnlyCollection<IAuthorization> authorisations;

		public virtual IReadOnlyCollection<ICustomsOfficeOfTransit> CustomsOfficesOfTransit => customsOfficesOfTransit ?? (customsOfficesOfTransit = depHeader.TransitCustomsOfficeCodeList.Select((co, index) => new CustomsOfficesOfTransitProvider(co.OfficeCode, co.ArrivalTime, index + 1)).ToArray<ICustomsOfficeOfTransit>());
		IReadOnlyCollection<ICustomsOfficeOfTransit> customsOfficesOfTransit;

		public IReadOnlyCollection<ICustomsOfficeOfExitForTransit> CustomsOfficesOfExitForTransit => customsOfficesOfExitForTransit ?? (customsOfficesOfExitForTransit = depHeader.ExitForTransitCustomsOfficeCodeList.Select((co, index) => new CustomsOfficesOfExitForTransitProvider(co.OfficeCode, index + 1)).ToArray<ICustomsOfficeOfExitForTransit>());
		IReadOnlyCollection<ICustomsOfficeOfExitForTransit> customsOfficesOfExitForTransit;

		public virtual IReadOnlyCollection<IGuarantee> Guarantees => guarantees ?? (guarantees = nctsHeader.MovementHeader.Guarantees.Cast<EU.NCTS.Business.NctsGuarantee>().Select((pw, index) => new GuaranteeProvider(pw, index + 1)).ToArray<IGuarantee>());
		IReadOnlyCollection<IGuarantee> guarantees;

		public virtual IConsignmentType20 Consignment => consignment ?? (consignment = new NCTSConsignmentProvider(nctsHeader));
		IConsignmentType20 consignment;

		public override string MessageType => ZString.Empty;
	}
}
