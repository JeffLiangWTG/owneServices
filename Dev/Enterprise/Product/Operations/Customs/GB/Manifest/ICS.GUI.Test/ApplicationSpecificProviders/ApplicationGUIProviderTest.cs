using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	public class ApplicationGUIProviderTest : ApplicationGUIProviderBaseTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[]
				{
					EU.Manifest.Business.AsycudaBill.Schema.SpecialMentions
				}, columnInfos.Select(s => s.ColumnName));

			var specialMentionsDropEditColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == EU.Manifest.Business.AsycudaBill.Schema.SpecialMentions);
			AssertEquals(false, specialMentionsDropEditColumnStyleInfo.IsVisible);
		}

		protected override ZString ManifestType => ICSManifestTypes.Codes.ICS;
	}
}
