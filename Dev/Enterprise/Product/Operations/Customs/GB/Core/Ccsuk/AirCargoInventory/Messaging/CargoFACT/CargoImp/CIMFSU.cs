using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSU : CargoImpBase, ICimParser
	{
		/// <summary>
		/// For parsing inbound messages
		/// </summary>
		public CIMFSU(List<string> list, EDIMessage inboundMessage)
			: base(new ErrorCollector())
		{
		}

		public const string Code = "FSU";

		public override ZString CargoImpCode
		{
			get { return Code; }
		}

		public ICcsukCusAwb Awb => null;

		protected override string[] CargoImpLinesWithoutType => System.Array.Empty<string>();

		public override ZString MessageInterpretation => throw new System.NotImplementedException();

		public void DoPrinting()
		{
		}

		public bool DoAllProcessingBeforePrinting() => false;
	}
}
