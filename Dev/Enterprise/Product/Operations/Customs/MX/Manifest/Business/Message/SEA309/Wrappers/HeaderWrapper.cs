using System;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class HeaderWrapper : IHeader
	{
		internal HeaderWrapper(AsycudaManifestHeader header)
		{
			var company = !header.AMA_GB.IsEmpty ? header.Factory?.Load<GlbBranch>(header.AMA_GB)?.Company : null;
			wsVucem = MXCustomsDataRegistry.Instance?.WSVucem?.GetFallBackValueAtAllLevels((Guid)(company?.PK.ToGuid()), Guid.Empty, Guid.Empty);
		}
		readonly MXWsVucem wsVucem;

		string IHeader.User => wsVucem?.SeaModeWSUsername ?? ZString.Empty;

		string IHeader.Password => wsVucem?.SeaModeWSPassword ?? ZString.Empty;

		DateTime IHeader.MessageDate => MXMessageHelper.SafeDateTime(ZDateTime.Now);

		string IHeader.UniformResourceLocator => wsVucem?.SeaModeWSResponse ?? ZString.Empty;
	}
}
