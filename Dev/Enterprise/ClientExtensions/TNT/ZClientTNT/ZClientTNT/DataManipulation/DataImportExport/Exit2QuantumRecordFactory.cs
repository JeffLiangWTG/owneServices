
using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class Exit2QuantumRecordFactory : QuantumRecordFactory
	{
		protected override IQDownBaseRecord GetNewRecord(ZString branchCode, ZString mBagNo, ZString line)
		{
			return new Exit2QuantumShipmentRecord(branchCode, mBagNo, line);
		}
	}
}
