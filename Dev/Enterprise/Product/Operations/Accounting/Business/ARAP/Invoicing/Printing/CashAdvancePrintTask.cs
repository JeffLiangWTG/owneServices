using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public class CashAdvancePrintTask : IDisposable
	{
		readonly IEnumerable<AccCashAdvanceRequestHeader> cashAdvanceRequestsToPrint;
		bool haveTransactionsBeenUpdatedAsPrinted;

		public CashAdvancePrintTask(IEnumerable<AccCashAdvanceRequestHeader> requestsToPrint)
		{
			this.cashAdvanceRequestsToPrint = requestsToPrint;
			haveTransactionsBeenUpdatedAsPrinted = false;
			var requests = requestsToPrint.ToArray();
			requestsGroupedByOrg = new Dictionary<string, List<AccCashAdvanceRequestHeader>>();
			AddRequests(requests);
			CreatePrintTask(requests);
		}

		internal readonly Dictionary<string, List<AccCashAdvanceRequestHeader>> requestsGroupedByOrg;
		protected bool isAllRequestsBelongingToSingleOrg;

		DocumentPrintSet Task
		{
			get;
			set;
		}

		public DocumentPrintSet Task_ForTestOnly
		{
			get { return Task; }
			set { Task = value; }
		}

		public int TaskCount
		{
			get { return Task != null ? Task.Count : 0; }
		}

		public void Run()
		{
			if (Task != null && Task.Count > 0)
			{
				if (!new DeliveryInstructionDestination[] { DeliveryInstructionDestination.None, DeliveryInstructionDestination.Preview, DeliveryInstructionDestination.UserCancelled }.Contains(Task.Run(Env.Security.None)))
				{
					MarkCashAdvanceRequestsPrinted();
				}
			}
		}

		void MarkCashAdvanceRequestsPrinted()
		{
			if (!haveTransactionsBeenUpdatedAsPrinted)
			{
				var factoryForUpdateOfPrintedFlag = new BusinessObjectFactory();
				var cashAdvanceRequestFactory = factoryForUpdateOfPrintedFlag.Load<AccCashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, cashAdvanceRequestsToPrint.Where(r => !r.CAH_Printed).Select(r => r.PK).ToArray()));

				factoryForUpdateOfPrintedFlag.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				try
				{
					foreach (var request in cashAdvanceRequestFactory)
					{
						if (Task.IsDocValidForPrinting(request.PK))
						{
							request.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
							request.CAH_Printed = true;
						}
					}
					factoryForUpdateOfPrintedFlag.Save();
				}
				finally
				{
					factoryForUpdateOfPrintedFlag.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				}
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				haveTransactionsBeenUpdatedAsPrinted = true;
			}
		}

		void CreatePrintTask(AccCashAdvanceRequestHeader[] requestsToPrint)
		{
			if (requestsToPrint.Any())
			{
				var firstRequest = requestsToPrint.First();
				var command = GetDocumentCommandForRequest(firstRequest);
				if (command == null)
				{
					throw new UnableToFindInvoiceDocumentCommandException("Unable to find advance payment document command for request " + firstRequest.CAH_RequestReferenceNumber);
				}
				else
				{
					Task = new DocumentPrintSet(command, null);
					Task.Clear();

					var createdDocumentPacks = new List<DocumentPack>(GetPacksIEnum());
					Task.AddRange(createdDocumentPacks);
				}
			}
		}

		public static DocumentCommand GetDocumentCommandForRequest(AccCashAdvanceRequestHeader requestToPrint)
		{
			DocumentCommand result = null;
			var documentSupportableParentJob = LoadParentJob(requestToPrint) as IDocumentSupportable;
			if (documentSupportableParentJob != null)
			{
				var printCommands = GetPrintCashAdvanceRequestMenuCommand(documentSupportableParentJob);
				if (printCommands == null || printCommands.Length == 0)
				{
					printCommands = GetPrintCashAdvanceRequestMenuCommand(requestToPrint);
				}
				if (printCommands != null && printCommands.Length == 1)
				{
					result = (DocumentCommand)printCommands[0];
				}
			}
			return result;
		}

		void AddRequests(params AccCashAdvanceRequestHeader[] requests)
		{
			isAllRequestsBelongingToSingleOrg = CheckAllRequestsBelongingToSingleOrg(requests);
			foreach (var request in requests)
			{
				if (request != null)
				{
					AddRequest(request);
				}
			}
		}

		void AddRequest(AccCashAdvanceRequestHeader request)
		{
			var requestKey = GetRequestKey(request);
			List<AccCashAdvanceRequestHeader> requestsForOrg;

			if (requestsGroupedByOrg.TryGetValue(requestKey, out requestsForOrg))
			{
				requestsForOrg.Add(request);
			}
			else
			{
				requestsGroupedByOrg[requestKey] = new List<AccCashAdvanceRequestHeader>(new AccCashAdvanceRequestHeader[] { request });
			}
		}

		string GetRequestKey(AccCashAdvanceRequestHeader request)
		{
			return  isAllRequestsBelongingToSingleOrg ? request.CAH_OH_Organization.ToStringKey() : request.PK.ToStringKey();
		}

		bool CheckAllRequestsBelongingToSingleOrg(params AccCashAdvanceRequestHeader[] requests)
		{
			return requests.GroupBy(x => x.CAH_OH_Organization).Count() == 1;
		}

		public static BusinessObject LoadParentJob(AccCashAdvanceRequestHeader requestHeader)
		{
			BusinessObject result = null;
			if (requestHeader.Job != null && requestHeader.Job.JH_ParentID.IsValid && !requestHeader.Job.JH_ParentTableCode.IsEmpty)
			{
				var genericJob = requestHeader.Job.LoadGenericJob<GenericJob.GenericJob>();
				result = genericJob?.JobType?.BizoType == null ? null : requestHeader.Factory.Load(genericJob.JobType.BizoType, genericJob.PK);
			}
			return result;
		}

		static BusinessObject[] GetPrintCashAdvanceRequestMenuCommand(IDocumentSupportable parentContainingMenuCommand)
		{
			var documentCommands = new DocumentCommandCollection(parentContainingMenuCommand);
			documentCommands.Load();
			ZQuery menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, DocumentMenuName);
			menuFilter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			BusinessObject[] result = documentCommands.Find(menuFilter);
			if (result.Length == 0)
			{
				menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, DocumentMenuName);
				result = documentCommands.Find(menuFilter);
			}
			return result;
		}

		internal static string DocumentMenuName => (NoResString)"DocBuilder Advance Payment Request";

		IEnumerable<DocumentPack> GetPacksIEnum()
		{
			foreach (var requestsGrouped in requestsGroupedByOrg.Values)
			{
				var packs = new List<DocumentPack>();
				foreach (var request in requestsGrouped)
				{
					AddRequestToPack(request, packs);
				}

				foreach (var pack in packs)
				{
					yield return pack;
				}
			}
		}

		protected void AddRequestToPack(AccCashAdvanceRequestHeader request, List<DocumentPack> packs)
		{
			var documentSupportableParentJob = LoadParentJob(request) as IDocumentSupportable;
			if (documentSupportableParentJob != null)
			{
				var printCommand = GetDocumentCommandForRequest(request) ?? throw new UnableToFindInvoiceDocumentCommandException("Unable to find advance payment document command for request " + request.CAH_RequestReferenceNumber);
				if (packs.Any())
				{
					var pack = packs.First();
					pack.AddReportsToPack(printCommand, null, request, null);
				}
				else
				{
					packs.AddRange(CreatePacks(request, printCommand));
				}
			}
		}

		List<DocumentPack> CreatePacks(AccCashAdvanceRequestHeader request, DocumentCommand command)
		{
			List<DocumentPack> result = new List<DocumentPack>();
			DocumentPack pack = new DocumentPack(command, request, null, null, false);
			result.Add(pack);
			return result;
		}

		public void Dispose()
		{
			if (Task != null)
			{
				Task.Dispose();
			}
		}
	}
}
