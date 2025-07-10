using System;
using Enterprise.FaxRouter.MailSecurity;

namespace Enterprise.FaxRouter
{
	class FaxCommand
	{
		readonly ICryptographicProvider cryptographicProvider;
		public FaxCommand(string commandString)
		{
			this.commandString = commandString;
			cryptographicProvider = new CryptProvider();
		}

		readonly string commandString;

		public string FaxNumber
		{
			get { return GetField("FAXNUMBER"); }
		}

		public string SysFaxJobID
		{
			get { return GetField("SYSFAXJOBID"); }
		}

		public string SysID
		{
			get { return GetField("SYSID"); }
		}

		public string SentDateTimeAsString
		{
			get { return GetField("SENTDATETIME"); }
		}

		public DateTime SentDateTime => DateTime.ParseExact(SentDateTimeAsString, cryptographicProvider.GetDateTimeFormat(), null);

		public string FaxKey
		{
			get { return GetField("FAXKEY"); }
		}

		public string FaxAttention
		{
			get { return GetField("FAXATTENTION"); }
		}

		public string FaxAttentionCompany
		{
			get { return GetField("FAXATTENTIONCOMPANY"); }
		}

		public string EnterpriseCode
		{
			get { return GetField("ENTERPRISECODE"); }
		}

		public string CompanyCode
		{
			get { return GetField("COMPANYCODE"); }
		}

		public string PhysicalServerID
		{
			get { return GetField("PHYSICALSERVERID"); }
		}

		string GetField(string fieldName)
		{
			string fieldIncludingSeparator = fieldName + "=";
			int nameIndex = commandString.IndexOf(fieldIncludingSeparator);
			if (nameIndex >= 0)
			{
				int fieldStart = nameIndex + fieldIncludingSeparator.Length;
				int fieldEnd = commandString.IndexOfAny(new char[] { '\r', '\n' }, fieldStart);

				if (fieldEnd < 0)
				{
					fieldEnd = commandString.Length;
				}

				return commandString.Substring(fieldStart, fieldEnd - fieldStart).Trim();
			}
			else
			{
				return "";
			}
		}
	}
}
