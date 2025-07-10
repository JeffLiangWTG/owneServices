using System;
using System.IO;
using Microsoft.Win32;

namespace Enterprise.DocumentScanning.GUI
{
	public static class ChartFXLicence
	{
		public static bool CreateChartFXLicenceRegistryItemIfRequired()
		{
			bool chartFXRegistered = false;
			string chartFXRegKeyName = "864054A1-108E-4F89-B54F-842C5A886248";
			string chartFXLicence = @"TevdGftbAACX8aEnbQLjQJdjAAABAAAABUNMTjYwC1NGWERPV05MT0FEBE5TPTEBAAAAAUYkODY0MDU0QTEtMTA4RS00Rjg5LUI1NEYtODQyQzVBODg2MjQ4AF4wMDowRjpFQTo0QjpEODo4Rj8/R0JUX19fP0FXUkRBQ1BJPzc2NDg3LTY0MC0zOTQ3ODc0LTIzOTAzPzIwMDYwMTMxMTAyOTE5LjAwMDAwMCs2NjA/ODg0MTZBN0U/AAAAAA==
ejRDYItKqUTxv+8lU6f2WeO9ywOpNIyqJMQo/N9IkMfGNEEFMChQ9Iw+/To7oiAWH4QK2WHSq3taB0ikpU2aDCHgVegkbUCoLyCcUpi97LlKZCIiLYF4ShSmZHh40IbSBjuZawWAGFFF0GSsc24apdrZnV0X1qhbKyPfP3SCpFQ=";

			bool mustCreate = true;
			RegistryKey licences = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\Licenses");
			if (licences != null)
			{
				RegistryKey chartFXKey = licences.OpenSubKey(chartFXRegKeyName);
				if (chartFXKey != null)
				{
					mustCreate = false;
					string existingLicence = chartFXKey.GetValue("").ToString();
					if (existingLicence != chartFXLicence)
					{
						mustCreate = true;
					}
					else
					{
						chartFXRegistered = true;
					}
				}
			}

			if (mustCreate)
			{
				try
				{
					using (RegistryKey licencesKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Classes\\Licenses", RegistryKeyPermissionCheck.ReadWriteSubTree))
					using (RegistryKey chartFXKey = licencesKey.CreateSubKey(chartFXRegKeyName, RegistryKeyPermissionCheck.ReadWriteSubTree))
					{
						chartFXKey.SetValue("", chartFXLicence);
						chartFXRegistered = true;
					}
				}
				catch (UnauthorizedAccessException)
				{
				}	// Forms must handle ChartFX not being registered
				catch (IOException)
				{
				}	// Forms must handle ChartFX not being registered
			}
			return chartFXRegistered;
		}
	}
}
