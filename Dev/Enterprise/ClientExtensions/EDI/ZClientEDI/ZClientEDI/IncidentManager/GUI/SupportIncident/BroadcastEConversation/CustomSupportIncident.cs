using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class CustomSupportIncident : NonPersistentBusinessObject
	{
		public ZBool IsChecked
		{
			get => isChecked;
			set
			{
				SetNonPersistentPropertyValue(IsCheckedInfo, ref isChecked, value);
			}
		}
		ZBool isChecked;

		public virtual ZPropertyInfo IsCheckedInfo
		{
			get { return GetZPropertyInfo(nameof(IsChecked)); }
		}

		public ZString ClientCode { get; private set; }
		public ZString ContactName { get; private set; }
		public ZString IncidentNumber { get; private set; }
		public ZString Description { get; private set; }
		public ZString Product { get; private set; }
		public ZString ProductArea { get; private set; }
		public ZString Priority { get; private set; }
		public SupportIncidentEConversation EConversation { get; private set; }

		public static class Schema
		{
			public const string TableName = "SupportIncident";
		}

		public CustomSupportIncident(SupportIncident incident)
			: base()
		{
			PopulateProperties(incident);
		}

		public CustomSupportIncident(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		void PopulateProperties(SupportIncident incident)
		{
			if (incident != null)
			{
				ClientCode = incident.Client != null ? incident.Client.OH_Code : ZString.Empty;
				ContactName = incident.Contact != null ? incident.Contact.OC_ContactName : ZString.Empty;
				IncidentNumber = incident.IM_IncidentNumber;
				Description = incident.IM_Description;
				Product = incident.IM_Product;
				ProductArea = incident.ProductArea;
				Priority = incident.IM_Priority;
				EConversation = incident.EConversation;
			}
		}
	}
}
