using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Schema
{
	public class ClientSpecificTableCreator
	{
		public void Create(DbConnection conn)
		{
			var extensionObjectsSource = GlobalServiceProvider.Instance.GetService<IExtensionObjectsSource>();
			var extensionObjects = extensionObjectsSource?.ExtensionObjects;
			if (extensionObjects == null)
			{
				return;
			}

			string clientDisplayName = extensionObjectsSource.DisplayName;
			string objectName = "";

			try
			{
				foreach (var tableScript in extensionObjects.TableCreationScripts)
				{
					objectName = tableScript.ObjectName;
					conn.ExecuteNonQuery(tableScript.CreateScript);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var errorMessage = string.Format(CultureInfo.InvariantCulture, "Failed to create {0} Tables\r\n{1}\r\n{2}", clientDisplayName, e.Message, objectName);
				throw new Exception(errorMessage, e);
			}
		}
	}
}
