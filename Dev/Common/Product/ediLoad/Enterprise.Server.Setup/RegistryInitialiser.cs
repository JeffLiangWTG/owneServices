using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Enterprise.Server.Setup
{
	public static class RegistryInitialiser
	{
		public static void SetRegistryItems(string databaseName, SqlConnection connection, string instanceNameForLicenceKey, string dllPath, SqlTransaction transaction)
		{
			ResolveEventHandler handler = delegate(object sender, ResolveEventArgs args)
			{ return Assembly.LoadFile(Path.Combine(dllPath, new AssemblyName(args.Name).Name + ".dll")); };
			AppDomain.CurrentDomain.AssemblyResolve += handler;
			try
			{
				DoWorkInOtherAppDomain(databaseName, connection, instanceNameForLicenceKey, transaction);
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= handler;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		static void DoWorkInOtherAppDomain(string databaseName, SqlConnection connection, string instanceNameForLicenceKey, SqlTransaction transaction)
		{
#pragma warning disable CW1138 // WI00568825 - Wrongful error on Unwrap
			object dateTimeRegistryItem = Activator.CreateInstance("Enterprise.ZArchitecture.Core", "Enterprise.ZArchitecture.Environment.DateTimeRegistryDataType").Unwrap();
#pragma warning restore CW1138
			byte[] cdDateSerialised = (byte[])dateTimeRegistryItem.GetType().GetMethod("Serialise").Invoke(dateTimeRegistryItem, new object[] { DateTime.Now.AddMonths(-3) });
			SetRegistryItem(databaseName, "EnterpriseCDDate", cdDateSerialised, "DT", connection, transaction);
		}

		static void SetRegistryItem(string databaseName, string name, byte[] binaryValue, string type, SqlConnection connection, SqlTransaction transaction)
		{
#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one - think twice if you really need a new connection.
			using (SqlCommand command = new SqlCommand(@"
								UPDATE [" + databaseName + @"].dbo.StmData
																				SET SD_Type = @SD_Type, SD_BinaryValue = @SD_BinaryValue
																				WHERE SD_Name = @SD_Name  AND SD_Owner is null  AND SD_DepartmentGuid is null
																IF (@@rowcount = 0)
																BEGIN
																				INSERT [" + databaseName + @"].dbo.StmData (SD_Name , SD_Type, SD_BinaryValue)
																				VALUES (@SD_Name , @SD_Type, @SD_BinaryValue)
																END
				", connection, transaction))
#pragma warning restore CW1116
			{
				command.Parameters.AddWithValue("@SD_Name", name);
				command.Parameters.AddWithValue("@SD_Type", type);
				command.Parameters.AddWithValue("@SD_BinaryValue", binaryValue);
				command.ExecuteNonQuery();
			}
		}
	}
}
