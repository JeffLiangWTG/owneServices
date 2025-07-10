using System.Collections;
using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI
{
	public interface IZDocumentMenuItem
	{
		void Setup(IDocumentSupportable parentBusinessObject, IDocumentEventsForMenu documentEventsForMenu, UserControlProviderList parentUserFieldList, UserControlProviderList parentSystemDefinedFieldList);
		void LoadMenus(Form parentForm);
#if DEBUG
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
#endif
		IList Items { get; }
	}
}
