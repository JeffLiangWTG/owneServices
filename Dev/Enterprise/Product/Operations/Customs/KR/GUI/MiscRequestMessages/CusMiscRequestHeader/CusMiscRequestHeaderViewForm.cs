using CargoWise.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CusMiscRequestHeaderViewForm : ZTemplateForm
	{
		public CusMiscRequestHeaderViewForm(CusMiscRequestHeader header)
			: base(header)
		{
			this.header = header;
			InitializeComponent();
			PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn);
		}
		readonly CusMiscRequestHeader header;

		public override string FormCaption
		{
			get
			{
				string caption = Res.GetString("9A3D86DA-8B8B-4005-B79D-EE7B7022A66F", "Misc Request");

				if (!this.IsDesignMode() && header != null)
				{
					if (header.CMR_MessageType == Messaging.ElectronicDocumentTypeList.Codes._5AC)
					{
						caption = Res.GetString("20C297B1-BFEE-4D11-B8B1-A7A826414E4A", "(Exp) Application for Extended Office Hours");
					}
					else if (header.CMR_MessageType == Messaging.ElectronicDocumentTypeList.Codes._5GW)
					{
						caption = Res.GetString("A5CEBC7B-1AA1-4DB4-99BE-38E1A46FB745", "(Imp) Application for Extended Office Hours");
					}

					caption += " - " + header.FormattedApplicationNumber;
				}
				return caption;
			}
		}
	}
}
