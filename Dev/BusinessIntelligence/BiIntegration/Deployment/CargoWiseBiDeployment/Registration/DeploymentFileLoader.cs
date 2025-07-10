namespace CargoWise.Bi.Registration
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using CargoWise.Bi.Registration.PowerBi;
	using CargoWise.Common;

	public class DeploymentFileLoader
	{
		public PowerBiReportCollection ReportCollection
		{
			get
			{
				return reportCollection = reportCollection ?? new PowerBiReportCollection();
			}
		}

		PowerBiReportCollection reportCollection;

		public IEnumerable<string> GetSsasResourceFileNames()
		{
			var thisType = typeof(DeploymentFileLoader);
			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.{1}..+.bim", thisType.Namespace, SsasFileResourceFolder), RegexOptions.IgnoreCase);
			return thisType.Assembly.GetManifestResourceNames().Where(r => regex.Match(r).Success);
		}

		public IEnumerable<PowerBiItem> GetPowerBiReports(BiReportCategory biReportCategory)
		{
			return ReportCollection.GetFilteredPowerBiReports(biReportCategory);
		}

		public PowerBiItem GetPowerBiReport(string reportName)
		{
			return GetPowerBiReports(BiReportCategory.All).FirstOrDefault(x => x.Name.Equals(reportName));
		}

		public IEnumerable<PowerBiItem> GetAllPowerBiReports()
		{
			return PowerBiReportCollection.GetAllPowerBiReportsFromAssembly();
		}

		public IEnumerable<string> GetSsrsReportResourceFileNames()
		{
			var thisType = this.GetType();
			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.{1}..+.rdl", thisType.Namespace, SsrsFileResourceFolder), RegexOptions.IgnoreCase);
			return thisType.Assembly.GetManifestResourceNames().Where(r => regex.Match(r).Success);
		}

		public IEnumerable<string> GetSsrsDataSetResourceFileNames()
		{
			var thisType = this.GetType();
			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, "{0}.{1}..+.rsd", thisType.Namespace, SsrsFileResourceFolder), RegexOptions.IgnoreCase);
			return thisType.Assembly.GetManifestResourceNames().Where(r => regex.Match(r).Success);
		}

		public IEnumerable<PowerBiItem> GetWebAPIDatasets(BiReportCategory biReportCategory)
		{
			return ReportCollection.GetFilteredAPIDatasetItems(biReportCategory);
		}

		public string LoadEmbeddedResource(string resourceName)
		{
			Argument.NotNullOrEmpty(resourceName, nameof(resourceName));

			var resourceStream = typeof(DeploymentFileLoader).Assembly.GetManifestResourceStream(resourceName)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Null resource stream returned from resource name: {0}", resourceName));

			using (var reader = new StreamReader(resourceStream))
			{
				return reader.ReadToEnd();
			}
		}

		public byte[] LoadPowerBiEmbeddedResource(string resourceName)
		{
			Argument.NotNullOrEmpty(resourceName, nameof(resourceName));

			var thisType = this.GetType();
			var resourceStream = thisType.Assembly.GetManifestResourceStream(resourceName)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Null resource stream returned from resource name: {0}", resourceName));

			byte[] byteArray = new byte[resourceStream.Length];

			while ((resourceStream.Read(byteArray, 0, byteArray.Length)) > 0)
			{
			}

			return byteArray;
		}

		const string SsasFileResourceFolder = "SSAS";
		const string SsrsFileResourceFolder = "SSRS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		const string WebAPIDatasetsResourceFolder = "WebAPIDatasets";

#if DEBUG
		public string LoadReportLayoutFilesFromEmbeddedResource(string businessArea, string reportName)
		{
			reportName = reportName.Replace(' ', '_');
			reportName = FirstCharacterIsDigit(reportName) ? "_" + reportName : reportName;
			var layoutResourceName = typeof(DeploymentFileLoader).Namespace + $".Test.PowerBi.Reports.{businessArea}.{reportName}.Report.Layout"; // Namespace
			var reportLayout = LoadEmbeddedResource(layoutResourceName).Replace("\0", "");
			return reportLayout;
		}

		bool FirstCharacterIsDigit(string s)
		{
			return !string.IsNullOrEmpty(s) && char.IsDigit(s[0]);
		}
#endif
	}
}
