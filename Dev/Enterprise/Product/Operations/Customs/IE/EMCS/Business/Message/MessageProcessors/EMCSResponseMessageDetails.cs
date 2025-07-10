using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using EMCSVersion4_1 = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSResponseMessageDetails : IEMCSResponseMessageDetails
	{
		public ResponseDetail GetResponseDetail(string messageType, string messageText = null)
		{
			return ResponseDetails.TryGetValue(messageType, out var responseDetailArray) ? responseDetailArray[0] : ResponseDetail.Empty;
		}

		public ImmutableDictionary<string, ResponseDetail[]> ResponseDetails => responseDetails ?? (responseDetails = ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail[]>
		{
			{ EMCSIncomingMessageTypeList.Codes.IE704, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE704.Ie704Type), typeof(IE704MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE801, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE801.Ie801Type), typeof(IE801MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE802, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE802.Ie802Type), typeof(IE802MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE803, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE803.Ie803Type), typeof(IE803MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE810, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE810.Ie810Type), typeof(IE810MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE813, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE813.Ie813Type), typeof(IE813MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE818, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE818.Ie818Type), typeof(IE818MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE819, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE819.Ie819Type), typeof(IE819MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE829, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE829.Ie829Type), typeof(IE829MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE839, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE839.Ie839Type), typeof(IE839MessageProcessor)) }
			},
			{ EMCSIncomingMessageTypeList.Codes.IE917, new ResponseDetail[] {
				new ResponseDetail(typeof(EMCSVersion4_1.IE917.Ie917Type), typeof(IE917MessageProcessor)) }
			}
		}));

		public ResponseDetail AcknowledgementResponseDetail => new ResponseDetail(xmlObjectType: null, processorType: typeof(MessageAcknowledgementProcessor));

		ImmutableDictionary<string, ResponseDetail[]> responseDetails;
	}
}
