using System;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class AuthorizationDetails : IDataObject, IDisposable
	{
		[Mandatory]
		public CodeDescriptionPair4Char Purpose { get; set; }

		public Country Country { get; set; }

		[MaxLength(10)]
		public ZString? Version { get; set; }

		public ZDateTimeOffset? Date { get; set; }

		[MaxLength(150)]
		public ZString? GovernmentNumber { get; set; }

		[MaxLength(80)]
		public ZString? GovernmentBatchReference { get; set; }

		[MaxLength(50)]
		public ZString? GovernmentCounter { get; set; }

		[MaxLength(2048)]
		public ZString? URL { get; set; }

		public SubStreamableStream PublicKey { get; set; }

		public SubStreamableStream SharedSpecialData { get; set; }

		public SubStreamableStream SharedTransactionHash { get; set; }

		[MaxLength(50)]
		public ZString? DebtorRegistrationNumber { get; set; }

		[MaxLength(50)]
		public ZString? IssuerCertificateID { get; set; }

		[MaxLength(10)]
		public ZString? PlaceOfIssue { get; set; }

		public SubStreamableStream IssuerSpecialData { get; set; }

		public void Dispose()
		{
			PublicKey?.Dispose();
			SharedSpecialData?.Dispose();
			SharedTransactionHash?.Dispose();
			IssuerSpecialData?.Dispose();
		}
	}
}
