using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Integration
{
	//NOTE: This class is placed in Accounting.Integration only to maintain the virginity of Rating.Intergration's references
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public static class CalculationLogsLoader
	{
		#region Load

		/// <summary>
		/// Load autorating calculation log.
		/// </summary>
		/// <param name="bizo">BusinessObject that contains autorating calculation log note(s)</param>
		/// <param name="loadOption">Which calculation log should be loaded</param>
		public static CalculationLogsWrapper Load(BusinessObject bizo, LoadOption loadOption = LoadOption.Auto)
		{
			StmNote calculationLogsNote = null;
			if (loadOption == LoadOption.Revenue || loadOption == LoadOption.Auto)
			{
				calculationLogsNote = GetRevenueNote(bizo);
			}

			if (loadOption == LoadOption.Costing || (loadOption == LoadOption.Auto && calculationLogsNote == null))
			{
				calculationLogsNote = GetCostNote(bizo);
			}

			return calculationLogsNote != null ? CalculationLogsWrapper.Deserialize(calculationLogsNote.ST_NoteData.ToUTF8()) : null;
		}

		public enum LoadOption
		{
			/// <summary>
			/// Load Revenue log; if it doesn't exist, load Costing log
			/// </summary>
			Auto,

			/// <summary>
			/// Load Costing log
			/// </summary>
			Costing,

			/// <summary>
			/// Load Revenue log
			/// </summary>
			Revenue
		}

		#endregion

		#region Save

		/// <summary>
		/// Save autorating calculation log to businessobject's hidden note. Existing note will be overridden.
		/// If logs wrapper will contain both costing and revenue logs, two separate logs wrappers will be saved.
		/// </summary>
		public static void Save(BusinessObject bizo, CalculationLogsWrapper logsWrapper)
		{
			if (logsWrapper != null)
			{
				if (logsWrapper.Logs.Any(log => log.IsCosting) && logsWrapper.Logs.Any(log => !log.IsCosting))
				{
					CalculationLogsWrapper costingWrapper = CreateWrapperFromExisting(logsWrapper, true);
					SaveCore(bizo, costingWrapper);

					CalculationLogsWrapper revenueWrapper = CreateWrapperFromExisting(logsWrapper, false);
					SaveCore(bizo, revenueWrapper);
				}
				else
				{
					SaveCore(bizo, logsWrapper);
				}
			}
		}

		static CalculationLogsWrapper CreateWrapperFromExisting(CalculationLogsWrapper logsWrapper, bool isCosting)
		{
			CalculationLogsWrapper newWrapper = new CalculationLogsWrapper();
			newWrapper.IsDisabled = logsWrapper.IsDisabled;
			newWrapper.Logs.AddRange(logsWrapper.Logs.Where(log => log.IsCosting == isCosting));

			return newWrapper;
		}

		static void SaveCore(BusinessObject bizo, CalculationLogsWrapper logsWrapper)
		{
			if (logsWrapper != null)
			{
				StmNote calculationLogsNote = logsWrapper.IsCosting ? GetCostNote(bizo) : GetRevenueNote(bizo);
				if (calculationLogsNote == null)
				{
					calculationLogsNote = bizo.GetNotes().Factory.New<HiddenStmNote>();
					calculationLogsNote.ST_ParentID = bizo.PK;
					calculationLogsNote.ST_Table = bizo.TableName;
					calculationLogsNote.ST_Description = logsWrapper.IsCosting ? NoteDescriptionCosting : NoteDescriptionRevenue;
					calculationLogsNote.ST_IsCustomDescription = true;
				}
				calculationLogsNote.ST_NoteData = ZBlob.FromUTF8(logsWrapper.Serialize());
			}
		}

		#endregion

		#region Disable

		/// <summary>
		/// Disable autorating calculation log. Could disable both Costing/Revenue logs or only one of them.
		/// </summary>
		public static void Disable(BusinessObject bizo, DisableOption disableOption = DisableOption.Both)
		{
			if ((disableOption & DisableOption.Costing) != 0)
			{
				Disable(bizo, LoadOption.Costing);
			}

			if ((disableOption & DisableOption.Revenue) != 0)
			{
				Disable(bizo, LoadOption.Revenue);
			}
		}

		static void Disable(BusinessObject bizo, LoadOption loadOption)
		{
			CalculationLogsWrapper logsWrapper = Load(bizo, loadOption);
			if (logsWrapper != null && !logsWrapper.IsDisabled)
			{
				logsWrapper.IsDisabled = true;
				CalculationLogsLoader.Save(bizo, logsWrapper);
			}
		}

		[Flags]
		public enum DisableOption
		{
			Costing = 1,
			Revenue = 2,
			Both = Costing | Revenue
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a note identifier")]
		internal const string NoteDescriptionCosting = "AutoRating Calculation Log Costing";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a note identifier")]
		internal const string NoteDescriptionRevenue = "AutoRating Calculation Log Revenue";

		static StmNote GetCostNote(BusinessObject bizo)
		{
			return GetNote(bizo, NoteDescriptionCosting);
		}

		static StmNote GetRevenueNote(BusinessObject bizo)
		{
			return GetNote(bizo, NoteDescriptionRevenue);
		}

		static StmNote GetNote(BusinessObject bizo, string noteDescription)
		{
			ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, bizo.PK);
			query.AddToFilter(StmNoteSchema.ST_Description, noteDescription);
			//query.AddToFilter(StmNoteSchema.ST_Table, bizo.TableName);
			query.FetchOnlyFromLocalCache = !bizo.IsInDatabase;

			return bizo.GetNotes().Factory.LoadTop1<HiddenStmNote>(query);
		}

		#endregion
	}
}
