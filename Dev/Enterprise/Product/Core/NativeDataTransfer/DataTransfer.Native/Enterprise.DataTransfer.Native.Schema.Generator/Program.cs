#pragma warning disable CW1161 // There's no need for resource strings here
#pragma warning disable CW1106 // Console logs are ok here

using CargoWise.Data;
using Enterprise.DataTransfer.Native.Business.NativeSchemaGenerator;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer.Native.Schema.Generator
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Usage: NativeSchemaGenerator.exe serverName databaseName");
				return;
			}

			try
			{
				Db.InitializeDatabaseDetails(args[0].Trim(), args[1].Trim());
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();

				var schemaCount = new NativeXMLSchemaGenerator().GenerateSchemas();
				Console.WriteLine($"Schema generation completed successfully: {schemaCount}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}
		}
	}
}

