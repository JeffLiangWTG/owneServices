using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC044ADeclarationWrapperExpected : CC044ADeclarationWrapper, ICC044ADeclaration
	{
		public CC044ADeclarationWrapperExpected(Business.NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			amh = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}

		ZString EU.NCTS.Messaging.ICC044ADeclaration.IdentityOfMeansOfTransportAtDeparture => amh.BM_TransportAtDeparture;

		ZString EU.NCTS.Messaging.ICC044ADeclaration.NationalityOfMeansOfTransportAtDeparture => amh.BM_RN_NKTransportAtDepartureCountry;

		ZInt EU.NCTS.Messaging.ICC044ADeclaration.TotalNumberOfItems => amh.TotalNumberOfItems;

		ZLong EU.NCTS.Messaging.ICC044ADeclaration.TotalNumberOfPackages => amh.TotalNumberOfPackages;

		ZDecimal EU.NCTS.Messaging.ICC044ADeclaration.TotalGrossMass => amh.TotalGrossMassInKilograms;

		public int NumberOfSeals => throw new NotSupportedException("Number of seals should not be accessed via the Expected wrapper, access it only via the Actual wrapper");

		public IReadOnlyCollection<IUnloadedGoodsItem> UnloadedGoodsItems => Array.Empty<IUnloadedGoodsItem>();

		public IReadOnlyCollection<ISealID> Seals => Array.Empty<ISealID>();

		readonly NctsArrivalMovementHeader amh;
	}
}
