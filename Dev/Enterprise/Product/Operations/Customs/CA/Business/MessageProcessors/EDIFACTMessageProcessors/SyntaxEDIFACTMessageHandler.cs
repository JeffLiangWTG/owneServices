using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class SyntaxEDIFACTMessageHandler : ResponseMessageProcessor
	{
		public SyntaxEDIFACTMessageHandler(LoggingInformation logger) : base(logger, MessageTypeList.Codes.SyntaxError, Res.GetString("434c944f-2d72-41aa-a67d-720f1c2c2643", "EDIFACT Syntax Error Response"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage) => ediMessage.EM_Status = EDIMessage.Status.Failed;

		#region Overridden Properties

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return Env.Registry.PostMasterGroup; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Constants.EmailTo.NominatedGroup; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return Env.Registry.PostMasterGroup; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Constants.EmailTo.NominatedGroup; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return Env.Registry.PostMasterGroup; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return Constants.EmailTo.NominatedGroup; }
		}

		protected override BusinessObject EmailResponseLinkedObject
		{
			get { return null; }
		}

		#endregion
	}
}
