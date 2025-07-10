using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test.Activation
{
	class ActivationReportBuilderTest : TestCaseWithFactoryAndMocks
	{
		[TestDate(2013, 1, 31)]
		public void TestBuildReport_UserHistoriesOnly()
		{
			var args = GetUserSyncArgs().ToArray();
			AssertReportEquals("User histories report", args, UsersOnlyReport, GetUsersOnlyAcceptedCellRefs(), GetUsersOnlyOverwrittenCellRefs(), GetUsersOnlyUnchangedCellRefs());
		}

		[TestDate(2013, 1, 31)]
		public void TestBuildReport_GroupHistoriesOnly()
		{
			var args = GetGroupSyncArgs().ToArray();
			AssertReportEquals("Group histories report", args, GroupsOnlyReport, GetGroupsOnlyAcceptedCellRefs(), GetGroupsOnlyOverwrittenCellRefs(), GetGroupsOnlyUnchangedCellRefs());
		}

		[TestDate(2013, 1, 31)]
		public void TestBuildReport_UserAndGroupHistories()
		{
			var args = GetUserSyncArgs().Concat(GetGroupSyncArgs()).ToArray();
			AssertReportEquals("User and group histories report", args, UsersAndGroupsReport, GetUsersAndGroupsAcceptedCellRefs(), GetUsersAndGroupsOverwrittenCellRefs(), GetUsersAndGroupsUnchangedCellRefs());
		}

		#region Expected Reports

		#region UsersOnlyReport

		const string UsersOnlyReport = @"{G}-[Legend]
{B}-[Active Directory Integration Activation Report]   {G}-[Accepted value]
{G}-[Overwritten value]
{B}-[Report generated at 31-Jan-13 00:00:00]
{A}-[Users]
{B}-[Login Name]   {C}-[Domain Name]   {D}-[Entities matched?]   {E}-[Active]   {F}-[Title]   {G}-[Full Name]   {H}-[Email Address]   {I}-[Work Extension]   {J}-[Work Phone]   {K}-[Mobile Phone]   {L}-[Fax Num]   {M}-[Home Phone]   {N}-[Pager]   {O}-[Street Address]   {P}-[City]   {Q}-[State]   {R}-[Postcode]   {S}-[Language]
{A}-[Active Directory]   {B}-[gandalf.the.white]   {C}-[domain1.zone]   {D}-[N]   {E}-[N]
{A}-[<ProductName>]   {B}-[galdalf.the.grey]   {C}-[domain2.zone]   {D}-[N]   {E}-[Y]
{A}-[Expected Synced Value]   {B}-[gandalf.the.white]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[Active Directory]   {B}-[Manwë]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {P}-[Mount Taniquetil]   {S}-[en-AU]
{A}-[<ProductName>]   {B}-[Manwe]   {C}-[domain2.zone]   {D}-[Y]   {E}-[Y]   {P}-[Mount Taniquetil ]   {S}-[EN-GB]
{A}-[Expected Synced Value]   {B}-[Manwë]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {P}-[Mount Taniquetil]   {S}-[en-GB]
{A}-[Active Directory]   {B}-[radagast.the.brown]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]
{A}-[<ProductName>]   {B}-[radagast.the.brown]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {G}-[radagast]
{A}-[Expected Synced Value]   {B}-[radagast.the.brown]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]
{A}-[Active Directory]   {B}-[saruman.the.white]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {P}-[Isengard]
{A}-[<ProductName>]   {B}-[saruman.the.many.coloured]   {C}-[domain2.zone]   {D}-[Y]   {E}-[Y]   {P}-[The Shire]
{A}-[Expected Synced Value]   {B}-[saruman.the.many.coloured]   {C}-[domain2.zone]   {D}-[Y]   {E}-[N]";

		IEnumerable<Tuple<int, int>> GetUsersOnlyAcceptedCellRefs()
		{
			yield return Tuple.Create(1, 6);

			yield return Tuple.Create(6, 1);
			yield return Tuple.Create(6, 2);
			yield return Tuple.Create(7, 4);
			yield return Tuple.Create(9, 1);
			yield return Tuple.Create(9, 2);
			yield return Tuple.Create(9, 15);
			yield return Tuple.Create(12, 6);
			yield return Tuple.Create(16, 1);
			yield return Tuple.Create(16, 2);
		}

		IEnumerable<Tuple<int, int>> GetUsersOnlyOverwrittenCellRefs()
		{
			yield return Tuple.Create(2, 6);

			yield return Tuple.Create(6, 4);
			yield return Tuple.Create(7, 1);
			yield return Tuple.Create(7, 2);
			yield return Tuple.Create(10, 1);
			yield return Tuple.Create(10, 2);
			yield return Tuple.Create(10, 15);
			yield return Tuple.Create(13, 6);
			yield return Tuple.Create(15, 1);
			yield return Tuple.Create(15, 2);
			yield return Tuple.Create(15, 4);
			yield return Tuple.Create(15, 15);
			yield return Tuple.Create(16, 4);
			yield return Tuple.Create(16, 15);
		}

		IEnumerable<Tuple<int, int>> GetUsersOnlyUnchangedCellRefs()
		{
			yield return Tuple.Create(8, 1);
			yield return Tuple.Create(8, 2);
			yield return Tuple.Create(8, 3);
			yield return Tuple.Create(8, 4);
			yield return Tuple.Create(8, 6);
			yield return Tuple.Create(9, 3);
			yield return Tuple.Create(9, 4);
			yield return Tuple.Create(10, 3);
			yield return Tuple.Create(10, 4);
			yield return Tuple.Create(11, 1);
			yield return Tuple.Create(11, 2);
			yield return Tuple.Create(11, 3);
			yield return Tuple.Create(11, 4);
			yield return Tuple.Create(11, 15);
			yield return Tuple.Create(12, 2);
			yield return Tuple.Create(12, 3);
			yield return Tuple.Create(12, 4);
			yield return Tuple.Create(13, 2);
			yield return Tuple.Create(13, 3);
			yield return Tuple.Create(13, 4);
			yield return Tuple.Create(14, 1);
			yield return Tuple.Create(14, 2);
			yield return Tuple.Create(14, 3);
			yield return Tuple.Create(14, 4);
			yield return Tuple.Create(14, 6);
			yield return Tuple.Create(17, 1);
			yield return Tuple.Create(17, 2);
			yield return Tuple.Create(17, 3);
			yield return Tuple.Create(17, 4);
		}

		#endregion

		#region GroupsOnlyReport

		const string GroupsOnlyReport = @"{G}-[Legend]
{B}-[Active Directory Integration Activation Report]   {G}-[Accepted value]
{G}-[Overwritten value]
{B}-[Report generated at 31-Jan-13 00:00:00]
{A}-[Groups]
{B}-[Group Name]   {C}-[Domain Name]   {D}-[Entities matched?]   {E}-[Active]   {F}-[Members]
{A}-[Active Directory]   {B}-[Ainur]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar, gandalf.the.white, radagast.the.brown]
{A}-[<ProductName>]   {B}-[~.=Ainur=.~]   {C}-[domain2.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar, gandalf.the.white, saruman.the.many.coloured, radagast.the.brown]
{A}-[Expected Synced Value]   {B}-[Ainur]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar, gandalf.the.white, radagast.the.brown]
{A}-[Active Directory]   {B}-[Blank]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[<ProductName>]   {B}-[Blank]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[Expected Synced Value]   {B}-[Blank]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[Active Directory]   {B}-[Maiar]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]   {F}-[gandalf.the.white, radagast.the.brown]
{A}-[<ProductName>]   {B}-[Maiar]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]   {F}-[gandalf.the.white, saruman.the.many.coloured, radagast.the.brown]
{A}-[Expected Synced Value]   {B}-[Maiar]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]   {F}-[gandalf.the.white, radagast.the.brown]
{A}-[Active Directory]   {B}-[Valar]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar]
{A}-[<ProductName>]   {B}-[~.=Valar=.~]   {C}-[domain2.zone]   {D}-[Y]   {E}-[N]   {F}-[illuvatar]
{A}-[Expected Synced Value]   {B}-[Valar]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar]";

		IEnumerable<Tuple<int, int>> GetGroupsOnlyAcceptedCellRefs()
		{
			yield return Tuple.Create(1, 6);

			yield return Tuple.Create(6, 1);
			yield return Tuple.Create(6, 2);
			yield return Tuple.Create(6, 5);
			yield return Tuple.Create(12, 5);
			yield return Tuple.Create(15, 1);
			yield return Tuple.Create(15, 2);
			yield return Tuple.Create(15, 4);
		}

		IEnumerable<Tuple<int, int>> GetGroupsOnlyOverwrittenCellRefs()
		{
			yield return Tuple.Create(2, 6);

			yield return Tuple.Create(7, 1);
			yield return Tuple.Create(7, 2);
			yield return Tuple.Create(7, 5);
			yield return Tuple.Create(13, 5);
			yield return Tuple.Create(16, 1);
			yield return Tuple.Create(16, 2);
			yield return Tuple.Create(16, 4);
		}

		IEnumerable<Tuple<int, int>> GetGroupsOnlyUnchangedCellRefs()
		{
			yield return Tuple.Create(6, 3);
			yield return Tuple.Create(6, 4);
			yield return Tuple.Create(7, 3);
			yield return Tuple.Create(7, 4);

			yield return Tuple.Create(8, 1);
			yield return Tuple.Create(8, 2);
			yield return Tuple.Create(8, 3);
			yield return Tuple.Create(8, 4);
			yield return Tuple.Create(8, 5);
			yield return Tuple.Create(9, 1);
			yield return Tuple.Create(9, 2);
			yield return Tuple.Create(9, 3);
			yield return Tuple.Create(9, 4);
			yield return Tuple.Create(9, 5);
			yield return Tuple.Create(10, 1);
			yield return Tuple.Create(10, 2);
			yield return Tuple.Create(10, 3);
			yield return Tuple.Create(10, 4);
			yield return Tuple.Create(10, 5);
			yield return Tuple.Create(11, 1);
			yield return Tuple.Create(11, 2);
			yield return Tuple.Create(11, 3);
			yield return Tuple.Create(11, 4);
			yield return Tuple.Create(11, 5);
			yield return Tuple.Create(12, 1);
			yield return Tuple.Create(12, 2);
			yield return Tuple.Create(12, 3);
			yield return Tuple.Create(12, 4);
			yield return Tuple.Create(13, 1);
			yield return Tuple.Create(13, 2);
			yield return Tuple.Create(13, 3);
			yield return Tuple.Create(13, 4);
			yield return Tuple.Create(14, 1);
			yield return Tuple.Create(14, 2);
			yield return Tuple.Create(14, 3);
			yield return Tuple.Create(14, 4);
			yield return Tuple.Create(15, 3);
			yield return Tuple.Create(15, 5);
			yield return Tuple.Create(16, 3);
			yield return Tuple.Create(16, 5);
			yield return Tuple.Create(17, 1);
			yield return Tuple.Create(17, 2);
			yield return Tuple.Create(17, 3);
			yield return Tuple.Create(17, 4);
			yield return Tuple.Create(17, 5);
		}

		#endregion

		#region UsersAndGroupsReport

		const string UsersAndGroupsReport = @"{G}-[Legend]
{B}-[Active Directory Integration Activation Report]   {G}-[Accepted value]
{G}-[Overwritten value]
{B}-[Report generated at 31-Jan-13 00:00:00]
{A}-[Users]
{B}-[Login Name]   {C}-[Domain Name]   {D}-[Entities matched?]   {E}-[Active]   {F}-[Title]   {G}-[Full Name]   {H}-[Email Address]   {I}-[Work Extension]   {J}-[Work Phone]   {K}-[Mobile Phone]   {L}-[Fax Num]   {M}-[Home Phone]   {N}-[Pager]   {O}-[Street Address]   {P}-[City]   {Q}-[State]   {R}-[Postcode]   {S}-[Language]
{A}-[Active Directory]   {B}-[gandalf.the.white]   {C}-[domain1.zone]   {D}-[N]   {E}-[N]
{A}-[<ProductName>]   {B}-[galdalf.the.grey]   {C}-[domain2.zone]   {D}-[N]   {E}-[Y]
{A}-[Expected Synced Value]   {B}-[gandalf.the.white]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[Active Directory]   {B}-[Manwë]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {P}-[Mount Taniquetil]   {S}-[en-AU]
{A}-[<ProductName>]   {B}-[Manwe]   {C}-[domain2.zone]   {D}-[Y]   {E}-[Y]   {P}-[Mount Taniquetil ]   {S}-[EN-GB]
{A}-[Expected Synced Value]   {B}-[Manwë]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {P}-[Mount Taniquetil]   {S}-[en-GB]
{A}-[Active Directory]   {B}-[radagast.the.brown]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]
{A}-[<ProductName>]   {B}-[radagast.the.brown]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {G}-[radagast]
{A}-[Expected Synced Value]   {B}-[radagast.the.brown]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]
{A}-[Active Directory]   {B}-[saruman.the.white]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {P}-[Isengard]
{A}-[<ProductName>]   {B}-[saruman.the.many.coloured]   {C}-[domain2.zone]   {D}-[Y]   {E}-[Y]   {P}-[The Shire]
{A}-[Expected Synced Value]   {B}-[saruman.the.many.coloured]   {C}-[domain2.zone]   {D}-[Y]   {E}-[N]

