using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Client.TNT
{
	/// <summary>
	/// Save interface details to client specific table as part of a SaveTogether (same transaction)
	/// </summary>
	internal class MawbInterfaceDetailSaver : SaveInTransactionActionWithMainConnection
	{
		public MawbInterfaceDetailSaver(QuantumMawbCollection exit2MawbsUploaded)
		{
			fExit2MawbsUploaded = exit2MawbsUploaded;
			fConsolRef = exit2MawbsUploaded[0].LinkedConsolUniqueConsignRef;
		}

		public static bool IsMawbInterfaceDetailStored(string consolRef)
		{
			string sqlText = "SELECT count(*) FROM ClientTntMawbInterface WHERE T9_ConsolRef = @ConsolRef";

			DbCommand command = Db.Connection.Command(sqlText);
			command.AddParameter("@ConsolRef", SqlDbType.VarChar, consolRef);

			int count = ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar());

			return (count > 0);
		}

		readonly QuantumMawbCollection fExit2MawbsUploaded;
		readonly string fConsolRef;

		protected override IChangedTableNames SaveInTransaction()
		{
			for (int i = 0; i < fExit2MawbsUploaded.Count; i++)
			{
				InsertOrUpdateMawbInterfaceTable(fExit2MawbsUploaded[i], fConsolRef);
			}
			return new ChangedTableNames(new[] { "ClientTntMawbInterface" });
		}

		protected void InsertOrUpdateMawbInterfaceTable(QuantumMawb mawb, string consolRef)
		{
			if (mawb.LinkedConsol != null)
			{
				string sqlText = @"
				IF EXISTS (SELECT null FROM ClientTntMawbInterface WHERE T9_MasterBill = @MasterBill and T9_ConsolRef = @ConsolRef)
					UPDATE ClientTntMawbInterface SET T9_UploadedShipments = @UploadedShipments, T9_UploadTime = getdate()
					WHERE T9_MasterBill = @MasterBill and T9_ConsolRef = @ConsolRef
				ELSE
					INSERT ClientTntMawbInterface 
				  (T9_MasterBill, T9_ConsolRef, T9_Flight, T9_Etd, T9_PortOfLoading, T9_PortOfDischarge, T9_UploadedShipments)
					VALUES (@MasterBill, @ConsolRef, @Flight, @Etd, @PortOfLoading, @PortOfDischarge, @UploadedShipments)";

				DbCommand command = Db.Connection.Command(sqlText);

				command.AddParameter("@MasterBill", SqlDbType.VarChar, 20, mawb.RawMasterBill);
				command.AddParameter("@ConsolRef", SqlDbType.VarChar, 35, consolRef);
				command.AddParameter("@Flight", SqlDbType.VarChar, 10, mawb.RawFlightNumber);
				if (string.IsNullOrWhiteSpace(mawb.RawDepartureDate))
				{
					command.AddParameter("@Etd", SqlDbType.VarChar, DBNull.Value);
				}
				else
				{
					command.AddParameter("@Etd", SqlDbType.VarChar, 20, mawb.RawDepartureDate);
				}
				command.AddParameter("@PortOfLoading", SqlDbType.VarChar, 10, mawb.RawPortOfLoading);
				command.AddParameter("@PortOfDischarge", SqlDbType.VarChar, 10, mawb.RawPortOfDischarge);
				command.AddParameter("@UploadedShipments", SqlDbType.Int, (int)mawb.RelatedShipmentsInFile);

				command.ExecuteNonQuery();
			}
		}
	}
}
