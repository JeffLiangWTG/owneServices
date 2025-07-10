using System.Collections;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class OrganisationXmlDataImporter : XmlDataImporter
	{
		public OrganisationXmlDataImporter(IValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		public OrganisationXmlDataImporter(BusinessObjectFactoryProvider factoryProvider, IValueObjectDataAdapter adapter)
			: base(factoryProvider, adapter)
		{
		}

		protected ImportedBusinessObjectCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new ImportedBusinessObjectCollection(FactoryProvider.Current);
				}

				return collection;
			}
		}
		ImportedBusinessObjectCollection collection;

		protected override void ImportXml(TextReader reader, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			OrgStatuses.Clear();
			XmlValueObjectSerializer serialiser = GetSerializer();
			serialiser.ImportXmlData(((StreamReader)reader).BaseStream, Adapter, Collection, factoryProvider, notifications);
			SetEDIInterchange(serialiser.ImportContext);
			AfterImportXml(Collection, notifications);
		}

		protected override void AfterImportXml(IBusinessObjectCollection bizObjsImported, INotifications notifications)
		{
			base.AfterImportXml(bizObjsImported, notifications);

			foreach (BusinessObject obj in bizObjsImported)
			{
				ZString cratedUpdated = obj.IsInDatabase ? " " + Res.GetString("317b13dd-f36a-473f-8769-cdf635615b11", "updated") : " " + Res.GetString("5a2a7d2f-9f5e-4eca-98b6-4e44324821c3", "created");
				OrgStatuses.Add(obj.PK, cratedUpdated);
			}
		}

		protected override void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
			if (sucessfullyImported)
			{
				foreach (BusinessObject obj in Collection)
				{
					buffer.Notify(new InfoNotification(obj.HumanReadableName + OrgStatuses[obj.PK].ToString()));
				}
			}
		}

		protected Hashtable OrgStatuses
		{
			get
			{
				if (orgStatuses == null)
				{
					orgStatuses = new Hashtable();
				}

				return orgStatuses;
			}
		}
		Hashtable orgStatuses;
	}
}