{A}-[Groups]
{B}-[Group Name]   {C}-[Domain Name]   {D}-[Entities matched?]   {E}-[Active]   {F}-[Members]
{A}-[Active Directory]   {B}-[Ainur]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar, gandalf.the.white, radagast.the.brown]
{A}-[<ProductName>]   {B}-[~.=Ainur=.~]   {C}-[domain2.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar, gandalf.the.white, saruman.the.many.coloured, radagast.the.brown]
{A}-[Expected Synced Value]   {B}-[Ainur]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar, gandalf.the.white, radagast.the.brown]
{A}-[Active Directory]   {B}-[Blank]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[<ProductName>]   {B}-[Blank]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[Expected Synced Value]   {B}-[Blank]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]
{A}-[Active Directory]   {B}-[Maiar]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]   {F}-[gandalf.the.white, radagast.the.brown]
{A}-[<ProductName>]   {B}-[Maiar]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]   {F}-[gandalf.the.white, saruman.the.many.coloured, radagast.the.brown]
{A}-[Expected Synced Value]   {B}-[Maiar]   {C}-[domain1.zone]   {D}-[N]   {E}-[Y]   {F}-[gandalf.the.white, radagast.the.brown]
{A}-[Active Directory]   {B}-[Valar]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar]
{A}-[<ProductName>]   {B}-[~.=Valar=.~]   {C}-[domain2.zone]   {D}-[Y]   {E}-[N]   {F}-[illuvatar]
{A}-[Expected Synced Value]   {B}-[Valar]   {C}-[domain1.zone]   {D}-[Y]   {E}-[Y]   {F}-[illuvatar]";

		IEnumerable<Tuple<int, int>> GetUsersAndGroupsAcceptedCellRefs()
		{
			yield return Tuple.Create(1, 6);

			// Users
			yield return Tuple.Create(6, 1);
			yield return Tuple.Create(6, 2);
			yield return Tuple.Create(7, 4);
			yield return Tuple.Create(9, 1);
			yield return Tuple.Create(9, 2);
			yield return Tuple.Create(9, 15);
			yield return Tuple.Create(12, 6);
			yield return Tuple.Create(16, 1);
			yield return Tuple.Create(16, 2);

			// Groups
			yield return Tuple.Create(21, 1);
			yield return Tuple.Create(21, 2);
			yield return Tuple.Create(21, 5);
			yield return Tuple.Create(27, 5);
			yield return Tuple.Create(30, 1);
			yield return Tuple.Create(30, 2);
			yield return Tuple.Create(30, 4);
		}

		IEnumerable<Tuple<int, int>> GetUsersAndGroupsOverwrittenCellRefs()
		{
			yield return Tuple.Create(2, 6);

			// Users
			yield return Tuple.Create(6, 4);
			yield return Tuple.Create(7, 1);
			yield return Tuple.Create(7, 2);
			yield return Tuple.Create(10, 1);
			yield return Tuple.Create(10, 2);
			yield return Tuple.Create(10, 15);
			yield return Tuple.Create(13, 6);
			yield return Tuple.Create(15, 1);
			yield return Tuple.Create(15, 2);
			yield return Tuple.Create(15, 4);
			yield return Tuple.Create(15, 15);
			yield return Tuple.Create(16, 4);
			yield return Tuple.Create(16, 15);

			// Groups
			yield return Tuple.Create(22, 1);
			yield return Tuple.Create(22, 2);
			yield return Tuple.Create(22, 5);
			yield return Tuple.Create(28, 5);
			yield return Tuple.Create(31, 1);
			yield return Tuple.Create(31, 2);
			yield return Tuple.Create(31, 4);
		}

		IEnumerable<Tuple<int, int>> GetUsersAndGroupsUnchangedCellRefs()
		{
			// Users
			yield return Tuple.Create(8, 1);
			yield return Tuple.Create(8, 2);
			yield return Tuple.Create(8, 3);
			yield return Tuple.Create(8, 4);
			yield return Tuple.Create(8, 6);
			yield return Tuple.Create(9, 3);
			yield return Tuple.Create(9, 4);
			yield return Tuple.Create(10, 3);
			yield return Tuple.Create(10, 4);
			yield return Tuple.Create(11, 1);
			yield return Tuple.Create(11, 2);
			yield return Tuple.Create(11, 3);
			yield return Tuple.Create(11, 4);
			yield return Tuple.Create(11, 15);
			yield return Tuple.Create(12, 2);
			yield return Tuple.Create(12, 3);
			yield return Tuple.Create(12, 4);
			yield return Tuple.Create(13, 2);
			yield return Tuple.Create(13, 3);
			yield return Tuple.Create(13, 4);
			yield return Tuple.Create(14, 1);
			yield return Tuple.Create(14, 2);
			yield return Tuple.Create(14, 3);
			yield return Tuple.Create(14, 4);
			yield return Tuple.Create(14, 6);
			yield return Tuple.Create(17, 1);
			yield return Tuple.Create(17, 2);
			yield return Tuple.Create(17, 3);
			yield return Tuple.Create(17, 4);

			// Groups
			yield return Tuple.Create(21, 3);
			yield return Tuple.Create(21, 4);
			yield return Tuple.Create(22, 3);
			yield return Tuple.Create(22, 4);
			yield return Tuple.Create(23, 1);
			yield return Tuple.Create(23, 2);
			yield return Tuple.Create(23, 3);
			yield return Tuple.Create(23, 4);
			yield return Tuple.Create(23, 5);
			yield return Tuple.Create(24, 1);
			yield return Tuple.Create(24, 2);
			yield return Tuple.Create(24, 3);
			yield return Tuple.Create(24, 4);
			yield return Tuple.Create(24, 5);
			yield return Tuple.Create(25, 1);
			yield return Tuple.Create(25, 2);
			yield return Tuple.Create(25, 3);
			yield return Tuple.Create(25, 4);
			yield return Tuple.Create(25, 5);
			yield return Tuple.Create(26, 1);
			yield return Tuple.Create(26, 2);
			yield return Tuple.Create(26, 3);
			yield return Tuple.Create(26, 4);
			yield return Tuple.Create(26, 5);
			yield return Tuple.Create(27, 1);
			yield return Tuple.Create(27, 2);
			yield return Tuple.Create(27, 3);
			yield return Tuple.Create(27, 4);
			yield return Tuple.Create(28, 1);
			yield return Tuple.Create(28, 2);
			yield return Tuple.Create(28, 3);
			yield return Tuple.Create(28, 4);
			yield return Tuple.Create(29, 1);
			yield return Tuple.Create(29, 2);
			yield return Tuple.Create(29, 3);
			yield return Tuple.Create(29, 4);
			yield return Tuple.Create(29, 5);
			yield return Tuple.Create(30, 3);
			yield return Tuple.Create(30, 5);
			yield return Tuple.Create(31, 3);
			yield return Tuple.Create(31, 5);
			yield return Tuple.Create(32, 1);
			yield return Tuple.Create(32, 2);
			yield return Tuple.Create(32, 3);
			yield return Tuple.Create(32, 4);
			yield return Tuple.Create(32, 5);
		}

		#endregion

		#endregion

		#region Implementation

		void AssertReportEquals(string typeOfReport, IEnumerable<EntitySynchronisedEventArgs> args, string expectedWorksheetToString, IEnumerable<Tuple<int, int>> acceptedCells, IEnumerable<Tuple<int, int>> overwrittenCells, IEnumerable<Tuple<int, int>> unchangedCells)
		{
			using (var report = Builder.BuildReport(args))
			{
				// Uncomment to see contents of the report:
				//report.SaveToFile(string.Format(@"C:\tmp\ad{0}.xls", typeOfReport));

				AssertEquals(1, report.WorkSheets.Count);
				var worksheet = report.WorkSheets[0];
				var worksheetContents = worksheet.ToString();

				AssertMultilineASCIIEquals(typeOfReport, expectedWorksheetToString, worksheetContents);

				var acceptedBackgroundColor = ActivationReportBuilder.AcceptedFormatBackgroundColor;
				var overwrittenBackgroundColor = ActivationReportBuilder.OverwrittenFormatBackgroundColor;
				var unchangedBackgroundColor = ActivationReportBuilder.DefaultFormatBackgroundColor;

				AssertBackgroundColorForCells(worksheet, acceptedCells, acceptedBackgroundColor, "Accepted");
				AssertBackgroundColorForCells(worksheet, overwrittenCells, overwrittenBackgroundColor, "Overwritten");
				AssertBackgroundColorForCells(worksheet, unchangedCells, unchangedBackgroundColor, "Unchanged");
			}
		}

		void AssertBackgroundColorForCells(IExcelWorkSheet worksheet, IEnumerable<Tuple<int, int>> cellRefs, Color background, string cellType)
		{
			CombineAssertions(string.Format("The following {0} cells had the wrong background color:", cellType), () =>
			{
				foreach (var cellRef in cellRefs)
				{
					var format = worksheet.GetCellFormat(cellRef.Item1, cellRef.Item2);

					var message = string.Format("Cell ref {0}{1}", (char)(65 + cellRef.Item2), cellRef.Item1 + 1);
					AssertColorEquals(message, background, format.BackgroundColor);
				}
			});
		}

		IEnumerable<EntitySynchronisedEventArgs> GetUserSyncArgs()
		{
			var args = CreateADUserSyncArgs("gandalf.the.grey");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GS_LoginName", ADStartingValue = "gandalf.the.white", EnterpriseStartingValue = new ZString("galdalf.the.grey"), SynchronisedValue = new ZString("gandalf.the.white") },
				new SyncEvent { PropertyName = "GS_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain2.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.False, EnterpriseStartingValue = ZBool.False, SynchronisedValue = ZBool.False, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GS_IsActive", ADStartingValue = ZBool.False, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True },
			};
			yield return args;

			args = CreateADUserSyncArgs("saruman.the.white");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GS_LoginName", ADStartingValue = "saruman.the.white", EnterpriseStartingValue = new ZString("saruman.the.many.coloured"), SynchronisedValue = new ZString("saruman.the.many.coloured") },
				new SyncEvent { PropertyName = "GS_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain2.zone"), SynchronisedValue = new ZString("domain2.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GS_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.False },
				new SyncEvent { PropertyName = "GS_City", ADStartingValue = "Isengard", EnterpriseStartingValue = "The Shire", SynchronisedValue = "" },
			};
			yield return args;

			args = CreateADUserSyncArgs("radagast.the.brown");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GS_LoginName", ADStartingValue = "radagast.the.brown", EnterpriseStartingValue = new ZString("radagast.the.brown"), SynchronisedValue = new ZString("radagast.the.brown") },
				new SyncEvent { PropertyName = "GS_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain1.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GS_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True },
				new SyncEvent { PropertyName = "GS_FullName", ADStartingValue = "", EnterpriseStartingValue = "radagast", SynchronisedValue = null },
			};
			yield return args;

			args = CreateADUserSyncArgs("Manwë");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GS_LoginName", ADStartingValue = "Manwë", EnterpriseStartingValue = "Manwe", SynchronisedValue = "Manwë" },
				new SyncEvent { PropertyName = "GS_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain2.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GS_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True },
				new SyncEvent { PropertyName = "GS_City", ADStartingValue = "Mount Taniquetil", EnterpriseStartingValue = "Mount Taniquetil ", SynchronisedValue = "Mount Taniquetil" },
				new SyncEvent { PropertyName = "GS_WorkingLanguage", ADStartingValue = "en-AU", EnterpriseStartingValue = "EN-GB", SynchronisedValue = "en-GB" },
			};
			yield return args;
		}

		IEnumerable<EntitySynchronisedEventArgs> GetGroupSyncArgs()
		{
			var args = CreateADGroupSyncArgs("Valar");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GG_Desc", ADStartingValue = "Valar", EnterpriseStartingValue = "~.=Valar=.~", SynchronisedValue = "Valar" },
				new SyncEvent { PropertyName = "GG_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain2.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GG_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.False, SynchronisedValue = ZBool.True },
				new SyncEvent { PropertyName = null, ADStartingValue = "illuvatar", EnterpriseStartingValue = "illuvatar", SynchronisedValue = "illuvatar" }
			};
			yield return args;

			args = CreateADGroupSyncArgs("Ainur");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GG_Desc", ADStartingValue = "Ainur", EnterpriseStartingValue = "~.=Ainur=.~", SynchronisedValue = "Ainur" },
				new SyncEvent { PropertyName = "GG_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain2.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GG_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True },
				new SyncEvent { PropertyName = null, ADStartingValue = "illuvatar, gandalf.the.white, radagast.the.brown", EnterpriseStartingValue = "illuvatar, gandalf.the.white, saruman.the.many.coloured, radagast.the.brown", SynchronisedValue = "illuvatar, gandalf.the.white, radagast.the.brown" }
			};
			yield return args;

			args = CreateADGroupSyncArgs("Maiar");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GG_Desc", ADStartingValue = "Maiar", EnterpriseStartingValue = "Maiar", SynchronisedValue = "Maiar" },
				new SyncEvent { PropertyName = "GG_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain1.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.False, EnterpriseStartingValue = ZBool.False, SynchronisedValue = ZBool.False, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GG_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True },
				new SyncEvent { PropertyName = null, ADStartingValue = "gandalf.the.white, radagast.the.brown", EnterpriseStartingValue = "gandalf.the.white, saruman.the.many.coloured, radagast.the.brown", SynchronisedValue = "gandalf.the.white, radagast.the.brown" }
			};
			yield return args;

			args = CreateADGroupSyncArgs("Blank");
			args.SyncEvents = new List<ISyncEvent>
			{
				new SyncEvent { PropertyName = "GG_Desc", ADStartingValue = "Blank", EnterpriseStartingValue = "Blank", SynchronisedValue = "Blank" },
				new SyncEvent { PropertyName = "GG_DomainName", ADStartingValue = "domain1.zone", EnterpriseStartingValue = new ZString("domain1.zone"), SynchronisedValue = new ZString("domain1.zone") },
				new SyncEvent { PropertyName = "IsADLinked", ADStartingValue = ZBool.False, EnterpriseStartingValue = ZBool.False, SynchronisedValue = ZBool.False, IsForcedToShowInReport = true },
				new SyncEvent { PropertyName = "GG_IsActive", ADStartingValue = ZBool.True, EnterpriseStartingValue = ZBool.True, SynchronisedValue = ZBool.True },
			};
			yield return args;
		}

		EntitySynchronisedEventArgs CreateADUserSyncArgs(string loginName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = loginName;
			return new EntitySynchronisedEventArgs { Entity = new ADUser(staff) };
		}

		EntitySynchronisedEventArgs CreateADGroupSyncArgs(string groupName)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = groupName;
			return new EntitySynchronisedEventArgs { Entity = new ADGroup(group) };
		}

		protected override void SetUp()
		{
			base.SetUp();
			Builder = new ActivationReportBuilder();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Builder = null;
		}

		IActivationReportBuilder Builder { get; set; }

		#endregion
	}
}
