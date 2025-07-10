using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class CusEntryHeaderDocumentSupporterConfigurator : IITCusEntryHeaderDocumentSupporterConfigurator
{
	public CancelEventArgs Configure(JobDeclarationSadDocumentSupporter supporter)
	{
		using (var sadDocumentSupporterConfiguratorForm = new SadDocumentSupporterConfiguratorForm(supporter, bgmReferenceReadOnly: true))
		{
			var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(sadDocumentSupporterConfiguratorForm);
			return new CancelEventArgs(dialogResult != DialogResult.OK);
		}
	}
}
