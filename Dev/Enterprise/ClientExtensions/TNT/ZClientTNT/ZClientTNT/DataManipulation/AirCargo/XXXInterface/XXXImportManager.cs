using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.AirCargo
{
	public class XXXImportManager : ImportManager
	{
		public XXXImportManager(BusinessObjectFactory factory, ZString filename)
			: base(factory, filename)
		{
		}

		#region MasterAirCargo

		public XXXMasterAirCargo MasterAirCargo
		{
			get
			{
				if (fMasterAirCargo == null)
				{
					fMasterAirCargo = new XXXMasterAirCargo(Factory);
				}

				return fMasterAirCargo;
			}
		}

		XXXMasterAirCargo fMasterAirCargo;

		#endregion

		public ReadOnlyCusMAWBCollection MatchingMasterbills
		{
			get { return MasterAirCargo.MatchingMasterbills; }
		}

		public XXXAirCargoCollection AirCargos
		{
			get { return MasterAirCargo.AirCargos; }
		}

		#region Implementation

		protected override string ProcessedDirectory
		{
			get { return TNTDataRegistry.Instance.XXXFileProcessedDirectory; }
		}

		internal override bool IsExtraCheckOK(string[] fileLines, NotificationBuffer buffer)
		{
			bool result = true;

			FlightRecord record = RecordFactory.NewRecord(fileLines[0], buffer) as FlightRecord;
			if (record == null || !record.IsValid(buffer))
			{
				buffer.Notify(new ErrorNotification(TNTErrorType.InvalidFileFormat, "First record is not a valid XXX Flight Record"));
				result = false;
			}

			return result;
		}

		protected override void PreLoadSetup()
		{
			MasterAirCargo.AirCargos.RemoveAndDeleteAll();
			AirCargo = null;
			FlightRec = null;
		}

		XXXAirCargo AirCargo;
		FlightRecord FlightRec;

		protected override void ProcessRecord(IQDownBaseRecord record, INotifications notify)
		{
			if (record != null)
			{
				FlightRecord flightRec = record as FlightRecord;
				if (flightRec != null)
				{
					if (flightRec.IsValid(notify))
					{
						this.FlightRec = flightRec;
					}
				}
				else
				{
					ConsignmentRecord consignment = record as ConsignmentRecord;
					if (consignment != null)
					{
						if (consignment.IsValid(notify))
						{
							AirCargo = MasterAirCargo.AirCargos[consignment];
							if (AirCargo == null)
							{
								AirCargo = new XXXAirCargo(Factory, this.FlightRec, consignment, BranchCode);
								MasterAirCargo.AirCargos.Add(AirCargo);
							}
						}
					}
					else
					{
						ConsignmentNoteRecord consignmentNote = record as ConsignmentNoteRecord;
						if (consignmentNote != null)
						{
							if (AirCargo != null && consignmentNote.IsValid(notify))
							{
								AirCargo.AddConsignmentNotes(consignmentNote);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
