using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Moq.Language;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert.Testing
{
	internal static class MockExtensions
	{
		public static void SetupSequence(this Mock<IDataReader> mock, DataTable dataTable)
		{
			var columns = dataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
			var readSequence = mock.SetupSequence(reader => reader.Read());
			var columnSequences = new Dictionary<string, ISetupSequentialResult<object>>();
			foreach (var column in columns)
			{
				columnSequences[column] = mock.SetupSequence(reader => reader[column]);
			}

			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				var row = dataTable.Rows[i];
				foreach (var column in columns)
				{
					columnSequences[column].Returns(row[column]);
				}

				readSequence.Returns(true);
			}

			readSequence.Returns(false);
		}
	}
}
