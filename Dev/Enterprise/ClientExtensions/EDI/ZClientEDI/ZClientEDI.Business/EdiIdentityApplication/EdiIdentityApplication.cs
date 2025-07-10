using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IdentityApplicationPermission.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.IdentityRedirectUrl.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IdentityApplication.Business
{
	[CodeProperty("IDA_ApplicationName")]
	[DescriptionProperty("IDA_ClientID")]
	public class EdiIdentityApplication : AutoEdiIdentityApplication
	{
		public EdiIdentityApplication(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public EdiIdentityApplication ParentApplication => Factory.Load<EdiIdentityApplication>(IDA_IDA_ParentApplication);

		[ChildEditable(true)]
		public EdiIdentityCertificateCollection Certificates
		{
			get
			{
				if (certificates == null)
				{
					certificates = new EdiIdentityCertificateCollection(Factory, this);
					RegisterEditableChildObject(certificates);
				}
				return certificates;
			}
		}

		EdiIdentityCertificateCollection certificates;

		[ChildEditable(true)]
		public EdiIdentityRedirectUrlCollection RedirectUrls
		{
			get
			{
				if (redirectUrls == null)
				{
					redirectUrls = new EdiIdentityRedirectUrlCollection(Factory, this);
					RegisterEditableChildObject(redirectUrls);
				}

				return redirectUrls;
			}
		}

		EdiIdentityRedirectUrlCollection redirectUrls;

		public EdiLicenceDatabaseCollection Licenses
		{
			get
			{
				if (licenses == null)
				{
					licenses = new EdiLicenceDatabaseCollection(Factory, new ZQuery(LicenceDatabaseSchema.PK, IDA_LD));
				}

				return licenses;
			}
		}

		EdiLicenceDatabaseCollection licenses;

		[ChildEditable(true)]
		public EdiIdentityApplicationPermissionCollection Permissions
		{
			get
			{
				if (permissions == null)
				{
					permissions = new EdiIdentityApplicationPermissionCollection(Factory, this);
					RegisterEditableChildObject(permissions);
				}

				return permissions;
			}
		}

		EdiIdentityApplicationPermissionCollection permissions;

		[ResourceStringData("CE59D27B-9A41-4129-B917-D6292A70EC57", Caption = "Parent Org.")]
		public override ZGuid IDA_OH_ParentOrg { get => base.IDA_OH_ParentOrg; set => base.IDA_OH_ParentOrg = value; }

		[RelatedBusinessObject("ParentApplication")]
		[List("Lookups.ParentApplications")]
		[ResourceStringData("2C4893B7-8585-4541-91FD-48D77C65844F", Caption = "Parent Application")]
		public override ZGuid IDA_IDA_ParentApplication { get => base.IDA_IDA_ParentApplication; set => base.IDA_IDA_ParentApplication = value; }

		[List("Lookups.EdiIdentityApplicationRedirectUrlStatusList")]
		public override ZString IDA_RedirectUrlStatus
		{
			get => base.IDA_RedirectUrlStatus;
			set => base.IDA_RedirectUrlStatus = value;
		}

		[ResourceStringData("2D3EF774-F9F7-489D-98A1-4CEF85CDC411", Caption = "Application Name")]
		public override ZString IDA_ApplicationName
		{
			get => base.IDA_ApplicationName;
			set => base.IDA_ApplicationName = value;
		}

		[ResourceStringData("71130B9A-2963-41C1-A8D5-AC5EE9371C88", Caption = "Client ID")]
		public override ZString IDA_ClientID
		{
			get => base.IDA_ClientID;
			set => base.IDA_ClientID = value;
		}

		[ResourceStringData("81FC097D-2BEF-4B93-8376-FC3652915FBE", Caption = "Is Active")]
		public override ZBool IDA_IsActive
		{
			get => base.IDA_IsActive;
			set => base.IDA_IsActive = value;
		}

		[ResourceStringData("912100E5-90D7-4A75-A63C-BD037DA732A9", Caption = "Application Type")]
		[List("Lookups.DatabaseTypesList")]
		[MaxLength(3)]
		public ZString ApplicationType
		{
			get
			{
				return IDA_LD.IsEmpty ? IDA_ApplicationType : LicenceDatabase.LD_LicenceType;
			}
			set
			{
				if (IDA_LD.IsEmpty)
				{
					IDA_ApplicationType = value;
				}
			}
		}

		public ZPropertyInfo ApplicationTypeInfo => IDA_ApplicationTypeInfo;

		[ResourceStringData("5C1CECF4-C63E-498A-9E08-6334599B75FD", Caption = "Product")]
		[List("Lookups.ProductTypeList")]
		[MaxLength(3)]
		public ZString Product
		{
			get
			{
				return IDA_LD.IsEmpty ? IDA_Product : LicenceDatabase.LD_Product;
			}
			set
			{
				if (IDA_LD.IsEmpty)
				{
					IDA_Product = value;
				}
			}
		}

		public EdiIdentityTenant Tenant => Factory.Load<EdiIdentityTenant>(IDA_IDT);

		[ResourceStringData("49CB94F7-5EA9-480C-9794-6424A7DF8331", Caption = "Tenant ID")]
		public ZString TenantID => Tenant == null ? ZString.Empty : Tenant.IDT_TenantId;

		public ZPropertyInfo ProductInfo => IDA_ProductInfo;

		public bool TenantID_ReadOnly => true;

		public bool Product_ReadOnly => LicenceDatabase != null;

		public bool ApplicationType_ReadOnly => LicenceDatabase != null;

		public bool IDA_ApplicationName_ReadOnly => IsInDatabase;

		public bool IDA_IDA_ParentApplication_ReadOnly => IsInDatabase;

		public bool IDA_OH_ParentOrg_ReadOnly => IsInDatabase;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("cb7d8f6b-c9cb-48be-85d7-344c38ce1d2c", "Application - {0}", IDA_ApplicationName);

		public LicenceDatabase LicenceDatabase => Factory.Load<LicenceDatabase>(IDA_LD);

		public ZString LicenceType => LicenceDatabase == null ? ZString.Empty : LicenceDatabase.LD_LicenceType;

		public ZString LicenceProductCode => LicenceDatabase == null ? ZString.Empty : LicenceDatabase.LD_Product;

		public ZBool IsLicenceActive => LicenceDatabase != null && LicenceDatabase.LD_IsActive;

		public override bool IsCancelled
		{
			get
			{
				return base.IsCancelled;
			}
			set
			{
				if (!value)
				{
					IDA_IsRollback = value;
				}

				base.IsCancelled = value;
			}
		}

		public bool IsCustomerApplication
		{
			get
			{
				if (IsInDatabase)
				{
					return IDA_IDT.IsEmpty;
				}
				else
				{
					return isCustomerApplication;
				}
			}
			set
			{
				isCustomerApplication = value;
			}
		}
		bool isCustomerApplication;

		public override void OnSaving()
		{
			base.OnSaving();

			if (IDA_RedirectUrlStatusInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(AutoEvents.EditedARecord, $"Status : {IDA_RedirectUrlStatusInfo.OriginalValue} to {IDA_RedirectUrlStatus}.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			if (IsCustomerApplication && IDA_ClientID.IsEmpty)
			{
				IDA_ClientID = ZGuid.NewZGuid().ToString();
			}

			if (!IsCustomerApplication && IDA_IDT.IsEmpty)
			{
				var tenantId = EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value;
				var tenant = Factory.LoadTop1<EdiIdentityTenant>(new ZQuery(EdiIdentityTenantSchema.IDT_TenantId, tenantId));
				if (tenant != null)
				{
					IDA_IDT = tenant.PK;
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_GraphClientId = ZGuid.NewZGuid().ToString();
			tenant.IDT_TenantId = ZGuid.NewZGuid().ToString();

			IDA_IDT = tenant.PK;
			IDA_ClientID = ZGuid.NewZGuid().ToString();

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}
}
