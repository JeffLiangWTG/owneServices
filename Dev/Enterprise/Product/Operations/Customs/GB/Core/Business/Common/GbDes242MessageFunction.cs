using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Common;

namespace Enterprise.Customs.GB.Business
{
	public abstract class GbDes242MessageFunction : CusdecMessageFunction
	{
		public const string Eac = "EAC";
		public virtual string AsgCode { get { return "001"; } }
		public abstract string FunctionHuman { get; }
		public virtual string FunctionCode { get { return Eac; } }
		public abstract string SubFunctionCode { get; }

		public class MucrClose : GbDes242MessageFunction
		{
			public override string SubFunctionCode
			{
				get { return GbCusDecMessageFunctionsList.Codes.Close; }
			}
			public override string FunctionHuman
			{
				get { return GbCusDecMessageFunctionsList.Descriptions.Close; }
			}
		}

		public abstract class MasterUcrWithChildUcrFunction : GbDes242MessageFunction
		{
			public ZString ChildUCRToBeAddedToMasterUCR
			{
				get => childUCRToBeAddedToMasterUCR;
				set
				{
					childUCRToBeAddedToMasterUCR = value;
					childUCRParts = childUCRToBeAddedToMasterUCR.Split("/");
				}
			}
			ZString childUCRToBeAddedToMasterUCR;
			ZString[] childUCRParts = Array.Empty<ZString>();

			public ZString ChildUCR => childUCRParts.Length > 0 ? childUCRParts[0] : ZString.Empty;

			public ZString ChildUCRPartNo => childUCRParts.Length == 2 ? childUCRParts[1] : ZString.Empty;
		}

		public class MucrAssociate : MasterUcrWithChildUcrFunction
		{
			public override string SubFunctionCode
			{
				get { return GbCusDecMessageFunctionsList.Codes.Associate; }
			}
			public override string FunctionHuman
			{
				get { return GbCusDecMessageFunctionsList.Descriptions.Associate; }
			}
		}

		public class MucrDisAssociate : MasterUcrWithChildUcrFunction
		{
			public override string SubFunctionCode
			{
				get { return GbCusDecMessageFunctionsList.Codes.Disassociate; }
			}
			public override string FunctionHuman
			{
				get { return GbCusDecMessageFunctionsList.Descriptions.Disassociate; }
			}
		}

		public class QueryMasterDEC : GbDes242MessageFunction
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
			public override string SubFunctionCode
			{
				get { return QueryMessageFunction.LevelsOfMessage.MasterLevel; }
			}
		}
	}
}
