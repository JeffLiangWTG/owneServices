using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Licensing.Billing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[CodeProperty(LicenceEnterprise.Schema.LE_EnterpriseID), DescriptionProperty(LicenceEnterprise.Schema.OrganisationName)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class LicenceEnterprise : AutoLicenceEnterprise
	{
		public LicenceEnterprise(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoLicenceEnterprise.Schema
		{
			public const string OrganisationName = "OrganisationName";
		}

		#region Parent Organisation

		[RelatedBusinessObject("Organisation")]
		public override ZGuid LE_OH
		{
			get { return base.LE_OH; }
			set { base.LE_OH = value; }
		}

		public EDIOrgHeader Organisation
		{
			get { return Factory.Load<EDIOrgHeader>(LE_OH); }
		}

		public ZString OrganisationName
		{
			get { return Organisation != null ? Organisation.OH_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OrganisationNameInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationName)); }
		}

		#endregion

		#region Business Object Overrides

		public override ZString LE_EnterpriseCode
		{
			get { return base.LE_EnterpriseCode; }
			set
			{
				base.LE_EnterpriseCode = value;
				if (Header != null)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		public bool LE_EnterpriseCode_ReadOnly
		{
			get
			{
				var products = EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode.Value;
				var hasDatabaseRequiringEntCode = Databases.OfType<LicenceDatabase>().Any(x => products.Contains(x.LD_Product.ToString()));
				return IsInDatabase && !LE_EnterpriseCode.IsEmpty && !LE_EnterpriseCodeInfo.HasChanges && hasDatabaseRequiringEntCode;
			}
		}

		public override void Delete()
		{
			base.Delete();
			Databases.RemoveAndDeleteAll();
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get { return "Enterprise Record " + LE_EnterpriseCode; }
		}

		#endregion

		#region Databases

		public LicenceDatabaseCollection Databases
		{
			get
			{
				if (fDatabases == null || DatabaseCollectionRequiresReload)
				{
					DatabaseCollectionRequiresReload = false;
					if (fDatabases == null)
					{
						fDatabases = new LicenceDatabaseCollection(this, Factory);
					}
					fDatabases.Load();
					fDatabases.IsManagedForDataRefresh = true;
				}
				return fDatabases;
			}
		}
		LicenceDatabaseCollection fDatabases;
		public bool DatabaseCollectionRequiresReload;

		public ZInt NumOfDatabasesRegistered
		{
			get { return Databases.Count; }
		}

		public ZPropertyInfo NumOfDatabasesRegisteredInfo
		{
			get { return GetZPropertyInfo(nameof(NumOfDatabasesRegistered)); }
		}

		#endregion

		#region Companies

		public LicenceCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					var localCompanies = new LicenceCompanyCollection(Factory, new ZQuery(LicenceCompanySchema.LC_LE, PK));
					localCompanies.Load();
					fCompanies = localCompanies;
				}

				return fCompanies;
			}
		}
		LicenceCompanyCollection fCompanies;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new LicenceEnterpriseFetchStrategy(this);
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceModify.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		static public bool IsUATEnterprise(ZGuid pk)
		{
			var settings = EDIDataRegistry.Instance.InternalIncidentLicenceSettings.Value;

			return pk.IsValid
				&& settings.UAT_ALP_Licence != null
				&& settings.UAT_ALP_Licence.Company != null
				&& pk == settings.UAT_ALP_Licence.Company.LC_LE;
		}

		#region LE_EnterpriseID

		protected bool LE_EnterpriseID_ReadOnly => true;

		public override void OnSaving()
		{
			base.OnSaving();
			SetEnterpriseIDIfRequired();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					LE_EnterpriseID = "";
				}
			}
		}

		void SetEnterpriseIDIfRequired()
		{
			if (!IsInDatabase && LE_EnterpriseID.IsEmpty)
			{
				LE_EnterpriseID = Modules.ClientNumberFountainRegistration.GetInstance().EnterpriseID.GetNextFormatted(Factory);
			}
		}

		#endregion

		protected override Licensing.Billing.Business.LicenceEnterpriseValidation GetNewValidation()
		{
			return new LicenceEnterpriseValidation(this);
		}

		protected override Licensing.Billing.Business.LicenceEnterpriseLookups GetNewLookups()
		{
			return new LicenceEnterpriseLookups(this);
		}
	}
}

