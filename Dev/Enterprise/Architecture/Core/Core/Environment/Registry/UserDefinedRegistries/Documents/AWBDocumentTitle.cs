using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.Environment
{
	public class AWBDocumentTitle
	{
#if DEBUG
		public AWBDocumentTitle(string original1Name, string original1Title, bool original1Printed, string original2Name, string original2Title, bool original2Printed, string original3Name, string original3Title, bool original3Printed, string copy4Name, string copy4Title, bool copy4Printed, string copy5Name, string copy5Title, bool copy5Printed, string copy6Name, string copy6Title, bool copy6Printed, string copy7Name, string copy7Title, bool copy7Printed, string copy8Name, string copy8Title, bool copy8Printed)
		{
			Original1 = new AWBDocumentTitleItem(original1Name, original1Title, original1Printed);
			Original2 = new AWBDocumentTitleItem(original2Name, original2Title, original2Printed);
			Original3 = new AWBDocumentTitleItem(original3Name, original3Title, original3Printed);
			Copy4 = new AWBDocumentTitleItem(copy4Name, copy4Title, copy4Printed);
			Copy5 = new AWBDocumentTitleItem(copy5Name, copy5Title, copy5Printed);
			Copy6 = new AWBDocumentTitleItem(copy6Name, copy6Title, copy6Printed);
			Copy7 = new AWBDocumentTitleItem(copy7Name, copy7Title, copy7Printed);
			Copy8 = new AWBDocumentTitleItem(copy8Name, copy8Title, copy8Printed);
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IATA regulatory name")]
		internal AWBDocumentTitle(byte[] xmlBytes)
		{
			if (xmlBytes != null)
			{
				var data = new DataSet();

				var lookup = new Dictionary<string, AWBDocumentTitleItem>();

				using (var xmlStream = new MemoryStream(xmlBytes))
				{
					data.ReadXml(xmlStream, XmlReadMode.Auto);

					foreach (DataRow row in data.Tables[0].Rows)
					{
						var item = new AWBDocumentTitleItem(row);
						lookup.Add(item.Name, item);
					}

					Original1 = GetItem(lookup, "Original 1 - (for Issuing Carrier)");
					Original2 = GetItem(lookup, "Original 2 - (for Consignee)");
					Original3 = GetItem(lookup, "Original 3 - (for Shipper)");
					Copy4 = GetItem(lookup, "Copy 4 - (Delivery Receipt)");
					Copy5 = GetItem(lookup, "Copy 5 - (Extra Copy)");
					Copy6 = GetItem(lookup, "Copy 6 - (Extra Copy)");
					Copy7 = GetItem(lookup, "Copy 7 - (Extra Copy)");
					Copy8 = GetItem(lookup, "Copy 8 - (for Agent)");
				}
			}
		}

		static AWBDocumentTitleItem GetItem(Dictionary<string, AWBDocumentTitleItem> lookup, string name)
		{
			return lookup.ContainsKey(name) ? lookup[name] : new AWBDocumentTitleItem();
		}

		public readonly AWBDocumentTitleItem Original1;
		public readonly AWBDocumentTitleItem Original2;
		public readonly AWBDocumentTitleItem Original3;
		public readonly AWBDocumentTitleItem Copy4;
		public readonly AWBDocumentTitleItem Copy5;
		public readonly AWBDocumentTitleItem Copy6;
		public readonly AWBDocumentTitleItem Copy7;
		public readonly AWBDocumentTitleItem Copy8;
	}
}
