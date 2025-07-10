using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class Declaration09Provider : IDeclaration09
	{
		public Declaration09Provider(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		protected readonly TemporaryStorageHeader header;

		public static Declaration09Provider New(TemporaryStorageHeader header) => header == null ? null : new Declaration09Provider(header);

		public DateTime DeclarationDateValue => header.DeclarationDate.ToDateTime();

		public IReadOnlyCollection<DateTime> PreviousDocuments => previousDocuments ?? (previousDocuments = new List<DateTime>() { header.AMA_DateAtCustomsOffice.IsValid ? header.AMA_DateAtCustomsOffice.ToDateTime() : default, }.AsReadOnly());
		IReadOnlyCollection<DateTime> previousDocuments;

		public string LRN => header.LRN;

		public string MRN => header.MRN;
	}
}
