using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.SEK
{
	public class SEKDocAWB : DocAWB
	{
		#region constructor

		public SEKDocAWB(ExportAWBHeader header, BusinessObjectFactory factory) : base(header, factory) { }

		public new static DocAWB New(ExportAWBHeader header, BusinessObjectFactory factory)
		{
			return (header == null) ? null : new SEKDocAWB(header, factory);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public ZDecimal TotalHAWBWeightKG
		{
			get
			{
				ZDecimal result = 0;
				if (ExportAWBHeader.Consol != null)
				{
					Array.ForEach(ExportAWBHeader.Consol.Shipments.ToArray(),
					b =>
					{
						ForwardingShipment s = (ForwardingShipment)b;
						result += s.JS_JS_ColoadMasterShipment.IsEmpty ? Core.Constants.Weight.Convert(s.TotalOuterPacksWeight, s.ShipmentWeightUnit, Core.Constants.Weight.Kilograms) : 0m;
					}
					);
				}
				return result;
			}
		}

		public ZInt AWBsCount
		{
			get
			{
				var awb = ExportAWBHeader as ConsolExportAWBHeader;
				return awb != null && awb.Consol != null ? 1 : 0;
			}
		}

		public ZString TotalConsolWeight
		{
			get
			{
				ZString result = ZString.Empty;

				if (DocConsol != null && DocConsol.IsAir && DocConsol.IsExportConsol)
				{
					result = DocConsol.WeightAsString;
				}

				return result;
			}
		}
	}
}