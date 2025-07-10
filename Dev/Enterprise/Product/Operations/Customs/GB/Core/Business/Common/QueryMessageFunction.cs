using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business
{
	public abstract class QueryMessageFunction : CusdecMessageFunction
	{
		public string MenuCaption
		{
			get
			{
				if (FunctionCode == SubFunction)
				{
					return string.Format("{0} ({1})", FunctionCode, FunctionHuman);
				}
				else
				{
					return string.Format("{0} ({1} - {2}UCR level)", FunctionCode, FunctionHuman, SubFunction);
				}
			}
		}

		public static class LevelsOfMessage
		{
			public static string DeclarationLevel = "D";
			public static string MasterLevel = "M";
		}

		public abstract string FunctionCode { get; }
		public abstract string FunctionHuman { get; }
		public abstract string AsgCode { get; }

		public virtual string SubFunction
		{
			get { return FunctionCode; }
		}
	}

	public class Interrogate_Dem : QueryMessageFunction
	{
		public const string FunctionCodeConst = "DEM";
		public string MovementNumber { get; private set; }

		public Interrogate_Dem(string movementNumber)
		{
			MovementNumber = movementNumber;
		}

		public Interrogate_Dem()
		{
		}

		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Display export movement"; }
		}
		public override string AsgCode
		{
			get { return "781"; }
		}
	}

	public class Interrogate_DevDucr : QueryMessageFunction
	{
		public const string FunctionCodeConst = "DEV";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Display entry version"; }
		}
		public override string AsgCode
		{
			get { return "780"; }
		}

		public override string SubFunction
		{
			get { return LevelsOfMessage.DeclarationLevel; }
		}
	}

	public class Interrogate_Des : QueryMessageFunction
	{
		public const string FunctionCodeConst = "DES";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Display Entry Status"; }
		}
		public override string AsgCode
		{
			get { return "804"; }
		}

		public override string SubFunction
		{
			get { return LevelsOfMessage.DeclarationLevel; }
		}
	}

	public class Interrogate_Req : QueryMessageFunction
	{
		public const string FunctionCodeConst = "REQ";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Reply to HMRC query N6/S6"; }
		}
		public override string AsgCode
		{
			get { return "761"; }
		}
	}

	public class Interrogate_EAD : QueryMessageFunction
	{
		public const string FunctionCodeConst = "ACD";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Request EAD"; }
		}
		public override string AsgCode
		{
			get { return "786"; }
		}
	}

	public class Interrogate_Lem : QueryMessageFunction
	{
		public const string FunctionCodeConst = "LEM";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "List export movements"; }
		}
		public override string AsgCode
		{
			get { return "783"; }
		}
	}

	public class Interrogate_DecMucr : QueryMessageFunction
	{
		public const string FunctionCodeConst = "DEC";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Display entry consignment"; }
		}
		public override string AsgCode
		{
			get { return "782"; }
		}
		public override string SubFunction
		{
			get { return LevelsOfMessage.MasterLevel; }
		}
	}

	public class Interrogate_DecDucr : QueryMessageFunction
	{
		public const string FunctionCodeConst = "DEC";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Display entry consignment"; }
		}
		public override string AsgCode
		{
			get { return "782"; }
		}
		public override string SubFunction
		{
			get { return LevelsOfMessage.DeclarationLevel; }
		}
	}

	public class Interrogate_DLU : QueryMessageFunction
	{
		public const string FunctionCodeConst = "DLU";
		public override string FunctionCode
		{
			get { return FunctionCodeConst; }
		}
		public override string FunctionHuman
		{
			get { return "Display licence usage"; }
		}
		public override string AsgCode
		{
			get { return "784"; }
		}
		public override string SubFunction
		{
			get { return LevelsOfMessage.DeclarationLevel; }  // Not used
		}
	}
}
