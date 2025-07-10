using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public static class CASSAdjustmentFileTestHelper
	{
		public static CASSAdjustmentHeader GetCASSAdjustmentHeader(BusinessObjectFactory factory, CASSBilling billing = null, string currency = "EUR")
		{
			var adapter = new CASSAdjustmentFileAdapter();
			if (billing == null)
			{
				billing = GetFullyPopulatedCASS(factory, currency);
			}
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "10203040506";
			var adjHeader = adapter.ExportToValueObject(billing, new ValueObjectExportContext(new NotificationBuffer()));
			return adjHeader;
		}

		public static CASSBilling GetFullyPopulatedCASS(BusinessObjectFactory factory, string currency = "EUR")
		{
			var cassBilling = new CASSBilling(factory);
			cassBilling.Initialize(GetCASSCostHeader(factory, currency));

			cassBilling.CostHeader.ExportLines[0].WeightChargePP = 25000.25M;
			cassBilling.CostHeader.ExportLines[0].AdjustmentReason = "10";
			cassBilling.CostHeader.ExportLines[0].AdjustmentReasonComment = "I want to test";

			cassBilling.CostHeader.ExportLines[1].WeightChargePP = 45000.25M;
			cassBilling.CostHeader.ExportLines[1].AdjustmentReason = "11";
			cassBilling.CostHeader.ExportLines[1].AdjustmentReasonComment = "I want to test again";

			return cassBilling;
		}

		public static CASSCostHeader GetCASSCostHeader(BusinessObjectFactory factory, string currency)
		{
			CASSCostHeader header = null;
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine(FormattableString.Invariant($"AAADE   08080108083108091001{currency} 2347006"));
				writer.WriteLine(FormattableString.Invariant($"AWM  16000144154  DUS23470068510R   HKG0808140011900K{currency}0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180"));
				writer.WriteLine(FormattableString.Invariant($"DCO Y17267828073  LEJ23470068510808640{currency}00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX"));
				writer.WriteLine(FormattableString.Invariant($"DCR N17267828073  LEJ23470068510808640{currency}00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX"));
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);
				}
			}
			return header;
		}
	}
}
