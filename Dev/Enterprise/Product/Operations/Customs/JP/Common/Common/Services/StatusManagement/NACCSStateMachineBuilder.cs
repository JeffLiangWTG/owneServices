using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	public static class NACCSStateMachineBuilder
	{
		public static NACCSStateMachine Build(INACCSStatus naccsStatus)
		{
			return new NACCSStateMachine
			(
				new MessageStatusModel(naccsStatus),
				new CustomsStatusModel(naccsStatus),
				new PhaseModel(naccsStatus),
				new NACCSProcessContext
				{
					IsExport = naccsStatus.IsExport,
				}
			);
		}

		public static INACCSRequest BuildRequest(EDIMessage message)
		{
			return new NACCSRequest(message);
		}
	}

	public class MessageStatusModel : IStatusModel
	{
		public MessageStatusModel(INACCSStatus naccsStatus)
		{
			this.naccsStatus = naccsStatus;
		}

		public string Status
		{
			get => naccsStatus.MessageStatus;
			set => naccsStatus.MessageStatus = value;
		}

		readonly INACCSStatus naccsStatus;
	}

	public class CustomsStatusModel : IStatusModel
	{
		public CustomsStatusModel(INACCSStatus naccsStatus)
		{
			this.naccsStatus = naccsStatus;
		}

		public string Status
		{
			get => naccsStatus.CustomsStatus;
			set => naccsStatus.CustomsStatus = value;
		}

		readonly INACCSStatus naccsStatus;
	}

	public class PhaseModel : IStatusModel
	{
		public PhaseModel(INACCSStatus naccsStatus)
		{
			this.naccsStatus = naccsStatus;
		}

		public string Status
		{
			get => naccsStatus.PhaseStatus;
			set => naccsStatus.PhaseStatus = value;
		}

		readonly INACCSStatus naccsStatus;
	}

	sealed class NACCSRequest : INACCSRequest
	{
		public NACCSRequest(EDIMessage message)
		{
			if (message.EM_ReceiveTransmit != ReceiveTransmitList.Codes.Transmit)
			{
				throw new DeveloperNotificationException("A request can only be an outbound message");
			}

			Phase = message.ProcedureCode;
			IsFromFlatFile = message.EM_ApplicationReference == EDIMessage.FlatFile;
			HasError = (message.Interchange?.EI_Status ?? string.Empty) == EDIInterchangeStatusList.Codes.Error;
		}

		public string Phase { get; }

		public bool IsFromFlatFile { get; }

		public bool HasError { get; }
	}
}
