using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageInterpreter : AutoEDIMessageInterpreter
	{
		public EDIMessageInterpreter()
			: base(new BusinessObjectFactory())
		{
		}

		[List("ApplicationCodes")]
		public override ZString ApplicationCode
		{
			get { return base.ApplicationCode; }
			set { base.ApplicationCode = value; }
		}

		public ApplicationCodeList ApplicationCodes => Factory.GetCachedValue<ApplicationCodeList>();

		public override ZString MessageInterpretation => messageInterpretation;
		ZString messageInterpretation;

		[List("DirectionList")]
		public override ZString ReceiveTransmit
		{
			get { return base.ReceiveTransmit; }
			set { base.ReceiveTransmit = value; }
		}

		public ReceiveTransmitList DirectionList => Factory.GetCachedValue<ReceiveTransmitList>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal usage only")]
		public void GenerateMessageTextInterpretation()
		{
			if (MessageText.IsEmpty)
			{
				messageInterpretation = "No text to interpret.\r\nPlease enter Message Text.";
			}
			else
			{
				RunPreSaveValidation();
				if (this.HasErrors())
				{
					messageInterpretation = "Could not interpret message text.\r\nPlease fix all errors before trying again.";
				}
				else
				{
					var suppressReportingOfErrors = ErrorReporter.SuppressReportingOfErrors;

					try
					{
						ErrorReporter.SuppressReportingOfErrors = true;
						var message = Factory.New<EDIMessage>();
						message.EM_ApplicationCode = ApplicationCode;
						message.EM_ApplicationReference = ApplicationReference;
						message.EM_MessageType = MessageType;
						message.EM_MessageSubType = MessageSubType;
						message.EM_ReceiveTransmit = ReceiveTransmit;
						var row = ((IBusinessObjectInternals)message).Row;
						var type = EDIMessage.TypeDecider.GetTypeForLoad(row, Factory);
						var typedMessage = (EDIMessage)Activator.CreateInstance(type, Factory, row);
						typedMessage.EM_MessageText = MessageText;
						messageInterpretation = typedMessage.EM_MessageInterpretation;
					}
					catch (Exception e)
					{
						messageInterpretation = "Could not interpret message text.\r\n\r\n" + e.ToString();
					}
					finally
					{
						ErrorReporter.SuppressReportingOfErrors = suppressReportingOfErrors;
					}
				}
			}
			MessageInterpretationInfo.RefreshBinding();
		}
	}
}
