using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;
using CUSDEC = Enterprise.Edifact.D99B.Messages.CUSDEC;
using CUSRES = Enterprise.Edifact.D99B.Messages.CUSRES;

namespace Enterprise.Customs.CA.Business
{
	class OverdueReleaseNoticeDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		internal OverdueReleaseNoticeDocumentWrapper(K84Message message)
			: base(message.Factory)
		{
			Argument.NotNull(message, "message");
			if (message.EM_MessageSubType != K84ReportTypes.Codes.Overdue)
			{
				throw new ArgumentException("OverdueReleaseNoticeDocumentWrapper is for OVR message only but was " + message.EM_MessageSubType);
			}
			this.message = message;
			var segmentGroup = message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			this.fCUSRESMessage = segmentGroup as CUSRES.CUSRESMessage;
			this.fCUSDECMessage = segmentGroup as CUSDEC.CUSDECMessage;
			if (fCUSRESMessage == null && fCUSDECMessage == null)
			{
				throw new ArgumentException("Supported EDIFACT message type is D99B CUSRES or CUSDEC");
			}
		}

		readonly K84Message message;
		readonly CUSRES.CUSRESMessage fCUSRESMessage;
		readonly CUSDEC.CUSDECMessage fCUSDECMessage;

		public ZDate CurrentDate
		{
			get { return D99BMessageUtilities.GetDate(fCUSRESMessage != null ? fCUSRESMessage.DTM : fCUSDECMessage.DTM, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime); }
		}

		public ZString AccountSecurityNumber
		{
			get { return D99BMessageUtilities.GetReference(fCUSRESMessage != null ? fCUSRESMessage.Group3[0].RFF : fCUSDECMessage.Group1[0].RFF, ReferenceFunctionCodeQualifierList.DeclarantsCustomsIdentityNumber); }
		}

		#region OverdueNotices

		public BusinessObjectCollectionWrapper<OverdueNotice> OverdueNotices
		{
			get
			{
				var result = new List<OverdueNotice>();

				if (fCUSRESMessage != null)
				{
					var groups = fCUSRESMessage.Group3.Cast<CUSRES.SegmentGroup3>().ToList();
					var groupsForOverdueNotice = new List<CUSRES.SegmentGroup3>();
					var shouldCreateNewOverdueNotice = false;
					var count = groups.Count;
					for (var i = AccountSecurityNumber.IsEmpty ? 0 : 1; i < count; i++)
					{
						groupsForOverdueNotice.Add(groups[i]);
						if (groups[i].DTM.Count > 0 || i == count - 1)
						{
							shouldCreateNewOverdueNotice = true;
						}
						if (shouldCreateNewOverdueNotice)
						{
							result.Add(new OverdueNotice(Factory, groupsForOverdueNotice.GetRange(0, groupsForOverdueNotice.Count), AccountSecurityNumber));
							shouldCreateNewOverdueNotice = false;
							groupsForOverdueNotice.Clear();
						}
					}
				}
				if (fCUSDECMessage != null)
				{
					var groups = fCUSDECMessage.Group1.Cast<CUSDEC.SegmentGroup1>().ToList();
					var groupsForOverdueNotice = new List<CUSDEC.SegmentGroup1>();
					var shouldCreateNewOverdueNotice = false;
					var count = groups.Count;
					for (var i = AccountSecurityNumber.IsEmpty ? 0 : 1; i < count; i++)
					{
						groupsForOverdueNotice.Add(groups[i]);
						if (groups[i].DTM.Count > 0 || i == count - 1)
						{
							shouldCreateNewOverdueNotice = true;
						}
						if (shouldCreateNewOverdueNotice)
						{
							result.Add(new OverdueNotice(Factory, groupsForOverdueNotice.GetRange(0, groupsForOverdueNotice.Count), AccountSecurityNumber));
							shouldCreateNewOverdueNotice = false;
							groupsForOverdueNotice.Clear();
						}
					}
				}
				return new BusinessObjectCollectionWrapper<OverdueNotice>(result);
			}
		}

