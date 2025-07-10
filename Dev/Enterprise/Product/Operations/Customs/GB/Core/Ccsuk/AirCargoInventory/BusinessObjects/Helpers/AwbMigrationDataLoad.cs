using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Helpers
{
	public class AwbMigrationDataLoad : DataLoad
	{
		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			var transactionPK = Guid.Empty;
			if (!Env.Security.AirCcsukMasterNew.IsAllowed || !Env.Security.AirCcsukHouseNew.IsAllowed)
			{
				DisplayExcludedRecordMessage(": user does not have security rights to create a new CCS-UK record, row skipped");
				RunCounters.RecsExcluded++;
			}
			else
			{
				if (ValidateLineFormatAndSaveMembers(line))
				{
					transactionPK = CheckOrCreateAwb();
				}
				else
				{
					DisplayExcludedRecordMessage(" excluded... data is inconsistent with required format.");
					RunCounters.RecsExcluded++;
				}
			}
			UpdateAndDisplayIfRequired(transactionPK, "AWB");
		}

		bool ValidateLineFormatAndSaveMembers(OCsvLine line)
		{
			pima = "";
			airport = "";
			shed = "";
			mawbNumber = "";
			hawbNumber = "";
			sDC = "";
			branchCode = "";

			if (line.FieldValues[0].Length == 14)
			{
				pima = line.FieldValues[0];
				if (line.FieldValues[1].Length == 3)
				{
					airport = line.FieldValues[1];
					if (line.FieldValues[2].Length == 3)
					{
						shed = line.FieldValues[2];
						if (line.FieldValues[3].Length == 11)
						{
							mawbNumber = line.FieldValues[3];
							if (line.FieldValues[4].Length == 8 || line.FieldValues[4].Length == 0)
							{
								hawbNumber = line.FieldValues[4];
								if (line.FieldValues[5].Length == 1)
								{
									sDC = line.FieldValues[5];
									if (line.FieldValues[6].Length == 3)
									{
										branchCode = line.FieldValues[6];
										npx = ZInt.ParseEmptyAsZero(line.FieldValues[7]);
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		Guid CheckOrCreateAwb()
		{
			var transactionPK = Guid.Empty;
			var branchPk = GetBranchPk();

			if (branchPk.IsEmpty)
			{
				DisplayExcludedRecordMessage(": Branch " + branchCode + " does not exist, skipping record");
				RunCounters.RecsExcluded++;
			}
			else
			{
				var outputMessageToDisplay = "";
				var mawbOrBasic = LoadMawbOrBasic();
				CusHAWB hawb = null;
				if (mawbOrBasic == null)
				{
					mawbOrBasic = Factory.New<CusMAWB>();
					mawbOrBasic.CM_MAWB = mawbNumber;
					mawbOrBasic.Profile = pima;
					mawbOrBasic.CM_GB = branchPk;
					mawbOrBasic.CargoTerminalOperatorAirport = airport;
					mawbOrBasic.CargoTerminalOperator = shed;
					mawbOrBasic.ShipmentDescriptionCode = sDC;
					mawbOrBasic.NumberOfPiecesExpected = (ZShort)npx;
					mawbOrBasic.Validation.ValidateAll();
					if (mawbOrBasic.HasErrors)
					{
						outputMessageToDisplay += string.Format("{0} - invalid, skipping. {1} ", mawbOrBasic.ReferenceNumber, mawbOrBasic.GetErrors().ToUniqueMessageListString("; "));
						mawbOrBasic.Delete();
						RunCounters.RecsExcluded++;
					}
					else
					{
						if (mawbOrBasic.Branch.PK == GlbBranch.CurrentBranch.PK)
						{
							outputMessageToDisplay += string.Format("MAWB {0}{1} created. ", mawbOrBasic.MasterLevelHouseHelper.CS_WarehouseLocation, mawbOrBasic.ReferenceNumber);
						}
						else
						{
							outputMessageToDisplay += string.Format("MAWB {0}{1} created under branch '{2}' as specified in the file.",
								mawbOrBasic.MasterLevelHouseHelper.CS_WarehouseLocation,
								mawbOrBasic.ReferenceNumber,
								mawbOrBasic.Branch.GB_BranchName);
						}
						RunCounters.RecsCreated++;
					}
				}
				else
				{
					// Mawb found 
					outputMessageToDisplay += string.Format("MAWB {0}{1} already exists. ", mawbOrBasic.MasterLevelHouseHelper.CS_WarehouseLocation, mawbOrBasic.ReferenceNumber);
				}

				if (!hawbNumber.IsEmpty && mawbOrBasic != null && !mawbOrBasic.IsDeleted)
				{
					hawb = new CusHAWB.Loader(Factory).FindExistingHawbOnMawb(mawbOrBasic, hawbNumber);
					if (hawb == null)
					{
						if (mawbOrBasic.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_AddChildAwb))
						{
							hawb = mawbOrBasic.ChildBills.AddNew();
							hawb.CS_HAWB = hawbNumber;
							hawb.CS_PiecesManifested = (ZShort)npx;
							hawb.Validation.ValidateAll();
							if (hawb.HasErrors)
							{
								outputMessageToDisplay += string.Format("{0} - invalid, skipping. {1}", hawb.ReferenceNumber, hawb.GetErrors().ToUniqueMessageListString("; "));
								hawb.Delete();
								RunCounters.RecsExcluded++;
							}
							else
							{
								outputMessageToDisplay += string.Format("HAWB {0} created. ", hawb.ReferenceNumber);
								RunCounters.RecsCreated++;
							}
						}
						else
						{
							outputMessageToDisplay += string.Format("MAWB {0}{1} cannot accept new HAWBs. HAWB {1} skipped. ", mawbOrBasic.MasterLevelHouseHelper.CS_WarehouseLocation, mawbOrBasic.ReferenceNumber, hawbNumber);
							RunCounters.RecsExcluded++;
						}
					}
					else
					{
						outputMessageToDisplay += string.Format("HAWB {0} already exists. ", hawb.ReferenceNumber);
						RunCounters.RecsExcluded++;
					}
				}

				DisplayLogMessage(outputMessageToDisplay);
				transactionPK = mawbOrBasic.PK.ToGuid();
			}
			return transactionPK;
		}

		CusMAWB LoadMawbOrBasic()
		{
			return CuscarFriConsignmentInserter.FindAllMawbsAtThisLocationWithinAYearOrPreArrival(new CusMAWB.Loader(Factory), mawbNumber, airport + shed).FirstOrDefault();
		}

		ZGuid GetBranchPk()
		{
			if (!BranchPks.ContainsKey(branchCode))
			{
				var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, branchCode));
				if (branch == null)
				{
					return ZGuid.Empty;
				}
				else
				{
					BranchPks[branchCode] = branch.PK;
				}
			}
			return BranchPks[branchCode];
		}

		Dictionary<string, ZGuid> branchPks;
		Dictionary<string, ZGuid> BranchPks
		{
			get { return branchPks ?? (branchPks = new Dictionary<string, ZGuid>()); }
		}

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", CSVTemplateHeaders); }
		}

		IEnumerable<string> CSVTemplateHeaders
		{
			get
			{
				yield return FieldNames.PIMA;
				yield return FieldNames.AIRPORT;
				yield return FieldNames.SHED;
				yield return FieldNames.MAWB;
				yield return FieldNames.HAWB;
				yield return FieldNames.SDC;
				yield return FieldNames.BRANCHCODE;
				yield return FieldNames.NPX;
			}
		}

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			//PIMA,AIRPORT,SHED,MAWB,HAWB,SPLIT,SDC,COMPANYCODE,BRANCHCODE,NPX
			return line.FieldValues.Length == 8
				&& line.FieldValues[0].ToUpperInvariant() == FieldNames.PIMA
				&& line.FieldValues[1].ToUpperInvariant() == FieldNames.AIRPORT
				&& line.FieldValues[2].ToUpperInvariant() == FieldNames.SHED
				&& line.FieldValues[3].ToUpperInvariant() == FieldNames.MAWB
				&& line.FieldValues[4].ToUpperInvariant() == FieldNames.HAWB
				&& line.FieldValues[5].ToUpperInvariant() == FieldNames.SDC
				&& line.FieldValues[6].ToUpperInvariant() == FieldNames.BRANCHCODE
				&& line.FieldValues[7].ToUpperInvariant() == FieldNames.NPX;
		}

		protected override void OutputFinalTotals(string dataType)
		{
			base.OutputFinalTotals(dataType);
			if (RunCounters.RecsCreated > 0)
			{
				DisplayLogMessage("You should now use Operational Actions to send FSR-with-update messages for these new AWBs to synchronise them with CCS-UK");
			}
		}

		protected override void ImportComplete()
		{
			base.ImportComplete();
			Factory.Save();
		}

		ZString pima;
		ZString airport;
		ZString shed;
		ZString mawbNumber;
		ZString hawbNumber;
		ZString sDC;
		ZString branchCode;
		ZInt npx;

		static class FieldNames
		{
			public const string PIMA = "PIMA";
			public const string AIRPORT = "AIRPORT";
			public const string SHED = "SHED";
			public const string MAWB = "MAWB";
			public const string HAWB = "HAWB";
			public const string SDC = "SDC";
			public const string BRANCHCODE = "BRANCHCODE";
			public const string NPX = "NPX";
		}
	}
}
