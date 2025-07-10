using System.Data;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CLREGContactInfoProvider : Customs.Business.MultiLineAddInfos.CusAddInfo<CLREGContactInfoProviderAddInfo>
	{
		public CLREGContactInfoProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		#pragma warning disable IDE0001 // Prevent simplification of explicit generic type
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<CLREGContactInfoProviderAddInfo>.Schema
		#pragma warning restore IDE0001 // Prevent simplification of explicit generic type
		{
			public const string ZA_Cont1 = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_Cont1;
			public const string ZA_Cont2 = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_Cont2;
			public const string ZA_ContAH = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContAH;
			public const string ZA_ContAHComment = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContAHComment;
			public const string ZA_ContAHPref = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContAHPref;
			public const string ZA_ContCity = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContCity;
			public const string ZA_ContEmail = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContEmail;
			public const string ZA_ContFax = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContFax;
			public const string ZA_ContFaxComment = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContFaxComment;
			public const string ZA_ContFaxPref = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContFaxPref;
			public const string ZA_ContMob = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContMob;
			public const string ZA_ContMobComment = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContMobComment;
			public const string ZA_ContName = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContName;
			public const string ZA_ContPh = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPh;
			public const string ZA_ContPhComment = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPhComment;
			public const string ZA_ContPhPref = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPhPref;
			public const string ZA_ContPort = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPort;
			public const string ZA_ContPost1 = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPost1;
			public const string ZA_ContPost2 = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPost2;
			public const string ZA_ContPostCity = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPostCity;
			public const string ZA_ContPostCode = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPostCode;
			public const string ZA_ContPostPort = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPostPort;
			public const string ZA_ContPostPostCode = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPostPostCode;
			public const string ZA_ContPostState = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPostState;
			public const string ZA_ContPurpose = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContPurpose;
			public const string ZA_ContState = AUCLREGContactInfoProviderAddInfoSchema.Constants.ZA_ContState;
		}
		#endregion

		#region Related Objects

		public CLREGInfoProvider CLREGProvider
		{
			get { return clREGProvider ?? (clREGProvider = Factory.Load<CLREGInfoProvider>(B7_ParentID)); }
		}
		CLREGInfoProvider clREGProvider;

		#endregion

		#region AddInfo Properties

		public ZString ZA_ContName
		{
			get { return AddInfo.ZA_ContName; }
			set { AddInfo.ZA_ContName = value; }
		}

		public ZPropertyInfo ZA_ContNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContName, x => AddInfo.ZA_ContNameInfo); }
		}

		public ZString ZA_ContPurpose
		{
			get { return AddInfo.ZA_ContPurpose; }
			set { AddInfo.ZA_ContPurpose = value; }
		}

		public ZPropertyInfo ZA_ContPurposeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPurpose, x => AddInfo.ZA_ContPurposeInfo); }
		}

		#region Contact Address

		[List(nameof(CLREGProvider) + "." + nameof(CLREGInfoProvider.AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.Addresses))]

		public ZGuid ZA_OA_ContactAddress
		{
			get { return oa_ContactAddress; }
			set
			{
				oa_ContactAddress = value;

				var address = GetAddress(oa_ContactAddress);
				if (address != null)
				{
					ZA_Cont1 = address.OA_Address1.Left(ZA_Cont1Info.MaxLength);
					ZA_Cont2 = address.OA_Address2.Left(ZA_Cont2Info.MaxLength);
					ZA_ContCity = address.OA_City.Left(ZA_ContCityInfo.MaxLength);
					ZA_ContPostCode = address.OA_PostCode;
					ZA_ContPort = address.OA_RL_NKRelatedPortCode;
					ZA_ContState = address.OA_State.Left(ZA_ContStateInfo.MaxLength);
				}
			}
		}
		ZGuid oa_ContactAddress;

		public OrgAddress GetAddress(ZGuid addressPK)
		{
			return Factory.Load<OrgAddress>(addressPK);
		}

		public ZString ZA_Cont1
		{
			get { return AddInfo.ZA_Cont1; }
			set { AddInfo.ZA_Cont1 = value; }
		}

		public ZPropertyInfo ZA_Cont1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Cont1, x => AddInfo.ZA_Cont1Info); }
		}

		public ZString ZA_Cont2
		{
			get { return AddInfo.ZA_Cont2; }
			set { AddInfo.ZA_Cont2 = value; }
		}

		public ZPropertyInfo ZA_Cont2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Cont2, x => AddInfo.ZA_Cont2Info); }
		}

		public ZString ZA_ContCity
		{
			get { return AddInfo.ZA_ContCity; }
			set { AddInfo.ZA_ContCity = value; }
		}

		public ZPropertyInfo ZA_ContCityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContCity, x => AddInfo.ZA_ContCityInfo); }
		}

		public ZString ZA_ContPostCode
		{
			get { return AddInfo.ZA_ContPostCode; }
			set { AddInfo.ZA_ContPostCode = value; }
		}

		public ZPropertyInfo ZA_ContPostCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPostCode, x => AddInfo.ZA_ContPostCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGContactInfoProviderAddInfoLookups.RefUNLOCOs))]
		public ZString ZA_ContPort
		{
			get { return AddInfo.ZA_ContPort; }
			set
			{
				AddInfo.ZA_ContPort = value;
				AddInfoValidation.ValidateZA_ContPostCode();
			}
		}

		public ZPropertyInfo ZA_ContPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPort, x => AddInfo.ZA_ContPortInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGContactInfoProviderAddInfoLookups.OA_StateListForContactAddress))]
		public ZString ZA_ContState
		{
			get { return AddInfo.ZA_ContState; }
			set { AddInfo.ZA_ContState = value; }
		}

		public ZPropertyInfo ZA_ContStateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContState, x => AddInfo.ZA_ContStateInfo); }
		}

		#endregion

		#region Contact Postal Address

		[List(nameof(CLREGProvider) + "." + nameof(CLREGInfoProvider.AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.Addresses))]
		public ZGuid ZA_OA_ContactPostalAddress
		{
			get { return oa_ContactPostalAddress; }
			set
			{
				oa_ContactPostalAddress = value;

				var address = GetAddress(oa_ContactPostalAddress);
				if (address != null)
				{
					ZA_ContPost1 = address.OA_Address1.Left(ZA_ContPost1Info.MaxLength);
					ZA_ContPost2 = address.OA_Address2.Left(ZA_ContPost2Info.MaxLength);
					ZA_ContPostCity = address.OA_City.Left(ZA_ContPostCityInfo.MaxLength);
					ZA_ContPostPostCode = address.OA_PostCode;
					ZA_ContPostPort = address.OA_RL_NKRelatedPortCode;
					ZA_ContPostState = address.OA_State.Left(ZA_ContPostStateInfo.MaxLength);
				}
			}
		}
		ZGuid oa_ContactPostalAddress;

		public ZString ZA_ContPost1
		{
			get { return AddInfo.ZA_ContPost1; }
			set { AddInfo.ZA_ContPost1 = value; }
		}

		public ZPropertyInfo ZA_ContPost1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPost1, x => AddInfo.ZA_ContPost1Info); }
		}

		public ZString ZA_ContPost2
		{
			get { return AddInfo.ZA_ContPost2; }
			set { AddInfo.ZA_ContPost2 = value; }
		}

		public ZPropertyInfo ZA_ContPost2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPost2, x => AddInfo.ZA_ContPost2Info); }
		}

		public ZString ZA_ContPostCity
		{
			get { return AddInfo.ZA_ContPostCity; }
			set { AddInfo.ZA_ContPostCity = value; }
		}

		public ZPropertyInfo ZA_ContPostCityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPostCity, x => AddInfo.ZA_ContPostCityInfo); }
		}

		public ZString ZA_ContPostPostCode
		{
			get { return AddInfo.ZA_ContPostPostCode; }
			set { AddInfo.ZA_ContPostPostCode = value; }
		}

		public ZPropertyInfo ZA_ContPostPostCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPostPostCode, x => AddInfo.ZA_ContPostPostCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGContactInfoProviderAddInfoLookups.RefUNLOCOs))]
		public ZString ZA_ContPostPort
		{
			get { return AddInfo.ZA_ContPostPort; }
			set
			{
				AddInfo.ZA_ContPostPort = value;
				AddInfoValidation.ValidateZA_ContPostPostCode();
			}
		}

		public ZPropertyInfo ZA_ContPostPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPostPort, x => AddInfo.ZA_ContPostPortInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AUCLREGContactInfoProviderAddInfoLookups.OA_StateListForContactPostalAddress))]
		public ZString ZA_ContPostState
		{
			get { return AddInfo.ZA_ContPostState; }
			set { AddInfo.ZA_ContPostState = value; }
		}

		public ZPropertyInfo ZA_ContPostStateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPostState, x => AddInfo.ZA_ContPostStateInfo); }
		}

		#endregion

		[List(nameof(CLREGProvider) + "." + nameof(CLREGInfoProvider.AddInfoLookups) + "." + nameof(AUCLREGInfoProviderAddInfoLookups.Contacts))]
		[RelatedBusinessObject(nameof(Contact))]
		public ZGuid ZA_OC_Contact
		{
			get { return oc_Contact; }
			set
			{
				oc_Contact = value;
				var contact = Contact;
				if (contact != null)
				{
					ZA_ContName = contact.OC_ContactName;
					ZA_ContPurpose = !contact.OC_JobCategory.IsEmpty ? contact.OC_JobCategory : contact.OC_Title;

					var orgMainAddress = CLREGProvider.OrganizationMainAddress;
					var mobileNumber = !contact.OC_Mobile.IsEmpty ? contact.OC_Mobile : orgMainAddress.OA_Mobile;
					ZA_ContMob = mobileNumber.Replace("+", "").Replace(" ", "").Replace("(", "").Replace(")", "").Left(ZA_ContMobInfo.MaxLength);
					ZA_ContEmail = !contact.OC_Email.IsEmpty ? contact.OC_Email : orgMainAddress.OA_Email;

					var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, contact.Location);
					if (port != null)
					{
						var phone = !contact.OC_Phone.IsEmpty ? contact.OC_Phone : orgMainAddress.OA_Phone;
						var phoneInInternationalFormat = PhoneNumberValidator.ConvertToInternationalFormattedPhoneNumber(phone, port);
						ZA_ContPhPref = GetPhPrefix(phoneInInternationalFormat);
						ZA_ContPh = GetPhone(phoneInInternationalFormat).Left(ZA_ContPhInfo.MaxLength);

						phone = !contact.OC_Fax.IsEmpty ? contact.OC_Fax : orgMainAddress.OA_Fax;
						phoneInInternationalFormat = PhoneNumberValidator.ConvertToInternationalFormattedPhoneNumber(phone, port);
						ZA_ContFaxPref = GetPhPrefix(phoneInInternationalFormat);
						ZA_ContFax = GetPhone(phoneInInternationalFormat).Left(ZA_ContFaxInfo.MaxLength);

						phoneInInternationalFormat = PhoneNumberValidator.ConvertToInternationalFormattedPhoneNumber(contact.OC_HomePhone, port);
						ZA_ContAHPref = GetPhPrefix(phoneInInternationalFormat);
						ZA_ContAH = GetPhone(phoneInInternationalFormat);
					}
				}
			}
		}
		ZGuid oc_Contact;

		ZString GetPhPrefix(ZString phoneInInternationalFormat)
		{
			var result = ZString.Empty;
			if (phoneInInternationalFormat.IndexOf("(") > 0)
			{
				result = phoneInInternationalFormat.SubstringSafe(phoneInInternationalFormat.IndexOf("(") + 1, 1);
			}
			return result;
		}

		ZString GetPhone(ZString phoneInInternationalFormat)
		{
			var index = 3;
			if (phoneInInternationalFormat.IndexOf(")") > 0)
			{
				index = phoneInInternationalFormat.IndexOf(")") + 1;
			}
			return phoneInInternationalFormat.SubstringSafe(index).Trim();
		}

		protected PhoneNumberFormatAndValidation PhoneNumberValidator
		{
			get { return fPhoneNumberValidator ?? (fPhoneNumberValidator = new PhoneNumberFormatAndValidation()); }
		}
		PhoneNumberFormatAndValidation fPhoneNumberValidator;

		public OrgContact Contact
		{
			get { return Factory.Load<OrgContact>(ZA_OC_Contact); }
		}

		public ZString ZA_ContPhPref
		{
			get { return AddInfo.ZA_ContPhPref; }
			set { AddInfo.ZA_ContPhPref = value; }
		}

		public ZPropertyInfo ZA_ContPhPrefInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPhPref, x => AddInfo.ZA_ContPhPrefInfo); }
		}

		public ZString ZA_ContPh
		{
			get { return AddInfo.ZA_ContPh; }
			set { AddInfo.ZA_ContPh = value; }
		}

		public ZPropertyInfo ZA_ContPhInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPh, x => AddInfo.ZA_ContPhInfo); }
		}

		public ZString ZA_ContPhComment
		{
			get { return AddInfo.ZA_ContPhComment; }
			set { AddInfo.ZA_ContPhComment = value; }
		}

		public ZPropertyInfo ZA_ContPhCommentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContPhComment, x => AddInfo.ZA_ContPhCommentInfo); }
		}

		public ZString ZA_ContFaxPref
		{
			get { return AddInfo.ZA_ContFaxPref; }
			set { AddInfo.ZA_ContFaxPref = value; }
		}

		public ZPropertyInfo ZA_ContFaxPrefInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContFaxPref, x => AddInfo.ZA_ContFaxPrefInfo); }
		}

		public ZString ZA_ContFax
		{
			get { return AddInfo.ZA_ContFax; }
			set { AddInfo.ZA_ContFax = value; }
		}

		public ZPropertyInfo ZA_ContFaxInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContFax, x => AddInfo.ZA_ContFaxInfo); }
		}

		public ZString ZA_ContFaxComment
		{
			get { return AddInfo.ZA_ContFaxComment; }
			set { AddInfo.ZA_ContFaxComment = value; }
		}

		public ZPropertyInfo ZA_ContFaxCommentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContFaxComment, x => AddInfo.ZA_ContFaxCommentInfo); }
		}

		public ZString ZA_ContAHPref
		{
			get { return AddInfo.ZA_ContAHPref; }
			set { AddInfo.ZA_ContAHPref = value; }
		}

		public ZPropertyInfo ZA_ContAHPrefInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContAHPref, x => AddInfo.ZA_ContAHPrefInfo); }
		}

		public ZString ZA_ContAH
		{
			get { return AddInfo.ZA_ContAH; }
			set { AddInfo.ZA_ContAH = value; }
		}

		public ZPropertyInfo ZA_ContAHInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContAH, x => AddInfo.ZA_ContAHInfo); }
		}

		public ZString ZA_ContAHComment
		{
			get { return AddInfo.ZA_ContAHComment; }
			set { AddInfo.ZA_ContAHComment = value; }
		}

		public ZPropertyInfo ZA_ContAHCommentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContAHComment, x => AddInfo.ZA_ContAHCommentInfo); }
		}

		public ZString ZA_ContMob
		{
			get { return AddInfo.ZA_ContMob; }
			set { AddInfo.ZA_ContMob = value; }
		}

		public ZPropertyInfo ZA_ContMobInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContMob, x => AddInfo.ZA_ContMobInfo); }
		}

		public ZString ZA_ContMobComment
		{
			get { return AddInfo.ZA_ContMobComment; }
			set { AddInfo.ZA_ContMobComment = value; }
		}

		public ZPropertyInfo ZA_ContMobCommentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContMobComment, x => AddInfo.ZA_ContMobCommentInfo); }
		}

		public ZString ZA_ContEmail
		{
			get { return AddInfo.ZA_ContEmail; }
			set { AddInfo.ZA_ContEmail = value; }
		}

		public ZPropertyInfo ZA_ContEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ContEmail, x => AddInfo.ZA_ContEmailInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public AUCLREGContactInfoProviderAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AUCLREGContactInfoProviderAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		CLREGContactInfoProviderAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CLREGContactInfoProviderAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					fAddInfo.CLREGContactInfoProvider = this;
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		CLREGContactInfoProviderAddInfo fAddInfo;

		#endregion
	}
}
