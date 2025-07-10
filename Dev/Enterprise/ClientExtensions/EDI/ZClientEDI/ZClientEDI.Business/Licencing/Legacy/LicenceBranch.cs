using System.Xml;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LicenceBranch
	{
		public LicenceBranch()
		{
		}

		#region Set Values

		public void Set(string code, string branchName, string address1, string address2, string city, string state, string postCode, string phone, string fax, string email, string webAddress, string homePortNK)
		{
			fCode = code;
			fBranchName = branchName;
			fAddress1 = address1;
			fAddress2 = address2;
			fCity = city;
			fState = state;
			fPostCode = postCode;
			fPhone = phone;
			fFax = fax;
			fEmail = email;
			fWebAddress = webAddress;
			fHomePortNK = homePortNK;
		}

		#endregion

		#region AddToLicenceNode

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void AddToLicenceNode(XmlNode licenceNode)
		{
			XmlDocument document = licenceNode.OwnerDocument;

			XmlAttribute codeAttribute = document.CreateAttribute("Code");
			codeAttribute.Value = Code;
			licenceNode.Attributes.Append(codeAttribute);

			XmlAttribute branchNameAttribute = document.CreateAttribute("BranchName");
			branchNameAttribute.Value = BranchName;
			licenceNode.Attributes.Append(branchNameAttribute);

			XmlAttribute address1Attribute = document.CreateAttribute("Address1");
			address1Attribute.Value = Address1;
			licenceNode.Attributes.Append(address1Attribute);

			XmlAttribute address2Attribute = document.CreateAttribute("Address2");
			address2Attribute.Value = Address2;
			licenceNode.Attributes.Append(address2Attribute);

			XmlAttribute cityAttribute = document.CreateAttribute("City");
			cityAttribute.Value = City;
			licenceNode.Attributes.Append(cityAttribute);

			XmlAttribute stateAttribute = document.CreateAttribute("State");
			stateAttribute.Value = State;
			licenceNode.Attributes.Append(stateAttribute);

			XmlAttribute postCodeAttribute = document.CreateAttribute("PostCode");
			postCodeAttribute.Value = PostCode;
			licenceNode.Attributes.Append(postCodeAttribute);

			XmlAttribute phoneAttribute = document.CreateAttribute("Phone");
			phoneAttribute.Value = Phone;
			licenceNode.Attributes.Append(phoneAttribute);

			XmlAttribute faxAttribute = document.CreateAttribute("Fax");
			faxAttribute.Value = Fax;
			licenceNode.Attributes.Append(faxAttribute);

			XmlAttribute emailAttribute = document.CreateAttribute("Email");
			emailAttribute.Value = Email;
			licenceNode.Attributes.Append(emailAttribute);

			XmlAttribute webAddressAttribute = document.CreateAttribute("WebAddress");
			webAddressAttribute.Value = WebAddress;
			licenceNode.Attributes.Append(webAddressAttribute);

			XmlAttribute homePortNKAttribute = document.CreateAttribute("HomePortNK");
			homePortNKAttribute.Value = HomePortNK;
			licenceNode.Attributes.Append(homePortNKAttribute);
		}

		#endregion

		#region LoadFromLicenceNode

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void LoadFromLicenceNode(XmlNode licenceNode)
		{
			fCode = licenceNode.Attributes["Code"].Value;
			fBranchName = licenceNode.Attributes["BranchName"].Value;
			fAddress1 = licenceNode.Attributes["Address1"].Value;
			fAddress2 = licenceNode.Attributes["Address2"].Value;
			fCity = licenceNode.Attributes["City"].Value;
			fState = licenceNode.Attributes["State"].Value;
			fPostCode = licenceNode.Attributes["PostCode"].Value;
			fPhone = licenceNode.Attributes["Phone"].Value;
			fFax = licenceNode.Attributes["Fax"].Value;
			fEmail = licenceNode.Attributes["Email"].Value;
			fWebAddress = licenceNode.Attributes["WebAddress"].Value;
			fHomePortNK = licenceNode.Attributes["HomePortNK"].Value;
		}

		#endregion

		#region Properties

		public string Code
		{
			get { return fCode; }
		}
		string fCode;

		public string BranchName
		{
			get { return fBranchName; }
		}
		string fBranchName;

		public string Address1
		{
			get { return fAddress1; }
		}
		string fAddress1;

		public string Address2
		{
			get { return fAddress2; }
		}
		string fAddress2;

		public string City
		{
			get { return fCity; }
		}
		string fCity;

		public string State
		{
			get { return fState; }
		}
		string fState;

		public string PostCode
		{
			get { return fPostCode; }
		}
		string fPostCode;

		public string Phone
		{
			get { return fPhone; }
		}
		string fPhone;

		public string Fax
		{
			get { return fFax; }
		}
		string fFax;

		public string Email
		{
			get { return fEmail; }
		}
		string fEmail;

		public string WebAddress
		{
			get { return fWebAddress; }
		}
		string fWebAddress;

		public string HomePortNK
		{
			get { return fHomePortNK; }
		}
		string fHomePortNK;

		#endregion
	}
}

	