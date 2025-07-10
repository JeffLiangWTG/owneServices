using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IStatement
	{
		ZString StatementText { get; }  // HDR-AI-STMT-TXT and ITEM-AI-STMT-TXT
		ZString Statement { get; }  // HDR-AI-STMT and ITEM-AI-STMT
		ZString ExportFromCountry { get; } // for NCTS special mentions
		ZBool ExportFromEC { get; }  // for NCTS special mentions
	}
}
