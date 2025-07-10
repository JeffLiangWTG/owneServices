using System;
using System.Collections;
using System.Data;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Client.AUS.Products
{
	public class ClientAUSProductInterfaceMediator
	{
		public ClientAUSProductInterfaceMediator(ZGuid importerPK, ZGuid supplierPK)
		{
			this.ImporterPK = importerPK;
			this.SupplierPK = supplierPK;
		}

		#region DataFields PartsData

		public class ClientAUSProductInterfaceData
		{
			public ZGuid PK;
			public ZGuid ImporterPK;
			public ZGuid SupplierPK;
			public ZString OriginalProductCode;
			public ZString OriginalProductDesc;
			public ZString OriginalImportClassification;
			public ZGuid ProductPK;
			public ZDateTime ImportDateTime;
			public ZString NewProductCode;
			public ZString NewProductDesc;
			public ZString NewImportClassification;
			public ZDateTime ClosedDateTime;
			public ZDateTime ModifiedDateTime;
			public Char Indicator;
			public ZString Status;
		}

		public class ProductInterfaceDetails
		{
			public ZGuid InterfacePK;
			public ZGuid PartPK;
		}

		public class ProductInterfaceData
		{
			public ZString ProductCode;
			public ZString ProductDesc;
			public ZString ImportClassification;
			public ZString Status;
		}

		#endregion

		#region Interface Methods

		public bool InterfaceRecordExists(ZGuid interfacePK)
		{
			string sqlText = @"SELECT count(*) FROM ClientAUSProductInterface WHERE T5_PK = @InterfacePK";

			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@InterfacePK", SqlDbType.UniqueIdentifier, interfacePK.ToGuid());
			int count = ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar());

			return (count > 0);
		}

		public bool InterfaceRecordExistsForPart(ZGuid partPK)
		{
			string sqlText = @"SELECT count(*) FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK and T5_OP = @PartPK";

			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, ImporterPK.ToGuid());
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, SupplierPK.ToGuid());
			command.AddParameter("@PartPK", SqlDbType.UniqueIdentifier, partPK.ToGuid());
			int count = ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar());

			return (count > 0);
		}

		public bool CreateExportFile()
		{
			string sqlText = @"SELECT count(*) FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK";

			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, ImporterPK.ToGuid());
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, SupplierPK.ToGuid());
			int count = ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar());

			return (count > 0);
		}

		public ProductInterfaceDetails[] GetAllProductKeysForThisImporterAndSupplier()
		{
			ProductInterfaceDetails[] interfaceData = Array.Empty<ProductInterfaceDetails>();
			DbCommand command = GetAllProductKeysSQLCommand();
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ArrayList newInterfaceDetailsList = new ArrayList(interfaceData)
					{
						GetInterfaceDetails(reader)
					};
					interfaceData = (ProductInterfaceDetails[])newInterfaceDetailsList.ToArray(typeof(ProductInterfaceDetails));
				}
			}

			return interfaceData;
		}

		public bool CheckChangedDetails(ZGuid interfacePK)
		{
			string sqlText = @"SELECT count(*) FROM ClientAUSProductInterface WHERE T5_PK = @InterfacePK
								AND (	T5_NewProductCode <> T5_OriginalProductCode
									 OR T5_NewProductDesc <> T5_OriginalProductDesc
									 OR	T5_NewClass <> T5_OriginalClass )
								AND T5_Indicator = 'I'";

			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@InterfacePK", SqlDbType.UniqueIdentifier, interfacePK.ToGuid());
			int count = ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar());

			return (count > 0);
		}

		public ProductInterfaceData[] GetAllProductsForThisImporterAndSupplier(bool includeManuallyAddedParts)
		{
			ProductInterfaceData[] productData = Array.Empty<ProductInterfaceData>();
			DbCommand command = GetAllProductsSQLCommand(includeManuallyAddedParts);
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ArrayList allProductsList = new ArrayList(productData)
					{
						GetProductData(reader)
					};
					productData = (ProductInterfaceData[])allProductsList.ToArray(typeof(ProductInterfaceData));
				}
			}

			return productData;
		}

		public ProductInterfaceData[] GetClientInvoicingForThisImporterAndSupplier()
		{
			ProductInterfaceData[] productData = Array.Empty<ProductInterfaceData>();
			DbCommand command = GetClientInvoicingSQLCommand();
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ArrayList allProductsList = new ArrayList(productData)
					{
						GetProductData(reader)
					};
					productData = (ProductInterfaceData[])allProductsList.ToArray(typeof(ProductInterfaceData));
				}
			}

			return productData;
		}

		public ProductInterfaceData[] GetClientUpdateDataForThisImporterAndSupplier(bool includeManuallyAddedParts)
		{
			ProductInterfaceData[] productData = Array.Empty<ProductInterfaceData>();
			DbCommand command = GetClientDataUpdateSQLCommand(includeManuallyAddedParts);
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ArrayList allProductsList = new ArrayList(productData)
					{
						GetProductData(reader)
					};
					productData = (ProductInterfaceData[])allProductsList.ToArray(typeof(ProductInterfaceData));
				}
			}

			return productData;
		}

		public ClientAUSProductInterfaceData[] GetAllProductsForBackUpForThisImporterAndSupplier()
		{
			ClientAUSProductInterfaceData[] backupProductData = Array.Empty<ClientAUSProductInterfaceData>();
			DbCommand command = GetBackUpDataExtractSQLCommand();
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ArrayList backupProductsList = new ArrayList(backupProductData)
					{
						GetBackupData(reader)
					};
					backupProductData = (ClientAUSProductInterfaceData[])backupProductsList.ToArray(typeof(ClientAUSProductInterfaceData));
				}
			}

			return backupProductData;
		}

		public ZDateTime GetHighWaterMarkForDataExportForThisImporterAndSupplier()
		{
			string sqlText = @"SELECT TOP 1 T5_ClosedDateTime FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK ORDER BY T5_ClosedDateTime DESC";
			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, ImporterPK.ToGuid());
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, SupplierPK.ToGuid());
			ZDateTime result = ZArchitecture.Core.Utilities.ConvertToDateTime(command.ExecuteScalar());

			if (!result.IsValidSmallDateTime)
			{
				sqlText = @"SELECT TOP 1 T5_ImportDateTime FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK ORDER BY T5_ImportDateTime DESC";
				command = ConnectToDB(sqlText);
				command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, ImporterPK.ToGuid());
				command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, SupplierPK.ToGuid());
				result = ZArchitecture.Core.Utilities.ConvertToDateTime(command.ExecuteScalar());
			}

			return result;
		}

		#endregion

		#region Interface Table Update Methods

		public void InsertRow(ClientAUSProductInterfaceData data)
		{
			string sqlText = @"
				INSERT ClientAUSProductInterface 
					(T5_OH_Importer,
					 T5_OH_Supplier,
					 T5_OriginalProductCode,
					 T5_OriginalProductDesc,
					 T5_OriginalClass,
					 T5_OP,
					 T5_ImportDateTime,
					 T5_NewProductCode,
					 T5_NewProductDesc,
					 T5_NewClass,
					 T5_ClosedDateTime,
					 T5_ModifiedDateTime,
					 T5_Indicator,
					 T5_Status)
					VALUES (@ImporterPK, @SupplierPK, @OriginalProductCode, @OriginalProductDesc, @OriginalClass, @PartPK, @ImportDateTime, @NewProductCode, @NewProductDesc, @NewClass, @ClosedDateTime, @ModifiedDateTime, @Indicator, @Status)";

			DbCommand command = GetImporterSupplierCommand(sqlText);
			command.AddParameter("@OriginalProductCode", SqlDbType.VarChar, 30, data.OriginalProductCode.ToString());
			command.AddParameter("@OriginalProductDesc", SqlDbType.VarChar, 80, data.OriginalProductDesc.ToString());
			command.AddParameter("@OriginalClass", SqlDbType.VarChar, 35, data.OriginalImportClassification.ToString());
			command.AddParameter("@PartPK", SqlDbType.UniqueIdentifier, data.ProductPK.ToGuid());
			if (data.ImportDateTime.IsEmpty)
			{
				command.AddParameter("@ImportDateTime", SqlDbType.DateTime, DBNull.Value);
			}
			else
			{
				command.AddParameter("@ImportDateTime", SqlDbType.DateTime, data.ImportDateTime.ToDateTime());
			}

			command.AddParameter("@NewProductCode", SqlDbType.VarChar, 30, data.NewProductCode.ToString());
			command.AddParameter("@NewProductDesc", SqlDbType.VarChar, 80, data.NewProductDesc.ToString());
			command.AddParameter("@NewClass", SqlDbType.VarChar, 35, data.NewImportClassification.ToString());
			if (data.ClosedDateTime.IsEmpty)
			{
				command.AddParameter("@ClosedDateTime", SqlDbType.DateTime, DBNull.Value);
			}
			else
			{
				command.AddParameter("@ClosedDateTime", SqlDbType.DateTime, data.ClosedDateTime.ToDateTime());
			}

			if (data.ModifiedDateTime.IsEmpty)
			{
				command.AddParameter("@ModifiedDateTime", SqlDbType.DateTime, DBNull.Value);
			}
			else
			{
				command.AddParameter("@ModifiedDateTime", SqlDbType.DateTime, data.ModifiedDateTime.ToDateTime());
			}

			command.AddParameter("@Indicator", SqlDbType.Char, data.Indicator);
			command.AddParameter("@Status", SqlDbType.VarChar, 25, data.Status.ToString());

			command.ExecuteNonQuery();
		}

		public void UpdateRowWithCurrentDetails(ClientAUSProductInterfaceData data, ZGuid interfacePK)
		{
			string sqlText = @"
				UPDATE ClientAUSProductInterface SET T5_NewProductCode = @NewProductCode, T5_NewProductDesc = @NewProductDesc, T5_NewClass = @NewClass
					WHERE T5_PK = @InterfacePK";

			DbCommand command = ConnectToDB(sqlText);

			command.AddParameter("@NewProductCode", SqlDbType.VarChar, 30, data.NewProductCode.ToString());
			command.AddParameter("@NewProductDesc", SqlDbType.VarChar, 80, data.NewProductDesc.ToString());
			command.AddParameter("@NewClass", SqlDbType.VarChar, 35, data.NewImportClassification.ToString());
			command.AddParameter("@InterfacePK", SqlDbType.UniqueIdentifier, interfacePK.ToGuid());

			command.ExecuteNonQuery();
		}

		public void UpdateRowStatus(ZGuid interfacePK, char indicator, string status)
		{
			string sqlText = @"UPDATE ClientAUSProductInterface SET T5_Indicator = @Indicator, T5_Status = @Status WHERE T5_PK = @InterfacePK";

			DbCommand command = ConnectToDB(sqlText);

			command.AddParameter("@Indicator", SqlDbType.Char, indicator);
			command.AddParameter("@Status", SqlDbType.VarChar, 25, status);
			command.AddParameter("@InterfacePK", SqlDbType.UniqueIdentifier, interfacePK.ToGuid());

			command.ExecuteNonQuery();
		}

		public void DeletePartFromProductInterface(ZGuid interfacePK)
		{
			string sqlText = @"DELETE FROM ClientAUSProductInterface WHERE T5_PK = @InterfacePK";

			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@InterfacePK", SqlDbType.UniqueIdentifier, interfacePK.ToGuid());

			command.ExecuteNonQuery();
		}

		public void UpdateProductAsClosed(ZGuid interfacePK, ZDateTime closedDateTime)
		{
			string sqlText = @"UPDATE ClientAUSProductInterface SET T5_Indicator = 'C', T5_Status = 'Closed', T5_ClosedDateTime = @ClosedDateTime
								 WHERE T5_PK = @InterfacePK";

			DbCommand command = ConnectToDB(sqlText);

			command.AddParameter("@InterfacePK", SqlDbType.UniqueIdentifier, interfacePK.ToGuid());
			command.AddParameter("@ClosedDateTime", SqlDbType.DateTime, closedDateTime.ToDateTime());

			command.ExecuteNonQuery();
		}

		#endregion

		#region Transaction Processing

		public ITransactionManager BeginTransactionWithManager()
		{
			return Db.Connection.BeginTransactionWithManager();
		}

		#endregion

		#region Implementation

		protected ProductInterfaceDetails GetInterfaceDetails(IDataReader reader)
		{
			ProductInterfaceDetails interfaceData = new ProductInterfaceDetails()
			{
				InterfacePK = (Guid)reader[0],  //T5_PK
				PartPK = (Guid)reader[1]     //T5_OP
			};
			return interfaceData;
		}

		protected ProductInterfaceData GetProductData(IDataReader reader)
		{
			ProductInterfaceData productData = new ProductInterfaceData()
			{
				ProductCode = (string)reader[0],    //T5_NewProductCode
				ProductDesc = (string)reader[1],    //T5_NewProductDesc
				ImportClassification = (string)reader[2],   //T5_NewClass
				Status = (string)reader[3]     //T5_Status
			};
			return productData;
		}

		protected ClientAUSProductInterfaceData GetBackupData(IDataReader reader)
		{
			ClientAUSProductInterfaceData backupData = new ClientAUSProductInterfaceData()
			{
				PK = (Guid)reader[0],
				ImporterPK = (Guid)reader[1],
				SupplierPK = (Guid)reader[2],
				OriginalProductCode = (string)reader[3],
				OriginalProductDesc = (string)reader[4],
				OriginalImportClassification = (string)reader[5],
				ProductPK = (Guid)reader[6]
			};
			if (reader[7] != DBNull.Value)
			{
				backupData.ImportDateTime = (DateTime)reader[7];
			}
			else
			{
				backupData.ImportDateTime = ZDateTime.Empty;
			}

			backupData.NewProductCode = (string)reader[8];
			backupData.NewProductDesc = (string)reader[9];
			backupData.NewImportClassification = (string)reader[10];
			if (reader[11] != DBNull.Value)
			{
				backupData.ClosedDateTime = (DateTime)reader[11];
			}
			else
			{
				backupData.ClosedDateTime = ZDateTime.Empty;
			}

			if (reader[12] != DBNull.Value)
			{
				backupData.ModifiedDateTime = (DateTime)reader[12];
			}
			else
			{
				backupData.ModifiedDateTime = ZDateTime.Empty;
			}

			backupData.Indicator = Convert.ToChar(reader[13]);
			backupData.Status = (string)reader[14];

			return backupData;
		}

		DbCommand GetAllProductKeysSQLCommand()
		{
			return GetImporterSupplierCommand(AllProducts);
		}

		DbCommand GetAllProductsSQLCommand(bool includeManuallyAdded)
		{
			string sqlText = "";
			if (includeManuallyAdded)
			{
				sqlText = PartsImportedEditedManuallyAdded;
			}
			else
			{
				sqlText = PartsImportedEdited;
			}

			return GetImporterSupplierCommand(sqlText);
		}

		DbCommand GetClientInvoicingSQLCommand()
		{
			return GetImporterSupplierCommand(PartsImportedEdited);
		}

		DbCommand GetClientDataUpdateSQLCommand(bool includeManuallyAdded)
		{
			string sqlText = "";
			if (includeManuallyAdded)
			{
				sqlText = PartsEditedManuallyAdded;
			}
			else
			{
				sqlText = PartsEdited;
			}

			return GetImporterSupplierCommand(sqlText);
		}

		DbCommand GetBackUpDataExtractSQLCommand()
		{
			return GetImporterSupplierCommand(PartsNotYetClosed);
		}

		DbCommand GetImporterSupplierCommand(string sqlText)
		{
			DbCommand command = ConnectToDB(sqlText);
			command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, ImporterPK.ToGuid());
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, SupplierPK.ToGuid());

			return command;
		}

		DbCommand ConnectToDB(string sqlText)
		{
			return Db.Connection.Command(sqlText);
		}

		#region Query Strings

		const string AllProducts = "SELECT T5_PK, T5_OP FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK";
		readonly string PartsImportedEdited = "SELECT T5_NewProductCode, T5_NewProductDesc, T5_NewClass, T5_Status " +
			" FROM ClientAUSProductInterface WHERE (T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK)" +
			" AND (T5_Indicator = 'E' OR T5_Indicator = 'I')" +
			" ORDER BY T5_Status, T5_NewProductCode";
		readonly string PartsImportedEditedManuallyAdded = "SELECT T5_NewProductCode, T5_NewProductDesc, T5_NewClass, T5_Status " +
			" FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK" +
			" AND T5_Indicator <> 'C'" +
			" ORDER BY T5_Status, T5_NewProductCode";
		readonly string PartsEdited = "SELECT T5_NewProductCode, T5_NewProductDesc, T5_NewClass, T5_Status " +
			" FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK" +
			" AND T5_Indicator = 'E'" +
			" ORDER BY T5_Status, T5_NewProductCode";
		readonly string PartsEditedManuallyAdded = "SELECT T5_NewProductCode, T5_NewProductDesc, T5_NewClass, T5_Status " +
			" FROM ClientAUSProductInterface WHERE (T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK)" +
			" AND (T5_Indicator = 'E' OR T5_Indicator = 'M')" +
			" ORDER BY T5_Status, T5_NewProductCode";
		readonly string PartsNotYetClosed = "SELECT * FROM ClientAUSProductInterface WHERE T5_OH_Importer = @ImporterPK and T5_OH_Supplier = @SupplierPK" +
			" AND T5_Indicator <> 'C'";

		#endregion

		#endregion

		internal ZGuid ImporterPK { get; private set; }
		internal ZGuid SupplierPK { get; private set; }
	}
}
