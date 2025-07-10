using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryLookups(CusReconEntry parent) : Customs.Business.CusReconEntryLookups(parent)
	{
		public KREntryHeaderDetailsViewCollection ImportEntryNumbers => Factory.GetCachedValue(Parent.CRE_OriginalEntryNumber, () => new KREntryHeaderDetailsViewCollection(Factory, KRJobMessageTypeList.Codes.Import, new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5UL(), Parent.Branch.Company.PK));
		public CodeDescriptionPairList Amendment5WNVersionNumbers
		{
			get
			{
				return Factory.GetCachedValue($"Amendment5WNVersionNumbers {Parent.FirstSnapShot?.PK ?? Parent.PK}", () =>
				{
					var result = new CodeDescriptionPairList();
					var refundAmounts = Parent.FirstSnapShot?.ImportEntryOrEntryLine?.RefundAmounts;
					if (refundAmounts != null)
					{
						foreach (var refundAmount in refundAmounts)
						{
							result.AddPair(refundAmount.VersionNumber.ToString(), refundAmount.VersionDescription);
						}
					}
					return result;
				});
			}
		}
		public KREntryCustomsBillsViewCollection CustomsBillsViewCollection
		{
			get
			{
				var companyPK = ZGuid.Empty;
				if (Parent.ReconDeclaration != null)
				{
					var branch = Factory.Load<GlbBranch>(Parent.ReconDeclaration.CRD_GB_Branch);
					companyPK = branch?.Company?.PK ?? ZGuid.Empty;
				}
				return Factory.GetCachedValue($"CustomsBillsViewCollection for {companyPK}", () => new KREntryCustomsBillsViewCollection(Factory, new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL(), companyPK));
			}
		}

		public new CusReconEntry Parent => (CusReconEntry)base.Parent;
	}
}
