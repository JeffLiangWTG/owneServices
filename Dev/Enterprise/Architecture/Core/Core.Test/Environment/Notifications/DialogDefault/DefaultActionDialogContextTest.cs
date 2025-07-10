using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.DialogDefault.Testing
{
	sealed class DefaultActionDialogContextTest : TestCaseWithFactory
	{
		public void TestDefaultEncoding()
		{
			var mahString = "Hello World";
			AssertArrayEqualsByElements(Encoding.Default.GetBytes(mahString), DialogDefaultContext.ToZBlob(mahString));
		}

		public void TestExplicitEncoding()
		{
			var bytes = new byte[] { 110, 97, 110, 100, 111, 115 }; // ASCII "nandos"
			var mahString = Encoding.ASCII.GetString(bytes);
			AssertArrayEqualsByElements(bytes, DialogDefaultContext.ToZBlob(mahString));
		}

		public void TestGetContext()
		{
			var noContextGuid = new DialogDefaultContext(
					new ZGuid("65D5507F-CC5C-4F5D-B941-FEE16C201E1B"),
					(NoResString)"Hey, I'm a test!",
					ZMessageBoxButtons.OK,
					ZMessageBoxIcon.Question);

			var guid1 = ZGuid.NewZGuid();
			var oneContextGuid = CopyAndMaybeModify(noContextGuid, contextBlob: DialogDefaultContext.ToZBlob(guid1));

			var guid2 = ZGuid.NewZGuid();
			var twoContextGuid = CopyAndMaybeModify(oneContextGuid, contextBlob: DialogDefaultContext.ToZBlob(guid1, guid2));

			Assert("No context items, no context", noContextGuid.Context.IsEmpty);
			AssertEquals(new ZBlob(guid1.ToGuid().ToByteArray()), oneContextGuid.Context);

			var twoGuidBlob = new ZBlob(guid1.ToGuid().ToByteArray().Concat(guid2.ToGuid().ToByteArray()).ToArray());
			AssertEquals(twoGuidBlob, twoContextGuid.Context);
		}

		public void TestCancelIsAlwaysValid()
		{
			var type = typeof(ZMessageBoxButtons);

			Enum.GetValues(type)
				.Cast<ZMessageBoxButtons>()
				.ForEach((buttons) => Assert(Enum.GetName(type, buttons), DialogDefaultContext.IsValidResult(buttons, ZDialogResult.Cancel)));
		}

		public void TestAllMessageBoxButtonsTypesAreValid()
		{
			Enum.GetValues(typeof(ZMessageBoxButtons))
				.Cast<ZMessageBoxButtons>()
				.ForEach((buttons) =>
					AssertNoExceptionThrown(() => DialogDefaultContext.IsValidResult(buttons, ZDialogResult.OK)));
		}

		public void TestDefaultResultIsValid()
		{
			var validDialog = new DialogDefaultContext(new Guid("7EF5D6F9-F67C-4649-9552-40402EF6D443"),
									 (NoResString)"Valid Dialog",
									 ZMessageBoxButtons.YesNoCancel,
									 ZMessageBoxIcon.Information, false, ZDialogResult.Yes);

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			var invalidDialog = new DialogDefaultContext(new Guid("6434CC38-1912-4152-B72B-8429B9BF7DCC"),
									(NoResString)"Invalid Dialog",
									ZMessageBoxButtons.YesNoCancel,
									ZMessageBoxIcon.Information, false, ZDialogResult.Abort);

			AssertEquals("MessageBoxButtons.YesNoCancel does not contain an option for Abort", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetHashCodeWorks()
		{
			var baseDialog = new DialogDefaultContext(
								new ZGuid("65D5507F-CC5C-4F5D-B941-FEE16C201E1B"),
								(NoResString)"Hey, I'm a test!",
								ZMessageBoxButtons.OK,
								ZMessageBoxIcon.Question);

			AssertEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog).GetHashCode());
			AssertNotEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog, id: new ZGuid("0F7CA66D-29FA-446F-9A65-B0E999F34B65")).GetHashCode());
			AssertNotEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog, caption: (NoResString)"Not the original test string").GetHashCode());
			AssertNotEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog, buttons: ZMessageBoxButtons.OKCancel).GetHashCode());
			AssertNotEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog, icon: ZMessageBoxIcon.Asterisk).GetHashCode());
			AssertNotEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog, isGlobalOnly: true).GetHashCode());
			AssertNotEquals(baseDialog.GetHashCode(), CopyAndMaybeModify(baseDialog, checkBoxCaption: Res.GetData("55C84E56-B149-44D8-9E5A-9BC69037601C", "Test")).GetHashCode());
		}

		public void TestToZBlobString()
		{
			var stringThatsShort = "abc";
			Assert("Since the string is short the ZBlob should be too", DialogDefaultContext.ToZBlob(stringThatsShort).Length < DialogDefaultContext.ContextMaxLength);

			var theAlphabetRepeatedToMaxLength = new string(Enumerable.Range(0, DialogDefaultContext.ContextMaxLength).Select(i => (char)('A' + (i % ('Z' - 'A')))).ToArray());

			var stringThatsLong = theAlphabetRepeatedToMaxLength + "SomeMoreText";
			AssertEquals("Even though the string exceeds the value the hash should not", DialogDefaultContext.ContextMaxLength, DialogDefaultContext.ToZBlob(stringThatsLong).Length);

			var s1 = theAlphabetRepeatedToMaxLength + "Some text";
			var s2 = theAlphabetRepeatedToMaxLength + "Sometext";
			AssertNotEquals("Different strings should yield different results", DialogDefaultContext.ToZBlob(s1), DialogDefaultContext.ToZBlob(s2));
		}

		DialogDefaultContext CopyAndMaybeModify(DialogDefaultContext context, ZGuid? id = null, string caption = null, ZMessageBoxButtons? buttons = null, ZMessageBoxIcon? icon = null, ZBlob? contextBlob = null, bool isGlobalOnly = false, ResourceStringData checkBoxCaption = null)
		{
			return new DialogDefaultContext(
				dialogIdentifier: id ?? context.DialogIdentifier,
				caption: caption ?? context.Caption,
				buttons: buttons ?? context.Buttons,
				icon: icon ?? context.Icon,
				checkBoxCaption: checkBoxCaption ?? context.CheckBoxCaption,
				isGlobalOnly: isGlobalOnly,
				context: contextBlob ?? context.Context);
		}
	}
}
