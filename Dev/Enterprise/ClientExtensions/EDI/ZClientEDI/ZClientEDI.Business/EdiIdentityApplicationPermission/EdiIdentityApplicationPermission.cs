using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business
{
	public class EdiIdentityApplicationPermission : AutoEdiIdentityApplicationPermission
	{
		public EdiIdentityApplicationPermission(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public EdiIdentityApplication Application => Factory.Load<EdiIdentityApplication>(IAP_IDA);

		[ResourceStringData("30DFF23F-64B9-433F-AE6E-B38EB862A999", Caption = "Scope")]
		public override ZString IAP_Scope { get => base.IAP_Scope; set => base.IAP_Scope = value; }

		public bool IAP_Scope_ReadOnly => true;

		[List("Lookups.ApplicationPermissions")]
		[ResourceStringData("3A30D3F1-4736-4500-A20F-CE7DD8E01F56", Caption = "Permission")]
		public ZString Permission
		{
			get
			{
				if (IAP_Scope == EdiIdentityApplicationPermissionLookups.AllApplicationsScope)
				{
					return EdiIdentityApplicationPermissionLookups.AllApplicationsScope;
				}

				if (CargoVisibilityAPIScope.Equals(IAP_Scope, StringComparison.InvariantCultureIgnoreCase))
				{
					return CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI;
				}

				if (AuditAPIScope.Equals(IAP_Scope, StringComparison.InvariantCultureIgnoreCase))
				{
					return CustomerApplicationPermissionsList.Codes.AuditAPI;
				}

				return ZString.Empty;
			}
			set
			{
				if (value == EdiIdentityApplicationPermissionLookups.AllApplicationsScope)
				{
					IAP_Scope = EdiIdentityApplicationPermissionLookups.AllApplicationsScope;
				}
				else if (CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					IAP_Scope = CargoVisibilityAPIScope;
				}
				else if (CustomerApplicationPermissionsList.Codes.AuditAPI.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					IAP_Scope = AuditAPIScope;
				}
				else
				{
					IAP_Scope = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo PermissionInfo => IAP_ScopeInfo;

		string ParentCW1ApplicationClientId => Application.ParentApplication?.IDA_ClientID.ToString() ?? string.Empty;

		string AuditAPIScope => string.Join("/", ParentCW1ApplicationClientId, CustomerApplicationPermissionsList.Codes.AuditAPI);

		const string CargoVisibilityAPIScope = "f30a1049-9790-4653-bbac-79c7a040278c";

		[RelatedBusinessObject("Application")]
		public override ZGuid IAP_IDA
		{
			get { return base.IAP_IDA; }
			set { base.IAP_IDA = value; }
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var cw1Application = Factory.NewWithValidTestData<EdiIdentityApplication>();

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IsCustomerApplication = true;
			application.IDA_IDA_ParentApplication = cw1Application.PK;

			IAP_IDA = application.PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

	}
}
