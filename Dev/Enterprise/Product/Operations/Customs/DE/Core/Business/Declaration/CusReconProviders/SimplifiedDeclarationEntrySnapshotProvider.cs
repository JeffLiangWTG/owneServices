using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class SimplifiedDeclarationEntrySnapshotProvider : IMonthlyClosingEntrySnapshot
	{
		public SimplifiedDeclarationEntrySnapshotProvider(IImportDecHeader importDecHeader)
		{
			this.importDecHeader = Argument.NotNull(importDecHeader, nameof(importDecHeader));
		}
		readonly IImportDecHeader importDecHeader;

		public string ReferenceNumber => null;

		public IImportParty Consignee => null;

		public Guid ConsigneePK => Guid.Empty;

		public string DeliveryTermsCode => null;

		public string DeliveryTermsDescription => null;

		public string DeliveryTermsPlace => null;

		public string DeliveryTermsKey => null;

		public IMoney PaymentTransaction => null;

		public string ForeignTradeStatisticsEntryCustomsOffice => null;

		public ICustomsValue CustomsValue => null;

		public IReadOnlyCollection<IImportDocument> Documents => documents ?? (documents = importDecHeader.Documents);
		IReadOnlyCollection<IImportDocument> documents;
	}
}
