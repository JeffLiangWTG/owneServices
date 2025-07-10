using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(ExportManifestImportForm))]
	sealed class ExportManifestImportFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			TemporaryManifestHolder holder = new TemporaryManifestHolder(Factory);
			holder.Manifests.Add(new TemporaryManifest(Factory, new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest)));
			return new ExportManifestImportForm(holder);
		}
	}
}
