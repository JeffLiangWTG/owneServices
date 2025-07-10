using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ComponentGridTest : BMSTestCaseWithFactory
	{
		#region Row/column count

		public void TestNumberOfRowsAndColumns_Buffer_DontShowZones()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, false, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 14, "one column per cells/section plus one for channel heading", 3, "two channels, 0 zone headings, one age heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 8, "one column per cells/section", 4, "two subsections, 0 zone headings, two age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 9, "one column per cells/section", 6, "three subsections, 0 zone headings, three age headings");

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, false, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 14, "one column per cells/section plus one for channel heading", 3, "two channels, 0 zone headings, one age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 8, "one column per cells/section", 4, "two subsections, 0 zone headings, two age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 9, "one column per cells/section", 6, "three subsections, 0 zone headings, three age headings");

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, false, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 3, "two channels, 0 zone headings, one age headings", 14, "one row per cells/section plus one for channel heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 4, "two subsections, 0 zone headings, two age headings", 8, "one row per cells/section");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 6, "three subsections, 0 zone headings, three age headings", 9, "one row per cells/section");

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, false, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 3, "two channels, 0 zone headings, one age headings", 14, "one row per cells/section plus one for channel heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 4, "two subsections, 0 zone headings, two age headings", 8, "one row per cells/section");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled, false);
			AssertColumnsAndRows(pair, 6, "three subsections, 0 zone headings, three age headings", 9, "one row per cells/section");
		}

		public void TestNumberOfRowsAndColumns_Buffer()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 14, "one column per cells/section plus one for channel heading", 4, "two channels, one zone headings, one age heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 8, "one column per cells/section", 6, "two subsections, two zone headings, two age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 9, "one column per cells/section", 9, "three subsections, three zone headings, three age headings");

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 14, "one column per cells/section plus one for channel heading", 4, "two channels, one zone headings, one age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 8, "one column per cells/section", 6, "two subsections, two zone headings, two age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 9, "one column per cells/section", 9, "three subsections, three zone headings, three age headings");

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 4, "two channels, one zone headings, one age headings", 14, "one row per cells/section plus one for channel heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 6, "two subsections, two zone headings, two age headings", 8, "one row per cells/section");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 9, "three subsections, three zone headings, three age headings", 9, "one row per cells/section");

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertColumnsAndRows(pair, 4, "two channels, one zone headings, one age headings", 14, "one row per cells/section plus one for channel heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 6, "two subsections, two zone headings, two age headings", 8, "one row per cells/section");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 9, "three subsections, three zone headings, three age headings", 9, "one row per cells/section");
		}

		public void TestNumberOfRowsAndColumns_Bucket()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			AssertColumnsAndRows(pair, 14, "one column per cells/section plus one for channel heading", 4, "three channels and one age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 8, "one column per cells/section", 4, "two subsections and two age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 9, "one column per cells/section", 6, "three subsections and three age headings");

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			AssertColumnsAndRows(pair, 14, "one column per cells/section plus one for channel heading", 4, "three channels and one age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 8, "one column per cells/section", 4, "two subsections and two age headings");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 9, "one column per cells/section", 6, "three subsections and three age headings");

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			AssertColumnsAndRows(pair, 4, "three channels and one age headings", 14, "one row per cells/section plus one for channel heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 4, "two subsections and two age headings", 8, "one row per cells/section");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 6, "three subsections and three age headings", 9, "one row per cells/section");

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			AssertColumnsAndRows(pair, 4, "three channels and one age headings", 14, "one row per cells/section plus one for channel heading");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 4, "two subsections and two age headings", 8, "one row per cells/section");

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertColumnsAndRows(pair, 6, "three subsections and three age headings", 9, "one row per cells/section");
		}

		public void TestNumberOfRowsAndColumns_Unchanneled_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			BMBoardSectionTestHelper.RemoveAndDeleteAllPrimaryAxisChannels(section);
			AssertColumnsAndRows(pair, 1, "Just one card cell column", 1, "Just one card cell row");
		}

		public void TestNumberOfRowsAndColumns_Unchanneled_Buffer()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			BMBoardSectionTestHelper.RemoveAndDeleteAllPrimaryAxisChannels(section);
			AssertColumnsAndRows(pair, 3, "Zone header, age header and cell columns", 4, "Four card cell rows");
		}

		public void TestNumberOfRowsAndColumns_Channeled_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var section = pair.Item1;
			var viewModel = pair.Item2;
			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			AssertColumnsAndRows(pair, 2, "One channel header column and one card cell column", 3, "Three channel rows");
		}

		public void TestNumberOfRowsAndColumns_Channeled_Buffer()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var section = pair.Item1;
			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			AssertColumnsAndRows(pair, 5, "Zone header, age header and three channels", 5, "Four card cell rows and channel header row");
		}

		public void TestNumberOfRowsAndColumns_2dChannels_Bucket()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>()).Item1;
			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(2, BMBoardSectionTestHelper.GetSecondaryAxisChannelCount(section));
			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertColumnsAndRows(section, viewModel, 3, "Horizontal axis channel column, and one column for each channel on vertical axis",
				4, "Vertical axis channel row, and one row for each channel on horizontal axis");
		}

		static void AssertColumnsAndRows(BMBoardSection section, BMBoardSectionViewModel viewModel, int expectedColumns, string columnMessage, int expectedRows, string rowMessage)
		{
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);
			CombineAssertions(() =>
			{
				AssertEquals(string.Format("Should be {0} rows - {1}", expectedRows, rowMessage), expectedRows, grid.TotalRows);
				AssertEquals(string.Format("Should be {0} columns - {1}", expectedColumns, columnMessage), expectedColumns, grid.TotalColumns);
			});
		}

		static void AssertColumnsAndRows(Tuple<BMBoardSection, BMBoardSectionViewModel> pair, int expectedColumns, string columnMessage, int expectedRows, string rowMessage)
		{
			AssertColumnsAndRows(pair.Item1, pair.Item2, expectedColumns, columnMessage, expectedRows, rowMessage);
		}

		#endregion

		#region Cell content

		public void TestCellContent_Buffer()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* Empty cell then 13 age headings */ Enumerable.Range(0, 1).Select(i => CellContentType.Label).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.AgeHeading)).ToArray(),
					/* Empty cell then 13 zone headings */ Enumerable.Range(0, 1).Select(i => CellContentType.Label).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.ZoneHeading)).ToArray(),
					/* Channel header then 13 card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
					/* Channel header then 13 card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 zone headings */ Enumerable.Range(0, 8).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 zone headings */ Enumerable.Range(0, 8).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 zone headings */ Enumerable.Range(0, 9).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 zone headings */ Enumerable.Range(0, 9).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 zone headings */ Enumerable.Range(0, 9).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
				});

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* Empty cell then 13 age headings */ Enumerable.Range(0, 1).Select(i => CellContentType.Label).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.AgeHeading)).ToArray(),
					/* Empty cell then 13 zone headings */ Enumerable.Range(0, 1).Select(i => CellContentType.Label).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.ZoneHeading)).ToArray(),
					/* Channel header then 13 card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
					/* Channel header then 13 card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 zone headings */ Enumerable.Range(0, 8).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 zone headings */ Enumerable.Range(0, 8).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 zone headings */ Enumerable.Range(0, 9).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 zone headings */ Enumerable.Range(0, 9).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 zone headings */ Enumerable.Range(0, 9).Select(i => CellContentType.ZoneHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
				});

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			var topRow = new[] { CellContentType.Label, CellContentType.Label, CellContentType.ChannelHeading, CellContentType.ChannelHeading };
			var otherRows = new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 1).Select(i => topRow).Concat(Enumerable.Range(0, 13).Select(i => otherRows)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var allRows = new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 8).Select(i => allRows).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			allRows = new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 9).Select(i => allRows).ToArray());

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			topRow = new[] { CellContentType.Label, CellContentType.Label, CellContentType.ChannelHeading, CellContentType.ChannelHeading };
			otherRows = new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 1).Select(i => topRow).Concat(Enumerable.Range(0, 13).Select(i => otherRows)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			allRows = new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 8).Select(i => allRows).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			allRows = new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 9).Select(i => allRows).ToArray());
		}

		public void TestCellContent_BufferWithCCRAndNotShownZones()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			// Flowing left - horizontal orientation
			const int cellsPerSubsection = 4;
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, cellsPerSubsection, FlowDirectionList.Codes.Left, string.Empty, ChannelTypeList.Codes.Resource, showZones: false, releaseGroup: config.ReleaseGroup.PK);

			Factory.Save();

			var section = pair.Item1;
			var viewModel = pair.Item2;
			Assert("Should be in constrained mode", viewModel.IsInConstrainedMode);

			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* Empty cell then age headings */ Enumerable.Range(0, 1).Select(i => CellContentType.Label).Concat(Enumerable.Range(0, cellsPerSubsection).Select(i => CellContentType.AgeHeading)).ToArray(),
					/* Channel header for CCR then card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, cellsPerSubsection).Select(i => CellContentType.Cards)).ToArray(),
					/* Channel header for non-CCR then card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, cellsPerSubsection).Select(i => CellContentType.Cards)).ToArray(),
					/* Channel header for non-CCR then card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, cellsPerSubsection).Select(i => CellContentType.Cards)).ToArray(),
				});
		}

		public void TestCellContent_Bucket()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* Empty cell then 13 age headers */ new[] { CellContentType.Label }.Concat(Enumerable.Range(0, 13).Select(i => CellContentType.AgeHeading)).ToArray(),
					/* Channel header then 13 card cells */ new[] { CellContentType.ChannelHeading }.Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
					/* Channel header then 13 card cells */ new[] { CellContentType.ChannelHeading }.Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
				});

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* Empty cell then 13 age headers */ new[] { CellContentType.Label }.Concat(Enumerable.Range(0, 13).Select(i => CellContentType.AgeHeading)).ToArray(),
					/* Channel header then 13 card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
					/* Channel header then 13 card cells */ Enumerable.Range(0, 1).Select(i => CellContentType.ChannelHeading).Concat(Enumerable.Range(0, 13).Select(i => CellContentType.Cards)).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
					/* 8 age headings */ Enumerable.Range(0, 8).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 8 card cells */ Enumerable.Range(0, 8).Select(i => CellContentType.Cards).ToArray(),
				});

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellContents(pair,
				new CellContentType[][]
				{
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
					/* 9 age headings */ Enumerable.Range(0, 9).Select(i => CellContentType.AgeHeading).ToArray(),
					/* 9 card cells */ Enumerable.Range(0, 9).Select(i => CellContentType.Cards).ToArray(),
				});

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			var topRow = new[] { CellContentType.Label, CellContentType.ChannelHeading, CellContentType.ChannelHeading };
			var otherRows = new[] { CellContentType.AgeHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 1).Select(i => topRow).Concat(Enumerable.Range(0, 13).Select(i => otherRows)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var allRows = new[] { CellContentType.AgeHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 8).Select(i => allRows).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			allRows = new[] { CellContentType.AgeHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 9).Select(i => allRows).ToArray());

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			topRow = new[] { CellContentType.Label, CellContentType.ChannelHeading, CellContentType.ChannelHeading };
			otherRows = new[] { CellContentType.AgeHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 1).Select(i => topRow).Concat(Enumerable.Range(0, 13).Select(i => otherRows)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			allRows = new[] { CellContentType.AgeHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 8).Select(i => allRows).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			allRows = new[] { CellContentType.AgeHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.Cards };
			AssertCellContents(pair, Enumerable.Range(0, 9).Select(i => allRows).ToArray());
		}

		public void TestCellContent_Unchanneled_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			BMBoardSectionTestHelper.RemoveAndDeleteAllPrimaryAxisChannels(pair.Item1);
			AssertCellContents(pair, new CellContentType[][]
			{
				new CellContentType[] { CellContentType.Cards },
			});
		}

		public void TestCellContent_Unchanneled_Buffer()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			BMBoardSectionTestHelper.RemoveAndDeleteAllPrimaryAxisChannels(pair.Item1);
			AssertCellContents(pair, GetExpectedContentTypes(4, new CellContentType[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards }));
		}

		public void TestCellContent_Channeled_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(pair.Item1));
			AssertCellContents(pair, new CellContentType[][]
			{
				new CellContentType[] { CellContentType.ChannelHeading, CellContentType.Cards },
				new CellContentType[] { CellContentType.ChannelHeading, CellContentType.Cards },
				new CellContentType[] { CellContentType.ChannelHeading, CellContentType.Cards },
			});
		}

		public void TestCellContent_Channeled_Buffer()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(pair.Item1));
			AssertCellContents(pair, new CellContentType[][]
			{
				new CellContentType[] { CellContentType.Label, CellContentType.Label, CellContentType.ChannelHeading, CellContentType.ChannelHeading, CellContentType.ChannelHeading },
				new CellContentType[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.Cards, CellContentType.Cards },
				new CellContentType[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.Cards, CellContentType.Cards },
				new CellContentType[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.Cards, CellContentType.Cards },
				new CellContentType[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.Cards, CellContentType.Cards },
			});
		}

		public void TestCellContent_2dChannels_Bucket()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>()).Item1;
			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(2, BMBoardSectionTestHelper.GetSecondaryAxisChannelCount(section));
			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertCellContents(section, viewModel, new CellContentType[][]
			{
				new CellContentType[] { CellContentType.Label, CellContentType.ChannelHeading, CellContentType.ChannelHeading, },
				new CellContentType[] { CellContentType.ChannelHeading, CellContentType.Cards, CellContentType.Cards, },
				new CellContentType[] { CellContentType.ChannelHeading, CellContentType.Cards, CellContentType.Cards, },
				new CellContentType[] { CellContentType.ChannelHeading, CellContentType.Cards, CellContentType.Cards, },
			});
		}

		static void AssertCellContents(BMBoardSection section, BMBoardSectionViewModel viewModel, CellContentType[][] expectedContentTypes)
		{
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);
			for (int row = 0; row < expectedContentTypes.Length; row++)
			{
				var rowTypes = expectedContentTypes[row];
				for (int col = 0; col < rowTypes.Length; col++)
				{
					var actualCell = grid[row, col];
					var expectedType = expectedContentTypes[row][col];
					var message = string.Format("Content at row={0} col={1} should have CellContentType.{2}", actualCell.Row, actualCell.Column, expectedType);
					AssertEquals(message, expectedType, actualCell.ContentType);
				}
			}
		}

		static void AssertCellContents(Tuple<BMBoardSection, BMBoardSectionViewModel> pair, CellContentType[][] expectedContentTypes)
		{
			AssertCellContents(pair.Item1, pair.Item2, expectedContentTypes);
		}

		#endregion

		#region Zone Number

		public void TestZoneNumber_StandardDirection()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(10, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(6, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(2, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(1, 1).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(4, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(0, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(4, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(0, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 8).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone2Coords: new[] { new Point(0, zoneHeadingPosition) }.Concat(Enumerable.Range(2, 7).Select(i => new Point(i, 3 + zoneHeadingPosition))).ToArray(),
				zone1Coords: new[] { new Point(0, 3 + zoneHeadingPosition), new Point(1, 3 + zoneHeadingPosition) }.Concat(Enumerable.Range(3, 6).Select(i => new Point(i, 6 + zoneHeadingPosition))).ToArray(),
				zone0Coords: Enumerable.Range(0, 3).Select(i => new Point(i, 6 + zoneHeadingPosition)).ToArray());

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(5, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(9, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(13, 1).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 4).Select(i => new Point(i, 0 + zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(4, 4).Select(i => new Point(i, 0 + zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(0, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(4, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 8).Select(i => new Point(i, 0 + zoneHeadingPosition)).ToArray(),
				zone2Coords: new[] { new Point(8, 0 + zoneHeadingPosition) }.Concat(Enumerable.Range(0, 7).Select(i => new Point(i, 3 + zoneHeadingPosition))).ToArray(),
				zone1Coords: new[] { new Point(7, 3 + zoneHeadingPosition), new Point(8, 3 + zoneHeadingPosition) }.Concat(Enumerable.Range(0, 6).Select(i => new Point(i, 6 + zoneHeadingPosition))).ToArray(),
				zone0Coords: Enumerable.Range(6, 3).Select(i => new Point(i, 6 + zoneHeadingPosition)).ToArray());

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(10, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(6, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(2, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(1, 1).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(4, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(0, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(4, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(0, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 8).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: new[] { new Point(zoneHeadingPosition, 0) }.Concat(Enumerable.Range(2, 7).Select(i => new Point(3 + zoneHeadingPosition, i))).ToArray(),
				zone1Coords: new[] { new Point(3 + zoneHeadingPosition, 0), new Point(3 + zoneHeadingPosition, 1) }.Concat(Enumerable.Range(3, 6).Select(i => new Point(6 + zoneHeadingPosition, i))).ToArray(),
				zone0Coords: Enumerable.Range(0, 3).Select(i => new Point(6 + zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 5, 1, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: new[] { new Point(13, 0) },
				zone2Coords: new[] { new Point(10, 0) },
				zone1Coords: new[] { new Point(7, 0) },
				zone0Coords: new[] { new Point(1, 0), new Point(4, 0) });

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(5, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(9, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(13, 1).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(4, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(0, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(4, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 8).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: new[] { new Point(zoneHeadingPosition, 8) }.Concat(Enumerable.Range(0, 7).Select(i => new Point(3 + zoneHeadingPosition, i))).ToArray(),
				zone1Coords: new[] { new Point(3 + zoneHeadingPosition, 7), new Point(3 + zoneHeadingPosition, 8) }.Concat(Enumerable.Range(0, 6).Select(i => new Point(6 + zoneHeadingPosition, i))).ToArray(),
				zone0Coords: Enumerable.Range(6, 3).Select(i => new Point(6 + zoneHeadingPosition, i)).ToArray());
		}

		public void TestZoneNumber_Retrograde()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Top, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(10, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(6, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(2, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(1, 1).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(4, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(0, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(4, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(0, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 8).Select(i => new Point(i, 6 + zoneHeadingPosition)).ToArray(),
				zone2Coords: new[] { new Point(0, 6 + zoneHeadingPosition) }.Concat(Enumerable.Range(2, 7).Select(i => new Point(i, 3 + zoneHeadingPosition))).ToArray(),
				zone1Coords: new[] { new Point(0, 3 + zoneHeadingPosition), new Point(1, 3 + zoneHeadingPosition) }.Concat(Enumerable.Range(3, 6).Select(i => new Point(i, zoneHeadingPosition))).ToArray(),
				zone0Coords: Enumerable.Range(0, 3).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(5, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(9, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(13, 1).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray(),
				zone2Coords: Enumerable.Range(4, 4).Select(i => new Point(i, 3 + zoneHeadingPosition)).ToArray(),
				zone1Coords: Enumerable.Range(0, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray(),
				zone0Coords: Enumerable.Range(4, 4).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 8).Select(i => new Point(i, 6 + zoneHeadingPosition)).ToArray(),
				zone2Coords: new[] { new Point(8, 6 + zoneHeadingPosition) }.Concat(Enumerable.Range(0, 7).Select(i => new Point(i, 3 + zoneHeadingPosition))).ToArray(),
				zone1Coords: new[] { new Point(7, 3 + zoneHeadingPosition), new Point(8, 3 + zoneHeadingPosition) }.Concat(Enumerable.Range(0, 6).Select(i => new Point(i, zoneHeadingPosition))).ToArray(),
				zone0Coords: Enumerable.Range(6, 3).Select(i => new Point(i, zoneHeadingPosition)).ToArray());

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(10, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(6, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(2, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(1, 1).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(4, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(0, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(4, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(0, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 8).Select(i => new Point(6 + zoneHeadingPosition, i)).ToArray(),
				zone2Coords: new[] { new Point(6 + zoneHeadingPosition, 0) }.Concat(Enumerable.Range(2, 7).Select(i => new Point(3 + zoneHeadingPosition, i))).ToArray(),
				zone1Coords: new[] { new Point(3 + zoneHeadingPosition, 0), new Point(3 + zoneHeadingPosition, 1) }.Concat(Enumerable.Range(3, 6).Select(i => new Point(zoneHeadingPosition, i))).ToArray(),
				zone0Coords: Enumerable.Range(0, 3).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Left, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(1, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(5, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(9, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(13, 1).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray(),
				zone2Coords: Enumerable.Range(4, 4).Select(i => new Point(3 + zoneHeadingPosition, i)).ToArray(),
				zone1Coords: Enumerable.Range(0, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray(),
				zone0Coords: Enumerable.Range(4, 4).Select(i => new Point(zoneHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertZoneNumbers(pair,
				zone3Coords: Enumerable.Range(0, 8).Select(i => new Point(6 + zoneHeadingPosition, i)).ToArray(),
				zone2Coords: new[] { new Point(6 + zoneHeadingPosition, 8) }.Concat(Enumerable.Range(0, 7).Select(i => new Point(3 + zoneHeadingPosition, i))).ToArray(),
				zone1Coords: new[] { new Point(3 + zoneHeadingPosition, 7), new Point(3 + zoneHeadingPosition, 8) }.Concat(Enumerable.Range(0, 6).Select(i => new Point(zoneHeadingPosition, i))).ToArray(),
				zone0Coords: Enumerable.Range(6, 3).Select(i => new Point(zoneHeadingPosition, i)).ToArray());
		}

		public void TestIsLastAgeIndexForZone()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			for (var i = 2; i < 4; i++)
			{
				AssertEquals(false, grid[i, 13].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 12].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 11].IsLastAgeIndexForZone);
				AssertEquals(true, grid[i, 10].IsLastAgeIndexForZone);

				AssertEquals(false, grid[i, 9].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 8].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 7].IsLastAgeIndexForZone);
				AssertEquals(true, grid[i, 6].IsLastAgeIndexForZone);

				AssertEquals(false, grid[i, 5].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 4].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 3].IsLastAgeIndexForZone);
				AssertEquals(true, grid[i, 2].IsLastAgeIndexForZone);

				AssertEquals(false, grid[i, 1].IsLastAgeIndexForZone);
				AssertEquals(false, grid[i, 0].IsLastAgeIndexForZone);
			}
		}

		static void AssertZoneNumbers(BMBoardSection section, BMBoardSectionViewModel viewModel, Point[] zone3Coords, Point[] zone2Coords, Point[] zone1Coords, Point[] zone0Coords, bool arePreConstraintZones = false, bool arePostConstraintZones = false)
		{
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);
			AssertZone(grid, zone3Coords, 3, arePreConstraintZones, arePostConstraintZones);
			AssertZone(grid, zone2Coords, 2, arePreConstraintZones, arePostConstraintZones);
			AssertZone(grid, zone1Coords, 1, arePreConstraintZones, arePostConstraintZones);
			AssertZone(grid, zone0Coords, 0, arePreConstraintZones, arePostConstraintZones);
		}

		static void AssertZoneNumbers(Tuple<BMBoardSection, BMBoardSectionViewModel> pair, Point[] zone3Coords, Point[] zone2Coords, Point[] zone1Coords, Point[] zone0Coords, bool arePreConstraintZones = false, bool arePostConstraintZones = false)
		{
			AssertZoneNumbers(pair.Item1, pair.Item2, zone3Coords, zone2Coords, zone1Coords, zone0Coords, arePreConstraintZones, arePostConstraintZones);
		}

		static void AssertZone(ComponentGrid grid, Point[] zoneCoords, int expectedZone, bool arePreConstraintZones, bool arePostConstraintZones)
		{
			foreach (var point in zoneCoords)
			{
				var cell = grid[point.Y, point.X];
				var message = string.Format("Cell at row={0} col={1} should be a zone heading for Zone {2}", cell.Row, cell.Column, expectedZone);

				if (arePreConstraintZones)
				{
					AssertEquals(message, CellContentType.SubComponentZoneHeading, cell.ContentType);
					AssertEquals(message, expectedZone, cell.Zone);
					AssertEquals(message, "Zone " + expectedZone, cell.Label);
				}
				else if (arePostConstraintZones)
				{
					AssertEquals(message, CellContentType.SubComponentZoneHeading, cell.ContentType);
					AssertEquals(message, expectedZone, cell.Zone);
					AssertEquals(message, "Zone " + expectedZone, cell.Label);
				}
				else
				{
					AssertEquals(message, CellContentType.ZoneHeading, cell.ContentType);
					AssertEquals(message, expectedZone, cell.Zone);
					AssertEquals(message, "Zone " + expectedZone, cell.Label);
				}
			}
		}

		#endregion

		#region Cell Age

		public void TestCellAge_StandardDirection()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Reverse().Select(i => new Point(i, ageHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Reverse().Select(i => new Point(i, 2))
				.Concat(Enumerable.Range(0, 8).Reverse().Select(i => new Point(i, 5))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Reverse().Select(i => new Point(i, 2))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(i, 5)))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(i, 8))).ToArray());

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Select(i => new Point(i, ageHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Select(i => new Point(i, 2))
				.Concat(Enumerable.Range(0, 8).Select(i => new Point(i, 5))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Select(i => new Point(i, 2))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(i, 5)))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(i, 8))).ToArray());

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Reverse().Select(i => new Point(ageHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Reverse().Select(i => new Point(2, i))
				.Concat(Enumerable.Range(0, 8).Reverse().Select(i => new Point(5, i))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Reverse().Select(i => new Point(2, i))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(5, i)))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(8, i))).ToArray());

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Select(i => new Point(ageHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Select(i => new Point(2, i))
				.Concat(Enumerable.Range(0, 8).Select(i => new Point(5, i))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Select(i => new Point(2, i))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(5, i)))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(8, i))).ToArray());
		}

		public void TestCellAge_CountDown()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;

			for (var row = 0; row < 3; row++)
			{
				AssertCellAge(pair, Enumerable.Range(1, 4).Select(i => new Point(i, row)).ToArray(), new[] { 1, 0, -1, -2 });
			}
		}

		public void TestCellAge_Retrograde()
		{
			// Flowing left - horizontal orientation
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Top, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Reverse().Select(i => new Point(i, ageHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Reverse().Select(i => new Point(i, 5))
				.Concat(Enumerable.Range(0, 8).Reverse().Select(i => new Point(i, 2))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Left, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Reverse().Select(i => new Point(i, 8))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(i, 5)))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(i, 2))).ToArray());

			// Flowing right - horizontal orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Select(i => new Point(i, ageHeadingPosition)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Select(i => new Point(i, 5))
				.Concat(Enumerable.Range(0, 8).Select(i => new Point(i, 2))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Select(i => new Point(i, 8))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(i, 5)))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(i, 2))).ToArray());

			// Flowing up - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Reverse().Select(i => new Point(ageHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Reverse().Select(i => new Point(5, i))
				.Concat(Enumerable.Range(0, 8).Reverse().Select(i => new Point(2, i))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Reverse().Select(i => new Point(8, i))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(5, i)))
				.Concat(Enumerable.Range(0, 9).Reverse().Select(i => new Point(2, i))).ToArray());

			// Flowing down - vertical orientation
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Left, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			AssertCellAge(pair, Enumerable.Range(1, 13).Select(i => new Point(ageHeadingPosition, i)).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Down, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 8).Select(i => new Point(5, i))
				.Concat(Enumerable.Range(0, 8).Select(i => new Point(2, i))).ToArray());

			pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 3, 9, FlowDirectionList.Codes.Down, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			AssertCellAge(pair, Enumerable.Range(0, 9).Select(i => new Point(8, i))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(5, i)))
				.Concat(Enumerable.Range(0, 9).Select(i => new Point(2, i))).ToArray());
		}

		static void AssertCellAge(Tuple<BMBoardSection, BMBoardSectionViewModel> pair, Point[] orderedCellAge, int[] ages = null)
		{
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			CombineAssertions(() =>
			{
				for (int i = 0; i < orderedCellAge.Length; i++)
				{
					var point = orderedCellAge[i];
					var cell = grid[point.Y, point.X];

					var message = string.Format("Cell at row={0} col={1} AgeIndex", cell.Row, cell.Column);
					var expectedAge = ages == null ? i : ages[i];
					AssertEquals(message, expectedAge, cell.TimeIndex);
				}
			});
		}

		#endregion

		#region Age Labels

		public void TestAgeLabels_ForegroundAndBackgroundColours()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96 * 60;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			var backColor = pair.Item1.BackgroundColorValue;

			CombineAssertions(() =>
			{
				AssertEquals(backColor, grid[ageHeadingPosition, 1].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 1].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 2].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 2].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 3].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 3].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 4].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 4].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 5].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 5].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 6].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 6].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 7].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 7].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 8].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 8].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 9].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 9].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 10].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 10].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 11].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 11].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 12].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 12].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 13].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 13].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 14].BackColor);
				AssertEquals(Color.Black, grid[ageHeadingPosition, 14].ForeColor);
			});

			pair.Item1.ForegroundColor = "White Smoke";
			grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			CombineAssertions(() =>
			{
				AssertEquals(backColor, grid[ageHeadingPosition, 1].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 1].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 2].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 2].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 3].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 3].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 4].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 4].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 5].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 5].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 6].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 6].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 7].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 7].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 8].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 8].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 9].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 9].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 10].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 10].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 11].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 11].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 12].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 12].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 13].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 13].ForeColor);

				AssertEquals(backColor, grid[ageHeadingPosition, 14].BackColor);
				AssertEquals(Color.WhiteSmoke, grid[ageHeadingPosition, 14].ForeColor);
			});
		}

		public void TestAgeLabels_Buffer_Weeks()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96 * 60 * 5;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			AssertStandard14SlotBufferPercentLabels(grid);
		}

		public void TestAgeLabels_Buffer_Weeks_CountDown()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96 * 60 * 5;
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			CombineAssertions(() =>
			{
				AssertEquals("-8.3 %", grid[ageHeadingPosition, 1].Label);
				AssertEquals("0 %", grid[ageHeadingPosition, 2].Label);
				AssertEquals("8.3 %", grid[ageHeadingPosition, 3].Label);
				AssertEquals("16.7 %", grid[ageHeadingPosition, 4].Label);
				AssertEquals("25 %", grid[ageHeadingPosition, 5].Label);
				AssertEquals("33.3 %", grid[ageHeadingPosition, 6].Label);
				AssertEquals("41.7 %", grid[ageHeadingPosition, 7].Label);
				AssertEquals("50 %", grid[ageHeadingPosition, 8].Label);
				AssertEquals("58.3 %", grid[ageHeadingPosition, 9].Label);
				AssertEquals("66.7 %", grid[ageHeadingPosition, 10].Label);
				AssertEquals("75 %", grid[ageHeadingPosition, 11].Label);
				AssertEquals("83.3 %", grid[ageHeadingPosition, 12].Label);
				AssertEquals("91.7 %", grid[ageHeadingPosition, 13].Label);
				AssertEquals("91.7 %+", grid[ageHeadingPosition, 14].Label);
			});
		}

		public void TestAgeLabels_Buffer_Days()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96 * 60;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			AssertStandard14SlotBufferPercentLabels(grid);
		}

		public void TestAgeLabels_Buffer_Hours()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 24 * 60;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			AssertStandard14SlotBufferPercentLabels(grid);
		}

		public void TestAgeLabels_Buffer_Minutes()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 24;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			AssertStandard14SlotBufferPercentLabels(grid);
		}

		void AssertStandard14SlotBufferPercentLabels(ComponentGrid grid)
		{
			CombineAssertions(() =>
			{
				AssertEquals("108.3 %+", grid[ageHeadingPosition, 1].Label);
				AssertEquals("108.3 %", grid[ageHeadingPosition, 2].Label);
				AssertEquals("100 %", grid[ageHeadingPosition, 3].Label);
				AssertEquals("91.7 %", grid[ageHeadingPosition, 4].Label);
				AssertEquals("83.3 %", grid[ageHeadingPosition, 5].Label);
				AssertEquals("75 %", grid[ageHeadingPosition, 6].Label);
				AssertEquals("66.7 %", grid[ageHeadingPosition, 7].Label);
				AssertEquals("58.3 %", grid[ageHeadingPosition, 8].Label);
				AssertEquals("50 %", grid[ageHeadingPosition, 9].Label);
				AssertEquals("41.7 %", grid[ageHeadingPosition, 10].Label);
				AssertEquals("33.3 %", grid[ageHeadingPosition, 11].Label);
				AssertEquals("25 %", grid[ageHeadingPosition, 12].Label);
				AssertEquals("16.7 %", grid[ageHeadingPosition, 13].Label);
				AssertEquals("8.3 %", grid[ageHeadingPosition, 14].Label);
			});
		}

		public void TestAgeLabels_Buffer_3Minutes7Cells()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 7, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 3;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			CombineAssertions(() =>
			{
				AssertEquals("100 %+", grid[ageHeadingPosition, 1].Label);
				AssertEquals("100 %", grid[ageHeadingPosition, 2].Label);
				AssertEquals("83.3 %", grid[ageHeadingPosition, 3].Label);
				AssertEquals("66.7 %", grid[ageHeadingPosition, 4].Label);
				AssertEquals("50 %", grid[ageHeadingPosition, 5].Label);
				AssertEquals("33.3 %", grid[ageHeadingPosition, 6].Label);
				AssertEquals("16.7 %", grid[ageHeadingPosition, 7].Label);
			});
		}

		public void TestAgeLabels_PercentPenetrationProperlyCalculated()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 4, 8, FlowDirectionList.Codes.Up, LastCellList.Codes.Left, ChannelTypeList.Codes.NotChanneled);
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			CombineAssertions(() =>
			{
				AssertEquals("129.2 %+", grid[ageHeadingPosition, 0].Label);
				AssertEquals("100 %", grid[ageHeadingPosition, 3].Label);
				AssertEquals("66.7 %", grid[ageHeadingPosition, 6].Label);
				AssertEquals("33.3 %", grid[ageHeadingPosition, 9].Label);
			});
		}

		public void TestAgeLabels_Buffer_3Minutes4Cells()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 3;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			CombineAssertions(() =>
			{
				AssertEquals("100 %+", grid[ageHeadingPosition, 1].Label);
				AssertEquals("100 %", grid[ageHeadingPosition, 2].Label);
				AssertEquals("66.7 %", grid[ageHeadingPosition, 3].Label);
				AssertEquals("33.3 %", grid[ageHeadingPosition, 4].Label);
			});
		}

		public void TestAgeLabels_Bucket_Weeks()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(8 * 60 * 5).GetDateTimeFromMinutes();
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("13w+", grid[0, 1].Label);
			AssertEquals("13w", grid[0, 2].Label);
			AssertEquals("12w", grid[0, 3].Label);
			AssertEquals("11w", grid[0, 4].Label);
			AssertEquals("10w", grid[0, 5].Label);
			AssertEquals("9w", grid[0, 6].Label);
			AssertEquals("8w", grid[0, 7].Label);
			AssertEquals("7w", grid[0, 8].Label);
			AssertEquals("6w", grid[0, 9].Label);
			AssertEquals("5w", grid[0, 10].Label);
			AssertEquals("4w", grid[0, 11].Label);
			AssertEquals("3w", grid[0, 12].Label);
			AssertEquals("2w", grid[0, 13].Label);
			AssertEquals("1w", grid[0, 14].Label);
		}

		public void TestAgeLabels_Bucket_Weeks_Overdue()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(8 * 60 * 5).GetDateTimeFromMinutes();
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("11w+", grid[0, 1].Label);
			AssertEquals("11w", grid[0, 2].Label);
			AssertEquals("10w", grid[0, 3].Label);
			AssertEquals("9w", grid[0, 4].Label);
			AssertEquals("8w", grid[0, 5].Label);
			AssertEquals("7w", grid[0, 6].Label);
			AssertEquals("6w", grid[0, 7].Label);
			AssertEquals("5w", grid[0, 8].Label);
			AssertEquals("4w", grid[0, 9].Label);
			AssertEquals("3w", grid[0, 10].Label);
			AssertEquals("2w", grid[0, 11].Label);
			AssertEquals("1w", grid[0, 12].Label);
			AssertEquals("-1w", grid[0, 13].Label);
			AssertEquals("-2w", grid[0, 14].Label);
		}

		public void TestAgeLabels_BucketWithMultipleSubsections_Weeks_Overdue()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 7, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(8 * 60 * 5).GetDateTimeFromMinutes();
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("11w+", grid[0, 0].Label);
			AssertEquals("11w", grid[0, 1].Label);
			AssertEquals("10w", grid[0, 2].Label);
			AssertEquals("9w", grid[0, 3].Label);
			AssertEquals("8w", grid[0, 4].Label);
			AssertEquals("7w", grid[0, 5].Label);
			AssertEquals("6w", grid[0, 6].Label);
			AssertEquals("5w", grid[2, 0].Label);
			AssertEquals("4w", grid[2, 1].Label);
			AssertEquals("3w", grid[2, 2].Label);
			AssertEquals("2w", grid[2, 3].Label);
			AssertEquals("1w", grid[2, 4].Label);
			AssertEquals("-1w", grid[2, 5].Label);
			AssertEquals("-2w", grid[2, 6].Label);
		}

		public void TestAgeLabels_Bucket_Weeks_CountDown()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(8 * 60 * 5).GetDateTimeFromMinutes();
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("-2w", grid[0, 1].Label);
			AssertEquals("-1w", grid[0, 2].Label);
			AssertEquals("1w", grid[0, 3].Label);
			AssertEquals("2w", grid[0, 4].Label);
			AssertEquals("3w", grid[0, 5].Label);
			AssertEquals("4w", grid[0, 6].Label);
			AssertEquals("5w", grid[0, 7].Label);
			AssertEquals("6w", grid[0, 8].Label);
			AssertEquals("7w", grid[0, 9].Label);
			AssertEquals("8w", grid[0, 10].Label);
			AssertEquals("9w", grid[0, 11].Label);
			AssertEquals("10w", grid[0, 12].Label);
			AssertEquals("11w", grid[0, 13].Label);
			AssertEquals("11w+", grid[0, 14].Label);
		}

		public void TestAgeLabels_Bucket_Days()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 2, 7, FlowDirectionList.Codes.Right, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(8 * 60).GetDateTimeFromMinutes();
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("13d+", grid[0, 6].Label);
			AssertEquals("13d", grid[0, 5].Label);
			AssertEquals("12d", grid[0, 4].Label);
			AssertEquals("11d", grid[0, 3].Label);
			AssertEquals("10d", grid[0, 2].Label);
			AssertEquals("9d", grid[0, 1].Label);
			AssertEquals("8d", grid[0, 0].Label);
			AssertEquals("7d", grid[2, 6].Label);
			AssertEquals("6d", grid[2, 5].Label);
			AssertEquals("5d", grid[2, 4].Label);
			AssertEquals("4d", grid[2, 3].Label);
			AssertEquals("3d", grid[2, 2].Label);
			AssertEquals("2d", grid[2, 1].Label);
			AssertEquals("1d", grid[2, 0].Label);
		}

		public void TestAgeLabels_Bucket_Hours()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 14, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(2 * 60).GetDateTimeFromMinutes();
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("2h", grid[0, 1].Label);
			AssertEquals("4h", grid[0, 2].Label);
			AssertEquals("6h", grid[0, 3].Label);
			AssertEquals("8h", grid[0, 4].Label);
			AssertEquals("10h", grid[0, 5].Label);
			AssertEquals("12h", grid[0, 6].Label);
			AssertEquals("14h", grid[0, 7].Label);
			AssertEquals("16h", grid[0, 8].Label);
			AssertEquals("18h", grid[0, 9].Label);
			AssertEquals("20h", grid[0, 10].Label);
			AssertEquals("22h", grid[0, 11].Label);
			AssertEquals("24h", grid[0, 12].Label);
			AssertEquals("26h", grid[0, 13].Label);
			AssertEquals("26h+", grid[0, 14].Label);
		}

		public void TestAgeLabels_Bucket_Minutes()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(2).GetDateTimeFromMinutes();
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("26m+", grid[0, 1].Label);
			AssertEquals("26m", grid[0, 2].Label);
			AssertEquals("24m", grid[0, 3].Label);
			AssertEquals("22m", grid[0, 4].Label);
			AssertEquals("20m", grid[0, 5].Label);
			AssertEquals("18m", grid[0, 6].Label);
			AssertEquals("16m", grid[0, 7].Label);
			AssertEquals("14m", grid[0, 8].Label);
			AssertEquals("12m", grid[0, 9].Label);
			AssertEquals("10m", grid[0, 10].Label);
			AssertEquals("8m", grid[0, 11].Label);
			AssertEquals("6m", grid[0, 12].Label);
			AssertEquals("4m", grid[0, 13].Label);
			AssertEquals("2m", grid[0, 14].Label);
		}

		public void TestAgeLabels_Buffer_14Slot()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 14, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96 * 60;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("108.3 %+", grid[ageHeadingPosition, 1].Label);
			AssertEquals("108.3 %", grid[ageHeadingPosition, 2].Label);
			AssertEquals("100 %", grid[ageHeadingPosition, 3].Label);
			AssertEquals("91.7 %", grid[ageHeadingPosition, 4].Label);
			AssertEquals("83.3 %", grid[ageHeadingPosition, 5].Label);
			AssertEquals("75 %", grid[ageHeadingPosition, 6].Label);
			AssertEquals("66.7 %", grid[ageHeadingPosition, 7].Label);
			AssertEquals("58.3 %", grid[ageHeadingPosition, 8].Label);
			AssertEquals("50 %", grid[ageHeadingPosition, 9].Label);
			AssertEquals("41.7 %", grid[ageHeadingPosition, 10].Label);
			AssertEquals("33.3 %", grid[ageHeadingPosition, 11].Label);
			AssertEquals("25 %", grid[ageHeadingPosition, 12].Label);
			AssertEquals("16.7 %", grid[ageHeadingPosition, 13].Label);
			AssertEquals("8.3 %", grid[ageHeadingPosition, 14].Label);
		}

		public void TestAgeLabels_Buffer_11Slot()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 11, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.Component.FC_BufferTimespanInMinutes = 96 * 60;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertEquals("111.1 %+", grid[ageHeadingPosition, 1].Label);
			AssertEquals("111.1 %", grid[ageHeadingPosition, 2].Label);
			AssertEquals("100 %", grid[ageHeadingPosition, 3].Label);
			AssertEquals("88.9 %", grid[ageHeadingPosition, 4].Label);
			AssertEquals("77.8 %", grid[ageHeadingPosition, 5].Label);
			AssertEquals("66.7 %", grid[ageHeadingPosition, 6].Label);
			AssertEquals("55.6 %", grid[ageHeadingPosition, 7].Label);
			AssertEquals("44.4 %", grid[ageHeadingPosition, 8].Label);
			AssertEquals("33.3 %", grid[ageHeadingPosition, 9].Label);
			AssertEquals("22.2 %", grid[ageHeadingPosition, 10].Label);
			AssertEquals("11.1 %", grid[ageHeadingPosition, 11].Label);
		}

		#endregion

		#region Back/Fore Colors

		public void TestBackForeColors_Buffer_DefaultColors()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser);
			pair.Item1.ForegroundColor = ZString.Empty;
			pair.Item1.BackgroundColor = ZString.Empty;
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertColorEquals(Color.Black, grid[0, 0].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 0].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 1].BackColor.Value);
			AssertColorEquals(BMConstants.Zone3DefaultColor, grid[1, 1].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 1].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 1].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 2].BackColor.Value);
			AssertColorEquals(BMConstants.Zone2DefaultColor, grid[1, 2].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 2].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 2].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 3].BackColor.Value);
			AssertColorEquals(BMConstants.Zone1DefaultColor, grid[1, 3].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 3].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 3].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 4].BackColor.Value);
			AssertColorEquals(BMConstants.Zone0DefaultColor, grid[1, 4].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 4].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 4].ForeColor.Value);
		}

		public void TestBackForeColors_Buffer_CustomColors()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser);
			pair.Item1.ForegroundColor = ZString.Empty;
			pair.Item1.BackgroundColor = ZString.Empty;
			BMBoardSectionTestHelper.SetBufferZoneColors(pair.Item1, "Black", "Green", "Purple", "Orange");

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertColorEquals(Color.Black, grid[0, 0].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 0].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 1].BackColor.Value);
			AssertColorEquals(Color.Orange, grid[1, 1].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 1].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 1].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 2].BackColor.Value);
			AssertColorEquals(Color.Purple, grid[1, 2].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 2].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 2].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 3].BackColor.Value);
			AssertColorEquals(Color.Green, grid[1, 3].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 3].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 3].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 4].BackColor.Value);
			AssertColorEquals(Color.Black, grid[1, 4].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 4].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 4].ForeColor.Value);
		}

		public void TestForeColors_ShouldCalculateBasedOnBackground()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser);
			pair.Item1.ForegroundColor = ZString.Empty;
			pair.Item1.BackgroundColor = ZString.Empty;
			BMBoardSectionTestHelper.SetBufferZoneColors(pair.Item1, "Black", "Green", "Light Gray", "White");

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertColorEquals(Color.Black, grid[0, 0].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 0].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 1].BackColor.Value);
			AssertColorEquals(Color.White, grid[1, 1].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 1].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 1].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 2].BackColor.Value);
			AssertColorEquals(Color.LightGray, grid[1, 2].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 2].ForeColor.Value);
			AssertColorEquals(Color.Black, grid[1, 2].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 3].BackColor.Value);
			AssertColorEquals(Color.Green, grid[1, 3].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 3].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 3].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 4].BackColor.Value);
			AssertColorEquals(Color.Black, grid[1, 4].BackColor.Value);
			AssertColorEquals(Color.Black, grid[0, 4].ForeColor.Value);
			AssertColorEquals(Color.White, grid[1, 4].ForeColor.Value);
		}

		public void TestForeColors_ShouldUseSectionOverride()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser);
			BMBoardSectionTestHelper.SetBufferZoneColors(pair.Item1, "Black", "Green", "Light Gray", "White");
			pair.Item1.ForegroundColor = "Pink";

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertColorEquals(Color.Pink, grid[0, 0].ForeColor.Value);
			AssertColorEquals(Color.Pink, grid[1, 0].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 1].BackColor.Value);
			AssertColorEquals(Color.White, grid[1, 1].BackColor.Value);
			AssertColorEquals(Color.Pink, grid[0, 1].ForeColor.Value);
			AssertColorEquals(Color.Pink, grid[1, 1].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 2].BackColor.Value);
			AssertColorEquals(Color.LightGray, grid[1, 2].BackColor.Value);
			AssertColorEquals(Color.Pink, grid[0, 2].ForeColor.Value);
			AssertColorEquals(Color.Pink, grid[1, 2].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 3].BackColor.Value);
			AssertColorEquals(Color.Green, grid[1, 3].BackColor.Value);
			AssertColorEquals(Color.Pink, grid[0, 3].ForeColor.Value);
			AssertColorEquals(Color.Pink, grid[1, 3].ForeColor.Value);

			AssertColorEquals(BMConstants.BackgroundDefaultColor, grid[0, 4].BackColor.Value);
			AssertColorEquals(Color.Black, grid[1, 4].BackColor.Value);
			AssertColorEquals(Color.Pink, grid[0, 4].ForeColor.Value);
			AssertColorEquals(Color.Pink, grid[1, 4].ForeColor.Value);
		}

		public void TestBackForeColors_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser);
			pair.Item1.BackgroundColor = Color.Pink.Name;
			pair.Item1.ForegroundColor = Color.Orange.Name;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);
			AssertColorEquals(Color.Pink, grid[0, 1].BackColor.Value);
			AssertColorEquals(Color.Orange, grid[0, 1].ForeColor.Value);

			AssertColorEquals(Color.Pink, grid[0, 2].BackColor.Value);
			AssertColorEquals(Color.Orange, grid[0, 2].ForeColor.Value);

			AssertColorEquals(Color.Pink, grid[0, 3].BackColor.Value);
			AssertColorEquals(Color.Orange, grid[0, 3].ForeColor.Value);

			AssertColorEquals(Color.Pink, grid[0, 4].BackColor.Value);
			AssertColorEquals(Color.Orange, grid[0, 4].ForeColor.Value);
		}

		public void TestBackForeColors_Bucket_CountDown()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 4, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var sectionConfiguration = pair.Item1.SectionConfiguration;
			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			sectionConfiguration.MaxOverdueSlots = 2;
			pair.Item1.BackgroundColor = Color.Black.Name;
			pair.Item1.ForegroundColor = Color.White.Name;
			sectionConfiguration.OverdueBackgroundColor = Color.Blue.Name;
			sectionConfiguration.OverdueForegroundColor = Color.Yellow.Name;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			for (var row = 0; row < 4; row++)
			{
				AssertCellColorsEqual(grid, row, 1, Color.Black, Color.White);
				AssertCellColorsEqual(grid, row, 2, Color.Black, Color.White);
				AssertCellColorsEqual(grid, row, 3, Color.Blue, Color.Yellow);
				AssertCellColorsEqual(grid, row, 4, Color.Blue, Color.Yellow);
			}
		}

		public void TestCCRChannelBackgroundColour()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");

			var buffer = section.Component;
			var preConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 64 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR1, resourceNonCCR1);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);

			var workflowPreConstraintFadeName = "WF-pre-fade";
			var workflowPreConstraintFade = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 10, sequence: 500, description: workflowPreConstraintFadeName + ": constraint");
			CreateTask(workflowPreConstraintFade, resourceNonCCR1.GS_Code, lowEstMinutes: 80, sequence: 400, description: workflowPreConstraintFadeName + ": pre");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			var ccrChannelColumn = 2;
			var ccrHeaderColumn = 1;
			var expectedZones = new int[] { 0, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 };
			for (var row = 1; row < expectedZones.Length + 1; row++)
			{
				var expectedZone = expectedZones[row - 1];
				var cellCCRHeaderZone = grid.Cells.SingleOrDefault(c => c.Column == ccrChannelColumn && c.Row == row).CCRHeaderZone;
				AssertEquals(string.Format("CCR-Channel row={0} must has ccr-zone={1}", row, expectedZone), expectedZone, cellCCRHeaderZone);
				AssertColorEquals("CCR-header-bar & CCR-channel must have same background colour", grid[row, ccrHeaderColumn].BackColor.Value, grid[row, ccrChannelColumn].BackColor.Value);
				row++;
			}

			for (var row = 1; row <= 13; row++)
			{
				var cellCCRHeaderZone = grid.Cells.SingleOrDefault(c => c.Column == 4 && c.Row == row).CCRHeaderZone;
				AssertEquals(string.Format("Non-CCR-Channel row={0} must have ccr-zone=null", row), null, cellCCRHeaderZone);
			}
		}

		static void AssertCellColorsEqual(ComponentGrid grid, int row, int col, Color background, Color foreground)
		{
			var cell = grid[row, col];
			var messageBase = "Expected {0} color of cell at row={1} col={2} with age index {3} to be {4} but was {5}";

			var message = string.Format(messageBase, "background", row, col, cell.TimeIndex, background, cell.BackColor.Value);
			AssertColorEquals(message, background, cell.BackColor.Value);

			message = string.Format(messageBase, "foreground", row, col, cell.TimeIndex, foreground, cell.ForeColor.Value);
			AssertColorEquals(message, foreground, cell.ForeColor.Value);
		}

		#endregion

		#region Channel Headers

		public void TestChannelHeaders_Resources()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var channelHeaders = grid.Cells.Where(c => c.ContentType == CellContentType.ChannelHeading).ToArray();
			AssertEquals(2, channelHeaders.Length);

			foreach (var header in channelHeaders)
			{
				AssertNotNull(header.Channel);
				AssertChannelIsType<GlbStaff>(Factory, header.Channel, "Should be a resource channel header");
			}
		}

		#endregion

		#region TimeInCell

		public void TestTimeInCell_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(39, cardCells.Length);

			foreach (var cell in cardCells)
			{
				AssertEquals(TimeSpan.FromMinutes(60), cell.TimeInCell);
			}
		}

		public void TestTimeInCell_Buffer()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser);
			pair.Item1.Component.FC_BufferTimespanInMinutes = 24 * 60;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(13, cardCells.Length);

			foreach (var cell in cardCells)
			{
				AssertEquals(TimeSpan.FromMinutes(120), cell.TimeInCell);
			}
		}

		#endregion

		#region Channels

		public void TestChannels_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(39, cardCells.Length);

			foreach (var cell in cardCells)
			{
				AssertNotNull(cell.Channel);
				AssertChannelIsType<GlbCapability>(Factory, cell.Channel, "Should be a capability channel header");
			}
		}

		public void TestChannels_Buffer()
		{
			var frodo = Factory.NewWithValidTestData<GlbStaff>();
			frodo.GS_FullName = "Frodo Baggins";
			var bilbo = Factory.NewWithValidTestData<GlbStaff>();
			bilbo.GS_FullName = "Bilbo Baggins";

			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, frodo, bilbo);
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(26, cardCells.Length);

			var frodoCells = cardCells.Take(13).ToList();
			frodoCells.ForEach(c => AssertEquals(frodo.PK, c.Channel.EntityPK));

			var bilboCells = cardCells.Skip(13).Take(13).ToList();
			bilboCells.ForEach(c => AssertEquals(bilbo.PK, c.Channel.EntityPK));
		}

		public void TestChannels_ReleaseScheduler()
		{
			var system = config.System;
			var buffer = config.Buffer;

			CreateSubBuffer(buffer);
			var group = config.ReleaseGroup;

			var section = CreateReleaseSchedulerBoardSection(buffer, group);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2, resource3, resource4);

			resource1.DesignateAsCCR(buffer);
			resource2.DesignateAsCCR(buffer);

			var releaseGroup = VisualBoardsTestHelper.GetReleaseGroup(Factory, system, group);

			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.IsReleaseScheduler = true;

			AssertNoErrors(section);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			AssertEquals("One column per channel, one for secondary channel headers", 4, grid.TotalColumns);
			AssertEquals("One row for channel headings, two for card cells (two secondary axis channels)", 3, grid.TotalRows);

			AssertEquals(CellContentType.Label, grid[0, 0].ContentType);
			AssertEquals(CellContentType.ChannelHeading, grid[0, 1].ContentType);
			AssertEquals(CellContentType.ChannelHeading, grid[0, 2].ContentType);
			AssertEquals(CellContentType.ChannelHeading, grid[0, 3].ContentType);

			AssertEquals(CellContentType.ChannelHeading, grid[1, 0].ContentType);
			AssertEquals(CellContentType.Cards, grid[1, 1].ContentType);
			AssertEquals(CellContentType.Cards, grid[1, 2].ContentType);
			AssertEquals(CellContentType.Cards, grid[1, 3].ContentType);

			AssertEquals(CellContentType.ChannelHeading, grid[2, 0].ContentType);
			AssertEquals(CellContentType.Cards, grid[2, 1].ContentType);
			AssertEquals(CellContentType.Cards, grid[2, 2].ContentType);
			AssertEquals(CellContentType.Cards, grid[2, 3].ContentType);
		}

		public void TestChannels_ReleaseScheduler_NoReleaseGroup()
		{
			var system = config.System;
			var buffer = config.Buffer;
			CreateSubBuffer(buffer);

			var section = CreateReleaseSchedulerBoardSection(buffer, null);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();

			AssertNoErrors(section);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			AssertEquals("One for non-constrained content, one for secondary channel headers", 2, grid.TotalColumns);
			AssertEquals("One row for primary channel heading, two for card cells (two secondary axis channels)", 3, grid.TotalRows);

			AssertEquals(CellContentType.Label, grid[0, 0].ContentType);
			AssertEquals(CellContentType.ChannelHeading, grid[0, 1].ContentType);

			AssertEquals(CellContentType.ChannelHeading, grid[1, 0].ContentType);
			AssertEquals(CellContentType.Cards, grid[1, 1].ContentType);

			AssertEquals(CellContentType.ChannelHeading, grid[2, 0].ContentType);
			AssertEquals(CellContentType.Cards, grid[2, 1].ContentType);
		}

		public void Test2DChannels_Bucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var section = pair.Item1;

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(2, BMBoardSectionTestHelper.GetSecondaryAxisChannelCount(section));
			section.SectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == resource1.PK).MSC_Sequence = 1;
			section.SectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == resource2.PK).MSC_Sequence = 2;

			AssertEquals(3, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);
			AssertNull("Should be no channel on the empty label", grid[0, 0].Channel);

			AssertEquals(resource1.GS_Code, grid[1, 1].SecondaryChannel.GetChannelName(DisplayNameType.ChannelCode));
			AssertEquals(resource1.GS_Code, grid[2, 1].SecondaryChannel.GetChannelName(DisplayNameType.ChannelCode));
			AssertEquals(resource1.GS_Code, grid[3, 1].SecondaryChannel.GetChannelName(DisplayNameType.ChannelCode));

			AssertPrimaryChannelIsType<GlbCapability>(Factory, grid[1, 1]);
			AssertPrimaryChannelIsType<GlbCapability>(Factory, grid[2, 1]);
			AssertPrimaryChannelIsType<GlbCapability>(Factory, grid[3, 1]);

			AssertEquals(resource2.GS_Code, grid[1, 2].SecondaryChannel.GetChannelName(DisplayNameType.ChannelCode));
			AssertEquals(resource2.GS_Code, grid[2, 2].SecondaryChannel.GetChannelName(DisplayNameType.ChannelCode));
			AssertEquals(resource2.GS_Code, grid[3, 2].SecondaryChannel.GetChannelName(DisplayNameType.ChannelCode));

			AssertPrimaryChannelIsType<GlbCapability>(Factory, grid[1, 2]);
			AssertPrimaryChannelIsType<GlbCapability>(Factory, grid[2, 2]);
			AssertPrimaryChannelIsType<GlbCapability>(Factory, grid[3, 2]);
		}

		static void AssertPrimaryChannelIsType<T>(BusinessObjectFactory factory, CellContent cell)
			where T : BusinessObject
		{
			var message = string.Format("Expected primary channel of cell at row={0}, col={1} to be of type {2}, but was: {3}", cell.Row, cell.Column, typeof(T).Name, cell.Channel == null ? "null" : cell.Channel.GetType().Name);
			AssertChannelIsType<T>(factory, cell.Channel, message);
		}

		static void AssertChannelIsType<T>(BusinessObjectFactory factory, IVisualBoardChannel channel, string message)
			where T : BusinessObject
		{
			if (!(typeof(T) == typeof(UnchanneledChannel)))
			{
				AssertNotNull(message, channel);
				Assert(message, BMSTestHelper.GetChannelEntity<T>(channel, factory) != null);
			}
			else
			{
				Assert(message, channel is UnchanneledChannel);
			}
		}

		#endregion

		#region IsLastCell/IsFirstCell

		public void TestIsFirstAndLastCell_Bucket_Age()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(39, cardCells.Length);

			var lastCellCells = cardCells.Where(c => c.IsLastCell).ToArray();
			AssertEquals("Should be one last-cell per channel", 3, lastCellCells.Length);
			Assert("LastCell cells should be age index 12", lastCellCells.All(c => c.TimeIndex == 12));

			var firstCellCells = cardCells.Where(c => c.IsFirstCell).ToArray();
			AssertEquals("Should be one first-cell per channel", 3, firstCellCells.Length);
			Assert("FirstCell cells should be age index 0", firstCellCells.All(c => c.TimeIndex == 0));
		}

		public void TestIsFirstAndLastCell_Buffer_Age()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(26, cardCells.Length);

			var lastCellCells = cardCells.Where(c => c.IsLastCell).ToArray();
			AssertEquals("Should be one last-cell per channel", 2, lastCellCells.Length);
			Assert("LastCell cells should be age index 12", lastCellCells.All(c => c.TimeIndex == 12));

			var firstCellCells = cardCells.Where(c => c.IsFirstCell).ToArray();
			AssertEquals("Should be one first-cell per channel", 2, firstCellCells.Length);
			Assert("FirstCell cells should be age index 0", firstCellCells.All(c => c.TimeIndex == 0));
		}

		public void TestIsFirstAndLastCell_Buffer_Age_2Subsections()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 2, 8, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled);
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(16, cardCells.Length);

			var lastCellCells = cardCells.Where(c => c.IsLastCell).ToArray();
			Assert("LastCell cells should be age index 15", lastCellCells.All(c => c.TimeIndex == 15));

			var firstCellCells = cardCells.Where(c => c.IsFirstCell).ToArray();
			Assert("FirstCell cells should be age index 0", firstCellCells.All(c => c.TimeIndex == 0));
		}

		public void TestIsFirstAndLastCell_Bucket_Due()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(39, cardCells.Length);

			var lastCellCells = cardCells.Where(c => c.IsLastCell).ToArray();
			AssertEquals("Should be one last-cell per channel", 3, lastCellCells.Length);
			Assert("LastCell cells should be age index -2", lastCellCells.All(c => c.TimeIndex == -2));

			var firstCellCells = cardCells.Where(c => c.IsFirstCell).ToArray();
			AssertEquals("Should be one first-cell per channel", 3, firstCellCells.Length);
			Assert("FirstCell cells should be age index 10", firstCellCells.All(c => c.TimeIndex == 10));

			//-1w
			pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Capability, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 1;
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			pair.Item1.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.AgreedDeliveryDate;

			grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(39, cardCells.Length);

			lastCellCells = cardCells.Where(c => c.IsLastCell).ToArray();
			AssertEquals("Should be one last-cell per channel", 3, lastCellCells.Length);
			Assert("LastCell cells should be age index -1", lastCellCells.All(c => c.TimeIndex == -1));

			firstCellCells = cardCells.Where(c => c.IsFirstCell).ToArray();
			AssertEquals("Should be one first-cell per channel", 3, firstCellCells.Length);
			Assert("FirstCell cells should be age index 11", firstCellCells.All(c => c.TimeIndex == 11));
		}

		public void TestIsFirstAndLastCell_Buffer_Due()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser, Factory.NewWithValidTestData<GlbStaff>());
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 2;
			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(pair.Item2, pair.Item1);

			var cardCells = grid.Cells.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			AssertEquals(26, cardCells.Length);

			var lastCellCells = cardCells.Where(c => c.IsLastCell).ToArray();
			AssertEquals("Should be one last-cell per channel", 2, lastCellCells.Length);
			Assert("LastCell cells should be age index -2", lastCellCells.All(c => c.TimeIndex == -2));

			var firstCellCells = cardCells.Where(c => c.IsFirstCell).ToArray();
			AssertEquals("Should be one first-cell per channel", 2, firstCellCells.Length);
			Assert("FirstCell cells should be age index 10", firstCellCells.All(c => c.TimeIndex == 10));
		}

		#endregion

		#region AllocateTasks

		[RequiresSTA]
		public void TestAllocateTasks_Bucket_NoChannels()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var workflow = CreateWorkflow(pair.Item1.Component);
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Description = "Mai Task";

			AssertTasksInCell(pair, new[] { task }, Tuple.Create(0, 0, new[] { task }));
		}

		public void TestUnbuffered_1CellsPerSubSection()
		{
			for (var subSectionsCount = 2; subSectionsCount < 10; subSectionsCount++)
			{
				var section = BMSTestHelper.CreateSectionAndViewModel(
					component: config.Bucket,
					subsections: subSectionsCount,
					cellsPerSubsection: 1
				).Item1;

				AssertEquals("GIVEN Unbuffered component i.e. bucket", false, section.Component.IsBuffer);
				AssertEquals($"GIVEN subsections>=2 i.e. {subSectionsCount}", subSectionsCount, section.SectionConfiguration.Subsections);
				AssertEquals("GIVEN cellsPerSubSection=1", 1, section.SectionConfiguration.CellsPerSubsection);

				var viewModel = BMSTestHelper.CreateViewModel(section);
				var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);

				CombineAssertions($"WHEN executing ComponentGridBuilderBase.Build, SHOULD has {subSectionsCount} cards with AgeHeading each", () =>
				{
					AssertEquals("AgeHeading", subSectionsCount, grid.Cells.Count(c => c.ContentType == CellContentType.AgeHeading));
					AssertEquals("Card", subSectionsCount, grid.Cells.Count(c => c.ContentType == CellContentType.Cards));
				});
			}
		}

		[RequiresSTA]
		public void TestAllocateTasks_Bucket_1DChannels()
		{
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Capability, capability1, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var section = pair.Item1;

			var workflow = CreateWorkflow(pair.Item1.Component);
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Description = "Mai Task";
			task.P9_EstDuration = new ZInt(10000).GetDateTimeFromMinutes();
			task.P9_G4_RequiredCapability = capability1.PK;

			section.SectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == capability1.PK).MSC_Sequence = 10;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertTasksInCell(section, viewModel, new[] { task }, Tuple.Create(1, 1, Array.Empty<ProcessTask>()), Tuple.Create(1, 2, new[] { task }));
		}

		[RequiresSTA]
		public void TestAllocateTasks_Bucket_2DChannels()
		{
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Capability, capability1, Factory.NewWithValidTestData<GlbCapability>(), Factory.NewWithValidTestData<GlbCapability>());
			var section = pair.Item1;
			var workflow = CreateWorkflow(pair.Item1.Component);
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Description = "Mai Task";
			task.P9_EstDuration = new ZInt(10000).GetDateTimeFromMinutes();
			task.P9_G4_RequiredCapability = capability1.PK;

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == resource1.PK).MSC_Sequence = 1;
			section.SectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == resource2.PK).MSC_Sequence = 2;

			section.SectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().First(c => c.MSC_ParentID == capability1.PK).MSC_Sequence = 10;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertTasksInCell(section, viewModel, new[] { task },
				Tuple.Create(1, 2, Array.Empty<ProcessTask>()),
				Tuple.Create(1, 3, new[] { task }),
				Tuple.Create(2, 2, Array.Empty<ProcessTask>()),
				Tuple.Create(2, 3, Array.Empty<ProcessTask>()));
		}

		[RequiresSTA]
		[TestDate(2013, 6, 17, 12, 0, 0)]
		public void TestAllocateTasks_DueTimeProgression_CurrentTime()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 3, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "w1", section.Component);
			var task = CreateTask(workflow, "", 22, description: "Mai Task");
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow;

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, timeProgressionMode: TimeProgressionModeList.Codes.Due, timePerCell: new ZInt(60 * 2).GetDateTimeFromMinutes(), timeField: TimeProgressionFieldList.Codes.AgreedDeliveryDate, maxOverdueSlots: 1);

			AssertTasksInCell(section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(1, 1, new[] { task }));
		}

		[RequiresSTA]
		[TestDate(2013, 6, 17, 12, 0, 0)]
		public void TestAllocateTasks_DueTimeProgression_PositiveTime()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 3, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "w1", pair.Item1.Component);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(3);
			var task = CreateTask(workflow, "", 22, description: "Mai Task");

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(pair.Item1, timeProgressionMode: TimeProgressionModeList.Codes.Due, timePerCell: new ZInt(60 * 2).GetDateTimeFromMinutes(), timeField: TimeProgressionFieldList.Codes.AgreedDeliveryDate, maxOverdueSlots: 1);

			AssertTasksInCell(pair, new[] { task }, Tuple.Create(0, 1, new[] { task }));
		}

		[RequiresSTA]
		[TestDate(2013, 6, 17, 12, 0, 0)]
		public void TestAllocateTasks_DueTimeProgression_PositiveTime_FarInFuture()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 3, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "w1", pair.Item1.Component);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(3000);
			var task = CreateTask(workflow, "", 22, description: "Mai Task");

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(pair.Item1, timeProgressionMode: TimeProgressionModeList.Codes.Due, timePerCell: new ZInt(60 * 2).GetDateTimeFromMinutes(), timeField: TimeProgressionFieldList.Codes.AgreedDeliveryDate, maxOverdueSlots: 1);

			AssertTasksInCell(pair, new[] { task }, Tuple.Create(0, 1, new[] { task }));
		}

		[RequiresSTA]
		[TestDate(2013, 6, 17, 12, 0, 0)]
		public void TestAllocateTasks_DueTimeProgression_NegativeTime()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 3, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "w1", pair.Item1.Component);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(-1);
			var task = CreateTask(workflow, "", 22, description: "Mai Task");

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(pair.Item1, timeProgressionMode: TimeProgressionModeList.Codes.Due, timePerCell: new ZInt(60 * 2).GetDateTimeFromMinutes(), timeField: TimeProgressionFieldList.Codes.AgreedDeliveryDate, maxOverdueSlots: 1);

			AssertTasksInCell(pair, new[] { task }, Tuple.Create(2, 1, new[] { task }));
		}

		[RequiresSTA]
		[TestDate(2013, 6, 17, 12, 0, 0)]
		public void TestAllocateTasks_DueTimeProgression_NegativeTime_ReallyOldWorkflow()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 3, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "w1", pair.Item1.Component);
			var task = CreateTask(workflow, "", 22, description: "Mai Task");

			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(-1000);

			pair.Item1.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			pair.Item1.SectionConfiguration.TimePerCell = new ZInt(60 * 2).GetDateTimeFromMinutes();
			pair.Item1.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.AgreedDeliveryDate;
			pair.Item1.SectionConfiguration.MaxOverdueSlots = 1;

			AssertTasksInCell(pair, new[] { task }, Tuple.Create(2, 1, new[] { task }));
		}

		[RequiresSTA]
		[TestDate(2013, 5, 24, 14, 0, 0)]
		public void TestAllocateTasks_ResourceChannel()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader, "W1", section.Component);
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-59);
			var workflow2 = CreateWorkflow(jobHeader, "W2", section.Component);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-119);

			var channel = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = CreateTask(workflow1, channel.GS_Code, 30, description: "t1");
			var task2 = CreateTask(workflow2, channel.GS_Code, 30, description: "t2");

			var boardChannel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			boardChannel.MSC_ParentID = channel.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertTasksInCell(section, viewModel, new[] { task1, task2 }, Tuple.Create(1, 1, new[] { task1 }), Tuple.Create(2, 1, new[] { task2 }));
		}

		[RequiresSTA]
		[TestDate(2013, 5, 24, 14, 0, 0)]
		public void TestAllocateTasks_UnChanneled()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ShowUnchanneled = true;
			section.SectionConfiguration.PrimaryAxisChannels[0].MSC_ChannelType = ChannelTypeList.Codes.Resource;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader, "W1", section.Component);
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-59);
			var workflow2 = CreateWorkflow(jobHeader, "W2", section.Component);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-119);

			var task1 = CreateTask(workflow1, "", 30, description: "t1");
			var task2 = CreateTask(workflow2, "", 30, description: "t2");

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertTasksInCell(section, viewModel, new[] { task1, task2 }, Tuple.Create(1, 1, new[] { task1 }), Tuple.Create(2, 1, new[] { task2 }));
		}

		[RequiresSTA]
		[TestDate(2013, 5, 24, 14, 0, 0)]
		public void TestAllocateTasks_OldWorkflows()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "w1", section.Component);
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-59);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "w2", section.Component);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-1000);

			var channel = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = CreateTask(workflow1, channel.GS_Code, 30, description: "t1");
			var task2 = CreateTask(workflow2, channel.GS_Code, 30, description: "t2");

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertTasksInCell(section, viewModel, new[] { task1, task2 }, Tuple.Create(0, 1, new[] { task1 }), Tuple.Create(1, 1, new[] { task2 }));
		}

		[RequiresSTA]
		[TestDate(2013, 5, 24, 14, 0, 0)]
		public void TestAllocateTasks_WorkflowMoved()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "w1", section.Component);
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-59);

			var channel = Factory.NewWithValidTestData<GlbStaff>();
			var task1 = CreateTask(workflow1, channel.GS_Code, 30, description: "t1");

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertTasksInCell(section, viewModel, new[] { task1 }, Tuple.Create(1, 1, Array.Empty<ProcessTask>()));
		}

		[TestDate(2013, 10, 3)]
		public void TestSubComponentPK()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer);
			subBuffer.FC_BufferTimespanInMinutes = 48 * 60;
			subBuffer.FC_OffsetInMinutes = 48 * 60;
			var constraint = BMSTestHelper.CreateConstraint(buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			Factory.Save();

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "w1", buffer);
			var task = CreateTask(workflow, "", 22, description: "Mai Task");

			var board = system.Boards.AddNew();

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 7, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser).Item1;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.FSG_GG_Group;
			section.MS_FC_Component = buffer.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);
			var subComponentZoneHeaders = grid.Cells.Where(c => c.ContentType == CellContentType.SubComponentZoneHeading).Where(c => c.SubComponentZones.Any()).ToArray();

			AssertEquals(7, subComponentZoneHeaders.Length);
			foreach (var cell in subComponentZoneHeaders)
			{
				AssertCollectionContains(subBuffer.PK, cell.SubComponentZones.Keys.ToArray());
			}
		}

		[TestDate(2014, 12, 2)]
		public void TestNullAllocatedTaskList()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ShowUnchanneled = true;
			section.SectionConfiguration.PrimaryAxisChannels[0].MSC_ChannelType = ChannelTypeList.Codes.Resource;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);

			var map = CardAllocationMap.NewAllocationMap(section, viewModel, TaskChannelMap.ForTest(section, viewModel));
			AssertNoExceptionThrown("No exception should be thrown heres", () => grid.ShowAllocatedTasks(Factory, viewModel, map));
		}

		[RequiresSTA]
		[ExpectNoExceptions]
		public void TestAllocateTasks_WhenStaffCodeIncludesSingleQuote()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "'AA", "Ruining Everything");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);

			config.BufferSection.SectionConfiguration.OverrideChannels = true;
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection).MSC_ParentID = resource.PK;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			viewModel.ComponentGrid.AllocateTasks_ForTest(config.BufferSection, viewModel, new[] { task });
		}

		#endregion

		#region Task Estimates

		[RequiresSTA]
		[TestDate(2013, 7, 11)]
		public void TestGetTaskEstimates()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var section = pair.Item1;
			var viewModel = pair.Item2;
			section.SectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-1);
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_ReleaseDateTime = ZDateTime.Now;

			var task1_1 = job.WorkflowItems.AddNew();
			task1_1.P9_FH_ProcessHeader = workflow1.PK;
			task1_1.P9_EstDuration = new ZInt(20).GetDateTimeFromMinutes(); // 30 mins STD est
			var task1_2 = job.WorkflowItems.AddNew();
			task1_2.P9_FH_ProcessHeader = workflow1.PK;
			task1_2.P9_EstDuration = new ZInt(20).GetDateTimeFromMinutes(); // 30 mins STD est
			var task2_1 = job.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = workflow2.PK;
			task2_1.P9_EstDuration = new ZInt(20).GetDateTimeFromMinutes(); // 30 mins STD est

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1_1, task1_2, task2_1 });

			AssertEquals(0.5m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, secondaryAxes: new[] { 0 }));
			AssertEquals(0.5m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, new[] { 1 }, new[] { 0 }));
			AssertEquals(0.0m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, new[] { 0 }, new[] { 0 }));

			AssertEquals(1.0m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, secondaryAxes: new[] { 1 }));
			AssertEquals(1.0m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, new[] { 1 }, new[] { 1 }));
			AssertEquals(0.0m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, new[] { 0 }, new[] { 1 }));

			AssertEquals(0.0m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, primaryAxes: new[] { 0 }));
			AssertEquals(1.5m, BMSTestHelper.GetTaskEstimatesInCells(viewModel.ComponentGrid, primaryAxes: new[] { 1 }));
		}

		#endregion

		#region Sub-buffer components

		public void TestSubComponents_SingleSubComponent()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Name = "sub-buffer";
			subBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer.FC_BufferTimespanInMinutes = 72 * 60;
			subBuffer.FC_OffsetInMinutes = 0;
			BMSTestHelper.CreateConstraint(buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 4, "two columns for main component headers, one for un-channeled cards, and one for post constraint", 13, "13 cells per subsection (no channel header)");

			var expected = GetExpectedContentTypes(13, new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards });
			AssertCellContents(section, viewModel, expected);

			var zoneHeadingPosition = BMConstants.ZoneHeadingPosition;

			AssertZoneNumbers(section, viewModel, // Standard zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition, 12),
					new Point(zoneHeadingPosition, 11),
					new Point(zoneHeadingPosition, 10),
					new Point(zoneHeadingPosition, 9),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition, 8),
					new Point(zoneHeadingPosition, 7),
					new Point(zoneHeadingPosition, 6),
					new Point(zoneHeadingPosition, 5),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition, 4),
					new Point(zoneHeadingPosition, 3),
					new Point(zoneHeadingPosition, 2),
					new Point(zoneHeadingPosition, 1),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition, 0),
				});

			AssertZoneNumbers(section, viewModel, // Post-constraint zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 12),
					new Point(zoneHeadingPosition + 1, 11),
					new Point(zoneHeadingPosition + 1, 10),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 9),
					new Point(zoneHeadingPosition + 1, 8),
					new Point(zoneHeadingPosition + 1, 7),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 6),
					new Point(zoneHeadingPosition + 1, 5),
					new Point(zoneHeadingPosition + 1, 4),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 3),
					new Point(zoneHeadingPosition + 1, 2),
					new Point(zoneHeadingPosition + 1, 1),
					new Point(zoneHeadingPosition + 1, 0),
				},
				arePreConstraintZones: false,
				arePostConstraintZones: true);
		}

		public void TestSubComponents_WhenNoReleaseGroupsDefined()
		{
			var system = config.System;
			var buffer = config.Buffer;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(buffer);

			config.RemoveReleaseGroup();

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 4, "two columns for main component headers, one for un-channeled cards, and one for post constraint", 13, "13 cells per subsection (no channel header)");
			AssertCellContents(section, viewModel, GetExpectedContentTypes(13, new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards }));
		}

		public void TestSubComponents_1Subsection_4CellsPerSubsection()
		{
			var system = config.System;
			var buffer = config.Buffer;

			var subBuffer1 = BMSTestHelper.CreateSubBuffer(buffer, "subBUF", 56 * 60, 8 * 60, 1);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(buffer, 1, 4, FlowDirectionList.Codes.Up, LastCellList.Codes.Top, ChannelTypeList.Codes.NotChanneled);
			var section = sectionAndViewModel.Item1;
			section.MS_GG_ReleaseGroup = releaseGroup.FSG_GG_Group;
			section.SectionConfiguration.ShowZones = false;
			var viewModel = sectionAndViewModel.Item2;

			AssertColumnsAndRows(section, viewModel, 2, "One column for age header, one for cards", 4, "4 cells per subsection (no channel header)");
			AssertCellContents(section, viewModel, GetExpectedContentTypes(4, new[] { CellContentType.AgeHeading, CellContentType.Cards }));

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 3, flowDirection: FlowDirectionList.Codes.Up);
			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 2, "One column for age header, one for cards", 3, "3 cells per subsection (no channel header)");
			AssertCellContents(section, viewModel, GetExpectedContentTypes(3, new[] { CellContentType.AgeHeading, CellContentType.Cards }));

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 5, flowDirection: FlowDirectionList.Codes.Up);
			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 2, "One column for age header, one for cards", 5, "5 cells per subsection (no channel header)");
			AssertCellContents(section, viewModel, GetExpectedContentTypes(5, new[] { CellContentType.AgeHeading, CellContentType.Cards }));
		}

		public void TestSubComponentHeaders_RepeatingHeaders()
		{
			var system = config.System;
			var buffer = config.Buffer;

			var subBuffer1 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer1", 56 * 60);
			subBuffer1.FC_OffsetInMinutes = 0;
			subBuffer1.FC_DisplaySequence = 1;

			BMSTestHelper.CreateConstraint(buffer);

			var subBuffer2 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer2", 32 * 60, 64 * 60, 2);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var nonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var nonCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			ccr.DesignateAsCCR(buffer);

			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(buffer, 1, 4, FlowDirectionList.Codes.Up, LastCellList.Codes.Top, ChannelTypeList.Codes.Resource, showZones: null, releaseGroup.FSG_GG_Group, nonCCR1, ccr, nonCCR2);
			var section = sectionAndViewModel.Item1;
			section.SectionConfiguration.ShowZones = true;
			var viewModel = sectionAndViewModel.Item2;

			AssertColumnsAndRows(section, viewModel, 11, "One for age headers, two zone headings, three cards, two sub component zone headings, one CCR target", 5, "4 cells per subsection plus channel header");
			AssertCellContents(section, viewModel, new CellContentType[][]
			{
				new[] { CellContentType.Label, CellContentType.Label, CellContentType.Label, CellContentType.Label, CellContentType.ChannelHeading, CellContentType.Label, CellContentType.ChannelHeading, CellContentType.Label, CellContentType.Label, CellContentType.Label, CellContentType.ChannelHeading },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards, CellContentType.CCRHeading, CellContentType.Cards, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards, CellContentType.CCRHeading, CellContentType.Cards, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards, CellContentType.CCRHeading, CellContentType.Cards, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards, CellContentType.CCRHeading, CellContentType.Cards, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards },
			});

			var grid = BMBoardSectionTestHelper.CreateComponentGrid(viewModel, section);

			AssertEquals("Zone 0", grid[1, 2].Label);
			AssertEquals("Zone 0", grid[2, 2].Label);
			AssertEquals("Zone 0", grid[3, 2].Label);
			AssertEquals("Zone 2", grid[4, 2].Label);

			AssertEquals("Zone 0", grid[1, 3].Label);
			AssertEquals("Zone 2", grid[2, 3].Label);
			AssertEquals("Zone 3", grid[3, 3].Label);
			AssertEquals("Zone 3", grid[4, 3].Label);

			AssertEquals("Zone 0", grid[1, 8].Label);
			AssertEquals("Zone 0", grid[2, 8].Label);
			AssertEquals("Zone 0", grid[3, 8].Label);
			AssertEquals("Zone 2", grid[4, 8].Label);

			AssertEquals("Zone 0", grid[1, 9].Label);
			AssertEquals("Zone 2", grid[2, 9].Label);
			AssertEquals("Zone 3", grid[3, 9].Label);
			AssertEquals("Zone 3", grid[4, 9].Label);
		}

		public void TestSubComponents_ConstrainedMode_ShouldUseSubComponentsForZoneColors()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer1 = buffer.ChildComponents.AddNew();
			subBuffer1.FC_Name = "sub-buffer1";
			subBuffer1.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer1.FC_BufferTimespanInMinutes = 56 * 60;
			subBuffer1.FC_OffsetInMinutes = 8 * 60;
			subBuffer1.FC_DisplaySequence = 1;

			BMSTestHelper.CreateConstraint(buffer);

			var subBuffer2 = buffer.ChildComponents.AddNew();
			subBuffer2.FC_Name = "sub-buffer2";
			subBuffer2.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer2.FC_BufferTimespanInMinutes = 32 * 60;
			subBuffer2.FC_OffsetInMinutes = 64 * 60;
			subBuffer2.FC_DisplaySequence = 2;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.BackgroundColor = "Aqua";

			BMBoardSectionTestHelper.SetBufferZoneColors(section, "HotPink", "Pink", "Beige", "Azure");

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			CombineAssertions("Primary zone heading cell background colors", () =>
			{
				AssertColorEquals("Should primary component zone 3 for zone heading background", Color.Azure, grid[12, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 3 for zone heading background", Color.Azure, grid[11, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 3 for zone heading background", Color.Azure, grid[10, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 2 for zone heading background", Color.Azure, grid[9, zoneHeadingPosition].BackColor.Value);

				AssertColorEquals("Should primary component zone 2 for zone heading background", Color.Beige, grid[8, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 2 for zone heading background", Color.Beige, grid[7, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 1 for zone heading background", Color.Beige, grid[6, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 1 for zone heading background", Color.Beige, grid[5, zoneHeadingPosition].BackColor.Value);

				AssertColorEquals("Should primary component zone 3 for zone heading background", Color.Pink, grid[4, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 2 for zone heading background", Color.Pink, grid[3, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 2 for zone heading background", Color.Pink, grid[2, zoneHeadingPosition].BackColor.Value);
				AssertColorEquals("Should primary component zone 1 for zone heading background", Color.Pink, grid[1, zoneHeadingPosition].BackColor.Value);

				AssertColorEquals("Should primary component zone 0 for zone heading background", Color.HotPink, grid[0, zoneHeadingPosition].BackColor.Value);
			});

			CombineAssertions("Card cell background colors", () =>
			{
				AssertColorEquals("Should use section background color when no sub-component zone is present", Color.Pink, grid[12, 4].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 3 for card background", Color.Pink, grid[11, 4].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 3 for card background", Color.Pink, grid[10, 4].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 2 for card background", Color.Pink, grid[9, 4].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 2 for card background", Color.Beige, grid[8, 4].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 2 for card background", Color.Beige, grid[7, 4].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 1 for card background", Color.Beige, grid[6, 4].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 1 for card background", Color.Beige, grid[5, 4].BackColor.Value);

				AssertColorEquals("Should sub-component2 zone 3 for card background", Color.Azure, grid[4, 4].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 2 for card background", Color.Azure, grid[3, 4].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 2 for card background", Color.Azure, grid[2, 4].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 1 for card background", Color.Azure, grid[1, 4].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 0 for card background", Color.Azure, grid[0, 4].BackColor.Value);
			});

			CombineAssertions("Post constraint heading cell background colors 1", () =>
			{
				AssertColorEquals("Should use section background color when no sub-component zone is present", Color.Azure, grid[12, 3].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[11, 3].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[10, 3].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[9, 3].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[8, 3].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[7, 3].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[6, 3].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[5, 3].BackColor.Value);

				AssertColorEquals("Should sub-component2 zone 3 for sub-component heading background", Color.Azure, grid[4, 3].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 2 for sub-component heading background", Color.Beige, grid[3, 3].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 2 for sub-component heading background", Color.Beige, grid[2, 3].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 1 for sub-component heading background", Color.Pink, grid[1, 3].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 0 for sub-component heading background", Color.HotPink, grid[0, 3].BackColor.Value);
			});

			CombineAssertions("Post constraint heading cell background colors 2", () =>
			{
				AssertColorEquals("Should use section background color when no sub-component zone is present", Color.Azure, grid[12, 2].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[11, 2].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Azure, grid[10, 2].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 3 for sub-component heading background", Color.Beige, grid[9, 2].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 2 for sub-component heading background", Color.Beige, grid[8, 2].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 2 for sub-component heading background", Color.Beige, grid[7, 2].BackColor.Value);

				AssertColorEquals("Should sub-component1 zone 2 for sub-component heading background", Color.Pink, grid[6, 2].BackColor.Value);
				AssertColorEquals("Should sub-component1 zone 2 for sub-component heading background", Color.Pink, grid[5, 2].BackColor.Value);

				AssertColorEquals("Should sub-component2 zone 1 for sub-component heading background", Color.HotPink, grid[4, 2].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 1 for sub-component heading background", Color.HotPink, grid[3, 2].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 1 for sub-component heading background", Color.HotPink, grid[2, 2].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 1 for sub-component heading background", Color.HotPink, grid[1, 2].BackColor.Value);
				AssertColorEquals("Should sub-component2 zone 0 for sub-component heading background", Color.HotPink, grid[0, 2].BackColor.Value);
			});
		}

		public void TestSubComponents_NonConstrainedMode_ShouldUsePrimaryComponentForZoneColors()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer1 = buffer.ChildComponents.AddNew();
			subBuffer1.FC_Name = "sub-buffer1";
			subBuffer1.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer1.FC_BufferTimespanInMinutes = 56 * 60;
			subBuffer1.FC_OffsetInMinutes = 8 * 60;
			subBuffer1.FC_DisplaySequence = 1;

			var subBuffer2 = buffer.ChildComponents.AddNew();
			subBuffer2.FC_Name = "sub-buffer2";
			subBuffer2.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer2.FC_BufferTimespanInMinutes = 32 * 60;
			subBuffer2.FC_OffsetInMinutes = 64 * 60;
			subBuffer2.FC_DisplaySequence = 2;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.BackgroundColor = "Aqua";

			BMBoardSectionTestHelper.SetBufferZoneColors(section, "HotPink", "Pink", "Beige", "Azure");

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			CombineAssertions(() =>
			{
				AssertColorEquals(Color.Azure, grid[12, 2].BackColor.Value);
				AssertColorEquals(Color.Azure, grid[11, 2].BackColor.Value);
				AssertColorEquals(Color.Azure, grid[10, 2].BackColor.Value);
				AssertColorEquals(Color.Azure, grid[9, 2].BackColor.Value);

				AssertColorEquals(Color.Beige, grid[8, 2].BackColor.Value);
				AssertColorEquals(Color.Beige, grid[7, 2].BackColor.Value);
				AssertColorEquals(Color.Beige, grid[6, 2].BackColor.Value);
				AssertColorEquals(Color.Beige, grid[5, 2].BackColor.Value);

				AssertColorEquals(Color.Pink, grid[4, 2].BackColor.Value);
				AssertColorEquals(Color.Pink, grid[3, 2].BackColor.Value);
				AssertColorEquals(Color.Pink, grid[2, 2].BackColor.Value);
				AssertColorEquals(Color.Pink, grid[1, 2].BackColor.Value);

				AssertColorEquals(Color.HotPink, grid[0, 2].BackColor.Value);
			});
		}

		public void TestSubComponents_SingleSubComponent_OddNumberOfZoneDays()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Name = "sub-buffer";
			subBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer.FC_BufferTimespanInMinutes = 40 * 60;
			subBuffer.FC_OffsetInMinutes = 8 * 60;

			BMSTestHelper.CreateConstraint(buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 4, "two columns for main component headers, one for un-channeled cards, one for post-constraint", 13, "13 cells per subsection (no channel header)");
			AssertCellContents(section, viewModel, GetExpectedContentTypes(13, new CellContentType[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards }));

			AssertZoneNumbers(section, viewModel, // Standard zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition, 12),
					new Point(zoneHeadingPosition, 11),
					new Point(zoneHeadingPosition, 10),
					new Point(zoneHeadingPosition, 9),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition, 8),
					new Point(zoneHeadingPosition, 7),
					new Point(zoneHeadingPosition, 6),
					new Point(zoneHeadingPosition, 5),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition, 4),
					new Point(zoneHeadingPosition, 3),
					new Point(zoneHeadingPosition, 2),
					new Point(zoneHeadingPosition, 1),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition, 0),
				});

			AssertZoneNumbers(section, viewModel, // Post-constraint zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 12),
					new Point(zoneHeadingPosition + 1, 11),
					new Point(zoneHeadingPosition + 1, 10),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 9),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 8),
					new Point(zoneHeadingPosition + 1, 7),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 6),
					new Point(zoneHeadingPosition + 1, 5),
					new Point(zoneHeadingPosition + 1, 4),
					new Point(zoneHeadingPosition + 1, 3),
					new Point(zoneHeadingPosition + 1, 2),
					new Point(zoneHeadingPosition + 1, 1),
					new Point(zoneHeadingPosition + 1, 0),
				},
				arePreConstraintZones: false,
				arePostConstraintZones: true);
		}

		public void TestSubComponents_SingleSubComponent_WithChannel()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Name = "sub-buffer";
			subBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer.FC_BufferTimespanInMinutes = 72 * 60;
			subBuffer.FC_OffsetInMinutes = 0;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.OverrideChannels = true;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel.MSC_ParentID = Factory.NewWithValidTestData<GlbStaff>().PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 3, "two columns for main component headers and one for un-channeled card", 14, "13 cells per subsection and one channel header");
			AssertCellContents(section, viewModel, new CellContentType[][]
			{
				new[] { CellContentType.Label, CellContentType.Label, CellContentType.ChannelHeading },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
				new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards },
			});

			AssertZoneNumbers(section, viewModel, // Standard zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition, 13),
					new Point(zoneHeadingPosition, 12),
					new Point(zoneHeadingPosition, 11),
					new Point(zoneHeadingPosition, 10),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition, 9),
					new Point(zoneHeadingPosition, 8),
					new Point(zoneHeadingPosition, 7),
					new Point(zoneHeadingPosition, 6),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition, 5),
					new Point(zoneHeadingPosition, 4),
					new Point(zoneHeadingPosition, 3),
					new Point(zoneHeadingPosition, 2),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition, 1),
				});
		}

		public void TestSubComponents_SingleSubComponent_Wrapped()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Name = "sub-buffer";
			subBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer.FC_BufferTimespanInMinutes = 64 * 60;
			subBuffer.FC_OffsetInMinutes = 0;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;

			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, subSections: 2, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 6, "2x two columns for component headers, two for un-channeled cards, and none for single sub-components - wrapped sections can't show sub-components because it's just too confusing", 13, "13 cells per subsection (no channel header)");
			AssertCellContents(section, viewModel, GetExpectedContentTypes(13, new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards, CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.Cards }));
		}

		public void TestSubComponents_NeatlyFitting()
		{
			var system = config.System;
			var buffer = config.Buffer;

			var subBuffer1 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer1", 64 * 60, 0, 1);
			subBuffer1.FC_OffsetInMinutes = 0;
			subBuffer1.FC_DisplaySequence = 1;

			BMSTestHelper.CreateConstraint(buffer, "constraint", 64 * 60);

			var subBuffer2 = BMSTestHelper.CreateSubBuffer(buffer, "sub-buffer2", 32 * 60, 64 * 60, 2);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled, showZones: null, releaseGroup.FSG_GG_Group);
			var section = sectionAndViewModel.Item1;
			//section.SectionConfiguration.ReleaseGroupPK = releaseGroup.FSG_GG_Group;
			var viewModel = sectionAndViewModel.Item2;

			AssertColumnsAndRows(section, viewModel, 5, "two columns for main component headers, one for un-channeled cards, and two for two constraints", 13, "13 cells per subsection (no channel header)");

			var expected = GetExpectedContentTypes(13, new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards });
			AssertCellContents(section, viewModel, expected);

			AssertZoneNumbers(section, viewModel, // Pre constraint zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 12),
					new Point(zoneHeadingPosition + 1, 11),
					new Point(zoneHeadingPosition + 1, 10),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 9),
					new Point(zoneHeadingPosition + 1, 8),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 7),
					new Point(zoneHeadingPosition + 1, 6),
					new Point(zoneHeadingPosition + 1, 5),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition + 1, 4),
					new Point(zoneHeadingPosition + 1, 3),
					new Point(zoneHeadingPosition + 1, 2),
					new Point(zoneHeadingPosition + 1, 1),
					new Point(zoneHeadingPosition + 1, 0),
				},
				arePreConstraintZones: true,
				arePostConstraintZones: false);

			AssertZoneNumbers(section, viewModel, // Post-constraint zone headers
				zone3Coords: new[]
				{
					new Point(zoneHeadingPosition + 2, 12),
					new Point(zoneHeadingPosition + 2, 11),
					new Point(zoneHeadingPosition + 2, 10),
					new Point(zoneHeadingPosition + 2, 9),
					new Point(zoneHeadingPosition + 2, 8),
					new Point(zoneHeadingPosition + 2, 7),
					new Point(zoneHeadingPosition + 2, 6),
					new Point(zoneHeadingPosition + 2, 5),
					new Point(zoneHeadingPosition + 2, 4),
				},
				zone2Coords: new[]
				{
					new Point(zoneHeadingPosition + 2, 3),
					new Point(zoneHeadingPosition + 2, 2),
				},
				zone1Coords: new[]
				{
					new Point(zoneHeadingPosition + 2, 1),
				},
				zone0Coords: new[]
				{
					new Point(zoneHeadingPosition + 2, 0),
				},
				arePreConstraintZones: false,
				arePostConstraintZones: true);
		}

		public void TestSubComponents_OverlappingSubComponents()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;

			var subBuffer1 = buffer.ChildComponents.AddNew();
			subBuffer1.FC_Name = "sub-buffer1";
			subBuffer1.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer1.FC_BufferTimespanInMinutes = 64 * 60;
			subBuffer1.FC_OffsetInMinutes = 0;
			subBuffer1.FC_DisplaySequence = 1;

			BMSTestHelper.CreateConstraint(buffer);

			var subBuffer2 = buffer.ChildComponents.AddNew();
			subBuffer2.FC_Name = "sub-buffer2";
			subBuffer2.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer2.FC_BufferTimespanInMinutes = 32 * 60;
			subBuffer2.FC_OffsetInMinutes = 32 * 60;
			subBuffer2.FC_DisplaySequence = 2;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 5, "two columns for main component headers, one for un-channeled cards, and two for each sub-component - they overlap so can't share a column", 13, "13 cells per subsection (no channel header)");

			var expected = GetExpectedContentTypes(11, new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Cards });
			AssertCellContents(section, viewModel, expected);
		}

		#region TestConstraintSubComponent

		public void TestConstraintSubComponent_FlowDirectionUp()
		{
			AssertConstraintSubComponentPositionAndPanelBorder(FlowDirectionList.Codes.Up, "0 5 0 0", "top");
		}

		public void TestConstraintSubComponent_FlowDirectionDown()
		{
			AssertConstraintSubComponentPositionAndPanelBorder(FlowDirectionList.Codes.Down, "0 0 0 5", "bottom");
		}

		public void TestConstraintSubComponent_FlowDirectionLeft()
		{
			AssertConstraintSubComponentPositionAndPanelBorder(FlowDirectionList.Codes.Left, "5 0 0 0", "left");
		}

		public void TestConstraintSubComponent_FlowDirectionRight()
		{
			AssertConstraintSubComponentPositionAndPanelBorder(FlowDirectionList.Codes.Right, "0 0 5 0", "right");
		}

		public void TestConstraintSubComponent_FlowDirectionUp_NoReleaseGroups()
		{
			AssertConstraintSubComponentPositionAndPanelBorder(FlowDirectionList.Codes.Up, "0 5 0 0", "top", createReleaseGroup: false);
		}

		public void TestConstraintSubComponent_FlowDirectionUp_NonConstrainedModeReleaseGroup()
		{
			AssertConstraintSubComponentPositionAndPanelBorder(FlowDirectionList.Codes.Up, "0 0 0 0", "top", createReleaseGroup: true, makeReleaseGroupInConstrainedMode: false);
		}

		void AssertConstraintSubComponentPositionAndPanelBorder(string flowDirection, string expectedPanelBorderThickness, string sideName, bool createReleaseGroup = true, bool makeReleaseGroupInConstrainedMode = true)
		{
			var system = config.System;
			var buffer = config.Buffer;
			var constraint = CreateConstraint(buffer, offsetMinutes: 64 * 60);

			ZGuid releaseGroupPK;

			if (createReleaseGroup)
			{
				releaseGroupPK = config.ReleaseGroup.PK;

				if (makeReleaseGroupInConstrainedMode)
				{
					ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);
				}
			}
			else
			{
				releaseGroupPK = ZGuid.Empty;

				config.RemoveReleaseGroup();
			}

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.ChannelBy = string.Empty;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: releaseGroupPK, cellsPerSubsection: 13, flowDirection: flowDirection);
			section.SectionConfiguration.OverrideChannels = true;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			var cellsWithBorderValue = grid.Cells.Where(c => !string.IsNullOrEmpty(c.BorderThickness)).ToArray();
			var cellsWithoutBorderValue = grid.Cells.Except(cellsWithBorderValue);

			if (expectedPanelBorderThickness == "0 0 0 0")
			{
				AssertEquals(0, cellsWithBorderValue.Length);
			}
			else
			{
				AssertEquals(3, cellsWithBorderValue.Length);
			}

			foreach (var cell in cellsWithBorderValue)
			{
				var message = string.Format("Constraint cell should have a 5px border on the {0} of the panel", sideName);
				AssertEquals(message, expectedPanelBorderThickness, cell.BorderThickness);
				AssertEquals("Constraint should be at time index 7", 7, cell.TimeIndex);
			}

			foreach (var cell in cellsWithoutBorderValue)
			{
				AssertNull(cell.BorderThickness);
			}
		}

		public void TestConstraintSubComponent_VerticalOrientation_NonCapacityConstrainedReleaseGroup()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_Name = "buffer";
			buffer.FC_BufferTimespanInMinutes = 96 * 60;
			var constraint = buffer.ChildComponents.AddNew();
			constraint.FC_Type = BMComponentTypeList.Codes.Constraint;
			constraint.FC_Name = "constraint";
			constraint.FC_OffsetInMinutes = 64 * 60;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, null);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = string.Empty;
			section.SectionConfiguration.OverrideChannels = true;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			foreach (var cell in grid.Cells)
			{
				AssertNull(cell.BorderThickness);
			}
		}

		[RequiresSTA]
		public void TestConstraintSubComponent_BufferWithNoZone_ShouldNotShowConstraint()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			config.Section.SectionConfiguration.ShowZones = false;
			Factory.Save();
			var viewModel = config.SectionViewModel;

			foreach (var cell in viewModel.ComponentGrid.Cells)
			{
				AssertNull("When Show-zones is false, constraint line should not show", cell.BorderThickness);
			}
		}

		#endregion

		public void TestSubComponents_SingleSubComponent_NonCapacityConstrainedReleaseGroup()
		{
			var system = Factory.New<BMSystem>();
			var buffer = CreateBuffer(system);
			var subBuffer = CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertColumnsAndRows(section, viewModel, 3, "two columns for main component headers, one for un-channeled cards, and none for sub-component since the release group is NOT in constrained mode", 13, "13 cells per subsection (no channel header)");
		}

		#endregion

		#region CardCellLabels

		public void TestCardCellLabels_UnchanneledBucket()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var viewModel = pair.Item2;
			var grid = viewModel.ComponentGrid;
			var cardCell = grid[0, 0];

			AssertEquals(CellContentType.Cards, cardCell.ContentType);
			AssertEquals(string.Empty, cardCell.Label);
		}

		public void TestCardCellLabels_ChanneledBucket()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "The Doctor";
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var cardCell = grid[1, 0];

			AssertEquals(CellContentType.Cards, cardCell.ContentType);
			AssertEquals("The Doctor", cardCell.Label);
		}

		public void TestCardCellLabels_DoubleChanneledBucket()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.ChannelSecondaryBy = string.Empty;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.OverrideSecondaryChannels = true;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "The Doctor";
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";
			capability.G4_Description = "Abc";
			BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability.PK);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var cardCell = grid[1, 1];

			AssertEquals(CellContentType.Cards, cardCell.ContentType);
			AssertEquals("The Doctor, Abc", cardCell.Label);
		}

		public void TestCardCellLabels_TimeChanneledBucket_SingleCell()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "The Doctor";
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var cardCell = grid[1, 0];

			AssertEquals(CellContentType.Cards, cardCell.ContentType);
			AssertEquals("The Doctor", cardCell.Label);
		}

		public void TestCardCellLabels_TimeChanneledBucket_MultipleAgeCells()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "The Doctor";
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var cardCell = grid[1, 1];

			AssertEquals(CellContentType.Cards, cardCell.ContentType);
			AssertEquals("The Doctor, 1d", cardCell.Label);
		}

		public void TestCardCellLabels_UnchanneledBuffer()
		{
			var pair = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 5, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled);
			var viewModel = pair.Item2;
			var grid = viewModel.ComponentGrid;
			var cardCell1 = grid[0, 2];
			var cardCell2 = grid[1, 2];
			var cardCell3 = grid[2, 2];
			var cardCell4 = grid[3, 2];
			var cardCell5 = grid[4, 2];

			AssertEquals(CellContentType.Cards, cardCell1.ContentType);
			AssertEquals("33.3 %", cardCell1.Label);

			AssertEquals(CellContentType.Cards, cardCell2.ContentType);
			AssertEquals("66.7 %", cardCell2.Label);

			AssertEquals(CellContentType.Cards, cardCell3.ContentType);
			AssertEquals("100 %", cardCell3.Label);

			AssertEquals(CellContentType.Cards, cardCell4.ContentType);
			AssertEquals("133.3 %", cardCell4.Label);

			AssertEquals(CellContentType.Cards, cardCell5.ContentType);
			AssertEquals("133.3 %+", cardCell5.Label);
		}

		public void TestCardCellLabels_TimeChanneledBuffer()
		{
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 5, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "The Doctor";
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var cardCell1 = grid[1, 2];
			var cardCell2 = grid[2, 2];
			var cardCell3 = grid[3, 2];
			var cardCell4 = grid[4, 2];
			var cardCell5 = grid[5, 2];

			AssertEquals(CellContentType.Cards, cardCell1.ContentType);
			AssertEquals("The Doctor, 33.3 %", cardCell1.Label);

			AssertEquals(CellContentType.Cards, cardCell2.ContentType);
			AssertEquals("The Doctor, 66.7 %", cardCell2.Label);

			AssertEquals(CellContentType.Cards, cardCell3.ContentType);
			AssertEquals("The Doctor, 100 %", cardCell3.Label);

			AssertEquals(CellContentType.Cards, cardCell4.ContentType);
			AssertEquals("The Doctor, 133.3 %", cardCell4.Label);

			AssertEquals(CellContentType.Cards, cardCell5.ContentType);
			AssertEquals("The Doctor, 133.3 %+", cardCell5.Label);
		}

		[ExpectNoExceptions]
		public void TestQueueIsNotEmpty_SecondaryChannel()
		{
			var sectionBucket = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty).Item1;
			sectionBucket.SectionConfiguration.OverrideChannels = true;
			sectionBucket.SectionConfiguration.ChannelSecondaryBy = "NOT";
			sectionBucket.SectionConfiguration.ShowUnchanneled = true;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "The Doctor";
			var secondaryChannel = BMSTestHelper.CreateSecondaryChannelForSection(sectionBucket, ChannelTypeList.Codes.Resource, resource.PK);

			var viewModel = BMSTestHelper.CreateViewModel(sectionBucket);
		}

		#endregion

		#region Age Heading Boldness

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestBoldAgeHeadingCellsMoveWhenWorkflowsChange()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item1;
			section.Component.FC_BufferTimespanInMinutes = 96 * 60;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today, staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-1), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-2), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-3), staffCode: resource1.GS_Code, lowEstMinutes: 60);

			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-4), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-7), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-8), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow8 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-9), staffCode: resource2.GS_Code, lowEstMinutes: 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0], workflow2.Parent.WorkflowItems[0], workflow3.Parent.WorkflowItems[0], workflow4.Parent.WorkflowItems[0],
				workflow5.Parent.WorkflowItems[0], workflow6.Parent.WorkflowItems[0], workflow7.Parent.WorkflowItems[0], workflow8.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var ageHeadingFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.AgeHeading && c.BackgroundFadeColor != null);
			var ageHeadingLocation = BMConstants.AgeHeadingPosition;

			AssertFadeCell(ageHeadingFadeCell, 7, ageHeadingLocation, Color.Beige, Color.Beige.FadeTowardsWhite());

			AssertEquals(false, grid[12, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[11, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[10, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[9, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[8, ageHeadingLocation].IsBoldLabel);
			AssertEquals(true, grid[7, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[6, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[5, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[4, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[3, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[2, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[1, ageHeadingLocation].IsBoldLabel);

			// Allocating tasks after moving workflows up the board - fade and bold should move up too.
			workflow1.FH_ReleaseDateTime = workflow1.FH_ReleaseDateTime.AddDays(-2);
			workflow2.FH_ReleaseDateTime = workflow2.FH_ReleaseDateTime.AddDays(-2);
			workflow3.FH_ReleaseDateTime = workflow3.FH_ReleaseDateTime.AddDays(-2);
			workflow4.FH_ReleaseDateTime = workflow4.FH_ReleaseDateTime.AddDays(-2);
			workflow5.FH_ReleaseDateTime = workflow5.FH_ReleaseDateTime.AddDays(-2);
			workflow6.FH_ReleaseDateTime = workflow6.FH_ReleaseDateTime.AddDays(-2);
			workflow7.FH_ReleaseDateTime = workflow7.FH_ReleaseDateTime.AddDays(-2);
			workflow8.FH_ReleaseDateTime = workflow8.FH_ReleaseDateTime.AddDays(-2);

			Factory.Save();

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			ageHeadingFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.AgeHeading && c.BackgroundFadeColor != null);
			AssertFadeCell(ageHeadingFadeCell, 5, ageHeadingLocation, Color.Beige, Color.Beige.FadeTowardsWhite());

			AssertEquals(false, grid[12, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[11, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[10, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[9, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[8, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[7, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[6, ageHeadingLocation].IsBoldLabel);
			AssertEquals(true, grid[5, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[4, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[3, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[2, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[1, ageHeadingLocation].IsBoldLabel);

			// Allocating tasks after moving workflows down the board - fade and bold should move down too.
			workflow1.FH_ReleaseDateTime = workflow1.FH_ReleaseDateTime.AddDays(4);
			workflow2.FH_ReleaseDateTime = workflow2.FH_ReleaseDateTime.AddDays(4);
			workflow3.FH_ReleaseDateTime = workflow3.FH_ReleaseDateTime.AddDays(4);
			workflow4.FH_ReleaseDateTime = workflow4.FH_ReleaseDateTime.AddDays(4);
			workflow5.FH_ReleaseDateTime = workflow5.FH_ReleaseDateTime.AddDays(4);
			workflow6.FH_ReleaseDateTime = workflow6.FH_ReleaseDateTime.AddDays(4);
			workflow7.FH_ReleaseDateTime = workflow7.FH_ReleaseDateTime.AddDays(4);
			workflow8.FH_ReleaseDateTime = workflow8.FH_ReleaseDateTime.AddDays(4);

			Factory.Save();

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			ageHeadingFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.AgeHeading && c.BackgroundFadeColor != null);
			AssertFadeCell(ageHeadingFadeCell, 9, ageHeadingLocation, Color.Beige, Color.Beige.FadeTowardsWhite());

			AssertEquals(false, grid[12, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[11, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[10, ageHeadingLocation].IsBoldLabel);
			AssertEquals(true, grid[9, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[8, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[7, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[6, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[5, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[4, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[3, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[2, ageHeadingLocation].IsBoldLabel);
			AssertEquals(false, grid[1, ageHeadingLocation].IsBoldLabel);
		}

		#endregion

		#region BackgroundFade

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestBackgroundFadeColor()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today, staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-1), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-2), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-3), staffCode: resource1.GS_Code, lowEstMinutes: 60);

			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-4), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-7), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-8), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow8 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-9), staffCode: resource2.GS_Code, lowEstMinutes: 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0], workflow2.Parent.WorkflowItems[0], workflow3.Parent.WorkflowItems[0], workflow4.Parent.WorkflowItems[0],
				workflow5.Parent.WorkflowItems[0], workflow6.Parent.WorkflowItems[0], workflow7.Parent.WorkflowItems[0], workflow8.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			// Allocating twice to check for fading the already faded color
			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var fadedZone0Color = BMConstants.Zone0DefaultColor.FadeTowardsWhite();
			var fadedZone1Color = BMConstants.Zone1DefaultColor.FadeTowardsWhite();
			var fadedZone2Color = BMConstants.Zone2DefaultColor.FadeTowardsWhite();
			var fadedZone3Color = BMConstants.Zone3DefaultColor.FadeTowardsWhite();

			var channel1FadeCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource1.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel1FadeCell, 10, 2, BMConstants.Zone3DefaultColor, fadedZone3Color);

			AssertColorEquals(fadedZone2Color, grid[9, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[8, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[7, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[6, 2].BackColor.Value);

			AssertColorEquals(fadedZone1Color, grid[5, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[4, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[3, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[2, 2].BackColor.Value);

			AssertColorEquals(fadedZone0Color, grid[1, 2].BackColor.Value);

			var channel2FadeCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource2.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel2FadeCell, 6, 3, BMConstants.Zone2DefaultColor, fadedZone2Color);

			AssertColorEquals(fadedZone1Color, grid[5, 3].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[4, 3].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[3, 3].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[2, 3].BackColor.Value);

			AssertColorEquals(fadedZone0Color, grid[1, 3].BackColor.Value);

			var ageHeadingFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.AgeHeading && c.BackgroundFadeColor != null);

			var ageHeadingLocation = BMConstants.AgeHeadingPosition;

			AssertFadeCell(ageHeadingFadeCell, 7, ageHeadingLocation, Color.Beige, Color.Beige.FadeTowardsWhite());

			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[6, ageHeadingLocation].BackColor.Value);

			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[5, ageHeadingLocation].BackColor.Value);
			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[4, ageHeadingLocation].BackColor.Value);
			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[3, ageHeadingLocation].BackColor.Value);
			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[2, ageHeadingLocation].BackColor.Value);

			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[1, ageHeadingLocation].BackColor.Value);

			// Allocating tasks after moving workflows up the board - fade should move up too.
			workflow1.FH_ReleaseDateTime = workflow1.FH_ReleaseDateTime.AddDays(-2);
			workflow2.FH_ReleaseDateTime = workflow2.FH_ReleaseDateTime.AddDays(-2);
			workflow3.FH_ReleaseDateTime = workflow3.FH_ReleaseDateTime.AddDays(-2);
			workflow4.FH_ReleaseDateTime = workflow4.FH_ReleaseDateTime.AddDays(-2);
			workflow5.FH_ReleaseDateTime = workflow5.FH_ReleaseDateTime.AddDays(-2);
			workflow6.FH_ReleaseDateTime = workflow6.FH_ReleaseDateTime.AddDays(-2);
			workflow7.FH_ReleaseDateTime = workflow7.FH_ReleaseDateTime.AddDays(-2);
			workflow8.FH_ReleaseDateTime = workflow8.FH_ReleaseDateTime.AddDays(-2);

			Factory.Save();

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			channel1FadeCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource1.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel1FadeCell, 9, 2, Color.LightBlue, fadedZone2Color);

			AssertColorEquals(BMConstants.Zone3DefaultColor, grid[12, 2].BackColor.Value);
			AssertNull(grid[12, 2].BackgroundFadeColor);
			AssertColorEquals(BMConstants.Zone3DefaultColor, grid[11, 2].BackColor.Value);
			AssertNull(grid[11, 2].BackgroundFadeColor);
			AssertColorEquals(BMConstants.Zone3DefaultColor, grid[10, 2].BackColor.Value);
			AssertNull(grid[10, 2].BackgroundFadeColor);

			AssertColorEquals(fadedZone2Color, grid[8, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[7, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[6, 2].BackColor.Value);

			AssertColorEquals(fadedZone1Color, grid[5, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[4, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[3, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[2, 2].BackColor.Value);

			AssertColorEquals(fadedZone0Color, grid[1, 2].BackColor.Value);
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestBackgroundFadeColor_Bucket()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.TimePerCell = new ZDateTime(2013, 01, 01, 8, 0, 0);
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today, staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-1), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-2), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-3), staffCode: resource1.GS_Code, lowEstMinutes: 60);

			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-4), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-7), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-8), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow8 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Today.AddDays(-9), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0], workflow2.Parent.WorkflowItems[0], workflow3.Parent.WorkflowItems[0], workflow4.Parent.WorkflowItems[0],
				workflow5.Parent.WorkflowItems[0], workflow6.Parent.WorkflowItems[0], workflow7.Parent.WorkflowItems[0], workflow8.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			// Allocating twice to check for fading the already faded color
			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var fadedBucketColor = Color.Beige.FadeTowardsWhite();
			var bucketDefaultColor = Color.Beige;

			var channel1GradientCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource1.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel1GradientCell, 10, 1, bucketDefaultColor, fadedBucketColor);
			AssertCellsAboveHaveBackColor(channel1GradientCell, grid, fadedBucketColor);

			var channel2GradientCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource2.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel2GradientCell, 6, 2, bucketDefaultColor, fadedBucketColor);
			AssertCellsAboveHaveBackColor(channel2GradientCell, grid, fadedBucketColor);

			var ageHeadingGradientCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.AgeHeading && c.BackgroundFadeColor != null);
			var ageHeadingColumn = BMConstants.AgeHeadingPosition;
			AssertFadeCell(ageHeadingGradientCell, 7, ageHeadingColumn, Color.Beige, Color.Beige.FadeTowardsWhite());
			AssertCellsAboveHaveBackColor(ageHeadingGradientCell, grid, fadedBucketColor);

			// Allocating tasks after moving workflows up the board - fade should move up too.
			workflow1.FH_ReleaseDateTime = workflow1.FH_ReleaseDateTime.AddDays(-2);
			workflow2.FH_ReleaseDateTime = workflow2.FH_ReleaseDateTime.AddDays(-2);
			workflow3.FH_ReleaseDateTime = workflow3.FH_ReleaseDateTime.AddDays(-2);
			workflow4.FH_ReleaseDateTime = workflow4.FH_ReleaseDateTime.AddDays(-2);
			workflow5.FH_ReleaseDateTime = workflow5.FH_ReleaseDateTime.AddDays(-2);
			workflow6.FH_ReleaseDateTime = workflow6.FH_ReleaseDateTime.AddDays(-2);
			workflow7.FH_ReleaseDateTime = workflow7.FH_ReleaseDateTime.AddDays(-2);
			workflow8.FH_ReleaseDateTime = workflow8.FH_ReleaseDateTime.AddDays(-2);

			Factory.Save();

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			channel1GradientCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource1.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel1GradientCell, 9, 1, bucketDefaultColor, fadedBucketColor);

			AssertColorEquals(bucketDefaultColor, grid[12, 1].BackColor.Value);
			AssertNull(grid[12, 1].BackgroundFadeColor);
			AssertColorEquals(bucketDefaultColor, grid[11, 1].BackColor.Value);
			AssertNull(grid[11, 1].BackgroundFadeColor);
			AssertColorEquals(bucketDefaultColor, grid[10, 1].BackColor.Value);
			AssertNull(grid[10, 1].BackgroundFadeColor);

			AssertCellsAboveHaveBackColor(channel1GradientCell, grid, fadedBucketColor);
		}

		void AssertCellsAboveHaveBackColor(CellContent cell, ComponentGrid grid, Color expectedBackColor)
		{
			for (var i = 1; i < cell.Row; i++)
			{
				AssertColorEquals(expectedBackColor, grid[i, cell.Column].BackColor.Value);
			}
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestBackgroundFadeColor_SubComponentZoneHeading()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR Resource");
			var resourceNonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCC", "Non CCR Resource");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(buffer);
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR, resourceNonCCR);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var task = workflow1.Parent.WorkflowItems[0];
			var taskPreConstraint = CreateTask(workflow1, resourceNonCCR.GS_Code, 60, sequence: 400, description: "Workflow 1 - task 2");
			Assert("Pre-constraint task", taskPreConstraint.P9_Sequence < task.P9_Sequence);
			var taskPostConstraint = CreateTask(workflow1, resourceNonCCR.GS_Code, 60, sequence: 600, description: "Workflow 1 - task 3");
			Assert("Post-constraint task", taskPostConstraint.P9_Sequence > task.P9_Sequence);

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0],
				workflow1.Parent.WorkflowItems[1],
				workflow1.Parent.WorkflowItems[2],
				workflow2.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var postConstraintZoneHeaders = grid.Cells.Where(c => c.ContentType == CellContentType.SubComponentZoneHeading).Where(c => c.Zone != null).ToArray();
			AssertEquals("Postconstraint-header must exist with 13 rows", 13, postConstraintZoneHeaders.Length);

			var postConstraintZoneHeadingFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.SubComponentZoneHeading && c.BackgroundFadeColor != null);
			var fadedZone3Color = BMConstants.Zone3DefaultColor.FadeTowardsWhite();
			AssertFadeCell(postConstraintZoneHeadingFadeCell, 13, 4, BMConstants.Zone3DefaultColor, fadedZone3Color);

			var postConstraintZoneHeadingFadeCellCollapseFrom = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.SubComponentZoneHeading && c.Row == 13);
			var postConstraintZoneHeadingFadeCellCollapseTo = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.SubComponentZoneHeading && c.Row == 11);
			postConstraintZoneHeadingFadeCellCollapseFrom.CollapsedToCell = postConstraintZoneHeadingFadeCellCollapseTo;
			grid.AllocateTasks_ForTest(section, viewModel, tasks);
			AssertFadeCell(postConstraintZoneHeadingFadeCellCollapseTo, 11, 4, BMConstants.Zone3DefaultColor, fadedZone3Color);
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestBackgroundFadeColor_CCRHeading()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR2", "CCR Resource 2");
			var resourceCCR3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR3", "CCR Resource 3");
			var resourceCCR4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR4", "CCR Resource 4");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceNonCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC2", "Non CCR Resource 2");
			var resourceNonCCR3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC3", "Non CCR Resource 3");

			var buffer = section.Component;
			var preConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 64 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR1, resourceCCR2, resourceCCR3, resourceCCR4, resourceNonCCR1, resourceNonCCR2, resourceNonCCR3);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			resourceCCR2.DesignateAsCCR(buffer);
			resourceCCR3.DesignateAsCCR(buffer);
			resourceCCR4.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR3.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR3.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR4.PK);

			var workflowPreConstraintFadeName = "WF-pre-fade";
			var workflowPreConstraintFade = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 10, sequence: 500, description: workflowPreConstraintFadeName + ": constraint");
			CreateTask(workflowPreConstraintFade, resourceNonCCR1.GS_Code, lowEstMinutes: 80, sequence: 400, description: workflowPreConstraintFadeName + ": pre");
			CreateTask(workflowPreConstraintFade, resourceNonCCR2.GS_Code, lowEstMinutes: 10, sequence: 600, description: workflowPreConstraintFadeName + ": post");

			var workflowPostConstraintFadeName = "WF-post-fade";
			var workflowPostConstraintFade = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today.AddDays(-365), staffCode: resourceCCR2.GS_Code, lowEstMinutes: 10, sequence: 500, description: workflowPostConstraintFadeName + ": constraint");
			CreateTask(workflowPostConstraintFade, resourceNonCCR1.GS_Code, lowEstMinutes: 10, sequence: 400, description: workflowPostConstraintFadeName + ": pre");
			CreateTask(workflowPostConstraintFade, resourceNonCCR2.GS_Code, lowEstMinutes: 80, sequence: 600, description: workflowPostConstraintFadeName + ": post");

			var workflowPreConstraintFadeGroup1Name = "WF-pre-fade-grp1";
			var workflowPreConstraintFadeGroup1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today.AddDays(-1), staffCode: resourceCCR3.GS_Code, lowEstMinutes: 5, description: workflowPreConstraintFadeGroup1Name + ": constraint");
			CreateTask(workflowPreConstraintFadeGroup1, resourceNonCCR1.GS_Code, lowEstMinutes: 5, sequence: 400, description: workflowPreConstraintFadeGroup1Name + ": pre");
			CreateTask(workflowPreConstraintFadeGroup1, resourceNonCCR2.GS_Code, lowEstMinutes: 40, sequence: 600, description: workflowPreConstraintFadeGroup1Name + ": post");

			var workflowPreConstraintFadeGroup2Name = "WF-pre-fade-grp2";
			var workflowPreConstraintFadeGroup2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today.AddDays(-2), staffCode: resourceCCR4.GS_Code, lowEstMinutes: 5, description: workflowPreConstraintFadeGroup2Name + ": constraint");
			CreateTask(workflowPreConstraintFadeGroup2, resourceNonCCR1.GS_Code, lowEstMinutes: 5, sequence: 400, description: workflowPreConstraintFadeGroup2Name + ": pre");
			CreateTask(workflowPreConstraintFadeGroup2, resourceNonCCR2.GS_Code, lowEstMinutes: 40, sequence: 600, description: workflowPreConstraintFadeGroup2Name + ": post");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = workflowPreConstraintFade.Tasks.Concat(workflowPostConstraintFade.Tasks).Concat(workflowPreConstraintFadeGroup1.Tasks).Concat(workflowPreConstraintFadeGroup2.Tasks).ToArray();

			Factory.Save(); // Dump the cache.

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var tests = new[]
			{
				Tuple.Create("pre-fading", 5, 13, BMConstants.Zone3DefaultColor),
				Tuple.Create("post-fading", 11, 1, BMConstants.Zone0DefaultColor),
				Tuple.Create("pre-fading group", 17, 11, BMConstants.Zone3DefaultColor),
			};

			foreach (var test in tests)
			{
				var column = test.Item2;
				var fadedRow = test.Item3;
				var cellColor = test.Item4;

				var ccrHeadings = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == column).ToArray();
				AssertEquals("CCR headings must exist with 13 rows", 13, ccrHeadings.Length);

				var ccrHeadingsFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.CCRHeading && c.Column == column && c.BackgroundFadeColor != null);
				var fadedColor = cellColor.FadeTowardsWhite();
				AssertFadeCell(ccrHeadingsFadeCell, fadedRow, column, cellColor, fadedColor);
			}
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestBackgroundFadeColor_WhenLastTaskInChannelRemoved()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item1;
			section.Component.FC_BufferTimespanInMinutes = 96 * 60;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: yesterday, staffCode: resource1.GS_Code, lowEstMinutes: 60);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0]
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var fadedZone0Color = BMConstants.Zone0DefaultColor.FadeTowardsWhite();
			var fadedZone1Color = BMConstants.Zone1DefaultColor.FadeTowardsWhite();
			var fadedZone2Color = BMConstants.Zone2DefaultColor.FadeTowardsWhite();
			var fadedZone3Color = BMConstants.Zone3DefaultColor.FadeTowardsWhite();

			var channel1FadeCell = grid.Cells.SingleOrDefault(c => c.Channel != null && c.Channel.EntityPK == resource1.PK && c.BackgroundFadeColor != null);
			AssertFadeCell(channel1FadeCell, 12, 2, BMConstants.Zone3DefaultColor, fadedZone3Color);

			AssertColorEquals(fadedZone2Color, grid[9, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[8, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[7, 2].BackColor.Value);
			AssertColorEquals(fadedZone2Color, grid[6, 2].BackColor.Value);

			AssertColorEquals(fadedZone1Color, grid[5, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[4, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[3, 2].BackColor.Value);
			AssertColorEquals(fadedZone1Color, grid[2, 2].BackColor.Value);

			AssertColorEquals(fadedZone0Color, grid[1, 2].BackColor.Value);

			var ageHeadingFadeCell = grid.Cells.SingleOrDefault(c => c.ContentType == CellContentType.AgeHeading && c.BackgroundFadeColor != null);

			var ageHeadingLocation = BMConstants.AgeHeadingPosition;

			AssertFadeCell(ageHeadingFadeCell, 12, ageHeadingLocation, Color.Beige, Color.Beige.FadeTowardsWhite());

			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[6, ageHeadingLocation].BackColor.Value);

			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[5, ageHeadingLocation].BackColor.Value);
			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[4, ageHeadingLocation].BackColor.Value);
			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[3, ageHeadingLocation].BackColor.Value);
			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[2, ageHeadingLocation].BackColor.Value);

			AssertColorEquals(Color.Beige.FadeTowardsWhite(), grid[1, ageHeadingLocation].BackColor.Value);

			tasks.First().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var emptyTasksCollection = Array.Empty<ProcessTask>();
			grid.AllocateTasks_ForTest(section, viewModel, emptyTasksCollection);

			Assert("When there are no tasks on the board, there should be no fades, and therefore all background fade colors should be null.", grid.Cells.All(c => c.BackgroundFadeColor == null));
		}

		static void AssertFadeCell(CellContent fadeCell, int expectedRow, int expectedColumn, Color expectedBackColor, Color expectedFadeColor)
		{
			AssertNotNull("Should have found a fade cell", fadeCell);
			AssertEquals("Cell row", expectedRow, fadeCell.Row);
			AssertEquals("Cell column", expectedColumn, fadeCell.Column);
			AssertColorEquals("BackColor", expectedBackColor, fadeCell.BackColor.Value);
			AssertColorEquals("FadeColor", expectedFadeColor, fadeCell.BackgroundFadeColor.Value);
		}

		#endregion

		#region CardSortType

		public void TestCardSortType_Bucket()
		{
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item2;
			var cardCells = viewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.Cards);

			foreach (var cell in cardCells)
			{
				AssertEquals(CardSortType.ReleaseSequence, cell.CardSortType);
			}
		}

		public void TestCardSortType_Buffer()
		{
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item2;
			var cardCells = viewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.Cards);

			foreach (var cell in cardCells)
			{
				AssertEquals(CardSortType.BufferWorkSequence, cell.CardSortType);
			}
		}

		public void TestCardSortType_ReleaseScheduler()
		{
			var system = config.System;
			var buffer = config.Buffer;
			CreateSubBuffer(buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var section = CreateReleaseSchedulerBoardSection(buffer, group);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2);
			resource1.DesignateAsCCR(buffer);

			AssertNoErrors(section);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			AssertEquals(3, grid.TotalColumns);
			AssertEquals(3, grid.TotalRows);

			AssertEquals(CellContentType.Cards, grid[1, 1].ContentType);
			AssertEquals(CellContentType.Cards, grid[1, 2].ContentType);
			AssertEquals("Should sort released content by released time", CardSortType.LastTransferTime, grid[1, 1].CardSortType);
			AssertEquals("Should sort released content by released time", CardSortType.LastTransferTime, grid[1, 2].CardSortType);

			AssertEquals(CellContentType.Cards, grid[2, 1].ContentType);
			AssertEquals(CellContentType.Cards, grid[2, 2].ContentType);
			AssertEquals("Should sort un-released content by release sequence", CardSortType.ReleaseSequence, grid[2, 1].CardSortType);
			AssertEquals("Should sort un-released content by release sequence", CardSortType.ReleaseSequence, grid[2, 2].CardSortType);
		}

		#endregion

		#region CCR Heading

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_Zone()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR2", "CCR Resource 2");
			var resourceCCR3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR3", "CCR Resource 3");

			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceNonCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC2", "Non CCR Resource 2");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceNonCCR1, resourceCCR1, resourceNonCCR2, resourceCCR2, resourceCCR3);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			resourceCCR2.DesignateAsCCR(buffer);
			resourceCCR3.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR3.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var task = workflow1.Parent.WorkflowItems[0];
			var taskPreConstraint = CreateTask(workflow1, resourceNonCCR1.GS_Code, 60, sequence: 400, description: "Workflow 1 - task 2");
			Assert("Pre-constraint task", taskPreConstraint.P9_Sequence < task.P9_Sequence);
			var taskPostConstraint = CreateTask(workflow1, resourceNonCCR1.GS_Code, 60, sequence: 600, description: "Workflow 1 - task 3");
			Assert("Post-constraint task", taskPostConstraint.P9_Sequence > task.P9_Sequence);

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0],
				workflow1.Parent.WorkflowItems[1],
				workflow1.Parent.WorkflowItems[2],
				workflow2.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var ccrHeadings1 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == 4).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col=  4", 13, ccrHeadings1.Length);

			var ccrHeadings2 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == 9).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col = 9", 13, ccrHeadings2.Length);

			foreach (var ccrHeadings in new IEnumerable<CellContent>[] { ccrHeadings1, ccrHeadings2 })
			{
				AssertEquals("row = 1 is Zone 0", 0, ccrHeadings.SingleOrDefault(c => c.Row == 1).Zone);
				AssertEquals("row = 2 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 2).Zone);
				AssertEquals("row = 3 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 3).Zone);
				AssertEquals("row = 4 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 4).Zone);
				AssertEquals("row = 5 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 5).Zone);
				AssertEquals("row = 6 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 6).Zone);
				AssertEquals("row = 7 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 7).Zone);
				AssertEquals("row = 8 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 8).Zone);
				AssertEquals("row = 9 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 9).Zone);
				AssertEquals("row = 10 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 10).Zone);
				AssertEquals("row = 11 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 11).Zone);
				AssertEquals("row = 12 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 12).Zone);
				AssertEquals("row = 13 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 13).Zone);
			}
		}

		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_Zone_2()
		{
			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.Component.FC_BufferTimespanInMinutes = 12 * 60;

			var resourceCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");

			var buffer = section.Component;
			var preConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, name: "pre", timespanMinutes: 8 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 8 * 60);
			var postConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, name: "post", timespanMinutes: 4 * 60, offsetMinutes: 8 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			var ccrHeadings1 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == 1).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col 1", 13, ccrHeadings1.Length);

			foreach (var ccrHeadings in new IEnumerable<CellContent>[] { ccrHeadings1 })
			{
				AssertEquals("row = 1 is Zone 0", 0, ccrHeadings.SingleOrDefault(c => c.Row == 1).Zone);
				AssertEquals("row = 2 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 2).Zone);
				AssertEquals("row = 3 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 3).Zone);
				AssertEquals("row = 4 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 4).Zone);
				AssertEquals("row = 5 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 5).Zone);
				AssertEquals("row = 6 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 6).Zone);
				AssertEquals("row = 7 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 7).Zone);
				AssertEquals("row = 8 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 8).Zone);
				AssertEquals("row = 9 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 9).Zone);
				AssertEquals("row = 10 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 10).Zone);
				AssertEquals("row = 11 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 11).Zone);
				AssertEquals("row = 12 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 12).Zone);
				AssertEquals("row = 13 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 13).Zone);
			}
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_Zone_FlowDown()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR2", "CCR Resource 2");
			var resourceCCR3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR3", "CCR Resource 3");

			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceNonCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC2", "Non CCR Resource 2");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceNonCCR1, resourceCCR1, resourceNonCCR2, resourceCCR2, resourceCCR3);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Down, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			resourceCCR2.DesignateAsCCR(buffer);
			resourceCCR3.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR3.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var task = workflow1.Parent.WorkflowItems[0];
			var taskPreConstraint = CreateTask(workflow1, resourceNonCCR1.GS_Code, 60, sequence: 400, description: "Workflow 1 - task 2");
			Assert("Pre-constraint task", taskPreConstraint.P9_Sequence < task.P9_Sequence);
			var taskPostConstraint = CreateTask(workflow1, resourceNonCCR1.GS_Code, 60, sequence: 600, description: "Workflow 1 - task 3");
			Assert("Post-constraint task", taskPostConstraint.P9_Sequence > task.P9_Sequence);

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0],
				workflow1.Parent.WorkflowItems[1],
				workflow1.Parent.WorkflowItems[2],
				workflow2.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var ccrHeadings1 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == 4).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col=  4", 13, ccrHeadings1.Length);

			var ccrHeadings2 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == 9).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col = 9", 13, ccrHeadings2.Length);

			foreach (var ccrHeadings in new IEnumerable<CellContent>[] { ccrHeadings1, ccrHeadings2 })
			{
				AssertEquals("row = 1 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 1).Zone);
				AssertEquals("row = 2 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 2).Zone);
				AssertEquals("row = 3 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 3).Zone);
				AssertEquals("row = 4 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 4).Zone);
				AssertEquals("row = 5 is Zone 3", 3, ccrHeadings.SingleOrDefault(c => c.Row == 5).Zone);
				AssertEquals("row = 6 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 6).Zone);
				AssertEquals("row = 7 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 7).Zone);
				AssertEquals("row = 8 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 8).Zone);
				AssertEquals("row = 9 is Zone 2", 2, ccrHeadings.SingleOrDefault(c => c.Row == 9).Zone);
				AssertEquals("row = 10 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 10).Zone);
				AssertEquals("row = 11 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 11).Zone);
				AssertEquals("row = 12 is Zone 1", 1, ccrHeadings.SingleOrDefault(c => c.Row == 12).Zone);
				AssertEquals("row = 13 is Zone 0", 0, ccrHeadings.SingleOrDefault(c => c.Row == 13).Zone);
			}
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_2Channels()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR1, resourceNonCCR1);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var tasks = new[]
			{
				workflow1.Parent.WorkflowItems[0],
				workflow2.Parent.WorkflowItems[0],
			};

			grid.AllocateTasks_ForTest(section, viewModel, tasks);

			var ccrHeadings1 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Column == 1).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col=  2", 13, ccrHeadings1.Length);

			var channelHeaders = grid.Cells.Where(c => c.Row == 0 && (c.Column == 2 || c.Column == 5)).ToArray();
			AssertNotNull("Channel's header must not have NULL channel", channelHeaders[0].Channel);
			AssertNotNull("Channel's header must not have NULL channel", channelHeaders[1].Channel);
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_OnNonConstrainedModeReleaseGroup()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var nonConstrainedModeGroup = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, nonConstrainedModeGroup, constrainedModeComponent: buffer);
			buffer.ReleaseGroupLinks[0].FO_IsConstrainedMode = ZBool.False;
			nonConstrainedModeGroup.Staff.AddRange(resourceCCR1, resourceNonCCR1);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: nonConstrainedModeGroup.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceNonCCR1.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			grid.AllocateTasks_ForTest(section, viewModel, new[] { workflow1.Parent.WorkflowItems[0], workflow2.Parent.WorkflowItems[0] });

			var ccrHeadings = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).ToArray();
			AssertEquals("CCR Headings must NOT exist", 0, ccrHeadings.Length);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestTaskEstimate_IncludePenetratedComponents()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			var buffer = section.Component;

			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, buffer.PK);

			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR");
			ccr.DesignateAsCCR(buffer);
			var nonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCR", "Non CCR");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, nonCCR.PK, overrideChannels: true);

			var preConstraintWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(0), staffCode: nonCCR.GS_Code, lowEstMinutes: 600, sequence: 1, description: "Workflow 2 - task 1");
			BMSTestHelper.CreateTask(preConstraintWorkflow, ccr.GS_Code, sequence: 20, lowEstMinutes: 15);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, preConstraintWorkflow.Tasks.ToArray());

			var primaryAxis = 4;
			var secondaryAxis = 13;

			var result = BMSTestHelper.GetTaskEstimatesInCells(grid, primaryAxes: primaryAxis.WrapWithEnumerable(), secondaryAxes: secondaryAxis.WrapWithEnumerable(), includePenetratedComponents: true, components: preBuffer.PK);
			AssertEquals("Task Estimate must include Penetrated-buffer preBuffer", 10m, result);

			result = BMSTestHelper.GetTaskEstimatesInCells(grid, primaryAxes: primaryAxis.WrapWithEnumerable(), secondaryAxes: secondaryAxis.WrapWithEnumerable(), components: preBuffer.PK);
			AssertEquals("Task Estimate must not include Penetrated-buffer preBuffer", 0m, result);

			result = BMSTestHelper.GetTaskEstimatesInCells(grid, primaryAxes: primaryAxis.WrapWithEnumerable(), includePenetratedComponents: true, components: preBuffer.PK);
			AssertEquals("Task Estimate must include Penetrated-buffer preBuffer", 10m, result);

			result = BMSTestHelper.GetTaskEstimatesInCells(grid, primaryAxes: primaryAxis.WrapWithEnumerable(), components: preBuffer.PK);
			AssertEquals("Task Estimate must not include Penetrated-buffer preBuffer", 0m, result);

			result = BMSTestHelper.GetTaskEstimatesInCells(grid, secondaryAxes: secondaryAxis.WrapWithEnumerable(), includePenetratedComponents: true, components: preBuffer.PK);
			AssertEquals("Task Estimate must include Penetrated-buffer preBuffer", 10m, result);

			result = BMSTestHelper.GetTaskEstimatesInCells(grid, secondaryAxes: secondaryAxis.WrapWithEnumerable(), components: preBuffer.PK);
			AssertEquals("Task Estimate must not include Penetrated-buffer preBuffer", 0m, result);
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_2Channels_HorizontalOrientation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceCCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR2", "CCR Resource 1");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR1, resourceNonCCR1, resourceCCR2);

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Left, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			resourceCCR2.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR2.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 1 - task 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, description: "Workflow 2 - task 1");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR2.GS_Code, lowEstMinutes: 60, sequence: 500, description: "Workflow 3 - task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;

			grid.AllocateTasks_ForTest(section, viewModel, new[] { workflow1.Parent.WorkflowItems[0], workflow2.Parent.WorkflowItems[0], workflow3.Parent.WorkflowItems[0] });

			var ccrHeadings1 = grid.Cells.Where(c => c.ContentType == CellContentType.CCRHeading).Where(c => c.Row == 1).ToArray();
			AssertEquals("CCR Headings must exist with 13 rows on col=  1", 13, ccrHeadings1.Length);

			var channelHeaders = grid.Cells.Where(c => c.Column == 0 && (c.Row == 2 || c.Row == 5 || c.Row == 7)).ToArray();
			AssertNotNull("Channel's header must not have NULL channel", channelHeaders[0].Channel);
			AssertNotNull("Channel's header must not have NULL channel", channelHeaders[1].Channel);
			AssertNotNull("Channel's header must not have NULL channel", channelHeaders[2].Channel);
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRHeading_Zone0Position_WithDifferentFlowDirections()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();

			foreach (var direction in new FlowDirectionList())
			{
				config.Section.SectionConfiguration.FlowDirection = direction.ToString();
				var viewModel = config.ResetViewModel();

				var zone0CCRHeadingCell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.PrimaryAxis == 3 && c.Zone == 0);
				var expectedSecondaryAxis = (new[] { FlowDirectionList.Codes.Down, FlowDirectionList.Codes.Right }).Contains(direction.ToString())
					? 13 : 1;
				var message = string.Format("Zone 0 for flow-direction {0} should be at secondary-axis: {1}", direction.ToString(), expectedSecondaryAxis);
				AssertEquals(message, expectedSecondaryAxis, zone0CCRHeadingCell.SecondaryAxis);
			}
		}

		#endregion

		#region Cache

		[RequiresSTA]
		[TestDate(2014, 10, 24)]
		public void TestAllocateTasks_ShouldPopulateViewModelCache()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff1 = CreateStaffInCurrentBranchDept("XW@", "X-Wing @Alicioussness");
			var staff2 = CreateStaffInCurrentBranchDept("TSW", "Tyroil Smoochie-Wallace");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(staff1, staff2);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = CreateTask(workflow, staff1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			var task2 = CreateTask(workflow, staff2.GS_Code, 60);
			Factory.Save();

			var (section, viewModel) = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, channels: [staff1, staff2]);

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, [task1, task2]);

			var resource1Channel = viewModel.PrimaryChannels.First(c => c.EntityPK == staff1.PK);
			var resource2Channel = viewModel.PrimaryChannels.First(c => c.EntityPK == staff2.PK);

			AssertEquals("Zone 3, Working", resource1Channel.GetStatusInCacheForTest());
			AssertEquals("Zone 3, Idle", resource2Channel.GetStatusInCacheForTest());
		}

		#endregion

		#region Grid as HTML

		[RequiresSTA]
		public void TestGetGridAsHTML()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			Factory.Save();
			var html = BMSTestHelper.GetGridAsHTML(config.SectionViewModel);
			var expected = @"<table border='1'><tr><td>row\column</td><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>6</td><td>7</td></tr>
<tr><td>0</td><td>Label</td><td>Label</td><td>ChnlHead-CCR</td><td>Label</td><td>Label</td><td>Label</td><td>ChnlHead-Non-CCR1</td><td>ChnlHead-Non-CCR2</td></tr>
<tr><td>1</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead0</td><td>Cons-Zone0</td><td>Cons-Zone0</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>2</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead1</td><td>Cons-Zone0</td><td>Cons-Zone1</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>3</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead1</td><td>Cons-Zone0</td><td>Cons-Zone2</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>4</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead1</td><td>Cons-Zone0</td><td>Cons-Zone2</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>5</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead1</td><td>Cons-Zone0</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>6</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead2</td><td>Cons-Zone1</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>7</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead2</td><td>Cons-Zone1</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>8</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead2</td><td>Cons-Zone1</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>9</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead2</td><td>Cons-Zone2</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>10</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead3</td><td>Cons-Zone2</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>11</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead3</td><td>Cons-Zone3</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>12</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/></td><td>ZoneHead3</td><td>Cons-Zone3</td><td>Cons-Zone3</td><td>Card<br/></td><td>Card<br/></td></tr>
<tr><td>13</td><td>AgeHead</td><td>CCRHead-Zone</td><td>Card<br/>flow=UP, task=complex task for CCR, workflow=workflow for CCR, Est=.25hrs, ReleaseDate-Today=0days, penetrated=bufferConstraint<br /></td><td>ZoneHead3</td><td>Cons-Zone3</td><td>Cons-Zone3</td><td>Card<br/>flow=UP, task=complex task for NC1, workflow=workflow for NC1, Est=.25hrs, ReleaseDate-Today=0days, penetrated=<br /></td><td>Card<br/>flow=UP, task=complex task for NC2, workflow=workflow for NC2, Est=.25hrs, ReleaseDate-Today=0days, penetrated=<br /></td></tr>
</table>";

			var message = string.Format(CultureInfo.InvariantCulture, "Expected: <div>{0}</div> Was: <div>{1}</div>", expected, html);
			AssertionWithHtml.HtmlAssertEquals(message, expected, html);
		}

		#endregion

		#region Flow Direction

		public void TestVisualBoardPositionTest_ConvertToFlowDirectioned()
		{
			var position = new VisualBoardPositionForTest<int>(primaryAxis: 3, secondaryAxis: 5);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, "INQ");
			config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			position.ConvertToFlowDirectioned(config.Section);
			AssertEquals("Given current position flow-direction = DOWN and config flow-direction = UP, position should be converted", 13 - 5 + 1, position.SecondaryAxis);

			position.ConvertToFlowDirectioned(config.Section);
			AssertEquals("re-converting should return previous conreted value", 13 - 5 + 1, position.SecondaryAxis);
		}

		#endregion

		#region Implementation

		ProcessHeader CreateWorkflow(BMComponent component)
		{
			return BMSTestHelper.CreateWorkflow(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory), "workflow", currentComponent: component);
		}

		CellContentType[][] GetExpectedContentTypes(int total, CellContentType[] expectedCell)
		{
			var expectedContentTypes = new CellContentType[total][];
			for (int i = 0; i < expectedContentTypes.Length; i++)
			{
				expectedContentTypes[i] = expectedCell;
			}
			return expectedContentTypes;
		}

		const int zoneHeadingPosition = BMConstants.ZoneHeadingPosition;
		const int ageHeadingPosition = BMConstants.AgeHeadingPosition;

		VisualBoardTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}
}
