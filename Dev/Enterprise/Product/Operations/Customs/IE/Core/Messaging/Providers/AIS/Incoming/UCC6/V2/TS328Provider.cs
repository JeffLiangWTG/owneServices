using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS328;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS328Provider
	{
		public TS328Provider(Ts328 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts328 xmlObject;
		public ZString LRN => xmlObject.Declaration?.Lrn;
		public ZString MRN => xmlObject.Declaration?.Mrn;
		public ZDateTime DateOfAcceptance => xmlObject.Declaration?.AcceptanceDate?.DateOfAcceptance ?? ZDateTime.Empty;
		public ZDateTime ResponseDateLimit => xmlObject.Declaration?.ResponseDateLimit ?? ZDateTime.Empty;
		public ZDateTime DateAndTimeOfPresentationOfGoods => xmlObject.Declaration?.PreviousDocument?.First()?.DateAndTimeOfPresentationOfTheGoods ?? ZDateTime.Empty;
		public ZString Remarks => xmlObject.Declaration?.Remarks;
	}
}
