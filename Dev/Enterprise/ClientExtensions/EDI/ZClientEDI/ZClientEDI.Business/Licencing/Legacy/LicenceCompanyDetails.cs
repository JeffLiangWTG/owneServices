using System;
using System.Data;
using System.Xml;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LicenceCompanyDetails :  LicenceSegment
	{
		public LicenceCompanyDetails(ICompany company)
		{
			this.Company = company;
		}

		public ICompany Company
		{
			get;
			private set;
		}

		#region Set Values

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames")]
		public void Set(Guid orgPKThatGeneratedThisLicence,
			string enterpriseCode,
			string physicalServerID,
			string code,
			string name,
			string address1, string address2, string city, string postCode, string state,
			Guid countryPK,
			string taxationRegNo,
			string businessRegNo,
			string localCurrencyCode,
			Guid localCurrencyPK,
			bool isReciprocal,
			bool isGSTRegistered,
			bool isGSTCashBasis,
			bool isWHTRegistered,
			bool isWHTCashBasis,
			string phone = "",
			string fax = "",
			string email = "",
			string webAddress = "")
		{
			this.orgPKThatGeneratedThisLicence = orgPKThatGeneratedThisLicence;
			this.enterpriseCode = enterpriseCode;
			this.physicalServerID = physicalServerID;
			this.code = code;
			this.name = name;
			this.address1 = address1;
			this.address2 = address2;
			this.city = city;
			this.postCode = postCode;
			this.state = state;
			this.countryPK = countryPK;
			this.taxationRegNo = taxationRegNo;
			this.businessRegNo = businessRegNo;
			if (!string.IsNullOrEmpty(localCurrencyCode))
			{
				this.localCurrencyCode = localCurrencyCode;
			}
			else
			{
				this.localCurrencyCode = GetRefCurrencyCode(localCurrencyPK);
			}
			this.isReciprocal = isReciprocal;
			this.isGSTRegistered = isGSTRegistered;
			this.isGSTCashBasis = isGSTCashBasis;
			this.isWHTRegistered = isWHTRegistered;
			this.isWHTCashBasis = isWHTCashBasis;
			this.phone = phone;
			this.fax = fax;
			this.email = email;
			this.webAddress = webAddress;
		}

		string GetRefCurrencyCode(Guid refCurrencyPk)
		{
			var query = "SELECT RX_Code FROM dbo.RefCurrency WHERE RX_PK = @refCurrencyPk";
			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@refCurrencyPk", SqlDbType.UniqueIdentifier, refCurrencyPk);
				return (string)command.ExecuteScalar();
			}
		}

		#endregion

		#region AddToLicenceNode

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void AddToLicenceNode(XmlNode licenceNode)
		{
			AddAttribute(licenceNode, "OrgPKThatGeneratedThisLicence", OrgPKThatGeneratedThisLicence.ToString());
			AddAttribute(licenceNode, "EnterpriseCode", EnterpriseCode);
			AddAttribute(licenceNode, "PhysicalServerID", PhysicalServerID);
			AddAttribute(licenceNode, "Code", Code);
			AddAttribute(licenceNode, "Name", Name);
			AddAttribute(licenceNode, "Address1", Address1);
			AddAttribute(licenceNode, "Address2", Address2);
			AddAttribute(licenceNode, "City", City);
			AddAttribute(licenceNode, "PostCode", PostCode);
			AddAttribute(licenceNode, "State", State);
			AddAttribute(licenceNode, "CountryPK", CountryPK.ToString());
			AddAttribute(licenceNode, "BusinessRegNo", BusinessRegNo);
			AddAttribute(licenceNode, "BusinessRegNo2", BusinessRegNo2);
			AddAttribute(licenceNode, "LocalCurrencyCode", LocalCurrencyCode);

			var refCurrencyPk = Guid.Empty.ToString();
			if (!string.IsNullOrEmpty(LocalCurrencyCode))
			{
				var query = "SELECT RX_PK FROM dbo.RefCurrency WHERE RX_Code = @LocalCurrencyCode";
				using (var command = Db.Connection.Command(query))
				{
					command.AddParameter("@LocalCurrencyCode", SqlDbType.VarChar, LocalCurrencyCode);
					refCurrencyPk = command.ExecuteScalar().ToString();
				}
			}
			AddAttribute(licenceNode, "LocalCurrencyPK", refCurrencyPk);

			AddAttribute(licenceNode, "IsReciprocal", IsReciprocal.ToString());
			AddAttribute(licenceNode, "IsGSTRegistered", IsGSTRegistered.ToString());
			AddAttribute(licenceNode, "IsGSTCashBasis", IsGSTCashBasis.ToString());
			AddAttribute(licenceNode, "IsWHTRegistered", IsWHTRegistered.ToString());
			AddAttribute(licenceNode, "IsWHTCashBasis", IsWHTCashBasis.ToString());

			AddAttribute(licenceNode, "Phone", Phone);
			AddAttribute(licenceNode, "Fax", Fax);
			AddAttribute(licenceNode, "Email", Email);
			AddAttribute(licenceNode, "WebAddress", WebAddress);
		}

		static void AddAttribute(XmlNode node, string name, string val)
		{
			var attribute = node.OwnerDocument.CreateAttribute(name);
			attribute.Value = val;
			node.Attributes.Append(attribute);
		}

		#endregion

		#region LoadFromLicenceNode

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void LoadFromLicenceNode(XmlNode licenceNode)
		{
			orgPKThatGeneratedThisLicence = GetGuidAttributeValueSafely(licenceNode, "OrgPKThatGeneratedThisLicence");

			physicalServerID = GetStringAttributeValueSafely(licenceNode, "PhysicalServerID");
			enterpriseCode = GetStringAttributeValueSafely(licenceNode, "EnterpriseCode");
			code = GetStringAttributeValueSafely(licenceNode, "Code");
			name = GetStringAttributeValueSafely(licenceNode, "Name");
			address1 = GetStringAttributeValueSafely(licenceNode, "Address1");
			address2 = GetStringAttributeValueSafely(licenceNode, "Address2");

			city = GetStringAttributeValueSafely(licenceNode, "City");
			postCode = GetStringAttributeValueSafely(licenceNode, "PostCode");
			state = GetStringAttributeValueSafely(licenceNode, "State");
			countryPK = new Guid(licenceNode.Attributes["CountryPK"].Value);

			taxationRegNo = GetStringAttributeValueSafely(licenceNode, "BusinessRegNo");
			businessRegNo = GetStringAttributeValueSafely(licenceNode, "BusinessRegNo2");
			localCurrencyCode = GetStringAttributeValueSafely(licenceNode, "LocalCurrencyCode");

			if (string.IsNullOrEmpty(localCurrencyCode))
			{
				var currencyPk = GetGuidAttributeValueSafely(licenceNode, "LocalCurrencyPK");
				if (currencyPk != Guid.Empty)
				{
					localCurrencyCode = GetRefCurrencyCode(currencyPk);
				}
			}

			isReciprocal = GetBoolAttributeValueSafely(licenceNode, "IsReciprocal");
			isGSTRegistered = GetBoolAttributeValueSafely(licenceNode, "IsGSTRegistered");
			isGSTCashBasis = GetBoolAttributeValueSafely(licenceNode, "IsGSTCashBasis");
			isWHTRegistered = GetBoolAttributeValueSafely(licenceNode, "IsWHTRegistered");
			isWHTCashBasis = GetBoolAttributeValueSafely(licenceNode, "IsWHTCashBasis");

			phone = GetStringAttributeValueSafely(licenceNode, "Phone");
			fax = GetStringAttributeValueSafely(licenceNode, "Fax");
			email = GetStringAttributeValueSafely(licenceNode, "Email");
			webAddress = GetStringAttributeValueSafely(licenceNode, "WebAddress");
		}

		#endregion

		#region IsCompanyDetailsCorrect

		public bool IsCompanyDetailsCorrect
		{
			get
			{
				bool incorrect = Code != Company.Code
					|| Name != Company.Name
					|| Address1 != Company.Address1
					|| Address2 != Company.Address2
					|| City != Company.City
					|| State != Company.State
					|| PostCode != Company.Postcode
					|| CountryPK != Company.Country.PK
					|| BusinessRegNo != Company.BusinessRegNo1
					|| BusinessRegNo2 != Company.BusinessRegNo2
					|| LocalCurrencyCode != Company.LocalCurrency.Code
					|| IsWHTRegistered != Company.IsWHTRegistered
					|| IsWHTCashBasis != Company.IsWHTCashBasis;
				return !incorrect;
			}
		}

		#endregion

		#region Properties

		public Guid OrgPKThatGeneratedThisLicence
		{
			get { return orgPKThatGeneratedThisLicence; }
			private set { orgPKThatGeneratedThisLicence = value; }
		}
		Guid orgPKThatGeneratedThisLicence = Guid.Empty;

		public string EnterpriseCode
		{
			get { return enterpriseCode; }
			internal set { enterpriseCode = value; }
		}
		string enterpriseCode = "";

		public string PhysicalServerID
		{
			get { return physicalServerID; }
			internal set { physicalServerID = value; }
		}
		string physicalServerID = "";

		public string Code
		{
			get { return code; }
			internal set { code = value; }
		}
		string code = "";

		public string Name
		{
			get { return name; }
		}
		string name = "";

		public string Address1
		{
			get { return address1; }
		}
		string address1 = "";

		public string Address2
		{
			get { return address2; }
		}
		string address2 = "";

		public string City
		{
			get { return city; }
		}
		string city = "";

		public string PostCode
		{
			get { return postCode; }
		}
		string postCode = "";

		public string State
		{
			get { return state; }
		}
		string state = "";

		public Guid CountryPK
		{
			get { return countryPK; }
		}
		Guid countryPK = Guid.Empty;

		public string BusinessRegNo
		{
			get { return taxationRegNo; }
		}
		string taxationRegNo = "";

		public string BusinessRegNo2
		{
			get { return businessRegNo; }
		}
		string businessRegNo = "";

		public string LocalCurrencyCode
		{
			get { return localCurrencyCode; }
		}
		string localCurrencyCode = "";

		public bool IsReciprocal
		{
			get { return isReciprocal; }
		}
		bool isReciprocal;

		public bool IsGSTRegistered
		{
			get { return isGSTRegistered; }
		}
		bool isGSTRegistered;

		public bool IsGSTCashBasis
		{
			get { return isGSTCashBasis; }
		}
		bool isGSTCashBasis;

		public bool IsWHTRegistered
		{
			get { return isWHTRegistered; }
		}
		bool isWHTRegistered;

		public bool IsWHTCashBasis
		{
			get { return isWHTCashBasis; }
		}
		bool isWHTCashBasis;

		public string Phone { get { return phone; } }
		string phone = "";

		public string Fax { get { return fax; } }
		string fax = "";

		public string Email { get { return email; } }
		string email = "";

		public string WebAddress { get { return webAddress; } }
		string webAddress = "";

		#endregion
	}
}

