using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.TNT
{
	public class EDNExporter
	{
		//TODO: code taken from UPS client source - extract to common location
		public EDNExporter(INotifications notify)
		{
			fNotify = notify;
			fNextLastEDNReturnTime = ZDateTime.UtcNow.AddMinutes(-5);
		}

		public void Run()
		{
			fNextLastEDNReturnTime = ZDateTime.UtcNow.AddMinutes(-5);

			if (RunEDNReturn())
			{
				TNTDataRegistry.Instance.LastEDNReturnTime = fNextLastEDNReturnTime;
			}
		}

		readonly INotifications fNotify;
		ZDateTime fNextLastEDNReturnTime;

		bool RunEDNReturn()
		{
			DataTable decCusEntryNums = GetDeclarationCusEntryNumbers();
			int entryNumbersReturned = ReturnDeclarationCusEntryNumsToUps(decCusEntryNums);
			fNotify.Notify(new InfoNotification(entryNumbersReturned + " Declaration Customs Entry Number(s) returned."));

			return (entryNumbersReturned == decCusEntryNums.Rows.Count);
		}

		int ReturnDeclarationCusEntryNumsToUps(DataTable decCusEntryNums)
		{
			int entryNumbersReturned = 0;

			foreach (DataRow decCusEntryNumRow in decCusEntryNums.Rows)
			{
				ZString jobNumber = new ZString(decCusEntryNumRow[BaseJobDeclaration.Schema.JE_DeclarationReference]);
				if (!jobNumber.IsEmpty)
				{
					fNotify.Notify(new InfoNotification("Returning EDN for Job #" + jobNumber));

					try
					{
						ZString eDNNumber = new ZString(decCusEntryNumRow[CusEntryNumber.Schema.CE_EntryNum]);
						ZString housebill = new ZString(decCusEntryNumRow[CommonShipment.Schema.JS_HouseBill]);
						ZDateTime eDNDateTime = new ZDateTime(decCusEntryNumRow[StmALog.Schema.SL_EventTime]);
						ZString originDestinationAndBranch_ToCartageWaybill = new ZString(decCusEntryNumRow[CommonShipment.Schema.JS_CartageWaybill]);
						ZString entryStatus = new ZString(decCusEntryNumRow[BaseJobDeclaration.Schema.JE_EntryStatus]).Trim();
						ZString conOrigin = originDestinationAndBranch_ToCartageWaybill.SubstringSafe(0, 3);
						ZString conDest = originDestinationAndBranch_ToCartageWaybill.SubstringSafe(4, 3);
						ZString conBranch = originDestinationAndBranch_ToCartageWaybill.SubstringSafe(8, 3);
						ZString cleared = "N";
						if (entryStatus == CustomsEntryStatus.ClearOriginal.Code ||
							entryStatus == CustomsEntryStatus.ClearReplacement.Code ||
							entryStatus == CustomsEntryStatus.ClearWithdrawal.Code)
						{
							cleared = "Y";
						}

						TNTReturnExitStatus returnFile = new TNTReturnExitStatus(fNotify);
						returnFile.SendEDNReply(conBranch, housebill, conOrigin, conDest, eDNNumber, entryStatus, cleared);
						entryNumbersReturned++;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						// If fails, notifies error and continues (just skipping the record).
						string errorMessage = System.Environment.NewLine + e.ToString();
						fNotify.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, errorMessage));
					}
				}
			}

			return entryNumbersReturned;
		}

		#region SQL Statements

		protected DataTable GetDeclarationCusEntryNumbers()
		{
			string sqlText = GetDeclarationCusEntryNumbersSelectCommand();
			return ZArchitecture.Core.Utilities.GetDataTableFromQuery(sqlText);
		}

		protected string GetDeclarationCusEntryNumbersSelectCommand()
		{
			string sqlText = String.Format(@"
				SELECT 
					JS_HouseBill, 
					JS_CartageWaybill, 
					JE_DeclarationReference, 
					JE_EntryStatus, 
					CE_EntryNum, 
					SL_EventTime 
				FROM
				  (SELECT SL_Parent, SL_EventTime FROM dbo.StmALog 
				   WHERE SL_Table = '{0}'
				   AND   SL_PostedTimeUtc >  '{1}'
				   AND   SL_PostedTimeUtc <= '{2}'
				   AND   SL_SE_NKEvent IN ('{3}', '{4}')
				  ) EDNLog
					INNER JOIN 
				  (SELECT CE_PK, CE_ParentID, CE_EntryNum FROM dbo.CusEntryNum
				   WHERE CE_ParentTable = '{5}'
				  ) DecEntryNum ON CE_PK = SL_Parent
					INNER JOIN dbo.JobDeclaration ON JE_PK = CE_ParentID
					INNER JOIN dbo.JobShipment ON JS_PK = JE_JS",

				CusEntryNumber.Schema.TableName,
				TNTDataRegistry.Instance.LastEDNReturnTime.SqlFormat,
				fNextLastEDNReturnTime.SqlFormat,
				Events.AddedARecordToTheSystem.Code,
				Events.EditedARecord.Code,
				BaseJobDeclaration.Schema.TableName
				);
			return sqlText;
		}

		#endregion
	}
}
