using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccountingDocManagerInfo : DocManagerInfo, ISupportReadOnlyOverride
	{
		public AccountingDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
			fReadOnly = false;
		}

		bool fReadOnly;
		public override bool ReadOnly
		{
			get { return fReadOnly; }
		}

		void ISupportReadOnlyOverride.SetReadOnly(bool readOnly)
		{
			fReadOnly = readOnly;
		}
	}

	public interface ISupportReadOnlyOverride
	{
		void SetReadOnly(bool readOnly);
	}

	public static class DocManagerReadOnlyOverrideHelper
	{
		public static void TrySetReadOnlyOverride(IBusiness businessEntity, ODisplayMode displayMode)
		{
			IDocManagerSupport docManager = businessEntity as IDocManagerSupport;
			if (docManager != null)
			{
				ISupportReadOnlyOverride docManagerInfo = (ISupportReadOnlyOverride)docManager.DocManagerInfo;
				docManagerInfo.SetReadOnly(displayMode == ODisplayMode.ReadOnly || displayMode == ODisplayMode.Delete);
			}
		}
	}
}
