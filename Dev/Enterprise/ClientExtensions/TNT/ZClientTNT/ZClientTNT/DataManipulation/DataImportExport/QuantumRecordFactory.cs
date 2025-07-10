
using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class QuantumRecordFactory
	{
		public IQDownBaseRecord NewRecord(ZString branchCode, ZString line)
		{
			IQDownBaseRecord result = null;
			if (line.Length > 2 && line[0] == '0' && line[1] == '1')
			{
				result = new QuantumConsolRecord(line);
			}
			else if (line.Length > 2 && line[0] == '0' && line[1] == '2')
			{
				MBagNo = line.Substring(2, 10);
			}
			else if (line.Length > 2 && line[0] == '0' && line[1] == '3')
			{
				result = GetNewRecord(branchCode, MBagNo, line);
			}
			else if (line.Length > 2 && line[0] == '0' && line[1] == '4')
			{
				result = new QuantumShipmentNotesRecord(line);
			}
			return result;
		}

		protected virtual IQDownBaseRecord GetNewRecord(ZString branchCode, ZString mBagNo, ZString line)
		{
			return QuantumShipmentRecord.New(branchCode, mBagNo, line);
		}

		ZString MBagNo;

		internal ZString TestMBagNo
		{
			get { return MBagNo; }
		}
	}
}
