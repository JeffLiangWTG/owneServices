using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Messaging.GUI
{
	public partial class CommunicationModeMigratorDataModel : NonPersistentBusinessObject
	{
		public CommunicationModeMigratorDataModel(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CommunicationModeMigratorDataModel(BusinessObjectFactory factory, ZGuid selected) : base(factory)
		{
			ECP_PK = selected;
		}

		[List("Lookups.Parties")]
		[ResourceStringData("ECP_PK", Caption = "EDI Client Config", ShortCaption = "EDI Client")]
		public ZGuid ECP_PK { get; set; }

		public bool CloseOnCompletion { get; set; }

		#region Lookups

		public CommunicationModeMigratorDataModelLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual CommunicationModeMigratorDataModelLookups GetNewLookups()
		{
			return new CommunicationModeMigratorDataModelLookups(this);
		}

		CommunicationModeMigratorDataModelLookups fLookups;

		#endregion
	}
}
