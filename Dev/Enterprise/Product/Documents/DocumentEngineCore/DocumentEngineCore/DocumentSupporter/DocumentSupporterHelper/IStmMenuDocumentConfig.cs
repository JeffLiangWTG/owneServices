using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore
{
	public interface IStmMenuDocumentConfig
	{
		ZBool S3_IsTemplate { get; set; }
		ZPropertyInfo S3_IsTemplateInfo { get; }
	}
}
