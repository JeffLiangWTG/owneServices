using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaEDIMessage : EDIMessage
	{
		public AsycudaEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				ZString result;
				result = SupportNoteMessageInterpretation()
					? MessageInterpretation.ST_NoteText
					: base.EM_MessageInterpretation;

				return result;
			}
			set => base.EM_MessageInterpretation = value;
		}

		public override ZString EM_MessageText
		{
			get => base.EM_MessageText;
			set
			{
				var oldValue = EM_MessageText;
				base.EM_MessageText = value;
				if (!IsCopying && oldValue != EM_MessageText && SupportNoteMessageInterpretation())
				{
					Notes.FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Code).DeleteAll();
					universalEvent = null;
					messageInterpretation = null;
				}
			}
		}

		protected UniversalEvent UniversalEvent
		{
			get
			{
				if (universalEvent == null && IsXmlUniversalEventData)
				{
					using (var reader = GetEM_MessageTextReader())
					{
						universalEvent = reader.Parse<UniversalEvent>();
					}
				}
				return universalEvent;
			}
		}
		UniversalEvent universalEvent;

		protected ZString ActionPurposeCode => UniversalEvent?.DataContext?.ActionPurposeCode ?? ZString.Empty;

		protected bool SupportNoteMessageInterpretation()
		{
			return IsXmlUniversalEventData && EM_ReceiveTransmit == EDIInterchange.Direction.Receive && SupportNoteMessageInterpretationCore();
		}

		bool IsXmlUniversalEventData => EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent && EM_MessageType == EDIMessageTypeList.Codes.XDC;

		protected virtual bool SupportNoteMessageInterpretationCore() => false;

		StmNote MessageInterpretation
		{
			get
			{
				if (object.ReferenceEquals(messageInterpretation, null) || messageInterpretation.IsDeleted) // Note: Do not use messageInterpretation == null as Factory.GetNull<StmNote>() == null will result true
				{
					messageInterpretation = Notes.FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Code).OrderBy(x => x.ST_CreatedDateUtc).FirstOrDefault();
					if (messageInterpretation == null)
					{
						var htmlGenerator = GetHTMLGenerator();
						if (htmlGenerator == null)
						{
							messageInterpretation = Factory.GetNull<StmNote>();
						}
						else
						{
							messageInterpretation = Factory.New<StmNote>();
							messageInterpretation.ST_ParentID = PK;
							messageInterpretation.ST_Table = EDIMessageSchema.Constants.TableName;
							messageInterpretation.ST_Description = PredefinedNoteTypes.Instance.MessageInterpretation.Code;
							messageInterpretation.ST_NoteText = htmlGenerator.GetInterpretedHTML();
							HasChanges = true; // Don't call factory save as we don't know what else it will save.
						}
					}
				}
				return messageInterpretation;
			}
		}
		StmNote messageInterpretation;

		AsycudaEventMessageInterpretationGenerator GetHTMLGenerator()
		{
			AsycudaEventMessageInterpretationGenerator result = null;
			var actionPurposeCode = ActionPurposeCode;
			if (!actionPurposeCode.IsEmpty)
			{
				result = GetNewAsycudaEventMessageInterpretationGeneratorCore(UniversalEvent);
			}
			return result;
		}

		protected virtual AsycudaEventMessageInterpretationGenerator GetNewAsycudaEventMessageInterpretationGeneratorCore(UniversalEvent messageUniversalEvent) => null;
	}
}
