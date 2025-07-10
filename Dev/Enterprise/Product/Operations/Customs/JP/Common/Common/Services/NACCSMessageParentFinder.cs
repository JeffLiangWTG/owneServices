using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Subject;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public sealed class NACCSMessageParentFinder : IDisposable
	{
		public NACCSMessageParentFinder()
		{
			cachedParents = [];
		}

		readonly Dictionary<string, IEnumerable<BusinessObject>> cachedParents;

		public BusinessObject FindParentFromHubID(BusinessObjectFactory factory, string hubID)
		{
			return GetOrAddParent(factory, EDIInterchangeSchema.Constants.Prefix, hubID, LoadFromHubID);
		}

		public BusinessObject FindParentFromMessageData(BusinessObjectFactory factory, byte[] messageData)
		{
			BusinessObject result = null;

			if (TryParseInboundHeader(factory, messageData, out var inboundMessageHeader))
			{
				result = FindMessageParents(factory, inboundMessageHeader).FirstOrDefault(c => c != null);
			}

			return result;
		}

		public BusinessObject FindParentFromInputReference(BusinessObjectFactory factory, string reference)
		{
			return GetOrAddParent(factory, CusEntryHeaderSchema.Constants.Prefix, reference, LoadEntryHeader)
				?? GetOrAddParent(factory, AsycudaManifestHeaderSchema.Constants.Prefix, reference, LoadManifestHeader);
		}

		#region FindParentFromSubject

		public BusinessObject FindParentFromSubject(BusinessObjectFactory factory, byte[] messageData)
		{
			var results = Enumerable.Empty<BusinessObject>();

			if (TryParseInboundHeader(factory, messageData, out var inboundMessageHeader))
			{
				if (TryParseSubject(factory, messageData, inboundMessageHeader.OutputInformationCode, out var dataProvider))
				{
					results = LoadFromSubject(factory, dataProvider);
				}

				var userCode = inboundMessageHeader.UserCode;
				var userMailAddress = inboundMessageHeader.UserMailAddress;

				if (!string.IsNullOrWhiteSpace(userCode) || !string.IsNullOrWhiteSpace(userMailAddress))
				{
					results = results.Where(x => IsMatchedUserCodeAndUserMailAddress(factory, x, userCode, userMailAddress));
				}
			}

			var result = results.OrderBy(x => GetLastestMessage(x)?.EM_SystemCreateTimeUtc).LastOrDefault();

			return result;
		}

		Messaging.Business.EDIMessage GetLastestMessage(BusinessObject bizo)
		{
			if (bizo is AsycudaManifestHeader manifestHeader)
			{
				return GetLastestMessage(manifestHeader.Messages);
			}
			else if (bizo is CusEntryHeader entryHeader)
			{
				return GetLastestMessage(entryHeader.Messages);
			}

			return null;
		}

		Messaging.Business.EDIMessage GetLastestMessage(Messaging.Business.EDIMessageCollection messages)
		{
			return messages?.OrderBy(x => x.EM_SystemCreateTimeUtc)?.LastOrDefault();
		}

		Messaging.Business.EDIMessage GetLastestMessage(EDIMessageCollectionNonDependent messages)
		{
			return messages.OrderBy(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		bool IsMatchedUserCodeAndUserMailAddress(BusinessObjectFactory factory, BusinessObject bizo, string userCode, string userMailAddress)
		{
			var company = GetCompanyFromBizo(bizo);
			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company as GlbCompany);
			var credential = wrapper?.MailboxCredential;

			if (credential?.FullMailBoxAddress.EqualsIgnoringCase(userMailAddress) ?? false)
			{
				var org = factory.Load<OrgHeader>(company.GC_OH_OrgProxy);
				if (org?.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(OrgCusCode.JapanCodeTypes.NUC, Core.Constants.CountryCodes.Japan).Any(x => x.OK_CustomsRegNo.EqualsIgnoringCase(userCode)) ?? false)
				{
					return true;
				}
			}

			return false;
		}

		IGlbCompany GetCompanyFromBizo(BusinessObject bizo)
		{
			if (bizo is IWorkflowTriggerEventSource source)
			{
				return source.JobHeaderCompany;
			}
			else if (bizo is AsycudaContainer container)
			{
				return container.Header?.JobHeaderCompany;
			}

			return null;
		}

		IEnumerable<BusinessObject> LoadFromSubject(BusinessObjectFactory factory, IJPInboundMessageDataProvider dataProvider)
		{
			return dataProvider switch
			{
				IDeclarationNumberSubject subject => GetOrAddParents(factory, nameof(IDeclarationNumberSubject.DeclarationNumber), subject.DeclarationNumber, LoadFromEntryNumber),
				IExportControlNumberSubject subject => GetOrAddParents(factory, nameof(IExportControlNumberSubject.ExportControlNumber), subject.ExportControlNumber, LoadFromExportControlNumber),
				IDeclarationNumberAndAWBNumberSubject subject => GetOrAddParents(factory, nameof(IDeclarationNumberAndAWBNumberSubject.DeclarationNumber), subject.DeclarationNumber, LoadFromEntryNumber),
				IAWBNumberSubject subject => GetOrAddParents(factory, nameof(IAWBNumberSubject.AWBNumber), subject.AWBNumber, LoadFromAWBNumber),
				_ => [],
			};
		}

		IEnumerable<BusinessObject> LoadFromEntryNumber(BusinessObjectFactory factory, string entryNumber)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export });
			query.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, true);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);

			return factory.Load<CusEntryNumber>(query).Select(x => x.Parent);
		}

		IEnumerable<BusinessObject> LoadFromExportControlNumber(BusinessObjectFactory factory, string exportControlNumber)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, exportControlNumber);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.JP.ExportControlNumber);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryInstructionSchema.Constants.TableName);

			var parents = factory.Load<CusEntryNumber>(query).Select(x => x.Parent);
			return parents.Where(x => x is CusEntryInstruction).Cast<CusEntryInstruction>().Select(x => x.EntryHeader);
		}

		IEnumerable<BusinessObject> LoadFromAWBNumber(BusinessObjectFactory factory, string awbNumber)
		{
			var manifestHeaderQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.Japan);
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_TransportMode, Core.Constants.TransportModes.Air);

			var masterbillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			masterbillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			masterbillQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, awbNumber);
			manifestHeaderQuery.AddSubQuery(masterbillQuery, JoinCondition.And);

			return factory.Load<AsycudaManifestHeader>(manifestHeaderQuery);
		}

		#endregion

		void IDisposable.Dispose()
		{
			cachedParents.Clear();
		}

		#region Implement

		IEnumerable<BusinessObject> FindMessageParents(BusinessObjectFactory factory, IJPInboundMessageHeader inboundMessageHeader)
		{
			yield return GetOrAddParent(factory, EDIMessageSchema.Constants.Prefix, inboundMessageHeader.MessageTag, LoadFromMessage);
			yield return GetOrAddParent(factory, CusEntryHeaderSchema.Constants.Prefix, inboundMessageHeader.InputReference, LoadEntryHeader);
			yield return GetOrAddParent(factory, AsycudaManifestHeaderSchema.Constants.Prefix, inboundMessageHeader.InputReference, LoadManifestHeader);
		}

		BusinessObject GetOrAddParent(BusinessObjectFactory factory, string prefix, string reference, Func<BusinessObjectFactory, string, BusinessObject> findParentFunc)
		{
			return GetOrAddParents(factory, prefix, reference, (factory, reference) => [findParentFunc(factory, reference)]).FirstOrDefault();
		}

		IEnumerable<BusinessObject> GetOrAddParents(BusinessObjectFactory factory, string prefix, string reference, Func<BusinessObjectFactory, string, IEnumerable<BusinessObject>> findParentsFunc)
		{
			var result = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrWhiteSpace(reference))
			{
				var key = prefix + reference;

				if (!cachedParents.TryGetValue(key, out result))
				{
					result = findParentsFunc(factory, reference)?.Where(c => c != null) ?? [];
					cachedParents[key] = result;
				}

				if (LastCacheUpdateTime.IsEmpty)
				{
					LastCacheUpdateTime = ZDateTime.Now;
				}
				else if ((ZDateTime.Now - LastCacheUpdateTime).TotalHours > 24)
				{
					cachedParents.Clear();
					cachedParents[key] = result;

					LastCacheUpdateTime = ZDateTime.Now;
				}
			}

			return result;
		}

		ZDateTime LastCacheUpdateTime { get; set; } = ZDateTime.Empty;

		BusinessObject LoadFromMessage(BusinessObjectFactory factory, string reference)
		{
			var branchPks = LoadAllActiveBranchPks(factory);

			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.JPCustoms);
			query.AddToFilter(EDIMessageSchema.EM_MessageNum, reference);
			query.AddToFilter(EDIMessageSchema.EM_GB, branchPks);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;

			return factory.LoadTop1<EDIMessage>(query)?.EM_LinkedObject;
		}

		BusinessObject LoadEntryHeader(BusinessObjectFactory factory, string reference)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, reference);
			query.AddToFilter(CusEntryHeaderSchema.CH_DataModel, Core.Constants.CountryCodes.Japan);
			query.OrderBy = CusEntryHeaderSchema.Constants.CH_SystemCreateTimeUtc + OrderByClause.Descending;

			return factory.LoadTop1<CusEntryHeader>(query);
		}

		BusinessObject LoadManifestHeader(BusinessObjectFactory factory, string reference)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);
			query.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, false);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.JP.InputReference);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeaderSchema.Constants.TableName);
			query.OrderBy = CusEntryNumSchema.Constants.CE_SystemCreateTimeUtc + OrderByClause.Descending;

			return factory.LoadTop1<CusEntryNumber>(query)?.Parent;
		}

		BusinessObject LoadFromHubID(BusinessObjectFactory factory, string hubID)
		{
			var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.JPCustoms);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchange.TransportType.xT);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, ZGuid.ParseSafe(hubID));

			var branchPks = LoadAllActiveBranchPks(factory);

			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.JPCustoms);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_GB, branchPks);
			query.AddSubQuery(EDIMessageSchema.EM_EI, interchangeSubQuery, JoinCondition.And);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;

			return factory.LoadTop1<EDIMessage>(query)?.EM_LinkedObject;
		}

		ZGuid[] LoadAllActiveBranchPks(BusinessObjectFactory factory)
		{
			var loader = new GlbBranch.Loader(factory);
			var branches = loader.LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.Japan);
			return branches.Select(c => c.PK).ToArray();
		}

		bool TryParseInboundHeader(BusinessObjectFactory factory, byte[] messageData, out IJPInboundMessageHeader inboundMessageHeader)
		{
			try
			{
				inboundMessageHeader = JPMessageUtils.ParseInboundHeaderOnly(NACCSFactoryService.GetMessageFlatParser(factory), messageData);
			}
			catch (Exception e)
			{
				ParseErrorMessages = e.Message;
				inboundMessageHeader = null;
			}

			return inboundMessageHeader != null;
		}

		bool TryParseSubject(BusinessObjectFactory factory, byte[] messageData, string outputInformationCode, out IJPInboundMessageDataProvider dataProvider)
		{
			try
			{
				dataProvider = JPMessageUtils.ParseSubject(NACCSFactoryService.GetInboundMessageParser(factory), messageData, outputInformationCode);
			}
			catch (Exception e)
			{
				ParseErrorMessages = e.Message;
				dataProvider = null;
			}

			return dataProvider != null;
		}

		public void ClearErrorMessage()
		{
			ParseErrorMessages = string.Empty;
		}

		public string ParseErrorMessages { get; private set; }

		#endregion
	}
}
