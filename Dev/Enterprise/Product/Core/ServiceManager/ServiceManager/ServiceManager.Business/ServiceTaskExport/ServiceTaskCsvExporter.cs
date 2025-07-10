using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskCsvExporter : IServiceTaskCsvExporter
	{
		public ServiceTaskCsvExporter(IHostedServiceAttributeProvider serviceAttributeProvider)
		{
			this.serviceAttributeProvider = serviceAttributeProvider;
		}

		public void WriteTo(Stream outputStream)
		{
			WriteLine(outputStream, headers);

			serviceAttributeProvider
				.GetHostedServiceAttributes()
				.OrderBy(o => o.Category)
				.ThenBy(o => o.Code)
				.Select(o => new[]
				{
					o.Category,
					ServiceTaskCategoryDescriptors.Get(o.Category)?.Description,
					o.Code,
					o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup ? string.Empty : o.MutuallyExclusiveTaskGroup.ToString(),
					ConvertToYesNo(o.AllowsMultipleInstances),
					ConvertToYesNo(o.IsScheduleReadOnly),
					o.Description,
				})
				.ToList()
				.ForEach(o => WriteLine(outputStream, o));
			
			static void WriteLine(Stream outputStream, IEnumerable<string> columnValues)
			{
				var row = string.Join(Delimiter, columnValues.Select(SanitizeValue)) + System.Environment.NewLine;

				var rowBytes = Encoding.UTF8.GetBytes(row);
				outputStream.Write(rowBytes, 0, rowBytes.Length);
			}

			static string ConvertToYesNo(bool value)
			{
				return value ? "Yes" : "No";
			}

			static string SanitizeValue(string value)
			{
				value ??= string.Empty;

				return $"\"{value.Replace("\"", "\"\"")}\"";
			}
		}

		readonly string[] headers =
		{
			"Product Area",
			"Product Area Description",
			"Code",
			"Mutually Exclusive Group",
			"Allow Multiple",
			"Schedule Readonly",
			"Description",
		};

		const string Delimiter = ",";

		readonly IHostedServiceAttributeProvider serviceAttributeProvider;
	}
}
