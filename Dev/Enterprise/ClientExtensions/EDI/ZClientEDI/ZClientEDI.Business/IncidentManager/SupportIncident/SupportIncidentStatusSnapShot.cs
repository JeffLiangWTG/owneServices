using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentStatusSnapShot
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory { RefreshEnabled = false });

		BusinessObjectFactory factory;

		#region Incident's Properties

		public ZString IM_Status { get; private set; }
		public ZString IM_ResolutionCode { get; private set; }
		public ZString IM_ClosureResolution { get; private set; }
		public ZString IM_Category { get; private set; }
		public ZString IM_Priority { get; private set; }
		public ZString IM_Module { get; private set; }
		public ZString IM_SourceModuleId { get; private set; }
		public ZString ProductArea { get; private set; }
		public ZString IM_Description { get; private set; }
		public ZString IM_Product { get; private set; }
		public ZGuid IM_Contact { get; private set; }
		public ZGuid IM_LD { get; private set; }
		public ZGuid IM_LCC { get; private set; }
		public ZGuid IM_OH_Client { get; private set; }
		public ZString IncidentDetails { get; set; }

		#endregion

		#region Related Properties

		OrgHeader Client
		{
			get
			{
				ReloadRelatedObjectsDBOnly();
				return client;
			}
			set
			{
				client = value;
			}
		}
		OrgHeader client;

		OrgContact Contact
		{
			get
			{
				ReloadRelatedObjectsDBOnly();
				return contact;
			}
			set
			{
				contact = value;
			}
		}
		OrgContact contact;

		LicenceDatabase DatabaseObject
		{
			get
			{
				ReloadRelatedObjectsDBOnly();
				return databaseObject;
			}
			set
			{
				databaseObject = value;
			}
		}
		LicenceDatabase databaseObject;

		ClientCompany ClientCompanyObject
		{
			get
			{
				ReloadRelatedObjectsDBOnly();
				return clientCompanyObject;
			}
			set
			{
				clientCompanyObject = value;
			}
		}
		ClientCompany clientCompanyObject;

		LicenceEnterprise LicEnterprise
		{
			get
			{
				ReloadRelatedObjectsDBOnly();
				return licEnterprise;
			}
			set
			{
				licEnterprise = value;
			}
		}
		LicenceEnterprise licEnterprise;

		public ZString SourceModuleWithPath
		{
			get
			{
				ReloadRelatedObjectsDBOnly();
				return sourceModuleWithPath;
			}
			private set
			{
				sourceModuleWithPath = value;
			}
		}
		ZString sourceModuleWithPath;

		public ZString ClientCode => Client?.OH_Code ?? ZString.Empty;

		public ZString ContactName => Contact?.OC_ContactName ?? ZString.Empty;

		public ZString Database => DatabaseObject?.LD_ServerCode ?? ZString.Empty;

		public ZString EnterpriseID => LicEnterprise?.LE_EnterpriseID ?? ZString.Empty;

		public ZString EnterpriseCode => LicEnterprise?.LE_EnterpriseCode ?? ZString.Empty;

		public ZString ClientCompanyCode => ClientCompanyObject?.LCC_Code ?? ZString.Empty;

		#endregion

		bool ShouldReloadRelatedObjects { get; set; }

		void ReloadRelatedObjects(bool dataBaseOnly)
		{
			Client = Factory.Load<OrgHeader>(IM_OH_Client);
			Contact = Factory.Load<OrgContact>(IM_Contact);
			DatabaseObject = Factory.Load<LicenceDatabase>(IM_LD);
			ClientCompanyObject = Factory.Load<ClientCompany>(IM_LCC);
			ShouldReloadRelatedObjects = false;

			var licenceEnterprisePK = SupportIncident.GetEnterprisePK(DatabaseObject, (EDIOrgHeader)Client, dataBaseOnly);
			LicEnterprise = Factory.Load<LicenceEnterprise>(licenceEnterprisePK);
			SourceModuleWithPath = SupportIncident.GetSourceModuleWithPath(IM_SourceModuleId, IM_Product);
		}

		void ReloadRelatedObjectsDBOnly()
		{
			if (ShouldReloadRelatedObjects)
			{
				ReloadRelatedObjects(true);
			}
		}

		void ReloadRelatedObjectsImmediately()
		{
			ReloadRelatedObjects(false);
		}

		void TakeSnapShotBase(SupportIncident incident, bool includeIncidentDetails)
		{
			factory = incident.Factory;
			IM_Status = incident.IM_Status;
			IM_ResolutionCode = incident.IM_ResolutionCode;
			IM_ClosureResolution = incident.IM_ClosureResolution;
			IM_Category = incident.IM_Category;
			IM_Priority = incident.IM_Priority;
			IM_Module = incident.IM_Module;
			ProductArea = incident.ProductArea;
			IM_Description = incident.IM_Description;
			IM_Product = incident.IM_Product;
			IM_SourceModuleId = incident.IM_SourceModuleId;

			IM_Contact = incident.IM_OC_Contact;
			IM_LD = incident.IM_LD;
			IM_LCC = incident.IM_LCC;
			IM_OH_Client = incident.IM_OH_Client;

			if (includeIncidentDetails)
			{
				IncidentDetails = incident.DetailNoteText;
			}
		}

		public void TakeSnapShot(SupportIncident incident, bool includeIncidentDetails)
		{
			TakeSnapShotBase(incident, includeIncidentDetails);
			ReloadRelatedObjectsImmediately();
		}

		public void TakeSnapShotLazyLoading(SupportIncident incident, bool includeIncidentDetails)
		{
			TakeSnapShotBase(incident, includeIncidentDetails);
			ShouldReloadRelatedObjects = true;
		}
	}
}
