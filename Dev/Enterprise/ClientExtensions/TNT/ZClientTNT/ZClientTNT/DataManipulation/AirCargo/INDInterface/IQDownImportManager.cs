using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.AirCargo
{
	public class IQDownImportManager : ImportManager
	{
		public IQDownImportManager(BusinessObjectFactory factory, ZString filename)
			: base(factory, filename)
		{
		}

		#region AirCargos

		public IQDownAirCargoCollection AirCargos
		{
			get { return airCargos ?? (airCargos = new IQDownAirCargoCollection(Factory)); }
		}
		IQDownAirCargoCollection airCargos;

		#endregion

		#region Implementation

		protected override string ProcessedDirectory
		{
			get { return TNTDataRegistry.Instance.IQDownFileProcessedDirectory; }
		}

		internal override bool IsExtraCheckOK(string[] fileLines, NotificationBuffer buffer)
		{
			bool result = true;

			FlightRecord record = RecordFactory.NewRecord(fileLines[0], buffer) as FlightRecord;
			if (record == null || !record.IsValid(buffer))
			{
				buffer.Notify(new ErrorNotification(TNTErrorType.InvalidFileFormat, "First record is not a valid IQDown Flight Record"));
				result = false;
			}

			return result;
		}

		protected override void PreLoadSetup()
		{
			AirCargos.RemoveAndDeleteAll();
			AirCargo = null;
		}

		IQDownAirCargo AirCargo;

		protected override void ProcessRecord(IQDownBaseRecord record, INotifications notify)
		{
			if (record != null)
			{
				FlightRecord flightRec = record as FlightRecord;
				if (flightRec != null)
				{
					if (flightRec.IsValid(notify))
					{
						AirCargo = AirCargos[flightRec];
						if (AirCargo == null)
						{
							AirCargo = new IQDownAirCargo(Factory, flightRec, BranchCode);
							AirCargos.Add(AirCargo);
						}
					}
				}
				else
				{
					AddConsignmentDetails(AirCargo, record, notify);
				}
			}
		}

		void AddConsignmentDetails(IQDownAirCargo airCargo, IQDownBaseRecord record, INotifications notify)
		{
			ConsignmentRecord consignmentRec = record as ConsignmentRecord;
			if (consignmentRec != null)
			{
				if (airCargo != null && consignmentRec.IsValid(notify))
				{
					airCargo.AddConsignmentDetail(consignmentRec);
				}
			}
			else
			{
				ConsignmentNoteRecord consignmentNoteRec = record as ConsignmentNoteRecord;
				if (consignmentNoteRec != null)
				{
					if (airCargo != null && consignmentNoteRec.IsValid(notify))
					{
						airCargo.AddConsignmentNotes(consignmentNoteRec);
					}
				}
			}
		}

		#endregion
	}
}
