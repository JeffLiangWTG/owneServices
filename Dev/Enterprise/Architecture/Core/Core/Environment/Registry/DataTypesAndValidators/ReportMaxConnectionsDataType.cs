using System;
using System.Xml;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class ReportMaxConnectionsDataType : IntRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue > 0)
			{
				var maxSecondaryProcessesCount = GetSRRMaximumSecondaryProcessesCount();
				if (maxSecondaryProcessesCount.HasValue && proposedValue <= maxSecondaryProcessesCount)
				{
					throw new RegistryValidationException(Res.GetString("747bfe22-d191-45ff-9cdd-df44948e623e", "{0} must be higher than Maximum Count of Secondary Process of Service Schedule Task which is now set to {1}", registryItem.Caption, maxSecondaryProcessesCount));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int? GetSRRMaximumSecondaryProcessesCount()
		{
			int? result = null;
			using (var cmdScheduleTask = Db.Connection.Command(@"select [dbo].[CLRUncompressAsString](S5_ScheduleState) 
from dbo.StmScheduleTask where S5_ScheduleType=@scheduleType"))
			{
				cmdScheduleTask.AddParameterBasedOnDbColumn("@scheduleType", "SRR", StmScheduleTaskSchema.S5_ScheduleType);
				using (var reader = cmdScheduleTask.ExecuteReader())
				{
					while (reader.Read())
					{
						var scheduleStateObj = reader[0];
						if (scheduleStateObj != DBNull.Value)
						{
							var scheduleState = scheduleStateObj.ToString();
							var xmlDocument = new XmlDocument();
							xmlDocument.LoadXml(scheduleState);
							var node = xmlDocument.SelectSingleNode("/HostedServiceSerializableSettings/SecondaryProcessesMaxCount");
							if (node != null && int.TryParse(node.InnerText, out int maxCount))
							{
								if (!result.HasValue || maxCount < result.Value)
								{
									result = maxCount;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
