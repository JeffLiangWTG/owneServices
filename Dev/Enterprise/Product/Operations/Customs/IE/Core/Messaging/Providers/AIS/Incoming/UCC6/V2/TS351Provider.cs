using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS351;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS351Provider
	{
		public TS351Provider(Ts351 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts351 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZString SpecificCircumstanceIndicator => xmlObject.Declaration?.SpecificCircumstanceIndicator;

		public IReadOnlyCollection<ZDateTime> DateAndTimesOfPresentationOfTheGoods => xmlObject.Declaration?.PreviousDocument?.Select(x => x.DateAndTimeOfPresentationOfTheGoods.ConvertToZDateTime()).ToArray();

		public ZString ControlResultCode => xmlObject.Declaration?.ControlResult?.Code;

		public ZDateTime ControlResultDate => (xmlObject.Declaration?.ControlResult?.Date).ConvertToZDateTime();

		public ZString ControlResultRemarks => xmlObject.Declaration?.ControlResult?.Remarks;

		public ZString Remarks => xmlObject.Declaration?.Remarks;
	}
}
