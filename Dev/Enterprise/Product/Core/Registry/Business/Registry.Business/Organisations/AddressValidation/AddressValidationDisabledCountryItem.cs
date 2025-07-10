using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AddressValidationDisabledCountryItem : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AddressValidationDisabledCountryItem();
		}

		#region Parent

		AddressValidationDisabledCountryItemCollection Parent => (AddressValidationDisabledCountryItemCollection)GetParentCollection(this, typeof(AddressValidationDisabledCountryItemCollection));

		#endregion

		#region CountryPK

		public ZGuid CountryPK
		{
			get => countryPK;
			set
			{
				SetNonPersistentPropertyValue(CountryPKInfo, ref countryPK, value);
				if (!IsValidationSuspended)
				{
					ValidateCountryPK();
				}
			}
		}

		public ZPropertyInfo CountryPKInfo => GetZPropertyInfo(Schema.CountryPK);

		ZGuid countryPK;

		#endregion

		#region CountryCollection

		public IBusinessObjectCollection CountryCollection => Parent?.CountryCollection;

		#endregion

		#region CountryName

		public ZString CountryName
		{
			get
			{
				var country = CurrentFactory.Load<IRefCountry>(countryPK);
				return country?.RN_Desc ?? ZString.Empty;
			}
		}

		#endregion

		#region DisabledForOverrideAddress

		public ZBool DisabledForOverrideAddress
		{
			get => disabledForOverrideAddress;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForOverrideAddressInfo, ref disabledForOverrideAddress, value);
		}

		public ZPropertyInfo DisabledForOverrideAddressInfo => GetZPropertyInfo(Schema.DisabledForOverrideAddress);

		ZBool disabledForOverrideAddress;

		#endregion

		#region DisabledForOrgAddress

		public ZBool DisabledForOrgAddress
		{
			get => disabledForOrgAddress;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForOrgAddressInfo, ref disabledForOrgAddress, value);
		}

		public ZPropertyInfo DisabledForOrgAddressInfo => GetZPropertyInfo(Schema.DisabledForOrgAddress);

		ZBool disabledForOrgAddress;

		#endregion

		#region DisabledForAdminPanel

		public ZBool DisabledForAdminPanel
		{
			get => disabledForAdminPanel;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForAdminPanelInfo, ref disabledForAdminPanel, value);
		}

		public ZPropertyInfo DisabledForAdminPanelInfo => GetZPropertyInfo(Schema.DisabledForAdminPanel);

		ZBool disabledForAdminPanel;

		#endregion

		#region DisabledForPerson

		public ZBool DisabledForPerson
		{
			get => disabledForPerson;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForPersonInfo, ref disabledForPerson, value);
		}

		public ZPropertyInfo DisabledForPersonInfo => GetZPropertyInfo(Schema.DisabledForPerson);

		ZBool disabledForPerson;

		#endregion

		#region DisabledForCompany

		public ZBool DisabledForCompany
		{
			get => disabledForCompany;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForCompanyInfo, ref disabledForCompany, value);
		}

		public ZPropertyInfo DisabledForCompanyInfo => GetZPropertyInfo(Schema.DisabledForCompany);

		ZBool disabledForCompany;

		#endregion

		#region DisabledForBranch

		public ZBool DisabledForBranch
		{
			get => disabledForBranch;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForBranchInfo, ref disabledForBranch, value);
		}

		public ZPropertyInfo DisabledForBranchInfo => GetZPropertyInfo(Schema.DisabledForBranch);

		ZBool disabledForBranch;

		#endregion

		#region DisabledForStaff

		public ZBool DisabledForStaff
		{
			get => disabledForStaff;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForStaffInfo, ref disabledForStaff, value);
		}

		public ZPropertyInfo DisabledForStaffInfo => GetZPropertyInfo(Schema.DisabledForStaff);

		ZBool disabledForStaff;

		#endregion

		#region DisabledForApplicant

		public ZBool DisabledForApplicant
		{
			get => disabledForApplicant;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForApplicantInfo, ref disabledForApplicant, value);
		}

		public ZPropertyInfo DisabledForApplicantInfo => GetZPropertyInfo(Schema.DisabledForApplicant);

		ZBool disabledForApplicant;

		#endregion

		#region DisabledForSalesInquiry

		public ZBool DisabledForSalesInquiry
		{
			get => disabledForSalesInquiry;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForSalesInquiryInfo, ref disabledForSalesInquiry, value);
		}

		public ZPropertyInfo DisabledForSalesInquiryInfo => GetZPropertyInfo(Schema.DisabledForSalesInquiry);

		ZBool disabledForSalesInquiry;

		#endregion

		#region DisabledForHVLVConsignment

		public ZBool DisabledForHVLVConsignment
		{
			get => disabledForHVLVConsignment;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForHVLVConsignmentInfo, ref disabledForHVLVConsignment, value);
		}

		public ZPropertyInfo DisabledForHVLVConsignmentInfo => GetZPropertyInfo(Schema.DisabledForHVLVConsignment);

		ZBool disabledForHVLVConsignment;

		#endregion

		#region DisabledForSupplierBookingLine

		public ZBool DisabledForSupplierBookingLine
		{
			get => disabledForSupplierBookingLine;
			set => SetNonPersistentPropertyValue<ZBool>(DisabledForSupplierBookingLineInfo, ref disabledForSupplierBookingLine, value);
		}

		public ZPropertyInfo DisabledForSupplierBookingLineInfo => GetZPropertyInfo(Schema.DisabledForSupplierBookingLine);

		ZBool disabledForSupplierBookingLine;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCountryPK();
		}

		void ValidateCountryPK()
		{
			CountryPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(CountryPKInfo);
			MandatoryValidation.CheckEntered(CountryPKInfo);

			if (!CountryPKInfo.HasMessageErrors() && !CountryPK.IsEmpty && Parent != null && Parent.Cast<AddressValidationDisabledCountryItem>().Any(u => u.PK != PK && u.CountryPK == CountryPK))
			{
				CountryPKInfo.AddError(Res.GetString("8C6D177A-5710-4742-93C1-5648967FE79E", "There are duplicate country/region configuration items, each country/region can't have more than one configuration item."));
			}
		}

		#endregion

		#region XML Serialization

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CountryPK = new ZGuid(reader.ReadElementString(Schema.CountryPK));
			DisabledForOverrideAddress = reader.ReadElementStringAsZBool(Schema.DisabledForOverrideAddress);
			DisabledForOrgAddress = reader.ReadElementStringAsZBool(Schema.DisabledForOrgAddress);
			DisabledForAdminPanel = reader.ReadElementStringAsZBool(Schema.DisabledForAdminPanel);
			DisabledForPerson = reader.ReadElementStringAsZBool(Schema.DisabledForPerson);
			DisabledForCompany = reader.ReadElementStringAsZBool(Schema.DisabledForCompany);
			DisabledForBranch = reader.ReadElementStringAsZBool(Schema.DisabledForBranch);
			DisabledForStaff = reader.ReadElementStringAsZBool(Schema.DisabledForStaff);
			DisabledForApplicant = reader.ReadElementStringAsZBool(Schema.DisabledForApplicant);
			DisabledForSalesInquiry = reader.ReadElementStringAsZBool(Schema.DisabledForSalesInquiry);
			DisabledForHVLVConsignment = reader.ReadElementStringAsZBool(Schema.DisabledForHVLVConsignment);
			DisabledForSupplierBookingLine = reader.ReadElementStringAsZBool(Schema.DisabledForSupplierBookingLine);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.CountryPK, CountryPK.ToString());
			writer.WriteElementString(Schema.DisabledForOverrideAddress, DisabledForOverrideAddress.ToString());
			writer.WriteElementString(Schema.DisabledForOrgAddress, DisabledForOrgAddress.ToString());
			writer.WriteElementString(Schema.DisabledForAdminPanel, DisabledForAdminPanel.ToString());
			writer.WriteElementString(Schema.DisabledForPerson, DisabledForPerson.ToString());
			writer.WriteElementString(Schema.DisabledForCompany, DisabledForCompany.ToString());
			writer.WriteElementString(Schema.DisabledForBranch, DisabledForBranch.ToString());
			writer.WriteElementString(Schema.DisabledForStaff, DisabledForStaff.ToString());
			writer.WriteElementString(Schema.DisabledForApplicant, DisabledForApplicant.ToString());
			writer.WriteElementString(Schema.DisabledForSalesInquiry, DisabledForSalesInquiry.ToString());
			writer.WriteElementString(Schema.DisabledForHVLVConsignment, DisabledForHVLVConsignment.ToString());
			writer.WriteElementString(Schema.DisabledForSupplierBookingLine, DisabledForSupplierBookingLine.ToString());

			base.WriteElements(writer);
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string CountryPK = "CountryPK";
			public const string DisabledForOverrideAddress = "DisabledForOverrideAddress";
			public const string DisabledForOrgAddress = "DisabledForOrgAddress";
			public const string DisabledForAdminPanel = "DisabledForAdminPanel";
			public const string DisabledForPerson = "DisabledForPerson";
			public const string DisabledForCompany = "DisabledForCompany";
			public const string DisabledForBranch = "DisabledForBranch";
			public const string DisabledForStaff = "DisabledForStaff";
			public const string DisabledForApplicant = "DisabledForApplicant";
			public const string DisabledForSalesInquiry = "DisabledForSalesInquiry";
			public const string DisabledForHVLVConsignment = "DisabledForHVLVConsignment";
			public const string DisabledForSupplierBookingLine = "DisabledForSupplierBookingLine";
		}

		#endregion
	}
}
