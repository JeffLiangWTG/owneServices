using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("FullName"), WrapperTypeName("StaffMember")]
	public class StaffWrapper : GenericWrapper
	{
		public StaffWrapper(GlbStaff staff, BusinessObjectFactory factory)
			: base(staff, factory)
		{
			if (staff == null)
			{
				throw new ArgumentNullException(nameof(staff));
			}
		}

		public ZString FullName
		{
			get { return Staff.GS_FullName; }
		}

		public ZString FirstName
		{
			get { return Staff.GS_FriendlyName; }
		}

		public ZString Title
		{
			get { return Staff.GS_Title; }
		}

		public Image Signature
		{
			get
			{
				if (signature != null && signature.IsDisposed())
				{
					signature = null;
				}

				if (signature == null && Staff.GS_UserSignature.Length > 0)
				{
					var usageDetailsCollector = Factory?.ServiceContainer.GetService<DocumentUsageDetailsCollector>();
					usageDetailsCollector?.GetIsUserSignatureUsed(true);

					MemoryStream stream = new MemoryStream(Staff.GS_UserSignature);
					signature = Image.FromStream(stream);
				}

				return signature;
			}
		}
		Image signature;

		public Image SignatureForQuoteDocuments
		{
			get { return Env.Registry.Rating.PrintScannedUserSignatureOnQuotationDocuments ? Signature : null; }
		}

		public SecureDetailWrapper WorkPhone
		{
			get { return workPhone ?? (workPhone = new SecureDetailWrapper(Staff.GS_WorkPhone_Formatted, Staff.GS_PublishWorkPhone, Factory)); }
		}
		SecureDetailWrapper workPhone;

		public SecureDetailWrapper WorkExtension
		{
			get { return workExtension ?? (workExtension = new SecureDetailWrapper(Staff.GS_WorkExtension, Staff.GS_PublishWorkExtension, Factory)); }
		}
		SecureDetailWrapper workExtension;

		public SecureDetailWrapper MobilePhone
		{
			get { return mobilePhone ?? (mobilePhone = new SecureDetailWrapper(Staff.GS_MobilePhone_Formatted, Staff.GS_PublishMobilePhone, Factory)); }
		}
		SecureDetailWrapper mobilePhone;

		public SecureDetailWrapper HomePhone
		{
			get { return homePhone ?? (homePhone = new SecureDetailWrapper(Staff.GS_HomePhone_Formatted, Staff.GS_PublishHomePhone, Factory)); }
		}
		SecureDetailWrapper homePhone;

		public SecureDetailWrapper FaxNum
		{
			get { return faxNum ?? (faxNum = new SecureDetailWrapper(Staff.GS_FaxNum_Formatted, Staff.GS_PublishFaxNum, Factory)); }
		}
		SecureDetailWrapper faxNum;

		public SecureDetailWrapper EmailAddress
		{
			get { return emailAddress ?? (emailAddress = new SecureDetailWrapper(Staff.GS_EmailAddress, Staff.GS_PublishEmailAddress, Factory)); }
		}
		SecureDetailWrapper emailAddress;

		#region Implementation

		GlbStaff Staff
		{
			get { return (GlbStaff)WrappedBO; }
		}

		#endregion
	}
}
