using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces.Snapshot;

namespace Enterprise.Customs.FR.Business.Snapshot
{
	public class ChildData : IChildData
	{
		public ChildData(ZString type, ZString code, ZString reference)
		{
			this.type = GetTypeFromString(type);
			this.code = code;
			this.reference = reference;
		}

		static ChildType GetTypeFromString(ZString type)
		{
			ChildType result = ChildType.DOC;
			switch (type)
			{
				case canaType:
					result = ChildType.CAN;
					break;
				case cacoType:
					result = ChildType.CAC;
					break;
			}

			return result;
		}

		public static ZString DOC => docType;
		public static ZString CAN => canaType;
		public static ZString CAC => cacoType;

		public ChildType Type => type;
		public string Code => code;
		public string Reference => reference;

		const string docType = "DOC";
		const string canaType = "CAN";
		const string cacoType = "CAC";
		readonly ChildType type;
		readonly string code;
		readonly string reference;
	}
}
