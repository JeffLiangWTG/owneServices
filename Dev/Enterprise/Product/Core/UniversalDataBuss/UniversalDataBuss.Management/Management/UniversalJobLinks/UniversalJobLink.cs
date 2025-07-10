using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	class UniversalJobLink : IUniversalJobLink
	{
		public UniversalJobLink(IStmUniversalJobLink link)
		{
			EnterpriseCode = link.UCL_EnterpriseCode;
			ServerCode = link.UCL_ServerCode;
			CompanyCode = link.UCL_CompanyCode;
			Enum.TryParse(link.UCL_SourceType, out DataContextType context);
			Context = context;
			Key = link.UCL_SourceKey;
		}
		public ZString EnterpriseCode { get; private set; }

		public ZString ServerCode { get; private set; }

		public ZString CompanyCode { get; private set; }

		public DataContextType Context { get; private set; }

		public ZString Key { get; private set; }
	}
}
