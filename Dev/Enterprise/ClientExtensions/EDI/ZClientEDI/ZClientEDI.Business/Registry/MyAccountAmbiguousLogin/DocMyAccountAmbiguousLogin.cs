using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class DocMyAccountAmbiguousLogin : DocBaseWrapper
	{
		public DocMyAccountAmbiguousLogin(MyAccountAmbiguousLogin objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
			Parent = objectToWrap;
		}

		public static DocMyAccountAmbiguousLogin New(MyAccountAmbiguousLogin objectToWrap, BusinessObjectFactory factory) => new DocMyAccountAmbiguousLogin(objectToWrap, factory);

		MyAccountAmbiguousLogin Parent { get; }

		[DocumentField("Company List")]
		public string CompanyList
		{
			get
			{
				var companyList = Parent.Contacts.Select(x => FormattableString.Invariant($"{x.OrganisationCode} - {x.WorkingAddressCompanyName}"));
				return string.Join("<br>", companyList);
			}
		}
	}
}
