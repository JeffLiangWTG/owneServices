using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.NCTS.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is currently not used but I keep it just in case it's needed")]
	public class PersonProvider : IContactPerson
	{
		readonly NctsHeader header;

		public PersonProvider(NctsHeader nctsHeader)
		{
			header = nctsHeader;
		}

		public string Name => header?.ContactFullName;

		public string PhoneNumber => header?.ContactPhone;

		public string EMailAddress => header?.ContactEmail;
	}
}
