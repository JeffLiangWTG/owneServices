#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CASSFileImportDefaultTaxIDControl
	{
		public ZArchitecture.GUI.ZGuidFindBox StandardRatedTaxIDGuidFindBox_ForTestOnly
		{
			get { return StandardRatedTaxIDGuidFindBox; }
			set { StandardRatedTaxIDGuidFindBox = value; }
		}

		public ZArchitecture.GUI.ZGuidFindBox ZeroRatedTaxIDGuidFindBox_ForTestOnly
		{
			get { return ZeroRatedTaxIDGuidFindBox; }
			set { ZeroRatedTaxIDGuidFindBox = value; }
		}
	}
}

#endif
