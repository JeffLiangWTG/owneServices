using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public interface IOrgCodeGenerator
	{
		string Generate(IOrgCodeInfo info, BusinessObjectFactory factory);
	}
}
