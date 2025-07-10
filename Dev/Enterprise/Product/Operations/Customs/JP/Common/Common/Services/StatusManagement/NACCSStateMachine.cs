using System;
using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Common
{
	public class NACCSStateMachine
	{
		public NACCSStateMachine(MessageStatusModel messageStatusModel, CustomsStatusModel customsStatusModel, PhaseModel phaseModel, NACCSProcessContext context)
		{
			MessageStatusModel = messageStatusModel;
			CustomsStatusModel = customsStatusModel;
			PhaseModel = phaseModel;
			Context = context;
		}

		public void TransitOn(INACCSRequest request)
		{
			if (!request.HasError)
			{
				var list = new JPProcedureCodeList.PhaseList();
				if (list.ContainsCode(request.Phase))
				{
					ChangePhase(request.Phase);
				}
			}
			ChangeMessageStatus(request);
		}

		public void TransitOn(IJPInboundMessageParseResult parseResult)
		{
			ChangeMessageStatus(parseResult);
			ChangeCustomsStatus(parseResult);
		}

		public string PickNextPhase() => ListNextPhases().FirstOrDefault();

		string[] ListNextPhases()
		{
			switch (MessageStatusModel.Status)
			{
				case JPMessageStatusList.Codes.Acknowledged:
					switch (PhaseModel.Status)
					{
						case JPProcedureCodeList.Codes.IVB01:
							return new[] { JPProcedureCodeList.Codes.IVB };
						case JPProcedureCodeList.Codes.IVB:
							return new[] { JPProcedureCodeList.Codes.EDB };
						case JPProcedureCodeList.Codes.EDB:
							return new[] { JPProcedureCodeList.Codes.EDA };
						case JPProcedureCodeList.Codes.EDA:
							return new[] { JPProcedureCodeList.Codes.EDC, JPProcedureCodeList.Codes.EDA01 };
						case JPProcedureCodeList.Codes.EDA01:
							return new[] { JPProcedureCodeList.Codes.EDE };
						case JPProcedureCodeList.Codes.EDC:
							if (CustomsStatusModel.Status == CustomsStatusList.Codes.Cleared)
							{
								return new[] { JPProcedureCodeList.Codes.EAC, JPProcedureCodeList.Codes.VAN, JPProcedureCodeList.Codes.VAE };
							}
							else
							{
								return new[] { JPProcedureCodeList.Codes.EDA01, JPProcedureCodeList.Codes.CEW };
							}
						case JPProcedureCodeList.Codes.CEW:
							if (CustomsStatusModel.Status == CustomsStatusList.Codes.Cleared)
							{
								return new[] { JPProcedureCodeList.Codes.EAC, JPProcedureCodeList.Codes.VAN, JPProcedureCodeList.Codes.VAE };
							}
							else
							{
								return Array.Empty<string>();
							}
						case JPProcedureCodeList.Codes.ECR11:
							return new[] { JPProcedureCodeList.Codes.ECR };
						case JPProcedureCodeList.Codes.ECR:
							return new[] { JPProcedureCodeList.Codes.EDB, JPProcedureCodeList.Codes.VAN, JPProcedureCodeList.Codes.VAE };
						case JPProcedureCodeList.Codes.IDB:
							return new[] { JPProcedureCodeList.Codes.IDA };
						case JPProcedureCodeList.Codes.IDA:
							return new[] { JPProcedureCodeList.Codes.IDC, JPProcedureCodeList.Codes.IDA01 };
						case JPProcedureCodeList.Codes.IDA01:
							return new[] { JPProcedureCodeList.Codes.IDE };
						case JPProcedureCodeList.Codes.IDC:
							return new[] { JPProcedureCodeList.Codes.IDA01 };
						default:
							return Array.Empty<string>();
					}
				case JPMessageStatusList.Codes.Rejected:
				case JPMessageStatusList.Codes.Error:
					switch (PhaseModel.Status)
					{
						default:
							return new[] { PhaseModel.Status };
					}
				default:
					return Context.IsExport ? new[] { JPProcedureCodeList.Codes.EDA } : new[] { JPProcedureCodeList.Codes.IDA };
			}
		}

		public string CurrentStateDescription
		{
			get
			{
				switch (PhaseModel.Status)
				{
					case JPProcedureCodeList.Codes.IDA:
					case JPProcedureCodeList.Codes.EDA:
						if (MessageStatusModel.Status == JPMessageStatusList.Codes.Rejected)
						{
							return Res.GetString("1AE4A2C4-900F-47D5-B629-689957D97A7F", "The original customs declaration registration has been rejected by the customs.");
						}
						else if (MessageStatusModel.Status == JPMessageStatusList.Codes.Acknowledged)
						{
							return Res.GetString("CA8F0259-C418-4452-BF6E-8DA4BEDF42F0", "The original customs declaration submission has been approved.");
						}
						else
						{
							return string.Empty;
						}
					case JPProcedureCodeList.Codes.IDA01:
					case JPProcedureCodeList.Codes.EDA01:
						if (MessageStatusModel.Status == JPMessageStatusList.Codes.Rejected)
						{
							return Res.GetString("EC76B87B-B7B5-4187-8923-6EADA0341021", "The customs declaration correction registration has been rejected.");
						}
						else if (MessageStatusModel.Status == JPMessageStatusList.Codes.Acknowledged)
						{
							return Res.GetString("B28EF31B-DC87-440A-B165-EA6A4E9C587F", "The customs declaration correction registration has been approved.");
						}
						else
						{
							return string.Empty;
						}
					case JPProcedureCodeList.Codes.IDC:
					case JPProcedureCodeList.Codes.EDC:
						if (MessageStatusModel.Status == JPMessageStatusList.Codes.Acknowledged)
						{
							return Res.GetString("F79FF000-AE0E-4A68-8391-3AEA873C167B", "The customs declaration submission has been approved.");
						}
						else
						{
							return string.Empty;
						}
					case "":
						return Res.GetString("B71E230D-F1F4-4E76-89E1-A8DBB3FB798D", "No customs declaration registration has been sent to the customs.");
					default:
						return string.Empty;
				}
			}
		}

		void ChangePhase(string phase)
		{
			PhaseModel.Status = phase;
		}

		void ChangeCustomsStatus(IJPInboundMessageParseResult parseResult)
		{
			if (parseResult.HasResultCode)
			{
				if (parseResult.HasWarnings)
				{
					CustomsStatusModel.Status = CustomsStatusList.Codes.Warning;
				}
			}
			else
			{
				switch (parseResult.MessageProvider.Schema)
				{
					case IDACopy _:
					case IDCCopy _:
					case EDACopy _:
					case EDCCopy _:
						if (CustomsStatusModel.Status != CustomsStatusList.Codes.Warning)
						{
							CustomsStatusModel.Status = CustomsStatusList.Codes.Copied;
						}
						break;
					case InsufficientGuaranteeBalance _:
						CustomsStatusModel.Status = CustomsStatusList.Codes.Insufficient;
						break;
					case PaymentSlipInformation _:
						CustomsStatusModel.Status = CustomsStatusList.Codes.Payment;
						break;
					case InspectionInformation _:
						CustomsStatusModel.Status = CustomsStatusList.Codes.Inspected;
						break;
					case PendingComplianceOfOtherLawsAndRegulations _:
						CustomsStatusModel.Status = CustomsStatusList.Codes.Compliance;
						break;
					case SpecialDeclarationError _:
						CustomsStatusModel.Status = CustomsStatusList.Codes.Error;
						break;
					case ImportClearanceNotice _:
					case ExportClearancePermit _:
						CustomsStatusModel.Status = CustomsStatusList.Codes.Cleared;
						break;
				}
			}
		}

		void ChangeMessageStatus(INACCSRequest request)
		{
			MessageStatusModel.Status = request.HasError
				? JPMessageStatusList.Codes.Error
				: request.IsFromFlatFile ? JPMessageStatusList.Codes.Exported : JPMessageStatusList.Codes.Sending;
		}

		void ChangeMessageStatus(IJPInboundMessageParseResult parseResult)
		{
			if (parseResult.HasResultCode)
			{
				if (parseResult.IsSuccess)
				{
					MessageStatusModel.Status = JPMessageStatusList.Codes.Acknowledged;
				}
				else
				{
					MessageStatusModel.Status = JPMessageStatusList.Codes.Rejected;
				}
			}
		}

		public IStatusModel MessageStatusModel { get; }

		public IStatusModel CustomsStatusModel { get; }

		public IStatusModel PhaseModel { get; }

		public NACCSProcessContext Context { get; }
	}

	public interface IStatusModel
	{
		string Status { get; set; }
	}

	public interface INACCSRequest
	{
		string Phase { get; }
		bool IsFromFlatFile { get; }
		bool HasError { get; }
	}

	public class NACCSProcessContext
	{
		public bool IsExport { get; set; }
	}
}
