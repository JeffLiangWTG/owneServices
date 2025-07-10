using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class SectionForeachArea : TreeDataArea
	{
		public SectionForeachArea(int start, int end, Report report, string parameterText, TreeDataArea parent, SectionBodyArea bodyArea) : base(start, end, report, parameterText, parent)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(bodyArea, nameof(bodyArea));

			SectionBody = bodyArea;
			SectionBody.AllChildForeachAreas.Add(this);
			parent.Children.Add(this);

			var param1 = Parameters.Length > 1 ? Parameters[1].Trim() : null;
			if (param1 == null || !param1.StartsWith("DATA=", StringComparison.OrdinalIgnoreCase))
			{
				throw new DocumentEngineException("Malformed BeginLoop Constructor. Should be: " + GetDocumentation().Useage + " But was: " + parameterText);
			}
		}

		SectionForeachArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			var explanation = ResString.GetMultilingualString("6E3A4976-8962-4B6F-8801-DB2C3FB26F5B",
				@"{0} Areas are used to output multiple rows from a given {1}. The {1} must be a Child Collection of the current {2} data source.
This Area can only be used within a {2} Area. It must begin with {3} and end with {4}.

E.g:
{5}
Here {6} and {7} are sub-collections of every record in {8}, and {9} is a sub-collection of every record in {6}.

Please note:
	1. the {1} of {10} must be a Collection.
	2. if the value of macros cannot be resolved via the Area {11}, {12} will try to resolve it against its parent's {11}.
	3. this Area is only to output records of collections and does not support SUM/TOTAL/Formula etc.
	4. The use of other contents on the same line as {3}/{4} is not allowed, the intended data will not show as those two lines will be removed at the end of the creation of the document.",
				"SectionForeach", "DataRowSource", "SectionBody", "#BeginLoop", "#EndLoop", @"
				#SectionBody:Data=XXDummyCollection
					<MacroA>
				#BeginLoop:Data=XXSubCollection1
					<MacroB>
				#BeginLoop:Data=XXXXSubSubCollection1
						<MacroC>
				#EndLoop
				#EndLoop
				#BeginLoop:Data=XXSubCollection2
					<MacroD>
				# EndLoop
", "XXSubCollection1", "XXSubCollection2", "XXDummyCollection", "XXXXSubSubCollection1", "SectionForeachArea",
				"DataSource", "DocEngine");
			return new ValueProviderDocumenter("#BeginLoop:Data={DataRowSource}", explanation);
		}

		public override Area Clone(int position)
		{
			throw new CloneAreaException("Cannot clone a #SectionForeach area.");
		}

		internal void CopySelfToParent(SectionBodyArea parent)
		{
			var copiedForeachArea = new SectionForeachArea(StartingRow, End, ParentReport, (NoResString)"#BeginLoop:Data=" + TableName, parent, parent);
			copiedForeachArea.SetDataRowSource(DataRowSource);
			copiedForeachArea.DataRowIndexWithRowRanges = DataRowIndexWithRowRanges;
			copiedForeachArea.IsForeachProcessed = IsForeachProcessed;
			CopySelfLocal(copiedForeachArea, this);

			void CopySelfLocal(SectionForeachArea target, SectionForeachArea source)
			{
				foreach (var foreachArea in source.Children.OfType<SectionForeachArea>())
				{
					var resultLocal = new SectionForeachArea(foreachArea.StartingRow, foreachArea.End, ParentReport, (NoResString)"#BeginLoop:Data=" + foreachArea.TableName, target, target.SectionBody);
					resultLocal.SetDataRowSource(foreachArea.DataRowSource);
					resultLocal.DataRowIndexWithRowRanges = foreachArea.DataRowIndexWithRowRanges;
					resultLocal.IsForeachProcessed = foreachArea.IsForeachProcessed;
					CopySelfLocal(resultLocal, foreachArea);
				}
			}
		}

		public override bool CanCloseAPage => true;

		internal override string Identifier => Constants.SectionForeachTags.BeginForeach;

		public override List<Area> Parents { get; } = new List<Area>();

		public override void ExpandForDataRows(int multiplier)
		{
			DataRowIndexWithRowRanges.Clear();
			int delta;

			if (multiplier < 0)
			{
				ParentReport.WorkSheetCurrentlyBeingProcessed.RemoveRows(StartOfBody, End);
				delta = StartOfBody - End;
			}
			else
			{
				for (var i = 0; i <= multiplier; i++)
				{
					DataRowIndexWithRowRanges.Add(i, new RowRange(StartOfBody + i * (End - StartOfBody), End - 1 + i * (End - StartOfBody)));
				}
				ParentReport.WorkSheetCurrentlyBeingProcessed.DuplicateRows(StartOfBody, End - 1, End, multiplier);
				delta = (End - StartOfBody) * multiplier;
			}

			var tempParent = Parent;
			while (tempParent != null)
			{
				tempParent.Children.Where(a => a.StartingRow > End).ForEach(a => a.Shift(delta));
				tempParent.ShiftEnd(delta);
				tempParent.UpdateRowRangesWithIndex(End, delta);

				tempParent = tempParent.Parent;
			}

			var bodyAreaIndex = ParentReport.Analyser.Areas.IndexOf(SectionBody);
			for (var i = bodyAreaIndex + 1; i < ParentReport.Analyser.Areas.Count; i++)
			{
				ParentReport.Analyser.Areas[i].Shift(delta);
			}

			fEnd += delta;
		}

		protected override int RealEnd => End - 1;

		internal void ExpandIncludingChildren()
		{
			var index = Parent.GetRowIndex(StartingRow);
			if (Parent.DataRowSource is BusinessObjectDataSource boDataSource && index != -1)
			{
				var provider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(boDataSource.GroupedOrFilteredCollection[index])), null);
				SetDataRowSource(provider.GetDataRowSource(TableName));
				if (StartOfBody != End)
				{
					ExpandForDataRows(DataRowSource.RowCount - 1);
				}
			}

			ProcessSectionForeachAreas(SectionBody);
			Children.OfType<SectionForeachArea>().ForEach(c => c.ExpandIncludingChildren());
		}

		public override IDataRowSource DataRowSource => dataRowSource;
		IDataRowSource dataRowSource;

		internal void SetDataRowSource(IDataRowSource source)
		{
			dataRowSource = source;
		}

		internal SectionBodyArea SectionBody { get; }

		internal static bool IsSectionForeachAreaBeginStart(string cellContents)
		{
			return cellContents.StartsWith(Constants.SectionForeachTags.BeginForeach, StringComparison.OrdinalIgnoreCase);
		}

		internal static bool IsSectionForeachAreaEndStart(string cellContents)
		{
			return cellContents.StartsWith(Constants.SectionForeachTags.EndForeach, StringComparison.OrdinalIgnoreCase);
		}
	}
}
