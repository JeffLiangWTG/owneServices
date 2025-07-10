using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT
{
	[Serializable]
	class DeclarationCustomResponseSenderSubscriber : LogSubscriber
	{
		public override string Name
		{
			get { return "TNTDeclarationCustomsResponse"; }
		}

		public override string FriendlyName
		{
			get { return "TNT Declaration Customs Response"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { JobDeclarationSchema.Constants.TableName, JobShipmentSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get
			{
				if (eventTypes == null)
				{
					var collection = TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.Value;
					eventTypes = collection.Cast<EventRegistryBusinessObject>().Select(x => (string)x.Code).Distinct().ToArray();
				}
				return eventTypes;
			}
		}
		string[] eventTypes;

		public override bool IsClientSpecificSubscriber => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var collection = TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.Value;
			foreach (IQueuedLog queuedLog in queuedLogs)
			{
				if (collection.FindByCodeAndReference(queuedLog.SJ_SE_NKEvent, queuedLog.SJ_Reference) != null)
				{
					BaseJobDeclaration declaration = null;
					ForwardingShipment shipment = null;

					if (queuedLog.SJ_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
					{
						declaration = queuedLog.Factory.Load<BaseJobDeclaration>(queuedLog.SJ_ParentID);
					}
					else if (queuedLog.SJ_ParentTableCode == JobShipmentSchema.Constants.Prefix)
					{
						shipment = queuedLog.Factory.Load<ForwardingShipment>(queuedLog.SJ_ParentID);
						declaration = shipment?.Declarations.Cast<BaseJobDeclaration>().FirstOrDefault(d => BranchBelongsToCurrentCompany(d.JE_GB));
					}

					if (declaration == null)
					{
						if (shipment == null)
						{
							DefaultLogger.Log(LogType.Information, ZString.Format("Customs Response File will not be sent: Queued Log Event is not in a Declaration or Shipment. (Parent Table Code: {0}, Parent ID: {1})", queuedLog.SJ_ParentTableCode, queuedLog.SJ_ParentID));
						}
						else
						{
							DefaultLogger.Log(LogType.Information, ZString.Format("Customs Response File will not be sent: Shipment does not contain a Declaration where the branch belongs to the Current Company. (Parent Table Code: {0}, Parent ID: {1})", queuedLog.SJ_ParentTableCode, queuedLog.SJ_ParentID));
						}
					}
					else if (!BranchBelongsToCurrentCompany(declaration.JE_GB))
					{
						DefaultLogger.Log(LogType.Information, ZString.Format("Customs Response File will not be sent: Declaration's branch does not belong to current company. (Declaration Reference: {0})", declaration.JE_DeclarationReference));
					}
					else if (!declaration.IsImport)
					{
						DefaultLogger.Log(LogType.Information, ZString.Format("Customs Response File will not be sent: Declaration Entry Type is not Import. (Declaration Reference: {0})", declaration.JE_DeclarationReference));
					}
					else
					{
						SendCustomResponseFile(declaration);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044", Justification = "False positive, checking for count above 0.")]
		void SendCustomResponseFile(BaseJobDeclaration declaration)
		{
			var houseBills = declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill);
			var housebillsSeparatedByCommaBeforeExport = declaration.Bills.HouseBillsCommaSeparated;
			int houseBillCountBeforeExport = houseBills.Length;

			var query = new ZQuery(CusDecHouseBillSchema.CU_JE, declaration.PK);
			query.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.HouseBill);

			var noOfHBLsInDBBeforeExport = declaration.Factory.GetDatabaseCount(typeof(Bill), query);

			if (houseBillCountBeforeExport > 0 || noOfHBLsInDBBeforeExport > 0)
			{
				ZString origin = ZString.Empty;
				ZString destination = ZString.Empty;
				ZString branchCode = ZString.Empty;
				var sentHouseBills = new List<ZString>();

				if (declaration.Shipment != null)
				{
					ZString cartageWayBill = declaration.Shipment.JS_CartageWaybill;
					origin = cartageWayBill.SubstringSafe(0, 3).Trim();
					destination = cartageWayBill.SubstringSafe(4, 3).Trim();
					branchCode = cartageWayBill.SubstringSafe(8, 3).Trim();
				}

				if (branchCode.IsEmpty && declaration.Branch != null)
				{
					branchCode = declaration.Branch.GB_Code;
				}

				DefaultLogger.Log(LogType.Information, ZString.Format("Export Started (Declaration: {4}):{5}All Bills count: {0}{5}No of MBL {1}{5}No of HBL returned by FindByBillType: {2}{5}No of HBL in DB: {3}"
					, declaration.Bills.Count, declaration.Bills.NumberOfMasterBill, houseBillCountBeforeExport, noOfHBLsInDBBeforeExport, declaration.JobNumber, System.Environment.NewLine));

				foreach (Bill bill in houseBills)
				{
					DeclarationCustomsResponseStatusSender statusSender = new DeclarationCustomsResponseStatusSender();
					var houseBill = bill.CU_HouseBill;
					ZString errorMessage = statusSender.SendEDNReply(branchCode, houseBill, origin, destination, declaration.DeclarationNumber, declaration.JE_EntryStatus, "Y");

					if (statusSender.NotificationBuffer.AsString != ZString.Empty)
					{
						DefaultLogger.Log(LogType.Information, statusSender.NotificationBuffer.AsString);
					}
					if (errorMessage.IsEmpty)
					{
						sentHouseBills.Add(houseBill);
					}
					else
					{
						DefaultLogger.Log(LogType.Error, ZString.Format("House Bill ({0}) cannot be sent: {1} (Declaration Reference: {2})", houseBill, errorMessage, declaration.JE_DeclarationReference));
					}
				}

				LogResult(declaration, houseBillCountBeforeExport, sentHouseBills);

				var billsAfterExport = declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill);

				var houseBillCountAfterExport = billsAfterExport.Length;

				var noOfHBLsInDBAfterExport = declaration.Factory.GetDatabaseCount(typeof(Bill), query);

				if (houseBillCountAfterExport != houseBillCountBeforeExport)
				{
					DefaultLogger.Log(LogType.Information, ZString.Format("Export Finished (Declaration: {0}):{1}Discrepancy on bills Collection before and after export{1}Before Export:{2}{1}After Export:{3}{1}"
					, declaration.JobNumber, System.Environment.NewLine, housebillsSeparatedByCommaBeforeExport, declaration.Bills.HouseBillsCommaSeparated));
				}
				else if (noOfHBLsInDBAfterExport != houseBillCountBeforeExport)
				{
					var anotherFactory = new BusinessObjectFactory();
					var billsSet = anotherFactory.Load<Bill>(query);
					var billFromDBBuilder = new ZStringBuilder();
					Array.ForEach(billsSet, delegate(Bill b)
					{ billFromDBBuilder.Append(b.CU_BillNum); });

					DefaultLogger.Log(LogType.Information, ZString.Format("Export Finished (Declaration: {0}):{1}Discrepancy on bills collection and no of bills returned from DB{1}Bills Collection before Export:{2}{1}Bills in DB After Export:{3}{1}"
					, declaration.JobNumber, System.Environment.NewLine, housebillsSeparatedByCommaBeforeExport, billFromDBBuilder.ToStringWithDelimiterBetweenAppends(",")));
				}
				else
				{
					DefaultLogger.Log(LogType.Information, ZString.Format("Export Finished (Declaration: {4}):{5}All Bills count: {0}{5}No of MBL {1}{5}No of HBL returned by FindByBillType: {2}{5}No of HBL in DB: {3}"
						, declaration.Bills.Count, declaration.Bills.NumberOfMasterBill, houseBillCountAfterExport, noOfHBLsInDBAfterExport, declaration.JobNumber, System.Environment.NewLine));
				}
			}
			else
			{
				DefaultLogger.Log(LogType.Information, ZString.Format("Customs Response File will not be sent: No house bill in the declaration. (Declaration Reference: {0})", declaration.JE_DeclarationReference));
			}
		}

		bool BranchBelongsToCurrentCompany(ZGuid branchPK)
		{
			return GlbCompany.CurrentCompany.Branches.FindByPK(branchPK) != null;
		}

		internal
 void LogResult(BaseJobDeclaration declaration, int houseBillCount, List<ZString> sentHouseBills)
		{
			int sentHouseBillCount = sentHouseBills.Count;
			if (sentHouseBillCount > 0)
			{
				DefaultLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Customs Response File with following {0} house bills has been successfully sent (Declaration Reference: {1}) :\r\n{2}", sentHouseBillCount, declaration.JE_DeclarationReference, string.Join(System.Environment.NewLine, sentHouseBills)));

				if (sentHouseBillCount == houseBillCount)
				{
					declaration.Logs.AddNew(Events.DataExport);
				}
			}
		}
	}
}
