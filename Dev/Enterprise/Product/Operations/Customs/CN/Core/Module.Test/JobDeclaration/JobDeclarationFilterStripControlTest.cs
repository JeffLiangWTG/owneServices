using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.Module.Testing
{
	class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestAddedGridColumns()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			using (var module = new JobDeclarationModule())
			using (var form = new ZForm())
			using (var filterStrip = new JobDeclarationFilterStripControl(module, declarations, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;
				AssertNotNull(filteredGrid.Columns[JobDeclarationFilterStripControl.ColumnNames.DeclarationDeadline]);
				AssertNotNull(filteredGrid.Columns[JobDeclarationFilterStripControl.ColumnNames.RemainingDaysForDeclaration]);
			}
		}

		public void TestColourDeciding()
		{
			var coll = new CNDeclarationDeadlineWarningThresholdCollection();
			var cnDeclarationDeadlineWarningThreshold = coll.AddNew();
			cnDeclarationDeadlineWarningThreshold.TransportMode = "ALL";
			cnDeclarationDeadlineWarningThreshold.FirstLevelThreshold = 1;
			cnDeclarationDeadlineWarningThreshold.FirstLevelWarningColor = ColorHelper.GetRGBbyColor(Color.Red);
			cnDeclarationDeadlineWarningThreshold.SecondLevelThreshold = 3;
			cnDeclarationDeadlineWarningThreshold.SecondLevelWarningColor = ColorHelper.GetRGBbyColor(Color.LightSalmon);
			cnDeclarationDeadlineWarningThreshold.ThirdLevelThreshold = 7;
			cnDeclarationDeadlineWarningThreshold.ThirdLevelWarningColor = ColorHelper.GetRGBbyColor(Color.LightYellow);
			cnDeclarationDeadlineWarningThreshold.DelayedWarningColor = ColorHelper.GetRGBbyColor(Color.Blue);
			var module = new JobDeclarationModule();
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			var declaration = declarations.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000001", ZDateTime.Empty);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (CNCustomsDataRegistry.Instance.CNDeclarationDeadlineWarningThresholdSetting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, coll))
			using (var form = new ZForm())
			using (var filterStrip = new Testing.JobDeclarationFilterStripControlForTest(module, declarations, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today, Color.Empty);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-9), Color.LightYellow);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-13), Color.LightSalmon);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-14), Color.Red);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-15), Color.Blue);
			}

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CNCustomsDataRegistry.Instance.CNDeclarationDeadlineWarningThresholdSetting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, coll))
			using (var form = new ZForm())
			using (var filterStrip = new Testing.JobDeclarationFilterStripControlForTest(module, declarations, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today, Color.Empty);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-9), Color.Empty);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-13), Color.Empty);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-14), Color.Empty);
				AssertGridRowColor(filterStrip, declaration, ZDateTime.Today.AddDays(-15), Color.Empty);
			}

			module.Dispose();
		}

		static void AssertGridRowColor(Testing.JobDeclarationFilterStripControlForTest userControl, JobDeclaration declaration, ZDateTime arriveDate, Color color)
		{
			declaration.JE_DateOfArrival = arriveDate;
			var args = new ColourDecidingEventArgs(declaration);
			userControl.OnColourDecidingForTest(args);
			AssertEquals(color, args.Colour);
		}
	}

	class JobDeclarationFilterStripControlForTest : JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControlForTest(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(module, gridCollection, filterBusinessObject)
		{
		}

		public void OnColourDecidingForTest(ColourDecidingEventArgs args) => grid_ColourDeciding(null, args);
	}
}
