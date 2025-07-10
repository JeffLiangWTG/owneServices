using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class Declaration14Provider : MessageProvider, IDeclaration14
	{
		public Declaration14Provider(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		protected readonly TemporaryStorageHeader header;

		public static Declaration14Provider New(TemporaryStorageHeader header) => header == null ? null : new Declaration14Provider(header);

		public string MRN => header.MRN;

		public string InvalidationReason => null;

		public DateTime InvalidationRequestDateAndTime => PreparationDateAndTime;

		public IFallbackProcedure FallbackProcedure => null;
	}
}
