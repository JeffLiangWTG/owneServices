using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class UsersAuthorizedToReopenClosedPeriods : RegistryBusinessObjectTemplate
	{
		public UsersAuthorizedToReopenClosedPeriods(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public UsersAuthorizedToReopenClosedPeriods()
			: base()
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string StaffPK = "StaffPK";
			public const string LoginName = "LoginName";
			public const string FullName = "FullName";
		}

		#endregion

		#region Implementation

		void PopulateInfoOfGlobalStaffProperties()
		{
			loginName = ZString.Empty;
			fullName = ZString.Empty;
			GlbStaff staff = CurrentFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPK));
			if (staff != null)
			{
				loginName = (ZString)staff[GlbStaffSchema.GS_LoginName];
				fullName = (ZString)staff[GlbStaffSchema.GS_FullName];
			}
		}

		#endregion

		#region Properties

		#region Global Staff

		ZGuid staffPK;
		[List("GlobalStaffList")]
		public ZGuid StaffPK
		{
			get { return staffPK; }
			set
			{
				if (staffPK != value)
				{
					SetNonPersistentPropertyValue(StaffPKInfo, ref staffPK, value);
					PopulateInfoOfGlobalStaffProperties();
				}
				if (!IsValidationSuspended)
				{
					ValidateStaffPK();
				}
			}
		}

		public ZPropertyInfo StaffPKInfo
		{
			get { return GetZPropertyInfo(Schema.StaffPK); }
		}

		public void ValidateStaffPK()
		{
			StaffPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StaffPKInfo);
			ListValidation.ErrorIfInvalidPK(StaffPKInfo, GlobalStaffList);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(StaffPKInfo, ErrorDuplicateGlobalStaff);
			}
		}

		#endregion

		#region Login Name

		ZString loginName;
		[BusinessObjectTestExclude]
		public ZString LoginName
		{
			get { return loginName; }
		}

		public ZPropertyInfo LoginNameInfo
		{
			get { return GetZPropertyInfo(Schema.LoginName); }
		}

		#endregion

		#region Full Name

		ZString fullName;
		[BusinessObjectTestExclude]
		public ZString FullName
		{
			get { return fullName; }
		}

		public ZPropertyInfo FullNameInfo
		{
			get { return GetZPropertyInfo(Schema.FullName); }
		}

		#endregion

		#region Global Staff List

		GlbStaffCollection globalStaffList;
		public GlbStaffCollection GlobalStaffList
		{
			get
			{
				if (globalStaffList == null)
				{
					globalStaffList = new GlbStaffCollection(CurrentFactory);
				}
				return globalStaffList;
			}
		}

		#endregion

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			UsersAuthorizedToReopenClosedPeriods clone = new UsersAuthorizedToReopenClosedPeriods(fallbackLevel, factory);
			clone.StaffPK = StaffPK;
			return clone;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.StaffPK, StaffPK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			staffPK = new ZGuid(reader.ReadElementString(Schema.StaffPK));
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateStaffPK();
		}
		#endregion

		public string ErrorDuplicateGlobalStaff = Res.GetString("2358f9c1-77f7-491a-a706-9f44532b33f1", "User has already been selected. Cannot select the same User twice.");
	}
}
