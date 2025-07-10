using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public interface ISupportMultiSubAccounts
	{
		AccGLHeader GLHeader { get; }
		bool IsJobRelated { get; }
		ISupportSubAccountCollection SubAccounts { get; }
		bool IsMultiSubAccountsSupported { get; }
	}
}
