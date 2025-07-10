using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class CodeDescriptionListEditControlForRegistryEx : CodeDescriptionListEditControlForRegistry
	{
		public CodeDescriptionListEditControlForRegistryEx(IRegistryItem registryItem,
			bool showCodeColumn,
			bool showDescriptionColumn,
			CharacterCasing codeFieldCasing,
			CharacterCasing descriptionFieldCasing,
			string codeColumnCaption,
			string descriptionColumnCaption,
			int codeColumnMaxLength)
			: base(registryItem, showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, descriptionColumnCaption, codeColumnMaxLength)
		{
			this.registryItem = registryItem;

			var pasteMenuItem = new ZMenuItem("&Paste", PasteMenuItem_Click);
			CodeDescriptionGrid.ContextMenu.MenuItems.Add(pasteMenuItem);
		}

		protected readonly IRegistryItem registryItem;

		protected void PasteMenuItem_Click(object sender, EventArgs e)
		{
#if !WINZOR
			string clipText = SafeClipboard.GetText();
			if (!string.IsNullOrEmpty(clipText))
			{
				byte[] val = FieldValue;
				var list = (val != null) ? new CodeDescriptionPairList(val) : new CodeDescriptionPairList();
				string[] lines = clipText.Split(new char[] { '\n' });
				bool modified = false;
				foreach (string line in lines)
				{
					string[] fields = line.Trim().Split(new char[] { '\t', ',' });
					if (fields.Length >= 2)
					{
						string country = fields[0].Trim();
						string region = fields[1].Trim();
						const int CountryCodeLength = 2;
						const int MinRegionLength = 1;
						if (country.Length == CountryCodeLength && region.Length >= MinRegionLength)
						{
							list.AddOverwriteIfExists(new CodeDescriptionPair(country, region));
							modified = true;
						}
					}
				}
				if (modified)
				{
					FieldValue = list.ToXMLByteArray();
				}
			}
#endif

		}
	}
}
