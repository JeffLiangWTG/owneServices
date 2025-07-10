using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class ContactAddressSource : DocumentWrapper, IContactDetails
	{
		ContactAddressSource(JobDocAddress jobDocAddress, BusinessObjectFactory factoryToWrap)
			: base(jobDocAddress, factoryToWrap)
		{
		}

		public static ContactAddressSource New(JobDocAddress jobDocAddress, BusinessObjectFactory factoryToWrap)
		{
			return (jobDocAddress != null) ? new ContactAddressSource(jobDocAddress, factoryToWrap) : null;
		}

		JobDocAddress JobDocAddress
		{
			get { return (JobDocAddress)WrappedObject; }
		}

		#region Custom Fields

		public override string ToString()
		{
			return ContactName;
		}

		public ZString PostalAddress
		{
			get { return GetPostalAddress(); }
		}

		public ZString PostalAddressInEnglish
		{
			get { return GetPostalAddress(true); }
		}

		ZString GetPostalAddress(bool inEnglish = false)
		{
			var result = ZString.Empty;

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var docCompany = DocCompany.New(currentCompany, Factory);

			if (docCompany != null)
			{
				var addressFormatter = new WrapperPostalAddressFormatter(DocDocAddress.New(JobDocAddress, Factory), docCompany);
				if (addressFormatter != null)
				{
					addressFormatter.EnglishOnly = inEnglish;

					if (ContactName.IsEmpty)
					{
						result = addressFormatter.PostalAddress();
					}
					else
					{
						result = ContactName + "\n" + addressFormatter.PostalAddress();
					}
				}
			}

			return result;
		}

		public ZString Name
		{
			get
			{
				ZString result = JobDocAddress.E2_Contact + "\n" + JobDocAddress.E2_CompanyName;
				return result.Trim();
			}
		}

		#endregion

		public ZString Code
		{
			get { return ""; }
		}

		public ZString AttachmentType
		{
			get { return ""; }
		}

		public ZDateTime Birthday
		{
			get { return ZDateTime.Empty; }
		}

		public ZString ContactName
		{
			get { return JobDocAddress.E2_Contact; }
		}

		public ZString Email
		{
			get { return JobDocAddress.E2_Email; }
		}

		public ZString Fax
		{
			get { return JobDocAddress.E2_Fax_Formatted; }
		}

		public ZString HomePhone
		{
			get { return ""; }
		}

		public ZString Language
		{
			get { return ""; }
		}

		public ZString Mobile
		{
			get { return ""; }
		}

		public ZString NotifyMode
		{
			get { return ""; }
		}

		public DocAddress OrgAddress
		{
			get { return null; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(JobDocAddress, Factory); }
		}

		public DocOrganisation AddressOverride
		{
			get { return DocOrganisation.New(JobDocAddress, Factory); }
		}

		public ZString OtherPhone
		{
			get { return ""; }
		}

		public ZString Pager
		{
			get { return ""; }
		}

		public ZString Password
		{
			get { return ""; }
		}

		public ZString PersonalInfo
		{
			get { return ""; }
		}

		public ZString Phone
		{
			get { return JobDocAddress.E2_Phone_Formatted; }
		}

		public ZString Title
		{
			get { return ""; }
		}
	}
}
