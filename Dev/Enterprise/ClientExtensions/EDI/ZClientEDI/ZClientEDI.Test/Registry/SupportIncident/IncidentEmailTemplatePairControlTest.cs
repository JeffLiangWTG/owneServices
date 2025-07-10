using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class IncidentEmailTemplatePairControlTest : ZFormBasherTest
	{
		public void TestInsertDocumentField()
		{
			var legacyTempalte = new NotificationEmailTemplate(typeof(DummyDocSource), "test legacy subject", "test legacy body");
			var eRequestV2Tempalte = new NotificationEmailTemplate(typeof(DummyDocSource), "test eRequest v2 subject", "test eRequest v2 body");
			var emailTemplatePair = new IncidentEmailTemplatePair(typeof(DummyDocSource), legacyTempalte, eRequestV2Tempalte);
			using (ZForm form = new ZForm(emailTemplatePair))
			using (var control = new IncidentEmailTemplatePairControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(emailTemplatePair, null);
				AssertEquals("test legacy subject", control.GetLegacyAndERequestV1EmailSubjectTextBoxForTesting().Text);
				AssertEquals("test legacy body", control.GetLegacyAndERequestV1EmailBodyTextBoxForTesting().Text);
				AssertEquals("test eRequest v2 subject", control.GetERequestV2EmailSubjectTextBoxForTesting().Text);
				AssertEquals("test eRequest v2 body", control.GetERequestV2EmailBodyTextBoxForTesting().Text);
				AssertEquals(2, control.GetDocumentFieldsGridForTesting().List.Count);
				control.GetLegacyAndERequestV1EmailBodyTextBoxForTesting().Select(5, 0);
				control.FireOnTextBoxLeave(control.GetLegacyAndERequestV1EmailBodyTextBoxForTesting());
				AssertEquals("Cursor pos before losing focus", 5, control.GetCursorPosBeforeLosingFocusForTesting());
				control.GetDocumentFieldsGridForTesting().Select(0);
				control.FireOnDocumentFieldsGridDoubleClick();
				AssertEquals("test (*SomeProperty1*)legacy body", control.GetLegacyAndERequestV1EmailBodyTextBoxForTesting().Text);
				control.GetLegacyAndERequestV1EmailSubjectTextBoxForTesting().Select(0, 0);
				control.FireOnTextBoxLeave(control.GetLegacyAndERequestV1EmailSubjectTextBoxForTesting());
				AssertEquals("Cursor pos before losing focus", 0, control.GetCursorPosBeforeLosingFocusForTesting());
				control.GetDocumentFieldsGridForTesting().ListManager.Position = 1;
				control.FireOnDocumentFieldsGridDoubleClick();
				AssertEquals("(*SomeProperty2*)test legacy subject", control.GetLegacyAndERequestV1EmailSubjectTextBoxForTesting().Text);
				control.GetERequestV2EmailSubjectTextBoxForTesting().Select(14, 0);
				control.FireOnTextBoxLeave(control.GetERequestV2EmailSubjectTextBoxForTesting());
				control.FireOnDocumentFieldsGridDoubleClick();
				AssertEquals("test eRequest (*SomeProperty2*)v2 subject", control.GetERequestV2EmailSubjectTextBoxForTesting().Text);
				control.GetERequestV2EmailBodyTextBoxForTesting().Select(4, 0);
				control.FireOnTextBoxLeave(control.GetERequestV2EmailBodyTextBoxForTesting());
				control.FireOnDocumentFieldsGridDoubleClick();
				AssertEquals("test(*SomeProperty2*) eRequest v2 body", control.GetERequestV2EmailBodyTextBoxForTesting().Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ZEmptyFormForBasherTest();
			form.MinimumSize = new Size(1024, 600);
			form.Size = new Size(1024, 600);
			form.CaptionRenderingEnabled = true;
			var control = new IncidentEmailTemplatePairControl();
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);
			return form;
		}

		#region Implementation
		class IncidentEmailTemplatePairControlForTest : IncidentEmailTemplatePairControl
		{
			public int GetCursorPosBeforeLosingFocusForTesting()
			{
				return cursorPosBeforeLosingFocus;
			}

			public ZGrid GetDocumentFieldsGridForTesting()
			{
				return DocumentFieldsGrid;
			}

			public void FireOnTextBoxLeave(ZTextBox textBox)
			{
				OnTextBoxLeave(textBox);
			}

			public void FireOnDocumentFieldsGridDoubleClick()
			{
				OnDocumentFieldsGridDoubleClick();
			}

			public ZTextBox GetLegacyAndERequestV1EmailSubjectTextBoxForTesting()
			{
				return legacyAndERequestV1EmailSubjectTextBox;
			}

			public ZTextBox GetLegacyAndERequestV1EmailBodyTextBoxForTesting()
			{
				return legacyAndERequestV1EmailBodyTextBox;
			}

			public ZTextBox GetERequestV2EmailSubjectTextBoxForTesting()
			{
				return eRequestV2EmailSubjectTextBox;
			}

			public ZTextBox GetERequestV2EmailBodyTextBoxForTesting()
			{
				return eRequestV2EmailBodyTextBox;
			}
		}

		class DummyDocSource
		{
			[DocumentField("Some Property 1")]
			public ZString SomeProperty1
			{
				get
				{
					return "123";
				}
			}

			[DocumentField("Some Property 2")]
			public ZString SomeProperty2
			{
				get
				{
					return "234";
				}
			}
		}
		#endregion
	}
}
