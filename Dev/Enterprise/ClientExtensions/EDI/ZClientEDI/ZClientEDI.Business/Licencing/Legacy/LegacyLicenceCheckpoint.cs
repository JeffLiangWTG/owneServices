using System;
using System.Globalization;
using System.Xml;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LegacyLicenceCheckpoint
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public LegacyLicenceCheckpoint(string name,
			string displayName,
			LegacyLicence parentLicences,
			string parentName,
			bool isManuallyEnabled = false)
		{
			this.Name = name;
			this.DisplayName = displayName;
			SetParentLicences(parentLicences);
			this.ParentModule = parentName ?? string.Empty;
			this.IsManuallyEnabled = isManuallyEnabled;
		}

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string ParentModule { get; internal set; }
		public int UserLimit { get; set; }
		public DateTime ExpiryDate { get; set; }
		protected LegacyLicence parentLicences;
		public bool IsWithoutUserLimit { get; set; }
		public bool IsDefaultTransactional { get; set; }
		public bool IsManuallyEnabled { get; private set; }

		public string LicenceType
		{
			get
			{
				if (licenceType == null)
				{
					licenceType = GetDefaultLicenceValue();
				}
				return licenceType;
			}
			set { licenceType = value; }
		}
		string licenceType;

		protected virtual void SetParentLicences(LegacyLicence licence)
		{
			if (licence != null)
			{
				licence.Add(this);
				this.parentLicences = licence;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void AddToLicenceNode(XmlNode licenceKeyNode)
		{
			XmlDocument licenceKeyXml = licenceKeyNode.OwnerDocument;
			XmlNode node = licenceKeyXml.CreateNode(XmlNodeType.Element, Name, "");
			licenceKeyNode.AppendChild(node);

			XmlAttribute licenceTypeAttribute = licenceKeyXml.CreateAttribute("Type");
			licenceTypeAttribute.Value = LicenceType;
			node.Attributes.Append(licenceTypeAttribute);

			XmlAttribute userLimitAttribute = licenceKeyXml.CreateAttribute("User");
			userLimitAttribute.Value = UserLimit.ToString(CultureInfo.InvariantCulture);
			node.Attributes.Append(userLimitAttribute);

			XmlAttribute expiryDateAttribute = licenceKeyXml.CreateAttribute("Expiry");
			expiryDateAttribute.Value = ExpiryDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			node.Attributes.Append(expiryDateAttribute);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void LoadFromLicenceNode(XmlNode licenceNode)
		{
			this.LicenceType = licenceNode.Attributes["Type"].Value;
			this.UserLimit = int.Parse(licenceNode.Attributes["User"].Value, CultureInfo.InvariantCulture);

			string expiryDateString = licenceNode.Attributes["Expiry"].Value;
			this.ExpiryDate = DateTime.ParseExact(expiryDateString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
		}

		public virtual string GetDefaultLicenceValue()
		{
			if (IsManuallyEnabled)
			{
				return LicenceTypes.Codes.NON;
			}
			else if (IsDefaultTransactional)
			{
				return LicenceTypes.Codes.CPT;
			}
			else
			{
				return LicenceTypes.Codes.ODM;
			}
		}

		public virtual string GetDefaultEnabledLicenceValue()
		{
			if (IsDefaultTransactional)
			{
				return LicenceTypes.Codes.CPT;
			}

			return LicenceTypes.Codes.NON;
		}
	}
}

