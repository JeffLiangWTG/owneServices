using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D13A;
using Enterprise.Edifact.D13A.Messages.GOVCBR;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class UniversalEventMessageInterpretationGenerator
	{
		public UniversalEventMessageInterpretationGenerator(BusinessObjectFactory factory, EDIMessage message)
			: this(factory, message?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
		{
		}

		public UniversalEventMessageInterpretationGenerator(BusinessObjectFactory factory, UniversalEvent universalEvent)
		{
			this.factory = Argument.NotNull(factory, "factory");
			this.universalEvent = universalEvent;
		}
		protected BusinessObjectFactory factory;
		protected UniversalEvent universalEvent;

		protected List<Context> ContextCollection => universalEvent?.ContextCollection;

		#region Email Body

		public ZString GetInterpretatedHTML()
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };

			if (ContextCollection != null)
			{
				WriteDateAndReferences(result);
				TableInterpretation.AddTableInterpretationIfRequired(result, PGADetailsBase, null, TableInterpretation.Attributes.AlignLeft);
				TableInterpretation.AddTableInterpretationIfRequired(result, Statuses, null, TableInterpretation.Attributes.AlignLeft);
				WriteNoticeRecipientIfRequired(result);
				TableInterpretation.AddTableInterpretationIfRequired(result, CloseMessageHouseBillses, null, TableInterpretation.Attributes.AlignLeft);
				TableInterpretation.AddTableInterpretationIfRequired(result, RequestingPGAs, null, TableInterpretation.Attributes.AlignLeft);
				TableInterpretation.AddTableInterpretationIfRequired(result, ErrorDetails, null, TableInterpretation.Attributes.AlignLeft);
				WriteContainerDetailsIfRequired(result);
				WriteRawMessageIfRequired(result);
			}

			return result.ToHtml();
		}

		#region Date And References

		internal ZString EventType => AutoEvents.All[universalEvent.EventType.ToString()]?.Description;

		internal ZString DocumentType
		{
			get
			{
				var documentType = UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection, UniversalEventMessageProcessorConstants.ContextType.NoticeDocumentTypeCode);
				if (!documentType.IsEmpty)
				{
					var currentReponseSequence = UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection, UniversalEventMessageProcessorConstants.ContextType.CurrentReponseSequence);
					var totalResponsesCount = UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection, UniversalEventMessageProcessorConstants.ContextType.TotalResponsesCount);
					var responseSequenceString = !currentReponseSequence.IsEmpty && !totalResponsesCount.IsEmpty ? ZString.Format(" {0} of {1}", currentReponseSequence, totalResponsesCount) : ZString.Empty;
					return documentType + responseSequenceString;
				}
				return string.Empty;
			}
		}

		internal ZDateTime? ProcessingDate => universalEvent?.EventTime?.ToZDateTime();

		internal ZString SendersReference
		{
			get
			{
				return UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection,
					UniversalEventMessageProcessorConstants.ContextType.OrganizationReference);
			}
		}

		internal ZString ReferenceNumber => UniversalEventMessageProcessorHelper.GetUniqueIDValueFromDataTarget(universalEvent);

		internal BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.RelatedDocument> RelatedDocuments => UniversalEventMessageProcessorHelper.GetRelatedDocument(universalEvent);

		void WriteDateAndReferences(HtmlTableCreator table)
		{
			var caption = new HtmlTableCreator(
				new[] {
					Res.GetString("5bc9bb1d-1e02-4b05-a6f5-28d6a1b44c64", "DATE AND REFERENCES"),
				},
				TableInterpretation.Attributes.FullWidth
				)
			{ EnableHTMLEncoding = false };

			var subTable = new HtmlTableCreator((IEnumerable<string>)null, TableInterpretation.Attributes.FullWidth);
			subTable.WriteRow(new string[] { Res.GetString("8bae40cc-6c15-4f79-93f0-bfebd910a85d", "Event Type"), EventType });
			WriteDocumentTypeIfApplicable(subTable);
			subTable.WriteRow(new string[] { Res.GetString("8bc03c34-fc5c-4ef4-bef7-f699261737b7", "Processing Date"), ProcessingDate.ToString() });
			subTable.WriteRow(new string[] { Res.GetString("b2013350-6479-4bcf-b6ca-aa75ed14cc79", "Senders Reference"), SendersReference });
			subTable.WriteRow(new string[] { Res.GetString("3aeec6b9-2a51-49cb-9fb6-052085ef8cf4", "Reference Number"), ReferenceNumber });
			WriteRelatedDocumentIfApplicable(subTable);
			caption.WriteRow(new string[] { subTable.ToHtml() });

			table.WriteRow(caption.ToHtml());
		}

		void WriteDocumentTypeIfApplicable(HtmlTableCreator table)
		{
			if (!DocumentType.IsEmpty)
			{
				table.WriteRow(new string[] { Res.GetString("3cd45444-144d-423e-9fd3-c3df139a9514", "Document Type"), DocumentType });
			}
		}

		void WriteRelatedDocumentIfApplicable(HtmlTableCreator table)
		{
			if (RelatedDocuments.Any())
			{
				foreach (D4MessageInterpretationGenerator.RelatedDocument relatedDocument in RelatedDocuments)
				{
					table.WriteRow(Res.GetString("3f7601cf-a2f1-4445-94d7-f709b4f84cf4", "Related Document"), relatedDocument.DocumentNumber);
					var type = relatedDocument.DocumentType;
					if (!type.IsEmpty)
					{
						table.WriteRow(Res.GetString("593DC591-49E2-40BB-A509-0CF4FF3D6110", "Related Document Type"), type);
					}
				}
			}
		}

		#endregion

		#region EDI Message
		internal ZString SentRawMessage
		{
			get
			{
				return UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection,
					UniversalEventMessageProcessorConstants.ContextType.SentRawMessage);
			}
		}

		internal ZString ResponseRawMessage
		{
			get
			{
				return UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection,
					UniversalEventMessageProcessorConstants.ContextType.ResponseRawMessage);
			}
		}

		internal string ToHtml(string inputMessage)
		{
			var result = "";
			var edifactMessageWithCharSet = GetGOVCBRMessageWithCharSet(inputMessage);
			if (edifactMessageWithCharSet.EDIFactMessage != null)
			{
				var interpretation = new MessageInterpretation(edifactMessageWithCharSet.EDIFactMessage, edifactMessageWithCharSet.CharSet);
				AddInterpretationsTop(interpretation, edifactMessageWithCharSet.EDIFactMessage);
				result = interpretation.ToHtml();
			}
			return result;
		}

		internal (CACharSet CharSet, GOVCBRMessage EDIFactMessage) GetGOVCBRMessageWithCharSet(string inputMessage)
		{
			var characterSet = new CACharSet();
			var processedMessage = ProcessMessage(characterSet, inputMessage);
			return (characterSet, new D13AMessageFactory().GetMessage(characterSet, processedMessage) as GOVCBRMessage);
		}

		void AddInterpretationsTop(MessageInterpretation topInterpretation, GOVCBRMessage message)
		{
			var unh = message.UNH[0];
			topInterpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			var topLevelFields = message.GetType().GetFields().Where(field => field.Name != nameof(GOVCBRMessage.UNH) && field.Name != nameof(GOVCBRMessage.UNT));
			foreach (var field in topLevelFields)
			{
				var itemLv1s = (IEnumerable)field.GetValue(message);

				foreach (var itemLv1 in itemLv1s)
				{
					AddInterpretationsForTopItem(topInterpretation, itemLv1);
				}
			}

			var unt = message.UNT[0];
			topInterpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);
		}

		void AddInterpretationsForTopItem(MessageInterpretation interpretation, object item)
		{
			if (item is Segment segment)
			{
				AddInterpretationsForSegment(interpretation, segment);
			}
			else if (item is SegmentGroup segmentGroup)
			{
				AddInterpretationsForGroup(interpretation, segmentGroup);
			}
		}

		void AddInterpretationsForGroup(MessageInterpretation interpretation, SegmentGroup segmentGroup)
		{
			foreach (var field in GetSubItems(segmentGroup))
			{
				foreach (var itemLv1 in (IEnumerable)field.value)
				{
					ISegmentInterpretation subInterpretation = null;
					if (itemLv1 is SegmentGroup segmentGroup1)
					{
						AddInterpretationsForGroup(interpretation, segmentGroup1);
					}
					else
					{
						foreach (var itemLv2 in GetSubItems(itemLv1))
						{
							var itemLv2Value = itemLv2.value;
							if (itemLv2Value is Segment segment)
							{
								subInterpretation = AddInterpretationsForSegment(interpretation, segment);
							}
							else if (itemLv2Value is SegmentGroup subSegmentGroup)
							{
								AddInterpretationsForGroup(interpretation, subSegmentGroup);
							}
							else if (itemLv1 is Segment lv1Segment)
							{
								if (subInterpretation == null)
								{
									subInterpretation = interpretation.AddNewSegmentInterpretation(lv1Segment);
								}

								if (itemLv2Value is ValueBase valueBase)
								{
									AddInterpretationForValueBase(subInterpretation, valueBase);
								}
								else
								{
									AddInterpretationForBasicValue(subInterpretation, itemLv2.name, lv1Segment);
								}
							}
						}
					}
				}
			}
		}

		ISegmentInterpretation AddInterpretationsForSegment(MessageInterpretation interpretation, Segment segment)
		{
			var subInterpretation = interpretation.AddNewSegmentInterpretation(segment);

			foreach (var segmentValue in GetSubItems(segment))
			{
				if (segmentValue.value is ValueBase segmentValueBase)
				{
					AddInterpretationForValueBase(subInterpretation, segmentValueBase);
				}
				else
				{
					AddInterpretationForBasicValue(subInterpretation, segmentValue.name, segment);
				}
			}
			return subInterpretation;
		}

		void AddInterpretationForValueBase(ISegmentInterpretation subInterpretation, ValueBase valueBase)
		{
			foreach (var valueField in GetSubItems(valueBase))
			{
				AddInterpretationForBasicValue(subInterpretation, valueField.name, valueBase);
			}
		}

		void AddInterpretationForBasicValue(ISegmentInterpretation subInterpretation, string fieldName, object parent)
		{
			var constant = Expression.Constant(parent);
			var field = Expression.Field(constant, fieldName);
			var lambda = Expression.Lambda<Func<object>>(field);

			if (!string.IsNullOrWhiteSpace(lambda.Compile().Invoke().ToString()))
			{
				subInterpretation.AddElementInterpretation(lambda);
			}
		}

		IEnumerable<(string name, object value)> GetSubItems(object item)
		{
			foreach (var field in item.GetType().GetFields())
			{
				yield return (field.Name, field.GetValue(item));
			}
		}

		string ProcessMessage(UNCharacterSet characterSet, string inputMessage)
		{
			var result = "";
			var replaced = inputMessage.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty);
			var segments = replaced.Split(characterSet.SegmentDelimiter.ToCharArray()).ToList();

			var unhIndex = 0;
			foreach (var segment in segments)
			{
				if (segment.StartsWith("UNH", StringComparison.Ordinal))
				{
					break;
				}
				unhIndex++;
			}
			for (int i = 0; i < unhIndex; i++)
			{
				segments.RemoveAt(0);
			}

			for (int endIndex = segments.Count; endIndex > 0; endIndex--)
			{
				if (!segments[endIndex - 1].StartsWith("UNE", StringComparison.Ordinal) && !segments[endIndex - 1].StartsWith("UNZ", StringComparison.Ordinal) && !segments[endIndex - 1].IsNullOrEmpty())
				{
					break;
				}
				else
				{
					segments.RemoveAt(endIndex - 1);
				}
			}

			foreach (var segment in segments)
			{
				result += segment + characterSet.SegmentDelimiter;
			}
			return result.TrimEnd(characterSet.SegmentDelimiter.ToCharArray());
		}

		#endregion

		#region PGA Details

		protected IEnumerable<PGADetailBase> PGADetailsBase
		{
			get
			{
				return
					ContextCollection?.Where(
						o =>
							o.Type != null &&
							o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.Name)
						.Select(CreatePGADetail) ?? Enumerable.Empty<PGADetailBase>();
			}
		}

		internal IEnumerable<PGADetail> PGADetails => PGADetailsBase.Cast<PGADetail>();

		PGADetailBase CreatePGADetail(Context context)
		{
			var pgaDetail = CreatePGADetailCore(context);
			pgaDetail.PopulatePropertyValues();
			return pgaDetail;
		}

		protected virtual PGADetailBase CreatePGADetailCore(Context context)
		{
			return new PGADetail(context, factory);
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public abstract class PGADetailBase : NonPersistentBusinessObject, ITableInterpretation
		{
			protected PGADetailBase(Context context, BusinessObjectFactory factory)
			{
				this.context = Argument.NotNull(context, "context");
				this.factory = Argument.NotNull(factory, "factory");
			}
			protected readonly Context context;
			protected BusinessObjectFactory factory;

			public void PopulatePropertyValues()
			{
				PGA = PGACodes.GetPGACodeFromGovAgencyID(context.Value.GetValueOrDefault());
				Name = factory.GetCachedValue<PGACodes>().GetDescriptionFromCode(PGA);
				var subContextCollection = context.SubContextCollection;
				if (subContextCollection != null)
				{
					PopulatePropertyValuesCore(subContextCollection);
				}
			}

			protected abstract void PopulatePropertyValuesCore(List<Context> subContextCollection);

			#region Properties

			[ColumnName(1)]
			public ZString PGA { get; private set; }

			[ColumnName(2)]
			public ZString Name { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption => Cation;

			protected virtual string Cation => Res.GetString("3c5f0dbf-9f0e-4e09-92d2-ecb929097362", "PGA DETAILS");

			IEnumerable<string> ITableInterpretation.Titles => Titles;

			protected virtual IEnumerable<string> Titles => PropertyNameProvider.GetColumnTitles<PGADetail>();

			IEnumerable<object> ITableValues.Values => Values;

			protected virtual IEnumerable<object> Values => new object[]
						{
							PGA,
							Name,
						};

			#endregion
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class PGADetail : PGADetailBase
		{
			public PGADetail(Context context, BusinessObjectFactory factory) : base(context, factory)
			{
			}

			protected override void PopulatePropertyValuesCore(List<Context> subContextCollection)
			{
				foreach (var subContext in subContextCollection)
				{
					var subContextType = subContext.Type;
					if (subContextType != null)
					{
						switch (subContextType.Type.GetValueOrDefault())
						{
							case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.InspectionPort:
								Port = subContext.Value.GetValueOrDefault();
								break;
							case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.InspectionWarehouse:
								WH = subContext.Value.GetValueOrDefault();
								break;
							case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.InspectionLocationOther:
								Site = subContext.Value.GetValueOrDefault();
								break;
							default:
								break;
						}
					}
				}

				Contact = GetContactDetails(subContextCollection);
			}

			#region Properties

			[ColumnName(3)]
			public ZString Port { get; private set; }

			[ColumnName(4, "W/H")]
			public ZString WH { get; private set; }

			[ColumnName(5)]
			public ZString Site { get; private set; }

			[ColumnName(6)]
			public ZString Contact { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			protected override string Cation => Res.GetString("3c5f0dbf-9f0e-4e09-92d2-ecb929097362", "PGA DETAILS");

			protected override IEnumerable<string> Titles => PropertyNameProvider.GetColumnTitles<PGADetail>();

			protected override IEnumerable<object> Values => new object[]
						{
							PGA,
							Name,
							Port,
							WH,
							Site,
							Contact
						};

			#endregion

			ZString GetContactDetails(List<Context> contextCollection)
			{
				var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };
				TableInterpretation.AddTableInterpretationIfRequired(result,
					contextCollection.Where(o => o.Type != null && o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.PGAContact.Name).Select(o => new ContactDetail(o)));
				return result.ToHtml();
			}

			[TestExcludeBusinessObjectsAllHaveTestCases]
			class ContactDetail : ITableInterpretation
			{
				internal ContactDetail(Context context)
				{
					this.context = Argument.NotNull(context, "context");
					PopulatePropertyValues();
				}
				readonly Context context;
				void PopulatePropertyValues()
				{
					Name = context.Value.GetValueOrDefault();
					var subContextCollection = context.SubContextCollection;
					if (subContextCollection != null)
					{
						foreach (var subContext in subContextCollection)
						{
							var subContextType = subContext.Type;
							if (subContextType != null)
							{
								switch (subContextType.Type.GetValueOrDefault())
								{
									case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.PGAContact.SubContextType.PGAContactPhone:
										Phone = subContext.Value.GetValueOrDefault();
										break;
									case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.PGAContact.SubContextType.PGAContactEmail:
										Email = subContext.Value.GetValueOrDefault();
										break;
									case UniversalEventMessageProcessorConstants.ContextType.InspectionPGA.SubContextType.PGAContact.SubContextType.PGAContactFax:
										Fax = subContext.Value.GetValueOrDefault();
										break;
									default:
										break;
								}
							}
						}
					}
				}

				#region Properties

				[ColumnName(1)]
				public ZString Name { get; private set; }
				[ColumnName(2)]
				public ZString Phone { get; private set; }

				[ColumnName(3)]
				public ZString Email { get; private set; }

				[ColumnName(4)]
				public ZString Fax { get; private set; }

				#endregion

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return ZString.Empty; }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get { return PropertyNameProvider.GetColumnTitles<ContactDetail>(); }
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						return new object[]
						{
							Name,
							Phone,
							Email,
							Fax
						};
					}
				}

				#endregion

			}
		}

		#endregion

		#region STATUS

		internal IEnumerable<Status> Statuses
		{
			get
			{
				return
					ContextCollection?.Where(
						o =>
							o.Type != null &&
							o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.Status.Name)
						.Select(o => new Status(o, factory)) ?? Enumerable.Empty<Status>();
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class Status : NonPersistentBusinessObject, ITableInterpretation
		{
			internal Status(Context context, BusinessObjectFactory factory)
			{
				this.context = Argument.NotNull(context, "context");
				this.factory = Argument.NotNull(factory, "factory");
				PopulatePropertyValues();
			}
			readonly Context context;
			readonly BusinessObjectFactory factory;

			void PopulatePropertyValues()
			{
				Code = context.Value.GetValueOrDefault();
				Description = CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(factory, Code);
			}

			#region Properties

			[ColumnName(1, "Status Code")]
			public ZString Code { get; private set; }

			[ColumnName(2, "Status Description")]
			public ZString Description { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("3564ae6c-54f0-4b0b-9e38-5b291db993ae", "STATUS"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<Status>(); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					return new object[]
						{
							Code,
							Description,
						};
				}
			}

			#endregion
		}

		#endregion

		#region NOTICE RECIPENT

		internal ZString NoticeRecipientType
		{
			get
			{
				if (ContextCollection != null)
				{
					return UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection, UniversalEventMessageProcessorConstants.ContextType.NoticeRecipientType);
				}
				return string.Empty;
			}
		}

		internal ZString NoticeRecipientReferenceNumber
		{
			get
			{
				if (ContextCollection != null)
				{
					return UniversalEventMessageProcessorHelper.GetContextValueByType(ContextCollection, UniversalEventMessageProcessorConstants.ContextType.NoticeRecipientReferenceNumber);
				}
				return string.Empty;
			}
		}

		void WriteNoticeRecipientIfRequired(HtmlTableCreator table)
		{
			if (!NoticeRecipientType.IsEmpty || !NoticeRecipientReferenceNumber.IsEmpty)
			{
				var caption = new HtmlTableCreator(
					new[] {
						Res.GetString("0c14b089-391e-4cd9-9667-5d34a2cac7d7", "NOTICE RECIPENT"),
					},
					TableInterpretation.Attributes.FullWidth
					)
				{ EnableHTMLEncoding = false };

				var subTable = new HtmlTableCreator((IEnumerable<string>)null, TableInterpretation.Attributes.FullWidth);
				subTable.WriteRow(new string[] {
					NoticeRecipientType.IsEmpty ? string.Empty : Res.GetString("a9bb437b-6aad-4ea8-9e1a-eacf433add23", "Type:") + NoticeRecipientType,
					NoticeRecipientReferenceNumber.IsEmpty ? string.Empty : Res.GetString("4c4a7b74-3c37-4e18-8a04-4620f9d65712", "Number:") + NoticeRecipientReferenceNumber
				});
				caption.WriteRow(new string[] { subTable.ToHtml() });
				table.WriteRow(caption.ToHtml());
			}
		}

		#endregion

		#region Close Message House Bills

		internal IEnumerable<CloseMessageHouseBills> CloseMessageHouseBillses
		{
			get
			{
				return
					ContextCollection?.Where(
						o =>
							o.Type != null &&
							o.Type.Type.GetValueOrDefault() ==
							UniversalEventMessageProcessorConstants.ContextType.CloseMessageHouseBills.Name)
						.Select(o => new CloseMessageHouseBills(o)) ?? Enumerable.Empty<CloseMessageHouseBills>();
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class CloseMessageHouseBills : NonPersistentBusinessObject, ITableInterpretation
		{
			internal CloseMessageHouseBills(Context context)
			{
				this.context = Argument.NotNull(context, "context");
				PopulatePropertyValues();
			}
			readonly Context context;

			void PopulatePropertyValues()
			{
				var subContextCollection = context.SubContextCollection;
				if (subContextCollection != null)
				{
					foreach (var subContext in subContextCollection)
					{
						var subContextType = subContext.Type;
						if (subContextType != null)
						{
							switch (subContextType.Type.GetValueOrDefault())
							{
								case UniversalEventMessageProcessorConstants.ContextType.CloseMessageHouseBills.SubContextType.DocumentID:
									DocumentID = subContext.Value.GetValueOrDefault();
									break;
								case UniversalEventMessageProcessorConstants.ContextType.CloseMessageHouseBills.SubContextType.DocumentType:
									DocumentType = subContext.Value.GetValueOrDefault();
									break;
								default:
									break;
							}
						}
					}
				}
			}

			#region Properties

			[ColumnName(1, "Document ID")]
			public ZString DocumentID { get; private set; }

			[ColumnName(2, "Document Type")]
			public ZString DocumentType { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("f7d75a46-534d-4639-8d87-9475dcc305d8", "Close Message House Bills"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<CloseMessageHouseBills>(); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					return new object[]
						{
							DocumentID,
							DocumentType,
						};
				}
			}

			#endregion
		}

		#endregion

		#region REQUEST FOR INFORMATION/MANUAL ERRORS

		internal IEnumerable<RequestingPGA> RequestingPGAs
		{
			get
			{
				return
					ContextCollection?.Where(
						o =>
							o.Type != null &&
							o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.RequestingPGA.Name)
						.Select(o => new RequestingPGA(o, factory)) ?? Enumerable.Empty<RequestingPGA>();
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class RequestingPGA : NonPersistentBusinessObject, ITableInterpretation
		{
			internal RequestingPGA(Context context, BusinessObjectFactory factory)
			{
				this.context = Argument.NotNull(context, "context");
				this.factory = Argument.NotNull(factory, "factory");
				PopulatePropertyValues();
			}
			readonly Context context;
			readonly BusinessObjectFactory factory;

			void PopulatePropertyValues()
			{
				PGA = PGACodes.GetPGACodeFromGovAgencyID(context.Value.GetValueOrDefault());
				Name = factory.GetCachedValue<PGACodes>().GetDescriptionFromCode(PGA);
				var subContextCollection = context.SubContextCollection;
				if (subContextCollection != null)
				{
					var seperator = System.Environment.NewLine;
					foreach (var subContext in subContextCollection)
					{
						var subContextType = subContext.Type;
						if (subContextType != null)
						{
							switch (subContextType.Type.GetValueOrDefault())
							{
								case UniversalEventMessageProcessorConstants.ContextType.RequestingPGA.SubContextType.RequestingReviewComments:
								case UniversalEventMessageProcessorConstants.ContextType.RequestingPGA.SubContextType.RequestingSpecialInstructions:
									CommentsSpecialInstructions += subContext.Value.GetValueOrDefault() + seperator;
									break;
								case UniversalEventMessageProcessorConstants.ContextType.RequestingPGA.SubContextType.RequestingErrorDescription:
									Errors += subContext.Value.GetValueOrDefault() + seperator;
									break;
								default:
									break;
							}
						}
					}
					var sepLen = seperator.Length;
					if (CommentsSpecialInstructions.Length > sepLen)
					{
						CommentsSpecialInstructions = CommentsSpecialInstructions.Remove(CommentsSpecialInstructions.Length - sepLen, sepLen);
					}
					if (Errors.Length > sepLen)
					{
						Errors = Errors.Remove(Errors.Length - sepLen, sepLen);
					}
				}
			}

			#region Properties

			[ColumnName(1)]
			public ZString PGA { get; private set; }

			[ColumnName(2)]
			public ZString Name { get; private set; }

			[ColumnName(3, "Comments/Special Instructions")]
			public ZString CommentsSpecialInstructions { get; private set; }

			[ColumnName(4)]
			public ZString Errors { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("7f9334ba-ecd3-414c-a761-667d6ef8a6d6", "REQUEST FOR INFORMATION/MANUAL ERRORS"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<RequestingPGA>(); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					return new object[]
						{
							PGA,
							Name,
							CommentsSpecialInstructions,
							Errors
						};
				}
			}

			#endregion
		}

		#endregion

		#region Error Details

		internal IEnumerable<ErrorDetail> ErrorDetails
		{
			get
			{
				return
					ContextCollection?.Where(
						o =>
							o.Type != null &&
							o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.ErrorDetail.Name)
						.Select(o => new ErrorDetail(o, factory)) ?? Enumerable.Empty<ErrorDetail>();
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class ErrorDetail : NonPersistentBusinessObject, ITableInterpretation
		{
			internal ErrorDetail(Context context, BusinessObjectFactory factory)
			{
				this.context = Argument.NotNull(context, "context");
				this.factory = Argument.NotNull(factory, "factory");
				PopulatePropertyValues();
			}
			readonly Context context;
			readonly BusinessObjectFactory factory;

			void PopulatePropertyValues()
			{
				Code = context.Value.GetValueOrDefault();
				EnglishDescription = ErrorDescriptionHelper.GetDescriptionFromCode(this.factory, Code);
				FrenchDescription = ErrorDescriptionHelper.GetFrenchDescriptionFromCode(this.factory, Code);
				var subContextCollection = context.SubContextCollection;
				if (subContextCollection != null)
				{
					foreach (var subContext in subContextCollection)
					{
						var subContextType = subContext.Type;
						if (subContextType != null)
						{
							switch (subContextType.Type.GetValueOrDefault())
							{
								case UniversalEventMessageProcessorConstants.ContextType.ErrorDetail.SubContextType.ErrorDescription:
									Text = subContext.Value.GetValueOrDefault();
									break;
								case UniversalEventMessageProcessorConstants.ContextType.ErrorDetail.SubContextType.ErrorLocation:
									Location = subContext.Value.GetValueOrDefault();
									break;
								default:
									break;
							}
						}
					}
				}
			}

			CAErrorCodesDescriptionHelper ErrorDescriptionHelper
			{
				get
				{
					if (errorDescriptionHelper == null)
					{
						errorDescriptionHelper = new CAErrorCodesDescriptionHelper();
					}
					return errorDescriptionHelper;
				}
			}
			CAErrorCodesDescriptionHelper errorDescriptionHelper;

			#region Properties

			[ColumnName(1)]
			public ZString Code { get; private set; }

			[ColumnName(2)]
			public ZString EnglishDescription { get; private set; }

			[ColumnName(3)]
			public ZString FrenchDescription { get; private set; }

			[ColumnName(4)]
			public ZString Text { get; private set; }

			[ColumnName(5)]
			public ZString Location { get; private set; }

			#endregion

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("2765108a-fc9d-4ce1-837d-06c03fcc2287", "Error Details"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<ErrorDetail>(); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					return new object[]
						{
							Code,
							EnglishDescription,
							FrenchDescription,
							Text,
							Location
						};
				}
			}

			#endregion
		}

		#endregion

		#region Container Details

		internal ZString ContainerString
		{
			get
			{
				if (ContextCollection != null)
				{
					return string.Join(", ", ContextCollection.Where(o => o.Type != null && o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.ContainerDetails.Name)
						.SelectMany(o => o.SubContextCollection).Where(o => o.Type != null && o.Type.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.ContainerDetails.SubContextType.ContainerNumber)
						.Select(o => o.Value.GetValueOrDefault()));
				}
				return ZString.Empty;
			}
		}

		void WriteContainerDetailsIfRequired(HtmlTableCreator table)
		{
			if (!ContainerString.IsEmpty)
			{
				var caption = new HtmlTableCreator(
					new[] {
					Res.GetString("ff05778e-253e-4327-a9ab-6552722aa08a", "Container Details"),
				},
					TableInterpretation.Attributes.FullWidth
				)
				{ EnableHTMLEncoding = false };

				var subTable = new HtmlTableCreator((IEnumerable<string>)null, TableInterpretation.Attributes.FullWidth);
				subTable.WriteRow(new string[] { Res.GetString("c7c04dcc-ba3f-40c5-a6ae-669a8b8cfb4a", "Containers"), ContainerString });
				caption.WriteRow(new string[] { subTable.ToHtml() });
				table.WriteRow(caption.ToHtml());
			}
		}

		#endregion

		#region Raw Message

		internal ZString RawMessage
		{
			get
			{
				var result = ZString.Empty;
				var contextCollection = ContextCollection;
				if (contextCollection != null)
				{
					var messageNumber = UniversalEventMessageProcessorHelper.GetContextValueByType(contextCollection, UniversalEventMessageProcessorConstants.ContextType.MessageNumber);
					var rawMessage = UniversalEventMessageProcessorHelper.GetContextValueByType(contextCollection, UniversalEventMessageProcessorConstants.ContextType.RawMessage);
					if (!messageNumber.IsEmpty && !rawMessage.IsEmpty)
					{
						var pattern = ZString.Format(@"UNH\+{0}.+?UNT\+.+?'", messageNumber);
						var match = Regex.Match(rawMessage, pattern);
						if (match.Success)
						{
							result = Regex.Replace(match.Value, @"(?<!\?)'", "'\r\n");
						}
					}
				}
				return result;
			}
		}

		void WriteRawMessageIfRequired(HtmlTableCreator table)
		{
			if (!RawMessage.IsEmpty)
			{
				var caption = new HtmlTableCreator(
					new[] {
					Res.GetString("f4d692be-8ab5-4d25-a8b9-74476c9d3f20", "Raw Message"),
				},
					TableInterpretation.Attributes.FullWidth
				)
				{ EnableHTMLEncoding = false };

				caption.WriteRow(RawMessage);
				table.WriteRow(caption.ToHtml());
			}
		}

		#endregion

		#endregion

	}
}
