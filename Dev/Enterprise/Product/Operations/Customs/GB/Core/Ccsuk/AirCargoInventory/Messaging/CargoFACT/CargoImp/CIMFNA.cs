using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFNA : CIMFMA
	{
		public CIMFNA(List<string> list, BusinessObjectFactory factory, CargoFactMessage cargoFact)
			: base(list, factory, cargoFact)
		{
		}

		public override ZString CargoImpCode
		{
			get { return Code; }
		}

		public new const string Code = "FNA";

		public override ZString MessageInterpretation
		{
			get
			{
				return string.Format(
				  @" {0}
<p><b>FNA Rejection</b></p>
<p><i>{1}</i>  </p>
<p>Original message type: {2} </p> 
",
					MessagePrettierCss.CSS,
					acknowledgementText,
					originalMessageType);
			}
		}

		protected override ZString OutboundMessageNewStatus
		{
			get { return EDIMessage.Status.Rejected; }
		}
	}
}
