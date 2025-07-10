using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public class TransactionTypeConverter : EnumConverter<TransactionType>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"JNL", "TRF", "CTR", "PAY", "REC",
				"EXX", "OVP", "DSC", "OPY", "ORC",
				"DPY", "DRC", "GJL", "RJL", "AJL",
				"INV", "CRD", "ADJ", "JRJ",	"WIP",
				"ACR", "REV", "CST", "NJL"
			};
		}

		protected override TransactionType[] GetEnumValues()
		{
			return new TransactionType[]
			{
				TransactionType.JNL,
				TransactionType.TRF,
				TransactionType.CTR,
				TransactionType.PAY,
				TransactionType.REC,
				TransactionType.EXX,
				TransactionType.OVP,
				TransactionType.DSC,
				TransactionType.OPY,
				TransactionType.ORC,
				TransactionType.DPY,
				TransactionType.DRC,
				TransactionType.GJL,
				TransactionType.RJL,
				TransactionType.AJL,
				TransactionType.INV,
				TransactionType.CRD,
				TransactionType.ADJ,
				TransactionType.JRJ,
				TransactionType.WIP,
				TransactionType.ACR,
				TransactionType.REV,
				TransactionType.CST,
				TransactionType.NJL
			};
		}
	}
}
