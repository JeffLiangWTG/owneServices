
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public abstract class _OrganisationLine : RecordLine
	{
		public _OrganisationLine(string value)
			: base(value)
		{
		}

		protected abstract string UnformattedAccountNumber { get; }
		public string AccountNumber
		{
			get
			{
				string result = UnformattedAccountNumber;
				if (result.StartsWith("0000"))
				{
					result = result.Substring(4);
				}
				return result;
			}
		}

		public abstract string Name { get; }
		public abstract string ContactName { get; }
		public abstract string Street1 { get; }
		public abstract string Street2 { get; }
		public abstract string City { get; }
		public abstract string Country { get; }
		public abstract string State { get; }
		public abstract string PostCode { get; }
		protected abstract string UnformattedPhoneNumber { get; }
		protected abstract string UnformattedFaxNumber { get; }

		public string Phone
		{
			get { return GetFormattedPhoneNumber(UnformattedPhoneNumber); }
		}

		string GetFormattedPhoneNumber(ZString phoneNumber)
		{
			string result = phoneNumber;

			if (phoneNumber.StartsWith("001161"))
			{
				result = phoneNumber.Remove(0, 6);
			}
			else if (phoneNumber.StartsWith("00116"))
			{
				result = phoneNumber.Remove(0, 5);
			}
			else if (phoneNumber.StartsWith("0011"))
			{
				result = phoneNumber.Remove(0, 4);
			}
			else
			{
				ZString tempPhoneNumber = phoneNumber.Replace("0", "");
				if (tempPhoneNumber.IsEmpty)
				{
					result = "";
				}
			}

			return result;
		}

		public ZString Fax
		{
			get { return GetFormattedPhoneNumber(UnformattedFaxNumber); }
		}
	}
}
