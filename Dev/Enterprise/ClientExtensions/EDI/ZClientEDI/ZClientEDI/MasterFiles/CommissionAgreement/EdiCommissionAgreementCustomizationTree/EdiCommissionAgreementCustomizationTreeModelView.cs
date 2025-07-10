using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EdiCommissionAgreementCustomizationTreeModelView : ZTreeModelView<EdiCommissionAgreementTreeBizObjWrapper>
	{
		public EdiCommissionAgreementCustomizationTreeModelView(EdiCommissionAgreementCustomizationTreeModel inner)
			: base(inner)
		{
		}

		public void Rebuild()
		{
			InnerModel.Rebuild();
			RefreshView();
		}

		protected new EdiCommissionAgreementCustomizationTreeModel InnerModel
		{
			get { return (EdiCommissionAgreementCustomizationTreeModel)base.InnerModel; }
		}
	}
}
