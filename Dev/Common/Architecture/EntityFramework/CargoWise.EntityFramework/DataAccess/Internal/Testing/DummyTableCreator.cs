#if DEBUG

using System;
using System.Data;
using System.IO;
using System.Xml;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
#pragma warning disable CA1052
	public class DummyTableCreator
#pragma warning restore CA1052
	{
		#region Create Dummy Table For Multiple DB support testing

		/// <summary>
		/// This method is duplicated here for testing purposes.
		/// It is originally on DbUpgrader solution.
		/// Should be used only to create DummyTable in another database.
		/// </summary>
		internal static void CreateDummyTableInAnotherDb(string dbName)
		{
			string dbNamePrefix = dbName + ".dbo.";
			string sqlText = String.Format(GetScriptFromExtraDevelopmentObjectFile("DummyBizo"), dbNamePrefix);
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		/// <summary>
		/// This method is duplicated here for testing purposes.
		/// It is originally on DbUpgrader solution.
		/// </summary>
		protected static string GetScriptFromExtraDevelopmentObjectFile(string scriptName)
		{
			scriptName = scriptName.ToLower();
			XmlDocument document = new XmlDocument();
			document.LoadXml(ExtraDevelopmentObjectsXml);
			foreach (XmlNode scriptNode in document.DocumentElement.ChildNodes)
			{
				if (scriptNode.Name == scriptName)
				{
					return scriptNode.InnerText;
				}
			}

			throw new ArgumentException("Invalid Script Name: " + scriptName);
		}

		static string ExtraDevelopmentObjectsXml
		{
			get
			{
				using (Stream stream = System.Reflection.Assembly.LoadFile(Path.Combine(CargoWise.Common.AssemblyLoader.GetBinPath(), @"Enterprise.DbUpgrader.Schema.Template.dll")).
					GetManifestResourceStream("Enterprise.DbUpgrader.Schema.Template.ExtraDevelopmentObjects.xml"))
				using (StreamReader streamReader = new StreamReader(stream))
				{
					return streamReader.ReadToEnd();
				}
			}
		}

		#endregion

		public static void AddRow(Guid pK, string z0_Description, int z0_Number, int z0_AnotherNumber, byte[] z0_VarBinaryMax)
		{
			AddRow(null, pK, z0_Description, z0_Number, z0_AnotherNumber, z0_VarBinaryMax);
		}

		public static void AddRow(string dBName, Guid pK, string z0_Description, int z0_Number, int z0_AnotherNumber, byte[] z0_VarBinaryMax)
		{
			string path;
			if (string.IsNullOrEmpty(dBName))
			{
				path = DummyBizoSchema.Constants.TableName;
			}
			else
			{
				path = dBName + ".dbo." + DummyBizoSchema.Constants.TableName;
			}

			string sql =
				@"INSERT INTO " + path + @"(Z0_PK, Z0_Description, Z0_Number, Z0_AnotherNumber, Z0_VarBinaryMax, Z0_Money, Z0_VarCharMax)
					VALUES(@PK, @Z0_Description, @Z0_Number, @Z0_AnotherNumber, @Z0_VarBinaryMax, 0, '')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
				command.AddParameterBasedOnDbColumn("@Z0_Description", z0_Description, DummyBizoSchema.Z0_Description);
				command.AddParameterBasedOnDbColumn("@Z0_Number", z0_Number, DummyBizoSchema.Z0_Number);
				command.AddParameterBasedOnDbColumn("@Z0_AnotherNumber", z0_AnotherNumber, DummyBizoSchema.Z0_AnotherNumber);
				command.AddParameterBasedOnDbColumn("@Z0_VarBinaryMax", z0_VarBinaryMax, DummyBizoSchema.Z0_VarBinaryMax);
				command.ExecuteNonQuery();
			}
		}

		public static void AddDummyBusinessObjectsToDB(Guid pK1, Guid pK2)
		{
			AddDummyBusinessObjectsToDB(null, pK1, pK2);
		}

		public static void AddDummyBusinessObjectsToDB(string dBName, Guid pK1, Guid pK2)
		{
			AddRow(dBName, pK1, DummyDescription1, DummyNumber1, 10, DummyByteArray1Compressed);
			AddRow(dBName, pK2, DummyDescription2, DummyNumber2, 200, DummyByteArray2Compressed);
		}

		public const string DummyDescription1 = "COMRADE";
		public const string DummyDescription2 = "NOODLE";

		public const int DummyNumber1 = 5;
		public const int DummyNumber2 = 100;

		public static byte[] DummyByteArray1
		{
			get { return ZBlob.FromUTF8(ZString.Replicate('A', 1000)); }
		}

		public static byte[] DummyByteArray2
		{
			get { return ZBlob.FromUTF8(ZString.Replicate('B', 1000)); }
		}

		public static byte[] DummyByteArray1Compressed
		{
			get { return (byte[])ZCompressor.GetCompressedVersion(DummyByteArray1, ""); }
		}

		public static byte[] DummyByteArray2Compressed
		{
			get { return (byte[])ZCompressor.GetCompressedVersion(DummyByteArray2, ""); }
		}
	}
}

#endif
