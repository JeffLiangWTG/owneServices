using System;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocStaff : DocumentWrapper
	{
		DocStaff(GlbStaff staff, BusinessObjectFactory factoryForWrapper)
			: base(staff, factoryForWrapper)
		{
		}

		public static DocStaff New(GlbStaff staff, BusinessObjectFactory factoryForWrapper)
		{
			return (staff != null) ? new DocStaff(staff, factoryForWrapper) : null;
		}

		public static DocStaff New(ZGuid staffPK, BusinessObjectFactory factoryForWrapper)
		{
			GlbStaff staff = factoryForWrapper.Load<GlbStaff>(staffPK);
			return New(staff, factoryForWrapper);
		}

		public static DocStaff New(ZString staffCode, BusinessObjectFactory factoryForWrapper)
		{
			GlbStaff staff = factoryForWrapper.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
			return New(staff, factoryForWrapper);
		}

		GlbStaff Staff
		{
			get { return (GlbStaff)WrappedObject; }
		}

		public override string ToString()
		{
			return FullName;
		}

		#region Signature

		public Image Signature
		{
			get
			{
				if (Staff.GS_UserSignature.Length > 0)
				{
					var usageDetailsCollector = Factory?.ServiceContainer.GetService<DocumentUsageDetailsCollector>();
					usageDetailsCollector?.GetIsUserSignatureUsed(true);

					MemoryStream stream = new MemoryStream(Staff.GS_UserSignature);
					return Image.FromStream(stream);
				}
				return null;
			}
		}

		public ZBool IsSignatureSpecified
		{
			get { return Signature != null; }
		}

		/// <summary>
		/// Used for Quotations and Rating Documents Only
		/// </summary>
		public Image PrintSignatureOnQuoteDocuments
		{
			get { return Env.Registry.Rating.PrintScannedUserSignatureOnQuotationDocuments ? Signature : null; }
		}

		#endregion

		#region Properties

		public ZString City
		{
			get { return Staff.GS_City; }
		}

		public ZString EmailAddress
		{
			get { return Staff.GS_EmailAddress; }
		}

		public ZString PublishEmailAddress
		{
			get { return Staff.GS_PublishEmailAddress ? Staff.GS_EmailAddress : ZString.Empty; }
		}

		public ZString EmergencyContactName
		{
			get { return Staff.GS_EmergencyContactName; }
		}

		public ZString EmergencyHomePhone
		{
			get { return Staff.GS_EmergencyHomePhone_Formatted; }
		}

		public ZString EmergencyWorkPhone
		{
			get { return Staff.GS_EmergencyWorkPhone_Formatted; }
		}

		public ZString FaxNum
		{
			get { return Staff.GS_FaxNum_Formatted; }
		}

		public ZString PublishFaxNum
		{
			get { return Staff.GS_PublishFaxNum ? Staff.GS_FaxNum_Formatted : ZString.Empty; }
		}

		public ZString FridayWorkingHours
		{
			get { return Staff.WorkTimes.FridayWorkingHours; }
		}

		public ZString FullName
		{
			get { return Staff.GS_FullName; }
		}

		public ZString HomePhone
		{
			get { return Staff.GS_HomePhone_Formatted; }
		}

		public ZString PublishHomePhone
		{
			get { return Staff.GS_PublishHomePhone ? Staff.GS_HomePhone_Formatted : ZString.Empty; }
		}

		public ZString Initials
		{
			get { return Staff.GS_Code; }
		}

		public ZString LoginName
		{
			get { return Staff.GS_LoginName; }
		}

		public ZString MobilePhone
		{
			get { return Staff.GS_MobilePhone_Formatted; }
		}

		public ZString PublishMobilePhone
		{
			get { return Staff.GS_PublishMobilePhone ? Staff.GS_MobilePhone_Formatted : ZString.Empty; }
		}

		public ZString MondayWorkingHours
		{
			get { return Staff.WorkTimes.MondayWorkingHours; }
		}

		public ZString NationalityCode
		{
			get { return Staff.GS_RN_NKNationalityCode; }
		}

		public ZString NextOfKin
		{
			get { return Staff.GS_NextOfKin; }
		}

		public ZString NextOfKinHomePhone
		{
			get { return Staff.GS_NextOfKinHomePhone_Formatted; }
		}

		public ZString NextOfKinWorkPhone
		{
			get { return Staff.GS_NextOfKinWorkPhone_Formatted; }
		}

		public ZString OutOnTask
		{
			get { return Staff.GS_OutOnTask; }
		}

		public ZString Pager
		{
			get { return Staff.GS_Pager; }
		}

		public ZString Passport
		{
			get { return Staff.GS_Passport; }
		}

		public ZString Postcode
		{
			get { return Staff.GS_Postcode; }
		}

		public ZString SaturdayWorkingHours
		{
			get { return Staff.WorkTimes.SaturdayWorkingHours; }
		}

		public ZString State
		{
			get { return Staff.GS_State; }
		}

		public ZString SundayWorkingHours
		{
			get { return Staff.WorkTimes.SundayWorkingHours; }
		}

		public ZString ThursdayWorkingHours
		{
			get { return Staff.WorkTimes.ThursdayWorkingHours; }
		}

		public ZString Title
		{
			get { return Staff.GS_Title; }
		}

		public ZString TuesdayWorkingHours
		{
			get { return Staff.WorkTimes.TuesdayWorkingHours; }
		}

		public ZString UserAddress1
		{
			get { return Staff.GS_UserAddress1; }
		}

		public ZString UserAddress2
		{
			get { return Staff.GS_UserAddress2; }
		}

		public ZString WednesdayWorkingHours
		{
			get { return Staff.WorkTimes.WednesdayWorkingHours; }
		}

		public ZString WorkExtension
		{
			get { return Staff.GS_WorkExtension; }
		}

		public ZString PublishWorkExtension
		{
			get { return Staff.GS_PublishWorkExtension ? Staff.GS_WorkExtension : ZString.Empty; }
		}

		public ZString WorkPhone
		{
			get { return Staff.GS_WorkPhone_Formatted; }
		}

		public ZString PublishWorkPhone
		{
			get { return Staff.GS_PublishWorkPhone ? Staff.GS_WorkPhone_Formatted : ZString.Empty; }
		}

		public ZString NationalIdentityNumber
		{
			get { return GlbStaff.CurrentUser.NationalIdentityNumber; }
		}

		public ZString CustomRelationshipToOrganisation
		{
			get { return fCustomRelationshipToOrganisation; }
			set { fCustomRelationshipToOrganisation = value; }
		}

		ZString fCustomRelationshipToOrganisation;

		public ZBool ChangePasswordAtNextLogin
		{
			get { return Staff.ChangePasswordAtNextLogin; }
		}

		public ZBool IsActive
		{
			get { return Staff.GS_IsActive; }
		}

		public ZBool IsController
		{
			get { return Staff.GS_IsController; }
		}

		public ZBool IsDeveloper
		{
			get { return Staff.GS_IsDeveloper; }
		}

		public ZBool IsInTrainingMode
		{
			get { return Staff.GS_IsInTrainingMode; }
		}

		public ZBool IsSalesRep
		{
			get { return Staff.GS_IsSalesRep; }
		}

		public ZBool IsSystemAccount
		{
			get { return Staff.GS_IsSystemAccount; }
		}

		public ZBool PasswordNeverChanges
		{
			get { return Staff.PasswordNeverChanges; }
		}

		public ZDateTime DueBack
		{
			get { return Staff.GS_DueBack; }
		}

		public ZDateTime EmploymentDate
		{
			get { return Staff.GS_EmploymentDate; }
		}

		public ZDateTime LastPasswordChangeDate
		{
			get { return Staff.LastPasswordChangeDate; }
		}

		public ZString BrokerID // This code should be rename to NZ specific Broker ID.
		{
			get
			{
				var wrapper = NZGlbStaffWrapper;
				var nZBPassword = wrapper?.GetGlbExternalPassword<Enterprise.Customs.NZ.Business.Declaration.GlbExternalPassword_NZ>(PasswordTypesList.Codes.NZB, GlbCompany.CurrentCompany.PK);
				return nZBPassword?.GP_UserID ?? ZString.Empty;
			}
		}

		Enterprise.Customs.NZ.Business.Declaration.NZGlbStaffWrapper NZGlbStaffWrapper
		{
			get
			{
				if (nzGlbStaffWrapper == null || nzGlbStaffWrapper.PK != Staff.PK)
				{
					nzGlbStaffWrapper = (Enterprise.Customs.NZ.Business.Declaration.NZGlbStaffWrapper)Staff.GetNZWrapper();
				}
				return nzGlbStaffWrapper;
			}
		}
		Enterprise.Customs.NZ.Business.Declaration.NZGlbStaffWrapper nzGlbStaffWrapper;

		#endregion

		#region base64 encoding

		public ZString SignatureEncoded
		{
			get
			{
				return EncodeBytes(Staff.GS_UserSignature);
			}
		}

		public ZString ProfilePhotoEncoded
		{
			get
			{
				return EncodeBytes(Staff.GS_ProfilePhoto);
			}
		}

		ZString EncodeBytes(byte[] bytes)
		{
			if (bytes != null && bytes.Length > 0)
			{
				return Convert.ToBase64String(bytes);
			}

			return ZString.Empty;
		}

		#endregion
	}
}
