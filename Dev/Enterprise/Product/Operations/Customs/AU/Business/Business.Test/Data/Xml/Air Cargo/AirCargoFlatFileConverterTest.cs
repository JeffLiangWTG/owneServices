using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestAirGetValueFromBooleanField()
		{
			var converter = new AirCargoFlatFileConverter(null, Factory);
			var dataRows = new FlatFileDataRow(4);
			dataRows[0] = "true";
			dataRows[1] = "false";
			dataRows[2] = "";
			dataRows[3] = "test";
			Assert("'true' can be parsed. Return true", converter.GetValueFromBooleanField(dataRows, 0));
			Assert("'false' can be parsed. Return false", !converter.GetValueFromBooleanField(dataRows, 1));
			Assert("empty string can't be parsed. Return false", !converter.GetValueFromBooleanField(dataRows, 2));
			Assert("'test' can't be parsed. Return false", !converter.GetValueFromBooleanField(dataRows, 3));
		}

		public void TestImportDataWithGoodsValueNotInTheCorrectFormat()
		{
			string flatFileContents =
@"AIRMWB|08143510541|83878|N||QF026||ITSPE|AUMEL|AUMEL|20050614|QANAIRSYD|QANTAS AIRLINES|||||||AU|67 005 944 203|
AIRHWB|HWB1|MAST1234|N||ITSPE|AUMEL|IT|PC|ConsignorCode1|ConsignorName1|ConsignorAddress11|ConsignorAddress12|Suburb1|PCode1|39 2 89 502 635|39 2 89 50 12 33|AU|||ConsigneeName1|ConsigneeAddress11|ConsigneeAddress12|Suburb3|PCode3|||AU|||NotifyName1|NotifyAddress11|NotifyAddress12|Suburb5|PCode5|||AU||I030H|9999Z|N|DodgyGoodsValue|AU
AIRGDS||||Goods Description 1||PK|282|     2978.000|KG|       28.600|M3|N|Y|N|Y|N|Y|N|Y|
";
			NotificationsForTesting notifications = new NotificationsForTesting();
			AirCargoFlatFileConverter converter = new AirCargoFlatFileConverter(notifications, Factory);
			Consol consol = new Consol();
			using (Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(flatFileContents)))
			using (TextReader textReader = new StreamReader(stream))
			{
				converter.ImportFlatFile(consol, new PipeDelimitedFlatFileFormat(), textReader);
				AssertMultilineASCIIEquals("notifications.ToString()", "Error: Invalid file format (Corrupt or invalid Air Cargo CSV file. Could not parse Goods Value [DodgyGoodsValue]. [Line 2])", notifications.ToString());
			}
		}

		sealed class NotificationsForTesting : List<INotification>, INotifications
		{
			public readonly List<INotification> Notifications = new List<INotification>();

			public override string ToString()
			{
				ZStringBuilder result = new ZStringBuilder();

				foreach (INotification notification in this)
				{
					result.Append(notification.Message);
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}
	}
}
