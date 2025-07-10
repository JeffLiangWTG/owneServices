using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.OutTurnDataImport
{
	public class OutTurnDataImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notify, out ITransactionParticipant[] additionalTransactionActions)
		{
			NotificationBuffer buffer = new NotificationBuffer(notify);
			additionalTransactionActions = null;
			OutTurnFileFormat fileFormat = new OutTurnFileFormat();
			string dataLine;

			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();

			while ((dataLine = dataReader.ReadLine()) != null)
			{
				try
				{
					OutTurnFlatFileDataRow row = (OutTurnFlatFileDataRow)fileFormat.ConvertToRow(dataLine);

					CusHAWB housebill = GetHouseBill(row, factoryProvider.Current, buffer);

					if (housebill != null)
					{
						bool retry;
						int retryCounter = 1;
						ZDateTime endDateTime = ZDateTime.Now.AddSeconds(timeOutSeconds);

						do
						{
							retry = false;
							try
							{
								SetPiecesLandedAndSave(row, housebill, buffer, factoryProvider);
							}
							catch (ZSaveConcurrencyException)
							{
								if (!AttemptRetry(out retry, ref retryCounter, endDateTime, buffer, housebill.CS_HAWB))
								{
									throw;
								}
							}
						} while (retry);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					buffer.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
			}
			return true;
		}

		bool AttemptRetry(out bool retry, ref int retryCounter, ZDateTime endDateTime, NotificationBuffer buffer, ZString housebill)
		{
			retry = (retryCounter++ < maxRetries && ZDateTime.Now < endDateTime);
			if (retry)
			{
				Thread.Sleep(new TimeSpan(0, 0, 2));
			}
			else
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error,
					ZString.Format("Air Cargo House Record ({0}) cannot be updated as it is currently modifying by other user or process", housebill)));
			}
			return retry;
		}

		CusHAWB GetHouseBill(OutTurnFlatFileDataRow row, BusinessObjectFactory factory, NotificationBuffer buffer)
		{
			CusHAWB[] housebills = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, row.HAWB));
			CusHAWB result = null;
			if (housebills.Length > 0)
			{
				foreach (CusHAWB housebill in housebills)
				{
					if (IsSectorMatched(housebill, row.SectorInformation, buffer))
					{
						result = housebill;
						break;
					}
				}
			}
			else
			{
				buffer.Notify(new WarningNotification(WarningType.Warning, "No Match found For HAWB " + row.HAWB));
			}

			return result;
		}

		bool IsSectorMatched(CusHAWB housebill, ZString sectorInformation, NotificationBuffer buffer)
		{
			bool result = false;
			StmNote[] notes = housebill.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			if (notes.Length > 0)
			{
				foreach (StmNote note in notes)
				{
					if (note.ST_NoteDataAsText.Contains(sectorInformation))
					{
						result = true;
						break;
					}
					else
					{
						buffer.Notify(new WarningNotification(WarningType.Warning, string.Format("{0} does not contain Sector Info ({1})", housebill.UnderbondHumanReadableName, sectorInformation)));
					}
				}
			}
			else
			{
				buffer.Notify(new WarningNotification(WarningType.Warning, housebill.UnderbondHumanReadableName + " is missing Sector Information"));
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected virtual
 void SetPiecesLandedAndSave(OutTurnFlatFileDataRow row, CusHAWB houseBill, NotificationBuffer buffer, BusinessObjectFactoryProvider factoryProvider)
		{
			if (!houseBill.CS_IsResponsePending)
			{
				ZInt manifestPieces = row.ManifestPieces;
				ZInt landedPieces = row.LandedPieces;

				CusUnderbond underbond = GetMasterUnderbond(houseBill.MAWB, row);
				CusOutturn outTurn = GetHouseOutTurn(underbond, houseBill);
				outTurn.C5_PackagesOutturned = landedPieces;
				SetOutTurnResultType(outTurn, manifestPieces, landedPieces);
				Application.DoEvents();
				factoryProvider.SaveCurrentAndCreateNew();
			}
		}

		CusUnderbond GetMasterUnderbond(CusMAWB masterBill, OutTurnFlatFileDataRow row)
		{
			ZQuery underbondFilter = new ZQuery(CusUnderbondSchema.C4_ParentID, masterBill.PK);
			underbondFilter.AddToFilter(CusUnderbondSchema.C4_FlightNo, masterBill.CM_FlightNo);
			underbondFilter.AddToFilter(CusUnderbondSchema.C4_ArrivalDate, masterBill.CM_ArrivalDate);
			underbondFilter.AddToFilter(CusUnderbondSchema.C4_MovementReason, CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination);

			CusUnderbond[] underbonds = (CusUnderbond[])masterBill.AllUnderbonds.Find(underbondFilter);
			CusUnderbond result = null;
			if (underbonds.Length > 0)
			{
				result = underbonds[0];
			}
			else
			{
				result = masterBill.AllUnderbonds.AddNew();
				result.C4_ParentID = masterBill.PK;
				result.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
				result.C4_FlightNo = masterBill.CM_FlightNo;
				result.C4_ArrivalDate = masterBill.CM_ArrivalDate;
			}

			return result;
		}

		CusOutturn GetHouseOutTurn(CusUnderbond underbond, CusHAWB houseBill)
		{
			Customs.Business.CusOutturn[] outTurns = (Customs.Business.CusOutturn[])underbond.Outturns.Find(new ZQuery(CusOutturnSchema.C5_ParentID, houseBill.PK));
			Customs.Business.CusOutturn result = null;
			if (outTurns.Length == 0)
			{
				result = underbond.Outturns.AddNew();
				result.Parent = houseBill;
			}
			else
			{
				result = outTurns[0];
			}

			return (CusOutturn)result;
		}

		void SetOutTurnResultType(CusOutturn outTurn, int manifestPieces, int landedPieces)
		{
			if (manifestPieces > landedPieces)
			{
				outTurn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			}
			else if (manifestPieces < landedPieces)
			{
				outTurn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
			}
			else
			{
				outTurn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			}
		}

		const int maxRetries = 3;
		const int timeOutSeconds = 80;
	}
}
