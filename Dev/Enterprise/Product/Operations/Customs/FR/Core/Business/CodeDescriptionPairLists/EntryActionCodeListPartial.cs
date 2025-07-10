using CargoWise.Types;

namespace Enterprise.Customs.FR.Business
{
	public partial class EntryActionCodeList
	{
		public static ZInt GetMessageCodeNumber(ZString messageCode, bool isDeltaC = true)
		{
			ZInt codeaction = new ZInt(-1);

			if (isDeltaC)
			{
				codeaction = GetDeltaCMessageCodeNumber(messageCode);
			}
			else if (!isDeltaC)
			{
				codeaction = GetDeltaDMessageCodeNumber(messageCode);
			}

			return codeaction;
		}
		static ZInt GetDeltaDMessageCodeNumber(ZString messageCode)
		{
			var messageCodeNumber = 0;

			switch (messageCode)
			{
				case Codes.VAL:
					messageCodeNumber = 1;
					break;
				case Codes.D2M:
					messageCodeNumber = 2;
					break;
				case Codes.ANT:
					messageCodeNumber = 3;
					break;
				case Codes.MDV:
					messageCodeNumber = 4;
					break;
				case Codes.RPS:
					messageCodeNumber = 5;
					break;
				case Codes.MDA:
					messageCodeNumber = 6;
					break;
				case Codes.VAA:
					messageCodeNumber = 7;
					break;
				case Codes.ANN:
					messageCodeNumber = 8;
					break;
				case Codes.REC:
					messageCodeNumber = 9;
					break;
				case Codes.INV:
					messageCodeNumber = 10;
					break;
				default:
					messageCodeNumber = -1;
					break;
			}
			return messageCodeNumber;
		}
		static ZInt GetDeltaCMessageCodeNumber(ZString messageCode)
		{
			var messageCodeNumber = 0;

			switch (messageCode)
			{
				case Codes.ANT:
					messageCodeNumber = 1;
					break;
				case Codes.VAL:
					messageCodeNumber = 2;
					break;
				case Codes.MAP:
					messageCodeNumber = 3;
					break;
				case Codes.ANA:
					messageCodeNumber = 4;
					break;
				case Codes.VAA:
					messageCodeNumber = 5;
					break;
				case Codes.EAV:
					messageCodeNumber = 6;
					break;
				case Codes.INV:
					messageCodeNumber = 7;
					break;
				case Codes.CMP:
					messageCodeNumber = 8;
					break;
				case Codes.RPS:
					messageCodeNumber = 9;
					break;
				case Codes.REC:
					messageCodeNumber = 10;
					break;
				case Codes.VAR:
					messageCodeNumber = 11;
					break;
				case Codes.ANR:
					messageCodeNumber = 12;
					break;
				default:
					messageCodeNumber = -1;
					break;
			}
			return messageCodeNumber;
		}
	}
}
