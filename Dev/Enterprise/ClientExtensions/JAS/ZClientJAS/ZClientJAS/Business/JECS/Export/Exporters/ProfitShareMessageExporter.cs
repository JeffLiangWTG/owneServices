using System.Collections;

using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class ProfitShareMessageExporter : JXCMessageExporter
	{
		public ProfitShareMessageExporter(JASForwardingConsol consol)
			: base(consol, new JXCExportLogger(consol))
		{
		}

		protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
		{
			return new MessageFileNameAndContents[1]
			{
				new MessageFileNameAndContents(ExportFileName, MessageLines)
			};
		}

		public override JXCExportValidationType ExportValidationTypeToUse
		{
			get { return JXCExportValidationType.ProfitShare; }
		}

		ZString ExportFileName
		{
			get { return string.Format("PS_{0}.{1}", Consol.MasterBillMAWB, Consol.MasterBillAirlinePrefix); }
		}

		MessageLine[] MessageLines
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(new APRSLine(Consol));
				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result.Add(new APRHLine(Consol, shipment));
				}
				return (MessageLine[])result.ToArray(typeof(MessageLine));
			}
		}

		JASForwardingConsol Consol
		{
			get { return (JASForwardingConsol)HeaderData; }
		}
	}
}

#region Implementation
#endregion
