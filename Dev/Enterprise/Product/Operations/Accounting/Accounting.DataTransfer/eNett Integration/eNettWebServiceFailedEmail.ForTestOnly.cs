#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	public partial class eNettWebServiceFailedEmail
	{
		public string GetSubject_ForTestOnly()
		{
			return GetSubject();
		}

		public BusinessObjectFactory Factory_ForTestOnly => Factory;

		public string GetContent_ForTestOnly() => GetContent();
	}
}

#endif
