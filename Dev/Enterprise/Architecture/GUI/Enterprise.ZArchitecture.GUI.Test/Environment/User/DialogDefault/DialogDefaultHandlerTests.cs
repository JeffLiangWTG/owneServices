using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.Core.DialogDefault.Testing
{
	sealed class DialogDefaultHandlerTests : TestCaseWithFactory
	{
		#region Common objects

		readonly ISerializer<DummyObj> Serializer = new ZXmlSerializerWrapper<DummyObj>();
		readonly DialogDefaultSaveOptions SaveForUser = new DialogDefaultSaveOptions { SaveNewDefaults = true, Level = "USR", KeepShowingDialog = false };
		readonly DialogDefaultContext ArbitraryContext = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Ello", ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Question, ZGuid.NewZGuid(), nullContextDescription: new ResourceStringData("EC028970-71F2-4FBA-8376-9C5372DC8CE5", "Save for all"));
		readonly DummyObj ArbitraryObject = new DummyObj { SomePrimitive = 23, SomeCSharpObj = new DateTime(1994, 12, 23), SomeZString = (NoResString)("Swiggety swoogity") };

		#endregion

		#region Dependancies

		public void TestCanSearchByVarbinary()
		{
			var def = Factory.NewWithValidTestData<StmDialogDefault>();
			var a = (byte)'a';
			def.SDD_Context = new ZBlob(new[] { a++, a++, a });
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(StmDialogDefault));
			query.AddToFilter(StmDialogDefaultSchema.SDD_Context, def.SDD_Context);
			var obj = Factory.Load<StmDialogDefault>(query);

			AssertEquals(def.PK, obj.FirstOrDefault().PK);
		}

		#endregion

		#region xml

		public void TestXmlSerializeAndRestore()
		{
			var originalResult = DialogResult.OK;
			var originalObj = ArbitraryObject;

			var handler = new DialogDefaultHandler(Factory);

			var xml = handler.SerializeExposed(originalResult, originalObj, Serializer);
			var retrieved = handler.DeserializeExposed(xml, Serializer);

			//Ensuring the bizobj's params survive serialization
			AssertNotNull(retrieved);
			AssertEquals("The DialogResult did not survive serialization", originalResult, retrieved.Item1);
			AssertEquals("The primitive's true value did not survive serialization", originalObj.SomePrimitive, retrieved.Item2.SomePrimitive);
			AssertEquals("The c# objects's true value did not survive serialization", originalObj.SomeCSharpObj, retrieved.Item2.SomeCSharpObj);
		}

		public void TestXmlSaveAndRestoreActuallyUsesCustomMethods()
		{
			var instance = new DialogDefaultHandler(Factory) { ForcedSaveOptions = SaveForUser };

			DummyObj ignored = null;
			var serializer = new DummySerializerForTest();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			instance.ShowOrDefault(ArbitraryContext, ref ignored, DummyControlCreator, serializer);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
			AssertEquals(DialogResult.OK, instance.ShowOrDefault(ArbitraryContext, ref ignored, DummyControlCreator, serializer));
		}

		class DummySerializerForTest : ISerializer<DummyObj>
		{
			readonly XElement expectedXml = XElement.Parse("<root>Hey thar</root>");

			public XElement Serialize(DummyObj obj)
			{
				return expectedXml;
			}

			public DummyObj Deserialize(XElement node)
			{
				AssertEquals(expectedXml.ToString(), node.ToString());
				return new DummyObj();
			}
		}

		#endregion

		#region Retrieving saved defaults

		public void TestSelectsCorrectUserDefaults()
		{
			ZGuid[] users =
			{
				new ZGuid("916EB3CB-C4F4-402E-B291-70EC6B666C96"), new ZGuid("29B67432-4BB1-496F-B5BD-D0EB767140D7"),
				new ZGuid("61614322-B0F9-4538-B236-A45D7A310C6D")
			};
			DialogDefaultContext[] dialogs =
			{
				new DialogDefaultContext(new ZGuid("94CE7FEF-889A-4D11-ADA3-80D6D252E90A"),
					(NoResString)"",
					ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Question),
				new DialogDefaultContext(new ZGuid("242E07CA-E801-4D32-B843-00517A537725"),
					(NoResString)"",
					ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Question),
				new DialogDefaultContext(new ZGuid("DF5F626E-270F-4F58-AD08-4CDC92AA0F49"),
					(NoResString)"",
					ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Question)
			};

			//Every user can have own defaults for all dialogs
			dialogs.ForEach(dialog =>
				users.ForEach(user =>
					NewDefault(dialog.DialogIdentifier, DialogDefaultLevel.Codes.User, user)));

			Factory.Save();

			var instance = new DialogDefaultHandler(Factory);
			var company = ZGuid.Empty;

			//Test basic functionality. No overrides or shared guids
			foreach (var userPk in users)
			{
				var defaults = instance.GetDefaultsForExposed(dialogs[0], userPk, company);

				AssertNotNull(defaults);
				AssertEquals(dialogs[0].DialogIdentifier, defaults.SDD_DialogIdentifier);
				AssertEquals(userPk, defaults.SDD_Owner);
				AssertEquals(DialogDefaultLevel.Codes.User, defaults.SDD_Level);
			}
		}

		public void TestSelectsCorrectWithCompanyDefaults()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			NewDefault(ArbitraryContext.DialogIdentifier,
				owner: company,
				level: DialogDefaultLevel.Codes.Company,
				serialized: "GDay",
				overrideChildren: true);

			NewDefault(ArbitraryContext.DialogIdentifier,
				owner: user,
				level: DialogDefaultLevel.Codes.User,
				serialized: "Mate");

			Factory.Save();
			DialogDefaultHandler.CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed = true;

			var defaults = instance.GetDefaultsForExposed(ArbitraryContext);
			AssertNotNull(defaults);
			AssertEquals("Mate", defaults.SDD_SerializedDefaults);

			DialogDefaultHandler.CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed = false;

			defaults = instance.GetDefaultsForExposed(ArbitraryContext);
			AssertNotNull(defaults);
			AssertEquals("GDay", defaults.SDD_SerializedDefaults);
		}

		public void TestFallbackToCompanyDefaults()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			var context = ArbitraryContext;

			NewDefault(context.DialogIdentifier,
				owner: company,
				level: DialogDefaultLevel.Codes.Company,
				overrideChildren: false,
				showDialog: true,
				save: true);

			var defaults = instance.GetDefaultsForExposed(context);
			AssertNotNull(defaults);
			AssertEquals(company, defaults.SDD_Owner);
		}

		public void TestUserCompanyGuidOverlap()
		{
			var guid = new ZGuid("8D330CC7-C6F3-41E4-B08D-7DC31AFFC1DA");
			var differentGuid = new ZGuid("B090487C-EE18-43F7-AFDD-B24C096DEBFA");
			var instance = new DialogDefaultHandler(Factory);
			var context = ArbitraryContext;

			NewDefault(context.DialogIdentifier,
				owner: guid,
				level: DialogDefaultLevel.Codes.Company,
				overrideChildren: false,
				showDialog: false);

			NewDefault(context.DialogIdentifier,
				owner: guid,
				level: DialogDefaultLevel.Codes.User,
				overrideChildren: false,
				showDialog: false,
				save: true);

			AssertEquals(instance.GetDefaultsForExposed(context, guid, differentGuid).SDD_Level, DialogDefaultLevel.Codes.User);
			AssertEquals(instance.GetDefaultsForExposed(context, differentGuid, guid).SDD_Level, DialogDefaultLevel.Codes.Company);
		}

		public void TestUserSavesCompanyAsPersonal()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			NewDefault(
				ArbitraryContext.DialogIdentifier,
				DialogDefaultLevel.Codes.Company, company, true,
				serialized: instance.SerializeExposed(DialogResult.Cancel, ArbitraryObject, Serializer),
				save: true);

			var shownCount = 0;
			instance.ForcedSaveOptions = SaveForUser;

			var retrieved = new DummyObj();

			DialogDefaultHandler.CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed = false;
			instance.ShowOrDefault(ArbitraryContext, ref retrieved, GetCreatorWithCallback(() => shownCount++));

			AssertEquals("The messagebox should be shown", 1, shownCount);
			AssertEquals(ArbitraryObject, retrieved);

			instance.ForcedSaveOptions = null;

			retrieved = new DummyObj();
			instance.ShowOrDefault(ArbitraryContext, ref retrieved, GetCreatorWithCallback(() => shownCount++));

			AssertEquals("The messagebox shouldn't be shown a second time", 1, shownCount);
			AssertEquals(ArbitraryObject, retrieved);
		}

		public void TestGlobalDefaultsAsOnlyDefault()
		{
			NewDefault(ArbitraryContext.DialogIdentifier,
				owner: null,
				level: DialogDefaultLevel.Codes.Global,
				serialized: "Not null",
				save: true);

			AssertNotNull(new DialogDefaultHandler(Factory).GetDefaultsForExposed(ArbitraryContext));
		}

		public void TestGlobalDefaults_WithOverride()
		{
			TestGlobalDefaults_helper(true, true, DialogDefaultLevel.Codes.Global);
		}

		public void TestGlobalDefaults_NoOverride()
		{
			TestGlobalDefaults_helper(false, false, DialogDefaultLevel.Codes.User);
		}

		public void TestGlobalDefaults_GlobalWithCompanyOverride()
		{
			TestGlobalDefaults_helper(false, true, DialogDefaultLevel.Codes.Company);
		}

		void TestGlobalDefaults_helper(bool globalOverride, bool companyOverride, string expectedLevel)
		{
			ZGuid user, company;
			var instance = GetHandler(out company, out user);

			NewDefault(ArbitraryContext.DialogIdentifier, DialogDefaultLevel.Codes.Global,
				overrideChildren: globalOverride,
				serialized: "globs");

			NewDefault(ArbitraryContext.DialogIdentifier, DialogDefaultLevel.Codes.Company,
				overrideChildren: companyOverride,
				owner: company,
				serialized: "comp");

			NewDefault(ArbitraryContext.DialogIdentifier, DialogDefaultLevel.Codes.User, user,
				serialized: "user");

			DialogDefaultHandler.CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed = false;

			Factory.Save();
			AssertEquals(expectedLevel, instance.GetDefaultsForExposed(ArbitraryContext).SDD_Level);
		}

		public void TestUserSavesCompanyAsPersonal_ThenCompanyDefaultChanges()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			DialogDefaultHandler.CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			var originalDefault = NewDefault(
				ArbitraryContext.DialogIdentifier,
				DialogDefaultLevel.Codes.Company, company,
				overrideChildren: true,
				serialized: instance.SerializeExposed(DialogResult.Cancel, ArbitraryObject, new ZXmlSerializerWrapper<DummyObj>()),
				save: true);

			var shownCount = 0;
			var controlCreator = GetCreatorWithCallback(() => shownCount++);

			var refObject = new DummyObj();

			instance.ForcedSaveOptions = SaveForUser;
			instance.ShowOrDefault(ArbitraryContext, ref refObject, controlCreator);

			instance.ForcedSaveOptions = null;

			refObject = new DummyObj();
			instance.ShowOrDefault(ArbitraryContext, ref refObject, controlCreator);

			AssertEquals(1, shownCount);
			AssertEquals(ArbitraryObject, refObject);

			var newObject = ArbitraryObject.Copy();
			newObject.SomePrimitive++;

			originalDefault.SDD_SerializedDefaults = instance.SerializeExposed(DialogResult.OK, newObject, Serializer);

			Factory.Save();

			instance.ShowOrDefault(ArbitraryContext, ref refObject, controlCreator);

			AssertEquals("The messagebox should be shown because the default has changed", 2, shownCount);
			AssertEquals(newObject, refObject);
		}

		public void TestGetCustomObjectWorks()
		{
			var handler = new DialogDefaultHandler(Factory);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var originalTime = new ZDateTime(1994, 12, 23);
			handler.ShowOrDefault(ArbitraryContext, ref originalTime, _ => new KUserControl(), control => ZDateTime.BrettsBirthday);

			AssertEquals(ZDateTime.BrettsBirthday, originalTime);
		}

		#endregion

		#region saving new defaults

		public void TestSaveForMeAsWell_True()
		{
			ZGuid user, company;
			var handler = GetHandler(out company, out user);
			handler.ForcedSaveOptions = new DialogDefaultSaveOptions { SaveNewDefaults = true, Level = DialogDefaultLevel.Codes.Company, SaveForMeAsWell = ZBool.True };

			var serialiser = new Mock<ISerializer<DummyObj>>();
			serialiser.Setup(s => s.Serialize(It.IsAny<DummyObj>())).Returns(new XElement("Hello", "world"));

			var o = ArbitraryObject;
			handler.ShowOrDefault(ArbitraryContext, ref o, DummyControlCreator, serialiser.Object);

			var defaults = handler.GetAllApplicableDefaultsSortedBySpecificityExposed(ArbitraryContext).ToArray();
			AssertEquals("Should be CMP + USR", 2, defaults.Length);

			CombineAssertions(() =>
			{
				var first = defaults.First();
				var last = defaults.Last();

				AssertEquals("One should be company and the other should be user", DialogDefaultLevel.Codes.User, first.SDD_Level);
				AssertEquals("One should be company and the other should be user", DialogDefaultLevel.Codes.Company, last.SDD_Level);

				var excludedColumns = new SchemaColumn[] { StmDialogDefaultSchema.PK, StmDialogDefaultSchema.SDD_Level, StmDialogDefaultSchema.SDD_Owner, StmDialogDefaultSchema.SDD_OverrideAllChildLevels, StmDialogDefaultSchema.SDD_ShowDialog, StmDialogDefaultSchema.SDD_SystemLastEditTimeUtc, StmDialogDefaultSchema.SDD_SystemCreateTimeUtc };
				foreach (var column in StmDialogDefaultSchema.All.Cast<SchemaColumn>().Except(excludedColumns))
				{
					AssertEquals("Should have same values [" + column.Name + "]", first[column], last[column]);
				}
			});
		}

		public void TestSaveForMeAsWell_False()
		{
			ZGuid user, company;
			var handler = GetHandler(out company, out user);
			handler.ForcedSaveOptions = new DialogDefaultSaveOptions { SaveNewDefaults = true, Level = DialogDefaultLevel.Codes.Company, SaveForMeAsWell = ZBool.False };

			var serialiser = new Mock<ISerializer<DummyObj>>();
			serialiser.Setup(s => s.Serialize(It.IsAny<DummyObj>())).Returns(new XElement("Hello", "world"));

			var o = ArbitraryObject;
			handler.ShowOrDefault(ArbitraryContext, ref o, DummyControlCreator, serialiser.Object);

			var defaults = handler.GetAllApplicableDefaultsSortedBySpecificityExposed(ArbitraryContext).ToArray();
			AssertEquals("Should be CMP only", 1, defaults.Length);
			AssertEquals("CMP", defaults.First().SDD_Level);
		}
		public void TestCaptionIsSaved()
		{
			AssertNotNull("PRE: No point testing being set if we have a null context anyway", ArbitraryContext.Context);

			ZGuid user, company;
			var instance = GetHandler(out company, out user);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			instance.ForcedSaveOptions = SaveForUser;
			instance.ShowOrDefault(ArbitraryContext, (NoResString)"Hello mum");

			AssertEquals(ArbitraryContext.Context, instance.GetDefaultsForExposed(ArbitraryContext).SDD_Context);
		}

		public void TestUserCanHaveMultipleContexts()
		{
			var first = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Some stuff", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Asterisk, ZGuid.NewZGuid());
			var second = new DialogDefaultContext(first.DialogIdentifier, first.Caption, first.Buttons, first.Icon, ZGuid.NewZGuid());

			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			NewDefault(first.DialogIdentifier, DialogDefaultLevel.Codes.User, user, serialized: instance.SerializeExposed(DialogResult.Yes, 0), context: first.Context);

			var wasShown = false;
			instance.ShowOrDefault(second, () => { wasShown = true; return new KUserControl(); });

			Assert("Since we have a different context the dialog should be shown", wasShown);
		}

		public void TestNullContextIsValidOnAllContexts()
		{
			var first = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Some stuff", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Asterisk, ZGuid.NewZGuid());
			var second = new DialogDefaultContext(ZGuid.NewZGuid(), first.Caption, first.Buttons, first.Icon, ZGuid.NewZGuid());
			var nullContext = new DialogDefaultContext(first.DialogIdentifier, first.Caption, first.Buttons, first.Icon);

			var instance = SetUpWithNewDefault<string>(ref nullContext);

			var wasShown = false;
			instance.ShowOrDefault(first, GetFuncCreatorWithCallback(() => wasShown = true));
			Assert("First not be shown because null context exists", !wasShown);

			instance.ShowOrDefault(second, GetFuncCreatorWithCallback(() => wasShown = true));
			Assert("Second should be shown as it has a different dialogId", wasShown);
		}

		public void TestFallbackToCompanyWithNullContext()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);
			var context = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Hello", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Asterisk, ZGuid.NewZGuid());

			NewDefault(context.DialogIdentifier, DialogDefaultLevel.Codes.User, user, contextGuids: new[] { ZGuid.NewZGuid() });
			var companyDefaults = NewDefault(context.DialogIdentifier, DialogDefaultLevel.Codes.Company, company, save: true);
			var retrievedDefaults = instance.GetDefaultsForExposed(context);

			AssertEquals("Since no user default exists with the same context the company default should be used", companyDefaults, retrievedDefaults);
		}

		public void TestNullOverrideCompanyDefaultOverridesUserLevelDefaults()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			var context = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Hello", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Asterisk, ZGuid.NewZGuid());

			var companyDefault = NewDefault(context.DialogIdentifier, DialogDefaultLevel.Codes.Company, company, serialized: "b", overrideChildren: true, context: ZBlob.Empty);
			var userDefault = NewDefault(context.DialogIdentifier, DialogDefaultLevel.Codes.User, user, serialized: "a", context: context.Context);

			Factory.Save();

			DialogDefaultHandler.CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed = false;
			var retrievedDefault = instance.GetDefaultsForExposed(context);
			AssertEquals("Since no user default exists with the same context the company default should be used", companyDefault, retrievedDefault);
		}

		public void TestSavingNullContextWhenNonNullExistsOverwritesIt()
		{
			ZGuid company, user;
			var handler = GetHandler(out company, out user);
			NewDefault(ArbitraryContext.DialogIdentifier, DialogDefaultLevel.Codes.User, user, context: ArbitraryContext.Context, save: true);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			handler.ForcedSaveOptions = new DialogDefaultSaveOptions { SaveNewDefaults = true, Level = DialogDefaultLevel.Codes.User, SaveForAllContexts = true };
			handler.ShowOrDefault(ArbitraryContext, (NoResString)"Ello that");

			var allDefaults = handler.GetAllApplicableDefaultsSortedBySpecificityExposed(ArbitraryContext);

			AssertEquals("The non null default should have been deleted", 1, allDefaults.Count());
			Assert(allDefaults.Single().SDD_Context.IsEmpty);
		}

		public void TestSavingSpecificContextWhenNullExistsDoesNotOverwrite()
		{
			ZGuid company, user;
			var handler = GetHandler(out company, out user);
			var originalDefault = NewDefault(ArbitraryContext.DialogIdentifier, DialogDefaultLevel.Codes.User, user, context: ZBlob.Empty, save: true);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			handler.ForcedSaveOptions = new DialogDefaultSaveOptions { SaveNewDefaults = true, Level = DialogDefaultLevel.Codes.User, SaveForAllContexts = false };
			handler.ShowOrDefault(ArbitraryContext, (NoResString)"Ello that");

			var allDefaults = handler.GetAllApplicableDefaultsSortedBySpecificityExposed(ArbitraryContext);

			AssertEquals("Both the null context and the exact context should exist", 2, allDefaults.Count());
			AssertEquals(ArbitraryContext.Context, allDefaults.First().SDD_Context);
			Assert(allDefaults.Last().SDD_Context.IsEmpty);
		}

		public void TestModifyingPropertyChangesSavedDefault_Global()
		{
			AssertModifyingPropertyForcesAnUpdate(DialogDefaultLevel.Codes.Global,
				def => def.SDD_OverrideAllChildLevels = false,
				opt => opt.OverridePersonal = true,
				def =>
				{
					AssertEquals(DialogDefaultLevel.Codes.Global, def.SDD_Level);
					Assert("Globals", def.SDD_OverrideAllChildLevels);
				});
		}

		public void TestModifyingPropertyChangesSavedDefault_Override()
		{
			AssertModifyingPropertyForcesAnUpdate(DialogDefaultLevel.Codes.Company,
				def => def.SDD_OverrideAllChildLevels = false,
				opt => opt.OverridePersonal = true,
				def => Assert("Override", def.SDD_OverrideAllChildLevels));
		}

		public void TestModifyingPropertyChangesSavedDefault_ShowDialog()
		{
			AssertModifyingPropertyForcesAnUpdate(DialogDefaultLevel.Codes.User,
				def => def.SDD_ShowDialog = true,
				opt => opt.KeepShowingDialog = false,
				def => Assert("ShowDialog", !def.SDD_ShowDialog));
		}

		public void TestSavesCaption()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);
			var context = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Hello World", ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Question);

			instance.ForcedSaveOptions = SaveForUser;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			instance.ShowOrDefault(context, (NoResString)"Hello Again");

			AssertEquals("Hello World", instance.GetDefaultsForExposed(context).SDD_Caption);
		}

		public void TestModifyingCaptionWillSaveTheMostRecent()
		{
			AssertModifyingPropertyForcesAnUpdate(DialogDefaultLevel.Codes.Company,
				def => def.SDD_Caption = "A different caption",
				opt => { },
				def => AssertEquals(ArbitraryContext.Caption, def.SDD_Caption));
		}

		public void TestUsingACaptionThatsTooLargeWillTruncateIt()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			var captionThatIsTooLarge = (NoResString)new String('c', StmDialogDefaultSchema.SDD_Caption.MaxLength + 1);
			var context = new DialogDefaultContext(ZGuid.NewZGuid(), captionThatIsTooLarge, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Asterisk);

			instance.ForcedSaveOptions = SaveForUser;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			AssertNoExceptionThrown(() => instance.ShowOrDefault(context, (NoResString)"Hello World"));
			AssertStartsWith("Should trunc long string", instance.GetDefaultsForExposed(context).SDD_Caption, captionThatIsTooLarge);
		}

		void AssertModifyingPropertyForcesAnUpdate(string level, Action<StmDialogDefault> setInitial, Action<DialogDefaultSaveOptions> setOverride, Action<StmDialogDefault> assertion)
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var context = ArbitraryContext;

			var ownerToUse = level == DialogDefaultLevel.Codes.User ? user : company;

			var savedDefaults = NewDefault(context.DialogIdentifier,
				owner: ownerToUse,
				level: level,
				overrideChildren: false,
				showDialog: true,
				serialized: instance.SerializeExposed<object>(DialogResult.Cancel, null, null),
				context: context.Context,
				save: false);

			setInitial(savedDefaults);
			Factory.Save();

			var forcedDialogOptions = new DialogDefaultSaveOptions
			{
				SaveNewDefaults = true,
				OverridePersonal = savedDefaults.SDD_OverrideAllChildLevels,
				KeepShowingDialog = savedDefaults.SDD_ShowDialog,
				Level = level
			};

			setOverride(forcedDialogOptions);
			instance.ForcedSaveOptions = forcedDialogOptions;

			instance.ShowOrDefault(context, (NoResString)"Test...");

			assertion(instance.GetDefaultsForExposed(context));
		}

		public void TestSaves()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			var context = ArbitraryContext;

			var originalObject = ArbitraryObject;

			var wasCreated = false;
			var controlCreator = GetCreatorWithCallback(() => wasCreated = true);

			instance.ForcedSaveOptions = SaveForUser;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			instance.ShowOrDefault(context, ref originalObject, controlCreator);
			Assert("Since no forced default exist, it should have been created", wasCreated);

			var savedDefault = instance.GetDefaultsForExposed(context);
			AssertNotNull(savedDefault);

			var newObject = instance.DeserializeExposed(savedDefault.SDD_SerializedDefaults, Serializer).Item2;
			AssertEquals(originalObject, newObject);
		}

		#endregion

		#region displaying the dialog

		public void TestShowsDialogWhenOwnerIsNotUser()
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			NewDefault(
				ArbitraryContext.DialogIdentifier,
				DialogDefaultLevel.Codes.Company, company, true, false,
				instance.SerializeExposed(DialogResult.Cancel, ArbitraryObject, Serializer), true);

			var wasShown = false;

			DummyObj ignored = null;
			instance.ShowOrDefault(ArbitraryContext, ref ignored, GetCreatorWithCallback(() => wasShown = true));

			Assert(
				"Since the user doesn't own this default, it should show him what he's agreeing too (even if they don't have a choice",
				wasShown);
		}

		public void DoesntSaveNoneResult()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;

			ZGuid company, user;
			var instance = GetHandler(out company, out user);
			instance.ForcedSaveOptions = SaveForUser;

			var result = instance.ShowOrDefault(ArbitraryContext, (NoResString)"Some Stuff");
			AssertEquals("PRE: Open dialog should have returned None", DialogResult.None, result);
			AssertNull("Should not have saved a None default", instance.GetDefaultsForExposed(ArbitraryContext, user, company));
		}

		public void TestDoesntShowDialogWhenDefaultPresent()
		{
			ZGuid user, company;
			var instance = GetHandler(out company, out user);

			var context = new DialogDefaultContext(
				new ZGuid("4E57AD21-22BE-4EC1-A54F-48445B126676"),
				ResString.GetMultilingualString("9CE00CD2-138C-4CCA-8415-F3BE87A86434", "RestoreTesting"),
				ZMessageBoxButtons.OKCancel,
				ZMessageBoxIcon.Error
			);

			NewDefault(context.DialogIdentifier,
				owner: user,
				level: DialogDefaultLevel.Codes.User,
				overrideChildren: true,
				showDialog: false,
				serialized: instance.SerializeExposed(DialogResult.OK, ArbitraryObject, Serializer),
				save: true);

			var wasCreated = false;
			var controlCreator = GetCreatorWithCallback(() => wasCreated = true);

			var newObject = new DummyObj();
			var newResult = instance.ShowOrDefault(context, ref newObject, controlCreator);

			Assert("A dialog shouldn't be shown when ShowDialog is false", !wasCreated);
			AssertEquals(DialogResult.OK, newResult);
			AssertEquals(ArbitraryObject, newObject);
		}

		public void TestSeveralContexts()
		{
			var dialogId = ZGuid.NewZGuid();

			ZGuid user, company;
			var instance = GetHandler(out company, out user);

			var defaultWithEmptyContext = NewDefault(dialogId, DialogDefaultLevel.Codes.User, user, showDialog: false, serialized: instance.SerializeExposed<object>(DialogResult.Yes, null));
			var defaultWithAbcContext = NewDefault(dialogId, DialogDefaultLevel.Codes.User, user, showDialog: false, serialized: instance.SerializeExposed<object>(DialogResult.No, null), context: Encoding.ASCII.GetBytes("ABC"));

			Factory.Save();

			var defContext = new DialogDefaultContext(dialogId, (NoResString)"Stuff", ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Asterisk, Encoding.ASCII.GetBytes("DEF"));

			var result = instance.ShowOrDefault(defContext, (NoResString)"Stuff");

			AssertEquals("Since it has a different context to the specific context one, it should return the null context's result", DialogResult.Yes, result);
		}

		#endregion

		#region Test error handling

		[ExpectNoExceptions]
		public void TestNoDefault()
		{
			AssertNull((new DialogDefaultHandler(Factory)).GetDefaultsForExposed(ArbitraryContext));
		}

		public void TestGracefullyHandlesSavedDefaultsWithTheWrongBindingMemberType()
		{
			DialogDefaultContext context = null;
			var instance = SetUpWithNewDefault<string>(ref context, "Here is some string");

			var wasRun = false;
			var completelyDifferentObject = ArbitraryObject;
			instance.ShowOrDefault(context, ref completelyDifferentObject, GetCreatorWithCallback(() => wasRun = true));

			Assert("The dialog should be shown if a serialized default cannot be properly retrieved", wasRun);
			Assert("An error should be reported when there was a failure",
				ErrorReporter.LastExceptionReported is InvalidOperationException);
			ErrorReporter.Clear();
		}

		public void TestGracefullyHandlesUnexpectedResponses()
		{
			DialogDefaultContext context = null;
			var instance = SetUpWithNewDefault<DummyObj>(ref context, DialogResult.Abort);

			var wasRun = false;
			var arbitraryObject = ArbitraryObject;
			instance.ShowOrDefault(context, ref arbitraryObject, GetCreatorWithCallback(() => wasRun = true));

			Assert("The dialog should be shown if the saved dialog result was not a valid response", wasRun);
			AssertEquals("There should not be more than 1 error reported", 1, ErrorReporter.TotalErrorCount);
			Assert("An error should be reported when there was a failure",
				ErrorReporter.LastExceptionReported.Message.Contains("Had result Abort saved for"));

			ErrorReporter.Clear();
		}

		#endregion

		#region common helpers

		Func<KUserControl> GetFuncCreatorWithCallback(Action callback)
		{
			return () => GetCreatorWithCallback(callback)(null);
		}

		public KUserControl DummyControlCreator(DummyObj obj)
		{
			return new KUserControl();
		}

		DefaultMessageBoxContentCreator<DummyObj> GetCreatorWithCallback(Action callback)
		{
			return obj =>
			{
				if (callback != null) { callback(); }
				return DummyControlCreator(obj);
			};
		}

		DialogDefaultHandler GetHandler(out ZGuid company, out ZGuid user)
		{
			company = ZGuid.NewZGuid();
			user = ZGuid.NewZGuid();

			return new DialogDefaultHandler(Factory)
			{
				CurrentCompanyGuidOverride = company,
				CurrentUserGuidOverride = user
			};
		}

		DialogDefaultHandler SetUpWithNewDefault<T>(ref DialogDefaultContext context, object o = null)
		{
			ZGuid company, user;
			var instance = GetHandler(out company, out user);

			context = context ?? ArbitraryContext;
			var serialized =
				(o is T) ? instance.SerializeExposed(DialogResult.Cancel, (T)o, new ZXmlSerializerWrapper<T>()) :
				(o is string) ? (string)o :
				(o is DialogResult) ? instance.SerializeExposed((DialogResult)o, ArbitraryObject, Serializer) :
				instance.SerializeExposed(DialogResult.Cancel, (T)o, new ZXmlSerializerWrapper<T>());

			NewDefault(context.DialogIdentifier,
				owner: user,
				level: DialogDefaultLevel.Codes.User,
				overrideChildren: true,
				showDialog: false,
				serialized: serialized,
				save: true);

			return instance;
		}

		StmDialogDefault NewDefault(ZGuid dId, string level, ZGuid? owner = null, bool overrideChildren = false,
			bool showDialog = true, string serialized = null, bool save = false, ZBlob? context = null, IEnumerable<ZGuid> contextGuids = null)
		{
			var def = Factory.New<StmDialogDefault>();
			def.SDD_DialogIdentifier = dId;
			def.SDD_Level = level;
			def.SDD_Owner = owner ?? ZGuid.Empty;
			def.SDD_OverrideAllChildLevels = (level == "USR") || overrideChildren;
			def.SDD_ShowDialog = showDialog;
			def.SDD_SerializedDefaults = serialized ?? "<root><Result>Cancel</Result></root>";

			def.SDD_Context = contextGuids != null ?
				new ZBlob(contextGuids.SelectMany(g => g.ToGuid().ToByteArray()).ToArray()) :
				(context ?? ZBlob.Empty);

			if (save)
			{
				Factory.Save();
			}

			return def;
		}

		#endregion
	}
}
