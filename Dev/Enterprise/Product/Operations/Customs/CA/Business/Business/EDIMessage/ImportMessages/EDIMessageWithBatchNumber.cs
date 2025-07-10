using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class EDIMessageWithBatchNumber : EDIMessage
	{
		public EDIMessageWithBatchNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZString BatchNumber
		{
			get
			{
				var result = string.Empty;
				if (EdifactMessage is Enterprise.Edifact.D99B.Messages.CUSDEC.CUSDECMessage)
				{
					var cusdec = (Enterprise.Edifact.D99B.Messages.CUSDEC.CUSDECMessage)EdifactMessage;
					if (cusdec.BGM.Count > 0)
					{
						result = cusdec.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
					}
				}
				else if (EdifactMessage is Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage)
				{
					var cusres = (Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage)EdifactMessage;
					if (cusres.BGM.Count > 0)
					{
						result = cusres.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
					}
				}
				else if (EdifactMessage is Enterprise.Edifact.D99B.Messages.CUSPED.CUSPEDMessage)
				{
					var cusres = (Enterprise.Edifact.D99B.Messages.CUSPED.CUSPEDMessage)EdifactMessage;
					if (cusres.BGM.Count > 0)
					{
						result = cusres.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
					}
				}
				return result;
			}
		}

		#endregion

	}
}
