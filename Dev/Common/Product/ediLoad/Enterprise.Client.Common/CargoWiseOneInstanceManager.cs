using System;
using System.Globalization;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace Enterprise.Client.Common
{
	public static class CargoWiseOneInstanceManager
	{
		public static void CreateInCurrentSchemaWithLoginIfRequired(Configuration configuration)
		{
			Argument.NotNull(configuration, nameof(configuration));

			try
			{
				configuration.Services.CargoWiseOneInstanceClass.CreateInCurrentSchema();
			}
			catch (UnauthorizedAccessException ex)
			{
				if (configuration.UILevel == UILevel.AutomatedWithNoUI)
				{
					throw;
				}
				Exception lastException = ex;
				bool ok;
				do
				{
					using (var form = new DomainLoginForm(lastException.Message))
					{
						if (configuration.Services.MessageBox.ShowDialog(form) == System.Windows.Forms.DialogResult.Cancel)
						{
							throw;
						}
						try
						{
							configuration.Services.CargoWiseOneInstanceClass.CreateInCurrentSchema(form.Username, form.Password);
							ok = true;
						}
						catch (Exception ex2) when (!ex2.IsCriticalException())
						{
							lastException = ex2;
							ok = false;
						}
					}
				}
				while (!ok);
			}
		}

		public static string SchemaModificationWarning
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "To register {0} instances on your domain, your Active Directory schema will be modified. One schema class (cargoWiseOne-Instance) and two schema properties (cargoWiseOne-ServerName, cargoWiseOne-DatabaseName) will be added.", BrandingFactory.Instance.ProductName);
			}
		}
	}
}
