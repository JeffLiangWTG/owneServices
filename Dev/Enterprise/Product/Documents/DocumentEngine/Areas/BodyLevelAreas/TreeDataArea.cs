using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Areas
{
	abstract class TreeDataArea : DataArea
	{
		protected TreeDataArea(int start, int end, Report report, string parameterText, TreeDataArea parent) : this(start, end, report, parameterText)
		{
			Parent = parent;
		}

		TreeDataArea(int start, int end, Report report, string parameterText) : base(start, end, report, parameterText)
		{
		}

		protected TreeDataArea() { }

		public abstract void ExpandForDataRows(int multiplier);

		public abstract IDataRowSource DataRowSource { get; }

		public override int DBRowCount => DataRowSource?.RowCount ?? 0;

		internal TreeDataArea Parent { get; }
		internal List<TreeDataArea> Children { get; } = new List<TreeDataArea>();

		internal List<(string cell, int row)> GetSectionForeachTags()
		{
			var result = new List<(string cell, int row)>();
			for (var i = StartingRow + 1; i <= RealEnd; i++)
			{
				var cell = ParentReport.WorkSheetCurrentlyBeingProcessed[i, 0].ToString();
				if (SectionForeachArea.IsSectionForeachAreaBeginStart(cell) || SectionForeachArea.IsSectionForeachAreaEndStart(cell))
				{
					result.Add((cell, i));
				}
			}

			return result;
		}

		protected virtual int RealEnd => End;

		internal bool ValidateSectionForeachTags(List<(string cell, int row)> tags)
		{
			if (!tags.Any())
			{
				return true;
			}

			var blockCounter = 0;
			for (var i = 0; i < tags.Count; i++)
			{
				if (SectionForeachArea.IsSectionForeachAreaBeginStart(tags[i].cell))
				{
					blockCounter++;
				}
				else if (SectionForeachArea.IsSectionForeachAreaEndStart(tags[i].cell))
				{
					if (blockCounter == 0)
					{
						return false;
					}

					blockCounter--;
				}
			}

			return blockCounter == 0;
		}

		public bool IsForeachProcessed { get; protected internal set; }

		internal void ProcessSectionForeachAreas(SectionBodyArea bodyArea)
		{
			if (IsForeachProcessed)
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Every {nameof(TreeDataArea)} can only be processed once."));
			}

			var tags = GetSectionForeachTags();

			if (!ValidateSectionForeachTags(tags))
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("3955E019-A87E-4F4D-84C1-8FCCC684D337", "Error when processing {0}, please check documentation of {0} for how to use it.", nameof(SectionForeachArea)), ReportProcessingErrorSeverity.Error));
				return;
			}

			if (!tags.Any())
			{
				return;
			}

			for (var i = 0; i < tags.Count; i++)
			{
				var tag = tags[i];
				if (SectionForeachArea.IsSectionForeachAreaBeginStart(tag.cell))
				{
					var endRowInfo = FindEndIndex(i);
					var node = new SectionForeachArea(tag.row, endRowInfo.endRow, ParentReport, tag.cell, this, bodyArea);
					i = endRowInfo.endRowIndex;
				}
			}

			IsForeachProcessed = true;

			(int endRow, int endRowIndex) FindEndIndex(int start)
			{
				var currentBalance = 1;
				for (var index = start + 1; index < tags.Count; index++)
				{
					currentBalance += SectionForeachArea.IsSectionForeachAreaBeginStart(tags[index].cell) ? 1 : -1;

					if (currentBalance == 0)
					{
						return (tags[index].row, index);
					}
				}
				return (-1, -1);
			}
		}

		#region DataRowIndexWithRowRanges

		/// <summary>
		/// This property stores a relation between rows and which index the rows belong to the DataSource. This property need to be updated whenever the report add or remove rows
		/// </summary>
		internal Dictionary<int, RowRange> DataRowIndexWithRowRanges { get; set; } = new Dictionary<int, RowRange>();

		internal int LastRowIndex { get; private set; }
		Passes lastPass = Passes.FirstPass;
		internal int GetRowIndex(int row)
		{
			if (ParentReport.Renderer.IsProcessingMacros)
			{
				if (ParentReport.Renderer.CurrentPass != lastPass)
				{
					LastRowIndex = 0;
				}

				lastPass = ParentReport.Renderer.CurrentPass;
				while (DataRowIndexWithRowRanges.ContainsKey(LastRowIndex))
				{
					var rowRange = DataRowIndexWithRowRanges[LastRowIndex];
					if (row <= rowRange.End && row >= rowRange.Start)
					{
						return LastRowIndex;
					}

					LastRowIndex++;
				}

				return -1;
			}

			return GetRowIndexDynamically(row);
		}

		int GetRowIndexDynamically(int row)
		{
			foreach (var rowRange in DataRowIndexWithRowRanges)
			{
				if (row <= rowRange.Value.End && row >= rowRange.Value.Start)
				{
					return rowRange.Key;
				}
			}

			return -1;
		}

		internal void UpdateRowRangesWithIndex(int row, int delta)
		{
			DataRowIndexWithRowRanges.Where(d => d.Value.End >= row).ForEach(d =>
			{
				if (d.Value.Start > row)
				{
					d.Value.Start += delta;
				}
				d.Value.End += delta;
			});
		}

		#endregion

#if DEBUG

		internal string ToTreeViewString()
		{
			var result = string.Empty;
			ToTreeViewStringCore(this, string.Empty);
			return result;

			void ToTreeViewStringCore(TreeDataArea node, string level)
			{
				var breakLine = string.IsNullOrEmpty(level) ? string.Empty : System.Environment.NewLine;
				result += $"{breakLine}{level}{node.GetType().Name}:Start={node.StartingRow},End={node.End}";

				foreach (var child in node.Children)
				{
					ToTreeViewStringCore(child, level + "  ");
				}
			}
		}

#endif
	}
}