		public ZGuid SourceIdentifier => throw new NotImplementedException();

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class OverdueNotice : NonPersistentBusinessObject, ITableInterpretation
		{
			internal OverdueNotice(BusinessObjectFactory factory, List<CUSRES.SegmentGroup3> groups, ZString accountSecurityNumber) : base(factory)
			{
				group3s = groups;
				this.accountSecurityNumber = accountSecurityNumber;
			}

			internal OverdueNotice(BusinessObjectFactory factory, List<CUSDEC.SegmentGroup1> groups, ZString accountSecurityNumber) : base(factory)
			{
				group1s = groups;
				this.accountSecurityNumber = accountSecurityNumber;
			}

			[ColumnName(1)]
			public ZString JobNumber
			{
				get { return Declaration != null ? EmailDefBuilder.GetJobLink(Declaration, Declaration.JE_DeclarationReference) : string.Empty; }
			}

			[ColumnName(5)]
			public ZString ReleaseOffice
			{
				get { return GetReferenceFromGroups(ReferenceFunctionCodeQualifierList.GovernmentAgencyReferenceNumber); }
			}

			[ColumnName(4)]
			public ZString Client
			{
				get
				{
					var result = ClientBusinessNumber;
					if (Declaration != null)
					{
						var importer = Declaration.Importer;
						if (importer != null)
						{
							result += string.Format(" ({0} - {1})", importer.OH_Code, importer.OH_FullNameTruncated);
						}
					}
					return result;
				}
			}

			public ZString ClientBusinessNumber
			{
				get { return GetReferenceFromGroups(ReferenceFunctionCodeQualifierList.NationalGovernmentBusinessIdentificationNumber); }
			}

			[ColumnName(2)]
			public ZString TransactionNumber
			{
				get { return GetReferenceFromGroups(ReferenceFunctionCodeQualifierList.TransactionReferenceNumber); }
			}

			[ColumnName(9)]
			public ZBool IsLVS
			{
				get { return GetReferenceIndicatorFromGroups(ReferenceFunctionCodeQualifierList.TransactionReferenceNumber); }
			}

			[ColumnName(3)]
			public ZString CargoControlNumber
			{
				get { return GetReferenceFromGroups(ReferenceFunctionCodeQualifierList.CargoManifestNumber); }
			}

			[ColumnName(7)]
			public ZInt AgeInDays
			{
				get { return ZInt.ParseEmptyAsZero(GetReferenceFromGroups(ReferenceFunctionCodeQualifierList.PostEntryReference)); }
			}

			[ColumnName(8)]
			public ZBool IsAQ
			{
				get { return GetReferenceIndicatorFromGroups(ReferenceFunctionCodeQualifierList.PostEntryReference); }
			}

			[ColumnName(6)]
			public ZDate ReleaseDate
			{
				get { return GetDateFromGroups(DateTimePeriodFunctionCodeQualifierList.ReleaseDateCustoms); }
			}

			public ZString ImporterCode
			{
				get { return Declaration != null && Declaration.Importer != null ? Declaration.Importer.OH_Code : ZString.Empty; }
			}

			public JobDeclaration Declaration
			{
				get
				{
					if (declaration == null)
					{
						declaration = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(Factory, accountSecurityNumber + TransactionNumber, ZString.Empty);
					}
					return declaration;
				}
			}
			JobDeclaration declaration;

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return string.Empty; }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<OverdueNotice>(); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { JobNumber, TransactionNumber, CargoControlNumber, Client, ReleaseOffice, ReleaseDate, AgeInDays, IsAQ, IsLVS }; }
			}

			#endregion

			readonly List<CUSRES.SegmentGroup3> group3s;
			readonly List<CUSDEC.SegmentGroup1> group1s;
			readonly ZString accountSecurityNumber;

			IEnumerable<RFFSegmentMessageSection> RFFSections
			{
				get
				{
					if (rFFSections == null)
					{
						rFFSections = group3s != null ? group3s.Select(x => x.RFF) : group1s.Select(x => x.RFF);
					}
					return rFFSections;
				}
			}
			IEnumerable<RFFSegmentMessageSection> rFFSections;

			IEnumerable<DTMSegmentMessageSection> DTMSections
			{
				get
				{
					if (dTMSections == null)
					{
						dTMSections = group3s != null ? group3s.Select(x => x.DTM) : group1s.Select(x => x.DTM);
					}
					return dTMSections;
				}
			}
			IEnumerable<DTMSegmentMessageSection> dTMSections;

			ZBool GetReferenceIndicatorFromGroups(ReferenceFunctionCodeQualifierList qualifier)
			{
				var result = false;
				foreach (RFFSegment rff in RFFSections.SelectMany(x => x.Cast<RFFSegment>()))
				{
					if (rff.Reference.ReferenceFunctionCodeQualifier == qualifier)
					{
						result = rff.Reference.LineNumber.Equals("Y", StringComparison.OrdinalIgnoreCase);
						break;
					}
				}
				return result;
			}

			ZString GetReferenceFromGroups(ReferenceFunctionCodeQualifierList qualifier)
			{
				var result = ZString.Empty;
				foreach (RFFSegment rff in RFFSections.SelectMany(x => x.Cast<RFFSegment>()))
				{
					if (rff.Reference.ReferenceFunctionCodeQualifier == qualifier)
					{
						result = rff.Reference.ReferenceIdentifier;
						break;
					}
				}
				return result;
			}

			ZDate GetDateFromGroups(DateTimePeriodFunctionCodeQualifierList qualifier)
			{
				var result = ZDate.Empty;
				foreach (DTMSegment dtm in DTMSections.SelectMany(x => x.Cast<DTMSegment>()))
				{
					if (dtm.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == qualifier)
					{
						result = D99BMessageUtilities.ParseDate(dtm.DateTimePeriod.DateTimePeriodValue);
						break;
					}
				}
				return result;
			}
		}

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.message.PK;
	}
}
