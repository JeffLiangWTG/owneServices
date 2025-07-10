using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class TempStorageRegisterForm : ZTemplateForm
	{
		public TempStorageRegisterForm()
		{
			InitializeComponent();
		}

		public TempStorageRegisterForm(CusTempStorageRegHeader header)
			: base(header)
		{
		}
	}
}
