using System.Text;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public static class IncidentAssociationSystemStatus
	{
		public const string StmFieldName = "IncidentAssociationSystemStatus";

		public const string Initializing = "INI";
		public const string ReadyToBootstrap = "RBS";
		public const string ReadyForNewIncidents = "RNI";

		public static void DeleteAllStatusRows(BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			foreach (var item in factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationSystemStatus.StmFieldName)))
			{
				item.Delete();
			}

			factory.Save();
		}

		public static void SetStatus(string status, BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			var row = factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, StmFieldName));

			if (row == null)
			{
				row = factory.New<StmData>();
				row.SD_Name = StmFieldName;
			}

			row.SD_BinaryValue = Encoding.UTF8.GetBytes(status);

			factory.Save();
		}

		public static string GetStatus(BusinessObjectFactory factory = null)
		{
			var row = (factory ?? new BusinessObjectFactory())
				.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, StmFieldName));

			return row == null ? null : Encoding.UTF8.GetString(row.SD_BinaryValue);
		}
	}
}
