using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public partial class TempStorageRegisterForm : ZTemplateForm
{
	public TempStorageRegisterForm()
	{
		InitializeComponent();
	}

	public TempStorageRegisterForm(CusTempStorageRegHeader header)
		: base(header)
	{ }

	public override string FormCaption
	{
		get
		{
			var caption = Res.GetString("F64ED2DE-6D5F-41E8-8F44-5230BFA4BAED", "Temp. Storage Register");
			var reference = RegHeader?.SRH_Reference;
			var jobReference = RegHeader?.SRH_InternalReference;
			if (!string.IsNullOrEmpty(reference))
			{
				caption += $" - {reference}/{jobReference}";
			}
			return caption;
		}
	}

	CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)BusinessEntity;

	protected override bool AllowNew => false;
}
