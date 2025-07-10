using System.Collections.Generic;
using System.Drawing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagMagnitude))]
	class TagMagnitudeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var tag = Factory.New<TagMagnitude>();
			tag.TGM_Description = "HRRRNGH GHHH UHHHHH";

			AssertEquals("HRRRNGH GHHH UHHHHH", tag.HumanReadableName);
		}

		public void TestCodeDescription()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AHH", "Ah ah ahhh");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "ONE", "One Brightly Beaked Bird");

			AssertEquals("ONE", ((ICodeDescription)tag).Code);
			AssertEquals("One Brightly Beaked Bird (Ah ah ahhh)", ((ICodeDescription)tag).Description);
		}

		public void TestSystemReadOnly()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "MAN", "Manly");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FER", "Ferries are hard");
			definition.TGD_IsSystem = true;

			AssertEquals(true, magnitude.TGM_CodeInfo.ReadOnly);
			AssertEquals(true, magnitude.TGM_DescriptionInfo.ReadOnly);
			AssertEquals(true, magnitude.TGM_RuleRunSequenceInfo.ReadOnly);

			AssertEquals(false, magnitude.TGM_NudgeAmountInfo.ReadOnly);
			AssertEquals(false, magnitude.TGM_VisualizationDataInfo.ReadOnly);

			AssertEquals(false, magnitude.ColorInfo.ReadOnly);
			AssertEquals(false, magnitude.ApplyColorToBackgroundInfo.ReadOnly);
			AssertEquals(false, magnitude.BorderStyleInfo.ReadOnly);
			AssertEquals(false, magnitude.ApplyColorToBorderInfo.ReadOnly);
			AssertEquals(false, magnitude.VisualStylePriorityInfo.ReadOnly);

			definition.TGD_IsSystem = false;

			AssertEquals(false, magnitude.TGM_CodeInfo.ReadOnly);
			AssertEquals(false, magnitude.TGM_DescriptionInfo.ReadOnly);
			AssertEquals(false, magnitude.TGM_RuleRunSequenceInfo.ReadOnly);

			AssertEquals(false, magnitude.TGM_NudgeAmountInfo.ReadOnly);
			AssertEquals(false, magnitude.TGM_VisualizationDataInfo.ReadOnly);

			AssertEquals(false, magnitude.ColorInfo.ReadOnly);
			AssertEquals(false, magnitude.ApplyColorToBackgroundInfo.ReadOnly);
			AssertEquals(false, magnitude.BorderStyleInfo.ReadOnly);
			AssertEquals(false, magnitude.ApplyColorToBorderInfo.ReadOnly);
			AssertEquals(false, magnitude.VisualStylePriorityInfo.ReadOnly);
		}

		public void TestCannotDeleteSystem()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "PUN", "Punk");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "Funk");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "JUN", "Junk");

			AssertNoExceptionThrown(() => magnitude1.Delete());

			definition.TGD_IsSystem = true;

			AssertExceptionThrown<CannotDeleteException>(() => magnitude2.Delete());
		}

		public void TestColor()
		{
			var definition = Factory.New<TagDefinition>();
			definition.TGD_Code = "Coo";
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "Dan", "Daniel Keogh is a pretty cool guy.");
			magnitude.Color = ColorList.NameFromColor(Color.Aqua);

			Factory.Save();

			AssertEquals(Color.Aqua, Factory.CreateNewFactory().Load<TagMagnitude>(magnitude.PK).GetColor());
		}

		public void TestColor_None()
		{
			var definition = Factory.New<TagDefinition>();
			definition.TGD_Code = "Coo";
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "Dan", "Daniel Keogh is a pretty cool guy.");

			Factory.Save();

			AssertEquals(Color.Empty, Factory.CreateNewFactory().Load<TagMagnitude>(magnitude.PK).GetColor());
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDataVersionLogsWithAuditLog()
		{
			using var adminConnection = Db.NewAdminConnection();
			var factory = new BusinessObjectFactory(adminConnection);
			var auditLogsHelperForTesting = new AuditLogsHelperForTesting(factory, TagMagnitudeSchema.Instance);

			var definition = BMSTestHelper.CreateTagDefinition(factory, "DEF", "Definition");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MAG", "Magnitude");

			factory.Save();

			Assert("Data Version Log after the first save,", auditLogsHelperForTesting.GetAuditLogCollection(magnitude).Count == 1);

			magnitude.TGM_Code = "MAT";

			factory.Save();

			Assert("The change has not been logged", auditLogsHelperForTesting.GetAuditLogCollection(magnitude).Count == 2);

			magnitude.TGM_Code = "MAT";
			factory.Save();

			Assert("The \"change\" has been incorrectly logged", auditLogsHelperForTesting.GetAuditLogCollection(magnitude).Count == 2);

			auditLogsHelperForTesting.DeleteAllLogs(magnitude);
			AssertEquals("The logs have not been cleared", 0, auditLogsHelperForTesting.GetAuditLogCollection(magnitude).Count);
		}

		#region ToModel

		public void TestToModel()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DDD");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TTT");

			definition.TGD_Description = "DDD tag group";
			magnitude.TGM_Description = "TTT tag";
			magnitude.ApplyColorToBackground = true;
			magnitude.ApplyColorToBorder = true;
			magnitude.BorderStyle = "BorderStyle";
			magnitude.Color = Color.DarkGoldenrod.Name;
			magnitude.VisualStylePriority = 1;

			var model = magnitude.ToModel();

			CombineAssertions(() =>
			{
				AssertEquals(magnitude.PK.ToGuid(), model.PK);
				AssertEquals(magnitude.ApplyColorToBackground, model.ApplyColorToBackground);
				AssertEquals(magnitude.ApplyColorToBorder, model.ApplyColorToBorder);
				AssertEquals(magnitude.BorderStyle, model.BorderStyle);
				AssertEquals(magnitude.TGM_Code, model.Code);
				AssertEquals(magnitude.GetColor().ToHex(), model.Color);
				AssertEquals(magnitude.Description, model.Description);
				AssertEquals(magnitude.DisplayText, model.DisplayText);
				AssertEquals(magnitude.VisualStylePriority, model.VisualStylePriority);
			});
		}

		public void TestToModel_ColorShouldBeNull_WhenNoColor()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DDD");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "TT1");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TT2");
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "TT3");

			magnitude1.Color = null;
			magnitude2.Color = "";
			magnitude3.Color = "this is not a valid color";

			var model1 = magnitude1.ToModel();
			var model2 = magnitude2.ToModel();
			var model3 = magnitude3.ToModel();

			AssertNull(model1.Color);
			AssertNull(model2.Color);
			AssertNull(model3.Color);
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF", "Definition");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MAG", "Magnitude");

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, magnitude.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			magnitude.TGM_Description = "New description";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			magnitude.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";
			return magnitude;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var definition = factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";
			return magnitude;
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "ApplyColorToBackground";
				yield return "ApplyColorToBorder";
				yield return "BorderStyle";
				yield return "Color";
				yield return "VisualStylePriority";
			}
		}

		#endregion
	}
}
