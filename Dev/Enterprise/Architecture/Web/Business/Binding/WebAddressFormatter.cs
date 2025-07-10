using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebAddressFormatter
	{
		#region Constructors

		public WebAddressFormatter(OrgAddress address)
		{
			this.address = address;
		}

		public WebAddressFormatter(JobDocAddress docAddress)
		{
			this.docAddress = docAddress;
		}

		#endregion

		#region Methods

		public string FormattedAddress()
		{
			formatter = null;
			if (Formatter != null)
			{
				return Formatter.PostalAddressAsASingleLineWithoutCompanyName();
			}
			return string.Empty;
		}

		public string FormattedAddressWithCompanyName()
		{
			string companyName = string.Empty;
			if (Address != null)
			{
				companyName = Address.EffectiveCompanyNameTruncated;
			}
			return FormattedAddressWithCompanyName(companyName);
		}

		public string FormattedAddressWithCompanyName(string companyName)
		{
			formatter = null;
			if (Formatter != null)
			{
				string fullAddress = Formatter.PostalAddressAsASingleLine();
				if (!string.IsNullOrEmpty(companyName.Trim()))
				{
					if (fullAddress.ToUpper().StartsWith(companyName.ToUpper()))
					{
						fullAddress = fullAddress.Substring(companyName.Length).Trim();
					}
					return ArrayToTextConverter.ConvertToCommaSeparatedMultilineText(companyName, fullAddress);
				}
				return fullAddress;
			}
			return string.Empty;
		}

		#endregion

		#region Implementation

		protected AddressFormatter Formatter
		{
			get
			{
				if (formatter == null)
				{
					if (Address != null)
					{
						formatter = new AddressFormatter(Address.Factory, Address, GlbCompany.CurrentCompany, false);
					}
					else if (DocAddress != null)
					{
						formatter = new AddressFormatter(DocAddress.Factory, DocAddress, GlbCompany.CurrentCompany, false);
					}
				}
				return formatter;
			}
		}

		AddressFormatter formatter;

		protected OrgAddress Address { get { return address; } }
		protected JobDocAddress DocAddress { get { return docAddress; } }

		readonly OrgAddress address;
		readonly JobDocAddress docAddress;

		#endregion
	}
}
