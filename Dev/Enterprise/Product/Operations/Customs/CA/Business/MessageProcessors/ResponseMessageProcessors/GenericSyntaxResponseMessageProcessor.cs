using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common;
using Enterprise.Edifact.D00A.Messages.CUSRES;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	[ApplicationIdentifier(ServiceOptions.Codes.GenericSyntax)]
	public class GenericSyntaxResponseMessageProcessor : ResponseMessageProcessor
	{
		public GenericSyntaxResponseMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypeList.Codes.G7Export, Res.GetString("bf8fd3cd-3aef-4eeb-98b5-009c2d61f092", "Generic Syntax Error Response"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var resultStatus = ZString.Empty;
			var message = (EDIMessage)ediMessage;
			var cusresMessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			if (cusresMessage != null)
			{
				var bgmReference = cusresMessage.BGM[0].DocumentMessageIdentification.DocumentIdentifier.Replace(" ", "");

				linkedObject = GetLinkedObject(message.Factory, bgmReference);
				if (linkedObject != null)
				{
					if (linkedObject is CusEntryHeader && ((CusEntryHeader)linkedObject).IsG7ExportDeclaration)
					{
						resultStatus = new G7ExportResponseMessageProcessor(Logger).DoProcessingReturningStatusForGenericSyntaxError(ediMessage);
					}
					else if (linkedObject is CusEntryNumber && ((CusEntryNumber)linkedObject).CE_ParentTable == CusSCAHouse.Schema.TableName)
					{
						resultStatus = new SupplementaryCargoReportResponseMessageProcessor(Logger).DoProcessingReturningStatusForGenericSyntaxError(ediMessage);
					}
				}
				else
				{
					throw new CouldNotFindLinkedObjectException(bgmReference, ediMessage, this);
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}

			return resultStatus;
		}

		BusinessObject GetLinkedObject(BusinessObjectFactory factory, string objectReference)
		{
			BusinessObject result = new Customs.Business.CusEntryHeader.Loader(factory).FindByEntryNumberAndCurrentCompany(objectReference);
			if (result == null)
			{
				ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, objectReference);
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
				query.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CusSCAHouse.Schema.TableName);
				result = factory.LoadTop1<CusEntryNumber>(query);
			}
			return result;
		}

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
			get { return linkedObject; }
		}

		#endregion

		BusinessObject linkedObject;
	}
}

// End to end test for this class is in each associated message processor
