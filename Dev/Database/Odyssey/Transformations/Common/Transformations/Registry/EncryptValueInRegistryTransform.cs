using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class EncryptValueInRegistryTransform : RegistryDataTransformation
	{
		public override string UserDescription => "Encrypting registry values";

		public abstract string[] RegistryNamesToEncrypt { get; }

		protected override void OfflinePostUpgradeTransform()
		{
			var registries = RegistryNamesToEncrypt;

			if (registries == null || registries.Length == 0)
			{
				return;
			}

			var iv = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");

			var results = new Dictionary<string, byte[]>();
			foreach (var regName in registries)
			{
				using (var cmd = Db.Connection.Command($"select SD_BinaryValue from dbo.StmData where SD_Name = '{regName}'")) // db commands are fine in the transformations
				{
					var dataObj = cmd.ExecuteScalar();
					if (dataObj is DBNull)
					{
						continue;
					}

					var data = (byte[])dataObj;
					if (data == null || data.Length == 0)
					{
						continue;
					}

					string asString = string.Empty;
					bool needsEncryption = false;
					try
					{
						asString = Encoding.Unicode.GetString(data);
						var decrypted = PasswordHashingTransformationHelper.DecryptWithTwoWayEncoder(iv, asString);
						continue;
					}
					catch (CryptographicException ex) when (!ex.IsCriticalException())
					{
						needsEncryption = true;
					}
					catch (FormatException ex) when (!ex.IsCriticalException())
					{
						needsEncryption = true;
					}

					if (needsEncryption && !string.IsNullOrEmpty(asString))
					{
						var encoded = PasswordHashingTransformationHelper.EncryptWithTwoWayEncoder(iv, asString);
						results.Add(regName, Encoding.Unicode.GetBytes(encoded));
					}
				}
			}

			foreach (var entry in results)
			{
				UpdateDatabaseValue(entry.Key, entry.Value);
			}
		}
	}
}
