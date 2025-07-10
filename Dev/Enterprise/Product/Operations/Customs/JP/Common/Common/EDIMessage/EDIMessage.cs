using System.Data;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Common
{
	public partial class EDIMessage : Messaging.Business.EDIMessage, Integration.Customs.JP.IEDIMessage, IDocumentSupportable
	{
		public EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = new MessageNumberStrategy(this);
		}

		public const string MessageDestination = "NACCS";
		public const string PasswordPlaceHolder = "<******>";
		public const string FlatFile = "Flat File";
		public const int MessageReferenceLength = 26;

		#region Schema
		public new class Schema : Messaging.Business.EDIMessage.Schema
		{
			public const string EM_Calc_ProcedureCode = "EM_Calc_ProcedureCode";
			public const string EM_Calc_ProcedureName = "EM_Calc_ProcedureName";
			public const string EM_Calc_OutputInformationCode = "EM_Calc_OutputInformationCode";
			public const string EM_Calc_OutputInformation = "EM_Calc_OutputInformation";
		}
		#endregion

		public ZString ProcedureCode
		{
			get => EM_MessageType + EM_MessageSubType;
			set
			{
				ZString effectiveProcedureCode = GetEffectiveProcedureCode(value);
				EM_MessageType = effectiveProcedureCode.SubstringSafe(0, EM_MessageTypeInfo.MaxLength);
				EM_MessageSubType = effectiveProcedureCode.SubstringSafe(EM_MessageTypeInfo.MaxLength, EM_MessageSubTypeInfo.MaxLength);
			}
		}

		internal static string GetEffectiveProcedureCode(string procedureCode) => procedureCode != null && new JPProcedureCodeList().ContainsCode(procedureCode) ? procedureCode : string.Empty;

		public override void OnSaving()
		{
			base.OnSaving();

			if (EM_MessageNum.IsEmpty)
			{
				PopulateMessageNumber();
				if (EM_ApplicationReference == EDIMessage.FlatFile && EM_ReceiveTransmit == Direction.Receive &&
					EM_LinkedObject is IStmALogProvider logProvider)
				{
					logProvider.AddEventWithReference(Events.DataImport, EM_MessageNum);
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (EM_MessageType == JPMessageTypes.Codes.XER)
				{
					if (messageInterpretation.IsEmpty && !base.EM_MessageText.IsEmpty)
					{
						messageInterpretation = new NACCSMessageInterpreter(this).ConvertXERXmlToHtmlTable();
					}
					if (!messageInterpretation.IsEmpty)
					{
						return messageInterpretation;
					}
					else
					{
						return base.EM_MessageInterpretation;
					}
				}

				var messageInterpretationNote = MessageInterpretationNoteManager.Value;
				if (messageInterpretationNote.IsEmpty)
				{
					if (messageInterpretation.IsEmpty)
					{
						messageInterpretation = new NACCSMessageInterpreter(this).Interprete();
					}

					return messageInterpretation;
				}
				else
				{
					return messageInterpretationNote;
				}
			}
			set => base.EM_MessageInterpretation = value;
		}
		ZString messageInterpretation;

		public override ZString EM_FormattedMessageText => messageDataText ??= new NACCSMessageInterpreter(this).MessageText;
		string messageDataText;

		public override ZString EM_MessageTextDetail => EM_FormattedMessageText;

		protected override void PopulateMessageNumber()
		{
			PopulateNumberPropertyIfRequired<ZString>(EM_MessageNumInfo, factory => GetMessageReferenceNumber());

			if (EM_ApplicationReference == EDIMessage.FlatFile && EM_ReceiveTransmit == Direction.Transmit &&
					EM_LinkedObject is IStmALogProvider logProvider)
			{
				logProvider.AddEventWithReference(Events.DataExport, EM_MessageNum);
			}
		}

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumberStrategy != null ? base.GetMessageReferenceNumber() : string.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.JPCustoms;
		}

		public static EDIMessage CreateFromInboundMessageLoad(BusinessObjectFactory factory, byte[] messageData)
		{
			var message = factory.New<EDIMessage>();

			var header = JPMessageUtils.ParseInboundHeaderOnly(NACCSFactoryService.GetMessageFlatParser(factory), messageData);
			message.MessageNumberStrategy = new MessageNumberStrategy(message, header);

			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageData = messageData;
			message.ProcedureCode = header.ProcedureCode;

			return message;
		}

		public ZString GetOutputInformationCodeSafe()
		{
			if (EM_MessageType == JPMessageTypes.Codes.XER)
			{
				return ZString.Empty;
			}

			if (EM_ReceiveTransmit == Direction.Receive)
			{
				var responseHeader = JPMessageUtils.ParseInboundHeaderOnly(NACCSFactoryService.GetMessageFlatParser(Factory), EM_MessageData);
				return responseHeader?.OutputInformationCode ?? ZString.Empty;
			}

			return ZString.Empty;
		}

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new MessageDocumentSupporter(this);

		#endregion

		#region EM_Calc_Property
		[ResourceStringData("EB4EFB8D-DBD3-47AB-BE89-7E3FAE1400C1", Caption = "Procedure Code")]
		public ZString EM_Calc_ProcedureCode => EM_MessageType != JPMessageTypes.Codes.XER ? ProcedureCode : ZString.Empty;

		[ResourceStringData("008C684A-F50E-43FD-B77D-AB24EE4029A6", Caption = "Procedure Name")]
		public ZString EM_Calc_ProcedureName => EM_Calc_ProcedureCode.IsEmpty ? ZString.Empty : JPProcedureCodeJPNameList.GetDescriptionFromCode(EM_Calc_ProcedureCode);

		[ResourceStringData("6B31C77F-4888-4A4B-808C-5D08273845C4", Caption = "Output Information Code", ShortCaption = "Output Code")]
		public ZString EM_Calc_OutputInformationCode
		{
			get
			{
				if (outputInformationCode.IsEmpty)
				{
					outputInformationCode = GetOutputInformationCodeSafe();
				}
				return outputInformationCode;
			}
		}

		[ResourceStringData("14FA5058-7150-4286-AB7D-79F5E4313B46", Caption = "Output Information", ShortCaption = "Output Name")]
		public ZString EM_Calc_OutputInformation => EM_Calc_OutputInformationCode.IsEmpty ? ZString.Empty : JPOutputInformationCodeJPNameList.GetDescriptionFromCode(EM_Calc_OutputInformationCode);

		JPProcedureCodeJPNameList JPProcedureCodeJPNameList => Factory.GetCachedValue<JPProcedureCodeJPNameList>();

		JPOutputInformationCodeJPNameList JPOutputInformationCodeJPNameList => Factory.GetCachedValue<JPOutputInformationCodeJPNameList>();

		ZString outputInformationCode;
		#endregion
	}
}
