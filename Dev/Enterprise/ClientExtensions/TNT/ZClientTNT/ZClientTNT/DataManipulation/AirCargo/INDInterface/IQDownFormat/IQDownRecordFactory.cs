using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT
{
	public class IQDownRecordFactory
	{
		public IQDownBaseRecord NewRecord(ZString line, INotifications notify)
		{
			IQDownBaseRecord result = null;

			if (line.Length > 2)
			{
				if (line[0] == '0' && line[1] == '1')
				{
					result = new FlightRecord(line);
				}
				else if (line[0] == '0' && line[1] == '2')
				{
					result = new TBagRecord(line);
				}
				else if (line[0] == '0' && line[1] == '3')
				{
					result = new ConsignmentRecord(line);
				}
				else if (line[0] == '0' && line[1] == '4')
				{
					result = new ConsignmentNoteRecord(line);
				}
				else
				{
					notify.Notify(new ErrorNotification(TNTErrorType.UnknownRecordType, "Not an IQDown record"));
				}
			}
			else
			{
				notify.Notify(new ErrorNotification(TNTErrorType.InvalidFileFormat, "Record line length too short"));
			}

			return result;
		}
	}
}
