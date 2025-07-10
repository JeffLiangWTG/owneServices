using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS332DeclarationTypeCustomsOfficesProvider : ITS332DeclarationTypeCustomsOffices
	{
		public static TS332DeclarationTypeCustomsOfficesProvider New(TemporaryStorageHeader header)
			=> (header == null) ? null : new TS332DeclarationTypeCustomsOfficesProvider(header);

		public string FirstEntryCustomsOffice => header.CustomsOfficeOfFirstEntry;

		TS332DeclarationTypeCustomsOfficesProvider(TemporaryStorageHeader header)
		{
			this.header = header;
		}

		readonly TemporaryStorageHeader header;
	}
}
